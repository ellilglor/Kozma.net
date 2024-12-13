using System.Text.Json;

namespace Kozma.net.Src.Helpers;

public class JsonFileReader : IFileReader
{
    public async Task<T> ReadAsync<T>(string filePath)
    {
        var projectRoot = (Directory.GetParent(Environment.CurrentDirectory)?.Parent?.Parent?.FullName) ?? throw new DirectoryNotFoundException($"Failed to find projectRoot for {filePath}");

        using FileStream stream = File.OpenRead(Path.Combine(projectRoot, "Src", filePath));
        return await JsonSerializer.DeserializeAsync<T>(stream) ?? throw new FileNotFoundException();
    }
}
