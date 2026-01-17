namespace StargazeWeatherConditions.Models;

/// <summary>
/// Represents a cache entry with metadata for expiration checking.
/// </summary>
/// <typeparam name="T">The type of data being cached.</typeparam>
public record CacheEntry<T>
{
    public required T Data { get; init; }
    public required DateTime Timestamp { get; init; }
    public required DateTime ExpiresAt { get; init; }
    public required DateOnly DataDate { get; init; }

    /// <summary>
    /// Indicates if the cache entry has expired.
    /// </summary>
    public bool IsExpired => DateTime.UtcNow > ExpiresAt;

    /// <summary>
    /// Indicates if the cached data is from the current date.
    /// </summary>
    public bool IsCurrentDate => DataDate == DateOnly.FromDateTime(DateTime.Now);

    /// <summary>
    /// Gets the age of the cached data.
    /// </summary>
    public TimeSpan Age => DateTime.UtcNow - Timestamp;
}

/// <summary>
/// Result of attempting to retrieve cached data.
/// </summary>
/// <typeparam name="T">The type of cached data.</typeparam>
public record CacheResult<T>
{
    public T? Data { get; init; }
    public bool IsHit { get; init; }
    public bool IsStale { get; init; }
    public DateTime? CachedAt { get; init; }
    
    /// <summary>
    /// Alias for CachedAt for backward compatibility.
    /// </summary>
    public DateTime Timestamp => CachedAt ?? DateTime.MinValue;

    public static CacheResult<T> Hit(T data, DateTime cachedAt, bool isStale = false) 
        => new() { Data = data, IsHit = true, IsStale = isStale, CachedAt = cachedAt };

    public static CacheResult<T> Miss() 
        => new() { Data = default, IsHit = false, IsStale = false, CachedAt = null };
}
