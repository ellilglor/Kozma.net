using System.Text.Json;

namespace Kozma.net.Src.Helpers;

public class ApiFetcher : IApiFetcher
{
    public async Task<T> FetchAsync<T>(string url, JsonSerializerOptions options)
    {
        using var client = new HttpClient();
        var response = await client.GetAsync(new Uri(url));
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(json, options) ?? throw new ArgumentNullException($"Failed to parse content from {url}");
    }
}
