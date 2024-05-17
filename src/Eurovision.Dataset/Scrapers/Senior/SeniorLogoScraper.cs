namespace Eurovision.Dataset.Scrapers.Senior;

internal class SeniorLogoScraper : BaseLogoScraper
{
    public SeniorLogoScraper() : base(Constants.SENIOR_LOGOS_PATH, new Ogaespain())
    { }
}
