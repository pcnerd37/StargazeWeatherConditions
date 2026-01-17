using StargazeWeatherConditions.Models;
using StargazeWeatherConditions.Utilities;

namespace StargazeWeatherConditions.Tests.Utilities;

public class RecommendationScorerTests
{
    private readonly RecommendationScorer _scorer = new();

    #region Cloud Cover Scoring Tests

    [Theory]
    [InlineData(0, 100)]
    [InlineData(10, 100)]
    [InlineData(15, 90)]
    [InlineData(25, 75)]
    [InlineData(35, 60)]
    [InlineData(45, 50)]
    [InlineData(55, 35)]
    [InlineData(65, 20)]
    [InlineData(75, 10)]
    [InlineData(85, 0)]
    [InlineData(100, 0)]
    public void ScoreCloudCover_ReturnsExpectedScore(int cloudPercent, double expectedScore)
    {
        var result = RecommendationScorer.ScoreCloudCover(cloudPercent);
        Assert.Equal(expectedScore, result);
    }

    #endregion

    #region Moon Illumination Scoring Tests

    [Theory]
    [InlineData(0, true, 100)]
    [InlineData(5, true, 100)]
    [InlineData(10, true, 95)]
    [InlineData(25, true, 75)]
    [InlineData(50, true, 50)]
    [InlineData(75, true, 25)]
    [InlineData(100, true, 10)]
    public void ScoreMoonIllumination_WhenMoonUp_ReturnsExpectedScore(
        int illumination, bool isMoonUp, double expectedScore)
    {
        var result = RecommendationScorer.ScoreMoonIllumination(illumination, isMoonUp);
        Assert.Equal(expectedScore, result);
    }

    [Theory]
    [InlineData(0, false, 100)]
    [InlineData(50, false, 100)]
    [InlineData(100, false, 100)]
    public void ScoreMoonIllumination_WhenMoonDown_AlwaysReturns100(
        int illumination, bool isMoonUp, double expectedScore)
    {
        var result = RecommendationScorer.ScoreMoonIllumination(illumination, isMoonUp);
        Assert.Equal(expectedScore, result);
    }

    #endregion

    #region Humidity Scoring Tests

    [Theory]
    [InlineData(30, 100)]
    [InlineData(45, 90)]
    [InlineData(55, 75)]
    [InlineData(65, 60)]
    [InlineData(72, 50)]
    [InlineData(78, 35)]
    [InlineData(82, 20)]
    [InlineData(88, 10)]
    [InlineData(95, 0)]
    public void ScoreHumidity_ReturnsExpectedScore(int humidity, double expectedScore)
    {
        var result = RecommendationScorer.ScoreHumidity(humidity);
        Assert.Equal(expectedScore, result);
    }

    #endregion

    #region Visibility Scoring Tests

    [Theory]
    [InlineData(20, 100)]
    [InlineData(13, 90)]
    [InlineData(11, 80)]
    [InlineData(9, 70)]
    [InlineData(7, 55)]
    [InlineData(5.5, 45)]
    [InlineData(4.5, 30)]
    [InlineData(3.5, 15)]
    [InlineData(2, 0)]
    public void ScoreVisibility_ReturnsExpectedScore(double visibility, double expectedScore)
    {
        var result = RecommendationScorer.ScoreVisibility(visibility);
        Assert.Equal(expectedScore, result);
    }

    #endregion

    #region Light Pollution Scoring Tests

    [Theory]
    [InlineData(1, 100)]
    [InlineData(2, 95)]
    [InlineData(3, 85)]
    [InlineData(4, 70)]
    [InlineData(5, 50)]
    [InlineData(6, 30)]
    [InlineData(7, 15)]
    [InlineData(8, 5)]
    [InlineData(9, 0)]
    public void ScoreLightPollution_ReturnsExpectedScore(int bortle, double expectedScore)
    {
        var result = RecommendationScorer.ScoreLightPollution(bortle);
        Assert.Equal(expectedScore, result);
    }

    #endregion

    #region Rating Tests

    [Theory]
    [InlineData(90, Rating.Excellent)]
    [InlineData(85, Rating.Excellent)]
    [InlineData(80, Rating.Good)]
    [InlineData(70, Rating.Good)]
    [InlineData(60, Rating.Fair)]
    [InlineData(50, Rating.Fair)]
    [InlineData(40, Rating.Poor)]
    [InlineData(20, Rating.Poor)]
    public void GetRating_ReturnsCorrectRating(double score, Rating expected)
    {
        var result = RecommendationScorer.GetRating(score);
        Assert.Equal(expected, result);
    }

    #endregion

    #region Full Recommendation Tests

    [Fact]
    public void CalculateRecommendation_WithExcellentConditions_ReturnsHighScore()
    {
        // Arrange
        var conditions = CreateConditions(cloudCover: 5, humidity: 40, visibilityKm: 15);
        var astronomy = CreateAstronomy(moonIllumination: 5, isMoonUp: false);
        var bortleScale = 2;

        // Act
        var result = _scorer.CalculateRecommendation(conditions, astronomy, bortleScale);

        // Assert
        Assert.True(result.OverallScore >= 85);
        Assert.Equal(Rating.Excellent, result.Rating);
    }

    [Fact]
    public void CalculateRecommendation_WithPoorConditions_ReturnsLowScore()
    {
        // Arrange
        var conditions = CreateConditions(cloudCover: 90, humidity: 95, visibilityKm: 2);
        var astronomy = CreateAstronomy(moonIllumination: 100, isMoonUp: true);
        var bortleScale = 8;

        // Act
        var result = _scorer.CalculateRecommendation(conditions, astronomy, bortleScale);

        // Assert
        Assert.True(result.OverallScore < 50);
        Assert.Equal(Rating.Poor, result.Rating);
    }

    [Fact]
    public void CalculateRecommendation_WithNoBortleScale_Uses50AsDefault()
    {
        // Arrange
        var conditions = CreateConditions(cloudCover: 20, humidity: 50, visibilityKm: 10);
        var astronomy = CreateAstronomy(moonIllumination: 30, isMoonUp: true);

        // Act
        var result = _scorer.CalculateRecommendation(conditions, astronomy, bortleScale: null);

        // Assert
        Assert.DoesNotContain(result.FactorScores, f => f.FactorName == "Light Pollution");
    }

    [Fact]
    public void CalculateRecommendation_ReturnsAllExpectedFactors()
    {
        // Arrange
        var conditions = CreateConditions(cloudCover: 30, humidity: 60, visibilityKm: 8);
        var astronomy = CreateAstronomy(moonIllumination: 50, isMoonUp: true);
        var bortleScale = 5;

        // Act
        var result = _scorer.CalculateRecommendation(conditions, astronomy, bortleScale);

        // Assert
        Assert.Equal(5, result.FactorScores.Count);
        Assert.Contains(result.FactorScores, f => f.FactorName == "Cloud Cover");
        Assert.Contains(result.FactorScores, f => f.FactorName == "Moon Illumination");
        Assert.Contains(result.FactorScores, f => f.FactorName == "Humidity");
        Assert.Contains(result.FactorScores, f => f.FactorName == "Visibility");
        Assert.Contains(result.FactorScores, f => f.FactorName == "Light Pollution");
    }

    [Fact]
    public void CalculateRecommendation_DsoScore_FavorsLowMoonAndDarkSkies()
    {
        // Arrange - conditions favorable for DSO (low moon, dark skies)
        var dsoConditions = CreateConditions(cloudCover: 10, humidity: 50, visibilityKm: 10);
        var dsoAstronomy = CreateAstronomy(moonIllumination: 5, isMoonUp: false);

        // Arrange - conditions better for planetary (doesn't care about moon)
        var planetaryConditions = CreateConditions(cloudCover: 10, humidity: 50, visibilityKm: 15);
        var planetaryAstronomy = CreateAstronomy(moonIllumination: 100, isMoonUp: true);

        // Act
        var dsoResult = _scorer.CalculateRecommendation(dsoConditions, dsoAstronomy, bortleScale: 2);
        var planetaryResult = _scorer.CalculateRecommendation(planetaryConditions, planetaryAstronomy, bortleScale: 2);

        // Assert - DSO score should be higher when moon is not affecting
        Assert.True(dsoResult.DsoScore > planetaryResult.DsoScore);
    }

    #endregion

    #region Helper Methods

    private static HourlyCondition CreateConditions(
        int cloudCover = 20,
        int humidity = 50,
        double visibilityKm = 10,
        double windSpeedKph = 10)
    {
        return new HourlyCondition
        {
            Time = DateTime.Now,
            TimeEpoch = DateTimeOffset.Now.ToUnixTimeSeconds(),
            TemperatureCelsius = 15,
            TemperatureFahrenheit = 59,
            FeelsLikeCelsius = 15,
            FeelsLikeFahrenheit = 59,
            CloudCoverPercent = cloudCover,
            HumidityPercent = humidity,
            VisibilityKm = visibilityKm,
            VisibilityMiles = visibilityKm * 0.621371,
            WindSpeedKph = windSpeedKph,
            WindSpeedMph = windSpeedKph * 0.621371,
            WindDegree = 0,
            WindDirection = "N",
            GustMph = windSpeedKph * 0.621371 * 1.3,
            GustKph = windSpeedKph * 1.3,
            PressureMb = 1013,
            PressureIn = 29.92,
            PrecipMm = 0,
            PrecipIn = 0,
            ChanceOfRainPercent = 0,
            ChanceOfSnowPercent = 0,
            DewPointCelsius = 8,
            DewPointFahrenheit = 46,
            UvIndex = 0,
            Condition = new WeatherCondition
            {
                Text = "Clear",
                IconUrl = "//cdn.weatherapi.com/weather/64x64/night/113.png",
                Code = 1000
            },
            IsDay = false
        };
    }

    private static AstronomyData CreateAstronomy(
        int moonIllumination = 50,
        bool isMoonUp = true,
        string moonPhase = "First Quarter")
    {
        return new AstronomyData
        {
            Sunrise = new TimeOnly(6, 30),
            Sunset = new TimeOnly(18, 30),
            Moonrise = new TimeOnly(20, 0),
            Moonset = new TimeOnly(6, 0),
            MoonPhase = moonPhase,
            MoonIlluminationPercent = moonIllumination,
            IsMoonUp = isMoonUp,
            IsSunUp = false
        };
    }

    #endregion
}
