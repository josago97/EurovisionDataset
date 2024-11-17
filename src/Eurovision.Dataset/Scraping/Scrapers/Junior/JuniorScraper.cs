using Eurovision.Dataset.Entities;

namespace Eurovision.Dataset.Scraping.Scrapers.Junior;

internal class JuniorScraper : BaseScraper<Contest, Contestant>
{
    protected override int FirstYear => 2003;

    protected override EurovisionWorld EurovisionWorld { get; } = new EurovisionWorld();

    protected override BaseLogoScraper LogoScraper { get; } = new JuniorLogoScraper();
}
