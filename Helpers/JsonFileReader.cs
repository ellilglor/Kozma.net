using System.Text.Json;

namespace Kozma.net.Helpers;

public class JsonFileReader : IFileReader
{
    public async Task<T?> ReadAsync<T>(string filePath)
    {
        using FileStream stream = File.OpenRead(filePath);
        return await JsonSerializer.DeserializeAsync<T>(stream);
    }
}
