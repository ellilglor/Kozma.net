using System.Text.Json;

namespace Kozma.net.Src.Interfaces.Helpers;

public interface IApiFetcher
{
    Task<T> FetchAsync<T>(string url, JsonSerializerOptions options);
}
