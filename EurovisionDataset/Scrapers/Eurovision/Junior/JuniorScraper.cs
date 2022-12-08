using EurovisionDataset.Data.Eurovision.Junior;

namespace EurovisionDataset.Scrapers.Eurovision.Junior;

public class JuniorScraper : BaseScraper
{
    private const int FIRST_YEAR = 2003;

    public async Task<IEnumerable<Contest>> GetDataAsync(int start, int end)
    {
        List<Contest> result = new List<Contest>();

        if (end >= FIRST_YEAR)
        {
            start = Math.Max(start, FIRST_YEAR);

            EurovisionWorld eurovisionWorld = new EurovisionWorld();
            await GetContestsAsync(start, end, result, eurovisionWorld.GetContestAsync);
        }

        return result;
    }
}
