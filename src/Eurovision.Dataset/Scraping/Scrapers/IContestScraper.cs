using Eurovision.Dataset.Entities;

namespace Eurovision.Dataset.Scraping.Scrapers;

internal interface IContestScraper<TContest> where TContest : Contest, new()
{
    Task<IReadOnlyList<TContest>> ScrapContestsAsync(int start, int end);
}
