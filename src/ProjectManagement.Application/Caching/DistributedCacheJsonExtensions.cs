using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using System.Threading.Tasks;

namespace ProjectManagement.Caching;

public static class DistributedCacheJsonExtensions
{
    private static readonly JsonSerializerOptions DefaultSerializerOptions = new(JsonSerializerDefaults.Web);

    public static async Task<T?> GetJsonAsync<T>(this IDistributedCache distributedCache, string key, JsonSerializerOptions? serializerOptions = null)
    {
        var cachedValue = await distributedCache.GetStringAsync(key);
        return cachedValue == null ? default : JsonSerializer.Deserialize<T>(cachedValue, serializerOptions ?? DefaultSerializerOptions);
    }

    public static async Task SetJsonAsync<T>(this IDistributedCache distributedCache, string key, T value, DistributedCacheEntryOptions options, JsonSerializerOptions? serializerOptions = null)
    {
        var serializedValue = JsonSerializer.Serialize(value, serializerOptions ?? DefaultSerializerOptions);
        await distributedCache.SetStringAsync(key, serializedValue, options);
    }
}