using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Linq;
using Eurovision.Dataset.Entities;
using Eurovision.Dataset.Scraping.Scrapers;
using Eurovision.Dataset.Scraping.Scrapers.Junior;
using Eurovision.Dataset.Scraping.Scrapers.Senior;
using Eurovision.Dataset.Utilities;
using EurovisionWorld = Eurovision.Dataset.Scraping.Scrapers.BaseEurovisionWorld;

namespace Eurovision.Dataset.Scraping;

internal class ScrapingHandler
{
    private static readonly JsonSerializerOptions JSON_OPTIONS = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = Properties.JSON_INDENTED,
        TypeInfoResolver = new PolymorphicTypeResolver(),
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public async Task ScrapAsync()
    {
        Console.WriteLine("Start scraping data...");

        await PlaywrightScraper.InitAsync(Properties.HIDE_BROWSER);
        await EurovisionWorld.AcceptCookiesAsync();
        await ScrapDataAsync();
        await PlaywrightScraper.DisposeAsync();
    }

    private async Task ScrapDataAsync()
    {
        Stopwatch stopwatch = Stopwatch.StartNew();

        Save(CountryCollection.COUNTRY_CODES, Constants.COUNTRIES_FILENAME);

        if (Properties.EUROVISION_JUNIOR)
            await ScrapContestsAsync("Eurovision Junior", Constants.JUNIOR_FILENAME, new JuniorScraper());

        if (Properties.EUROVISION_SENIOR) 
            await ScrapContestsAsync("Eurovision", Constants.SENIOR_FILENAME, new SeniorScraper());

        Console.WriteLine($"All data estracted at time: {stopwatch.Elapsed}");
    }

    private async Task ScrapContestsAsync<TContest>(string name, string fileName,
        IContestScraper<TContest> scraper) where TContest : Contest, new()
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        List<TContest> contests = new List<TContest>();
        int start = Properties.START;
        int end = Properties.END;

        if (TryReuseData(fileName, out TContest[] oldContests))
        {
            int lastYear = oldContests[^1].Year;
            start = lastYear + 1;
            contests.AddRange(oldContests);

            Console.WriteLine($"Restored {name} data, last contest year: {lastYear}");
        }

        Console.WriteLine($"Scraping {name} data, start: {start}, end: {end}");

        IReadOnlyList<TContest> newContests = await scraper.ScrapContestsAsync(start, end);
        contests.AddRange(newContests);
        Save(contests, fileName);

        Console.WriteLine($"{name} data scraped in {stopwatch.Elapsed}");
    }

    private bool TryReuseData<TContest>(string fileName, out TContest[] oldContests)
    {
        oldContests = null;

        if (Properties.REUSE_OLD_DATA)
        {
            oldContests = Load<TContest>(fileName);
        }

        return oldContests != null && oldContests.Length > 0;
    }

    private T[] Load<T>(string filename)
    {
        T[] contests = null;
        string filePath = GetDataFilePath(filename);

        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            contests = JsonSerializer.Deserialize<T[]>(json, JSON_OPTIONS);
        }

        return contests;
    }

    private void Save<T>(T data, string filename)
    {
        Directory.CreateDirectory(Constants.DATASET_PATH);
        string jsonData = JsonSerializer.Serialize(data, JSON_OPTIONS);
        File.WriteAllText(GetDataFilePath(filename), jsonData);
    }

    private string GetDataFilePath(string filename)
    {
        return $"{Constants.DATASET_PATH}/{filename}.json";
    }
}
