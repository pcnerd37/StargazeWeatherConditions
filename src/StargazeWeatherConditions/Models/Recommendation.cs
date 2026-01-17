namespace StargazeWeatherConditions.Models;

/// <summary>
/// Represents a stargazing recommendation based on weather and astronomical conditions.
/// </summary>
public record Recommendation
{
    public required double OverallScore { get; init; }
    public required Rating Rating { get; init; }
    public required string Summary { get; init; }
    public required IReadOnlyList<FactorScore> FactorScores { get; init; }
    public required double DsoScore { get; init; }
    public required double PlanetaryScore { get; init; }
    public required DateTime CalculatedFor { get; init; }

    /// <summary>
    /// Gets the CSS color class for this rating.
    /// </summary>
    public string RatingColorClass => Rating switch
    {
        Rating.Excellent => "rating-excellent",
        Rating.Good => "rating-good",
        Rating.Fair => "rating-fair",
        Rating.Poor => "rating-poor",
        _ => "rating-poor"
    };
}

/// <summary>
/// Represents a single factor's contribution to the overall score.
/// </summary>
public record FactorScore
{
    public required string FactorName { get; init; }
    public required double Score { get; init; }
    public required double Weight { get; init; }
    public required string Label { get; init; }
    public required double RawValue { get; init; }
    public string? Unit { get; init; }

    /// <summary>
    /// Gets the weighted contribution to the overall score.
    /// </summary>
    public double WeightedScore => Score * Weight;
}

/// <summary>
/// Overall rating categories for stargazing conditions.
/// </summary>
public enum Rating
{
    Excellent,
    Good,
    Fair,
    Poor
}

/// <summary>
/// Types of astronomical observation.
/// </summary>
public enum ObservationType
{
    DeepSky,
    Planetary,
    Lunar,
    General
}
