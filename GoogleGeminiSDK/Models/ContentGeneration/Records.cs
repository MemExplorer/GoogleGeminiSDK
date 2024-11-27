using GoogleGeminiSDK.Models.Components;

namespace GoogleGeminiSDK.Models.ContentGeneration;

public record AttributionSourceId(GroundingPassageId? GroundingPassage, SemanticRetrieverChunk? SemanticRetrieverChunk);

public record CitationMetadata(CitationSource[] CitationSources);

public record CitationSource(uint? StartIndex, uint? EndIndex, string? Uri, string? License);

public record GeminiGenerateContentRequest(
	Content[] Contents,
	Tool[]? Tools = null,
	ToolConfig? ToolConfig = null,
	SafetySetting[]? SafetySettings = null,
	Content? SystemInstruction = null,
	GenerationConfig? GenerationConfig = null,
	string? CachedContent = null
);

public record GenerateContentResponse(
	ResponseCandidate[] Candidates,
	PromptFeedback PromptFeedback,
	UsageMetadata UsageMetadata
);

public record GenerationConfig(
	string[]? StopSequences = null,
	string? ResponseMimeType = null,
	Schema? ResponseSchema = null,
	int? CandidateCount = null,
	uint? MaxOutputTokens = null,
	float? Temperature = null,
	float? TopP = null,
	int? TopK = null,
	int? PresencePenalty = null,
	int? FrequencyPenalty = null,
	// ReSharper disable once IdentifierTypo
	bool? ResponseLogprobs = null,
	// ReSharper disable once IdentifierTypo
	int? Logprobs = null
);

public record GroundingAttribution(AttributionSourceId Source, Content Content);

public record GroundingChunk(Web? Web);

public record GroundingMetadata(
	GroundingChunk[] GroundingChunks,
	GroundingSupport[] GroundingSupports,
	string[] WebSearchQueries,
	SearchEntryPoint? SearchEntryPoint,
	RetrievalMetadata RetrievalMetadata);

public record GroundingPassageId(string PassageId, uint PartIndex);

public record GroundingSupport(uint[] GroundingChunkIndices, float[] ConfidenceScores, Segment Segment);

public record LogprobsResult(TopCandidates[] TopCandidates, ProbsCandidate[] ChosenCandidates);

// ReSharper disable once IdentifierTypo
public record ProbsCandidate(string Token, uint TokenId, float LogProbability);

public record PromptFeedback(BlockReason BlockReason, SafetyRating[] SafetyRatings);

public record ResponseCandidate(
	Content Content,
	FinishReason? FinishReason,
	SafetyRating[] SafetyRatings,
	CitationMetadata CitationMetadata,
	uint TokenCount,
	GroundingAttribution[] GroundingAttributions,
	GroundingMetadata GroundingMetadata,
	// ReSharper disable once IdentifierTypo
	float AvgLogprobs,
	// ReSharper disable once IdentifierTypo
	LogprobsResult LogprobsResult,
	uint Index
);

public record RetrievalMetadata(uint? GoogleSearchDynamicRetrievalScore);

public record SafetyRating(HarmCategory Category, HarmProbability Probability, bool Blocked);

public record SafetySetting(HarmCategory Category, HarmBlockThreshold Threshold);

public record SearchEntryPoint(string? RenderedContent, byte[]? SdkBlob);

public record Segment(uint PartIndex, uint StartIndex, uint EndIndex, string Text);

public record SemanticRetrieverChunk(string Source, string Chunk);

public record TopCandidates(ProbsCandidate[] Candidates);

public record UsageMetadata(
	uint PromptTokenCount,
	uint CachedContentTokenCount,
	uint CandidatesTokenCount,
	uint TotalTokenCount);

public record Web(string Uri, string Title);
