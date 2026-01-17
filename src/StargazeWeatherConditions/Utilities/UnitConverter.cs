namespace StargazeWeatherConditions.Utilities;

/// <summary>
/// Utility class for unit conversions.
/// </summary>
public static class UnitConverter
{
    #region Temperature

    /// <summary>
    /// Converts Celsius to Fahrenheit.
    /// </summary>
    public static double CelsiusToFahrenheit(double celsius)
        => (celsius * 9.0 / 5.0) + 32.0;

    /// <summary>
    /// Converts Fahrenheit to Celsius.
    /// </summary>
    public static double FahrenheitToCelsius(double fahrenheit)
        => (fahrenheit - 32.0) * 5.0 / 9.0;

    /// <summary>
    /// Formats temperature with unit.
    /// </summary>
    public static string FormatTemperature(double celsius, bool useMetric)
        => useMetric
            ? $"{celsius:F0}°C"
            : $"{CelsiusToFahrenheit(celsius):F0}°F";

    /// <summary>
    /// Gets temperature value in the preferred unit.
    /// </summary>
    public static double GetTemperature(double celsius, double fahrenheit, bool useMetric)
        => useMetric ? celsius : fahrenheit;

    #endregion

    #region Distance

    /// <summary>
    /// Converts kilometers to miles.
    /// </summary>
    public static double KilometersToMiles(double km)
        => km * 0.621371;

    /// <summary>
    /// Converts miles to kilometers.
    /// </summary>
    public static double MilesToKilometers(double miles)
        => miles * 1.60934;

    /// <summary>
    /// Formats distance with unit.
    /// </summary>
    public static string FormatDistance(double km, bool useMetric)
        => useMetric
            ? $"{km:F1} km"
            : $"{KilometersToMiles(km):F1} mi";

    /// <summary>
    /// Gets distance value in the preferred unit.
    /// </summary>
    public static double GetDistance(double km, double miles, bool useMetric)
        => useMetric ? km : miles;

    #endregion

    #region Speed

    /// <summary>
    /// Converts kph to mph.
    /// </summary>
    public static double KphToMph(double kph)
        => kph * 0.621371;

    /// <summary>
    /// Converts mph to kph.
    /// </summary>
    public static double MphToKph(double mph)
        => mph * 1.60934;

    /// <summary>
    /// Formats speed with unit.
    /// </summary>
    public static string FormatSpeed(double kph, bool useMetric)
        => useMetric
            ? $"{kph:F0} kph"
            : $"{KphToMph(kph):F0} mph";

    /// <summary>
    /// Gets speed value in the preferred unit.
    /// </summary>
    public static double GetSpeed(double kph, double mph, bool useMetric)
        => useMetric ? kph : mph;

    #endregion

    #region Time

    /// <summary>
    /// Formats time according to user preference.
    /// </summary>
    public static string FormatTime(DateTime time, bool use24Hour)
        => use24Hour
            ? time.ToString("HH:mm")
            : time.ToString("h:mm tt");

    /// <summary>
    /// Formats time only according to user preference.
    /// </summary>
    public static string FormatTime(TimeOnly time, bool use24Hour)
        => use24Hour
            ? time.ToString("HH:mm")
            : time.ToString("h:mm tt");

    /// <summary>
    /// Formats time span as duration.
    /// </summary>
    public static string FormatDuration(TimeSpan duration)
    {
        if (duration.TotalHours >= 1)
        {
            return $"{(int)duration.TotalHours}h {duration.Minutes}m";
        }
        return $"{duration.Minutes}m";
    }

    /// <summary>
    /// Formats a relative time (e.g., "2 hours ago", "in 30 minutes").
    /// </summary>
    public static string FormatRelativeTime(DateTime time)
    {
        var diff = time - DateTime.Now;
        var absDiff = diff.Duration();

        if (absDiff.TotalMinutes < 1)
            return "now";

        string timeStr;
        if (absDiff.TotalMinutes < 60)
            timeStr = $"{(int)absDiff.TotalMinutes} minute{(absDiff.TotalMinutes >= 2 ? "s" : "")}";
        else if (absDiff.TotalHours < 24)
            timeStr = $"{(int)absDiff.TotalHours} hour{(absDiff.TotalHours >= 2 ? "s" : "")}";
        else
            timeStr = $"{(int)absDiff.TotalDays} day{(absDiff.TotalDays >= 2 ? "s" : "")}";

        return diff.TotalMinutes > 0 ? $"in {timeStr}" : $"{timeStr} ago";
    }

    #endregion

    #region Pressure

    /// <summary>
    /// Converts millibars to inches of mercury.
    /// </summary>
    public static double MillibarsToInHg(double mb)
        => mb * 0.02953;

    /// <summary>
    /// Converts inches of mercury to millibars.
    /// </summary>
    public static double InHgToMillibars(double inHg)
        => inHg / 0.02953;

    /// <summary>
    /// Formats pressure with unit.
    /// </summary>
    public static string FormatPressure(double mb, bool useMetric)
        => useMetric
            ? $"{mb:F0} mb"
            : $"{MillibarsToInHg(mb):F2} inHg";

    #endregion
}
