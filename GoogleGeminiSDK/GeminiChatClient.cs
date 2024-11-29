using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using GoogleGeminiSDK.Models.Components;
using GoogleGeminiSDK.Models.ContentGeneration;
using GoogleGeminiSDK.Tools;
using Microsoft.Extensions.AI;

// ReSharper disable StringLiteralTypo

namespace GoogleGeminiSDK;

public class GeminiChatClient : IChatClient
{
	/// <inheritdoc />
	public ChatClientMetadata Metadata { get; }
	private string ApiKey { get; }

	private static readonly Uri DefaultGeminiEndpoint = new("https://generativelanguage.googleapis.com/");
	private static readonly ChatRole ModelRole = new("model");

	private readonly HttpClient _httpClient;

	/// <summary>
	/// Initializes a new instance of the <see cref="GeminiChatClient"/> class.
	/// </summary>
	/// <param name="apiKey">Your Gemini API Key</param>
	/// <param name="modelId">Model Id</param>
	/// <param name="httpClient">Customizable HTTPClient</param>
	public GeminiChatClient(string apiKey, string modelId, HttpClient? httpClient = null)
	{
		ApiKey = apiKey;
		_httpClient = httpClient ?? new HttpClient();
		Metadata = new ChatClientMetadata("Google Gemini", DefaultGeminiEndpoint, modelId);
	}

	/// <inheritdoc />
	public async Task<ChatCompletion> CompleteAsync(IList<ChatMessage> chatMessages, ChatOptions? options = null,
		CancellationToken cancellationToken = new())
	{
		var response = await SendToGemini(chatMessages, cancellationToken, options);

		// overwrite response when there are tool calls
		response = await HandleAiTools(response, chatMessages, options, cancellationToken);

		// deserialize gemini response
		return new ChatCompletion(FromGeminiResponse(response))
		{
			ModelId = Metadata.ModelId, FinishReason = ToFinishReason(response)
		};
	}

	/// <inheritdoc />
	public async IAsyncEnumerable<StreamingChatCompletionUpdate> CompleteStreamingAsync(IList<ChatMessage> chatMessages,
		ChatOptions? options = null,
		[EnumeratorCancellation] CancellationToken cancellationToken = new())
	{
		await foreach (var msgChunk in SendToGeminiStream(chatMessages, cancellationToken, options))
		{
			var newResponse = HandleAiToolsStream(msgChunk, chatMessages, options, cancellationToken);
			await foreach (var newChunkResponse in newResponse)
			{
				var firstCandidate = newChunkResponse.Candidates[0];
				var chatContent = firstCandidate.Content;
				var firstPart = chatContent.Parts.First();
				yield return new StreamingChatCompletionUpdate
				{
					Text = firstPart.Text, Role = new ChatRole(chatContent.Role!)
				};
			}
		}
	}

	/// <inheritdoc />
	public object? GetService(Type serviceType, object? serviceKey = null) =>
		serviceKey is null && serviceType.IsInstanceOfType(this) ? this : null;

	public TService? GetService<TService>(object? key = null) where TService : class =>
		(TService?)GetService(typeof(TService), key);

	/// <inheritdoc />
	public void Dispose() => _httpClient.Dispose();

	private Uri GetChatEndpoint(bool streaming = false) =>
		new(DefaultGeminiEndpoint,
			$"v1beta/models/{Metadata.ModelId}:{(streaming ? "streamGenerateContent?alt=sse&" : "generateContent?")}key={ApiKey}");

	private async Task<GenerateContentResponse> SendToGemini(IList<ChatMessage> chatMessages,
		CancellationToken cancellationToken, ChatOptions? options = null)
	{
		using var httpResponse = await _httpClient.PostAsJsonAsync(
			GetChatEndpoint(),
			ToGeminiMessage(chatMessages, options),
			JsonContext.Default.GeminiGenerateContentRequest,
			cancellationToken).ConfigureAwait(false);

		if (!httpResponse.IsSuccessStatusCode)
		{
			string strResp = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
			throw new Exception(strResp);
		}

		var response = (await httpResponse.Content
			.ReadFromJsonAsync(JsonContext.Default.GenerateContentResponse, cancellationToken)
			.ConfigureAwait(false))!;

		return response;
	}

	private async IAsyncEnumerable<GenerateContentResponse> SendToGeminiStream(IList<ChatMessage> chatMessages,
		[EnumeratorCancellation] CancellationToken cancellationToken, ChatOptions? options = null)
	{
		using HttpRequestMessage request = new(HttpMethod.Post, GetChatEndpoint(true));
		request.Content = JsonContent.Create(ToGeminiMessage(chatMessages, options),
			JsonContext.Default.GeminiGenerateContentRequest);

		using var httpResponse = await _httpClient
			.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);

		if (!httpResponse.IsSuccessStatusCode)
		{
			string strResp = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
			throw new Exception(strResp);
		}

		await using var responseStream =
			await httpResponse.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
		using var streamReader = new StreamReader(responseStream);
		while (await streamReader.ReadLineAsync(cancellationToken).ConfigureAwait(false) is { } line)
		{
			if (string.IsNullOrEmpty(line))
				continue;

			// temporary solution to get rid of the "data:" that has no double quotes at the header of the json data
			string removedHeader = line[5..];
			var chunk = JsonSerializer.Deserialize(removedHeader, JsonContext.Default.GenerateContentResponse);
			if (chunk is null)
				continue;

			yield return chunk;
		}
	}

	# region Tool Handlers

	private async Task<GenerateContentResponse> HandleAiTools(GenerateContentResponse response,
		IList<ChatMessage> chatMessages, ChatOptions? options,
		CancellationToken cancellationToken)
	{
		var updatedChats = await HandleAiToolsInternal(response, chatMessages, options, cancellationToken);
		if (updatedChats is null)
			return response;

		return await SendToGemini(updatedChats, cancellationToken, null);
	}

	private async IAsyncEnumerable<GenerateContentResponse> HandleAiToolsStream(GenerateContentResponse response,
		IList<ChatMessage> chatMessages, ChatOptions? options,
		[EnumeratorCancellation] CancellationToken cancellationToken)
	{
		var updatedChats = await HandleAiToolsInternal(response, chatMessages, options, cancellationToken);
		if (updatedChats is null)
		{
			yield return response;
			yield break;
		}

		await foreach (var chunk in SendToGeminiStream(updatedChats, cancellationToken, null))
		{
			yield return chunk;
		}
	}

	private async Task<IList<ChatMessage>?> HandleAiToolsInternal(GenerateContentResponse response,
		IList<ChatMessage> chatMessages, ChatOptions? options,
		CancellationToken cancellationToken)
	{
		var fCallModelContent = new List<AIContent>();
		var fCallReturnContent = new List<AIContent>();

		// extract content
		var firstCandidate = response.Candidates[0];
		var chatContent = firstCandidate.Content;

		// check if we have function calls
		bool hasFuncCalls = chatContent.Parts.Any(x => x.FunctionCall != null);
		if (!hasFuncCalls)
			return null;

		// tools should not be empty if we have function call in response content
		if (options?.Tools is null || options.Tools.Count == 0)
			throw new Exception("Invalid Tool Call when Tools in options is invalid.");

		var aiFuncTools = options.Tools.OfType<AIFunction>().ToList();
		foreach (var p in chatContent.Parts)
		{
			if (p.FunctionCall == null)
				continue;

			// fetch AI function by name
			var currentAiFunction = aiFuncTools.First(x => x.Metadata.Name == p.FunctionCall.Name);

			//execute function called by LLM
			var jsonRetValue =
				(JsonElement?)await currentAiFunction.InvokeAsync(p.FunctionCall.Args, cancellationToken);

			// deserialize return value
			object? deserializedRetValue =
				jsonRetValue?.Deserialize(currentAiFunction.Metadata.ReturnParameter.ParameterType!);

			// deserialize args to its correct type
			IDictionary<string, object?> deserializedArgs = new Dictionary<string, object?>();
			var argTypeMap = currentAiFunction.Metadata.Parameters.ToDictionary(x => x.Name, x => x.ParameterType);
			if (p.FunctionCall.Args != null)
			{
				foreach (var arg in p.FunctionCall.Args)
				{
					object? deserializedArg = null;
					if (arg.Value is JsonElement je)
						deserializedArg = je.Deserialize(argTypeMap[arg.Key]!);
					else
						throw new Exception("Failed deserializing argument!");

					deserializedArgs.Add(arg.Key, deserializedArg);
				}
			}
			

			// append to function call response
			var modelCallContent = new FunctionCallContent("", p.FunctionCall.Name,
				deserializedArgs);
			var fReturnContent = new FunctionResultContent(modelCallContent.CallId, p.FunctionCall.Name,
				new Dictionary<string, object?> { { p.FunctionCall.Name, deserializedRetValue } });
			fCallModelContent.Add(modelCallContent);
			fCallReturnContent.Add(fReturnContent);
		}

		// update messages without affecting the original messages to exclude tools-related messages
		var chatsCopy = new List<ChatMessage>(chatMessages);
		var modelMessage = new ChatMessage(ModelRole, fCallModelContent);

		// a message that returns the result of all the functions called by the LLM
		var fRetMessage = new ChatMessage(new ChatRole("function"), fCallReturnContent);
		chatsCopy.Add(modelMessage);
		chatsCopy.Add(fRetMessage);
		return chatsCopy;
	}

	#endregion

	# region Response Serialization

	private static ChatFinishReason ToFinishReason(GenerateContentResponse response)
	{
		var firstCandidate = response.Candidates[0];
		var finishReason = firstCandidate.FinishReason;
		return new ChatFinishReason(Enum.GetName(finishReason!.Value) ?? string.Empty);
	}

	private static ChatMessage FromGeminiResponse(GenerateContentResponse response)
	{
		var firstCandidate = response.Candidates[0];
		var chatContent = firstCandidate.Content;
		return chatContent.ToChatMessage();
	}

	private static GeminiGenerateContentRequest ToGeminiMessage(IList<ChatMessage> chatMessages, ChatOptions? options)
	{
		var convertedMessages = chatMessages.ToGemini().ToList();
		var additionalProperties = options?.AdditionalProperties;
		var generationConfig = new GenerationConfig(
			options?.StopSequences,
			additionalProperties?.GetValueOrDefault<string?>("responseMimeType"),
			additionalProperties?.GetValueOrDefault<Schema?>("responseSchema"),
			additionalProperties?.GetValueOrDefault<int?>("candidateCount"),
			(uint?)options?.MaxOutputTokens,
			options?.Temperature,
			options?.TopP,
			options?.TopK,
			(int?)options?.PresencePenalty,
			(int?)options?.FrequencyPenalty,
			additionalProperties?.GetValueOrDefault<bool?>("responseLogprobs"),
			additionalProperties?.GetValueOrDefault<int?>("logprobs"));
		var systemInstr =
			additionalProperties != null &&
			additionalProperties.TryGetValue("systemInstruction", out string? outSysInstr)
				? new Content([new Part(outSysInstr)])
				: null;
		string? cachedContent = additionalProperties?.GetValueOrDefault<string?>("cachedContent");
		var safetySettings = additionalProperties?.GetValueOrDefault<IList<SafetySetting>?>("safetySettings");
		var tools = CreateToolFromOptions(options);
		var toolConfig = options is { Tools.Count: > 0 } && options.Tools.Any(x => x is AIFunction)
			? new ToolConfig(new FunctionCallingConfig(options.ToolMode switch
				{
					AutoChatToolMode _ => FunctionCallingMode.AUTO,
					RequiredChatToolMode _ => FunctionCallingMode.ANY,
					_ => FunctionCallingMode.NONE
				},
				additionalProperties?.GetValueOrDefault<IList<string>?>("allowedFunctionNames")))
			: null;

		return new GeminiGenerateContentRequest(
			convertedMessages,
			tools,
			toolConfig,
			safetySettings,
			systemInstr,
			generationConfig,
			cachedContent
		);
	}

	// Supported Tools: Functions
	private static IList<Tool>? CreateToolFromOptions(ChatOptions? options)
	{
		var toolList = new List<Tool>();
		if (options?.Tools == null || options.Tools.Count == 0)
			return null;

		var searchRetrieval = options.Tools.OfType<GroundingTool>()
			.Select(gt =>
			{
				var retrievalConfig = new DynamicRetrievalConfig(gt.Mode, gt.Threshold);
				return new GoogleSearchRetrieval(retrievalConfig);
			}).ToList();
		
		if (searchRetrieval.Count > 1)
			throw new Exception("Only a single instance of the GroundingTool is allowed.");
		
		var funcDecls = options.Tools.OfType<AIFunction>()
			.Select(af =>
			{
				string fName = af.Metadata.Name;
				string fDesc = af.Metadata.Description;
				var fParams = af.Metadata.Parameters.Count > 0
					? new Schema(SchemaType.OBJECT, // only allowed for OBJECT type
						Properties: af.Metadata.Parameters.ToDictionary(x => x.Name,
							y => new Schema(GetSchemaType(y.ParameterType), Description: y.Description)))
					: null;
				return new FunctionDeclaration(fName, fDesc, fParams);
			}).ToList();

		if (funcDecls.Count > 0)
			toolList.Add(new Tool(FunctionDeclarations: funcDecls));

		if (searchRetrieval.Count > 0)
			toolList.Add(new Tool(GoogleSearchRetrieval: searchRetrieval.First()));

		// TODO: Add support for Code Execution
		return toolList.Count == 0 ? null : toolList;
	}

	private static SchemaType GetSchemaType(Type? t)
	{
		if (t == typeof(string))
			return SchemaType.STRING;
		if (t == typeof(int))
			return SchemaType.INTEGER;
		if (t == typeof(float))
			return SchemaType.NUMBER;
		if (t == typeof(bool))
			return SchemaType.BOOLEAN;
		if (t is { IsSZArray: true })
			return SchemaType.ARRAY;

		return SchemaType.OBJECT;
	}

	#endregion
}
