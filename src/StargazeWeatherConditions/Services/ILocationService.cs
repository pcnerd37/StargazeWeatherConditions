using StargazeWeatherConditions.Models;

namespace StargazeWeatherConditions.Services;

/// <summary>
/// Service for location-related functionality.
/// </summary>
public interface ILocationService
{
    /// <summary>
    /// Gets the current location using browser geolocation.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Location coordinates or null if unavailable/denied.</returns>
    Task<(double Latitude, double Longitude)?> GetCurrentLocationAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the last used location from session storage.
    /// </summary>
    Task<LocationInfo?> GetLastLocationAsync();

    /// <summary>
    /// Saves the current location to session storage.
    /// </summary>
    Task SaveLastLocationAsync(LocationInfo location);

    /// <summary>
    /// Checks if geolocation is available in the browser.
    /// </summary>
    Task<bool> IsGeolocationAvailableAsync();

    /// <summary>
    /// Gets the current position with error handling.
    /// </summary>
    Task<GeolocationResult> GetCurrentPositionAsync();
}

/// <summary>
/// Result of a geolocation request.
/// </summary>
public record GeolocationResult(
    bool Success,
    double? Latitude,
    double? Longitude,
    string? Error);
