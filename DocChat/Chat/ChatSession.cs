using Microsoft.Extensions.AI;

namespace DocChat.Chat;

// Short-term memory: kept only in the process's memory for the lifetime of this terminal
// session, trimmed to the last N messages so the context sent to the model doesn't grow
// without bound during a long conversation.
public sealed class ChatSession(int historyWindowSize)
{
    private readonly List<ChatMessage> _messages = [];

    public IReadOnlyList<ChatMessage> Messages => _messages;

    public void AddUserMessage(string text) => Add(ChatRole.User, text);

    public void AddAssistantMessage(string text) => Add(ChatRole.Assistant, text);

    private void Add(ChatRole role, string text)
    {
        _messages.Add(new ChatMessage(role, text));

        var excess = _messages.Count - historyWindowSize;
        if (excess > 0)
            _messages.RemoveRange(0, excess);
    }
}
