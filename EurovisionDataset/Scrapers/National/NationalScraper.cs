using EurovisionDataset.Data.National;

namespace EurovisionDataset.Scrapers.National;

public class NationalScraper : BaseScraper
{
    public async Task<IEnumerable<Contest>> GetDataAsync(int start, int end)
    {
        List<Contest> result = new List<Contest>();
        EurovisionWorld eurovisionWorld = new EurovisionWorld();
        await GetContestsAsync(start, end, result, eurovisionWorld.GetContestAsync);

        return result;
    }
}
