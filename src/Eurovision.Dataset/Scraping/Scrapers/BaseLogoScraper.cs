using Eurovision.Dataset.Entities;
using SkiaSharp;

namespace Eurovision.Dataset.Scraping.Scrapers;

internal abstract class BaseLogoScraper
{
    private const string GITHUB_URL = $"{Constants.GITHUB_ASSETS_URL}";

    private Dictionary<int, string> StoredLogos { get; init; }
    private BaseOgaespain Ogaespain { get; init; }
    private string RelativeFolderPath { get; init; }
    private string FolderPath { get; init; }

    public BaseLogoScraper(string folder, BaseOgaespain ogaespain)
    {
        RelativeFolderPath = folder;
        Ogaespain = ogaespain;
        FolderPath = $"{Constants.ASSETS_PATH}/{RelativeFolderPath}";
        StoredLogos = GetStoredLogos();
    }

    private Dictionary<int, string> GetStoredLogos()
    {
        return Directory.EnumerateFiles(FolderPath)
            .ToDictionary(
                path => int.Parse(Path.GetFileNameWithoutExtension(path)),
                Path.GetFileName
            );
    }


    public async Task ScrapAsync(Contest contest)
    {
        int year = contest.Year;

        if (!StoredLogos.TryGetValue(year, out string logoFileName))
        {
            SKData logoRaw = await Ogaespain.ScrapAsync(year);
            logoFileName = SaveLogo(year, logoRaw);
        }

        contest.LogoUrl = $"{GITHUB_URL}{RelativeFolderPath}/{logoFileName}";
    }

    private string SaveLogo(int year, SKData logoRaw)
    {
        string fileName = $"{year}.png";
        string logoPath = $"{FolderPath}/{fileName}";
        using FileStream file = File.Create(logoPath);
        logoRaw.SaveTo(file);

        return fileName;
    }
}
