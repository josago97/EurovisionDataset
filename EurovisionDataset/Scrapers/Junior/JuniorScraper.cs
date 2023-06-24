using EurovisionDataset.Data;

namespace EurovisionDataset.Scrapers.Junior;

public class JuniorScraper : BaseScraper<Contest, Contestant>
{
    protected override int FirstYear => 2003;

    protected override EurovisionWorld EurovisionWorld { get; } = new EurovisionWorld();

    protected override async Task GetContestsAsync(int start, int end, IList<Contest> result)
    {
        await GetContestsAsync(start, end, result, EurovisionWorld.GetContestAsync);
    }
}
