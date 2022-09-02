using System.Threading;
using Microsoft.Playwright;

namespace EurovisionDataset.Scrapers;

public class PlaywrightScraper : IDisposable
{
    private static readonly BrowserTypeLaunchOptions BROWSER_OPTIONS = new BrowserTypeLaunchOptions()
    {
        Headless = true
    };

    private static IPlaywright playwright;
    private static IBrowser browser;
    private static IBrowserContext context;

    public IPage Page { get; private set; }

    public static async Task InitAsync()
    {
        playwright = await Playwright.CreateAsync();
        browser = await playwright.Chromium.LaunchAsync(BROWSER_OPTIONS);
        context = await browser.NewContextAsync();
        context.SetDefaultTimeout(0);
    }

    public static async Task DisposeAsync()
    {
        if (context != null) await context.CloseAsync();
        if (browser != null) await browser.CloseAsync();
        if (playwright != null) playwright.Dispose();
    }

    public async Task<IResponse?> LoadPageAsync(string url, WaitUntilState waitUntil = WaitUntilState.Load, int? timeout = null)
    {
        IResponse result = null;

        PageGotoOptions pageGotoOptions = new PageGotoOptions()
        {
            WaitUntil = waitUntil
        };

        if (timeout.HasValue) pageGotoOptions.Timeout = timeout.Value;
        if (Page == null) Page = await context.NewPageAsync();

        while (result == null)
        {
            try
            {
                result = await Page.GotoAsync(url, pageGotoOptions);
            }
            catch { }
        }

        return result;
    }

    public async void Dispose()
    {
        if (Page != null && !Page.IsClosed) await Page.CloseAsync();
    }

}
