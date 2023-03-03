using System.Text.RegularExpressions;
using EurovisionDataset.Data.Senior;
using Microsoft.Playwright;

namespace EurovisionDataset.Scrapers.Eurovision.Senior;

public class EurovisionWorld : Eurovision.EurovisionWorld
{
    private const string BROADCASTERS_KEY = "broadcaster";

    #region Contest

    protected override string GetContestPageUrl(int year)
    {
        return $"/eurovision/{year}";
    }

    protected override async Task<Data.Contest> GetContestAsync(PlaywrightScraper playwright, int year)
    {
        Contest result = new Contest() { Year = year };
        Dictionary<string, string> contestData = await GetContestDataAsync(playwright.Page);
        SetContestInfo(result, contestData);

        IList<Contestant> contestants = await GetContestantsAsync(playwright.Page, year);
        result.Contestants = contestants;
        result.Rounds = await GetRoundsAsync(playwright, year, contestants);

        return result;
    }

    private async Task<Dictionary<string, string>> GetContestDataAsync(IPage page)
    {
        Dictionary<string, string> result = new Dictionary<string, string>();
        IElementHandle contestInfoElement = await GetContestInfoElementAsync(page);
        string contestInfo = await contestInfoElement.InnerTextAsync();

        foreach (string line in contestInfo.Split("\n"))
        {
            if (!string.IsNullOrEmpty(line))
            {
                string[] data = line.Split(':');

                if (data.Length > 1)
                {
                    string key = data[0].Trim();
                    string value = data[1].Trim();

                    AddData(result, key, value);
                }
            }
        }

        return result;
    }

    protected override void SetContestInfo(Data.Contest contest, Dictionary<string, string> data)
    {
        base.SetContestInfo(contest, data);

        Contest seniorContest = contest as Contest;

        if (data.TryGetValue(BROADCASTERS_KEY, out string broadcasters))
            seniorContest.Broadcasters = broadcasters.Split(", ");
    }

    #endregion

    #region Contestant

    private async Task<IList<Contestant>> GetContestantsAsync(IPage page, int year)
    {
        List<Contestant> result = new List<Contestant>();
        IReadOnlyList<IElementHandle> tables = await page.QuerySelectorAllAsync("#voting_table");

        foreach (IElementHandle table in tables)
        {
            IReadOnlyList<IElementHandle> rows = await table.QuerySelectorAllAsync("tbody tr");

            for (int i = 0; i < rows.Count; i++)
            {
                IElementHandle row = rows[i];
                IReadOnlyList<IElementHandle> linkElements = await row.QuerySelectorAllAsync("a");
                string countryCode = await GetCountryCodeAsync(row, year);

                Contestant contestant = new Contestant() { Id = i, Country = countryCode };
                await GetContestantAsync(linkElements[1], contestant);
                result.Add(contestant);
            }
        }

        return result.ToArray();
    }

    protected override void SetContestantData(Dictionary<string, string> data, Data.Contestant contestant)
    {
        base.SetContestantData(data, contestant);

        Contestant contestantAux = (Contestant)contestant;

        if (data.TryGetValue("broadcaster", out string broadcaster))
            contestantAux.Broadcaster = broadcaster;

        if (data.TryGetValue("spokesperson", out string spokesperson))
            contestantAux.Spokesperson = spokesperson;

        if (data.TryGetValue("commentator", out string commentator))
            contestantAux.Commentators = commentator.Split(", ");
    }

    #endregion

    #region Round

    private async Task<Round[]> GetRoundsAsync(PlaywrightScraper playwright, int year, IList<Contestant> contestants)
    {
        List<Round> result = new List<Round>();

        string[] roundNames = year switch
        {
            < 2004 => new[] { "final" },
            < 2008 => new[] { "final", "semi-final" },
            2020 => new string[0],
            _ => new[] { "final", "semi-final-1", "semi-final-2" }
        };

        foreach (string roundName in roundNames)
        {
            string url = $"/eurovision/{year}";
            if (roundName != "final") url += $"/{roundName}";

            if (playwright.Page.Url == url || await LoadPageAsync(playwright, url))
            {
                Round round = await GetRoundAsync(playwright.Page, year, roundName, contestants);
                result.Add(round);
            }
        }

        return result.ToArray();
    }

    private async Task<Round> GetRoundAsync(IPage page, int year, string round, IList<Contestant> contestants)
    {
        string date = await GetDateAsync(page);
        Regex regex = new Regex(@"[0-9]*:"); //Para quitar la hora y ponerla bien
        Match match = regex.Match(date);
        date = $"{(match.Success ? date.Substring(0, match.Index) : date).Trim()} 21:00 +2";

        return new Round()
        {
            Name = round.Replace("-", ""),
            Date = date,
            Performances = await GetPerformancesAsync(page, year, contestants)
        };
    }

    private async Task<string> GetDateAsync(IPage page)
    {
        IElementHandle contestInfoElement = await GetContestInfoElementAsync(page);
        string contestInfo = await contestInfoElement.InnerTextAsync();

        string result = contestInfo.Split("\n")
            .FirstOrDefault(s => s.StartsWith("Date"))
            ?.Split(":")[1];

        return result ?? string.Empty;
    }

    private async Task<Performance[]> GetPerformancesAsync(IPage page, int year, IList<Contestant> contestants)
    {
        List<Performance> result = new List<Performance>();

        string selector = "#voting_table .v_table_main tbody tr";
        IReadOnlyList<IElementHandle> rows = await page.QuerySelectorAllAsync(selector);
        Dictionary<string, IList<Score>> scores = await GetAllScoresAsync(page);

        foreach (IElementHandle row in rows)
        {
            Performance performance = new Performance();
            string countryCode = await GetCountryCodeAsync(row, year);
            IReadOnlyList<IElementHandle> columns = await row.QuerySelectorAllAsync("td");

            if (year == 1956)
            {
                string song = (await columns[2].InnerTextAsync()).Split("\n")[0].Trim();

                performance.ContestantId = contestants.First(c => c.Country == countryCode
                            && c.Song.Equals(song, StringComparison.OrdinalIgnoreCase)).Id;

                performance.Scores = new Score[0];
            }
            else
            {
                performance.ContestantId = contestants.First(c => c.Country == countryCode).Id;
                performance.Scores = scores[countryCode].ToArray();
            }

            performance.Place = int.Parse(await columns[0].InnerTextAsync());
            performance.Running = int.Parse(await columns[columns.Count - 1].InnerTextAsync());

            result.Add(performance);
        }

        return result.ToArray();
    }

    private async Task<Dictionary<string, IList<Score>>> GetAllScoresAsync(IPage page)
    {
        Dictionary<string, IList<Score>> result = new Dictionary<string, IList<Score>>();
        string buttonSelector = ".scoreboard_button_div button";
        IReadOnlyList<IElementHandle> buttons = await page.QuerySelectorAllAsync(buttonSelector);

        if (buttons == null || buttons.Count <= 1)
        {
            await GetScoresAsync(page, "total", result);
        }
        else
        {
            int buttonsCount = buttons.Count;

            for (int i = 0; i < buttonsCount; i++)
            {
                IElementHandle button = buttons[i];
                string scoreName = (await button.InnerTextAsync()).ToLower();
                await button.ClickAsync();
                await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
                await GetScoresAsync(page, scoreName, result);
                buttons = await page.QuerySelectorAllAsync(buttonSelector);
            }
        }

        return result;
    }

    private async Task GetScoresAsync(IPage page, string scoreName, Dictionary<string, IList<Score>> allScores)
    {
        string selector = "table.scoreboard_table tbody tr";
        IReadOnlyList<IElementHandle> rows = await page.QuerySelectorAllAsync(selector);

        foreach (IElementHandle row in rows)
        {
            string countryCode = (await row.GetAttributeAsync("id")).Split("_").Last().ToUpper();
            IReadOnlyList<IElementHandle> columns = await row.QuerySelectorAllAsync("td[data-to]");
            Score score = new Score()
            {
                Name = scoreName,
                Points = int.Parse(await columns[3].InnerTextAsync()),
                Votes = new Dictionary<string, int>()
            };

            for (int i = 4; i < columns.Count; i++)
            {
                IElementHandle column = columns[i];
                string fromCountry = (await column.GetAttributeAsync("data-from")).ToUpper();

                if (countryCode != fromCountry)
                {
                    int.TryParse(await column.InnerTextAsync(), out int points);
                    score.Votes.Add(fromCountry, points);
                }
            }

            if (allScores.TryGetValue(countryCode, out IList<Score> scores))
                scores.Add(score);
            else
            {
                allScores.Add(countryCode, scores = new List<Score>());
                scores.Add(score);
            }
        }
    }

    #endregion

    private async Task<IElementHandle> GetContestInfoElementAsync(IPage page)
    {
        string selector = "div.voting_info:not(.voting_info_1956)";

        return await page.QuerySelectorAsync(selector);
    }

    private async Task<string> GetCountryCodeAsync(IElementHandle row, int year)
    {
        string result = (await row.GetAttributeAsync("id")).Split('_').Last().ToUpper();

        if (year == 1956)
        {
            result = result.Substring(0, 2);
        }

        return result;
    }
}
