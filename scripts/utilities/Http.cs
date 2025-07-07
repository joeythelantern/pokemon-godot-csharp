using System;
using System.Net.Http;
using System.Threading.Tasks;
using Game.Core;
using Newtonsoft.Json.Linq;

namespace Game.Utilities;

public static class HttpModule
{
    public static async Task<JObject?> FetchDataFromPokeApi(HttpClient _httpClient, string endpoint)
    {
        string url = $"https://pokeapi.co/api/v2/{endpoint}/";
        const int maxAttempts = 3;
        const int retryDelayMs = 1000; // 1 second

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            try
            {
                var response = await _httpClient.GetAsync(url);
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    Logger.Error($"404 Not Found for {endpoint}.");
                    return null;
                }

                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                return JObject.Parse(json);
            }
            catch (Exception e)
            {
                Logger.Error($"Error fetching {endpoint} (attempt {attempt + 1}/{maxAttempts}): {e.Message}");
                if (attempt < maxAttempts - 1)
                {
                    await Task.Delay(retryDelayMs);
                }
            }
        }

        Logger.Error($"Failed to fetch {endpoint} after {maxAttempts} retries.");
        return null;
    }
}