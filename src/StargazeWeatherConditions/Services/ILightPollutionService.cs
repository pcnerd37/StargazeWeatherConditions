using StargazeWeatherConditions.Models;

namespace StargazeWeatherConditions.Services;

/// <summary>
/// Service for retrieving light pollution data.
/// </summary>
public interface ILightPollutionService
{
    /// <summary>
    /// Gets light pollution data for the specified coordinates.
    /// </summary>
    /// <param name="latitude">Latitude in decimal degrees.</param>
    /// <param name="longitude">Longitude in decimal degrees.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Light pollution data or null if unavailable.</returns>
    Task<LightPollutionData?> GetLightPollutionAsync(
        double latitude, 
        double longitude, 
        CancellationToken cancellationToken = default);
}
