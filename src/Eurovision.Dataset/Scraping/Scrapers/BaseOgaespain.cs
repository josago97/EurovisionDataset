using Microsoft.Playwright;
using SkiaSharp;

namespace Eurovision.Dataset.Scraping.Scrapers;

internal abstract class BaseOgaespain
{
    private const string BASE_URL = "https://www.ogaespain.com/";

    protected abstract string GetPageUrl(int year);

    public async Task<SKData> ScrapAsync(int year)
    {
        using HttpClient httpClient = new ScraperHttpClient();
        using PlaywrightScraper playwrightScraper = new PlaywrightScraper();

        await playwrightScraper.LoadPageAsync($"{BASE_URL}/{GetPageUrl(year)}");

        ILocator logo = playwrightScraper.Page.Locator("div.entry-content img").First;
        string logoUrl = await FindBestUrlAsync(logo);

        using Stream logoStream = await httpClient.GetStreamAsync(logoUrl);
        using MemoryStream memoryStream = new MemoryStream();
        logoStream.CopyTo(memoryStream);
        memoryStream.Position = 0;
        SKBitmap logoBitmap = SKBitmap.Decode(memoryStream);
        SKData logoRaw = logoBitmap.Encode(SKEncodedImageFormat.Jpeg, 100);

        return logoRaw;
    }

    private async Task<string> FindBestUrlAsync(ILocator logo)
    {
        string logoUrl;
        string scrset = await logo.GetAttributeAsync("srcset");

        if (string.IsNullOrEmpty(scrset))
        {
            logoUrl = await logo.GetAttributeAsync("src");
        }
        else
        {
            logoUrl = scrset.Split(',')
                .Select(urlMedia => urlMedia.TrimStart().Split(' ')[0])
                .MinBy(url => url.Length);
        }

        return logoUrl;
    }
}
