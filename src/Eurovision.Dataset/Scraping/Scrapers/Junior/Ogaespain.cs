namespace Eurovision.Dataset.Scraping.Scrapers.Junior;

internal class Ogaespain : BaseOgaespain
{
    protected override string GetPageUrl(int year)
    {
        return $"eurovision-junior/junior-{year}";
    }
}
