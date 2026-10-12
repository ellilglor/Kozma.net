using Kozma.net.Src.Interfaces.Helpers;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Kozma.net.Src.Helpers;

public class ApiFetcher : IApiFetcher
{
    public async Task<T> FetchAsync<T>(string url, JsonSerializerOptions options, string? token = null)
    {
        using var client = new HttpClient
        {
            DefaultRequestHeaders =
            {
                Accept = { new MediaTypeWithQualityHeaderValue("application/json") }
            }
        };

        if (!string.IsNullOrWhiteSpace(token))
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Trim());

        var response = await client.GetAsync(new Uri(url));
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(json, options)
            ?? throw new ArgumentNullException($"Failed to parse content from {url}");
    }
}