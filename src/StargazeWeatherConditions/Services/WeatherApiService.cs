using System.Globalization;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using StargazeWeatherConditions.Models;
using StargazeWeatherConditions.Models.Api;

namespace StargazeWeatherConditions.Services;

/// <summary>
/// Implementation of weather API service using WeatherAPI.com.
/// </summary>
public class WeatherApiService : IWeatherApiService
{
    private readonly HttpClient _httpClient;
    private readonly IPreferencesService _preferencesService;
    private readonly ICacheService _cacheService;
    private readonly string _defaultApiKey;

    public WeatherApiService(
        HttpClient httpClient,
        IPreferencesService preferencesService,
        ICacheService cacheService,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _preferencesService = preferencesService;
        _cacheService = cacheService;
        _defaultApiKey = configuration["WeatherApi:ApiKey"] ?? "";
    }

    public async Task<WeatherForecast?> GetForecastAsync(
        string location,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(location);
        
        var cacheKey = $"forecast_{location.ToLowerInvariant().Replace(" ", "_")}";
        CacheResult<WeatherForecast>? cacheResult = null;
        
        try
        {
            cacheResult = await _cacheService.GetAsync<WeatherForecast>(cacheKey);
        }
        catch
        {
            // Ignore cache errors
        }
        
        if (cacheResult is { IsHit: true, IsStale: false, Data: not null })
        {
            return cacheResult.Data;
        }

        try
        {
            var apiKey = await GetApiKeyAsync();
            var url = $"forecast.json?key={apiKey}&q={Uri.EscapeDataString(location)}&days=3&aqi=no";
            
            var httpResponse = await _httpClient.GetAsync(url, cancellationToken);
            
            // Don't swallow authentication errors - these indicate configuration problems
            if (httpResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized ||
                httpResponse.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                throw new HttpRequestException($"Authentication failed: {httpResponse.StatusCode}", null, httpResponse.StatusCode);
            }
            
            // For other non-success codes, return null or cached data
            if (!httpResponse.IsSuccessStatusCode)
            {
                return cacheResult is { IsHit: true } ? cacheResult.Data : null;
            }
            
            var response = await httpResponse.Content.ReadFromJsonAsync<WeatherApiResponse>(cancellationToken);
            
            if (response is null)
            {
                return cacheResult is { IsHit: true } ? cacheResult.Data : null;
            }

            var forecast = MapToWeatherForecast(response);
            await _cacheService.SetAsync(cacheKey, forecast, TimeSpan.FromHours(2));
            
            return forecast;
        }
        catch (HttpRequestException ex) when (ex.StatusCode is not (System.Net.HttpStatusCode.Unauthorized or System.Net.HttpStatusCode.Forbidden))
        {
            // Return stale cache if available for network errors, but NOT for auth errors
            return cacheResult is { IsHit: true } ? cacheResult.Data : null;
        }
    }

    public async Task<WeatherForecast?> GetForecastAsync(
        double latitude,
        double longitude,
        CancellationToken cancellationToken = default)
    {
        var location = $"{latitude.ToString(CultureInfo.InvariantCulture)},{longitude.ToString(CultureInfo.InvariantCulture)}";
        return await GetForecastAsync(location, cancellationToken);
    }

    public async Task<IReadOnlyList<LocationSearchResult>> SearchLocationsAsync(
        string query,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
        {
            return Array.Empty<LocationSearchResult>();
        }

        try
        {
            var apiKey = await GetApiKeyAsync();
            var url = $"search.json?key={apiKey}&q={Uri.EscapeDataString(query)}";
            
            var response = await _httpClient.GetFromJsonAsync<List<WeatherApiSearchResult>>(url, cancellationToken);
            
            if (response is null)
            {
                return Array.Empty<LocationSearchResult>();
            }

            return response.Select(r => new LocationSearchResult
            {
                Id = r.Id,
                Name = r.Name,
                Region = r.Region,
                Country = r.Country,
                Latitude = r.Lat,
                Longitude = r.Lon
            }).ToList();
        }
        catch (HttpRequestException)
        {
            return Array.Empty<LocationSearchResult>();
        }
    }

    public async Task<bool> ValidateApiKeyAsync(
        string apiKey,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return false;
        }

        try
        {
            var url = $"search.json?key={apiKey}&q=London";
            var response = await _httpClient.GetAsync(url, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    private async Task<string> GetApiKeyAsync()
    {
        var customKey = await _preferencesService.GetCustomApiKeyAsync();
        return string.IsNullOrWhiteSpace(customKey) ? _defaultApiKey : customKey;
    }

    private static WeatherForecast MapToWeatherForecast(WeatherApiResponse response)
    {
        return new WeatherForecast
        {
            Location = new LocationInfo
            {
                Name = response.Location.Name,
                Region = response.Location.Region,
                Country = response.Location.Country,
                Latitude = response.Location.Lat,
                Longitude = response.Location.Lon,
                TimeZoneId = response.Location.TzId,
                LocalTime = ParseLocalTime(response.Location.Localtime)
            },
            ForecastDays = response.Forecast.ForecastDay.Select(MapToForecastDay).ToList(),
            RetrievedAt = DateTime.UtcNow
        };
    }

    private static ForecastDay MapToForecastDay(WeatherApiForecastDay day)
    {
        return new ForecastDay
        {
            Date = DateOnly.Parse(day.Date),
            DaySummary = new DaySummary
            {
                MaxTempCelsius = day.Day.MaxTempC,
                MaxTempFahrenheit = day.Day.MaxTempF,
                MinTempCelsius = day.Day.MinTempC,
                MinTempFahrenheit = day.Day.MinTempF,
                AvgTempCelsius = day.Day.AvgTempC,
                AvgTempFahrenheit = day.Day.AvgTempF,
                MaxWindMph = day.Day.MaxWindMph,
                MaxWindKph = day.Day.MaxWindKph,
                TotalPrecipMm = day.Day.TotalPrecipMm,
                TotalPrecipIn = day.Day.TotalPrecipIn,
                AvgVisibilityKm = day.Day.AvgVisKm,
                AvgVisibilityMiles = day.Day.AvgVisMiles,
                AvgHumidityPercent = day.Day.AvgHumidity,
                ChanceOfRainPercent = day.Day.DailyChanceOfRain,
                ChanceOfSnowPercent = day.Day.DailyChanceOfSnow,
                Condition = MapCondition(day.Day.Condition)
            },
            Astronomy = new AstronomyData
            {
                Sunrise = ParseTimeOnly(day.Astro.Sunrise),
                Sunset = ParseTimeOnly(day.Astro.Sunset),
                Moonrise = TryParseTimeOnly(day.Astro.Moonrise),
                Moonset = TryParseTimeOnly(day.Astro.Moonset),
                MoonPhase = day.Astro.MoonPhase,
                MoonIlluminationPercent = day.Astro.MoonIllumination,
                IsMoonUp = day.Astro.IsMoonUp == 1,
                IsSunUp = day.Astro.IsSunUp == 1
            },
            HourlyConditions = day.Hour.Select(MapToHourlyCondition).ToList()
        };
    }

    private static HourlyCondition MapToHourlyCondition(WeatherApiHour hour)
    {
        return new HourlyCondition
        {
            Time = DateTime.Parse(hour.Time),
            TimeEpoch = hour.TimeEpoch,
            TemperatureCelsius = hour.TempC,
            TemperatureFahrenheit = hour.TempF,
            FeelsLikeCelsius = hour.FeelsLikeC,
            FeelsLikeFahrenheit = hour.FeelsLikeF,
            CloudCoverPercent = hour.Cloud,
            HumidityPercent = hour.Humidity,
            VisibilityKm = hour.VisKm,
            VisibilityMiles = hour.VisMiles,
            WindSpeedMph = hour.WindMph,
            WindSpeedKph = hour.WindKph,
            WindDegree = hour.WindDegree,
            WindDirection = hour.WindDir,
            GustMph = hour.GustMph,
            GustKph = hour.GustKph,
            PressureMb = hour.PressureMb,
            PressureIn = hour.PressureIn,
            PrecipMm = hour.PrecipMm,
            PrecipIn = hour.PrecipIn,
            ChanceOfRainPercent = hour.ChanceOfRain,
            ChanceOfSnowPercent = hour.ChanceOfSnow,
            DewPointCelsius = hour.DewpointC,
            DewPointFahrenheit = hour.DewpointF,
            UvIndex = hour.Uv,
            Condition = MapCondition(hour.Condition),
            IsDay = hour.IsDay == 1
        };
    }

    private static WeatherCondition MapCondition(WeatherApiCondition condition)
    {
        return new WeatherCondition
        {
            Text = condition.Text,
            IconUrl = condition.Icon.StartsWith("//") 
                ? $"https:{condition.Icon}" 
                : condition.Icon,
            Code = condition.Code
        };
    }

    private static TimeOnly ParseTimeOnly(string time)
    {
        // WeatherAPI returns times like "07:15 AM" or "05:02 PM"
        if (DateTime.TryParse(time, out var dateTime))
        {
            return TimeOnly.FromDateTime(dateTime);
        }
        return TimeOnly.MinValue;
    }

    private static TimeOnly? TryParseTimeOnly(string? time)
    {
        if (string.IsNullOrWhiteSpace(time) || time == "No moonrise" || time == "No moonset")
        {
            return null;
        }
        if (DateTime.TryParse(time, out var dateTime))
        {
            return TimeOnly.FromDateTime(dateTime);
        }
        return null;
    }

    private static DateTime? ParseLocalTime(string? localtime)
    {
        if (string.IsNullOrWhiteSpace(localtime))
        {
            return null;
        }
        if (DateTime.TryParse(localtime, out var dateTime))
        {
            return dateTime;
        }
        return null;
    }
}
