namespace Kozma.net.Helpers;

public interface IFileReader
{
    Task<T?> ReadAsync<T>(string filePath);
}
