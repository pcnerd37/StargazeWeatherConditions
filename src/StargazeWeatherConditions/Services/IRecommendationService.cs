using StargazeWeatherConditions.Models;

namespace StargazeWeatherConditions.Services;

/// <summary>
/// Service for calculating stargazing recommendations.
/// </summary>
public interface IRecommendationService
{
    /// <summary>
    /// Calculates a recommendation for a specific hour.
    /// </summary>
    Recommendation CalculateRecommendation(
        HourlyCondition conditions,
        AstronomyData astronomy,
        LightPollutionData? lightPollution = null);

    /// <summary>
    /// Calculates the best hours for stargazing from a forecast day.
    /// </summary>
    IReadOnlyList<(HourlyCondition Hour, Recommendation Recommendation)> GetBestHours(
        ForecastDay forecastDay,
        LightPollutionData? lightPollution = null,
        int maxHours = 5);

    /// <summary>
    /// Gets the overall recommendation for a night.
    /// </summary>
    Recommendation GetNightOverview(
        ForecastDay forecastDay,
        LightPollutionData? lightPollution = null);
}
