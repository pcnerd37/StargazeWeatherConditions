namespace StargazeWeatherConditions.Models;

/// <summary>
/// Represents calculated twilight times for a given date and location.
/// </summary>
public record TwilightTimes
{
    public required DateOnly Date { get; init; }
    public required double Latitude { get; init; }
    public required double Longitude { get; init; }
    
    // Evening twilight (descending sun)
    public required DateTime Sunset { get; init; }
    public required DateTime CivilDusk { get; init; }
    public required DateTime NauticalDusk { get; init; }
    public required DateTime AstronomicalDusk { get; init; }
    
    // Morning twilight (ascending sun)
    public required DateTime AstronomicalDawn { get; init; }
    public required DateTime NauticalDawn { get; init; }
    public required DateTime CivilDawn { get; init; }
    public required DateTime Sunrise { get; init; }

    /// <summary>
    /// Duration of true astronomical darkness (optimal for deep-sky observation).
    /// </summary>
    public TimeSpan OptimalViewingDuration => AstronomicalDawn - AstronomicalDusk;

    /// <summary>
    /// Indicates if the given time falls within the optimal viewing window.
    /// </summary>
    public bool IsOptimalViewingTime(DateTime time) 
        => time >= AstronomicalDusk && time <= AstronomicalDawn;

    /// <summary>
    /// Gets the twilight phase for a given time.
    /// </summary>
    public TwilightPhase GetTwilightPhase(DateTime time)
    {
        if (time < Sunset || time > Sunrise)
            return TwilightPhase.Day;
        if (time >= Sunset && time < CivilDusk)
            return TwilightPhase.CivilEvening;
        if (time >= CivilDusk && time < NauticalDusk)
            return TwilightPhase.NauticalEvening;
        if (time >= NauticalDusk && time < AstronomicalDusk)
            return TwilightPhase.AstronomicalEvening;
        if (time >= AstronomicalDusk && time <= AstronomicalDawn)
            return TwilightPhase.Night;
        if (time > AstronomicalDawn && time <= NauticalDawn)
            return TwilightPhase.AstronomicalMorning;
        if (time > NauticalDawn && time <= CivilDawn)
            return TwilightPhase.NauticalMorning;
        if (time > CivilDawn && time <= Sunrise)
            return TwilightPhase.CivilMorning;
        
        return TwilightPhase.Day;
    }
}

/// <summary>
/// Represents the different phases of twilight.
/// </summary>
public enum TwilightPhase
{
    Day,
    CivilEvening,
    NauticalEvening,
    AstronomicalEvening,
    Night,
    AstronomicalMorning,
    NauticalMorning,
    CivilMorning
}
