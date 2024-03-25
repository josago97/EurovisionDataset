using System.Linq;
using EurovisionDataset.Entities;
using EurovisionDataset.Utilities;
using Microsoft.Playwright;
using SkiaSharp;

namespace EurovisionDataset.Scrapers;

internal abstract class BaseOgaespain
{
    private const string LOGOS_FOLDER_PATH = "logos";
    private const string BASE_URL = "https://www.ogaespain.com/";
    private const string REMOTE_URL_BASE = "https://raw.githubusercontent.com/josago97/EurovisionDataset/main/Assets/Logos/Senior/1956.png";

    protected abstract string LogosFolderName { get; }

    public async Task ScrapAsync(IReadOnlyList<Contest> contests)
    {
        if (contests.Count == 0) return;

        Dictionary<int, string> storedLogos = GetStoredLogos();
        //int lastYearWithLogo = FindLastYearWithLogo();
        //int start = Math.Max(contests[0].Year, lastYearWithLogo);
        //int end = Math.Max(contests[^1].Year, lastYearWithLogo);

        string folderPath = Asset.GetFileSystemAbsolutePath($"{LOGOS_FOLDER_PATH}/{LogosFolderName}");
        using HttpClient httpClient = new HttpClient();
        using PlaywrightScraper playwrightScraper = new PlaywrightScraper();

        foreach (Contest contest in contests)
        {
            int year = contest.Year;

            if (storedLogos.TryGetValue(year, out string logoPath))
            {

            }
            else
            {
                SKData logoRaw = await GetLogoRawAsync(year, httpClient, playwrightScraper);
                string logoUrl = SaveAndGetLogoUrl(year, logoRaw, folderPath);
            }

            contest.LogoUrl = "";
        }
    }

    protected abstract string GetPageUrl(int year);

    private async Task<SKData> GetLogoRawAsync(int year, HttpClient httpClient, PlaywrightScraper playwrightScraper)
    {
        await playwrightScraper.LoadPageAsync($"{BASE_URL}/{GetPageUrl(year)}");

        string logoSelector = "div.single_text > p > img";
        IElementHandle logoElement = await playwrightScraper.Page.QuerySelectorAsync(logoSelector);
        string logoUrl = await logoElement.GetAttributeAsync("src");

        using Stream logoStream = await httpClient.GetStreamAsync(logoUrl);
        SKBitmap logoBitmap = SKBitmap.Decode(logoStream);
        SKData logoRaw = logoBitmap.Encode(SKEncodedImageFormat.Png, 100);

        return logoRaw;
    }

    private string SaveAndGetLogoUrl(int year, SKData logoRaw, string folderPath)
    {
        string imagePath = $"{folderPath}/{year}.png";
        using FileStream file = File.Create(imagePath);
        logoRaw.SaveTo(file);

        return REMOTE_URL_BASE;
    }

    private Dictionary<int, string> GetStoredLogos()
    {
        string folderPath = Asset.GetFileSystemAbsolutePath($"{LOGOS_FOLDER_PATH}/{LogosFolderName}");

        return Directory.EnumerateFiles(folderPath)
            .ToDictionary(
                path => int.Parse(Path.GetFileNameWithoutExtension(path)),
                path => Path.GetRelativePath(path, path)
            );
    }
}
