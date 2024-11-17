using Eurovision.Dataset.Scraping.Scrapers;

namespace Eurovision.Dataset.Scraping.Scrapers.Senior;

internal class SeniorLogoScraper : BaseLogoScraper
{
    public SeniorLogoScraper() : base(Constants.SENIOR_LOGOS_FOLDER, new Ogaespain())
    { }
}
