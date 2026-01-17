using StargazeWeatherConditions.Models;

namespace StargazeWeatherConditions.Utilities;

/// <summary>
/// Interface for recommendation scoring algorithm.
/// </summary>
public interface IRecommendationScorer
{
    /// <summary>
    /// Calculates a recommendation based on weather and astronomical conditions.
    /// </summary>
    Recommendation CalculateRecommendation(
        HourlyCondition conditions,
        AstronomyData astronomy,
        int? bortleScale);
}

/// <summary>
/// Implements weighted scoring algorithm for stargazing recommendations.
/// </summary>
public class RecommendationScorer : IRecommendationScorer
{
    // Weight distribution
    private const double CloudCoverWeight = 0.35;
    private const double MoonIlluminationWeight = 0.25;
    private const double HumidityWeight = 0.15;
    private const double VisibilityWeight = 0.15;
    private const double LightPollutionWeight = 0.10;

    // DSO-specific weights
    private const double DsoCloudWeight = 0.40;
    private const double DsoMoonWeight = 0.35;
    private const double DsoLightPollutionWeight = 0.15;
    private const double DsoVisibilityWeight = 0.10;

    // Planetary/Lunar weights
    private const double PlanetaryCloudWeight = 0.50;
    private const double PlanetaryVisibilityWeight = 0.30;
    private const double PlanetaryHumidityWeight = 0.20;

    public Recommendation CalculateRecommendation(
        HourlyCondition conditions,
        AstronomyData astronomy,
        int? bortleScale)
    {
        var cloudScore = ScoreCloudCover(conditions.CloudCoverPercent);
        var moonScore = ScoreMoonIllumination(astronomy.MoonIlluminationPercent, astronomy.IsMoonUp);
        var humidityScore = ScoreHumidity(conditions.HumidityPercent);
        var visibilityScore = ScoreVisibility(conditions.VisibilityKm);
        var lightPollutionScore = bortleScale.HasValue
            ? ScoreLightPollution(bortleScale.Value)
            : 50.0; // Neutral if unavailable

        var overallScore =
            (cloudScore * CloudCoverWeight) +
            (moonScore * MoonIlluminationWeight) +
            (humidityScore * HumidityWeight) +
            (visibilityScore * VisibilityWeight) +
            (lightPollutionScore * LightPollutionWeight);

        var dsoScore =
            (cloudScore * DsoCloudWeight) +
            (moonScore * DsoMoonWeight) +
            (lightPollutionScore * DsoLightPollutionWeight) +
            (visibilityScore * DsoVisibilityWeight);

        var planetaryScore =
            (cloudScore * PlanetaryCloudWeight) +
            (visibilityScore * PlanetaryVisibilityWeight) +
            (humidityScore * PlanetaryHumidityWeight);

        var factorScores = new List<FactorScore>
        {
            new()
            {
                FactorName = "Cloud Cover",
                Score = cloudScore,
                Weight = CloudCoverWeight,
                Label = GetCloudLabel(conditions.CloudCoverPercent),
                RawValue = conditions.CloudCoverPercent,
                Unit = "%"
            },
            new()
            {
                FactorName = "Moon Illumination",
                Score = moonScore,
                Weight = MoonIlluminationWeight,
                Label = GetMoonLabel(astronomy.MoonIlluminationPercent),
                RawValue = astronomy.MoonIlluminationPercent,
                Unit = "%"
            },
            new()
            {
                FactorName = "Humidity",
                Score = humidityScore,
                Weight = HumidityWeight,
                Label = GetHumidityLabel(conditions.HumidityPercent),
                RawValue = conditions.HumidityPercent,
                Unit = "%"
            },
            new()
            {
                FactorName = "Visibility",
                Score = visibilityScore,
                Weight = VisibilityWeight,
                Label = GetVisibilityLabel(conditions.VisibilityKm),
                RawValue = conditions.VisibilityKm,
                Unit = "km"
            }
        };

        if (bortleScale.HasValue)
        {
            factorScores.Add(new FactorScore
            {
                FactorName = "Light Pollution",
                Score = lightPollutionScore,
                Weight = LightPollutionWeight,
                Label = GetLightPollutionLabel(bortleScale.Value),
                RawValue = bortleScale.Value,
                Unit = "Bortle"
            });
        }

        return new Recommendation
        {
            OverallScore = overallScore,
            Rating = GetRating(overallScore),
            Summary = GenerateSummary(cloudScore, moonScore, astronomy.MoonPhase, overallScore),
            FactorScores = factorScores,
            DsoScore = dsoScore,
            PlanetaryScore = planetaryScore,
            CalculatedFor = conditions.Time
        };
    }

    #region Scoring Methods

    public static double ScoreCloudCover(int cloudPercent) => cloudPercent switch
    {
        <= 10 => 100,
        <= 20 => 90,
        <= 30 => 75,
        <= 40 => 60,
        <= 50 => 50,
        <= 60 => 35,
        <= 70 => 20,
        <= 80 => 10,
        _ => 0
    };

    public static double ScoreMoonIllumination(int illuminationPercent, bool isMoonUp)
    {
        // If moon is not up, conditions are excellent regardless of illumination
        if (!isMoonUp)
        {
            return 100;
        }

        return illuminationPercent switch
        {
            <= 5 => 100,
            <= 10 => 95,
            <= 20 => 85,
            <= 30 => 75,
            <= 40 => 60,
            <= 50 => 50,
            <= 60 => 35,
            <= 75 => 25,
            <= 85 => 15,
            _ => 10
        };
    }

    public static double ScoreHumidity(int humidityPercent) => humidityPercent switch
    {
        <= 40 => 100,
        <= 50 => 90,
        <= 60 => 75,
        <= 70 => 60,
        <= 75 => 50,
        <= 80 => 35,
        <= 85 => 20,
        <= 90 => 10,
        _ => 0
    };

    public static double ScoreVisibility(double visibilityKm) => visibilityKm switch
    {
        > 15 => 100,
        > 12 => 90,
        > 10 => 80,
        > 8 => 70,
        > 6 => 55,
        > 5 => 45,
        > 4 => 30,
        > 3 => 15,
        _ => 0
    };

    public static double ScoreLightPollution(int bortleScale) => bortleScale switch
    {
        1 => 100,
        2 => 95,
        3 => 85,
        4 => 70,
        5 => 50,
        6 => 30,
        7 => 15,
        8 => 5,
        _ => 0
    };

    #endregion

    #region Label Methods

    private static string GetCloudLabel(int cloudPercent) => cloudPercent switch
    {
        <= 10 => "Clear",
        <= 25 => "Mostly Clear",
        <= 50 => "Partly Cloudy",
        <= 75 => "Mostly Cloudy",
        _ => "Overcast"
    };

    private static string GetMoonLabel(int illumination) => illumination switch
    {
        <= 5 => "New Moon",
        <= 25 => "Crescent",
        <= 50 => "Quarter",
        <= 75 => "Gibbous",
        _ => "Full Moon"
    };

    private static string GetHumidityLabel(int humidity) => humidity switch
    {
        <= 50 => "Low",
        <= 70 => "Moderate",
        <= 85 => "High",
        _ => "Very High"
    };

    private static string GetVisibilityLabel(double visibilityKm) => visibilityKm switch
    {
        > 10 => "Excellent",
        > 6 => "Good",
        > 3 => "Moderate",
        _ => "Poor"
    };

    private static string GetLightPollutionLabel(int bortle) => bortle switch
    {
        <= 2 => "Dark Sky",
        <= 4 => "Rural",
        <= 6 => "Suburban",
        _ => "Urban"
    };

    #endregion

    #region Rating & Summary

    public static Rating GetRating(double score) => score switch
    {
        >= 85 => Rating.Excellent,
        >= 70 => Rating.Good,
        >= 50 => Rating.Fair,
        _ => Rating.Poor
    };

    private static string GenerateSummary(
        double cloudScore, 
        double moonScore, 
        string moonPhase,
        double overallScore)
    {
        if (overallScore >= 85)
        {
            if (moonScore >= 80)
                return $"Excellent for deep sky imaging - {moonPhase}, clear skies";
            return "Excellent conditions for planetary and lunar observation";
        }

        if (overallScore >= 70)
        {
            if (moonScore >= 60)
                return $"Good conditions for most targets - {moonPhase}";
            return "Good for planets and brighter deep sky objects";
        }

        if (overallScore >= 50)
        {
            if (cloudScore < 50)
                return "Fair conditions - cloud cover may interfere";
            return "Fair conditions - consider brighter targets";
        }

        if (cloudScore < 30)
            return "Poor conditions - high cloud cover expected";
        
        return "Poor conditions - consider postponing observation";
    }

    #endregion
}
