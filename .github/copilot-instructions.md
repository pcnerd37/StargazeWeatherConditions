# Copilot Instructions for StarGaze Weather Conditions

This document provides guidelines and best practices for GitHub Copilot when assisting with development of the StarGaze Weather Conditions application.

---

## Project Overview

**StarGaze Weather Conditions** is a .NET 10 Blazor WebAssembly application that helps stargazers and astrophotographers determine optimal conditions for nighttime observation. The app displays weather forecasts, astronomical data, and light pollution information with recommendations for stargazing.

### Technology Stack

- **Framework**: .NET 10, Blazor WebAssembly
- **Language**: C# 13
- **Hosting**: GitHub Pages (static files)
- **APIs**: WeatherAPI.com, Light Pollution Map API
- **Testing**: xUnit, Moq, bUnit
- **CI/CD**: GitHub Actions

---

## Project Structure

Follow this directory structure for all new files:

```
src/StargazeWeatherConditions/
├── Components/           # Reusable Blazor components
│   ├── Layout/          # Layout components (MainLayout, NavBar, Footer)
│   ├── Weather/         # Weather display components
│   ├── Astronomy/       # Moon, twilight, sun components
│   ├── Recommendations/ # Scoring and recommendation components
│   ├── Location/        # Location search and geolocation
│   └── Settings/        # User preferences components
├── Pages/               # Routable page components
├── Services/            # Business logic and API services
├── Models/              # Data models and DTOs
├── Utilities/           # Helper classes and calculations
└── wwwroot/             # Static assets (CSS, appsettings.json)

tests/StargazeWeatherConditions.Tests/
├── Services/            # Service unit tests
├── Utilities/           # Utility unit tests
└── Components/          # bUnit component tests
```

---

## Coding Standards

### General C# Guidelines

1. **Use file-scoped namespaces**:
   ```csharp
   namespace StargazeWeatherConditions.Services;
   
   public class WeatherApiService : IWeatherApiService
   {
       // ...
   }
   ```

2. **Use primary constructors for dependency injection** (.NET 10):
   ```csharp
   public class WeatherApiService(HttpClient httpClient, ICacheService cacheService) 
       : IWeatherApiService
   {
       public async Task<WeatherForecast?> GetForecastAsync(string location)
       {
           // Use httpClient and cacheService directly
       }
   }
   ```

3. **Use pattern matching and switch expressions**:
   ```csharp
   public string GetRatingLabel(double score) => score switch
   {
       >= 85 => "Excellent",
       >= 70 => "Good",
       >= 50 => "Fair",
       _ => "Poor"
   };
   ```

4. **Use nullable reference types** - all nullable types should be explicit:
   ```csharp
   public WeatherForecast? CachedForecast { get; private set; }
   public async Task<LocationInfo?> GetLocationAsync(string query)
   ```

5. **Use `required` keyword for mandatory properties**:
   ```csharp
   public class LocationInfo
   {
       public required string Name { get; init; }
       public required double Latitude { get; init; }
       public required double Longitude { get; init; }
       public string? Region { get; init; }
   }
   ```

### Async/Await Patterns

1. **Always use async/await for I/O operations**:
   ```csharp
   public async Task<WeatherForecast> GetForecastAsync(
       string location, 
       CancellationToken cancellationToken = default)
   {
       var response = await _httpClient.GetAsync(url, cancellationToken);
       response.EnsureSuccessStatusCode();
       return await response.Content.ReadFromJsonAsync<WeatherForecast>(cancellationToken) 
           ?? throw new InvalidOperationException("Failed to parse forecast");
   }
   ```

2. **Include CancellationToken parameters** on async methods:
   ```csharp
   Task<T> GetDataAsync(CancellationToken cancellationToken = default);
   ```

3. **Use ConfigureAwait(false) in library/service code**:
   ```csharp
   var data = await _httpClient.GetStringAsync(url).ConfigureAwait(false);
   ```

### Error Handling

1. **Use Result pattern for expected failures**:
   ```csharp
   public record ApiResult<T>(T? Data, string? Error, bool IsSuccess)
   {
       public static ApiResult<T> Success(T data) => new(data, null, true);
       public static ApiResult<T> Failure(string error) => new(default, error, false);
   }
   ```

2. **Throw exceptions for unexpected failures**:
   ```csharp
   public async Task<WeatherForecast> GetForecastAsync(string location)
   {
       ArgumentException.ThrowIfNullOrWhiteSpace(location);
       // ...
   }
   ```

3. **Log errors appropriately** (when logging is added):
   ```csharp
   catch (HttpRequestException ex)
   {
       _logger.LogError(ex, "Failed to fetch weather for {Location}", location);
       return ApiResult<WeatherForecast>.Failure("Weather service unavailable");
   }
   ```

---

## Service Implementation Patterns

### Interface-First Design

Always define interfaces for services:

```csharp
// IWeatherApiService.cs
namespace StargazeWeatherConditions.Services;

public interface IWeatherApiService
{
    Task<WeatherForecast?> GetForecastAsync(
        string location, 
        CancellationToken cancellationToken = default);
    
    Task<IReadOnlyList<LocationSearchResult>> SearchLocationsAsync(
        string query, 
        CancellationToken cancellationToken = default);
}
```

### HttpClient Configuration with Polly

```csharp
// In Program.cs
builder.Services.AddHttpClient<IWeatherApiService, WeatherApiService>(client =>
{
    client.BaseAddress = new Uri("https://api.weatherapi.com/v1/");
    client.Timeout = TimeSpan.FromSeconds(30);
})
.AddTransientHttpErrorPolicy(policy => 
    policy.WaitAndRetryAsync(
        retryCount: 3,
        sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
        onRetry: (outcome, timespan, attempt, context) =>
        {
            // Log retry attempts
        }));
```

### Caching Service

```csharp
public interface ICacheService
{
    Task<T?> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T value, TimeSpan? ttl = null);
    Task RemoveAsync(string key);
    Task<bool> HasValidCacheAsync(string key);
}

public class CacheService(IJSRuntime jsRuntime) : ICacheService
{
    private static readonly TimeSpan DefaultTtl = TimeSpan.FromHours(2);

    public async Task<T?> GetAsync<T>(string key)
    {
        var json = await jsRuntime.InvokeAsync<string?>("localStorage.getItem", key);
        if (string.IsNullOrEmpty(json)) return default;

        var entry = JsonSerializer.Deserialize<CacheEntry<T>>(json);
        if (entry is null || entry.ExpiresAt < DateTime.UtcNow)
        {
            await RemoveAsync(key);
            return default;
        }

        return entry.Data;
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? ttl = null)
    {
        var entry = new CacheEntry<T>
        {
            Data = value,
            Timestamp = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.Add(ttl ?? DefaultTtl)
        };
        var json = JsonSerializer.Serialize(entry);
        await jsRuntime.InvokeVoidAsync("localStorage.setItem", key, json);
    }
}

public record CacheEntry<T>
{
    public required T Data { get; init; }
    public required DateTime Timestamp { get; init; }
    public required DateTime ExpiresAt { get; init; }
}
```

### Preferences Service

```csharp
public interface IPreferencesService
{
    Task<UserPreferences> GetPreferencesAsync();
    Task SavePreferencesAsync(UserPreferences preferences);
    Task<string?> GetCustomApiKeyAsync();
    Task SetCustomApiKeyAsync(string? apiKey);
}

public record UserPreferences
{
    public bool IsDarkTheme { get; init; } = true;
    public bool UseMetricUnits { get; init; } = false;
    public bool Use24HourTime { get; init; } = false;
}
```

---

## Model Definitions

### Weather Models

```csharp
namespace StargazeWeatherConditions.Models;

public record WeatherForecast
{
    public required LocationInfo Location { get; init; }
    public required IReadOnlyList<ForecastDay> ForecastDays { get; init; }
}

public record ForecastDay
{
    public required DateOnly Date { get; init; }
    public required AstronomyData Astronomy { get; init; }
    public required IReadOnlyList<HourlyCondition> HourlyConditions { get; init; }
}

public record HourlyCondition
{
    public required DateTime Time { get; init; }
    public required double TemperatureCelsius { get; init; }
    public required double TemperatureFahrenheit { get; init; }
    public required int CloudCoverPercent { get; init; }
    public required int HumidityPercent { get; init; }
    public required double VisibilityKm { get; init; }
    public required double VisibilityMiles { get; init; }
    public required double WindSpeedMph { get; init; }
    public required double WindSpeedKph { get; init; }
    public required string WindDirection { get; init; }
    public required int ChanceOfRainPercent { get; init; }
    public required WeatherCondition Condition { get; init; }
    public required bool IsDay { get; init; }
}

public record WeatherCondition
{
    public required string Text { get; init; }
    public required string IconUrl { get; init; }
    public required int Code { get; init; }
}

public record AstronomyData
{
    public required TimeOnly Sunrise { get; init; }
    public required TimeOnly Sunset { get; init; }
    public required TimeOnly? Moonrise { get; init; }
    public required TimeOnly? Moonset { get; init; }
    public required string MoonPhase { get; init; }
    public required int MoonIlluminationPercent { get; init; }
    public required bool IsMoonUp { get; init; }
}
```

### Twilight Models

```csharp
public record TwilightTimes
{
    public required DateTime Sunset { get; init; }
    public required DateTime CivilDusk { get; init; }
    public required DateTime NauticalDusk { get; init; }
    public required DateTime AstronomicalDusk { get; init; }
    public required DateTime AstronomicalDawn { get; init; }
    public required DateTime NauticalDawn { get; init; }
    public required DateTime CivilDawn { get; init; }
    public required DateTime Sunrise { get; init; }
    
    public TimeSpan OptimalViewingDuration => AstronomicalDawn - AstronomicalDusk;
}
```

### Recommendation Models

```csharp
public record Recommendation
{
    public required double OverallScore { get; init; }
    public required Rating Rating { get; init; }
    public required string Summary { get; init; }
    public required IReadOnlyList<FactorScore> FactorScores { get; init; }
    public required double DsoScore { get; init; }
    public required double PlanetaryScore { get; init; }
}

public record FactorScore
{
    public required string FactorName { get; init; }
    public required double Score { get; init; }
    public required double Weight { get; init; }
    public required string Label { get; init; }
}

public enum Rating
{
    Excellent,
    Good,
    Fair,
    Poor
}
```

---

## Utility Implementation Patterns

### Twilight Calculator

```csharp
namespace StargazeWeatherConditions.Utilities;

public interface ITwilightCalculator
{
    TwilightTimes Calculate(double latitude, double longitude, DateOnly date);
}

public class TwilightCalculator : ITwilightCalculator
{
    private const double CivilTwilightAngle = -6.0;
    private const double NauticalTwilightAngle = -12.0;
    private const double AstronomicalTwilightAngle = -18.0;

    public TwilightTimes Calculate(double latitude, double longitude, DateOnly date)
    {
        // Implement solar position algorithm
        // Reference: NOAA Solar Calculator or simplified formula
        
        var sunset = CalculateSunEvent(latitude, longitude, date, 0, isRising: false);
        var sunrise = CalculateSunEvent(latitude, longitude, date.AddDays(1), 0, isRising: true);
        
        return new TwilightTimes
        {
            Sunset = sunset,
            CivilDusk = CalculateSunEvent(latitude, longitude, date, CivilTwilightAngle, false),
            NauticalDusk = CalculateSunEvent(latitude, longitude, date, NauticalTwilightAngle, false),
            AstronomicalDusk = CalculateSunEvent(latitude, longitude, date, AstronomicalTwilightAngle, false),
            AstronomicalDawn = CalculateSunEvent(latitude, longitude, date.AddDays(1), AstronomicalTwilightAngle, true),
            NauticalDawn = CalculateSunEvent(latitude, longitude, date.AddDays(1), NauticalTwilightAngle, true),
            CivilDawn = CalculateSunEvent(latitude, longitude, date.AddDays(1), CivilTwilightAngle, true),
            Sunrise = sunrise
        };
    }

    private DateTime CalculateSunEvent(
        double latitude, 
        double longitude, 
        DateOnly date, 
        double angle, 
        bool isRising)
    {
        // Solar position calculation implementation
        // Use well-tested algorithms (NOAA/Jean Meeus)
        throw new NotImplementedException();
    }
}
```

### Recommendation Scorer

```csharp
namespace StargazeWeatherConditions.Utilities;

public interface IRecommendationScorer
{
    Recommendation CalculateRecommendation(
        HourlyCondition conditions,
        AstronomyData astronomy,
        int? bortleScale);
}

public class RecommendationScorer : IRecommendationScorer
{
    private const double CloudCoverWeight = 0.35;
    private const double MoonIlluminationWeight = 0.25;
    private const double HumidityWeight = 0.15;
    private const double VisibilityWeight = 0.15;
    private const double LightPollutionWeight = 0.10;

    public Recommendation CalculateRecommendation(
        HourlyCondition conditions,
        AstronomyData astronomy,
        int? bortleScale)
    {
        var cloudScore = ScoreCloudCover(conditions.CloudCoverPercent);
        var moonScore = ScoreMoonIllumination(astronomy.MoonIlluminationPercent);
        var humidityScore = ScoreHumidity(conditions.HumidityPercent);
        var visibilityScore = ScoreVisibility(conditions.VisibilityKm);
        var lightPollutionScore = bortleScale.HasValue 
            ? ScoreLightPollution(bortleScale.Value) 
            : 50; // Neutral if unavailable

        var overallScore = 
            (cloudScore * CloudCoverWeight) +
            (moonScore * MoonIlluminationWeight) +
            (humidityScore * HumidityWeight) +
            (visibilityScore * VisibilityWeight) +
            (lightPollutionScore * LightPollutionWeight);

        return new Recommendation
        {
            OverallScore = overallScore,
            Rating = GetRating(overallScore),
            Summary = GenerateSummary(cloudScore, moonScore, astronomy.MoonPhase),
            FactorScores = CreateFactorScores(cloudScore, moonScore, humidityScore, visibilityScore, lightPollutionScore),
            DsoScore = CalculateDsoScore(cloudScore, moonScore, lightPollutionScore, visibilityScore),
            PlanetaryScore = CalculatePlanetaryScore(cloudScore, visibilityScore, humidityScore)
        };
    }

    private static double ScoreCloudCover(int cloudPercent) => cloudPercent switch
    {
        <= 20 => 100,
        <= 40 => 75,
        <= 60 => 50,
        <= 80 => 25,
        _ => 0
    };

    private static double ScoreMoonIllumination(int illuminationPercent) => illuminationPercent switch
    {
        <= 10 => 100,
        <= 30 => 85,
        <= 50 => 60,
        <= 70 => 35,
        _ => 10
    };

    private static double ScoreHumidity(int humidityPercent) => humidityPercent switch
    {
        <= 50 => 100,
        <= 70 => 75,
        <= 80 => 50,
        <= 90 => 25,
        _ => 0
    };

    private static double ScoreVisibility(double visibilityKm) => visibilityKm switch
    {
        > 10 => 100,
        > 7 => 75,
        > 5 => 50,
        > 3 => 25,
        _ => 0
    };

    private static double ScoreLightPollution(int bortleScale) => bortleScale switch
    {
        <= 2 => 100,
        3 => 85,
        4 => 65,
        5 => 45,
        6 => 25,
        _ => 10
    };

    private static Rating GetRating(double score) => score switch
    {
        >= 85 => Rating.Excellent,
        >= 70 => Rating.Good,
        >= 50 => Rating.Fair,
        _ => Rating.Poor
    };

    private static string GenerateSummary(double cloudScore, double moonScore, string moonPhase)
    {
        // Generate contextual summary based on conditions
        if (cloudScore >= 80 && moonScore >= 80)
            return $"Excellent for deep sky imaging - {moonPhase}, clear skies";
        if (cloudScore >= 80 && moonScore < 40)
            return "Good for planetary/lunar observation - clear skies, bright moon";
        if (cloudScore < 50)
            return "Poor conditions - high cloud cover expected";
        return "Fair conditions - consider planetary or lunar targets";
    }

    // Additional helper methods...
}
```

### Unit Converter

```csharp
namespace StargazeWeatherConditions.Utilities;

public static class UnitConverter
{
    public static double CelsiusToFahrenheit(double celsius) 
        => (celsius * 9 / 5) + 32;

    public static double FahrenheitToCelsius(double fahrenheit) 
        => (fahrenheit - 32) * 5 / 9;

    public static double KilometersToMiles(double km) 
        => km * 0.621371;

    public static double MilesToKilometers(double miles) 
        => miles * 1.60934;

    public static double KphToMph(double kph) 
        => kph * 0.621371;

    public static double MphToKph(double mph) 
        => mph * 1.60934;

    public static string FormatTemperature(double celsius, bool useMetric)
        => useMetric 
            ? $"{celsius:F1}°C" 
            : $"{CelsiusToFahrenheit(celsius):F1}°F";

    public static string FormatDistance(double km, bool useMetric)
        => useMetric 
            ? $"{km:F1} km" 
            : $"{KilometersToMiles(km):F1} mi";

    public static string FormatSpeed(double kph, bool useMetric)
        => useMetric 
            ? $"{kph:F1} kph" 
            : $"{KphToMph(kph):F1} mph";

    public static string FormatTime(DateTime time, bool use24Hour)
        => use24Hour 
            ? time.ToString("HH:mm") 
            : time.ToString("h:mm tt");
}
```

---

## Blazor Component Patterns

### Component Structure

```razor
@* HourlyForecastCard.razor *@
@namespace StargazeWeatherConditions.Components.Weather

<div class="hourly-card @(IsOptimalHour ? "optimal" : "")">
    <div class="hour-time">@FormatTime(Hour.Time)</div>
    <img src="@Hour.Condition.IconUrl" alt="@Hour.Condition.Text" class="weather-icon" />
    <div class="temperature">@FormatTemperature(Hour.TemperatureCelsius)</div>
    <div class="cloud-cover">
        <span class="label">Clouds</span>
        <div class="progress-bar">
            <div class="progress" style="width: @Hour.CloudCoverPercent%"></div>
        </div>
        <span class="value">@Hour.CloudCoverPercent%</span>
    </div>
    <div class="humidity">@Hour.HumidityPercent% humidity</div>
</div>

@code {
    [Parameter, EditorRequired]
    public required HourlyCondition Hour { get; set; }

    [Parameter]
    public bool IsOptimalHour { get; set; }

    [CascadingParameter]
    public UserPreferences? Preferences { get; set; }

    private bool UseMetric => Preferences?.UseMetricUnits ?? false;
    private bool Use24Hour => Preferences?.Use24HourTime ?? false;

    private string FormatTemperature(double celsius) 
        => UnitConverter.FormatTemperature(celsius, UseMetric);

    private string FormatTime(DateTime time) 
        => UnitConverter.FormatTime(time, Use24Hour);
}
```

### Cascading Parameters for Preferences

```razor
@* MainLayout.razor *@
@inherits LayoutComponentBase
@inject IPreferencesService PreferencesService

<CascadingValue Value="@_preferences">
    <div class="app-container" data-theme="@(_preferences?.IsDarkTheme == true ? "dark" : "light")">
        <NavBar OnSettingsClick="@ShowSettings" />
        <main>
            @Body
        </main>
        <Footer />
    </div>
</CascadingValue>

@code {
    private UserPreferences? _preferences;

    protected override async Task OnInitializedAsync()
    {
        _preferences = await PreferencesService.GetPreferencesAsync();
    }

    private async Task OnPreferencesChanged(UserPreferences newPreferences)
    {
        _preferences = newPreferences;
        await PreferencesService.SavePreferencesAsync(newPreferences);
        StateHasChanged();
    }
}
```

### Error Boundary Usage

```razor
@* Index.razor *@
@page "/"

<ErrorBoundary @ref="_errorBoundary">
    <ChildContent>
        <WeatherDisplay Location="@_currentLocation" />
    </ChildContent>
    <ErrorContent Context="exception">
        <div class="error-container">
            <h2>Something went wrong</h2>
            <p>@exception.Message</p>
            <button @onclick="RecoverFromError">Try Again</button>
        </div>
    </ErrorContent>
</ErrorBoundary>

@code {
    private ErrorBoundary? _errorBoundary;

    private void RecoverFromError()
    {
        _errorBoundary?.Recover();
    }
}
```

---

## CSS and Theming

### Theme Implementation

```css
/* themes.css */
:root {
    /* Light Theme (default) */
    --color-background: #ffffff;
    --color-surface: #f3f4f6;
    --color-surface-elevated: #ffffff;
    --color-text-primary: #111827;
    --color-text-secondary: #6b7280;
    --color-text-tertiary: #9ca3af;
    --color-border: #e5e7eb;
    --color-accent: #3b82f6;
    --color-accent-hover: #2563eb;
    
    /* Rating colors */
    --color-excellent: #22c55e;
    --color-good: #3b82f6;
    --color-fair: #eab308;
    --color-poor: #ef4444;
    
    /* Shadows */
    --shadow-sm: 0 1px 2px 0 rgb(0 0 0 / 0.05);
    --shadow-md: 0 4px 6px -1px rgb(0 0 0 / 0.1);
    
    /* Transitions */
    --transition-theme: background-color 200ms ease, color 200ms ease;
}

[data-theme="dark"] {
    --color-background: #0f172a;
    --color-surface: #1e293b;
    --color-surface-elevated: #334155;
    --color-text-primary: #f1f5f9;
    --color-text-secondary: #94a3b8;
    --color-text-tertiary: #64748b;
    --color-border: #334155;
    --color-accent: #60a5fa;
    --color-accent-hover: #93c5fd;
    
    --shadow-sm: 0 1px 2px 0 rgb(0 0 0 / 0.3);
    --shadow-md: 0 4px 6px -1px rgb(0 0 0 / 0.4);
}
```

### Responsive Breakpoints

```css
/* app.css */

/* Base (mobile-first) */
.container {
    padding: 1rem;
    max-width: 100%;
}

/* Tablet */
@media (min-width: 640px) {
    .container {
        padding: 1.5rem;
    }
    
    .forecast-grid {
        grid-template-columns: repeat(2, 1fr);
    }
}

/* Desktop */
@media (min-width: 768px) {
    .container {
        padding: 2rem;
        max-width: 1024px;
        margin: 0 auto;
    }
    
    .forecast-grid {
        grid-template-columns: repeat(3, 1fr);
    }
}

/* Wide */
@media (min-width: 1024px) {
    .container {
        max-width: 1280px;
    }
}
```

### Touch-Friendly Targets

```css
/* Minimum 44x44px touch targets */
button,
.clickable,
input[type="checkbox"],
input[type="radio"] {
    min-height: 44px;
    min-width: 44px;
}

/* Comfortable tap spacing */
.button-group > * + * {
    margin-left: 0.5rem;
}
```

---

## Unit Testing Patterns

### Service Tests with Moq

```csharp
namespace StargazeWeatherConditions.Tests.Services;

public class WeatherApiServiceTests
{
    private readonly Mock<HttpMessageHandler> _mockHandler;
    private readonly HttpClient _httpClient;
    private readonly Mock<ICacheService> _mockCacheService;
    private readonly WeatherApiService _service;

    public WeatherApiServiceTests()
    {
        _mockHandler = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_mockHandler.Object)
        {
            BaseAddress = new Uri("https://api.weatherapi.com/v1/")
        };
        _mockCacheService = new Mock<ICacheService>();
        _service = new WeatherApiService(_httpClient, _mockCacheService.Object);
    }

    [Fact]
    public async Task GetForecastAsync_WhenApiSucceeds_ReturnsParsedForecast()
    {
        // Arrange
        var responseJson = """
        {
            "location": { "name": "Denver", "lat": 39.74, "lon": -104.99 },
            "forecast": { "forecastday": [] }
        }
        """;
        
        _mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
            });

        // Act
        var result = await _service.GetForecastAsync("Denver");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Denver", result.Location.Name);
    }

    [Fact]
    public async Task GetForecastAsync_WhenApiFailsWithCache_ReturnsCachedData()
    {
        // Arrange
        var cachedForecast = new WeatherForecast { /* ... */ };
        
        _mockCacheService
            .Setup(c => c.GetAsync<WeatherForecast>(It.IsAny<string>()))
            .ReturnsAsync(cachedForecast);
        
        _mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException());

        // Act
        var result = await _service.GetForecastAsync("Denver");

        // Assert
        Assert.NotNull(result);
        Assert.Same(cachedForecast, result);
    }
}
```

### Utility Tests

```csharp
namespace StargazeWeatherConditions.Tests.Utilities;

public class RecommendationScorerTests
{
    private readonly RecommendationScorer _scorer = new();

    [Theory]
    [InlineData(0, 100)]
    [InlineData(20, 100)]
    [InlineData(21, 75)]
    [InlineData(60, 50)]
    [InlineData(100, 0)]
    public void ScoreCloudCover_ReturnsExpectedScore(int cloudPercent, double expectedScore)
    {
        // Arrange
        var conditions = CreateConditionsWithCloud(cloudPercent);
        var astronomy = CreateDefaultAstronomy();

        // Act
        var result = _scorer.CalculateRecommendation(conditions, astronomy, null);

        // Assert
        var cloudFactor = result.FactorScores.First(f => f.FactorName == "Cloud Cover");
        Assert.Equal(expectedScore, cloudFactor.Score);
    }

    [Theory]
    [InlineData(85, Rating.Excellent)]
    [InlineData(70, Rating.Good)]
    [InlineData(50, Rating.Fair)]
    [InlineData(30, Rating.Poor)]
    public void GetRating_ReturnsCorrectRating(double score, Rating expectedRating)
    {
        // Test rating thresholds
    }
}
```

### bUnit Component Tests

```csharp
namespace StargazeWeatherConditions.Tests.Components;

public class ThemeToggleTests : TestContext
{
    [Fact]
    public void ThemeToggle_WhenClicked_TogglesTheme()
    {
        // Arrange
        var preferences = new UserPreferences { IsDarkTheme = false };
        var preferencesChanged = false;
        
        var cut = RenderComponent<ThemeToggle>(parameters => parameters
            .Add(p => p.Preferences, preferences)
            .Add(p => p.PreferencesChanged, EventCallback.Factory.Create<UserPreferences>(
                this, p => preferencesChanged = true)));

        // Act
        cut.Find("button").Click();

        // Assert
        Assert.True(preferencesChanged);
    }

    [Fact]
    public void ThemeToggle_WhenDarkMode_ShowsSunIcon()
    {
        // Arrange
        var preferences = new UserPreferences { IsDarkTheme = true };
        
        var cut = RenderComponent<ThemeToggle>(parameters => parameters
            .Add(p => p.Preferences, preferences));

        // Assert
        Assert.Contains("sun", cut.Find("button").InnerHtml);
    }
}
```

---

## API Key Handling

### Configuration

```json
// appsettings.json (template, actual key injected by CI/CD)
{
  "WeatherApi": {
    "ApiKey": "WEATHERAPI_KEY_PLACEHOLDER",
    "BaseUrl": "https://api.weatherapi.com/v1/"
  },
  "LightPollutionApi": {
    "BaseUrl": "https://www.lightpollutionmap.info/..."
  }
}
```

### Service Implementation

```csharp
public class WeatherApiService(
    HttpClient httpClient, 
    ICacheService cacheService,
    IPreferencesService preferencesService,
    IConfiguration configuration) : IWeatherApiService
{
    private readonly string _defaultApiKey = configuration["WeatherApi:ApiKey"] 
        ?? throw new InvalidOperationException("WeatherAPI key not configured");

    private async Task<string> GetApiKeyAsync()
    {
        var customKey = await preferencesService.GetCustomApiKeyAsync();
        return string.IsNullOrWhiteSpace(customKey) ? _defaultApiKey : customKey;
    }

    public async Task<WeatherForecast?> GetForecastAsync(
        string location, 
        CancellationToken cancellationToken = default)
    {
        var apiKey = await GetApiKeyAsync();
        var url = $"forecast.json?key={apiKey}&q={Uri.EscapeDataString(location)}&days=3&aqi=no";
        
        // ... implementation
    }
}
```

---

## Common Patterns to Follow

### DO

- ✅ Use `required` and `init` for immutable model properties
- ✅ Use primary constructors for DI
- ✅ Use pattern matching and switch expressions
- ✅ Include CancellationToken on async methods
- ✅ Use interface-based service design
- ✅ Write comprehensive unit tests (80%+ coverage)
- ✅ Follow mobile-first responsive design
- ✅ Use CSS custom properties for theming
- ✅ Handle null cases explicitly
- ✅ Cache API responses in localStorage
- ✅ Provide graceful degradation on errors

### DON'T

- ❌ Use mutable properties on models
- ❌ Skip error handling for API calls
- ❌ Hard-code API keys in source code
- ❌ Ignore CancellationToken propagation
- ❌ Create components without tests
- ❌ Use fixed pixel widths (prefer relative/responsive)
- ❌ Block UI during async operations without feedback
- ❌ Store sensitive data in unencrypted localStorage
- ❌ Make unnecessary API calls (check cache first)
- ❌ Ignore accessibility requirements

---

## Quick Reference

### Essential Commands

```bash
# Build
dotnet build

# Run locally
dotnet run --project src/StargazeWeatherConditions

# Run tests
dotnet test

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Publish for GitHub Pages
dotnet publish -c Release -p:RunAOTCompilation=true
```

### Key Files

| File | Purpose |
|------|---------|
| `Program.cs` | DI configuration, HTTP client setup |
| `wwwroot/appsettings.json` | API configuration (key injected at build) |
| `wwwroot/css/themes.css` | Theme CSS variables |
| `.github/workflows/deploy.yml` | CI/CD pipeline |

---

**End of Copilot Instructions**
