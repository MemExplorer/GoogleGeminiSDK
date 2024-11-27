using System.Text.Json.Serialization;
using GoogleGeminiSDK.Models.Components;
using GoogleGeminiSDK.Models.ContentGeneration;

namespace GoogleGeminiSDK;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
	DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, UseStringEnumConverter = true)]

// Components
[JsonSerializable(typeof(Blob))]
[JsonSerializable(typeof(CodeExecutionResult))]
[JsonSerializable(typeof(Content))]
[JsonSerializable(typeof(DynamicRetrievalConfig))]
[JsonSerializable(typeof(ExecutableCode))]
[JsonSerializable(typeof(FileData))]
[JsonSerializable(typeof(FunctionCall))]
[JsonSerializable(typeof(FunctionCallingConfig))]
[JsonSerializable(typeof(FunctionDeclaration))]
[JsonSerializable(typeof(FunctionResponse))]
[JsonSerializable(typeof(GoogleSearchRetrieval))]
[JsonSerializable(typeof(Part))]
[JsonSerializable(typeof(Schema))]
[JsonSerializable(typeof(Tool))]
[JsonSerializable(typeof(ToolConfig))]

// ContentGeneration
[JsonSerializable(typeof(AttributionSourceId))]
[JsonSerializable(typeof(CitationMetadata))]
[JsonSerializable(typeof(CitationSource))]
[JsonSerializable(typeof(GeminiGenerateContentRequest))]
[JsonSerializable(typeof(GenerateContentResponse))]
[JsonSerializable(typeof(GroundingAttribution))]
[JsonSerializable(typeof(GroundingChunk))]
[JsonSerializable(typeof(GroundingMetadata))]
[JsonSerializable(typeof(GroundingPassageId))]
[JsonSerializable(typeof(GroundingSupport))]
[JsonSerializable(typeof(LogprobsResult))]
[JsonSerializable(typeof(ProbsCandidate))]
[JsonSerializable(typeof(PromptFeedback))]
[JsonSerializable(typeof(ResponseCandidate))]
[JsonSerializable(typeof(RetrievalMetadata))]
[JsonSerializable(typeof(SafetyRating))]
[JsonSerializable(typeof(SearchEntryPoint))]
[JsonSerializable(typeof(Segment))]
[JsonSerializable(typeof(SemanticRetrieverChunk))]
[JsonSerializable(typeof(TopCandidates))]
[JsonSerializable(typeof(UsageMetadata))]
[JsonSerializable(typeof(Web))]
internal sealed partial class JsonContext : JsonSerializerContext;
