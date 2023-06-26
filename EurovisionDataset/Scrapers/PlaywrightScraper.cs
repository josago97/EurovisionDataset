using Microsoft.Playwright;

namespace EurovisionDataset.Scrapers;

public class PlaywrightScraper : IDisposable
{
    private static IPlaywright playwright;
    private static IBrowser browser;
    private static IBrowserContext context;

    public IPage Page { get; private set; }

    public static async Task InitAsync(bool headless)
    {
        BrowserTypeLaunchOptions options = new BrowserTypeLaunchOptions()
        {
            Headless = headless
        };

        playwright = await Playwright.CreateAsync();
        browser = await playwright.Chromium.LaunchAsync(options);
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
                Console.WriteLine($"Ultima página visitada: {url}"); // TODO: QUITAR
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
