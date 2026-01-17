namespace StargazeWeatherConditions.Models;

/// <summary>
/// Represents location information returned from the weather API.
/// </summary>
public record LocationInfo
{
    public required string Name { get; init; }
    public required double Latitude { get; init; }
    public required double Longitude { get; init; }
    public string? Region { get; init; }
    public string? Country { get; init; }
    public string? TimeZoneId { get; init; }
    public DateTime? LocalTime { get; init; }
}

/// <summary>
/// Represents a location search result from autocomplete.
/// </summary>
public record LocationSearchResult
{
    public required int Id { get; init; }
    public required string Name { get; init; }
    public required double Latitude { get; init; }
    public required double Longitude { get; init; }
    public string? Region { get; init; }
    public string? Country { get; init; }

    public string DisplayName => string.IsNullOrEmpty(Region) 
        ? $"{Name}, {Country}" 
        : $"{Name}, {Region}, {Country}";
}
