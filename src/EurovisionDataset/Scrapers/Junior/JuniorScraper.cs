using EurovisionDataset.Entities;

namespace EurovisionDataset.Scrapers.Junior;

public class JuniorScraper : BaseScraper<Contest, Contestant>
{
    protected override int FirstYear => 2003;

    protected override EurovisionWorld EurovisionWorld { get; } = new EurovisionWorld();
}
