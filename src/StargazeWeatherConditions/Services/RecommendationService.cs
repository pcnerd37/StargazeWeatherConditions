using StargazeWeatherConditions.Models;
using StargazeWeatherConditions.Utilities;

namespace StargazeWeatherConditions.Services;

/// <summary>
/// Implementation of recommendation service using weighted scoring.
/// </summary>
public class RecommendationService : IRecommendationService
{
    private readonly IRecommendationScorer _scorer;

    public RecommendationService(IRecommendationScorer scorer)
    {
        _scorer = scorer;
    }

    public Recommendation CalculateRecommendation(
        HourlyCondition conditions,
        AstronomyData astronomy,
        LightPollutionData? lightPollution = null)
    {
        return _scorer.CalculateRecommendation(conditions, astronomy, lightPollution?.BortleClass);
    }

    public IReadOnlyList<(HourlyCondition Hour, Recommendation Recommendation)> GetBestHours(
        ForecastDay forecastDay,
        LightPollutionData? lightPollution = null,
        int maxHours = 5)
    {
        var nightHours = forecastDay.GetNighttimeHours().ToList();
        
        if (nightHours.Count == 0)
        {
            return Array.Empty<(HourlyCondition, Recommendation)>();
        }

        var recommendations = nightHours
            .Select(hour => (
                Hour: hour,
                Recommendation: CalculateRecommendation(hour, forecastDay.Astronomy, lightPollution)
            ))
            .OrderByDescending(x => x.Recommendation.OverallScore)
            .Take(maxHours)
            .ToList();

        return recommendations;
    }

    public Recommendation GetNightOverview(
        ForecastDay forecastDay,
        LightPollutionData? lightPollution = null)
    {
        var nightHours = forecastDay.GetNighttimeHours().ToList();

        if (nightHours.Count == 0)
        {
            // Return a poor recommendation if no night hours
            return new Recommendation
            {
                OverallScore = 0,
                Rating = Rating.Poor,
                Summary = "No nighttime hours available for this date.",
                FactorScores = Array.Empty<FactorScore>(),
                DsoScore = 0,
                PlanetaryScore = 0,
                CalculatedFor = forecastDay.Date.ToDateTime(TimeOnly.MinValue)
            };
        }

        // Calculate average conditions for the night
        var avgCloud = nightHours.Average(h => h.CloudCoverPercent);
        var avgHumidity = nightHours.Average(h => h.HumidityPercent);
        var avgVisibility = nightHours.Average(h => h.VisibilityKm);
        var minCloud = nightHours.Min(h => h.CloudCoverPercent);

        // Use the hour with the best (lowest) cloud cover as representative
        var bestHour = nightHours.OrderBy(h => h.CloudCoverPercent).First();
        var recommendation = CalculateRecommendation(bestHour, forecastDay.Astronomy, lightPollution);

        // Adjust summary to be night-level
        var summary = GenerateNightSummary(
            avgCloud, 
            minCloud, 
            forecastDay.Astronomy.MoonIlluminationPercent,
            forecastDay.Astronomy.MoonPhase,
            recommendation.Rating);

        return recommendation with
        {
            Summary = summary,
            CalculatedFor = forecastDay.Date.ToDateTime(forecastDay.Astronomy.Sunset)
        };
    }

    private static string GenerateNightSummary(
        double avgCloud,
        int minCloud,
        int moonIllumination,
        string moonPhase,
        Rating rating)
    {
        var cloudDesc = avgCloud switch
        {
            <= 20 => "Clear skies expected",
            <= 40 => "Mostly clear skies",
            <= 60 => "Partly cloudy",
            <= 80 => "Mostly cloudy",
            _ => "Overcast skies"
        };

        var moonDesc = moonIllumination switch
        {
            <= 10 => "excellent dark sky conditions",
            <= 30 => "minimal moonlight",
            <= 50 => "moderate moonlight",
            <= 70 => "significant moonlight",
            _ => "bright moonlight"
        };

        var targetRecommendation = rating switch
        {
            Rating.Excellent when moonIllumination <= 30 => 
                "Ideal for deep sky objects and astrophotography.",
            Rating.Excellent => 
                "Great for planetary and lunar observation.",
            Rating.Good when moonIllumination <= 50 => 
                "Good conditions for most targets.",
            Rating.Good => 
                "Best for planets and the Moon.",
            Rating.Fair => 
                "Consider bright objects only.",
            Rating.Poor => 
                "Consider postponing observation.",
            _ => ""
        };

        return $"{cloudDesc} with {moonDesc} ({moonPhase}). {targetRecommendation}";
    }
}
