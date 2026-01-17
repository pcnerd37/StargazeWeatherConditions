namespace StargazeWeatherConditions.Models;

/// <summary>
/// Represents hourly weather conditions.
/// </summary>
public record HourlyCondition
{
    public required DateTime Time { get; init; }
    public required long TimeEpoch { get; init; }
    public required double TemperatureCelsius { get; init; }
    public required double TemperatureFahrenheit { get; init; }
    public required double FeelsLikeCelsius { get; init; }
    public required double FeelsLikeFahrenheit { get; init; }
    public required int CloudCoverPercent { get; init; }
    public required int HumidityPercent { get; init; }
    public required double VisibilityKm { get; init; }
    public required double VisibilityMiles { get; init; }
    public required double WindSpeedMph { get; init; }
    public required double WindSpeedKph { get; init; }
    public required int WindDegree { get; init; }
    public required string WindDirection { get; init; }
    public required double GustMph { get; init; }
    public required double GustKph { get; init; }
    public required double PressureMb { get; init; }
    public required double PressureIn { get; init; }
    public required double PrecipMm { get; init; }
    public required double PrecipIn { get; init; }
    public required int ChanceOfRainPercent { get; init; }
    public required int ChanceOfSnowPercent { get; init; }
    public required double DewPointCelsius { get; init; }
    public required double DewPointFahrenheit { get; init; }
    public required double UvIndex { get; init; }
    public required WeatherCondition Condition { get; init; }
    public required bool IsDay { get; init; }
}

/// <summary>
/// Represents weather condition with icon and code.
/// </summary>
public record WeatherCondition
{
    public required string Text { get; init; }
    public required string IconUrl { get; init; }
    public required int Code { get; init; }
}
