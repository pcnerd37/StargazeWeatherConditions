using StargazeWeatherConditions.Models;
using StargazeWeatherConditions.Utilities;

namespace StargazeWeatherConditions.Tests.Utilities;

public class TwilightCalculatorTests
{
    private readonly TwilightCalculator _calculator = new();

    [Fact]
    public void Calculate_ReturnsAllTwilightTimes()
    {
        // Arrange
        var date = new DateOnly(2024, 6, 21); // Summer solstice
        var sunset = new TimeOnly(20, 30);
        var sunrise = new TimeOnly(5, 30);
        var latitude = 39.7392;  // Denver
        var longitude = -104.9903;

        // Act
        var result = _calculator.Calculate(latitude, longitude, date, sunset, sunrise);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(date, result.Date);
        Assert.Equal(latitude, result.Latitude);
        Assert.Equal(longitude, result.Longitude);
    }

    [Fact]
    public void Calculate_TwilightPhases_AreInCorrectOrder()
    {
        // Arrange
        var date = new DateOnly(2024, 3, 20); // Spring equinox
        var sunset = new TimeOnly(19, 00);
        var sunrise = new TimeOnly(7, 00);

        // Act
        var result = _calculator.Calculate(40, -100, date, sunset, sunrise);

        // Assert - Evening twilight should progress from sunset to night
        Assert.True(result.CivilDusk > result.Sunset);
        Assert.True(result.NauticalDusk > result.CivilDusk);
        Assert.True(result.AstronomicalDusk > result.NauticalDusk);

        // Assert - Morning twilight should progress from night to sunrise
        Assert.True(result.AstronomicalDawn < result.NauticalDawn);
        Assert.True(result.NauticalDawn < result.CivilDawn);
        Assert.True(result.CivilDawn < result.Sunrise);
    }

    [Fact]
    public void Calculate_OptimalViewingDuration_IsPositive()
    {
        // Arrange
        var date = new DateOnly(2024, 1, 15);
        var sunset = new TimeOnly(17, 00);
        var sunrise = new TimeOnly(7, 00);

        // Act
        var result = _calculator.Calculate(40, -100, date, sunset, sunrise);

        // Assert
        Assert.True(result.OptimalViewingDuration > TimeSpan.Zero);
    }

    [Fact]
    public void Calculate_NightDuration_ExcludesEveningAndMorningTwilight()
    {
        // Arrange
        var date = new DateOnly(2024, 6, 21);
        var sunset = new TimeOnly(20, 30);
        var sunrise = new TimeOnly(5, 30);

        // Act
        var result = _calculator.Calculate(39.7392, -104.9903, date, sunset, sunrise);

        // Assert - Optimal viewing (true night) should be shorter than total darkness
        var totalDarkness = result.Sunrise - result.Sunset;
        Assert.True(result.OptimalViewingDuration < totalDarkness);
    }

    [Theory]
    [InlineData(0)]    // Equator
    [InlineData(45)]   // Mid-latitude
    [InlineData(60)]   // High latitude
    public void Calculate_DifferentLatitudes_ProducesValidResults(double latitude)
    {
        // Arrange
        var date = new DateOnly(2024, 3, 20);
        var sunset = new TimeOnly(18, 30);
        var sunrise = new TimeOnly(6, 30);

        // Act
        var result = _calculator.Calculate(latitude, 0, date, sunset, sunrise);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.AstronomicalDusk > result.Sunset);
        Assert.True(result.AstronomicalDawn < result.Sunrise);
    }

    [Fact]
    public void Calculate_HighLatitudeSummer_HasLongerTwilight()
    {
        // Arrange
        var date = new DateOnly(2024, 6, 21);
        var sunset = new TimeOnly(21, 0);
        var sunrise = new TimeOnly(4, 0);

        var lowLatitude = 30.0;
        var highLatitude = 60.0;

        // Act
        var lowLatResult = _calculator.Calculate(lowLatitude, 0, date, sunset, sunrise);
        var highLatResult = _calculator.Calculate(highLatitude, 0, date, sunset, sunrise);

        // Assert - Higher latitude should have longer civil twilight duration
        var lowLatCivilDuration = lowLatResult.CivilDusk - lowLatResult.Sunset;
        var highLatCivilDuration = highLatResult.CivilDusk - highLatResult.Sunset;

        Assert.True(highLatCivilDuration > lowLatCivilDuration);
    }

    [Fact]
    public void CalculateSolarDeclination_SummerSolstice_ReturnsPositiveMax()
    {
        // Day 172 is approximately June 21 (summer solstice)
        var declination = TwilightCalculator.CalculateSolarDeclination(172);
        
        // Should be approximately +23.45 degrees
        Assert.InRange(declination, 23.0, 24.0);
    }

    [Fact]
    public void CalculateSolarDeclination_WinterSolstice_ReturnsNegativeMax()
    {
        // Day 355 is approximately December 21 (winter solstice)
        var declination = TwilightCalculator.CalculateSolarDeclination(355);
        
        // Should be approximately -23.45 degrees
        Assert.InRange(declination, -24.0, -23.0);
    }

    [Fact]
    public void CalculateSolarDeclination_Equinox_ReturnsNearZero()
    {
        // Day 80 is approximately March 21 (spring equinox)
        var declination = TwilightCalculator.CalculateSolarDeclination(80);
        
        // Should be near 0 degrees
        Assert.InRange(declination, -3.0, 3.0);
    }

    [Fact]
    public void GetTwilightPhase_BeforeSunset_ReturnsDay()
    {
        // Arrange
        var date = new DateOnly(2024, 6, 21);
        var sunset = new TimeOnly(20, 30);
        var sunrise = new TimeOnly(5, 30);
        var result = _calculator.Calculate(40, -100, date, sunset, sunrise);

        // Act
        var phase = result.GetTwilightPhase(date.ToDateTime(new TimeOnly(18, 0)));

        // Assert
        Assert.Equal(TwilightPhase.Day, phase);
    }

    [Fact]
    public void GetTwilightPhase_DuringCivilTwilight_ReturnsCivilEvening()
    {
        // Arrange
        var date = new DateOnly(2024, 6, 21);
        var sunset = new TimeOnly(20, 30);
        var sunrise = new TimeOnly(5, 30);
        var result = _calculator.Calculate(40, -100, date, sunset, sunrise);

        // Act - Check just after sunset
        var justAfterSunset = result.Sunset.AddMinutes(10);
        var phase = result.GetTwilightPhase(justAfterSunset);

        // Assert
        Assert.Equal(TwilightPhase.CivilEvening, phase);
    }

    [Fact]
    public void GetTwilightPhase_DuringTrueNight_ReturnsNight()
    {
        // Arrange
        var date = new DateOnly(2024, 6, 21);
        var sunset = new TimeOnly(20, 30);
        var sunrise = new TimeOnly(5, 30);
        var result = _calculator.Calculate(40, -100, date, sunset, sunrise);

        // Act - Check in the middle of the night
        var midnight = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0).AddDays(1);
        var phase = result.GetTwilightPhase(midnight);

        // Assert
        Assert.Equal(TwilightPhase.Night, phase);
    }
}
