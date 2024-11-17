namespace Eurovision.Dataset.Scraping.Scrapers;

internal class ScraperHttpClient : HttpClient
{
    public ScraperHttpClient()
    {
        DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0");
    }
}
