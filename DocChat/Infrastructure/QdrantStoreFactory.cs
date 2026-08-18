using CommunityToolkit.VectorData.Qdrant;
using DocChat.Configuration;
using DocChat.Search;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using Qdrant.Client;

namespace DocChat.Infrastructure;

public static class QdrantStoreFactory
{
    // The vector store is configured with the embedding generator up front, so [VectorStoreVector]
    // string properties get embedded automatically on upsert and on SearchAsync — no manual calls.
    public static VectorStoreCollection<Guid, DocumentChunk> CreateCollection(
        QdrantOptions options,
        IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator)
    {
        var qdrantClient = new QdrantClient(options.Host, options.Port, options.Https);

        var vectorStore = new QdrantVectorStore(qdrantClient, ownsClient: true, new QdrantVectorStoreOptions
        {
            EmbeddingGenerator = embeddingGenerator
        });

        return vectorStore.GetCollection<Guid, DocumentChunk>(options.CollectionName);
    }
}
