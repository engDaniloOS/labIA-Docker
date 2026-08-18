namespace DocChat.Ingestion;

public static class DocumentLoader
{
    public static IReadOnlyList<(string Source, string Text)> LoadMarkdownFiles(string folderPath)
    {
        if (!Directory.Exists(folderPath))
            throw new DirectoryNotFoundException($"Knowledge base folder not found: {folderPath}");

        var documents = new List<(string Source, string Text)>();

        foreach (var filePath in Directory.EnumerateFiles(folderPath, "*.md", SearchOption.AllDirectories))
        {
            var source = Path.GetFileNameWithoutExtension(filePath);
            var text = File.ReadAllText(filePath);
            documents.Add((source, text));
        }

        return documents;
    }
}
