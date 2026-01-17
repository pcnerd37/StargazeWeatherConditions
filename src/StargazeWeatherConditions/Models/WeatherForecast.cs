namespace StargazeWeatherConditions.Models;

/// <summary>
/// Represents a complete weather forecast response.
/// </summary>
public record WeatherForecast
{
    public required LocationInfo Location { get; init; }
    public required IReadOnlyList<ForecastDay> ForecastDays { get; init; }
    public DateTime RetrievedAt { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// Represents a single day's forecast with astronomy and hourly data.
/// </summary>
public record ForecastDay
{
    public required DateOnly Date { get; init; }
    public required DaySummary DaySummary { get; init; }
    public required AstronomyData Astronomy { get; init; }
    public required IReadOnlyList<HourlyCondition> HourlyConditions { get; init; }
    
    /// <summary>
    /// Gets the nighttime hours (after sunset until midnight, then midnight until sunrise next day).
    /// </summary>
    public IEnumerable<HourlyCondition> GetNighttimeHours()
    {
        return HourlyConditions.Where(h => !h.IsDay);
    }
}

/// <summary>
/// Represents daily summary conditions.
/// </summary>
public record DaySummary
{
    public required double MaxTempCelsius { get; init; }
    public required double MaxTempFahrenheit { get; init; }
    public required double MinTempCelsius { get; init; }
    public required double MinTempFahrenheit { get; init; }
    public required double AvgTempCelsius { get; init; }
    public required double AvgTempFahrenheit { get; init; }
    public required double MaxWindMph { get; init; }
    public required double MaxWindKph { get; init; }
    public required double TotalPrecipMm { get; init; }
    public required double TotalPrecipIn { get; init; }
    public required double AvgVisibilityKm { get; init; }
    public required double AvgVisibilityMiles { get; init; }
    public required int AvgHumidityPercent { get; init; }
    public required int ChanceOfRainPercent { get; init; }
    public required int ChanceOfSnowPercent { get; init; }
    public required WeatherCondition Condition { get; init; }
}
