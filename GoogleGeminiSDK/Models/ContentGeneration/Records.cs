using GoogleGeminiSDK.Models.Components;

namespace GoogleGeminiSDK.Models.ContentGeneration;

internal record AttributionSourceId(GroundingPassageId? GroundingPassage, SemanticRetrieverChunk? SemanticRetrieverChunk);

internal record CitationMetadata(CitationSource[] CitationSources);

internal record CitationSource(uint? StartIndex, uint? EndIndex, string? Uri, string? License);

internal record GeminiGenerateContentRequest(
	Content[] Contents,
	Tool[]? Tools = null,
	ToolConfig? ToolConfig = null,
	SafetySetting[]? SafetySettings = null,
	Content? SystemInstruction = null,
	GenerationConfig? GenerationConfig = null,
	string? CachedContent = null
);

internal record GenerateContentResponse(
	ResponseCandidate[] Candidates,
	PromptFeedback PromptFeedback,
	UsageMetadata UsageMetadata
);

internal record GenerationConfig(
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

internal record GroundingAttribution(AttributionSourceId Source, Content Content);

internal record GroundingChunk(Web? Web);

internal record GroundingMetadata(
	GroundingChunk[] GroundingChunks,
	GroundingSupport[] GroundingSupports,
	string[] WebSearchQueries,
	SearchEntryPoint? SearchEntryPoint,
	RetrievalMetadata RetrievalMetadata);

internal record GroundingPassageId(string PassageId, uint PartIndex);

internal record GroundingSupport(uint[] GroundingChunkIndices, float[] ConfidenceScores, Segment Segment);

internal record LogprobsResult(TopCandidates[] TopCandidates, ProbsCandidate[] ChosenCandidates);

// ReSharper disable once IdentifierTypo
internal record ProbsCandidate(string Token, uint TokenId, float LogProbability);

internal record PromptFeedback(BlockReason BlockReason, SafetyRating[] SafetyRatings);

internal record ResponseCandidate(
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

internal record RetrievalMetadata(uint? GoogleSearchDynamicRetrievalScore);

internal record SafetyRating(HarmCategory Category, HarmProbability Probability, bool Blocked);

internal record SafetySetting(HarmCategory Category, HarmBlockThreshold Threshold);

internal record SearchEntryPoint(string? RenderedContent, byte[]? SdkBlob);

internal record Segment(uint PartIndex, uint StartIndex, uint EndIndex, string Text);

internal record SemanticRetrieverChunk(string Source, string Chunk);

internal record TopCandidates(ProbsCandidate[] Candidates);

internal record UsageMetadata(
	uint PromptTokenCount,
	uint CachedContentTokenCount,
	uint CandidatesTokenCount,
	uint TotalTokenCount);

internal record Web(string Uri, string Title);
