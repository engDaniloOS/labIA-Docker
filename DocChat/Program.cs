using DocChat.Agent;
using DocChat.Chat;
using DocChat.Configuration;
using DocChat.Infrastructure;
using DocChat.Ingestion;
using DocChat.Search;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;

var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: true)
    .AddEnvironmentVariables()
    .Build();

var options = configuration.Get<DocChatOptions>() ?? new DocChatOptions();

Console.WriteLine("Starting DocChat...");

var chatClient = await OllamaClientFactory.CreateAsync(options.Ollama);
var embeddingGenerator = EmbeddingClientFactory.Create(options.Embeddings);
var collection = QdrantStoreFactory.CreateCollection(options.Qdrant, embeddingGenerator);

var knowledgeBasePath = Path.Combine(AppContext.BaseDirectory, options.Documents.Path);
var chunkCount = await DocumentIndexer.IndexAsync(collection, knowledgeBasePath, options.Chunking);
Console.WriteLine($"Indexed {chunkCount} chunks from '{knowledgeBasePath}'.");

var searchTool = new SearchDocumentsTool(collection);
var searchAiTool = AIFunctionFactory.Create(searchTool.SearchDocumentsAsync);

var agent = DocChatAgentFactory.Create(chatClient, searchAiTool);
var session = new ChatSession(options.Session.HistoryWindowSize);

Console.WriteLine("Ready. Ask a question about the knowledge base (type 'exit' to quit).");

while (true)
{
    Console.Write("\n> ");
    var input = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(input) || input.Equals("exit", StringComparison.OrdinalIgnoreCase))
        break;

    session.AddUserMessage(input);

    var response = await agent.RunAsync(session.Messages);

    Console.WriteLine(response.Text);
    session.AddAssistantMessage(response.Text);
}
