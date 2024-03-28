using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

namespace Eurovision.WebApp.Views.Layout;

public partial class NavMenu
{
    private bool collapseNavMenu = true;

    [Inject]
    public Navigator Navigator { get; set; }
    private string GoBackButtonCssClass => !Navigator.CanNavigateBack ? "hide" : null;
    private string NavMenuCssClass => collapseNavMenu ? "collapse" : null;

    protected override void OnInitialized()
    {
        base.OnInitialized();

        Navigator.LocationChanged += OnNavigated; 
    }

    private void OnNavigated(object sender, LocationChangedEventArgs e)
    {
        StateHasChanged();
    }

    private void ToggleNavMenu()
    {
        collapseNavMenu = !collapseNavMenu;
    }
}