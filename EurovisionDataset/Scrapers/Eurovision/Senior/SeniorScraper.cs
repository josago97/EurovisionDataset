using EurovisionDataset.Data.Senior;

namespace EurovisionDataset.Scrapers.Eurovision.Senior;

public class SeniorScraper : BaseScraper<Contest, Contestant>
{
    protected override int FirstYear => 1956;

    protected override async Task GetContestsAsync(int start, int end, List<Contest> result)
    {
        await GetContestsFromEurovisionWorld(result, start, end);
        await GetContestsFromEschome(result);
        GetContestsFromEurovisionLOD(result);
    }

    private async Task GetContestsFromEurovisionWorld(List<Contest> contests, int start, int end)
    {
        EurovisionWorld2 eurovisionWorld = new EurovisionWorld2();

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

    protected override void InsertUnavailableData(Contest contest)
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
                contestant.Lyrics = GetLyrics("English", contestant.Song, "2015_russia_lyrics");
                contestant.VideoUrls = new[] { "https://www.youtube.com/embed/jBVY7Glcd84" };
                break;

            case 2005:
                contestant = contest.Contestants.First(c => c.Country == "RU");
                contestant.Lyrics = GetLyrics("English", contestant.Song, "2005_russia_lyrics");
                contestant.VideoUrls = new[] { "https://www.youtube.com/embed/HQhgevOeh1E" };
                break;

            case 1995:
                contestant = contest.Contestants.First(c => c.Country == "RU");
                contestant.Lyrics = GetLyrics("Russian", contestant.Song, "1995_russia_lyrics");
                contestant.VideoUrls = new[] { "https://www.youtube.com/embed/mZTZPE1mV2s" };
                break;
        }
    }

    protected override void CheckUnvailableData(Contestant contestant, List<string> noAvailable)
    {
        base.CheckUnvailableData(contestant, noAvailable);

        if (string.IsNullOrEmpty(contestant.Broadcaster))
            noAvailable.Add("Broadcaster");
    }
}
