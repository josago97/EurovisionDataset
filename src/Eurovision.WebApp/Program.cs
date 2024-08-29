using System.Globalization;
using BlazorPro.BlazorSize;
using Eurovision.WebApp.Models;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace Eurovision.WebApp;

public class Program
{
    public static async Task Main(string[] args)
    {
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;

        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        builder.RootComponents.Add<App>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");

        builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

        builder.Services.AddSingleton<Navigator>();
        builder.Services.AddSingleton<IRepository, Repository>();

        // Blazor booststrap
        builder.Services.AddBootstrapBlazor();

        // BlazorPro.BlazorSize
        builder.Services.AddMediaQueryService();
        builder.Services.AddResizeListener();

        await builder.Build().RunAsync();
    }
}
