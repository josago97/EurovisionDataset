using System.Text.RegularExpressions;
using EurovisionDataset.Data.Senior;
using Microsoft.Playwright;

namespace EurovisionDataset.Scrapers.Eurovision.Senior;

public class EurovisionWorld2 : EurovisionWorld2<Contest, Contestant>
{
    private const string CONTESTANT_BROADCASTER_KEY = "broadcaster";
    private const string CONTESTANT_COMMENTATOR_KEY = "commentator";
    private const string CONTESTANT_CONDUCTOR_KEY = "conductor";
    private const string CONTESTANT_JURY_KEY = "jury member";
    private const string CONTESTANT_SPOKEPERSON_KEY = "spokeperson";
    private const string CONTESTANT_STAGE_DIRECTOR_KEY = "stage director";

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
                string[] keyAndValue = line.Split(':');

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

    #endregion


    #region Contestant

    protected override async Task<IElementHandle> GetContestantsTableAsync(IPage page)
    {
        return await page.QuerySelectorAsync("#voting_table");
    }

    protected override async Task GetContestantDataAsync(IElementHandle row, IPage page, Dictionary<string, string> data)
    {
        await GetArtistAndSongAsync(page, data);
        await GetContestantDataAsync(page, data);

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

        if (data.TryGetValue(CONTESTANT_SPOKEPERSON_KEY, out string spokeperson))
            contestant.Spokesperson = spokeperson;

        if (data.TryGetValue(CONTESTANT_STAGE_DIRECTOR_KEY, out string stageDirector))
            contestant.StageDirector = stageDirector;
    }

    #endregion

    #region Round

    protected override async Task<IReadOnlyList<Data.Round>> GetRoundsAsync(PlaywrightScraper playwright, int year, Dictionary<string, string> contestData, IReadOnlyList<Contestant> contestants)
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
                Round round = await GetRoundAsync(playwright.Page, year, roundName, contestData, contestants);
                result.Add(round);
            }
        }

        return result.ToArray();
    }

    private async Task<Round> GetRoundAsync(IPage page, int year, string roundName, Dictionary<string, string> contestData, IReadOnlyList<Contestant> contestants)
    {
        return new Round()
        {
            Name = roundName.Replace("-", ""),
            Date = GetDate(contestData),
            Performances = await GetPerformancesAsync(page, year, contestants)
        };
    }

    private string GetDate(Dictionary<string, string> contestData)
    {
        string result = null;

        if (contestData.TryGetValue("date", out string date))
        {
            Regex regex = new Regex(@"[0-9]*:"); //Para quitar la hora y ponerla bien
            Match match = regex.Match(date);
            result = $"{(match.Success ? date.Substring(0, match.Index) : date).Trim()} 21:00 +2";
        }

        return result;
    }

    private async Task<IReadOnlyList<Performance>> GetPerformancesAsync(IPage page, int year, IReadOnlyList<Contestant> contestants)
    {
        List<Performance> result = new List<Performance>();

        string selector = "#voting_table .v_table_main tbody tr";
        IReadOnlyList<IElementHandle> rows = await page.QuerySelectorAllAsync(selector);
        /*Dictionary<string, IReadOnlyList<Score>> scores = await GetAllScoresAsync(page);

        foreach (IElementHandle row in rows)
        {
            Performance performance = new Performance();
            string countryCode = await GetCountryCodeAsync(row);
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
                performance.Scores = scores[countryCode];
            }

            performance.Place = int.Parse(await columns[0].InnerTextAsync());
            performance.Running = int.Parse(await columns[columns.Count - 1].InnerTextAsync());

            result.Add(performance);
        }*/

        return result;
    }

    private async Task<Dictionary<string, IList<Score>>> GetAllScoresAsync(IPage page)
    {
        Dictionary<string, IList<Score>> result = new Dictionary<string, IList<Score>>();
        string buttonSelector = ".scoreboard_button_div button";
        IReadOnlyList<IElementHandle> buttons = await page.QuerySelectorAllAsync(buttonSelector);

        if (buttons == null || buttons.Count <= 1)
        {
            await GetScoresFromScoreboardAsync(page, "total", result);
        }
        else
        {
            foreach (IElementHandle button in buttons)
            {
                string scoreName = (await button.InnerTextAsync()).ToLower();
                await button.ClickAsync();
                await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
                await GetScoresFromScoreboardAsync(page, scoreName, result);
                buttons = await page.QuerySelectorAllAsync(buttonSelector);
            }
        }

        return result;
    }

    private async Task GetScoresFromScoreboardAsync(IPage page, string scoreName, Dictionary<string, IList<Score>> allScores)
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

    #region Utils

    private async Task<string> GetCountryCodeAsync(IElementHandle row)
    {
        return (await row.GetAttributeAsync("id"))
            .Split('_').Last().Substring(0, 2).ToUpper();
    }

    #endregion
}
