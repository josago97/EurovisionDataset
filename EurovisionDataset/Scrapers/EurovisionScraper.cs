using EurovisionDataset.Data;

namespace EurovisionDataset.Scrapers
{
    public class EurovisionScraper
    {
        private const string RESOURCES_PATH = "EurovisionDataset.Resources";
        private const int TASK_GROUP_SIZE = 5;

        public async Task<Eurovision> GetDataAsync(int start, int end)
        {
            return new Eurovision()
            {
                Contests = await GetContestsAsync(start, end),
                Countries = Utils.COUNTRY_CODES
            };
        }

        private async Task<Contest[]> GetContestsAsync(int start, int end)
        {
            List<Contest> contests = new List<Contest>();

            Eschome eschome = new Eschome();
            EurovisionWorld eurovisionWorld = new EurovisionWorld();
            List<Task> tasks = new List<Task>();

            await eurovisionWorld.RemovePopUpAsync();

            foreach (Contest contest in await eschome.GetContestsAsync(start, end))
            {
                Console.WriteLine($"Add {contest.Year}");
                tasks.Add(Task.Run(async () =>
                {
                    await eurovisionWorld.GetContestAsync(contest);
                    InsertNoAvailableData(contest);
                    LogNoAvailableData(contest);
                    lock (contests) contests.Add(contest);
                }));

                if (tasks.Count == TASK_GROUP_SIZE) 
                { 
                    await Task.WhenAll(tasks);
                    tasks.Clear();
                }
            }

            if (tasks.Count > 0) await Task.WhenAll(tasks);

            if (start <= 2020 && 2020 <= end)
            {
                Console.WriteLine("Add 2020");
                contests.Add(await eurovisionWorld.GetContestAsync(2020));
            }

            return contests.OrderBy(c => c.Year).ToArray();
        }

        private void InsertNoAvailableData(Contest contest)
        {
            Contestant contestant;

            switch (contest.Year)
            {
                case 2015:
                    contestant = contest.Contestants.First(c => c.Country == "RU");
                    contestant.Lyrics = Utils.ReadEmbeddedTextResource($"{RESOURCES_PATH}.2015_russia_lyrics.txt");
                    contestant.VideoUrl = "https://www.youtube.com/embed/jBVY7Glcd84";
                    break;

                case 2005:
                    contestant = contest.Contestants.First(c => c.Country == "RU");
                    contestant.Lyrics = Utils.ReadEmbeddedTextResource($"{RESOURCES_PATH}.2005_russia_lyrics.txt");
                    contestant.VideoUrl = "https://www.youtube.com/embed/HQhgevOeh1E";
                    break;

                case 1995:
                    contestant = contest.Contestants.First(c => c.Country == "RU");
                    contestant.Lyrics = Utils.ReadEmbeddedTextResource($"{RESOURCES_PATH}.1995_russia_lyrics.txt");
                    contestant.VideoUrl = "https://www.youtube.com/embed/mZTZPE1mV2s";
                    break;
            }
        }

        private void LogNoAvailableData(Contest contest)
        {
            foreach (Contestant contestant in contest.Contestants)
            {
                string countryName = Utils.GetCountryName(contestant.Country);

                if (string.IsNullOrEmpty(contestant.Lyrics))
                    Console.WriteLine($"Lyrics no available: {contest.Year} {countryName}");

                if (string.IsNullOrEmpty(contestant.VideoUrl))
                    Console.WriteLine($"Video no available: {contest.Year} {countryName}");
            }
        }
    }
}
