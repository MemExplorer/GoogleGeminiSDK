namespace GoogleGeminiSDK.Models.ContentGeneration;

// ReSharper disable InconsistentNaming
internal enum FinishReason
{
	FINISH_REASON_UNSPECIFIED,
	STOP,
	MAX_TOKENS,
	SAFETY,
	RECITATION,
	LANGUAGE,
	OTHER,
	BLOCKLIST,
	PROHIBITED_CONTENT,

	// ReSharper disable once IdentifierTypo
	SPII,
	MALFORMED_FUNCTION_CALL
}
