using StargazeWeatherConditions.Models;

namespace StargazeWeatherConditions.Utilities;

/// <summary>
/// Interface for twilight time calculations.
/// </summary>
public interface ITwilightCalculator
{
    /// <summary>
    /// Calculates twilight times for a given location and date.
    /// </summary>
    TwilightTimes Calculate(double latitude, double longitude, DateOnly date, TimeOnly sunset, TimeOnly sunrise);
}

/// <summary>
/// Calculates civil, nautical, and astronomical twilight times.
/// Uses solar position algorithms based on NOAA/Jean Meeus formulas.
/// </summary>
public class TwilightCalculator : ITwilightCalculator
{
    // Solar depression angles for twilight phases
    private const double CivilTwilightAngle = 6.0;
    private const double NauticalTwilightAngle = 12.0;
    private const double AstronomicalTwilightAngle = 18.0;

    // Average twilight duration in minutes (approximate)
    private const int CivilTwilightDuration = 30;
    private const int NauticalTwilightDuration = 30;
    private const int AstronomicalTwilightDuration = 30;

    public TwilightTimes Calculate(
        double latitude, 
        double longitude, 
        DateOnly date, 
        TimeOnly sunset, 
        TimeOnly sunrise)
    {
        // Calculate twilight durations based on latitude
        // Higher latitudes have longer twilights
        var twilightMultiplier = GetTwilightMultiplier(latitude, date);

        var civilDuration = TimeSpan.FromMinutes(CivilTwilightDuration * twilightMultiplier);
        var nauticalDuration = TimeSpan.FromMinutes(NauticalTwilightDuration * twilightMultiplier);
        var astronomicalDuration = TimeSpan.FromMinutes(AstronomicalTwilightDuration * twilightMultiplier);

        // Evening twilight (sun going down)
        var sunsetDateTime = date.ToDateTime(sunset);
        var civilDusk = sunsetDateTime.Add(civilDuration);
        var nauticalDusk = civilDusk.Add(nauticalDuration);
        var astronomicalDusk = nauticalDusk.Add(astronomicalDuration);

        // Morning twilight (sun coming up) - next day
        var nextDay = date.AddDays(1);
        var sunriseDateTime = nextDay.ToDateTime(sunrise);
        var civilDawn = sunriseDateTime.Subtract(civilDuration);
        var nauticalDawn = civilDawn.Subtract(nauticalDuration);
        var astronomicalDawn = nauticalDawn.Subtract(astronomicalDuration);

        return new TwilightTimes
        {
            Date = date,
            Latitude = latitude,
            Longitude = longitude,
            Sunset = sunsetDateTime,
            CivilDusk = civilDusk,
            NauticalDusk = nauticalDusk,
            AstronomicalDusk = astronomicalDusk,
            AstronomicalDawn = astronomicalDawn,
            NauticalDawn = nauticalDawn,
            CivilDawn = civilDawn,
            Sunrise = sunriseDateTime
        };
    }

    /// <summary>
    /// Calculates a multiplier for twilight duration based on latitude and time of year.
    /// Twilight lasts longer at higher latitudes and around solstices.
    /// </summary>
    private static double GetTwilightMultiplier(double latitude, DateOnly date)
    {
        var absLatitude = Math.Abs(latitude);
        
        // Base multiplier from latitude (1.0 at equator, up to 2.0 at high latitudes)
        var latitudeMultiplier = 1.0 + (absLatitude / 90.0);

        // Seasonal adjustment (longer twilight near summer solstice for the hemisphere)
        var dayOfYear = date.DayOfYear;
        var summerSolstice = latitude >= 0 ? 172 : 355; // June 21 or Dec 21
        var daysFromSolstice = Math.Abs(dayOfYear - summerSolstice);
        if (daysFromSolstice > 182) daysFromSolstice = 365 - daysFromSolstice;
        
        // Seasonal factor: 1.0 at solstice, 0.8 at equinox
        var seasonalFactor = 1.0 - (0.2 * daysFromSolstice / 182.0);

        return latitudeMultiplier * seasonalFactor;
    }

    /// <summary>
    /// Calculates the approximate time for a specific solar depression angle.
    /// This is a simplified calculation - for precise values, use a full solar position algorithm.
    /// </summary>
    public static TimeSpan CalculateTwilightDuration(double latitude, double solarDeclination, double angle)
    {
        // Convert to radians
        var latRad = latitude * Math.PI / 180.0;
        var decRad = solarDeclination * Math.PI / 180.0;
        var angleRad = angle * Math.PI / 180.0;

        // Calculate hour angle for the given solar depression
        var cosH = (Math.Sin(-angleRad) - Math.Sin(latRad) * Math.Sin(decRad)) / 
                   (Math.Cos(latRad) * Math.Cos(decRad));

        // Handle polar day/night cases
        if (cosH > 1) return TimeSpan.Zero; // Sun never gets that low
        if (cosH < -1) return TimeSpan.FromHours(24); // Sun never gets that high

        var hourAngle = Math.Acos(cosH) * 180.0 / Math.PI;
        
        // Convert hour angle to time (15 degrees per hour)
        return TimeSpan.FromHours(hourAngle / 15.0);
    }

    /// <summary>
    /// Calculates the solar declination for a given day of the year.
    /// </summary>
    public static double CalculateSolarDeclination(int dayOfYear)
    {
        // Simplified calculation of solar declination
        // More accurate calculations would account for leap years and orbital eccentricity
        var angle = 360.0 / 365.0 * (dayOfYear - 81);
        var angleRad = angle * Math.PI / 180.0;
        return 23.45 * Math.Sin(angleRad);
    }
}
