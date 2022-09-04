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
            List<Contest> result = new List<Contest>();

            Eschome eschome = new Eschome();
            EurovisionLOD eurovisionLOD = new EurovisionLOD();
            EurovisionWorld eurovisionWorld = new EurovisionWorld();

            Contest[] contests = await eschome.GetContestsAsync(start, end);
            eurovisionLOD.GetContests(contests);

            List<Task> tasks = new List<Task>();
            await eurovisionWorld.RemovePopUpAsync();

            for (int i = 0; i < contests.Length; i++)
            {
                Contest contest = contests[i]; 
                Console.WriteLine($"Add {contest.Year}");
                tasks.Add(eurovisionWorld.GetContestAsync(contest));

                if (tasks.Count == TASK_GROUP_SIZE || i == contests.Length - 1)
                {
                    await Task.WhenAll(tasks);
                    tasks.Clear();
                }
            }

            result.AddRange(contests);

            if (start <= 2020 && 2020 <= end)
            {
                Console.WriteLine("Add 2020");
                result.Add(await eurovisionWorld.GetContestAsync(2020));
            }

            foreach (Contest contest in result)
            {
                InsertNoAvailableData(contest);
                LogNoAvailableData(contest);
            }

            return result.OrderBy(c => c.Year).ToArray();
        }

        private void InsertNoAvailableData(Contest contest)
        {
            Contestant contestant;

            switch (contest.Year)
            {
                case 2022:
                    contest.LogoUrl = "https://upload.wikimedia.org/wikipedia/en/thumb/0/01/Eurovision_2022_Official_Logo.jpg/250px-Eurovision_2022_Official_Logo.jpg";
                    break;

                case 2020:
                    contest.LogoUrl = "https://upload.wikimedia.org/wikipedia/en/thumb/6/6f/Eurovision_Song_Contest_2020.svg/188px-Eurovision_Song_Contest_2020.svg.png";
                    break;

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
