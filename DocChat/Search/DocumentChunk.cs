using Microsoft.Extensions.VectorData;

namespace DocChat.Search;

public static class EmbeddingSettings
{
    // Must match the output dimension of the model served by the embeddings container
    // (default: BAAI/bge-small-en-v1.5 -> 384). [VectorStoreVector] requires a compile-time
    // constant, so this needs updating by hand if Embeddings:Model changes to a different model.
    public const int Dimensions = 384;
}

public sealed class DocumentChunk
{
    // Qdrant point IDs only support ulong or Guid, so the key can't be the natural
    // "source-chunkIndex" string; Source/ChunkIndex are kept as separate data fields instead.
    [VectorStoreKey]
    public Guid Id { get; set; } = Guid.NewGuid();

    [VectorStoreData]
    public string Source { get; set; } = string.Empty;

    [VectorStoreData]
    public int ChunkIndex { get; set; }

    [VectorStoreData]
    public string Content { get; set; } = string.Empty;

    // Raw text goes in here; the vector store's configured IEmbeddingGenerator turns it into
    // a vector automatically on upsert and on search, so no manual embedding calls are needed.
    [VectorStoreVector(EmbeddingSettings.Dimensions)]
    public string Embedding { get; set; } = string.Empty;
}
