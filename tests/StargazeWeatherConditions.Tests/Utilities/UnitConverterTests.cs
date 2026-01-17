using StargazeWeatherConditions.Utilities;

namespace StargazeWeatherConditions.Tests.Utilities;

public class UnitConverterTests
{
    #region Temperature Conversion Tests

    [Theory]
    [InlineData(0, 32)]
    [InlineData(100, 212)]
    [InlineData(-40, -40)]
    [InlineData(20, 68)]
    [InlineData(37, 98.6)]
    public void CelsiusToFahrenheit_ReturnsCorrectValue(double celsius, double expectedFahrenheit)
    {
        var result = UnitConverter.CelsiusToFahrenheit(celsius);
        Assert.Equal(expectedFahrenheit, result, precision: 1);
    }

    [Theory]
    [InlineData(32, 0)]
    [InlineData(212, 100)]
    [InlineData(-40, -40)]
    [InlineData(68, 20)]
    public void FahrenheitToCelsius_ReturnsCorrectValue(double fahrenheit, double expectedCelsius)
    {
        var result = UnitConverter.FahrenheitToCelsius(fahrenheit);
        Assert.Equal(expectedCelsius, result, precision: 1);
    }

    [Fact]
    public void FormatTemperature_WithMetric_ReturnsCelsius()
    {
        var result = UnitConverter.FormatTemperature(25, useMetric: true);
        Assert.Equal("25°C", result);
    }

    [Fact]
    public void FormatTemperature_WithImperial_ReturnsFahrenheit()
    {
        var result = UnitConverter.FormatTemperature(25, useMetric: false);
        Assert.Equal("77°F", result);
    }

    #endregion

    #region Distance Conversion Tests

    [Theory]
    [InlineData(1, 0.621371)]
    [InlineData(10, 6.21371)]
    [InlineData(100, 62.1371)]
    public void KilometersToMiles_ReturnsCorrectValue(double km, double expectedMiles)
    {
        var result = UnitConverter.KilometersToMiles(km);
        Assert.Equal(expectedMiles, result, precision: 4);
    }

    [Theory]
    [InlineData(1, 1.60934)]
    [InlineData(10, 16.0934)]
    [InlineData(62.1371, 100)]
    public void MilesToKilometers_ReturnsCorrectValue(double miles, double expectedKm)
    {
        var result = UnitConverter.MilesToKilometers(miles);
        Assert.Equal(expectedKm, result, precision: 2);
    }

    [Fact]
    public void FormatDistance_WithMetric_ReturnsKilometers()
    {
        var result = UnitConverter.FormatDistance(10.5, useMetric: true);
        Assert.Equal("10.5 km", result);
    }

    [Fact]
    public void FormatDistance_WithImperial_ReturnsMiles()
    {
        var result = UnitConverter.FormatDistance(10, useMetric: false);
        Assert.Equal("6.2 mi", result);
    }

    #endregion

    #region Speed Conversion Tests

    [Theory]
    [InlineData(100, 62.1371)]
    [InlineData(50, 31.06855)]
    public void KphToMph_ReturnsCorrectValue(double kph, double expectedMph)
    {
        var result = UnitConverter.KphToMph(kph);
        Assert.Equal(expectedMph, result, precision: 3);
    }

    [Fact]
    public void FormatSpeed_WithMetric_ReturnsKph()
    {
        var result = UnitConverter.FormatSpeed(50, useMetric: true);
        Assert.Equal("50 kph", result);
    }

    [Fact]
    public void FormatSpeed_WithImperial_ReturnsMph()
    {
        var result = UnitConverter.FormatSpeed(50, useMetric: false);
        Assert.Equal("31 mph", result);
    }

    #endregion

    #region Time Formatting Tests

    [Fact]
    public void FormatTime_DateTime_With24Hour_ReturnsCorrectFormat()
    {
        var time = new DateTime(2024, 1, 1, 14, 30, 0);
        var result = UnitConverter.FormatTime(time, use24Hour: true);
        Assert.Equal("14:30", result);
    }

    [Fact]
    public void FormatTime_DateTime_With12Hour_ReturnsCorrectFormat()
    {
        var time = new DateTime(2024, 1, 1, 14, 30, 0);
        var result = UnitConverter.FormatTime(time, use24Hour: false);
        Assert.Equal("2:30 PM", result);
    }

    [Fact]
    public void FormatTime_TimeOnly_With24Hour_ReturnsCorrectFormat()
    {
        var time = new TimeOnly(22, 15);
        var result = UnitConverter.FormatTime(time, use24Hour: true);
        Assert.Equal("22:15", result);
    }

    [Fact]
    public void FormatTime_TimeOnly_With12Hour_ReturnsCorrectFormat()
    {
        var time = new TimeOnly(22, 15);
        var result = UnitConverter.FormatTime(time, use24Hour: false);
        Assert.Equal("10:15 PM", result);
    }

    [Theory]
    [InlineData(90, "1h 30m")]
    [InlineData(45, "45m")]
    [InlineData(120, "2h 0m")]
    [InlineData(185, "3h 5m")]
    public void FormatDuration_ReturnsCorrectFormat(int totalMinutes, string expected)
    {
        var duration = TimeSpan.FromMinutes(totalMinutes);
        var result = UnitConverter.FormatDuration(duration);
        Assert.Equal(expected, result);
    }

    #endregion

    #region Pressure Conversion Tests

    [Theory]
    [InlineData(1013.25, 29.92)]
    [InlineData(1000, 29.53)]
    public void MillibarsToInHg_ReturnsCorrectValue(double mb, double expectedInHg)
    {
        var result = UnitConverter.MillibarsToInHg(mb);
        Assert.Equal(expectedInHg, result, precision: 2);
    }

    [Fact]
    public void FormatPressure_WithMetric_ReturnsMillibars()
    {
        var result = UnitConverter.FormatPressure(1013, useMetric: true);
        Assert.Equal("1013 mb", result);
    }

    [Fact]
    public void FormatPressure_WithImperial_ReturnsInchesHg()
    {
        var result = UnitConverter.FormatPressure(1013, useMetric: false);
        Assert.Contains("inHg", result);
    }

    #endregion
}
