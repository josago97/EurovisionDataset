using Eurovision.Dataset.Entities;
using Eurovision.Dataset.Utilities;
using Sharplus.System.Linq;

namespace Eurovision.Dataset.Scrapers;

internal abstract class BaseScraper<TContest, TContestant>
    where TContest : Contest, new()
    where TContestant : Contestant, new()
{
    protected abstract int FirstYear { get; }
    protected abstract BaseEurovisionWorld<TContest, TContestant> EurovisionWorld { get; }
    protected abstract BaseLogoScraper LogoScraper { get; }

    public async Task<IReadOnlyList<TContest>> ScrapContestsAsync(int start, int end)
    {
        if (end < FirstYear) return [];

        start = Math.Max(start, FirstYear);
        end = Math.Min(end, await EurovisionWorld.GetLastYearAsync());

        List<TContest> contests = new List<TContest>();

        for (int year = start; year <= end; year++)
        {
            TContest contest = await EurovisionWorld.GetContestAsync(year);

            if (contest == null)
                Console.WriteLine($"ERROR, no contest for {year}");
            else
            {
                await LogoScraper.ScrapAsync(contest);
                contests.Add(contest);
                Console.WriteLine($"Added contest {year}");
            }
        }

        await ScrapContests(contests);

        foreach (TContest contest in contests)
        {
            InsertUnavailableData(contest);
            LogUnavailableData(contest);
        }

        return contests;
    }

    protected virtual Task ScrapContests(IReadOnlyList<TContest> contests) => Task.CompletedTask;

    protected virtual void InsertUnavailableData(TContest contest) { }

    private void LogUnavailableData(TContest contest)
    {
        List<string> unavailable = new List<string>();

        GetLogUnavailableData(CheckUnavailableData, contest, $"Unavailable data of contest {contest.Year}:", unavailable);

        if (unavailable.Count > 0)
        {
            int tab = 0;
            Console.WriteLine();

            foreach (string log in unavailable)
            {
                if (log == "+")
                    tab++;
                else if (log == "-")
                    tab--;
                else
                    Console.WriteLine(new string('\t', tab) + log);
            }

            Console.WriteLine();

            if (Properties.THROW_EXCEPTION_UNAVAILABLE_DATA)
            {
                throw new Exception("Unavailable data");
            }
        }
    }

    private void GetLogUnavailableData<T>(Action<T, List<string>> method, T data, string header, List<string> unavailable)
    {
        List<string> noAvailableAux = new List<string>();

        method(data, noAvailableAux);

        if (noAvailableAux.Count > 0)
        {
            unavailable.Add(header);
            unavailable.Add("+");
            unavailable.AddRange(noAvailableAux);
            unavailable.Add("-");
        }
    }

    protected virtual void CheckUnavailableData(TContest contest, List<string> unavailable)
    {
        if (string.IsNullOrEmpty(contest.Country))
            unavailable.Add("Country");

        if (string.IsNullOrEmpty(contest.Arena))
            unavailable.Add("Arena");

        if (string.IsNullOrEmpty(contest.City))
            unavailable.Add("City");

        if (contest.Contestants.IsNullOrEmpty())
            unavailable.Add("Contestants");
        else
        {
            foreach (TContestant contestant in contest.Contestants)
            {
                string countryName = CountryCollection.GetCountryName(contestant.Country);
                GetLogUnavailableData(CheckUnvailableData, contestant, $"Contestant {countryName}:", unavailable);
            }
        }

        if (contest.Rounds.IsNullOrEmpty())
            unavailable.Add("Rounds");
    }

    protected virtual void CheckUnvailableData(TContestant contestant, List<string> unavailable)
    {
        if (string.IsNullOrEmpty(contestant.Song))
            unavailable.Add("Song title");

        if (string.IsNullOrEmpty(contestant.Artist))
            unavailable.Add("Artist");

        if (contestant.Lyrics.IsNullOrEmpty())
            unavailable.Add("Lyrics");

        if (contestant.VideoUrls.IsNullOrEmpty())
            unavailable.Add("Video");
    }

    protected IList<Lyrics> GetLyrics(string languages, string title, string path)
    {
        return
        [
            new Lyrics
            {
                Languages = languages.Split(", "),
                Title = title,
                Content = Asset.ReadEmbedTextResource($"{path}.txt")
            }
        ];
    }
}