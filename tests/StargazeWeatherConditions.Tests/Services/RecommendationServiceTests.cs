using Moq;
using StargazeWeatherConditions.Models;
using StargazeWeatherConditions.Services;
using StargazeWeatherConditions.Utilities;

namespace StargazeWeatherConditions.Tests.Services;

public class RecommendationServiceTests
{
    private readonly Mock<IRecommendationScorer> _mockScorer;
    private readonly RecommendationService _service;

    public RecommendationServiceTests()
    {
        _mockScorer = new Mock<IRecommendationScorer>();
        _service = new RecommendationService(_mockScorer.Object);
    }

    [Fact]
    public void CalculateRecommendation_CallsScorer()
    {
        // Arrange
        var conditions = CreateSampleConditions();
        var astronomy = CreateSampleAstronomy();
        var lightPollution = new LightPollutionData
        {
            Latitude = 40,
            Longitude = -100,
            BortleClass = 5,
            ArtificialBrightness = 1.0,
            RetrievedAt = DateTime.UtcNow
        };

        var expectedRecommendation = new Recommendation
        {
            OverallScore = 75,
            Rating = Rating.Good,
            Summary = "Good conditions",
            FactorScores = new List<FactorScore>(),
            DsoScore = 70,
            PlanetaryScore = 80,
            CalculatedFor = conditions.Time
        };

        _mockScorer.Setup(s => s.CalculateRecommendation(conditions, astronomy, 5))
            .Returns(expectedRecommendation);

        // Act
        var result = _service.CalculateRecommendation(conditions, astronomy, lightPollution);

        // Assert
        Assert.Same(expectedRecommendation, result);
        _mockScorer.Verify(s => s.CalculateRecommendation(conditions, astronomy, 5), Times.Once);
    }

    [Fact]
    public void CalculateRecommendation_WithNullBortle_PassesNullToScorer()
    {
        // Arrange
        var conditions = CreateSampleConditions();
        var astronomy = CreateSampleAstronomy();

        _mockScorer.Setup(s => s.CalculateRecommendation(conditions, astronomy, null))
            .Returns(CreateSampleRecommendation());

        // Act
        _service.CalculateRecommendation(conditions, astronomy, null);

        // Assert
        _mockScorer.Verify(s => s.CalculateRecommendation(conditions, astronomy, null), Times.Once);
    }

    [Fact]
    public void CalculateRecommendations_ForMultipleHours_ReturnsAllResults()
    {
        // Arrange
        var conditions = new List<HourlyCondition>
        {
            CreateSampleConditions(hour: 20),
            CreateSampleConditions(hour: 21),
            CreateSampleConditions(hour: 22)
        };
        var astronomy = CreateSampleAstronomy();

        _mockScorer.Setup(s => s.CalculateRecommendation(It.IsAny<HourlyCondition>(), astronomy, null))
            .Returns(CreateSampleRecommendation());

        // Act
        var results = conditions.Select(c => _service.CalculateRecommendation(c, astronomy, null)).ToList();

        // Assert
        Assert.Equal(3, results.Count);
        _mockScorer.Verify(s => s.CalculateRecommendation(It.IsAny<HourlyCondition>(), astronomy, null), 
            Times.Exactly(3));
    }

    private static HourlyCondition CreateSampleConditions(int hour = 22)
    {
        return new HourlyCondition
        {
            Time = new DateTime(2024, 6, 21, hour, 0, 0),
            TimeEpoch = new DateTimeOffset(2024, 6, 21, hour, 0, 0, TimeSpan.Zero).ToUnixTimeSeconds(),
            TemperatureCelsius = 20,
            TemperatureFahrenheit = 68,
            FeelsLikeCelsius = 20,
            FeelsLikeFahrenheit = 68,
            CloudCoverPercent = 20,
            HumidityPercent = 50,
            VisibilityKm = 10,
            VisibilityMiles = 6.2,
            WindSpeedKph = 10,
            WindSpeedMph = 6.2,
            WindDegree = 0,
            WindDirection = "N",
            GustMph = 8,
            GustKph = 13,
            PressureMb = 1013,
            PressureIn = 29.92,
            PrecipMm = 0,
            PrecipIn = 0,
            ChanceOfRainPercent = 0,
            ChanceOfSnowPercent = 0,
            DewPointCelsius = 10,
            DewPointFahrenheit = 50,
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

    private static AstronomyData CreateSampleAstronomy()
    {
        return new AstronomyData
        {
            Sunrise = new TimeOnly(6, 0),
            Sunset = new TimeOnly(20, 0),
            Moonrise = new TimeOnly(22, 0),
            Moonset = new TimeOnly(8, 0),
            MoonPhase = "First Quarter",
            MoonIlluminationPercent = 50,
            IsMoonUp = true,
            IsSunUp = false
        };
    }

    private static Recommendation CreateSampleRecommendation()
    {
        return new Recommendation
        {
            OverallScore = 75,
            Rating = Rating.Good,
            Summary = "Good conditions",
            FactorScores = new List<FactorScore>(),
            DsoScore = 70,
            PlanetaryScore = 80,
            CalculatedFor = DateTime.Now
        };
    }
}
