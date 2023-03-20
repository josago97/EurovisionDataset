using EurovisionDataset.Data;

namespace EurovisionDataset.Scrapers;

public abstract class BaseScraper<TContest, TContestant>
    where TContest : Contest
    where TContestant : Contestant
{
    private const string RESOURCES_PATH = "EurovisionDataset.Resources";

    protected abstract int FirstYear { get; }

    public async Task<IEnumerable<TContest>> GetDataAsync(int start, int end)
    {
        List<TContest> result = new List<TContest>();

        if (end >= FirstYear)
        {
            start = Math.Max(start, FirstYear);

            await GetContestsAsync(start, end, result);

            foreach (TContest contest in result)
            {
                InsertUnavailableData(contest);
                LogUnavailableData(contest);
            }

            result.Sort((a, b) => a.Year - b.Year);
        }

        return result;
    }

    protected abstract Task GetContestsAsync(int start, int end, IList<TContest> result);

    protected virtual void InsertUnavailableData(TContest contest) { }

    private void LogUnavailableData(TContest contest)
    {
        List<string> unavailable = new List<string>();

        GetLogUnavailableData(CheckUnavailableData, contest, $"Unavailable of contest {contest.Year}:", unavailable);

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
                string countryName = Utils.GetCountryName(contestant.Country);
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

    protected async Task GetContestsAsync(int start, int end, IList<TContest> contests, Func<int, Task<TContest>> func)
    {
        for (int year = start; year <= end; year++)
        {
            TContest contest = await func(year);

            if (contest == null)
                Console.WriteLine($"ERROR, no contest for {year}");
            else
            {
                Console.WriteLine($"Added {year}");
                contests.Add(contest);
            }
        }
        /*
        IList<T> result = await Utils.ParallelTaskFor(start, end + 1, TASKS_GROUP_SIZE, async year =>
        {
            T result = (T)await func(year);

            if (result == null)
                Console.WriteLine($"ERROR, no contest for {year}");
            else
                Console.WriteLine($"Added {year}");

            return result;
        });

        contests.AddRange(result);*/
    }

    protected IList<Lyrics> GetLyrics(string languages, string title, string path)
    {
        return new List<Lyrics>
        {
            new Lyrics
            {
                Languages = languages.Split(", "),
                Title = title,
                Content = Utils.ReadEmbeddedTextResource($"{RESOURCES_PATH}.{path}.txt")
            }
        };
    }
}
