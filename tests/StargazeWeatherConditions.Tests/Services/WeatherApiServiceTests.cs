using System.Net;
using System.Text;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using StargazeWeatherConditions.Services;

namespace StargazeWeatherConditions.Tests.Services;

public class WeatherApiServiceTests
{
    private readonly Mock<HttpMessageHandler> _mockHandler;
    private readonly HttpClient _httpClient;
    private readonly Mock<ICacheService> _mockCacheService;
    private readonly Mock<IPreferencesService> _mockPreferencesService;

    public WeatherApiServiceTests()
    {
        _mockHandler = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_mockHandler.Object)
        {
            BaseAddress = new Uri("https://api.weatherapi.com/v1/")
        };
        _mockCacheService = new Mock<ICacheService>();
        _mockPreferencesService = new Mock<IPreferencesService>();

        // Default: no custom API key
        _mockPreferencesService.Setup(p => p.GetCustomApiKeyAsync())
            .ReturnsAsync((string?)null);
    }

    [Fact]
    public async Task GetForecastAsync_WhenApiSucceeds_ReturnsParsedForecast()
    {
        // Arrange
        var responseJson = GetSampleForecastResponse();

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

        var service = CreateService();

        // Act
        var result = await service.GetForecastAsync("Denver");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Denver", result.Location.Name);
        Assert.NotEmpty(result.ForecastDays);
    }

    [Fact]
    public async Task GetForecastAsync_IncludesApiKeyInRequest()
    {
        // Arrange
        var responseJson = GetSampleForecastResponse();
        HttpRequestMessage? capturedRequest = null;

        _mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, _) => capturedRequest = req)
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
            });

        var service = CreateService();

        // Act
        await service.GetForecastAsync("Denver");

        // Assert
        Assert.NotNull(capturedRequest);
        Assert.Contains("key=", capturedRequest.RequestUri!.Query);
    }

    [Fact]
    public async Task GetForecastAsync_UsesCustomApiKeyWhenAvailable()
    {
        // Arrange
        var customKey = "custom_api_key_123";
        _mockPreferencesService.Setup(p => p.GetCustomApiKeyAsync())
            .ReturnsAsync(customKey);

        HttpRequestMessage? capturedRequest = null;
        var responseJson = GetSampleForecastResponse();

        _mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, _) => capturedRequest = req)
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
            });

        var service = CreateService();

        // Act
        await service.GetForecastAsync("Denver");

        // Assert
        Assert.NotNull(capturedRequest);
        Assert.Contains($"key={customKey}", capturedRequest.RequestUri!.Query);
    }

    [Fact]
    public async Task GetForecastAsync_WhenApiReturns401_ThrowsHttpRequestException()
    {
        // Arrange
        _mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Content = new StringContent("{\"error\":{\"message\":\"API key invalid\"}}")
            });

        var service = CreateService();

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => service.GetForecastAsync("Denver"));
    }

    [Fact]
    public async Task GetForecastAsync_WhenApiReturns404_ReturnsNull()
    {
        // Arrange
        _mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest,
                Content = new StringContent("{\"error\":{\"code\":1006,\"message\":\"No matching location found\"}}")
            });

        var service = CreateService();

        // Act
        var result = await service.GetForecastAsync("InvalidLocation12345");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task SearchLocationsAsync_WhenApiSucceeds_ReturnsLocationResults()
    {
        // Arrange
        var responseJson = """
        [
            {"id":1,"name":"Denver","region":"Colorado","country":"USA","lat":39.74,"lon":-104.99},
            {"id":2,"name":"Denver City","region":"Texas","country":"USA","lat":32.96,"lon":-102.83}
        ]
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

        var service = CreateService();

        // Act
        var results = await service.SearchLocationsAsync("Denver");

        // Assert
        Assert.NotNull(results);
        Assert.Equal(2, results.Count);
        Assert.Equal("Denver", results[0].Name);
        Assert.Equal("Colorado", results[0].Region);
    }

    [Fact]
    public async Task SearchLocationsAsync_WhenNoResults_ReturnsEmptyList()
    {
        // Arrange
        var responseJson = "[]";

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

        var service = CreateService();

        // Act
        var results = await service.SearchLocationsAsync("NonexistentPlace12345");

        // Assert
        Assert.NotNull(results);
        Assert.Empty(results);
    }

    [Fact]
    public async Task SearchLocationsAsync_EncodesQueryProperly()
    {
        // Arrange
        HttpRequestMessage? capturedRequest = null;

        _mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, _) => capturedRequest = req)
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("[]", Encoding.UTF8, "application/json")
            });

        var service = CreateService();

        // Act
        await service.SearchLocationsAsync("New York");

        // Assert
        Assert.NotNull(capturedRequest);
        Assert.Contains("q=New%20York", capturedRequest.RequestUri!.Query);
    }

    private WeatherApiService CreateService()
    {
        var configuration = new Mock<IConfiguration>();
        configuration.Setup(c => c["WeatherApi:ApiKey"]).Returns("test_api_key");
        
        return new WeatherApiService(
            _httpClient,
            _mockPreferencesService.Object,
            _mockCacheService.Object,
            configuration.Object);
    }

    private static string GetSampleForecastResponse()
    {
        return """
        {
            "location": {
                "name": "Denver",
                "region": "Colorado",
                "country": "United States of America",
                "lat": 39.74,
                "lon": -104.99,
                "tz_id": "America/Denver",
                "localtime": "2024-06-21 12:00"
            },
            "forecast": {
                "forecastday": [
                    {
                        "date": "2024-06-21",
                        "day": {
                            "maxtemp_c": 30.0,
                            "maxtemp_f": 86.0,
                            "mintemp_c": 15.0,
                            "mintemp_f": 59.0,
                            "avgtemp_c": 22.5,
                            "avgtemp_f": 72.5,
                            "condition": {
                                "text": "Sunny",
                                "icon": "//cdn.weatherapi.com/weather/64x64/day/113.png",
                                "code": 1000
                            }
                        },
                        "astro": {
                            "sunrise": "05:32 AM",
                            "sunset": "08:32 PM",
                            "moonrise": "09:45 PM",
                            "moonset": "06:30 AM",
                            "moon_phase": "Waxing Gibbous",
                            "moon_illumination": 85,
                            "is_moon_up": 0
                        },
                        "hour": [
                            {
                                "time": "2024-06-21 00:00",
                                "temp_c": 18.0,
                                "temp_f": 64.4,
                                "condition": {
                                    "text": "Clear",
                                    "icon": "//cdn.weatherapi.com/weather/64x64/night/113.png",
                                    "code": 1000
                                },
                                "wind_mph": 5.0,
                                "wind_kph": 8.0,
                                "wind_dir": "NW",
                                "humidity": 45,
                                "cloud": 10,
                                "vis_km": 16.0,
                                "vis_miles": 9.0,
                                "chance_of_rain": 0,
                                "is_day": 0
                            }
                        ]
                    }
                ]
            }
        }
        """;
    }
}
