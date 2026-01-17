namespace StargazeWeatherConditions.Models;

/// <summary>
/// Represents astronomical data for a given day/location.
/// </summary>
public record AstronomyData
{
    public required TimeOnly Sunrise { get; init; }
    public required TimeOnly Sunset { get; init; }
    public TimeOnly? Moonrise { get; init; }
    public TimeOnly? Moonset { get; init; }
    public required string MoonPhase { get; init; }
    public required int MoonIlluminationPercent { get; init; }
    public required bool IsMoonUp { get; init; }
    public required bool IsSunUp { get; init; }

    /// <summary>
    /// Gets a description of moon conditions for stargazing.
    /// </summary>
    public string MoonDescription => MoonIlluminationPercent switch
    {
        <= 10 => "Excellent - New Moon",
        <= 30 => "Very Good - Crescent",
        <= 50 => "Moderate - Quarter",
        <= 70 => "Fair - Gibbous",
        _ => "Bright - Full Moon"
    };

    /// <summary>
    /// Indicates if moon conditions are favorable for deep-sky observation.
    /// </summary>
    public bool IsFavorableForDeepSky => MoonIlluminationPercent <= 30 || !IsMoonUp;
}
