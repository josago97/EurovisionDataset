using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;

namespace Eurovision.WebApp;

public class Navigator : IDisposable
{
    private static readonly string[] BASE_URL = ["junior", "senior" ];

    private readonly NavigationManager _navigationManager;
    private readonly IJSRuntime _jsRuntime;
    private readonly List<string> _history;

    /// <summary>
    /// Returns true if it is possible to navigate to the previous url.
    /// </summary>
    public bool CanNavigateBack => _history.Count > 1;

    /// <summary>
    /// Returns the previous url.
    /// </summary>
    public string LastUrl => CanNavigateBack ? _history[^2] : string.Empty;

    /// <summary>
    /// An event that fires when the navigation location has changed.
    /// </summary>
    public event EventHandler<LocationChangedEventArgs> LocationChanged;

    public Navigator(NavigationManager navigationManager, IJSRuntime jsRuntime)
    {
        _navigationManager = navigationManager;
        _jsRuntime = jsRuntime;
        _history = new List<string>();
        AddUrl(_navigationManager.Uri);
        _navigationManager.LocationChanged += OnLocationChanged;
    }

    /// <summary>
    /// Navigates to the specified url.
    /// </summary>
    /// <param name="url">The destination url (relative or absolute).</param>
    public void NavigateTo(string url, bool forceLoad = false, bool replace = false)
    {
        _navigationManager.NavigateTo(url, forceLoad, replace);
    }

    /// <summary>
    /// Navigates to the previous url if possible or does nothing if it is not.
    /// </summary>
    public async Task GoBackAsync()
    {
        await _jsRuntime.InvokeVoidAsync("history.back");
    }

    private void OnLocationChanged(object sender, LocationChangedEventArgs e)
    {
        AddUrl(e.Location);
        LocationChanged?.Invoke(this, e);
    }

    private void AddUrl(string absoluteUrl)
    {
        string relative = _navigationManager.ToBaseRelativePath(absoluteUrl);

        if (BASE_URL.Contains(relative))
        {
            _history.Clear();
        }
        else
        {
            int relativeIndex = _history.IndexOf(relative);
            if (relativeIndex != -1)
            {
                _history.RemoveRange(relativeIndex, _history.Count - relativeIndex);
            }
        }

        _history.Add(relative);
    }

    public void Dispose()
    {
        _navigationManager.LocationChanged -= OnLocationChanged;
    }
}
