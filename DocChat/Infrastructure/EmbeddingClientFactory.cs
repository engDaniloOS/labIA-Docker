using System.ClientModel;
using DocChat.Configuration;
using Microsoft.Extensions.AI;
using OpenAI;

namespace DocChat.Infrastructure;

public static class EmbeddingClientFactory
{
    // The embeddings container (Hugging Face TEI) exposes an OpenAI-compatible endpoint, so we
    // reuse the OpenAI client instead of hand-rolling HTTP calls; it doesn't check the API key.
    public static IEmbeddingGenerator<string, Embedding<float>> Create(EmbeddingsOptions options)
    {
        var clientOptions = new OpenAIClientOptions { Endpoint = new Uri(options.BaseUrl) };
        var openAiClient = new OpenAIClient(new ApiKeyCredential("not-needed"), clientOptions);

        return openAiClient
            .GetEmbeddingClient(options.Model)
            .AsIEmbeddingGenerator();
    }
}
