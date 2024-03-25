using EurovisionDataset.Entities;
using EurovisionDataset.Utilities;
using Microsoft.Playwright;

namespace EurovisionDataset.Scrapers;

internal abstract class BaseOgaespain
{
    private const string LOGOS_FOLDER_PATH = "logos";// festival-de-eurovision/eurovision-1989/
    private const string BASE_URL = "https://www.ogaespain.com/";// festival-de-eurovision/eurovision-1989/
    //eurovision-junior/junior-2023/
    protected abstract string LogosFolderName { get; } 

    public async Task ScrapAsync(IReadOnlyList<Contest> contests)
    {
        if (contests.Count == 0) return;

        int lastYearWithLogo = FindLastYearWithLogo();
        int start = Math.Max(contests[0].Year, lastYearWithLogo);
        int end = Math.Max(contests[^1].Year, lastYearWithLogo);

        using PlaywrightScraper playwrightScraper = new PlaywrightScraper();

        foreach (Contest contest in contests)
        {
            await playwrightScraper.LoadPageAsync($"{BASE_URL}/{GetPageUrl(contest.Year)}");

            string logoSelector = "div.single_text > p:nth-child(2) > img";
            IElementHandle logoElement = await playwrightScraper.Page.QuerySelectorAsync(logoSelector);
            string logoUrl = await logoElement.GetAttributeAsync("src");

            contest.LogoUrl = logoUrl;
        }
    }

    protected abstract string GetPageUrl(int year);

    private async Task<byte[]> GetLogoAsync()
    {
        return null;
    } 

    private int FindLastYearWithLogo()
    {
        string folderPath = Asset.GetFileSystemAbsolutePath($"{LOGOS_FOLDER_PATH}/{LogosFolderName}");

        return Directory.EnumerateFiles(folderPath)
            .Select(Path.GetFileNameWithoutExtension)
            .Select(int.Parse)
            .Max();
    }
}
