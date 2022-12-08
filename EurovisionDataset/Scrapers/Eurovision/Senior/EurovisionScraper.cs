using EurovisionDataset.Data.Eurovision.Senior;

namespace EurovisionDataset.Scrapers.Eurovision.Senior;

public class EurovisionScraper : BaseScraper
{
    private const string RESOURCES_PATH = "EurovisionDataset.Resources";

    public async Task<IEnumerable<Contest>> GetDataAsync(int start, int end)
    {
        List<Contest> result = new List<Contest>();

        await GetContestsFromEurovisionWorld(result, start, end);
        await GetContestsFromEschome(result);
        GetContestsFromEurovisionLOD(result);

        foreach (Contest contest in result)
        {
            InsertNoAvailableData(contest);
            LogNoAvailableData(contest);
        }

        result.Sort((a, b) => a.Year - b.Year);

        return result;
    }

    private async Task GetContestsFromEurovisionWorld(List<Contest> contests, int start, int end)
    {
        EurovisionWorld eurovisionWorld = new EurovisionWorld();

        await GetContestsAsync(start, end, contests, eurovisionWorld.GetContestAsync);
    }

    private async Task GetContestsFromEschome(List<Contest> contests)
    {
        Eschome eschome = new Eschome();
        await eschome.GetContestsInfoAsync(contests);
    }

    private void GetContestsFromEurovisionLOD(List<Contest> contests)
    {
        EurovisionLOD eurovisionLOD = new EurovisionLOD();
        eurovisionLOD.GetContests(contests);
    }

    private void InsertNoAvailableData(Contest contest)
    {
        Contestant contestant;

        switch (contest.Year)
        {
            case 2022:
                contest.LogoUrl = "https://upload.wikimedia.org/wikipedia/en/thumb/0/01/Eurovision_2022_Official_Logo.jpg/250px-Eurovision_2022_Official_Logo.jpg";
                break;

            case 2020:
                contest.LogoUrl = "https://upload.wikimedia.org/wikipedia/en/thumb/6/6f/Eurovision_Song_Contest_2020.svg/188px-Eurovision_Song_Contest_2020.svg.png";
                break;

            case 2015:
                contestant = contest.Contestants.First(c => c.Country == "RU");
                contestant.Lyrics = GetLyrics("English", "2015_russia_lyrics");
                contestant.VideoUrls = new[] { "https://www.youtube.com/embed/jBVY7Glcd84" };
                break;

            case 2005:
                contestant = contest.Contestants.First(c => c.Country == "RU");
                contestant.Lyrics = GetLyrics("English", "2005_russia_lyrics");
                contestant.VideoUrls = new[] { "https://www.youtube.com/embed/HQhgevOeh1E" };
                break;

            case 1995:
                contestant = contest.Contestants.First(c => c.Country == "RU");
                contestant.Lyrics = GetLyrics("Russian", "1995_russia_lyrics");
                contestant.VideoUrls = new[] { "https://www.youtube.com/embed/mZTZPE1mV2s" };
                break;
        }
    }

    private void LogNoAvailableData(Contest contest)
    {
        foreach (Contestant contestant in contest.Contestants)
        {
            string countryName = Utils.GetCountryName(contestant.Country);

            if (contestant.Lyrics.IsNullOrEmpty())
                Console.WriteLine($"Lyrics no available: {contest.Year} {countryName}");

            if (contestant.VideoUrls.IsNullOrEmpty())
                Console.WriteLine($"Video no available: {contest.Year} {countryName}");
        }
    }

    private IList<Data.Lyrics> GetLyrics(string languages, string path)
    {
        List<Data.Lyrics> result = new List<Data.Lyrics>
        {
            new Data.Lyrics
            {
                Languages = languages.Split(", "),
                Content = Utils.ReadEmbeddedTextResource($"{RESOURCES_PATH}.{path}.txt")
            }
        };

        return result;
    }
}
