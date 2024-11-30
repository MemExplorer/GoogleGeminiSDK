
using Microsoft.Extensions.AI;

namespace GoogleGeminiSDK;
public class ChatReceiveEventArgs : EventArgs
{
	public ChatMessage Message { get; }

	internal ChatReceiveEventArgs(ChatMessage message) =>
		Message = message;
}
