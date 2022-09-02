using System.Text;
using System.Text.RegularExpressions;
using EurovisionDataset.Data;
using Microsoft.Playwright;

namespace EurovisionDataset.Scrapers;

public class EurovisionWorld
{
    private const string URL = "https://eurovisionworld.com";

    //private enum TableHeader { None, Place, Country, Song, Artist, Song_Artist, Points, Running };

    public async Task RemovePopUpAsync()
    {
        using PlaywrightScraper playwright = new PlaywrightScraper();
        await playwright.LoadPageAsync(URL, WaitUntilState.NetworkIdle);

        string selector = "popup_follow_close";
        IElementHandle popUpElement = await playwright.Page.QuerySelectorAsync(selector);

        if (popUpElement != null) await popUpElement.ClickAsync();
    }

    public async Task GetContestAsync(Contest contest)
    {
        using PlaywrightScraper playwright = new PlaywrightScraper();

        if (await LoadContestPageAsync(playwright, contest.Year))
        {
            contest.Slogan = (await GetContestInfoAsync(playwright, "Slogan"))[0];

            foreach (Contestant contestant in contest.Contestants)
            {
                string countryName = Utils.GetCountryName(contestant.Country);
                await GetContestantInfoAsync(playwright, contest.Year, countryName, contestant);
            }

            contest.Rounds = await GetRoundsAsync(playwright, contest.Year, contest.Contestants);
        }
    }

    public async Task<Contest> GetContestAsync(int year)
    {
        Contest result = null;
        using PlaywrightScraper playwright = new PlaywrightScraper();

        if (await LoadContestPageAsync(playwright, year))
        {
            result = new Contest() { Year = year };

            string[] headers = new[] { "Location", "Arena", "Broadcaster", "Host", "Slogan" };
            string[] contestInfo = await GetContestInfoAsync(playwright, headers);
            await GetLocationAsync(playwright, contestInfo[0], contestInfo[1], result);
            result.Broadcasters = contestInfo[2]?.Split(", ");
            result.Presenters = contestInfo[3]?.Replace(" and ", ", ")?.Split(", ");
            result.Slogan = contestInfo[4];
            result.Contestants = await GetContestantsAsync(playwright, year);
            result.Rounds = await GetRoundsAsync(playwright, year, result.Contestants);
        }

        return result;
    }

    private async Task<bool> LoadContestPageAsync(PlaywrightScraper playwright, int year)
    {
        string url = $"{URL}/eurovision/{year}";

        return await LoadPageAsync(playwright, url);
    }

    private async Task<bool> LoadPageAsync(PlaywrightScraper playwright, string url)
    {
        await playwright.LoadPageAsync(url, WaitUntilState.DOMContentLoaded);

        return playwright.Page.Url.Equals(url, StringComparison.OrdinalIgnoreCase);
    }

    private async Task<IElementHandle> GetContestInfoElementAsync(PlaywrightScraper playwright)
    {
        string selector = "div.voting_info:not(.voting_info_1956)";

        return await playwright.Page.QuerySelectorAsync(selector);
    }

    private async Task<string[]> GetContestInfoAsync(PlaywrightScraper playwright, params string[] headers)
    {
        string[] result = new string[headers.Length];

        IElementHandle contestInfoElement = await GetContestInfoElementAsync(playwright);
        string contestInfo = await contestInfoElement.InnerTextAsync();

        foreach (string line in contestInfo.Split("\n"))
        {
            for (int i = 0; i < headers.Length; i++)
            {
                string header = headers[i];

                if (line.Contains(header, StringComparison.OrdinalIgnoreCase))
                    result[i] = string.Join(':', line.Split(':').Skip(1)).Trim();
            }
        }

        return result;
    }

    private async Task GetLocationAsync(PlaywrightScraper playwright, string locationLine, string arenaLine, Contest contest)
    {
        string[] location = locationLine.Split(',');
        contest.Country = Utils.GetCountryCode(location[location.Length - 1].Trim());

        if (location.Length > 2)
        {
            contest.Arena = location[0].Trim();
            contest.City = location[1].Trim();
        }
        else if (location.Length == 2 && !string.IsNullOrEmpty(arenaLine))
        {
            contest.Arena = arenaLine.Split(":")[1].Trim();
            contest.City = location[0].Trim();
        }
        else
        {
            IElementHandle contestInfoElement = await GetContestInfoElementAsync(playwright);
            IElementHandle eventInfoElement = await contestInfoElement.QuerySelectorAsync("a[href*=\"#city\"]");

            if (eventInfoElement == null)
            {
                if (contest.Country == "LU")
                {
                    contest.Arena = location[0].Trim();
                    contest.City = "Luxembourg";
                }
                else
                    contest.City = location[0].Trim();
            }
            else
            {
                string cityUrl = await eventInfoElement.GetAttributeAsync("href");
                using PlaywrightScraper playwrightLocation = new PlaywrightScraper();
                string url = $"{URL}{cityUrl}";
                await LoadPageAsync(playwrightLocation, url);

                contest.Arena = await playwrightLocation.Page.WaitForSelectorAsync("#arena")
                        .ContinueWithResult(e => e.InnerTextAsync())
                        .ContinueWithResult(s => s.Split(':')[1].Trim());

                contest.City = await playwrightLocation.Page.WaitForSelectorAsync("#city")
                        .ContinueWithResult(e => e.InnerTextAsync())
                        .ContinueWithResult(s => s.Split(':')[1].Trim());
            }
        }
    }

    private async Task<Contestant[]> GetContestantsAsync(PlaywrightScraper playwright, int year)
    {
        List<Contestant> result = new List<Contestant>();
        IReadOnlyList<IElementHandle> tables = await playwright.Page.QuerySelectorAllAsync("#voting_table");

        foreach (IElementHandle table in tables)
        {
            IReadOnlyList<IElementHandle> rows = await table.QuerySelectorAllAsync("tbody tr");

            foreach (IElementHandle row in rows)
            {
                string countryCode = (await row.GetAttributeAsync("id")).Split('_').Last().ToUpper();
                string countryName = Utils.GetCountryName(countryCode);

                Contestant contestant = new Contestant() { Country = countryCode };
                await GetContestantInfoAsync(playwright, year, countryName, contestant);
                result.Add(contestant);
            }
        }

        return result.ToArray();
    }

    private async Task GetContestantInfoAsync(PlaywrightScraper playwright, int year, string country, Contestant contestant)
    {
        if (year == 1956)
        {
            string selector = "#voting_table tbody tr";
            IReadOnlyList<IElementHandle> rows = await playwright.Page.QuerySelectorAllAsync(selector);

            foreach (IElementHandle row in rows)
            {
                string countryCode = (await row.GetAttributeAsync("id")).Split('_').Last().ToUpper();
                IReadOnlyList<IElementHandle> columns = await row.QuerySelectorAllAsync("td");
                string song = (await columns[2].InnerTextAsync()).Split("\n")[0].Trim();

                if (countryCode.Contains(contestant.Country) && song.Equals(contestant.Song, StringComparison.OrdinalIgnoreCase))
                {
                    if (countryCode.Contains('2')) country += '2';
                    break;
                }
            }
        }

        using PlaywrightScraper infoPlaywright = new PlaywrightScraper();
        country = country.Replace(" and ", "-").Replace(' ', '-').ToLower();
        string url = $"{URL}/eurovision/{year}/{country}";
        await LoadPageAsync(infoPlaywright, url);

        if (string.IsNullOrEmpty(contestant.Artist) || string.IsNullOrEmpty(contestant.Song))
            await GetArtistAndSongAsync(infoPlaywright, contestant);

        if (string.IsNullOrEmpty(contestant.VideoUrl))
            await GetVideoUrlAsync(infoPlaywright, contestant);

        if (string.IsNullOrEmpty(contestant.Lyrics))
            await GetLyricsAsync(infoPlaywright, contestant);

        string[] categories = new[] { "Composers", "Lyricists", "Songwriters" };
        string[][] peopleByCategory = await GetPeopleCategoriesAsync(infoPlaywright, categories);

        if (peopleByCategory != null)
        {
            if (contestant.Composers == null || contestant.Composers.Length == 0)
                contestant.Composers = peopleByCategory[0] ?? peopleByCategory[2];

            if (contestant.Writers == null || contestant.Writers.Length == 0)
                contestant.Writers = peopleByCategory[1] ?? peopleByCategory[2];
        }
    }

    private async Task GetArtistAndSongAsync(PlaywrightScraper playwright, Contestant contestant)
    {
        IElementHandle element = await playwright.Page.QuerySelectorAsync("h1.mm.lyrics_header");
        string[] data = (await element.InnerTextAsync()).Split('\n')
            .First(s => s.Contains('-')).Split('-');

        contestant.Artist = data[0].Trim();
        contestant.Song = data[1].Replace("\"", "").Trim();
    }

    private async Task GetLyricsAsync(PlaywrightScraper playwright, Contestant contestant)
    {
        IReadOnlyList<IElementHandle> paragraphs = await playwright.Page.QuerySelectorAllAsync("#lyrics_0 p");

        if (paragraphs != null && paragraphs.Count > 0)
        {
            StringBuilder stringBuilder = new StringBuilder();

            foreach (IElementHandle paragraph in paragraphs)
            {
                stringBuilder.AppendLine(await paragraph.InnerTextAsync());
            }

            contestant.Lyrics = stringBuilder.ToString();
        }
    }

    private async Task GetVideoUrlAsync(PlaywrightScraper playwright, Contestant contestant)
    {
        IElementHandle videoElement = await playwright.Page.QuerySelectorAsync(".vid_ratio iframe");

        if (videoElement != null)
        {
            string videoUrl = await videoElement.GetAttributeAsync("src");
            Regex regex = new Regex(@"\?");
            Match match = regex.Match(videoUrl);
            if (match.Success) videoUrl = videoUrl.Substring(0, match.Index);
            contestant.VideoUrl = videoUrl;
        }
    }

    private async Task<string[][]> GetPeopleCategoriesAsync(PlaywrightScraper playwright, string[] categories)
    {
        string[][] result = new string[categories.Length][];
        string selector = ".people_wrap.mm .people_category";
        IReadOnlyList<IElementHandle> elements = await playwright.Page.QuerySelectorAllAsync(selector);

        foreach (IElementHandle element in elements)
        {
            string name = await element.QuerySelectorAsync("h4.label")
                .ContinueWithResult(e => e.InnerTextAsync());
            int index = categories.FindIndex(c => c.Equals(name, StringComparison.OrdinalIgnoreCase));

            if (index >= 0)
            {
                string[] people = await element.QuerySelectorAllAsync("li b")
                    .ContinueWithResult(async a => await Task.WhenAll(a.Select(e => e.InnerTextAsync())));

                result[index] = people;
            }
        }

        return result;
    }

    private async Task<Round[]> GetRoundsAsync(PlaywrightScraper playwright, int year, Contestant[] contestants)
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
            Round round = await GetRoundAsync(playwright, year, roundName, contestants);
            result.Add(round);
        }

        return result.ToArray();
    }

    private async Task<Round> GetRoundAsync(PlaywrightScraper playwright, int year, string round, Contestant[] contestants)
    {
        Round result = new Round { Name = round.Replace("-", "") };
        string url = $"{URL}/eurovision/{year}{(round == "final" ? "" : $"/{round}")}";

        if (playwright.Page.Url != url) await LoadPageAsync(playwright, url);

        string date = (await GetContestInfoAsync(playwright, "Date"))[0];
        Regex regex = new Regex(@"[0-9]*:");
        Match match = regex.Match(date);
        date = $"{(match.Success ? date.Substring(0, match.Index) : date).Trim()} 21:00 +2";
        result.Date = DateTime.Parse(date);

        result.Performances = await GetPerformancesAsync(playwright, year, contestants);

        return result;
    }

    private async Task<Performance[]> GetPerformancesAsync(PlaywrightScraper playwright, int year, Contestant[] contestants)
    {
        List<Performance> result = new List<Performance>();

        string selector = "#voting_table .v_table_main tbody tr";
        IReadOnlyList<IElementHandle> rows = await playwright.Page.QuerySelectorAllAsync(selector);
        Dictionary<string, IList<Score>> scores = await GetAllScoresAsync(playwright);

        foreach (IElementHandle row in rows)
        {
            Performance performance = new Performance();
            string countryCode = (await row.GetAttributeAsync("id")).Split('_').Last().ToUpper();
            IReadOnlyList<IElementHandle> columns = await row.QuerySelectorAllAsync("td");

            if (year == 1956)
            {
                countryCode = countryCode.Substring(0, 2);
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

    private async Task<Dictionary<string, IList<Score>>> GetAllScoresAsync(PlaywrightScraper playwright)
    {
        Dictionary<string, IList<Score>> result = new Dictionary<string, IList<Score>>();
        string buttonSelector = ".scoreboard_button_div button";
        IReadOnlyList<IElementHandle> buttons = await playwright.Page.QuerySelectorAllAsync(buttonSelector);

        if (buttons == null || buttons.Count <= 1)
        {
            await GetScoresAsync(playwright, "total", result);
        }
        else
        {
            int buttonsCount = buttons.Count;

            for (int i = 0; i < buttonsCount; i++)
            {
                IElementHandle button = buttons[i];
                string scoreName = (await button.InnerTextAsync()).ToLower();
                await button.ClickAsync();
                await playwright.Page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
                await GetScoresAsync(playwright, scoreName, result);
                buttons = await playwright.Page.QuerySelectorAllAsync(buttonSelector);
            }
        }

        return result;
    }

    private async Task GetScoresAsync(PlaywrightScraper playwright, string scoreName, Dictionary<string, IList<Score>> allScores)
    {
        string selector = "table.scoreboard_table tbody tr";
        IReadOnlyList<IElementHandle> rows = await playwright.Page.QuerySelectorAllAsync(selector);

        foreach (IElementHandle row in rows)
        {
            string countryCode = (await row.GetAttributeAsync("id")).Split("_").Last().ToUpper();
            IReadOnlyList<IElementHandle> columns = await row.QuerySelectorAllAsync("td[data-to]");
            Score score = new Score() { Name = scoreName };
            score.Points = int.Parse(await columns[3].InnerTextAsync());
            score.Votes = new Dictionary<string, int>();

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
}
