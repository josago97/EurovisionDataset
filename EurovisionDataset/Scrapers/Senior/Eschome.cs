using EurovisionDataset.Data.Senior;
using Microsoft.Playwright;

namespace EurovisionDataset.Scrapers.Senior;

public class Eschome
{
    private const string URL = "https://eschome.net/";

    public async Task GetContestsInfoAsync(IList<Contest> contests)
    {
        using PlaywrightScraper playwright = new PlaywrightScraper();

        await GoToContestsTableAsync(playwright);
        IList<ContestData> contestsData = await GetContestsDataFromTableAsync(playwright);

        foreach (Contest contest in contests)
        {
            int year = contest.Year;
            if (year == 2020) year = 2021; // 2020 was cancelled, so 2021 data are equal to 2020 data
            ContestData data = contestsData.FirstOrDefault(c => c.Year == year);

            if (data != null)
            {
                contest.Country = data.Country;
                contest.City = data.City;
                contest.Arena = data.Location;
                contest.Presenters = data.Presenters;
            }

            await GetContestantsInfoAsync(playwright, year, contest.Contestants.Cast<Contestant>());
        }
    }

    private async Task GoToContestsTableAsync(PlaywrightScraper playwright)
    {
        await playwright.LoadPageAsync(URL);

        IElementHandle submit = await playwright.Page.WaitForSelectorAsync("#submit0");
        IElementHandle dropdown = await submit.QuerySelectorAsync("select");
        await dropdown.SelectOptionAsync(new[] { "0" });
        IElementHandle checkbox = await submit.QuerySelectorAsync("input");
        await checkbox.SetCheckedAsync(true);

        await submit.ClickAsync();
        await playwright.Page.WaitForLoadStateAsync(LoadState.Load);
    }

    private async Task<IList<ContestData>> GetContestsDataFromTableAsync(PlaywrightScraper playwright)
    {
        List<ContestData> result = new List<ContestData>();
        IReadOnlyList<IElementHandle> rows = await playwright.Page.QuerySelectorAllAsync("#tabelle1 tbody tr");

        for (int i = 0; i < rows.Count; i += 2)
        {
            IElementHandle top = rows[i];
            IElementHandle buttom = rows[i + 1];

            IReadOnlyList<IElementHandle> topColumns = await top.QuerySelectorAllAsync("td");
            IReadOnlyList<IElementHandle> buttomColumns = await buttom.QuerySelectorAllAsync("td");

            ContestData contest = new ContestData()
            {
                Year = int.Parse(await topColumns[0].InnerTextAsync()),
                Country = await GetCountry(topColumns[2]),
                City = await topColumns[3].InnerTextAsync(),
                Location = await topColumns[4].InnerTextAsync(),
                Broadcasters = new[] { await topColumns[5].InnerTextAsync() },
                Presenters = (await buttomColumns[4].InnerTextAsync()).Split(", "),
            };

            result.Add(contest);
        }

        return result;
    }

    private async Task GetContestantsInfoAsync(PlaywrightScraper playwright, int year, IEnumerable<Contestant> contestants)
    {
        await GoToContestantsTableAsync(playwright, year);
        IList<ContestantData> contestantsData = await GetContestantsFromTableAsync(playwright);

        foreach (Contestant contestant in contestants)
        {
            // In 1956 each country had 2 contestants
            ContestantData data = contestantsData.FirstOrDefault(c => c.Country.Equals(contestant.Country, StringComparison.OrdinalIgnoreCase));

            if (data != null)
            {
                contestant.Broadcaster = data.Broadcaster;
            }
        }
    }

    private async Task GoToContestantsTableAsync(PlaywrightScraper playwright, int year)
    {
        await playwright.LoadPageAsync(URL);

        IElementHandle submit = await playwright.Page.WaitForSelectorAsync("#submit4");
        IElementHandle dropdown = await submit.QuerySelectorAsync("select");
        await dropdown.SelectOptionAsync(new[] { year.ToString() });
        IElementHandle checkbox = await submit.QuerySelectorAsync("input");
        await checkbox.SetCheckedAsync(true);

        await submit.ClickAsync();
        await playwright.Page.WaitForLoadStateAsync(LoadState.Load);
    }

    private async Task<IList<ContestantData>> GetContestantsFromTableAsync(PlaywrightScraper playwright)
    {
        List<ContestantData> result = new List<ContestantData>();
        IReadOnlyList<IElementHandle> rows = await playwright.Page.QuerySelectorAllAsync("#tabelle1 tbody tr");

        for (int i = 0; i < rows.Count; i += 2)
        {
            IElementHandle top = rows[i];
            IElementHandle buttom = rows[i + 1];

            IReadOnlyList<IElementHandle> topRow = await top.QuerySelectorAllAsync("td");
            IReadOnlyList<IElementHandle> buttomRow = await buttom.QuerySelectorAllAsync("td");

            ContestantData contestant = new ContestantData()
            {
                Country = await GetCountry(topRow[1]),
                //Artist = await topRow[2].InnerTextAsync(),
                //Song = await topRow[3].InnerTextAsync(),
                //Composers = (await buttomRow[2].InnerTextAsync()).Split(", "),
                //Writers = (await buttomRow[3].InnerTextAsync()).Split(", "),
                Broadcaster = await buttomRow[1].InnerTextAsync(),
            };

            result.Add(contestant);
        }

        return result.ToArray();
    }

    private async Task<string> GetCountry(IElementHandle element)
    {
        string countryName = await element.InnerTextAsync();

        if (countryName == "Marocco") countryName = "Morocco";

        return Utils.GetCountryCode(countryName);
    }

    private class ContestData
    {
        public int Year { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public string Location { get; set; }
        public string[] Broadcasters { get; set; }
        public string[] Presenters { get; set; }
    }

    private class ContestantData
    {
        public string Country { get; set; }
        public string Artist { get; set; }
        public string Song { get; set; }
        public string[] Composers { get; set; }
        public string[] Writers { get; set; }
        public string Broadcaster { get; set; }
    }
}
