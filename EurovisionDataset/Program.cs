using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using EurovisionDataset.Scrapers;
using EurovisionDataset.Scrapers.Eurovision.Junior;
using EurovisionDataset.Scrapers.Eurovision.Senior;

namespace EurovisionDataset;

public class Program
{
    private const string FOLDERNAME = "Dataset";
    private const string COUNTRIES_FILENAME = "countries";
    private const string SENIOR_FILENAME = "eurovision";
    private const string JUNIOR_FILENAME = "junior";

    public static async Task Main(params string[] args)
    {
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
        Microsoft.Playwright.Program.Main(new[] { "install" });

        Properties.ReadArguments(args);

        await PlaywrightScraper.InitAsync(Properties.HIDE_BROWSER);
        Stopwatch stopwatch = Stopwatch.StartNew();
        Console.WriteLine("Extracting data...");

        await Scrapers.EurovisionWorld.RemovePopUpAsync();

        GetCountries();
        if (Properties.EUROVISION_JUNIOR) await GetJuniorAsync();
        if (Properties.EUROVISION_SENIOR) await GetSeniorAsync();

        Console.WriteLine($"All data estracted at time: {stopwatch.Elapsed}");
        await PlaywrightScraper.DisposeAsync();
        Console.ReadLine();
    }

    private static void GetCountries()
    {
        Save(Utils.COUNTRY_CODES, COUNTRIES_FILENAME);
    }

    private static async Task GetSeniorAsync()
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        Console.WriteLine("Getting Eurovision data");

        SeniorScraper seniorScraper = new SeniorScraper();
        var data = await seniorScraper.GetDataAsync(Properties.START, Properties.END);

        Save(data, SENIOR_FILENAME);

        Console.WriteLine("Data extracted in {0}", stopwatch.Elapsed);
    }

    private static async Task GetJuniorAsync()
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        Console.WriteLine("Getting Eurovision Junior data");

        JuniorScraper juniorScraper = new JuniorScraper();
        var data = await juniorScraper.GetDataAsync(Properties.START, Properties.END);

        Save(data, JUNIOR_FILENAME);

        Console.WriteLine("Data extracted in {0}", stopwatch.Elapsed);
    }

    private static void Save(object data, string filename)
    {
        JsonSerializerOptions serializeOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            TypeInfoResolver = new PolymorphicTypeResolver(),
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        Directory.CreateDirectory(FOLDERNAME);
        string jsonData = JsonSerializer.Serialize(data, serializeOptions);
        File.WriteAllText($"{FOLDERNAME}/{filename}.json", jsonData);
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
                    if (type.IsSubclassOf(parent)) yield return type;
        }
    }
}
