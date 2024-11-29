
using Microsoft.Extensions.AI;

namespace GoogleGeminiSDK;
public class ChatEventArgs : EventArgs
{
	public ChatMessage Message { get; }

	internal ChatEventArgs(ChatMessage message) =>
		Message = message;
}
