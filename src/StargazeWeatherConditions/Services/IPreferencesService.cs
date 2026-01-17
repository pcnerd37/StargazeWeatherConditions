using StargazeWeatherConditions.Models;

namespace StargazeWeatherConditions.Services;

/// <summary>
/// Service for managing user preferences.
/// </summary>
public interface IPreferencesService
{
    /// <summary>
    /// Gets the current user preferences.
    /// </summary>
    Task<UserPreferences> GetPreferencesAsync();

    /// <summary>
    /// Saves user preferences.
    /// </summary>
    Task SavePreferencesAsync(UserPreferences preferences);

    /// <summary>
    /// Gets the custom API key if set.
    /// </summary>
    Task<string?> GetCustomApiKeyAsync();

    /// <summary>
    /// Sets a custom API key.
    /// </summary>
    Task SetCustomApiKeyAsync(string? apiKey);

    /// <summary>
    /// Clears the custom API key.
    /// </summary>
    Task ClearCustomApiKeyAsync();

    /// <summary>
    /// Checks if a custom API key is set.
    /// </summary>
    Task<bool> HasCustomApiKeyAsync();

    /// <summary>
    /// Event raised when preferences change.
    /// </summary>
    event EventHandler<UserPreferences>? PreferencesChanged;
}
