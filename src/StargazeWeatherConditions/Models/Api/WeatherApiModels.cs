using System.Text.Json.Serialization;

namespace StargazeWeatherConditions.Models.Api;

/// <summary>
/// WeatherAPI.com forecast response structure.
/// </summary>
public record WeatherApiResponse
{
    [JsonPropertyName("location")]
    public required WeatherApiLocation Location { get; init; }

    [JsonPropertyName("forecast")]
    public required WeatherApiForecast Forecast { get; init; }
}

public record WeatherApiLocation
{
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("region")]
    public string? Region { get; init; }

    [JsonPropertyName("country")]
    public string? Country { get; init; }

    [JsonPropertyName("lat")]
    public double Lat { get; init; }

    [JsonPropertyName("lon")]
    public double Lon { get; init; }

    [JsonPropertyName("tz_id")]
    public string? TzId { get; init; }

    [JsonPropertyName("localtime_epoch")]
    public long LocaltimeEpoch { get; init; }

    [JsonPropertyName("localtime")]
    public string? Localtime { get; init; }
}

public record WeatherApiForecast
{
    [JsonPropertyName("forecastday")]
    public required IReadOnlyList<WeatherApiForecastDay> ForecastDay { get; init; }
}

public record WeatherApiForecastDay
{
    [JsonPropertyName("date")]
    public required string Date { get; init; }

    [JsonPropertyName("date_epoch")]
    public long DateEpoch { get; init; }

    [JsonPropertyName("day")]
    public required WeatherApiDay Day { get; init; }

    [JsonPropertyName("astro")]
    public required WeatherApiAstro Astro { get; init; }

    [JsonPropertyName("hour")]
    public required IReadOnlyList<WeatherApiHour> Hour { get; init; }
}

public record WeatherApiDay
{
    [JsonPropertyName("maxtemp_c")]
    public double MaxTempC { get; init; }

    [JsonPropertyName("maxtemp_f")]
    public double MaxTempF { get; init; }

    [JsonPropertyName("mintemp_c")]
    public double MinTempC { get; init; }

    [JsonPropertyName("mintemp_f")]
    public double MinTempF { get; init; }

    [JsonPropertyName("avgtemp_c")]
    public double AvgTempC { get; init; }

    [JsonPropertyName("avgtemp_f")]
    public double AvgTempF { get; init; }

    [JsonPropertyName("maxwind_mph")]
    public double MaxWindMph { get; init; }

    [JsonPropertyName("maxwind_kph")]
    public double MaxWindKph { get; init; }

    [JsonPropertyName("totalprecip_mm")]
    public double TotalPrecipMm { get; init; }

    [JsonPropertyName("totalprecip_in")]
    public double TotalPrecipIn { get; init; }

    [JsonPropertyName("avgvis_km")]
    public double AvgVisKm { get; init; }

    [JsonPropertyName("avgvis_miles")]
    public double AvgVisMiles { get; init; }

    [JsonPropertyName("avghumidity")]
    public int AvgHumidity { get; init; }

    [JsonPropertyName("daily_chance_of_rain")]
    public int DailyChanceOfRain { get; init; }

    [JsonPropertyName("daily_chance_of_snow")]
    public int DailyChanceOfSnow { get; init; }

    [JsonPropertyName("condition")]
    public required WeatherApiCondition Condition { get; init; }
}

public record WeatherApiAstro
{
    [JsonPropertyName("sunrise")]
    public required string Sunrise { get; init; }

    [JsonPropertyName("sunset")]
    public required string Sunset { get; init; }

    [JsonPropertyName("moonrise")]
    public string? Moonrise { get; init; }

    [JsonPropertyName("moonset")]
    public string? Moonset { get; init; }

    [JsonPropertyName("moon_phase")]
    public required string MoonPhase { get; init; }

    [JsonPropertyName("moon_illumination")]
    public int MoonIllumination { get; init; }

    [JsonPropertyName("is_moon_up")]
    public int IsMoonUp { get; init; }

    [JsonPropertyName("is_sun_up")]
    public int IsSunUp { get; init; }
}

public record WeatherApiHour
{
    [JsonPropertyName("time_epoch")]
    public long TimeEpoch { get; init; }

    [JsonPropertyName("time")]
    public required string Time { get; init; }

    [JsonPropertyName("temp_c")]
    public double TempC { get; init; }

    [JsonPropertyName("temp_f")]
    public double TempF { get; init; }

    [JsonPropertyName("feelslike_c")]
    public double FeelsLikeC { get; init; }

    [JsonPropertyName("feelslike_f")]
    public double FeelsLikeF { get; init; }

    [JsonPropertyName("humidity")]
    public int Humidity { get; init; }

    [JsonPropertyName("cloud")]
    public int Cloud { get; init; }

    [JsonPropertyName("vis_km")]
    public double VisKm { get; init; }

    [JsonPropertyName("vis_miles")]
    public double VisMiles { get; init; }

    [JsonPropertyName("wind_mph")]
    public double WindMph { get; init; }

    [JsonPropertyName("wind_kph")]
    public double WindKph { get; init; }

    [JsonPropertyName("wind_degree")]
    public int WindDegree { get; init; }

    [JsonPropertyName("wind_dir")]
    public required string WindDir { get; init; }

    [JsonPropertyName("gust_mph")]
    public double GustMph { get; init; }

    [JsonPropertyName("gust_kph")]
    public double GustKph { get; init; }

    [JsonPropertyName("pressure_mb")]
    public double PressureMb { get; init; }

    [JsonPropertyName("pressure_in")]
    public double PressureIn { get; init; }

    [JsonPropertyName("precip_mm")]
    public double PrecipMm { get; init; }

    [JsonPropertyName("precip_in")]
    public double PrecipIn { get; init; }

    [JsonPropertyName("chance_of_rain")]
    public int ChanceOfRain { get; init; }

    [JsonPropertyName("chance_of_snow")]
    public int ChanceOfSnow { get; init; }

    [JsonPropertyName("dewpoint_c")]
    public double DewpointC { get; init; }

    [JsonPropertyName("dewpoint_f")]
    public double DewpointF { get; init; }

    [JsonPropertyName("uv")]
    public double Uv { get; init; }

    [JsonPropertyName("is_day")]
    public int IsDay { get; init; }

    [JsonPropertyName("condition")]
    public required WeatherApiCondition Condition { get; init; }
}

public record WeatherApiCondition
{
    [JsonPropertyName("text")]
    public required string Text { get; init; }

    [JsonPropertyName("icon")]
    public required string Icon { get; init; }

    [JsonPropertyName("code")]
    public int Code { get; init; }
}

/// <summary>
/// WeatherAPI.com search/autocomplete response.
/// </summary>
public record WeatherApiSearchResult
{
    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("region")]
    public string? Region { get; init; }

    [JsonPropertyName("country")]
    public string? Country { get; init; }

    [JsonPropertyName("lat")]
    public double Lat { get; init; }

    [JsonPropertyName("lon")]
    public double Lon { get; init; }
}
