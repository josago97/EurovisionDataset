using System.Globalization;
using Eurovision.Dataset.Scraping;

namespace Eurovision.Dataset;

public class Program
{
    public static async Task Main(params string[] args)
    {
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
        Microsoft.Playwright.Program.Main(["install"]);

        Properties.ReadArguments(args);

        ScrapingHandler scrapingHandler = new ScrapingHandler();
        await scrapingHandler.ScrapAsync();

        Console.WriteLine("Press enter to exit...");
        Console.Read();
    }
}
