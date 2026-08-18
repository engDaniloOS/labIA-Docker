using DocChat.Configuration;
using DocChat.Search;
using Microsoft.Extensions.VectorData;
using SkTextChunker = Microsoft.SemanticKernel.Text.TextChunker;

namespace DocChat.Ingestion;

// Re-indexes the knowledge base from scratch on every run: this is a didactic project, so
// simplicity and reproducibility (always reflecting the current KnowledgeBase/ contents) win
// over the cost of re-embedding everything on each startup.
public static class DocumentIndexer
{
    public static async Task<int> IndexAsync(
        VectorStoreCollection<Guid, DocumentChunk> collection,
        string knowledgeBasePath,
        ChunkingOptions chunkingOptions,
        CancellationToken cancellationToken = default)
    {
        await collection.EnsureCollectionDeletedAsync(cancellationToken);
        await collection.EnsureCollectionExistsAsync(cancellationToken);

        var documents = DocumentLoader.LoadMarkdownFiles(knowledgeBasePath);
        var chunks = new List<DocumentChunk>();

        foreach (var (source, text) in documents)
        {
            var lines = SkTextChunker.SplitPlainTextLines(text, chunkingOptions.MaxCharsPerLine);
            var paragraphs = SkTextChunker.SplitPlainTextParagraphs(lines, chunkingOptions.MaxCharsPerParagraph, chunkingOptions.OverlapChars);

            for (var i = 0; i < paragraphs.Count; i++)
            {
                chunks.Add(new DocumentChunk
                {
                    Source = source,
                    ChunkIndex = i,
                    Content = paragraphs[i],
                    Embedding = paragraphs[i]
                });
            }
        }

        if (chunks.Count > 0)
            await collection.UpsertAsync(chunks, cancellationToken);

        return chunks.Count;
    }
}
