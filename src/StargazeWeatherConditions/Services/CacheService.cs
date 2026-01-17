using System.Text.Json;
using Microsoft.JSInterop;
using StargazeWeatherConditions.Models;

namespace StargazeWeatherConditions.Services;

/// <summary>
/// Implementation of cache service using browser localStorage.
/// </summary>
public class CacheService : ICacheService
{
    private readonly IJSRuntime _jsRuntime;
    private static readonly TimeSpan DefaultTtl = TimeSpan.FromHours(2);
    private const string CachePrefix = "stargaze_";

    public CacheService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task<CacheResult<T>> GetAsync<T>(string key)
    {
        try
        {
            var fullKey = GetFullKey(key);
            var json = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", fullKey);
            
            if (string.IsNullOrEmpty(json))
            {
                return CacheResult<T>.Miss();
            }

            var entry = JsonSerializer.Deserialize<CacheEntry<T>>(json);
            
            if (entry is null)
            {
                return CacheResult<T>.Miss();
            }

            // Check if cache is from a different date (stale for weather data)
            var isStale = entry.IsExpired || !entry.IsCurrentDate;

            return CacheResult<T>.Hit(entry.Data, entry.Timestamp, isStale);
        }
        catch (JsonException)
        {
            // Invalid cache data, remove it
            await RemoveAsync(key);
            return CacheResult<T>.Miss();
        }
        catch (JSException)
        {
            // JS interop failed (e.g., during prerendering)
            return CacheResult<T>.Miss();
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? ttl = null)
    {
        try
        {
            var fullKey = GetFullKey(key);
            var entry = new CacheEntry<T>
            {
                Data = value,
                Timestamp = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.Add(ttl ?? DefaultTtl),
                DataDate = DateOnly.FromDateTime(DateTime.Now)
            };

            var json = JsonSerializer.Serialize(entry);
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", fullKey, json);
        }
        catch (JSException)
        {
            // JS interop failed, ignore
        }
    }

    public async Task RemoveAsync(string key)
    {
        try
        {
            var fullKey = GetFullKey(key);
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", fullKey);
        }
        catch (JSException)
        {
            // JS interop failed, ignore
        }
    }

    public async Task ClearAsync()
    {
        try
        {
            // Get all keys and remove those with our prefix
            var length = await _jsRuntime.InvokeAsync<int>("eval", "localStorage.length");
            var keysToRemove = new List<string>();

            for (var i = 0; i < length; i++)
            {
                var key = await _jsRuntime.InvokeAsync<string>("localStorage.key", i);
                if (key?.StartsWith(CachePrefix) == true)
                {
                    keysToRemove.Add(key);
                }
            }

            foreach (var key in keysToRemove)
            {
                await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", key);
            }
        }
        catch (JSException)
        {
            // JS interop failed, ignore
        }
    }

    private static string GetFullKey(string key) => $"{CachePrefix}{key}";

    public async Task<CacheResult<T>> GetWithMetadataAsync<T>(string key)
    {
        return await GetAsync<T>(key);
    }
}
