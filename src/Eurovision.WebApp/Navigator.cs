using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;

namespace Eurovision.WebApp;

public class Navigator : IDisposable
{
    private const char URL_SEPARATOR = '/';
    private static readonly string[] BASE_URL = ["junior", "senior"];

    private readonly NavigationManager _navigationManager;
    private readonly IJSRuntime _jsRuntime;
    private readonly List<Path> _history;

    /// <summary>
    /// Returns true if it is possible to navigate to the previous url.
    /// </summary>
    public bool CanNavigateBack => _history.Count > 1;

    /// <summary>
    /// An event that fires when the navigation location has changed.
    /// </summary>
    public event EventHandler<LocationChangedEventArgs> LocationChanged;

    public Navigator(NavigationManager navigationManager, IJSRuntime jsRuntime)
    {
        _navigationManager = navigationManager;
        _jsRuntime = jsRuntime;
        _history = new List<Path>();
        _navigationManager.LocationChanged += OnLocationChanged;

        Init();
    }

    // Search in the initial url and add the previous pages to the history
    private void Init()
    {
        string currentUrl = _navigationManager.ToBaseRelativePath(_navigationManager.Uri);
        int separatorIndex = -1;

        while ((separatorIndex = currentUrl.IndexOf(URL_SEPARATOR, separatorIndex + 1)) != -1)
        {
            string url = currentUrl.Substring(0, separatorIndex);
            AddUrl(url, false, true);
        }

        AddUrl(currentUrl, true, true);
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
        if (!CanNavigateBack) return;

        Path lastPath = _history[^2];

        if (lastPath.CanBackBrowser)
        {
            await _jsRuntime.InvokeVoidAsync("history.back");
        }
        else
        {
            NavigateTo(lastPath.Url);
        }
    }

    private void OnLocationChanged(object sender, LocationChangedEventArgs e)
    {
        AddUrl(e.Location, true, false);
        LocationChanged?.Invoke(this, e);
    }

    private void AddUrl(string url, bool canBrowserBack, bool isRelative)
    {
        string relativeUrl = isRelative ? url : _navigationManager.ToBaseRelativePath(url);

        if (string.IsNullOrEmpty(relativeUrl) || BASE_URL.Contains(relativeUrl))
        {
            _history.Clear();
        }

        int relativeIndex = _history.FindIndex(path => path.Url == relativeUrl);

        if (relativeIndex != -1)
        {
            _history.RemoveRange(relativeIndex, _history.Count - relativeIndex - 1);
        }
        else
        {
            _history.Add(new Path(relativeUrl, canBrowserBack));
        }
    }

    public void Dispose()
    {
        _navigationManager.LocationChanged -= OnLocationChanged;
    }

    private record Path(string Url, bool CanBackBrowser);
}
