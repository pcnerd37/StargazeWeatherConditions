namespace StargazeWeatherConditions.Models;

/// <summary>
/// Represents light pollution data for a location.
/// </summary>
public record LightPollutionData
{
    public required double Latitude { get; init; }
    public required double Longitude { get; init; }
    
    /// <summary>
    /// Bortle scale value (1-9).
    /// </summary>
    public required int BortleClass { get; init; }

    /// <summary>
    /// Alias for BortleClass for compatibility.
    /// </summary>
    public int BortleScale => BortleClass;
    
    /// <summary>
    /// Artificial brightness in mcd/m² (optional).
    /// </summary>
    public double? ArtificialBrightness { get; init; }
    
    /// <summary>
    /// Naked-eye limiting magnitude estimate.
    /// </summary>
    public double? NakedEyeLimitingMagnitude { get; init; }

    public DateTime RetrievedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Gets a description of the Bortle class.
    /// </summary>
    public string BortleDescription => BortleClass switch
    {
        1 => "Excellent dark-sky site",
        2 => "Typical dark site",
        3 => "Rural sky",
        4 => "Rural/suburban transition",
        5 => "Suburban sky",
        6 => "Bright suburban sky",
        7 => "Suburban/urban transition",
        8 => "City sky",
        9 => "Inner-city sky",
        _ => "Unknown"
    };

    /// <summary>
    /// Gets the quality rating for stargazing.
    /// </summary>
    public string QualityRating => BortleClass switch
    {
        <= 2 => "Excellent",
        3 => "Very Good",
        4 => "Good",
        5 => "Moderate",
        6 => "Fair",
        7 => "Poor",
        _ => "Very Poor"
    };

    /// <summary>
    /// Indicates if the location is suitable for deep-sky observation.
    /// </summary>
    public bool IsSuitableForDeepSky => BortleClass <= 5;
}
