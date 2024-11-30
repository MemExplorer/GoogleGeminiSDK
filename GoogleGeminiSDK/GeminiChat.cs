using Microsoft.Extensions.AI;

namespace GoogleGeminiSDK;

/// <summary>
/// High-level version of <see cref="GeminiChatClient"/>
/// </summary>
public class GeminiChat
{
	public string ApiKey { get; init; }
	public string ModelId { get; init; }

	/// <summary>
	/// Occurs when a new chat message is received.
	/// </summary>
	/// <remarks>
	/// The event handler provides <see cref="ChatReceiveEventArgs"/> containing details about the received message.
	/// </remarks>
	public event EventHandler<ChatReceiveEventArgs>? OnChatReceive;

	private readonly GeminiChatClient _client;
	private List<ChatMessage> _messages;

	/// <summary>
	/// Initializes a new instance of the <see cref="GeminiChat"/> class.
	/// </summary>
	/// <param name="apiKey">Gemini API Key</param>
	/// <param name="modelId">Model Id</param>
	/// <param name="httpClient">Customizable HTTPClient</param>
	public GeminiChat(string apiKey, string modelId, HttpClient? httpClient = null)
	{
		ApiKey = apiKey;
		ModelId = modelId;

		_messages = [];
		_client = new GeminiChatClient(apiKey, ModelId, httpClient);
	}

	/// <summary>
	/// Loads chat history from your list of messages
	/// </summary>
	/// <param name="messages">Chat history messages</param>
	public void LoadHistory(IList<ChatMessage> messages) =>
		_messages = [.. messages];

	/// <summary>
	/// Append chat message to history
	/// </summary>
	/// <param name="message">Abstracted chat message</param>
	public void AppendToHistory(ChatMessage message) =>
		_messages.Add(message);
	
	/// <summary>
	/// Get the list of messages in the conversation
	/// </summary>
	/// <returns>A list of chat message</returns>
	public IList<ChatMessage> GetHistory() => _messages;

	/// <summary>
	/// Clears chat history
	/// </summary>
	public void ClearHistory() =>
		_messages.Clear();

	/// <summary>
	/// Sends a message to Gemini.
	/// </summary>
	/// <param name="message">
	/// The content of the message to send.
	/// </param>
	/// <param name="attachments">
	/// An optional list of data arrays representing the attachments to include with the message.<br/>
	/// Supported File Types: BMP, GIF, JPEG, PDF, GIF
	/// </param>
	/// <param name="settings">
	/// Optional settings to customize the behavior of Gemini, represented by a <see cref="GeminiSettings"/> object. 
	/// Pass <c>null</c> to use default settings.
	/// </param>
	/// <returns>
	/// A <see cref="ChatMessage"/> object representing containing the response message.
	/// </returns>
	public async Task<ChatMessage> SendMessage(string message, IList<byte[]>? attachments = null, GeminiSettings? settings = null)
	{
		var convertedOptions = settings?.ToChatOption();
		PrepareMessage(message, attachments);

		// send to gemini
		var chatResponse = await _client.CompleteAsync(_messages, convertedOptions);
		
		// set message id of gemini response before appending to message history
		chatResponse.Message.AdditionalProperties = new AdditionalPropertiesDictionary()
		{
			{"id", (ulong)_messages.Count}	
		};
		_messages.Add(chatResponse.Message);

		OnChatReceive?.Invoke(this, new ChatReceiveEventArgs(chatResponse.Message));

		if (settings is { Conversational: false })
			ClearHistory();

		return chatResponse.Message;
	}

	/// <summary>
	/// Sends a message to Gemini in streaming mode.
	/// </summary>
	/// <param name="message">
	/// The content of the message to send.
	/// </param>
	/// <param name="attachments">
	/// An optional list of data arrays representing the attachments to include with the message.<br/>
	/// Supported File Types: BMP, GIF, JPEG, PDF, GIF
	/// </param>
	/// <param name="settings">
	/// Optional settings to customize the behavior of Gemini, represented by a <see cref="GeminiSettings"/> object. 
	/// Pass <c>null</c> to use default settings.
	/// </param>
	/// <returns>
	/// A <see cref="ChatMessage"/> object representing containing the response message.
	/// </returns>
	public async IAsyncEnumerable<StreamingChatCompletionUpdate> SendMessageStreaming(string message, IList<byte[]>? attachments = null, GeminiSettings? settings = null)
	{
		var convertedOptions = settings?.ToChatOption();

		/*
		 * TODO: 
		 * Add conversation support for SendMessageStreaming
		 * Add event	 
		 * 
		 */
		if (settings is { Conversational: true })
			throw new NotSupportedException("Conversational messaging is not supported for SendMessageStreaming!");

		PrepareMessage(message, attachments);

		// send to gemini
		var chatResponse = _client.CompleteStreamingAsync(_messages, convertedOptions);
		await foreach (var update in chatResponse)
			yield return update;
	}

	private void PrepareMessage(string message, IList<byte[]>? attachments = null)
	{
		var content = new List<AIContent>();

		// Check attachments
		if (attachments is { Count: > 0 })
		{
			foreach (var attachment in attachments)
			{
				var mimeType = FileHelpers.GetMimeType(attachment);
				if (mimeType == null)
					throw new Exception("Unsupported file type!");

				content.Add(new DataContent(attachment.AsMemory(), mimeType));
			}
		}

		// add text message
		content.Add(new TextContent(message));

		// set message id of user message before appending to message history
		var userMsg = new ChatMessage(ChatRole.User, content) { AdditionalProperties = new AdditionalPropertiesDictionary()
			{
				{"id", (ulong)_messages.Count}	
			}
		};
		_messages.Add(userMsg);

		OnChatReceive?.Invoke(this, new ChatReceiveEventArgs(userMsg));
	}

}
