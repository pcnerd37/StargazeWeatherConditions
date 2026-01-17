namespace StargazeWeatherConditions.Models;

/// <summary>
/// User preferences for display and functionality.
/// </summary>
public record UserPreferences
{
    /// <summary>
    /// Whether dark theme is enabled.
    /// </summary>
    public bool IsDarkTheme { get; init; } = true;
    
    /// <summary>
    /// Whether to use metric units (Celsius, km, kph) vs imperial (Fahrenheit, miles, mph).
    /// </summary>
    public bool UseMetricUnits { get; init; } = false;
    
    /// <summary>
    /// Whether to use 24-hour time format.
    /// </summary>
    public bool Use24HourTime { get; init; } = false;

    /// <summary>
    /// Creates a copy with dark theme toggled.
    /// </summary>
    public UserPreferences ToggleTheme() => this with { IsDarkTheme = !IsDarkTheme };

    /// <summary>
    /// Creates a copy with unit system toggled.
    /// </summary>
    public UserPreferences ToggleUnits() => this with { UseMetricUnits = !UseMetricUnits };

    /// <summary>
    /// Creates a copy with time format toggled.
    /// </summary>
    public UserPreferences ToggleTimeFormat() => this with { Use24HourTime = !Use24HourTime };
}
