namespace Eurovision.Dataset.Scraping.Scrapers.Junior;

internal class JuniorLogoScraper : BaseLogoScraper
{
    public JuniorLogoScraper() : base(Constants.JUNIOR_LOGOS_FOLDER, new Ogaespain())
    { }
}
