namespace EurovisionDataset.Utilities;

public static class Utils
{
    public static async Task<IList<T>> ParallelTaskFor<T>(int fromInclusive, int toExclusive, int groupSize, Func<int, Task<T>> func)
    {
        List<Task<T>> tasks = new List<Task<T>>();
        List<T> result = new List<T>();
        int count = 0;

        for (int i = fromInclusive; i < toExclusive; i++)
        {
            tasks.Add(func(i));

            if (tasks.Count == groupSize || i == toExclusive - 1)
            {
                result.AddRange(await Task.WhenAll(tasks));
                tasks.Clear();
            }

            count++;
        }

        return result;
    }
}
