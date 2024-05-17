using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Eurovision.Dataset.Scrapers;
using Eurovision.Dataset.Scrapers.Junior;
using Eurovision.Dataset.Scrapers.Senior;
using Eurovision.Dataset.Utilities;
using EurovisionWorld = Eurovision.Dataset.Scrapers.BaseEurovisionWorld;

namespace Eurovision.Dataset;

public class Program
{
    private const string DATA_FOLDER = "Dataset";
    private const string COUNTRIES_FILENAME = "countries";
    private const string SENIOR_FILENAME = "eurovision";
    private const string JUNIOR_FILENAME = "junior";

    public static async Task Main(params string[] args)
    {
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
        Microsoft.Playwright.Program.Main(["install"]);

        Properties.ReadArguments(args);

        Console.WriteLine($"Start: {Properties.START}, End: {Properties.END}");
        Console.WriteLine("Extracting data...");

        await PlaywrightScraper.InitAsync(Properties.HIDE_BROWSER);
        await EurovisionWorld.AcceptCookiesAsync();
        await GetDataAsync();
        await PlaywrightScraper.DisposeAsync();

        Console.WriteLine("Press enter to exit...");
        Console.Read();
    }

    private static async Task GetDataAsync()
    {
        /*
        var contests = Enumerable.Range(1956, 32).Select(x => new Entities.Contest() { Year = x }).ToArray();
        var logoScraper = new SeniorLogoScraper();
        
        foreach(var contest in contests)
        {
            await logoScraper.ScrapAsync(contest);
        }
        */

        var contests = Enumerable.Range(2003, 21).Select(x => new Entities.Contest() { Year = x }).ToArray();
        var logoScraper = new JuniorLogoScraper();

        foreach (var contest in contests)
        {
            await logoScraper.ScrapAsync(contest);
        }

        //await Task.WhenAll(contests.Select(logoScraper.ScrapAsync));

        Stopwatch stopwatch = Stopwatch.StartNew();

        GetCountries();
        if (Properties.EUROVISION_JUNIOR) await GetJuniorAsync();
        if (Properties.EUROVISION_SENIOR) await GetSeniorAsync();

        Console.WriteLine($"All data estracted at time: {stopwatch.Elapsed}");
    }

    private static void GetCountries()
    {
        Save(CountryCollection.COUNTRY_CODES, COUNTRIES_FILENAME);
    }

    private static Task GetSeniorAsync()
    {
        return ScrapContestsAsync("Eurovision", SENIOR_FILENAME, new SeniorScraper());
    }

    private static Task GetJuniorAsync()
    {
        return ScrapContestsAsync("Eurovision Junior", JUNIOR_FILENAME, new JuniorScraper());
    }

    private static async Task ScrapContestsAsync<TContest, TContestant>(string name, string fileName, 
        BaseScraper<TContest, TContestant> scraper) 
        where TContest : Entities.Contest, new()
        where TContestant : Entities.Contestant, new()
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        Console.WriteLine($"Getting {name} data");

        IReadOnlyList<TContest> contests = await scraper.ScrapContestsAsync(Properties.START, Properties.END);

        Save(contests, fileName);

        Console.WriteLine("Data extracted in {0}", stopwatch.Elapsed);
    }

    private static void Save(object data, string filename)
    {
        JsonSerializerOptions serializeOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = Properties.JSON_INDENTED,
            TypeInfoResolver = new PolymorphicTypeResolver(),
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        Directory.CreateDirectory(DATA_FOLDER);
        string jsonData = JsonSerializer.Serialize(data, serializeOptions);
        File.WriteAllText($"{DATA_FOLDER}/{filename}.json", jsonData);
    }

    private class PolymorphicTypeResolver : DefaultJsonTypeInfoResolver
    {
        private Assembly CurrentAssembly { get; set; }

        public PolymorphicTypeResolver()
        {
            CurrentAssembly = GetType().Assembly;
        }

        public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
        {
            JsonTypeInfo jsonTypeInfo = base.GetTypeInfo(type, options);

            if (CurrentAssembly == type.Assembly)
            {
                IEnumerable<Type> derivedTypes = GetAllSubclassOf(type);

                if (derivedTypes.Any())
                {
                    jsonTypeInfo.PolymorphismOptions = new JsonPolymorphismOptions
                    {
                        IgnoreUnrecognizedTypeDiscriminators = true,
                        UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FailSerialization
                    };

                    foreach (Type derivedType in derivedTypes)
                        jsonTypeInfo.PolymorphismOptions.DerivedTypes.Add(new JsonDerivedType(derivedType));
                }
            }

            return jsonTypeInfo;
        }

        public IEnumerable<Type> GetAllSubclassOf(Type parent)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                foreach (Type type in assembly.GetTypes())
                    if (type.IsSubclassOf(parent)) 
                        yield return type;
        }
    }
}
