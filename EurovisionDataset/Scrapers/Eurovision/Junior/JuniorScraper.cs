using EurovisionDataset.Data.Junior;

namespace EurovisionDataset.Scrapers.Eurovision.Junior;

public class JuniorScraper : BaseScraper<Contest, Contestant>
{
    protected override int FirstYear => 2003;

    protected override async Task GetContestsAsync(int start, int end, List<Contest> result)
    {
        EurovisionWorld eurovisionWorld = new EurovisionWorld();
        await GetContestsAsync(start, end, result, eurovisionWorld.GetContestAsync);
    }
}
