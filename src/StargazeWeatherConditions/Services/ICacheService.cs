using StargazeWeatherConditions.Models;

namespace StargazeWeatherConditions.Services;

/// <summary>
/// Service for caching data in browser local storage.
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Gets a cached item.
    /// </summary>
    /// <typeparam name="T">Type of cached data.</typeparam>
    /// <param name="key">Cache key.</param>
    /// <returns>Cache result with data if found.</returns>
    Task<CacheResult<T>> GetAsync<T>(string key);

    /// <summary>
    /// Sets a cached item.
    /// </summary>
    /// <typeparam name="T">Type of data to cache.</typeparam>
    /// <param name="key">Cache key.</param>
    /// <param name="value">Value to cache.</param>
    /// <param name="ttl">Time to live (optional, defaults to 2 hours).</param>
    Task SetAsync<T>(string key, T value, TimeSpan? ttl = null);

    /// <summary>
    /// Removes a cached item.
    /// </summary>
    /// <param name="key">Cache key.</param>
    Task RemoveAsync(string key);

    /// <summary>
    /// Clears all cached items.
    /// </summary>
    Task ClearAsync();

    /// <summary>
    /// Gets a cached item with metadata about staleness.
    /// </summary>
    Task<CacheResult<T>> GetWithMetadataAsync<T>(string key);
}
