using EurovisionDataset.Data;
using Microsoft.Playwright;

namespace EurovisionDataset.Scrapers;

public class Eschome
{
    private const string URL = "https://eschome.net/";

    public async Task<Contest[]> GetContestsAsync(int start, int end)
    {
        using PlaywrightScraper playwright = new PlaywrightScraper();
        await GoToContestsTable(playwright);

        return await GetContestsFromTableAsync(playwright, start, end);
    }

    private async Task GoToContestsTable(PlaywrightScraper playwright)
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

    private async Task<Contest[]> GetContestsFromTableAsync(PlaywrightScraper playwright, int start, int end)
    {
        List<Contest> result = new List<Contest>();
        IReadOnlyList<IElementHandle> rows = await playwright.Page.QuerySelectorAllAsync("#tabelle1 tbody tr");

        for (int i = 0; i < rows.Count; i += 2)
        {
            IElementHandle top = rows[i];
            IElementHandle buttom = rows[i + 1];

            IReadOnlyList<IElementHandle> topColumns = await top.QuerySelectorAllAsync("td");
            IReadOnlyList<IElementHandle> buttomColumns = await buttom.QuerySelectorAllAsync("td");

            int year = int.Parse(await topColumns[0].InnerTextAsync());

            if (start <= year && year <= end)
            {
                Contest contest = new Contest()
                {
                    Year = year,
                    Country = await GetCountry(topColumns[2]),
                    City = await topColumns[3].InnerTextAsync(),
                    Arena = await topColumns[4].InnerTextAsync(),
                    Broadcasters = new[] { await topColumns[5].InnerTextAsync() },
                    Presenters = (await buttomColumns[4].InnerTextAsync()).Split(", "),
                };

                contest.Contestants = await GetContestantsAsync(contest.Year);
                result.Add(contest);
            }
        }

        return result.ToArray();
    }

    private async Task<Contestant[]> GetContestantsAsync(int year)
    {
        using PlaywrightScraper playwright = new PlaywrightScraper();
        await GoToContestantsTable(playwright, year);

        return await GetContestantsFromTableAsync(playwright);
    }

    private async Task GoToContestantsTable(PlaywrightScraper playwright, int year)
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

    private async Task<Contestant[]> GetContestantsFromTableAsync(PlaywrightScraper playwright)
    {
        List<Contestant> result = new List<Contestant>();
        IReadOnlyList<IElementHandle> rows = await playwright.Page.QuerySelectorAllAsync("#tabelle1 tbody tr");

        for (int i = 0; i < rows.Count; i += 2)
        {
            IElementHandle top = rows[i];
            IElementHandle buttom = rows[i + 1];

            IReadOnlyList<IElementHandle> topRow = await top.QuerySelectorAllAsync("td");
            IReadOnlyList<IElementHandle> buttomRow = await buttom.QuerySelectorAllAsync("td");

            Contestant contestant = new Contestant()
            {
                Country = await GetCountry(topRow[1]),
                Artist = await topRow[2].InnerTextAsync(),
                Song = await topRow[3].InnerTextAsync(),
                Composers = (await buttomRow[2].InnerTextAsync()).Split(", "),
                Writers = (await buttomRow[3].InnerTextAsync()).Split(", "),
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
}
