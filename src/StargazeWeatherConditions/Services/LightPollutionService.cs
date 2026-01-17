using StargazeWeatherConditions.Models;

namespace StargazeWeatherConditions.Services;

/// <summary>
/// Implementation of light pollution service.
/// Note: This is a simplified implementation that estimates Bortle class
/// based on a basic algorithm. In production, integrate with an actual 
/// light pollution map API.
/// </summary>
public class LightPollutionService : ILightPollutionService
{
    private readonly ICacheService _cacheService;

    public LightPollutionService(ICacheService cacheService)
    {
        _cacheService = cacheService;
    }

    public async Task<LightPollutionData?> GetLightPollutionAsync(
        double latitude,
        double longitude,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = $"lightpollution_{latitude:F4}_{longitude:F4}";
        var cacheResult = await _cacheService.GetAsync<LightPollutionData>(cacheKey);

        if (cacheResult.IsHit && cacheResult.Data is not null)
        {
            return cacheResult.Data;
        }

        try
        {
            // For now, return an estimated Bortle class
            // In production, this would call the Light Pollution Map API
            var bortleClass = EstimateBortleClass(latitude, longitude);

            var data = new LightPollutionData
            {
                Latitude = latitude,
                Longitude = longitude,
                BortleClass = bortleClass,
                RetrievedAt = DateTime.UtcNow
            };

            await _cacheService.SetAsync(cacheKey, data, TimeSpan.FromHours(24));
            return data;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Estimates Bortle class based on location.
    /// This is a placeholder - in production, use actual light pollution data.
    /// </summary>
    private static int EstimateBortleClass(double latitude, double longitude)
    {
        // This is a very simplified estimation
        // Real implementation would use actual light pollution map data
        
        // Default to suburban (Bortle 5) as a reasonable middle ground
        // Users can override or this can be enhanced with actual API data
        return 5;
    }
}
