using System.ComponentModel;
using Microsoft.Extensions.VectorData;

namespace DocChat.Search;

// Exposed to the agent as a tool via AIFunctionFactory.Create — the agent decides on its own,
// based on the conversation, whether and when to call this (agentic RAG), instead of the app
// always injecting retrieved context before every turn.
public sealed class SearchDocumentsTool(VectorStoreCollection<Guid, DocumentChunk> collection)
{
    [Description("Searches the indexed knowledge base for passages relevant to a query. " +
        "Use this whenever the user asks something that could be answered from the project's documents." +
        "Consider all documents only talk about physics learned at high school")]
    public async Task<string> SearchDocumentsAsync(
        [Description("The search query, in natural language")] string query,
        [Description("Maximum number of passages to return")] int topK = 3)
    {
        var results = new List<string>();

        await foreach (var result in collection.SearchAsync(query, topK))
            results.Add($"[{result.Record.Source} | score {result.Score:F3}] {result.Record.Content}");

        return results.Count == 0
            ? "No relevant passages found in the knowledge base."
            : string.Join("\n\n", results);
    }
}
