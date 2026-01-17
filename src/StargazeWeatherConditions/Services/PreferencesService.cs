using System.Text.Json;
using Microsoft.JSInterop;
using StargazeWeatherConditions.Models;

namespace StargazeWeatherConditions.Services;

/// <summary>
/// Implementation of preferences service using browser localStorage.
/// </summary>
public class PreferencesService : IPreferencesService
{
    private readonly IJSRuntime _jsRuntime;
    private const string PreferencesKey = "user-preferences";
    private const string CustomApiKeyKey = "custom-api-key";
    private UserPreferences? _cachedPreferences;

    public event EventHandler<UserPreferences>? PreferencesChanged;

    public PreferencesService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task<UserPreferences> GetPreferencesAsync()
    {
        if (_cachedPreferences is not null)
        {
            return _cachedPreferences;
        }

        try
        {
            var json = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", PreferencesKey);
            
            if (!string.IsNullOrEmpty(json))
            {
                _cachedPreferences = JsonSerializer.Deserialize<UserPreferences>(json);
            }
        }
        catch (Exception)
        {
            // Ignore errors, return default
        }

        _cachedPreferences ??= await GetDefaultPreferencesAsync();
        return _cachedPreferences;
    }

    public async Task SavePreferencesAsync(UserPreferences preferences)
    {
        try
        {
            var json = JsonSerializer.Serialize(preferences);
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", PreferencesKey, json);
            _cachedPreferences = preferences;
            
            // Update theme attribute on document
            var theme = preferences.IsDarkTheme ? "dark" : "light";
            await _jsRuntime.InvokeVoidAsync("eval", 
                $"document.documentElement.setAttribute('data-theme', '{theme}')");

            PreferencesChanged?.Invoke(this, preferences);
        }
        catch (Exception)
        {
            // Ignore errors
        }
    }

    public async Task<string?> GetCustomApiKeyAsync()
    {
        try
        {
            return await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", CustomApiKeyKey);
        }
        catch
        {
            return null;
        }
    }

    public async Task SetCustomApiKeyAsync(string? apiKey)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                await ClearCustomApiKeyAsync();
            }
            else
            {
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", CustomApiKeyKey, apiKey);
            }
        }
        catch
        {
            // Ignore errors
        }
    }

    public async Task ClearCustomApiKeyAsync()
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", CustomApiKeyKey);
        }
        catch
        {
            // Ignore errors
        }
    }

    public async Task<bool> HasCustomApiKeyAsync()
    {
        var key = await GetCustomApiKeyAsync();
        return !string.IsNullOrWhiteSpace(key);
    }

    private async Task<UserPreferences> GetDefaultPreferencesAsync()
    {
        // Check system preference for dark mode
        var prefersDark = true;
        try
        {
            prefersDark = await _jsRuntime.InvokeAsync<bool>(
                "eval", 
                "window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches");
        }
        catch
        {
            // Default to dark theme for stargazing app
        }

        return new UserPreferences
        {
            IsDarkTheme = prefersDark,
            UseMetricUnits = false,
            Use24HourTime = false
        };
    }
}
