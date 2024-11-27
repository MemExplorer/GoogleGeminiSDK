using System.Net.Http.Json;
using System.Text.Json;
using GoogleGeminiSDK.Models.Components;
using GoogleGeminiSDK.Models.ContentGeneration;
using Microsoft.Extensions.AI;

// ReSharper disable StringLiteralTypo

namespace GoogleGeminiSDK;

public class GeminiChatClient : IChatClient
{
	public ChatClientMetadata Metadata { get; }
	private string ApiKey { get; }

	private static readonly Uri DefaultGeminiEndpoint = new("https://generativelanguage.googleapis.com/");
	private static readonly ChatRole ModelRole = new("model");

	private readonly HttpClient _httpClient;

	public GeminiChatClient(string apiKey, string modelId, HttpClient? httpClient = null)
	{
		ApiKey = apiKey;
		_httpClient = httpClient ?? new HttpClient();
		Metadata = new ChatClientMetadata("Google Gemini", DefaultGeminiEndpoint, modelId);
	}

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

	public IAsyncEnumerable<StreamingChatCompletionUpdate> CompleteStreamingAsync(IList<ChatMessage> chatMessages,
		ChatOptions? options = null,
		CancellationToken cancellationToken = new()) =>
		// TODO: Support streaming chat
		throw new NotImplementedException();

	public object? GetService(Type serviceType, object? serviceKey = null) =>
		serviceKey is not null && typeof(IChatClient) == serviceType ? this : null;

	public TService? GetService<TService>(object? key = null) where TService : class =>
		GetService(typeof(TService), key) as TService;

	public void Dispose() => _httpClient.Dispose();

	private Uri GetChatEndpoint(bool streaming = false) =>
		new(DefaultGeminiEndpoint,
			$"v1beta/models/{Metadata.ModelId}:{(streaming ? "streamGenerateContent" : "generateContent")}?key={ApiKey}");

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

	# region Response Serialization

	private async Task<GenerateContentResponse> HandleAiTools(GenerateContentResponse response,
		IList<ChatMessage> chatMessages, ChatOptions? options,
		CancellationToken cancellationToken)
	{
		// extract content
		var funcCallContentList = new List<AIContent>();
		var funcCallResponseContentList = new List<AIContent>();
		var firstCandidate = response.Candidates[0];
		var chatContent = firstCandidate.Content;

		// check if we have function calls
		bool hasFuncCalls = chatContent.Parts.Any(x => x.FunctionCall != null);
		if (!hasFuncCalls)
			return response;

		// tools should not be empty if we have function calls
		if (options?.Tools is null || options.Tools.Count == 0)
			throw new Exception("Invalid Tool Call when Tools in options is invalid.");

		var aiFuncTools = options.Tools.OfType<AIFunction>().ToList();
		foreach (var p in chatContent.Parts)
			if (p.FunctionCall != null)
			{
				// run function called by LLM
				var currentAiFunction = aiFuncTools.First(x => x.Metadata.Name == p.FunctionCall.Name);

				// deserialize return value
				var jsonRetValue =
					(JsonElement?)await currentAiFunction.InvokeAsync(p.FunctionCall.Args, cancellationToken);
				object? deserializedRetValue =
					jsonRetValue?.Deserialize(currentAiFunction.Metadata.ReturnParameter.ParameterType!);

				// deserialize args to its correct type
				IDictionary<string, object?> deserializedArgs = new Dictionary<string, object?>();
				var argTypeMap = currentAiFunction.Metadata.Parameters.ToDictionary(x => x.Name, x => x.ParameterType);
				if (p.FunctionCall.Args != null)
					foreach (var arg in p.FunctionCall.Args)
					{
						object? deserializedArg = null;
						if (arg.Value is JsonElement je)
							deserializedArg = je.Deserialize(argTypeMap[arg.Key]!);

						deserializedArgs.Add(arg.Key, deserializedArg);
					}

				// append to function call response
				var callContentResponse = new FunctionResultContent("", p.FunctionCall.Name,
					new Dictionary<string, object?> { { p.FunctionCall.Name, deserializedRetValue } });
				var callContent = new FunctionCallContent(callContentResponse.CallId, callContentResponse.Name,
					deserializedArgs);
				funcCallResponseContentList.Add(callContentResponse);
				funcCallContentList.Add(callContent);
			}

		// update messages without affecting the original messages
		// to exclude tools-related messages
		var chatsCopy = new List<ChatMessage>(chatMessages);
		var modelToolResponse = new ChatMessage(ModelRole, funcCallContentList);
		var messageToolResponse = new ChatMessage(ChatRole.User, funcCallResponseContentList);
		chatsCopy.Add(modelToolResponse);
		chatsCopy.Add(messageToolResponse);
		return await SendToGemini(chatsCopy, cancellationToken, options);
	}

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

		var geminiContentList = new List<AIContent>();
		foreach (var p in chatContent.Parts)
			if (!string.IsNullOrEmpty(p.Text))
			{
				var textContent = new TextContent(p.Text);
				geminiContentList.Add(textContent);
			}
			else
			{
				throw new NotSupportedException("Unsupported gemini content type");
			}

		return new ChatMessage(new ChatRole(chatContent.Role!), geminiContentList);
	}

	private static GeminiGenerateContentRequest ToGeminiMessage(IList<ChatMessage> chatMessages, ChatOptions? options)
	{
		var convertedMessages = chatMessages.ToGemini().ToArray();
		var additionalProperties = options?.AdditionalProperties;
		var generationConfig = new GenerationConfig(
			options?.StopSequences?.ToArray(),
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
		var safetySettings = additionalProperties?.GetValueOrDefault<SafetySetting[]>("safetySettings");
		var tools = CreateToolFromOptions(options);
		var toolConfig = options is { Tools.Count: > 0 } && options.Tools.Any(x => x is AIFunction)
			? new ToolConfig(new FunctionCallingConfig(options.ToolMode switch
				{
					AutoChatToolMode _ => FunctionCallingMode.AUTO,
					RequiredChatToolMode _ => FunctionCallingMode.ANY,
					_ => FunctionCallingMode.NONE
				},
				additionalProperties?.GetValueOrDefault<string[]?>("allowedFunctionNames")))
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
	private static Tool[]? CreateToolFromOptions(ChatOptions? options)
	{
		var toolList = new List<Tool>();
		var funcDecls = new List<FunctionDeclaration>();
		if (options?.Tools == null || options.Tools.Count == 0)
			return null;

		foreach (var aiTool in options.Tools)
			if (aiTool is AIFunction af)
			{
				string fName = af.Metadata.Name;
				string fDesc = af.Metadata.Description;
				var fParams = af.Metadata.Parameters.Count > 0
					? new Schema(SchemaType.OBJECT, // only allowed for OBJECT type
						Properties: af.Metadata.Parameters.ToDictionary(x => x.Name,
							y => new Schema(GetSchemaType(y.ParameterType), Description: y.Description)))
					: null;
				var funcDecl = new FunctionDeclaration(fName, fDesc, fParams);
				funcDecls.Add(funcDecl);
			}

		if (funcDecls.Count > 0)
			toolList.Add(new Tool(funcDecls.ToArray(), null, null));

		// TODO: Add support for Search Retrieval and Code Execution
		if (toolList.Count == 0)
			return null;

		return toolList.ToArray();
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
