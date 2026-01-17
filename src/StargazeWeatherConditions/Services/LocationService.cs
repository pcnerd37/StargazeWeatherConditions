using System.Text.Json;
using Microsoft.JSInterop;
using StargazeWeatherConditions.Models;

namespace StargazeWeatherConditions.Services;

/// <summary>
/// Implementation of location service using browser APIs.
/// </summary>
public class LocationService : ILocationService
{
    private readonly IJSRuntime _jsRuntime;
    private const string LastLocationKey = "last-location";

    public LocationService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task<(double Latitude, double Longitude)?> GetCurrentLocationAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _jsRuntime.InvokeAsync<GeolocationResult?>(
                "getGeolocation",
                cancellationToken);

            if (result is not null && result.Success && result.Latitude.HasValue && result.Longitude.HasValue)
            {
                return (result.Latitude.Value, result.Longitude.Value);
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    public async Task<LocationInfo?> GetLastLocationAsync()
    {
        try
        {
            var json = await _jsRuntime.InvokeAsync<string?>(
                "sessionStorage.getItem", 
                LastLocationKey);

            if (!string.IsNullOrEmpty(json))
            {
                return JsonSerializer.Deserialize<LocationInfo>(json);
            }
        }
        catch
        {
            // Ignore errors
        }

        return null;
    }

    public async Task SaveLastLocationAsync(LocationInfo location)
    {
        try
        {
            var json = JsonSerializer.Serialize(location);
            await _jsRuntime.InvokeVoidAsync(
                "sessionStorage.setItem", 
                LastLocationKey, 
                json);
        }
        catch
        {
            // Ignore errors
        }
    }

    public async Task<bool> IsGeolocationAvailableAsync()
    {
        try
        {
            return await _jsRuntime.InvokeAsync<bool>(
                "eval", 
                "'geolocation' in navigator");
        }
        catch
        {
            return false;
        }
    }

    public async Task<GeolocationResult> GetCurrentPositionAsync()
    {
        try
        {
            var result = await GetCurrentLocationAsync();
            if (result.HasValue)
            {
                return new GeolocationResult(true, result.Value.Latitude, result.Value.Longitude, null);
            }
            return new GeolocationResult(false, null, null, "Location not available");
        }
        catch (Exception ex)
        {
            return new GeolocationResult(false, null, null, ex.Message);
        }
    }
}
