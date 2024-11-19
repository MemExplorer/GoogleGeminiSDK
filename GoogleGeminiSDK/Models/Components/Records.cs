namespace GoogleGeminiSDK.Models.Components;

public record Blob(string MimeType, ReadOnlyMemory<byte>? Data);

public record CodeExecutionResult(CodeExecutionOutcome CodeExecutionOutcome, string? Output);

public record Content(Part[] Parts, string? Role = null);

public record DynamicRetrievalConfig(RetrievalMode Mode, float? DynamicThreshold);

public record ExecutableCode(CodeLanguage CodeLanguage, string Code);

public record FileData(string FileUri, string? MimeType);

public record FunctionCall(string Name, Dictionary<string, object>? Args);

public record FunctionCallingConfig(FunctionCallingMode? Mode, string[]? AllowedFunctionNames);

public record FunctionDeclaration(string Name, string Description, Schema? Parameters);

public record FunctionResponse(string Name, Dictionary<string, object> Response);

public record GoogleSearchRetrieval(DynamicRetrievalConfig DynamicRetrievalConfig);

public record Part(
	string? Text = null,
	Blob? InlineData = null,
	FunctionCall? FunctionCall = null,
	FunctionResponse? FunctionResponse = null,
	FileData? FileData = null,
	ExecutableCode? ExecutableCode = null,
	CodeExecutionResult? CodeExecutionResult = null
);

public record Schema(
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

public record Tool(
	FunctionDeclaration[]? FunctionDeclarations,
	GoogleSearchRetrieval? GoogleSearchRetrieval,
	object? CodeExecution);

public record ToolConfig(FunctionCallingConfig? FunctionCallingConfig);
