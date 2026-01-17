using StargazeWeatherConditions.Utilities;

namespace StargazeWeatherConditions.Tests.Utilities;

public class CoordinateValidatorTests
{
    #region Latitude Validation Tests

    [Theory]
    [InlineData(0)]
    [InlineData(45)]
    [InlineData(-45)]
    [InlineData(90)]
    [InlineData(-90)]
    [InlineData(39.7392)]
    public void IsValidLatitude_WithValidValues_ReturnsTrue(double latitude)
    {
        Assert.True(CoordinateValidator.IsValidLatitude(latitude));
    }

    [Theory]
    [InlineData(90.1)]
    [InlineData(-90.1)]
    [InlineData(180)]
    [InlineData(-180)]
    [InlineData(1000)]
    public void IsValidLatitude_WithInvalidValues_ReturnsFalse(double latitude)
    {
        Assert.False(CoordinateValidator.IsValidLatitude(latitude));
    }

    #endregion

    #region Longitude Validation Tests

    [Theory]
    [InlineData(0)]
    [InlineData(90)]
    [InlineData(-90)]
    [InlineData(180)]
    [InlineData(-180)]
    [InlineData(-104.9903)]
    public void IsValidLongitude_WithValidValues_ReturnsTrue(double longitude)
    {
        Assert.True(CoordinateValidator.IsValidLongitude(longitude));
    }

    [Theory]
    [InlineData(180.1)]
    [InlineData(-180.1)]
    [InlineData(360)]
    [InlineData(-360)]
    public void IsValidLongitude_WithInvalidValues_ReturnsFalse(double longitude)
    {
        Assert.False(CoordinateValidator.IsValidLongitude(longitude));
    }

    #endregion

    #region Coordinate Parsing Tests

    [Theory]
    [InlineData("39.7392,-104.9903")]
    [InlineData("39.7392, -104.9903")]
    [InlineData("39.7392  -104.9903")]
    public void TryParseCoordinates_WithValidFormats_ReturnsTrue(string input)
    {
        var result = CoordinateValidator.TryParseCoordinates(input, out var lat, out var lon);
        
        Assert.True(result);
        Assert.Equal(39.7392, lat, precision: 4);
        Assert.Equal(-104.9903, lon, precision: 4);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("Denver")]
    [InlineData("abc,def")]
    [InlineData("200,-104")] // Invalid latitude
    [InlineData("39,-200")] // Invalid longitude
    public void TryParseCoordinates_WithInvalidFormats_ReturnsFalse(string input)
    {
        var result = CoordinateValidator.TryParseCoordinates(input, out _, out _);
        Assert.False(result);
    }

    [Theory]
    [InlineData(null)]
    public void TryParseCoordinates_WithNull_ReturnsFalse(string? input)
    {
        var result = CoordinateValidator.TryParseCoordinates(input!, out _, out _);
        Assert.False(result);
    }

    #endregion

    #region Looks Like Tests

    [Theory]
    [InlineData("39.7392,-104.9903", true)]
    [InlineData("39.7392, -104.9903", true)]
    [InlineData("Denver", false)]
    [InlineData("80202", false)]
    public void LooksLikeCoordinates_ReturnsExpected(string input, bool expected)
    {
        var result = CoordinateValidator.LooksLikeCoordinates(input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("80202", true)]
    [InlineData("80202-1234", true)]
    [InlineData("SW1A 1AA", true)]
    [InlineData("K1A 0B1", true)]
    [InlineData("Denver", false)]
    [InlineData("39.7392,-104.9903", false)]
    public void LooksLikePostalCode_ReturnsExpected(string input, bool expected)
    {
        var result = CoordinateValidator.LooksLikePostalCode(input);
        Assert.Equal(expected, result);
    }

    #endregion

    #region Distance Calculation Tests

    [Fact]
    public void CalculateDistance_SamePoint_ReturnsZero()
    {
        var distance = CoordinateValidator.CalculateDistance(39.7392, -104.9903, 39.7392, -104.9903);
        Assert.Equal(0, distance, precision: 5);
    }

    [Fact]
    public void CalculateDistance_DenverToNewYork_ReturnsApproximatelyCorrect()
    {
        // Denver to NYC is approximately 2615 km
        var distance = CoordinateValidator.CalculateDistance(
            39.7392, -104.9903,  // Denver
            40.7128, -74.0060);  // New York
        
        Assert.InRange(distance, 2600, 2650);
    }

    [Fact]
    public void CalculateDistance_LondonToTokyo_ReturnsApproximatelyCorrect()
    {
        // London to Tokyo is approximately 9560 km
        var distance = CoordinateValidator.CalculateDistance(
            51.5074, -0.1278,    // London
            35.6762, 139.6503);  // Tokyo
        
        Assert.InRange(distance, 9500, 9600);
    }

    #endregion

    #region Format Coordinates Tests

    [Fact]
    public void FormatCoordinates_NorthEast_FormatsCorrectly()
    {
        var result = CoordinateValidator.FormatCoordinates(39.7392, -104.9903, 4);
        Assert.Equal("39.7392°N, 104.9903°W", result);
    }

    [Fact]
    public void FormatCoordinates_SouthWest_FormatsCorrectly()
    {
        var result = CoordinateValidator.FormatCoordinates(-33.8688, 151.2093, 4);
        Assert.Equal("33.8688°S, 151.2093°E", result);
    }

    #endregion
}
