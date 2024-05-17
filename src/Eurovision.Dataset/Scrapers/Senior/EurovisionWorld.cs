using Eurovision.Dataset.Entities;
using Microsoft.Playwright;
using Sharplus.Tasks;
using Contest = Eurovision.Dataset.Entities.Senior.Contest;
using Contestant = Eurovision.Dataset.Entities.Senior.Contestant;
using Score = Eurovision.Dataset.Entities.Senior.Score;

namespace Eurovision.Dataset.Scrapers.Senior;

public class EurovisionWorld : BaseEurovisionWorld<Contest, Contestant>
{
    private const string CONTESTANT_BROADCASTER_KEY = "broadcaster";
    private const string CONTESTANT_COMMENTATOR_KEY = "commentator";
    private const string CONTESTANT_CONDUCTOR_KEY = "conductor";
    private const string CONTESTANT_JURY_KEY = "jury member";
    private const string CONTESTANT_SPOKESPERSON_KEY = "spokesperson";
    private const string CONTESTANT_STAGE_DIRECTOR_KEY = "stage director";

    protected override string ContestListUrl => "/eurovision";

    #region Contest

    protected override string GetContestPageUrl(int year)
    {
        return $"/eurovision/{year}";
    }

    protected override async Task GetContestDataAsync(IPage page, Dictionary<string, string> data)
    {
        string selector = "div.voting_info:not(.voting_info_1956)";
        IElementHandle contestDataElement = await page.QuerySelectorAsync(selector);
        string contestData = await contestDataElement.InnerTextAsync();

        foreach (string line in contestData.Split("\n"))
        {
            if (!string.IsNullOrEmpty(line))
            {
                string[] keyAndValue = line.Split(": ");

                if (keyAndValue.Length > 1)
                {
                    string key = keyAndValue[0].Trim();
                    string value = keyAndValue[1].Trim();

                    AddData(data, key, value);
                }
            }
        }
    }

    protected override void SetContestData(Contest contest, Dictionary<string, string> data)
    {
        base.SetContestData(contest, data);

        if (data.TryGetValue("broadcaster", out string broadcasters))
            contest.Broadcasters = broadcasters.Split(", ");
    }
    /*
    private async Task<string[][]> GetContestDataLinesAsync(IPage page)
    {
        string selector = "div.voting_info:not(.voting_info_1956)";
        IElementHandle contestDataElement = await page.QuerySelectorAsync(selector);
        string contestData = await contestDataElement.InnerTextAsync();

        return contestData.Split("\n").Select(line => line.Split(": ")).ToArray();
    } */

    #endregion


    #region Contestant

    protected override async Task<IElementHandle> GetContestantsTableAsync(IPage page)
    {
        return await page.QuerySelectorAsync("#voting_table");
    }

    protected override async Task GetContestantDataAsync(IElementHandle row, IPage page, Dictionary<string, string> data)
    {
        await GetContestantDataAsync(page, data);
        // Sobrescribir el artista para que coincida con el del título
        await GetArtistAndSongAsync(page, data);

        AddData(data, CONTESTANT_COUNTRY_KEY, await GetCountryCodeAsync(row));
    }

    private async Task GetArtistAndSongAsync(IPage page, Dictionary<string, string> data)
    {
        IElementHandle artistAndSongElement = await page.QuerySelectorAsync("h1.mm");

        string[] artistAndSong = (await artistAndSongElement.InnerTextAsync()).Split("\n")
            .Last().Split('-').Select(s => s.Trim()).ToArray();

        AddData(data, CONTESTANT_SONG_KEY, artistAndSong[1].Trim(' ', '\"'));
        AddData(data, CONTESTANT_ARTIST_KEY, artistAndSong[0].Trim());
    }

    private async Task GetContestantDataAsync(IPage page, Dictionary<string, string> data)
    {
        IReadOnlyList<IElementHandle> elements = await page.QuerySelectorAllAsync("div.lyr_inf div div div");

        for (int i = 0; i < elements.Count; i += 2)
        {
            string key = await elements[i].InnerTextAsync();
            string value = await elements[i + 1].InnerTextAsync();

            AddData(data, key, value);
        }

        string selector = ".people_wrap.mm .people_category";
        IReadOnlyList<IElementHandle> peopleTable = await page.QuerySelectorAllAsync(selector);

        foreach (IElementHandle element in peopleTable)
        {
            string key = await element.QuerySelectorAsync("h4.label")
                .ContinueWithResult(e => e.InnerTextAsync());

            string value = await element.QuerySelectorAllAsync("li b")
                .ContinueWithResult(a => a.Select(e => e.InnerTextAsync()))
                .ContinueWithResult(a => string.Join(", ", a));

            AddData(data, key, value);
        }
    }

    protected override void SetContestantData(Contestant contestant, Dictionary<string, string> data)
    {
        base.SetContestantData(contestant, data);

        if (data.TryGetValue(CONTESTANT_BROADCASTER_KEY, out string broadcaster))
            contestant.Broadcaster = broadcaster;

        if (data.TryGetValue(CONTESTANT_COMMENTATOR_KEY, out string commentators))
            contestant.Commentators = SplitData(commentators);

        if (data.TryGetValue(CONTESTANT_CONDUCTOR_KEY, out string conductor))
            contestant.Conductor = conductor;

        if (data.TryGetValue(CONTESTANT_JURY_KEY, out string juryMembers))
            contestant.Jury = SplitData(juryMembers);

        if (data.TryGetValue(CONTESTANT_SPOKESPERSON_KEY, out string spokesperson))
            contestant.Spokesperson = spokesperson;

        if (data.TryGetValue(CONTESTANT_STAGE_DIRECTOR_KEY, out string stageDirector))
            contestant.StageDirector = stageDirector;
    }

    #endregion

    #region Round

    protected override async Task<IReadOnlyList<Round>> GetRoundsAsync(PlaywrightScraper playwright, int year,
        IReadOnlyDictionary<string, string> contestData, IReadOnlyList<Contestant> contestants)
    {
        List<Round> result = new List<Round>();
        Dictionary<string, string> contestDataAux = new Dictionary<string, string>();

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
                await GetContestDataAsync(playwright.Page, contestDataAux);
                Round round = await GetRoundAsync(playwright.Page, year, roundName, contestDataAux, contestants);
                result.Add(round);
            }
        }

        return result.ToArray();
    }

    private async Task<Round> GetRoundAsync(IPage page, int year, string roundName,
        IReadOnlyDictionary<string, string> contestData, IReadOnlyList<Contestant> contestants)
    {
        (DateOnly Date, TimeOnly? Time) dateTime = GetDateAndTime(year, contestData);

        return new Round()
        {
            Name = roundName.Replace("-", ""),
            Date = dateTime.Date,
            Time = dateTime.Time,
            Performances = await GetPerformancesAsync(page, year, contestants)
        };
    }

    private (DateOnly, TimeOnly?) GetDateAndTime(int year, IReadOnlyDictionary<string, string> contestData)
    {
        (DateOnly Date, TimeOnly? Time) dateTime = GetDateAndTime(contestData);

        if (!dateTime.Time.HasValue && year >= 1963)
            dateTime.Time = new TimeOnly(19, 0);

        return dateTime;
    }

    private async Task<IReadOnlyList<Performance>> GetPerformancesAsync(IPage page, int year,
        IReadOnlyList<Contestant> contestants)
    {
        List<Performance> result = new List<Performance>();

        string selector = "#voting_table .v_table_main tbody tr";
        IReadOnlyList<IElementHandle> rows = await page.QuerySelectorAllAsync(selector);
        Dictionary<string, List<Score>> scores = await GetAllScoresAsync(page);

        foreach (IElementHandle row in rows)
        {
            Performance performance = await GetPerformanceAsync(row, year, contestants, scores);
            if (performance != null) result.Add(performance);
        }

        return result;
    }

    private async Task<Dictionary<string, List<Score>>> GetAllScoresAsync(IPage page)
    {
        Dictionary<string, List<Score>> result = new Dictionary<string, List<Score>>();
        string buttonSelector = ".scoreboard_button_div button";
        IReadOnlyList<ILocator> buttons = await page.Locator(buttonSelector).AllAsync();

        if (buttons == null || buttons.Count <= 1)
        {
            await GetScoresFromScoreboardAsync(page, "total", result);
        }
        else
        {
            foreach (ILocator button in buttons)
            {
                string scoreName = (await button.InnerTextAsync()).ToLower();
                await button.ClickAsync();
                await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
                await GetScoresFromScoreboardAsync(page, scoreName, result);
            }
        }

        return result;
    }

    private async Task GetScoresFromScoreboardAsync(IPage page, string scoreName, Dictionary<string, List<Score>> allScores)
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

            if (allScores.TryGetValue(countryCode, out List<Score> scores))
                scores.Add(score);
            else
            {
                allScores.Add(countryCode, scores = new List<Score>());
                scores.Add(score);
            }
        }
    }

    private async Task<Performance> GetPerformanceAsync(IElementHandle row, int year, IReadOnlyList<Contestant> contestants, Dictionary<string, List<Score>> scores)
    {
        Performance result = null;
        string countryCode = await GetCountryCodeAsync(row);
        IReadOnlyList<IElementHandle> columns = await row.QuerySelectorAllAsync("td");

        if (year == 1956)
        {
            string song = (await columns[2].InnerTextAsync()).Split("\n")[0].Trim();

            result = new Performance()
            {
                ContestantId = contestants.First(c => c.Country == countryCode
                        && c.Song.Equals(song, StringComparison.OrdinalIgnoreCase)).Id,
                Scores = new Score[0]
            };
        }
        else if (scores.ContainsKey(countryCode))
        {
            result = new Performance()
            {
                ContestantId = contestants.First(c => c.Country == countryCode).Id,
                Scores = scores[countryCode]
            };
        }

        if (result != null)
        {
            result.Place = int.Parse(await columns[0].InnerTextAsync());
            result.Running = int.Parse(await columns[columns.Count - 1].InnerTextAsync());
        }

        return result;
    }

    #endregion

    #region Utils

    private async Task<string> GetCountryCodeAsync(IElementHandle row)
    {
        return (await row.GetAttributeAsync("id"))
            .Split('_').Last().Substring(0, 2).ToUpper();
    }

    #endregion
}
