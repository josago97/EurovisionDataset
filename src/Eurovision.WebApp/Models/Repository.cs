using System.Net.Http.Json;
using System.Text.Json;

namespace Eurovision.WebApp.Models;

public class Repository : IRepository
{
    protected const string COUNTRIES_FILENAME = "countries.json";
    protected const string CONTESTS_FILENAME = "eurovision.json";
    protected const string JUNIOR_CONTESTS_FILENAME = "junior.json";
    private static readonly JsonSerializerOptions JSON_OPTIONS = new JsonSerializerOptions()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IServiceProvider _serviceProvider;
    private bool _isInitialized = false;

    public IDictionary<string, string> Countries { get; private set; }
    public IReadOnlyList<Contest> SeniorContests { get; private set; }
    public IReadOnlyList<Contest> JuniorContests { get; private set; }

    public Repository(IServiceProvider serviceProvider) : base()
    {
        _serviceProvider = serviceProvider;
    }

    public async Task InitAsync()
    {
        if (!_isInitialized)
        {
            await Task.WhenAll(GetCountriesAsync(),
                GetSeniorContestsAsync(),
                GetJuniorContestsAsync());

            _isInitialized = true;
        }
        else
        {
            await Task.CompletedTask;
        }
    }

    private async Task GetCountriesAsync()
    {
        Countries = await GetAsync<Dictionary<string, string>>(COUNTRIES_FILENAME);
    }

    private async Task GetSeniorContestsAsync()
    {
        SeniorContests = await GetAsync<Contest[]>(CONTESTS_FILENAME);
    }

    private async Task GetJuniorContestsAsync()
    {
        JuniorContests = await GetAsync<Contest[]>(JUNIOR_CONTESTS_FILENAME);
    }

    private async Task<T> GetAsync<T>(string filename)
    {
        using IServiceScope scope = _serviceProvider.CreateScope();
        HttpClient httpClient = scope.ServiceProvider.GetRequiredService<HttpClient>();

        T result = await httpClient.GetFromJsonAsync<T>($"data/{filename}", JSON_OPTIONS, default);

        return result;
        //return JsonSerializer.Generic.Utf16.Deserialize<T, IncludeNullsCamelCaseResolver<char>>(json);
    }
}
