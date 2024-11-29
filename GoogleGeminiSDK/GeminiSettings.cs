using GoogleGeminiSDK.Models.Components;
using GoogleGeminiSDK.Models.ContentGeneration;
using Microsoft.Extensions.AI;

namespace GoogleGeminiSDK;
public class GeminiSettings
{
	/// <summary>
	/// Determines whether previous conversations are included when sending a message to Gemini.
	/// </summary>
	public bool Conversational { get; set; } = true;

	/// <summary>
	/// Optional. Controls the randomness of the output.
	/// </summary>
	/// <remarks>
	/// For more information, see the
	/// <see href="https://ai.google.dev/api/generate-content#generationconfig">GenerationConfig documentation</see>.
	/// </remarks>
	public float? Temperature { get; set; }

	/// <summary>
	/// Optional. The maximum cumulative probability of tokens to consider when sampling.
	/// </summary>
	/// <remarks>
	/// For more information, see the
	/// <see href="https://ai.google.dev/api/generate-content#generationconfig">GenerationConfig documentation</see>.
	/// </remarks>
	public int? TopK { get; set; }

	/// <summary>
	/// Optional. The maximum number of tokens to consider when sampling.
	/// </summary>
	/// <remarks>
	/// For more information, see the
	/// <see href="https://ai.google.dev/api/generate-content#generationconfig">GenerationConfig documentation</see>.
	/// </remarks>
	public float? TopP { get; set; }

	/// <summary>
	/// Optional. The set of character sequences (up to 5) that will stop output generation. 
	/// </summary>
	/// <remarks>
	/// For more information, see the
	/// <see href="https://ai.google.dev/api/generate-content#generationconfig">GenerationConfig documentation</see>.
	/// </remarks>
	public IList<string>? StopSequences { get; set; }

	/// <summary>
	/// Optional. MIME type of the generated candidate text.
	/// </summary>
	/// <remarks>
	/// For more information, see the
	/// <see href="https://ai.google.dev/api/generate-content#generationconfig">GenerationConfig documentation</see>.
	/// </remarks>
	public string? ResponseMimeType { get; set; }

	/// <summary>
	/// Optional. Output schema of the generated candidate text.
	/// </summary>
	/// <remarks>
	/// For more information, see the
	/// <see href="https://ai.google.dev/api/generate-content#generationconfig">GenerationConfig documentation</see>.
	/// </remarks>
	public Schema? ResponseSchema { get; set; }

	/// <summary>
	/// Optional. Number of generated responses to return.
	/// </summary>
	/// <remarks>
	/// For more information, see the
	/// <see href="https://ai.google.dev/api/generate-content#generationconfig">GenerationConfig documentation</see>.
	/// </remarks>
	public int? CandidateCount { get; set; }

	/// <summary>
	/// Optional. The maximum number of tokens to include in a response candidate.
	/// </summary>
	/// <remarks>
	/// For more information, see the
	/// <see href="https://ai.google.dev/api/generate-content#generationconfig">GenerationConfig documentation</see>.
	/// </remarks>
	public int? MaxOutputTokenCount { get; set; }

	/// <summary>
	/// Optional. Presence penalty applied to the next token's logprobs if the token has already been seen in the response.
	/// </summary>
	/// <remarks>
	/// For more information, see the
	/// <see href="https://ai.google.dev/api/generate-content#generationconfig">GenerationConfig documentation</see>.
	/// </remarks>
	public float? PresencePenalty { get; set; }

	/// <summary>
	/// Optional. Frequency penalty applied to the next token's logprobs, multiplied by the number of times each token has been seen in the respponse so far.
	/// </summary>
	/// <remarks>
	/// For more information, see the
	/// <see href="https://ai.google.dev/api/generate-content#generationconfig">GenerationConfig documentation</see>.
	/// </remarks>
	public float? FrequencyPenalty { get; set; }

	/// <summary>
	/// Optional. If true, export the logprobs results in response.
	/// </summary>
	/// <remarks>
	/// For more information, see the
	/// <see href="https://ai.google.dev/api/generate-content#generationconfig">GenerationConfig documentation</see>.
	/// </remarks>
	public bool? ResponseLogprobs { get; set; }

	/// <summary>
	/// Optional. Only valid if <see cref="ResponseLogprobs"></see> is set to <c>true</c>. 
	/// </summary>
	/// <remarks>
	/// For more information, see the
	/// <see href="https://ai.google.dev/api/generate-content#generationconfig">GenerationConfig documentation</see>.
	/// </remarks>
	public int? Logprobs { get; set; }

	/// <summary>
	/// Optional. Developer set system instruction(s). Currently, text only.
	/// </summary>
	/// <remarks>
	/// For more information, see the
	/// <see href="https://ai.google.dev/api/generate-content#request-body">Gemini request body documentation</see>.
	/// </remarks>
	public string? SystemInstructions { get; set; }

	/// <summary>
	/// Optional. The name of the content cached to use as context to serve the prediction.
	/// </summary>
	/// <remarks>
	/// For more information, see the
	/// <see href="https://ai.google.dev/api/generate-content#request-body">Gemini request body documentation</see>.
	/// </remarks>
	public string? CachedContent { get; set; }

	/// <summary>
	/// Optional. A list of unique <c>SafetySetting</c> instances for blocking unsafe content.
	/// </summary>
	/// <remarks>
	/// For more information, see the
	/// <see href="https://ai.google.dev/api/generate-content#request-body">Gemini request body documentation</see>.
	/// </remarks>
	public IList<SafetySetting>? SafetySettings { get; set; }

	/// <summary>
	/// Optional. A set of function names that, when provided, limits the functions the model will call.
	/// </summary>
	/// <remarks>
	/// For more information, see the
	/// <see href="https://ai.google.dev/api/caching#FunctionCallingConfig">FunctionCallingConfig documentation</see>.
	/// </remarks>
	public IList<string>? AllowedFunctionNames { get; set; }

	/// <summary>
	/// Optional. A list of FunctionDeclarations available to the model that can be used for function calling.
	/// </summary>
	/// <remarks>
	/// For more information, see the
	/// <see href="https://ai.google.dev/api/caching#Tool">Tool documentation</see>.
	/// </remarks>
	public IList<AIFunction>? Functions { get; set; }

	/// <summary>
	/// Optional. Specifies the mode in which function calling should execute.
	/// </summary>
	/// <remarks>
	/// For more information, see the
	/// <see href="https://ai.google.dev/api/caching#FunctionCallingConfig">FunctionCallingConfig documentation</see>.
	/// </remarks>
	public ChatToolMode? FunctionCallingMode { get; set; }

	internal ChatOptions ToChatOption()
	{
		var options = new ChatOptions();
		options.Temperature = Temperature;
		options.TopP = TopP;
		options.TopK = TopK;
		options.StopSequences = StopSequences;
		options.MaxOutputTokens = MaxOutputTokenCount;
		options.PresencePenalty = PresencePenalty;
		options.FrequencyPenalty = FrequencyPenalty;

		options.AdditionalProperties = new AdditionalPropertiesDictionary();
		options.AdditionalProperties["responseMimeType"] = ResponseMimeType;
		options.AdditionalProperties["responseSchema"] = ResponseSchema;
		options.AdditionalProperties["candidateCount"] = CandidateCount;
		options.AdditionalProperties["responseLogprobs"] = ResponseLogprobs;
		options.AdditionalProperties["logprobs"] = Logprobs;
		options.AdditionalProperties["systemInstruction"] = SystemInstructions;
		options.AdditionalProperties["cachedContent"] = CachedContent;
		options.AdditionalProperties["safetySettings"] = SafetySettings;
		options.AdditionalProperties["allowedFunctionNames"] = AllowedFunctionNames;

		if (FunctionCallingMode is not null)
			options.ToolMode = FunctionCallingMode;

		if (Functions is not null && Functions.Count > 0)
			options.Tools = new List<AITool>(Functions);

		return options;
	}
}
