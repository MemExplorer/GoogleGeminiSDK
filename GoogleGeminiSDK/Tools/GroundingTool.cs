using GoogleGeminiSDK.Models.Components;
using Microsoft.Extensions.AI;

namespace GoogleGeminiSDK.Tools;

/// <summary>
/// Improve the accuracy and recency of responses with the power of Google Search
/// </summary>
public class GroundingTool : AITool
{
	/// <summary>
	/// Default value is <see cref="RetrievalMode.MODE_DYNAMIC"/>
	/// </summary>
	public RetrievalMode Mode { get; set; } = RetrievalMode.MODE_DYNAMIC;

	/// <summary>
	/// Default value is 0.3f
	/// </summary>
	public float Threshold { get; set; } = 0.3f;
}
