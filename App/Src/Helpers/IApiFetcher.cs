using System.Text.Json;

namespace Kozma.net.Src.Helpers;

public interface IApiFetcher
{
    Task<T> FetchAsync<T>(string url, JsonSerializerOptions options);
}
