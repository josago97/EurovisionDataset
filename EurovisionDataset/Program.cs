using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using EurovisionDataset.Data;
using EurovisionDataset.Scrapers;

namespace EurovisionDataset;

public class Program
{
    private const string DATA_FILENAME = "eurovision.json";

    public static async Task Main(params string[] args)
    {
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
        Microsoft.Playwright.Program.Main(new[] { "install" });

        Properties.ReadArguments(args);

        Stopwatch stopwatch = Stopwatch.StartNew();
        Console.WriteLine("Extracting data...");
        Eurovision eurovision = await GetEurovisionAsync();
        Save(eurovision);
        Console.WriteLine($"Data estracted at time: {stopwatch.Elapsed}");
        Console.ReadLine();
    }

    private static async Task<Eurovision> GetEurovisionAsync()
    {
        Eurovision result;

        await PlaywrightScraper.InitAsync();
        EurovisionScraper eurovisionScraper = new EurovisionScraper();
        result = await eurovisionScraper.GetDataAsync(Properties.START, Properties.END);
        await PlaywrightScraper.DisposeAsync();

        return result;
    }

    private static void Save(Eurovision eurovision)
    {
        JsonSerializerOptions serializeOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        string jsonData = JsonSerializer.Serialize(eurovision, serializeOptions);
        File.WriteAllText(DATA_FILENAME, jsonData);
    }
}
