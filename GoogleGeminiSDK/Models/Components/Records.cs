using GoogleGeminiSDK.Tools;

namespace GoogleGeminiSDK.Models.Components;

internal record Blob(string MimeType, ReadOnlyMemory<byte>? Data);

internal record CodeExecutionResult(CodeExecutionOutcome CodeExecutionOutcome, string? Output);

internal record Content(Part[] Parts, string? Role = null);

internal record DynamicRetrievalConfig(RetrievalMode Mode, float? DynamicThreshold);

internal record ExecutableCode(CodeLanguage CodeLanguage, string Code);

internal record FileData(string FileUri, string? MimeType);

internal record FunctionCall(string Name, IDictionary<string, object?>? Args);

internal record FunctionCallingConfig(FunctionCallingMode? Mode, string[]? AllowedFunctionNames);

internal record FunctionDeclaration(string Name, string Description, Schema? Parameters);

internal record FunctionResponse(string Name, Dictionary<string, object?>? Response);

internal record GoogleSearchRetrieval(DynamicRetrievalConfig DynamicRetrievalConfig);

internal record Part(
	string? Text = null,
	Blob? InlineData = null,
	FunctionCall? FunctionCall = null,
	FunctionResponse? FunctionResponse = null,
	FileData? FileData = null,
	ExecutableCode? ExecutableCode = null,
	CodeExecutionResult? CodeExecutionResult = null
);

internal record Schema(
	SchemaType Type,
	string? Format = null,
	string? Description = null,
	bool? Nullable = null,
	string[]? Enum = null,
	long? MaxItems = null,
	Dictionary<string, Schema>? Properties = null,
	string[]? Required = null,
	Schema? Items = null
);

internal record Tool(
	FunctionDeclaration[]? FunctionDeclarations = null,
	GoogleSearchRetrieval? GoogleSearchRetrieval = null,
	object? CodeExecution = null);

internal record ToolConfig(FunctionCallingConfig? FunctionCallingConfig);
