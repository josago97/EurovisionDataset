using EurovisionDataset.Data;

namespace EurovisionDataset.Scrapers;

public abstract class BaseScraper
{
    private const int TASKS_GROUP_SIZE = 5;

    protected async Task GetContestsAsync<T, G>(int start, int end, List<T> contests, Func<int, Task<G>> func) where G : Contest where T : G
    {
        IList<T> result = await Utils.ParallelTaskFor(start, end + 1, TASKS_GROUP_SIZE, async year =>
        {
            T result = (T)await func(year);

            if (result == null)
                Console.WriteLine($"ERROR, no contest for {year}");
            else
                Console.WriteLine($"Added {year}");

            return result;
        });

        contests.AddRange(result);
    }
}
