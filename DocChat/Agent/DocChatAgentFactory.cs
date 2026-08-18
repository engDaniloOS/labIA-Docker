using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace DocChat.Agent;

public static class DocChatAgentFactory
{
    private const string Instructions = """
        You are DocChat, an assistant that answers questions using the project's own knowledge
        base. Call the SearchDocuments tool whenever a question could be answered from the
        indexed documents, and base your answer only on the passages it returns. If the search
        results don't contain enough information to answer, say so clearly instead of guessing
        or relying on outside knowledge.
        """;

    public static AIAgent Create(IChatClient chatClient, AITool searchDocumentsTool) =>
        chatClient.AsAIAgent(
            instructions: Instructions,
            name: "DocChatAgent",
            tools: [searchDocumentsTool]);
}
