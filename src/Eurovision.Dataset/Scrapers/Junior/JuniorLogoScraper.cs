namespace Eurovision.Dataset.Scrapers.Junior;

internal class JuniorLogoScraper : BaseLogoScraper
{
    public JuniorLogoScraper() : base(Constants.JUNIOR_LOGOS_PATH, new Ogaespain())
    { }
}
