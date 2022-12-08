using System.Data;
using EurovisionDataset.Data.Eurovision.Junior;
using Microsoft.Playwright;

namespace EurovisionDataset.Scrapers.Eurovision.Junior;

public class EurovisionWorld : Eurovision.EurovisionWorld
{
    #region Contest

    protected override string GetContestPageUrl(int year)
    {
        return $"/junior-eurovision/{year}";
    }

    protected override async Task<Data.Contest> GetContestAsync(PlaywrightScraper playwright, int year)
    {
        Contest result = new Contest() { Year = year };
        Dictionary<string, string> contestData = await GetNationalDataAsync(playwright.Page);
        
        FixContestData(contestData);
        SetContestInfo(result, contestData);

        var (contestants, performances) = await GetContestantsAndPerformancesAsync(playwright.Page);

        Round round = new Round()
        {
            Name = "final",
            Date = contestData.GetValueOrDefault("date"),
            Performances = performances
        };

        result.Contestants = contestants;
        result.Rounds = new[] { round };

        return result;
    }

    private void FixContestData(Dictionary<string, string> data)
    {
        if (data.TryGetValue("location", out string arena))
        {
            data.Add("arena", arena);
            data.Remove("location");
        }

        if (data.TryGetValue("city", out string city))
        {
            string[] cityAndCountry = city.Split(", ");
            data["city"] = cityAndCountry[0];

            if (cityAndCountry.Length > 1)
                data.Add("country", Utils.GetCountryCode(cityAndCountry[1]));
        }
    }

    protected override void SetContestInfo(Data.Eurovision.Contest contest, Dictionary<string, string> data)
    {
        base.SetContestInfo(contest, data);

        if (data.TryGetValue("arena", out string arena))
            contest.Arena = arena;

        if (data.TryGetValue("city", out string city))
        {
            string[] cityAndCountry = city.Split(", ");
            contest.City = cityAndCountry[0];
            if (cityAndCountry.Length > 1) 
                contest.Country = Utils.GetCountryCode(cityAndCountry[1]);
        }

        if (data.TryGetValue("logo", out string logo))
            contest.LogoUrl = logo;
    }

    #endregion

    private async Task<(IList<Contestant>, IList<Performance>)> GetContestantsAndPerformancesAsync(IPage page)
    {
        IElementHandle table = await page.QuerySelectorAsync(".national_table");
        IReadOnlyList<IElementHandle> headerColumns = await table.QuerySelectorAllAsync("thead tr:last-child th");
        string[] headers = await Task.WhenAll(headerColumns.Select(e =>
            e.InnerTextAsync().ContinueWithResult(s => s.ToLower())));

        IReadOnlyList<IElementHandle> rows = await table.QuerySelectorAllAsync("tbody tr");
        List<Contestant> contestants = new List<Contestant>(rows.Count);
        List<Performance> performances = new List<Performance>(rows.Count);

        for (int i = 0; i < rows.Count; i++)
        {
            IElementHandle row = rows[i];
            IReadOnlyList<IElementHandle> columns = await row.QuerySelectorAllAsync("td");

            Contestant contestant = new Contestant() { Id = i };
            await GetContestantAsync(columns[2], contestant);
            contestants.Add(contestant);

            Performance performance = await GetPerformanceAsync(headers, columns);
            if (performance != null)
            {
                performance.ContestantId = i;
                performances.Add(performance);
            }
        }

        return (contestants, performances);
    }

    #region Contestant
    
    protected override void SetContestantData(Dictionary<string, string> data, Data.Contestant contestant)
    {
        base.SetContestantData(data, contestant);

        Contestant contestantAux = (Contestant)contestant;

        if (data.TryGetValue("country", out string country))
            contestantAux.Country = Utils.GetCountryCode(country);
    }

    #endregion

    #region Performance

    private async Task<Performance> GetPerformanceAsync(string[] headers, IReadOnlyList<IElementHandle> columns)
    {
        Performance performance;

        try
        {
            int place = int.Parse(await columns[0].InnerTextAsync());
            int running = int.Parse(await columns[columns.Count - 2].InnerTextAsync());

            performance = new Performance()
            {
                Running = running,
                Place = place,
                Scores = await GetScoresAsync(headers, columns)
            };
        }
        catch
        {
            performance = null;
        }

        return performance;
    }

    private async Task<IList<Score>> GetScoresAsync(string[] headers, IReadOnlyList<IElementHandle> columns)
    {
        List<Score> result = new List<Score>();

        for (int i = 3; i < columns.Count - 2; i++)
        {
            string name = headers[i];
            if (name == "points") name = "total";
            int points = int.Parse(await columns[i].InnerTextAsync());

            result.Add(new Score() { Name = name, Points = points });
        }

        return result;
    }

    #endregion
}
