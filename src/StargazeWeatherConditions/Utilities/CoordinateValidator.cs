using System.Text.RegularExpressions;

namespace StargazeWeatherConditions.Utilities;

/// <summary>
/// Utility class for validating coordinates and location inputs.
/// </summary>
public static partial class CoordinateValidator
{
    /// <summary>
    /// Validates latitude value.
    /// </summary>
    /// <param name="latitude">Latitude in decimal degrees.</param>
    /// <returns>True if valid (-90 to 90).</returns>
    public static bool IsValidLatitude(double latitude)
        => latitude >= -90.0 && latitude <= 90.0;

    /// <summary>
    /// Validates longitude value.
    /// </summary>
    /// <param name="longitude">Longitude in decimal degrees.</param>
    /// <returns>True if valid (-180 to 180).</returns>
    public static bool IsValidLongitude(double longitude)
        => longitude >= -180.0 && longitude <= 180.0;

    /// <summary>
    /// Validates both latitude and longitude.
    /// </summary>
    public static bool AreValidCoordinates(double latitude, double longitude)
        => IsValidLatitude(latitude) && IsValidLongitude(longitude);

    /// <summary>
    /// Tries to parse a coordinate string in various formats.
    /// Supports: "lat,lon", "lat, lon", "lat lon"
    /// </summary>
    public static bool TryParseCoordinates(string input, out double latitude, out double longitude)
    {
        latitude = 0;
        longitude = 0;

        if (string.IsNullOrWhiteSpace(input))
            return false;

        // Try to match coordinate patterns
        var match = CoordinatePattern().Match(input.Trim());
        
        if (match.Success && 
            double.TryParse(match.Groups["lat"].Value, out latitude) &&
            double.TryParse(match.Groups["lon"].Value, out longitude))
        {
            return AreValidCoordinates(latitude, longitude);
        }

        return false;
    }

    /// <summary>
    /// Determines if the input looks like coordinates.
    /// </summary>
    public static bool LooksLikeCoordinates(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return false;

        return CoordinatePattern().IsMatch(input.Trim());
    }

    /// <summary>
    /// Determines if the input looks like a postal/ZIP code.
    /// </summary>
    public static bool LooksLikePostalCode(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return false;

        var trimmed = input.Trim();
        
        // US ZIP code (5 digits or 5+4)
        if (UsZipCodePattern().IsMatch(trimmed))
            return true;

        // UK postcode
        if (UkPostcodePattern().IsMatch(trimmed))
            return true;

        // Canadian postal code
        if (CanadianPostalCodePattern().IsMatch(trimmed))
            return true;

        return false;
    }

    /// <summary>
    /// Formats coordinates for display.
    /// </summary>
    public static string FormatCoordinates(double latitude, double longitude, int decimals = 4)
    {
        var latDir = latitude >= 0 ? "N" : "S";
        var lonDir = longitude >= 0 ? "E" : "W";
        
        return $"{Math.Abs(latitude).ToString($"F{decimals}")}°{latDir}, " +
               $"{Math.Abs(longitude).ToString($"F{decimals}")}°{lonDir}";
    }

    /// <summary>
    /// Calculates the distance between two points using the Haversine formula.
    /// </summary>
    /// <returns>Distance in kilometers.</returns>
    public static double CalculateDistance(
        double lat1, double lon1, 
        double lat2, double lon2)
    {
        const double earthRadiusKm = 6371.0;

        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return earthRadiusKm * c;
    }

    private static double ToRadians(double degrees) => degrees * Math.PI / 180.0;

    // Regex patterns using source generators for performance
    [GeneratedRegex(@"^(?<lat>-?\d+\.?\d*)[,\s]+(?<lon>-?\d+\.?\d*)$")]
    private static partial Regex CoordinatePattern();

    [GeneratedRegex(@"^\d{5}(-\d{4})?$")]
    private static partial Regex UsZipCodePattern();

    [GeneratedRegex(@"^[A-Z]{1,2}\d[A-Z\d]?\s*\d[A-Z]{2}$", RegexOptions.IgnoreCase)]
    private static partial Regex UkPostcodePattern();

    [GeneratedRegex(@"^[A-Z]\d[A-Z]\s*\d[A-Z]\d$", RegexOptions.IgnoreCase)]
    private static partial Regex CanadianPostalCodePattern();
}
