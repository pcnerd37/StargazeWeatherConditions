using StargazeWeatherConditions.Models;

namespace StargazeWeatherConditions.Services;

/// <summary>
/// Service for retrieving weather forecast data from WeatherAPI.com.
/// </summary>
public interface IWeatherApiService
{
    /// <summary>
    /// Gets a 3-day weather forecast for the specified location.
    /// </summary>
    /// <param name="location">Location query (city name, coordinates, or postal code).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Weather forecast data or null if unavailable.</returns>
    Task<WeatherForecast?> GetForecastAsync(
        string location, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a 3-day weather forecast for the specified coordinates.
    /// </summary>
    /// <param name="latitude">Latitude in decimal degrees.</param>
    /// <param name="longitude">Longitude in decimal degrees.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Weather forecast data or null if unavailable.</returns>
    Task<WeatherForecast?> GetForecastAsync(
        double latitude, 
        double longitude, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches for locations matching the query for autocomplete.
    /// </summary>
    /// <param name="query">Search query.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of matching locations.</returns>
    Task<IReadOnlyList<LocationSearchResult>> SearchLocationsAsync(
        string query, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates an API key by making a test request.
    /// </summary>
    /// <param name="apiKey">API key to validate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the API key is valid.</returns>
    Task<bool> ValidateApiKeyAsync(
        string apiKey, 
        CancellationToken cancellationToken = default);
}
