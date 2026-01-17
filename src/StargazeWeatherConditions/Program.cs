using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using StargazeWeatherConditions;
using StargazeWeatherConditions.Services;
using StargazeWeatherConditions.Utilities;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configure HttpClient for WeatherAPI
builder.Services.AddHttpClient<IWeatherApiService, WeatherApiService>(client =>
{
    client.BaseAddress = new Uri("https://api.weatherapi.com/v1/");
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Register services
builder.Services.AddScoped<ICacheService, CacheService>();
builder.Services.AddScoped<ILightPollutionService, LightPollutionService>();
builder.Services.AddScoped<IPreferencesService, PreferencesService>();
builder.Services.AddScoped<ILocationService, LocationService>();
builder.Services.AddSingleton<IRecommendationScorer, RecommendationScorer>();
builder.Services.AddSingleton<ITwilightCalculator, TwilightCalculator>();
builder.Services.AddScoped<IRecommendationService, RecommendationService>();

// Add configuration
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: false);

await builder.Build().RunAsync();
