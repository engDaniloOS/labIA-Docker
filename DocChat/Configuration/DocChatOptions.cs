namespace DocChat.Configuration;

public sealed class DocChatOptions
{
    public OllamaOptions Ollama { get; set; } = new();
    public EmbeddingsOptions Embeddings { get; set; } = new();
    public QdrantOptions Qdrant { get; set; } = new();
    public ChunkingOptions Chunking { get; set; } = new();
    public SessionOptions Session { get; set; } = new();
    public DocumentsOptions Documents { get; set; } = new();
}

public sealed class OllamaOptions
{
    public string BaseUrl { get; set; } = "http://localhost:11434";
    public string ChatModel { get; set; } = "qwen3:1.7b";
}

public sealed class EmbeddingsOptions
{
    // Must end in /v1 — the OpenAI-compatible client appends /embeddings to this base address.
    public string BaseUrl { get; set; } = "http://localhost:8080/v1";
    public string Model { get; set; } = "BAAI/bge-small-en-v1.5";
}

public sealed class QdrantOptions
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 6334;
    public bool Https { get; set; } = false;
    public string CollectionName { get; set; } = "doc-chat";
}

public sealed class ChunkingOptions
{
    public int MaxCharsPerLine { get; set; } = 100;
    public int MaxCharsPerParagraph { get; set; } = 400;
    public int OverlapChars { get; set; } = 60;
}

public sealed class SessionOptions
{
    public int HistoryWindowSize { get; set; } = 12;
}

public sealed class DocumentsOptions
{
    public string Path { get; set; } = "KnowledgeBase";
}
