using Microsoft.JSInterop;

namespace VotingSystem_Claude.Services;

public enum ThemeMode { Light, Dark, System }

public class ThemeService
{
    private readonly IJSRuntime _js;
    private ThemeMode _current = ThemeMode.System;
    private bool _initialized;

    public event Action? OnChanged;
    public ThemeMode Current => _current;

    public ThemeService(IJSRuntime js) => _js = js;

    public async Task InitializeAsync()
    {
        if (_initialized) return;
        try
        {
            var saved = await _js.InvokeAsync<string>("themeManager.getTheme");
            _current = Parse(saved);
            _initialized = true;
        }
        catch
        {
            // JS not available yet (pre-render) — default stays System
        }
    }

    public async Task SetAsync(ThemeMode mode)
    {
        _current = mode;
        try
        {
            await _js.InvokeVoidAsync("themeManager.setTheme", mode.ToString().ToLower());
        }
        catch { }
        OnChanged?.Invoke();
    }

    /// <summary>Cycles Light → Dark → System → Light</summary>
    public ThemeMode Next() => _current switch
    {
        ThemeMode.Light => ThemeMode.Dark,
        ThemeMode.Dark  => ThemeMode.System,
        _               => ThemeMode.Light
    };

    public string Icon => _current switch
    {
        ThemeMode.Light  => "bi bi-sun-fill",
        ThemeMode.Dark   => "bi bi-moon-fill",
        _                => "bi bi-circle-half"
    };

    public string Label => _current switch
    {
        ThemeMode.Light  => "Light",
        ThemeMode.Dark   => "Dark",
        _                => "System"
    };

    private static ThemeMode Parse(string? s) => s?.ToLower() switch
    {
        "light" => ThemeMode.Light,
        "dark"  => ThemeMode.Dark,
        _       => ThemeMode.System
    };
}
