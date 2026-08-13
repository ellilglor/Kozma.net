namespace Kozma.net.Src.Interfaces.Helpers;

public interface IFileReader
{
    Task<T> ReadAsync<T>(string filePath);
}
