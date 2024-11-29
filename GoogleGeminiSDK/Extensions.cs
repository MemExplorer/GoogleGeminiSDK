using GoogleGeminiSDK.Models.Components;
using Microsoft.Extensions.AI;

namespace GoogleGeminiSDK;

internal static class Extensions
{
	internal static TPropertyType? GetValueOrDefault<TPropertyType>(this AdditionalPropertiesDictionary dictionary,
		string key) =>
		(TPropertyType?)dictionary.GetValueOrDefault(key);

	internal static IEnumerable<Content> ToGemini(this IList<ChatMessage> messages) =>
		messages.Select(x => new Content(
			ToGeminiMessageParts(x),
			x.Role.Value
		));

	internal static ChatMessage ToChatMessage(this Content content)
	{
		var geminiContentList = new List<AIContent>();
		foreach (var p in content.Parts)
		{
			if (!string.IsNullOrEmpty(p.Text))
			{
				var textContent = new TextContent(p.Text);
				geminiContentList.Add(textContent);
			}
			else
			{
				throw new NotSupportedException("Unsupported gemini content type");
			}
		}

		return new ChatMessage(new ChatRole(content.Role!), geminiContentList);
	}

	private static List<Part> ToGeminiMessageParts(ChatMessage chatMessage)
	{
		var parts = new List<Part>();
		foreach (var content in chatMessage.Contents)
		{
			var part = content switch
			{
				TextContent textContent => new Part(textContent.Text),
				DataContent dataContent => new Part(InlineData: new Blob(dataContent.MediaType!, dataContent.Data)),
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

		return parts;
	}
}
