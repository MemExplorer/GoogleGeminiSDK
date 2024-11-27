using GoogleGeminiSDK.Models.Components;
using Microsoft.Extensions.AI;

namespace GoogleGeminiSDK;

public static class Extensions
{
	public static TPropertyType? GetValueOrDefault<TPropertyType>(this AdditionalPropertiesDictionary dictionary,
		string key) =>
		(TPropertyType?)dictionary.GetValueOrDefault(key);

	public static IEnumerable<Content> ToGemini(this IList<ChatMessage> messages) =>
		messages.Select(x => new Content(
			ToGeminiMessageParts(x),
			x.Role.Value
		));

	private static Part[] ToGeminiMessageParts(ChatMessage chatMessage)
	{
		var parts = new List<Part>();
		foreach (var content in chatMessage.Contents)
		{
			var part = content switch
			{
				TextContent textContent => new Part(textContent.Text),
				ImageContent imageContent => new Part(InlineData: new Blob(imageContent.MediaType!, imageContent.Data)),
				FunctionCallContent functionCall => new Part(
					FunctionCall: new FunctionCall(functionCall.Name, functionCall.Arguments)),
				FunctionResultContent functionResultContent => new Part(
					FunctionResponse: new FunctionResponse(functionResultContent.Name,
						(Dictionary<string, object?>?)functionResultContent.Result)),
				_ => null
			};

			if (part != null)
				parts.Add(part);
		}

		return parts.ToArray();
	}
}
