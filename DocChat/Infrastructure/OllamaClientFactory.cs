using DocChat.Configuration;
using OllamaSharp;

namespace DocChat.Infrastructure;

public static class OllamaClientFactory
{
    public static async Task<OllamaApiClient> CreateAsync(OllamaOptions options, CancellationToken cancellationToken = default)
    {
        // Local CPU inference (and tool-calling round-trips) can comfortably exceed the
        // HttpClient default of 100 seconds, especially for the first request after startup.
        var httpClient = new HttpClient
        {
            BaseAddress = new Uri(options.BaseUrl),
            Timeout = TimeSpan.FromMinutes(10)
        };

        var client = new OllamaApiClient(httpClient)
        {
            SelectedModel = options.ChatModel
        };

        await EnsureModelIsPulledAsync(client, options.ChatModel, cancellationToken);

        return client;
    }

    private static async Task EnsureModelIsPulledAsync(OllamaApiClient client, string model, CancellationToken cancellationToken)
    {
        var localModels = await client.ListLocalModelsAsync(cancellationToken);
        if (localModels.Any(m => m.Name.Equals(model, StringComparison.OrdinalIgnoreCase)))
            return;

        Console.WriteLine($"Pulling Ollama model '{model}' (first run only, this can take a while)...");

        var lastReportedPercent = -1;

        await foreach (var progress in client.PullModelAsync(model, cancellationToken))
        {
            var percent = (int)(progress?.Percent ?? 0);
            if (percent == lastReportedPercent)
                continue;

            lastReportedPercent = percent;
            Console.Write($"\r  {progress?.Status,-20} {percent,3}%   ");
        }

        Console.WriteLine();
        Console.WriteLine($"Model '{model}' ready.");
    }
}
