# Product Requirements Document (PRD)
# StarGaze Weather Conditions

**Version:** 1.0  
**Date:** January 16, 2026  
**Author:** Development Team  
**Status:** Draft

---

## Table of Contents

1. [Executive Summary](#executive-summary)
2. [Project Vision & Goals](#project-vision--goals)
3. [User Personas](#user-personas)
4. [Feature Specifications](#feature-specifications)
5. [Technical Architecture](#technical-architecture)
6. [API Integrations](#api-integrations)
7. [Caching & Offline Strategy](#caching--offline-strategy)
8. [Recommendation Engine](#recommendation-engine)
9. [User Interface & Design](#user-interface--design)
10. [Unit Testing Requirements](#unit-testing-requirements)
11. [Deployment Strategy](#deployment-strategy)
12. [Success Metrics](#success-metrics)
13. [Future Considerations](#future-considerations)

---

## Executive Summary

StarGaze Weather Conditions is a web application designed to help amateur astronomers, astrophotographers, and casual stargazers determine optimal conditions for nighttime sky observation. The application provides detailed weather forecasts focused on conditions that matter most for stargazing, including cloud cover, humidity, moon phase/illumination, visibility, light pollution, and astronomical twilight times.

The application will be built using **.NET 10 Blazor WebAssembly** and hosted on **GitHub Pages**, ensuring zero ongoing hosting costs. Weather data will be sourced from **WeatherAPI.com** (free tier with 1 million calls/month), and light pollution data from the **Light Pollution Map API**.

---

## Project Vision & Goals

### Vision
To provide stargazers and astrophotographers with a simple, beautiful, and accurate tool to plan their observation sessions by presenting weather and astronomical conditions in an easy-to-understand format with actionable recommendations.

### Primary Goals

1. **Accessibility**: Free to use, no account required, works on any modern browser
2. **Accuracy**: Provide reliable 3-day/3-night forecasts with hourly granularity
3. **Actionable Insights**: Generate clear recommendations (Excellent/Good/Fair/Poor) based on multiple conditions
4. **User Experience**: Responsive design that works seamlessly on mobile and desktop
5. **Cost Efficiency**: Zero hosting costs using GitHub Pages, free-tier APIs
6. **Reliability**: Client-side caching to minimize API calls and handle service interruptions gracefully

### Non-Goals (Initial Version)

- User accounts or saved locations history
- Push notifications or alerts
- Historical weather data analysis
- Equipment recommendations
- Social/community features
- Analytics or telemetry

---

## User Personas

### 1. Amateur Stargazer - "Sarah"

**Background**: Casual observer who enjoys looking at the night sky with naked eye or basic binoculars  
**Goals**: Find clear nights to observe meteor showers, bright planets, or the moon  
**Pain Points**: Doesn't know which weather conditions matter for stargazing  
**Needs**: Simple "good/bad" recommendations, easy-to-use interface

### 2. Astrophotographer - "Marcus"

**Background**: Serious hobbyist who photographs deep-sky objects with telescope and camera  
**Goals**: Plan multi-hour imaging sessions requiring excellent conditions  
**Pain Points**: Needs detailed data on moon illumination, humidity (affects dew), transparency  
**Needs**: Detailed hourly breakdowns, twilight calculations, moon-aware recommendations

### 3. Casual Night Sky Observer - "Alex"

**Background**: Someone curious about astronomy who occasionally wants to see notable celestial events  
**Goals**: Check conditions before heading out to observe an event they heard about  
**Pain Points**: Unsure if conditions are "good enough" to bother going outside  
**Needs**: Quick overview, clear yes/no guidance, mobile-friendly interface

---

## Feature Specifications

### 4.1 Location Services

#### Browser Geolocation
- Request geolocation permission on user action (click "Use My Location" button)
- Handle permission states: granted, denied, unavailable
- Display clear messaging for each state
- Fall back gracefully to manual entry if geolocation unavailable

#### Manual Location Search
- Text input field with autocomplete/typeahead functionality
- Debounced API calls (300ms delay) to WeatherAPI Search endpoint
- Support multiple input formats:
  - City name (e.g., "Denver, Colorado")
  - Coordinates (e.g., "39.7392, -104.9903")
  - ZIP/Postal codes (e.g., "80202")
- Display dropdown with matching locations showing city, region, country
- Persist last successful location to sessionStorage for page refresh recovery

### 4.2 Weather Forecast Display

#### 3-Day/3-Night Forecast
- Retrieve and display 3-day forecast from WeatherAPI (free tier limit)
- Focus on nighttime hours (sunset to sunrise) for each day
- Display hourly conditions for each night

#### Hourly Weather Metrics
For each hour during nighttime periods, display:

| Metric | Source | Display Format |
|--------|--------|----------------|
| Cloud Cover | WeatherAPI `cloud` | Percentage (0-100%) with visual indicator |
| Temperature | WeatherAPI `temp_c`/`temp_f` | Degrees with unit preference |
| Feels Like | WeatherAPI `feelslike_c`/`feelslike_f` | Degrees with unit preference |
| Humidity | WeatherAPI `humidity` | Percentage (0-100%) |
| Visibility | WeatherAPI `vis_km`/`vis_miles` | Distance with unit preference |
| Wind Speed | WeatherAPI `wind_mph`/`wind_kph` | Speed with direction |
| Precipitation Chance | WeatherAPI `chance_of_rain` | Percentage |
| Weather Condition | WeatherAPI `condition` | Icon (from WeatherAPI CDN) + text |

### 4.3 Astronomical Data Display

#### Moon Information
- Moon phase (New Moon, Waxing Crescent, First Quarter, etc.)
- Moon illumination percentage (0-100%)
- Moonrise and moonset times
- Moon altitude/visibility indicator (`is_moon_up`)

#### Sun & Twilight Information
- Sunset and sunrise times
- **Calculated twilight times**:
  - Civil twilight (sun -6° below horizon)
  - Nautical twilight (sun -12° below horizon)
  - Astronomical twilight (sun -18° below horizon)
- Optimal stargazing window (between evening and morning astronomical twilight)

#### Visual Timeline Component
Display a graphical timeline for each night showing:
- Sunset time
- Civil, nautical, and astronomical twilight transitions
- Moonrise/moonset markers
- Highlighted "optimal viewing window" period
- Current time indicator (if viewing tonight)

### 4.4 Light Pollution Data

#### Integration with Light Pollution Map API
- Query light pollution data based on location coordinates
- Display Bortle scale rating (1-9)
- Provide interpretation:
  - Bortle 1-3: Excellent (dark sky site)
  - Bortle 4-5: Moderate (suburban/rural transition)
  - Bortle 6-7: Poor (suburban)
  - Bortle 8-9: Very Poor (urban/city center)

#### Graceful Degradation
- If Light Pollution API unavailable, display "Light pollution data unavailable"
- Do not block other functionality
- Exclude from recommendation calculation if unavailable

### 4.5 Recommendation Engine

#### Overall Condition Rating
Generate ratings for each night:
- **Excellent**: Score ≥ 85%
- **Good**: Score 70-84%
- **Fair**: Score 50-69%
- **Poor**: Score < 50%

#### Contextual Recommendations
Provide specific guidance based on conditions:
- "Excellent for deep sky imaging - new moon, clear skies, dark site"
- "Good for planetary observation - bright moon, but clear skies"
- "Fair conditions - some clouds expected, consider planetary/lunar targets"
- "Poor conditions - high cloud cover, consider rescheduling"

#### Observation Type Suitability
Display separate scores for:
- **Deep Sky Objects (DSO)**: Galaxies, nebulae, star clusters - requires dark skies, low moon
- **Planetary/Lunar**: Planets, moon features - tolerates bright moon, needs steady seeing

### 4.6 User Preferences

#### Theme Toggle
- Dark mode / Light mode switch
- Default to system preference (`prefers-color-scheme`)
- Smooth transition animation (200ms)
- Persist preference to localStorage

#### Unit Preferences
- **Temperature**: Fahrenheit (°F) / Celsius (°C)
- **Distance/Visibility**: Miles / Kilometers
- **Wind Speed**: mph / kph
- **Time Format**: 12-hour / 24-hour
- Persist all preferences to localStorage

#### Settings Panel
- Accessible from header/navigation
- Custom API key input field (optional)
- Validation of custom API keys before saving
- Clear indicator showing default vs. custom key status

---

## Technical Architecture

### 5.1 Technology Stack

| Component | Technology |
|-----------|------------|
| Framework | .NET 10 Blazor WebAssembly |
| Hosting | GitHub Pages (static files) |
| Language | C# 13 |
| CSS | Standard CSS with custom properties (CSS variables) |
| HTTP Client | HttpClient with IHttpClientFactory |
| Resilience | Polly for retry policies |
| Storage | Browser localStorage / sessionStorage |
| Build/Deploy | GitHub Actions |

### 5.2 Project Structure

```
StargazeWeatherConditions/
├── .github/
│   ├── copilot-instructions.md
│   └── workflows/
│       └── deploy.yml
├── src/
│   └── StargazeWeatherConditions/
│       ├── Components/
│       │   ├── Layout/
│       │   │   ├── MainLayout.razor
│       │   │   ├── NavBar.razor
│       │   │   └── Footer.razor
│       │   ├── Weather/
│       │   │   ├── HourlyForecastCard.razor
│       │   │   ├── NightForecastPanel.razor
│       │   │   └── WeatherConditionIcon.razor
│       │   ├── Astronomy/
│       │   │   ├── MoonPhaseDisplay.razor
│       │   │   ├── TwilightTimeline.razor
│       │   │   └── SunMoonTimes.razor
│       │   ├── Recommendations/
│       │   │   ├── ConditionScore.razor
│       │   │   └── RecommendationCard.razor
│       │   ├── Location/
│       │   │   ├── LocationSearch.razor
│       │   │   └── GeolocationButton.razor
│       │   └── Settings/
│       │       ├── ThemeToggle.razor
│       │       ├── UnitPreferences.razor
│       │       └── ApiKeySettings.razor
│       ├── Pages/
│       │   ├── Index.razor
│       │   └── Settings.razor
│       ├── Services/
│       │   ├── IWeatherApiService.cs
│       │   ├── WeatherApiService.cs
│       │   ├── ILightPollutionService.cs
│       │   ├── LightPollutionService.cs
│       │   ├── ILocationService.cs
│       │   ├── LocationService.cs
│       │   ├── ICacheService.cs
│       │   ├── CacheService.cs
│       │   ├── IPreferencesService.cs
│       │   ├── PreferencesService.cs
│       │   └── IRecommendationService.cs
│       │   └── RecommendationService.cs
│       ├── Models/
│       │   ├── WeatherForecast.cs
│       │   ├── HourlyCondition.cs
│       │   ├── AstronomyData.cs
│       │   ├── TwilightTimes.cs
│       │   ├── LightPollutionData.cs
│       │   ├── LocationInfo.cs
│       │   ├── UserPreferences.cs
│       │   └── Recommendation.cs
│       ├── Utilities/
│       │   ├── TwilightCalculator.cs
│       │   ├── RecommendationScorer.cs
│       │   ├── UnitConverter.cs
│       │   └── CoordinateValidator.cs
│       ├── wwwroot/
│       │   ├── css/
│       │   │   ├── app.css
│       │   │   └── themes.css
│       │   ├── index.html
│       │   └── appsettings.json
│       ├── Program.cs
│       └── StargazeWeatherConditions.csproj
└── tests/
    └── StargazeWeatherConditions.Tests/
        ├── Services/
        ├── Utilities/
        ├── Components/
        └── StargazeWeatherConditions.Tests.csproj
```

### 5.3 Dependency Injection Configuration

```csharp
// Program.cs
builder.Services.AddScoped<IWeatherApiService, WeatherApiService>();
builder.Services.AddScoped<ILightPollutionService, LightPollutionService>();
builder.Services.AddScoped<ILocationService, LocationService>();
builder.Services.AddScoped<ICacheService, CacheService>();
builder.Services.AddScoped<IPreferencesService, PreferencesService>();
builder.Services.AddScoped<IRecommendationService, RecommendationService>();

// HttpClient with Polly
builder.Services.AddHttpClient("WeatherApi", client =>
{
    client.BaseAddress = new Uri("https://api.weatherapi.com/v1/");
})
.AddTransientHttpErrorPolicy(policy => 
    policy.WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt))));
```

---

## API Integrations

### 6.1 WeatherAPI.com

#### API Overview
- **Base URL**: `https://api.weatherapi.com/v1/`
- **Authentication**: API key as query parameter
- **Free Tier**: 1,000,000 calls/month, 3-day forecast
- **Documentation**: https://www.weatherapi.com/docs/

#### Endpoints Used

##### Forecast API
```
GET /forecast.json?key={API_KEY}&q={location}&days=3&aqi=no
```

**Response includes:**
- `location`: Name, region, country, coordinates, timezone
- `current`: Current conditions (optional, may not be needed)
- `forecast.forecastday[]`: Array of daily forecasts
  - `date`: Forecast date
  - `day`: Daily summary (max/min temp, conditions)
  - `astro`: Astronomy data (sunrise, sunset, moonrise, moonset, moon_phase, moon_illumination)
  - `hour[]`: 24 hourly forecasts with all weather metrics

##### Search/Autocomplete API
```
GET /search.json?key={API_KEY}&q={query}
```

**Response includes:**
- Array of matching locations with id, name, region, country, lat, lon

##### Astronomy API
```
GET /astronomy.json?key={API_KEY}&q={location}&dt={date}
```

**Response includes:**
- `astronomy.astro`: sunrise, sunset, moonrise, moonset, moon_phase, moon_illumination, is_moon_up, is_sun_up

#### API Key Management

1. **Shared Key (Default)**
   - Stored in GitHub Secrets as `WEATHERAPI_KEY`
   - Injected into `appsettings.json` during GitHub Actions build
   - Bundled with published Blazor WASM app
   - Note: Client-side keys are visible in network traffic (acceptable for free tier)

2. **Custom Key (User Override)**
   - User can input their own API key in Settings
   - Stored in browser localStorage under key `custom-api-key`
   - Takes precedence over default key when present
   - Validate key with test API call before saving
   - Display indicator showing which key is active

### 6.2 Light Pollution Map API

#### API Overview
- **Purpose**: Retrieve light pollution / Bortle scale rating for coordinates
- **Integration**: Query by latitude/longitude
- **Fallback**: Gracefully handle unavailability

#### Usage
```
GET endpoint with lat/lon parameters
Response: Bortle scale value or equivalent light pollution index
```

#### Error Handling
- If API fails or times out: Display "Light pollution data unavailable"
- Exclude from recommendation calculation
- Do not block main functionality

---

## Caching & Offline Strategy

### 7.1 Cache Implementation

#### Storage Mechanism
- Use browser `localStorage` for forecast data caching
- Use `sessionStorage` for current session data (last location)

#### Cache Key Structure
```
forecast_{lat}_{lon}_{date}
```
Example: `forecast_39.7392_-104.9903_2026-01-16`

#### Cache Entry Structure
```json
{
  "data": { /* forecast response */ },
  "timestamp": "2026-01-16T18:30:00Z",
  "expiresAt": "2026-01-16T20:30:00Z"
}
```

#### Time-To-Live (TTL)
- Forecast data: **2 hours**
- Light pollution data: **24 hours** (changes infrequently)
- User preferences: **No expiration**

### 7.2 Cache Retrieval Logic

```
1. Check if cache exists for location + current date
2. If exists and not expired (within TTL):
   - Return cached data
3. If exists but expired:
   - Attempt fresh API call
   - If successful: Update cache, return fresh data
   - If failed: Return cached data with staleness warning
4. If no cache:
   - Attempt API call
   - If successful: Store in cache, return data
   - If failed: Show error message
```

### 7.3 Error Handling & User Messaging

#### API Failure with Valid Same-Day Cache
Display banner:
> ⚠️ **Data may be outdated** (last updated: 6:30 PM)  
> Weather service temporarily unavailable. Showing cached data.

#### API Failure with No Cache or Different-Day Cache
Display error:
> ❌ **Weather service currently unavailable**  
> Please try again later.  
> [Retry Button]

#### Manual Refresh
- Provide "Refresh" button in UI
- Bypasses cache and forces new API call
- Updates cache on success

---

## Recommendation Engine

### 8.1 Scoring Algorithm

#### Weight Distribution

| Factor | Weight | Rationale |
|--------|--------|-----------|
| Cloud Cover | 35% | Most critical - clouds block all observation |
| Moon Illumination | 25% | Major factor for deep-sky observation |
| Humidity | 15% | Affects transparency and dew formation |
| Visibility | 15% | Indicates atmospheric clarity |
| Light Pollution | 10% | Location-based factor, less variable |

#### Scoring Criteria

##### Cloud Cover (35% weight)
| Condition | Score | Label |
|-----------|-------|-------|
| 0-20% | 100 | Excellent |
| 21-40% | 75 | Good |
| 41-60% | 50 | Fair |
| 61-80% | 25 | Poor |
| 81-100% | 0 | Very Poor |

##### Moon Illumination (25% weight)
| Condition | Score | Label | Notes |
|-----------|-------|-------|-------|
| 0-10% | 100 | Excellent | New moon, ideal for DSO |
| 11-30% | 85 | Very Good | Crescent, minimal interference |
| 31-50% | 60 | Moderate | Quarter moon |
| 51-70% | 35 | Fair | Gibbous moon |
| 71-100% | 10 | Poor for DSO | Full moon, good for lunar |

##### Humidity (15% weight)
| Condition | Score | Label |
|-----------|-------|-------|
| 0-50% | 100 | Excellent |
| 51-70% | 75 | Good |
| 71-80% | 50 | Fair |
| 81-90% | 25 | Poor |
| 91-100% | 0 | Very Poor |

##### Visibility (15% weight)
| Condition (km) | Score | Label |
|----------------|-------|-------|
| > 10 | 100 | Excellent |
| 7-10 | 75 | Good |
| 5-7 | 50 | Fair |
| 3-5 | 25 | Poor |
| < 3 | 0 | Very Poor |

##### Light Pollution - Bortle Scale (10% weight)
| Bortle | Score | Label |
|--------|-------|-------|
| 1-2 | 100 | Excellent |
| 3 | 85 | Very Good |
| 4 | 65 | Good |
| 5 | 45 | Fair |
| 6 | 25 | Poor |
| 7-9 | 10 | Very Poor |

### 8.2 Overall Score Calculation

```csharp
double overallScore = 
    (cloudScore * 0.35) +
    (moonScore * 0.25) +
    (humidityScore * 0.15) +
    (visibilityScore * 0.15) +
    (lightPollutionScore * 0.10);
```

### 8.3 Rating Thresholds

| Score Range | Rating | Color Code |
|-------------|--------|------------|
| 85-100 | Excellent | Green (#22c55e) |
| 70-84 | Good | Blue (#3b82f6) |
| 50-69 | Fair | Yellow (#eab308) |
| 0-49 | Poor | Red (#ef4444) |

### 8.4 Contextual Recommendations

Generate human-readable recommendations based on individual factors:

```
if (cloudScore >= 80 && moonScore >= 80)
    "Excellent for deep sky imaging - new moon, clear skies"
else if (cloudScore >= 80 && moonScore < 40)
    "Good for planetary/lunar observation - clear skies, bright moon"
else if (cloudScore < 50)
    "Poor conditions - high cloud cover expected"
else if (humidityScore < 30)
    "Caution: High humidity may cause dew formation on optics"
```

### 8.5 Observation Type Suitability

Calculate separate scores:

**Deep Sky Objects (DSO) Score:**
- Cloud Cover: 40%
- Moon Illumination: 35%
- Light Pollution: 15%
- Visibility: 10%

**Planetary/Lunar Score:**
- Cloud Cover: 50%
- Visibility: 30%
- Humidity: 20%
- (Moon illumination not penalized)

---

## User Interface & Design

### 9.1 Responsive Design Requirements

#### Breakpoints
| Name | Width | Typical Devices |
|------|-------|-----------------|
| Mobile | < 640px | Phones (portrait) |
| Tablet | 640-767px | Phones (landscape), small tablets |
| Desktop | 768-1023px | Tablets, small laptops |
| Wide | ≥ 1024px | Laptops, desktops |

#### Mobile-First Approach
- Base styles target mobile devices
- Progressive enhancement for larger screens
- Touch-optimized controls (minimum 44×44px tap targets)

### 9.2 Layout Specifications

#### Header
- Logo/App name (left)
- Location display with edit button (center)
- Theme toggle and settings button (right)
- Sticky on scroll

#### Main Content - Mobile
- Single column layout
- Collapsible accordion sections
- Horizontal scroll for hourly forecast cards with snap scrolling
- Stacked night panels (Tonight, Tomorrow Night, Night 3)

#### Main Content - Desktop
- Multi-column grid layout
- Side-by-side comparison of nights
- Expanded hourly forecast grid
- Twilight timeline prominently displayed

#### Footer
- WeatherAPI attribution (required by terms)
- Light Pollution Map attribution
- App version

### 9.3 Theming

#### CSS Custom Properties (Variables)
```css
:root {
  /* Light Theme (default) */
  --color-background: #ffffff;
  --color-surface: #f3f4f6;
  --color-text-primary: #111827;
  --color-text-secondary: #6b7280;
  --color-accent: #3b82f6;
  --color-excellent: #22c55e;
  --color-good: #3b82f6;
  --color-fair: #eab308;
  --color-poor: #ef4444;
}

[data-theme="dark"] {
  --color-background: #111827;
  --color-surface: #1f2937;
  --color-text-primary: #f9fafb;
  --color-text-secondary: #9ca3af;
  --color-accent: #60a5fa;
}
```

#### Theme Toggle Behavior
1. On first visit: Check `prefers-color-scheme` media query
2. Apply system preference as default
3. User toggle overrides and persists to localStorage
4. Smooth transition (200ms) on theme change

#### Accessibility Requirements
- WCAG 2.1 AA compliance
- Minimum contrast ratio: 4.5:1 (normal text), 3:1 (large text)
- Both themes must meet contrast requirements
- Focus indicators visible in both themes

### 9.4 Key UI Components

#### Location Search
- Input field with search icon
- "Use My Location" button with location icon
- Dropdown autocomplete results
- Clear button when location selected

#### Hourly Forecast Card
- Hour time
- Weather icon (from WeatherAPI CDN)
- Temperature
- Cloud cover percentage with visual bar
- Key metrics summary

#### Twilight Timeline
- Horizontal timeline visualization
- Color-coded twilight phases
- Moon position indicators
- Optimal viewing window highlight
- Current time marker (if applicable)

#### Recommendation Card
- Large rating badge (Excellent/Good/Fair/Poor)
- Overall percentage score
- Individual factor breakdown
- Contextual recommendation text
- DSO vs. Planetary/Lunar suitability indicators

---

## Unit Testing Requirements

### 10.1 Testing Framework & Tools

| Purpose | Tool |
|---------|------|
| Test Framework | xUnit |
| Mocking | Moq |
| Blazor Components | bUnit |
| Assertions | FluentAssertions (optional) |
| Coverage | Coverlet |

### 10.2 Coverage Requirements

- **Minimum Overall Coverage**: 80%
- **Services Layer**: 90% coverage
- **Utilities Layer**: 95% coverage
- **Components**: 70% coverage (focus on logic, not markup)

### 10.3 Test Categories

#### Service Tests
| Service | Test Scenarios |
|---------|----------------|
| WeatherApiService | Success response parsing, API error handling, retry behavior, timeout handling |
| LightPollutionService | Success response, API unavailable graceful degradation |
| CacheService | Cache hit, cache miss, TTL expiration, different-day cache handling |
| PreferencesService | Load defaults, save preferences, theme toggle, unit changes |
| RecommendationService | Score calculation, boundary conditions, all rating thresholds |

#### Utility Tests
| Utility | Test Scenarios |
|---------|----------------|
| TwilightCalculator | Known location/date calculations, polar edge cases, accuracy validation |
| RecommendationScorer | Each factor scoring, weight calculations, edge cases |
| UnitConverter | Temperature conversion, distance conversion, speed conversion |
| CoordinateValidator | Valid coordinates, invalid inputs, boundary values |

#### Component Tests (bUnit)
| Component | Test Scenarios |
|-----------|----------------|
| LocationSearch | Input handling, autocomplete display, selection |
| ThemeToggle | Toggle behavior, persistence |
| HourlyForecastCard | Data binding, unit display |
| RecommendationCard | Rating display, color coding |

### 10.4 Test Patterns

#### Arrange-Act-Assert
```csharp
[Fact]
public void CalculateTwilight_WithValidCoordinates_ReturnsCorrectTimes()
{
    // Arrange
    var calculator = new TwilightCalculator();
    var latitude = 39.7392;
    var longitude = -104.9903;
    var date = new DateTime(2026, 1, 16);

    // Act
    var result = calculator.Calculate(latitude, longitude, date);

    // Assert
    Assert.NotNull(result);
    Assert.True(result.AstronomicalDusk < result.AstronomicalDawn);
}
```

#### Mocking External Dependencies
```csharp
[Fact]
public async Task GetForecast_WhenApiSucceeds_ReturnsParsedData()
{
    // Arrange
    var mockHttpClient = new Mock<HttpClient>();
    mockHttpClient.Setup(/* ... */);
    var service = new WeatherApiService(mockHttpClient.Object);

    // Act
    var result = await service.GetForecastAsync("Denver", CancellationToken.None);

    // Assert
    Assert.NotNull(result);
    Assert.Equal(3, result.ForecastDays.Count);
}
```

### 10.5 Test Execution in CI/CD

- Run tests on every push and pull request
- Fail pipeline if any tests fail
- Generate coverage report
- Block deployment if coverage drops below threshold

---

## Deployment Strategy

### 11.1 Hosting Platform

**GitHub Pages** (Free tier)
- Static file hosting
- Custom domain support (optional)
- HTTPS included
- Deployed from `gh-pages` branch

### 11.2 GitHub Actions Workflow

**File**: `.github/workflows/deploy.yml`

```yaml
name: Deploy to GitHub Pages

on:
  push:
    branches: [ main ]
  workflow_dispatch:

permissions:
  contents: read
  pages: write
  id-token: write

jobs:
  build-and-deploy:
    runs-on: ubuntu-latest
    
    steps:
    - name: Checkout
      uses: actions/checkout@v4

    - name: Setup .NET 10
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '10.0.x'

    - name: Restore dependencies
      run: dotnet restore

    - name: Build
      run: dotnet build --configuration Release --no-restore

    - name: Run tests
      run: dotnet test --no-build --configuration Release --verbosity normal

    - name: Inject API Key
      run: |
        sed -i 's/WEATHERAPI_KEY_PLACEHOLDER/${{ secrets.WEATHERAPI_KEY }}/g' \
          src/StargazeWeatherConditions/wwwroot/appsettings.json

    - name: Publish
      run: dotnet publish src/StargazeWeatherConditions/StargazeWeatherConditions.csproj \
        -c Release \
        -o release \
        -p:RunAOTCompilation=true

    - name: Configure base href
      run: |
        sed -i 's|<base href="/" />|<base href="/StargazeWeatherConditions/" />|g' \
          release/wwwroot/index.html

    - name: Add .nojekyll
      run: touch release/wwwroot/.nojekyll

    - name: Deploy to GitHub Pages
      uses: peaceiris/actions-gh-pages@v3
      with:
        github_token: ${{ secrets.GITHUB_TOKEN }}
        publish_dir: ./release/wwwroot
        force_orphan: true
```

### 11.3 Build Configuration

#### AOT Compilation
- Enable Ahead-of-Time compilation for better performance
- Increases build time but improves runtime performance
- Reduces download size with trimming

#### Base Href Configuration
- Must match GitHub Pages repository path
- Format: `/<repository-name>/`
- Updated during CI/CD pipeline

### 11.4 Environment Secrets

| Secret Name | Purpose |
|-------------|---------|
| `WEATHERAPI_KEY` | Shared WeatherAPI.com API key |
| `GITHUB_TOKEN` | Automatically provided, for Pages deployment |

### 11.5 Deployment Triggers

- **Automatic**: Push to `main` branch
- **Manual**: Workflow dispatch from GitHub Actions UI

---

## Success Metrics

### 12.1 Technical Metrics

| Metric | Target |
|--------|--------|
| Initial Load Time | < 3 seconds on 4G |
| Time to Interactive | < 5 seconds |
| Lighthouse Performance Score | > 80 |
| Unit Test Coverage | > 80% |
| Build Success Rate | > 99% |

### 12.2 Functional Metrics

| Metric | Target |
|--------|--------|
| API Call Success Rate | > 99% |
| Cache Hit Rate | > 60% (return visitors) |
| Error Rate | < 1% |

### 12.3 Quality Metrics

| Metric | Target |
|--------|--------|
| WCAG AA Compliance | 100% |
| Cross-browser Compatibility | Chrome, Firefox, Safari, Edge |
| Mobile Responsiveness | Functional 320px - 2560px |

---

## Future Considerations

The following features are explicitly **out of scope** for version 1.0 but may be considered for future releases:

### Potential Future Features

1. **Saved Locations**: Allow users to save favorite observation sites
2. **Weather Alerts**: Notify users of upcoming excellent conditions
3. **Historical Data**: Show past conditions for location comparison
4. **Equipment Integration**: Recommendations based on user's equipment
5. **Celestial Events**: Integration with astronomical event calendars
6. **Seeing Forecast**: Atmospheric seeing/turbulence predictions
7. **PWA Support**: Offline capability with service workers
8. **Multi-language Support**: Localization for international users
9. **Social Sharing**: Share conditions or observation plans
10. **Analytics**: Usage tracking for feature prioritization

### Technical Debt Considerations

1. Monitor WeatherAPI usage against 1M monthly limit
2. Evaluate need for backend proxy if API key exposure becomes problematic
3. Consider IndexedDB for larger cache storage needs
4. Evaluate performance impact of AOT compilation

---

## Appendix

### A. WeatherAPI Response Structure (Relevant Fields)

```json
{
  "location": {
    "name": "Denver",
    "region": "Colorado",
    "country": "USA",
    "lat": 39.74,
    "lon": -104.99,
    "tz_id": "America/Denver",
    "localtime": "2026-01-16 18:30"
  },
  "forecast": {
    "forecastday": [
      {
        "date": "2026-01-16",
        "astro": {
          "sunrise": "07:15 AM",
          "sunset": "05:02 PM",
          "moonrise": "03:45 PM",
          "moonset": "06:30 AM",
          "moon_phase": "Waning Gibbous",
          "moon_illumination": 68,
          "is_moon_up": 1,
          "is_sun_up": 0
        },
        "hour": [
          {
            "time": "2026-01-16 18:00",
            "temp_c": -2.5,
            "temp_f": 27.5,
            "humidity": 45,
            "cloud": 15,
            "vis_km": 16.0,
            "vis_miles": 10.0,
            "wind_mph": 8.5,
            "wind_kph": 13.7,
            "wind_dir": "NW",
            "chance_of_rain": 0,
            "feelslike_c": -7.2,
            "feelslike_f": 19.0,
            "condition": {
              "text": "Clear",
              "icon": "//cdn.weatherapi.com/weather/64x64/night/113.png",
              "code": 1000
            }
          }
        ]
      }
    ]
  }
}
```

### B. Twilight Calculation Reference

**Solar Depression Angles:**
- Civil Twilight: Sun 0° to -6° below horizon
- Nautical Twilight: Sun -6° to -12° below horizon
- Astronomical Twilight: Sun -12° to -18° below horizon
- Night: Sun more than -18° below horizon

**Optimal Stargazing**: Between end of evening astronomical twilight and start of morning astronomical twilight

### C. Bortle Scale Reference

| Class | Description | Naked-Eye Limiting Magnitude |
|-------|-------------|------------------------------|
| 1 | Excellent dark-sky site | 7.6-8.0 |
| 2 | Typical dark site | 7.1-7.5 |
| 3 | Rural sky | 6.6-7.0 |
| 4 | Rural/suburban transition | 6.1-6.5 |
| 5 | Suburban sky | 5.6-6.0 |
| 6 | Bright suburban sky | 5.1-5.5 |
| 7 | Suburban/urban transition | 4.6-5.0 |
| 8 | City sky | 4.1-4.5 |
| 9 | Inner-city sky | 4.0 or less |

---

**Document End**
