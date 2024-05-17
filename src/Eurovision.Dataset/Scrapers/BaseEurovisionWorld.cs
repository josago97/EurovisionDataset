using System.Text;
using System.Text.RegularExpressions;
using Eurovision.Dataset.Entities;
using Eurovision.Dataset.Utilities;
using Microsoft.Playwright;
using Sharplus.System.Linq;
using Sharplus.Tasks;

namespace Eurovision.Dataset.Scrapers;

public abstract class BaseEurovisionWorld
{
    protected const string URL = "https://eurovisionworld.com";
    //protected const int DELAY_REQUEST = 400; //ms
    protected static readonly TimeSpan TOO_MANY_REQUESTS_DELAY = TimeSpan.FromSeconds(10); //ms

    protected abstract string ContestListUrl { get; }

    public static async Task AcceptCookiesAsync()
    {
        using PlaywrightScraper playwright = new PlaywrightScraper();
        await LoadPageAsync(playwright, string.Empty, WaitUntilState.NetworkIdle);

        string acceptCookiesButtonSelector = "button.fc-cta-consent";
        await playwright.Page.ClickAsync(acceptCookiesButtonSelector);
    }

    protected async static Task<bool> LoadPageAsync(PlaywrightScraper playwright, string url, WaitUntilState waitUntilState = WaitUntilState.DOMContentLoaded)
    {
        string absoluteUrl = URL + url;
        IResponse response;

        do
        {
            response = await playwright.LoadPageAsync(absoluteUrl, waitUntilState);
            //await Task.Delay(DELAY_REQUEST);

            switch (response.Status)
            {
                case 429: // Too Many Requests
                    await Task.Delay(TOO_MANY_REQUESTS_DELAY);
                    break;
            }
        }
        while (!response.Ok);

        return response.Ok && playwright.Page.Url.Equals(absoluteUrl, StringComparison.OrdinalIgnoreCase);
    }

    public async Task<int> GetLastYearAsync()
    {
        int lastYear = -1;
        using PlaywrightScraper playwright = new PlaywrightScraper();
        await LoadPageAsync(playwright, ContestListUrl);

        string contestRowsSelector = "table.table_sort tbody tr";
        IReadOnlyList<IElementHandle> contestRows = await playwright.Page.QuerySelectorAllAsync(contestRowsSelector);
        int index = 0;

        do
        {
            IElementHandle contestRow = contestRows[index];
            IReadOnlyList<IElementHandle> contestColumns = await contestRow.QuerySelectorAllAsync("td");
            IElementHandle pointsElement = contestColumns.Last();
            string points = (await pointsElement.InnerTextAsync()).Trim();
            bool hasPoints = Regex.IsMatch(points, @"[0-9]+");

            if (hasPoints)
            {
                IElementHandle yearColumn = contestColumns.First();
                string yearAndCity = await yearColumn.InnerTextAsync();
                lastYear = int.Parse(yearAndCity.Substring(0, 4));
            }

            index++;
        } 
        while (lastYear == -1);

        return lastYear;
    }
}

public abstract class BaseEurovisionWorld<TContest, TContestant> : BaseEurovisionWorld
    where TContest : Contest, new()
    where TContestant : Contestant, new()
{
    protected const string CONTEST_PRESENTERS_KEY = "host";
    protected const string CONTEST_SLOGAN_KEY = "slogan";
    protected const string CONTEST_VOTING_KEY = "voting";

    protected const string CONTESTANT_COUNTRY_KEY = "country";
    protected const string CONTESTANT_ARTIST_KEY = "artist";
    protected const string CONTESTANT_SONG_KEY = "title";

    protected const string ROUND_DATETIME_KEY = "date";

    protected const string DATA_SEPARATOR = ", ";

    #region Contest

    public virtual async Task<TContest> GetContestAsync(int year)
    {
        TContest contest = new TContest { Year = year };
        using PlaywrightScraper playwright = new PlaywrightScraper();
        Dictionary<string, string> data = new Dictionary<string, string>();

        await LoadPageAsync(playwright, GetContestPageUrl(year));
        await GetContestDataAsync(playwright.Page, data);
        SetContestData(contest, data);

        IReadOnlyList<TContestant> contestans = await GetContestantsAsync(playwright.Page, year);
        contest.Contestants = contestans;
        contest.Rounds = await GetRoundsAsync(playwright, year, data, contestans);

        return contest;
    }

    protected abstract string GetContestPageUrl(int year);

    protected abstract Task GetContestDataAsync(IPage page, Dictionary<string, string> data);

    protected virtual void SetContestData(TContest contest, Dictionary<string, string> data)
    {
        if (data.TryGetValue(CONTEST_PRESENTERS_KEY, out string presenters))
            contest.Presenters = SplitData(presenters);

        if (data.TryGetValue(CONTEST_SLOGAN_KEY, out string slogan))
            contest.Slogan = slogan;

        if (data.TryGetValue(CONTEST_VOTING_KEY, out string voting))
            contest.Voting = voting;
    }

    #endregion

    #region Contestant

    private async Task<IReadOnlyList<TContestant>> GetContestantsAsync(IPage page, int year)
    {
        List<TContestant> result = new List<TContestant>();
        IReadOnlyList<IElementHandle> rows = await GetContestantsTableRowsAsync(page);
        using PlaywrightScraper playwright = new PlaywrightScraper();

        for (int i = 0; i < rows.Count; i++)
        {
            IElementHandle row = rows[i];
            TContestant contestant = await GetContestantAsync(playwright, row, year);
            contestant.Id = i;

            result.Add(contestant);
        }

        return result;
    }

    protected abstract Task<IElementHandle> GetContestantsTableAsync(IPage page);

    private async Task<IReadOnlyList<IElementHandle>> GetContestantsTableRowsAsync(IPage page)
    {
        IElementHandle table = await GetContestantsTableAsync(page);

        return await table.QuerySelectorAllAsync("tbody tr");
    }

    protected async Task<TContestant> GetContestantAsync(PlaywrightScraper playwright, IElementHandle row, int year)
    {
        TContestant result = new TContestant();
        Dictionary<string, string> data = new Dictionary<string, string>();
        IPage page = await GoToContestantPageAsync(playwright, row);

        await GetContestantDataAsync(row, page, data);
        SetContestantData(result, data);

        result.Lyrics = await GetLyricsAsync(page, data);
        result.VideoUrls = await GetVideoUrlsAsync(page);

        return result;
    }

    private async Task<IPage> GoToContestantPageAsync(PlaywrightScraper playwright, IElementHandle row)
    {
        IReadOnlyList<IElementHandle> links = await row.QuerySelectorAllAsync("a");
        string url = await links[1].GetAttributeAsync("href");
        await LoadPageAsync(playwright, url, WaitUntilState.Load);

        return playwright.Page;
    }

    protected abstract Task GetContestantDataAsync(IElementHandle row, IPage page, Dictionary<string, string> data);

    protected virtual void SetContestantData(TContestant contestant, Dictionary<string, string> data)
    {
        if (data.TryGetValue(CONTESTANT_COUNTRY_KEY, out string country))
            contestant.Country = country;

        if (data.TryGetValue(CONTESTANT_ARTIST_KEY, out string artist))
            contestant.Artist = artist;

        if (data.TryGetValue(CONTESTANT_SONG_KEY, out string song))
            contestant.Song = song;

        if (data.TryGetValue("backing", out string backings))
            contestant.Backings = SplitData(backings);

        if (data.TryGetValue("dancer", out string dancers))
            contestant.Dancers = SplitData(dancers);

        if (data.TryGetValue("lyricist", out string lyricists))
            contestant.Lyricists = SplitData(lyricists);

        if (data.TryGetValue("songwriter", out string writers))
            contestant.Writers = SplitData(writers);

        if (data.TryGetValue("composer", out string composers))
            contestant.Composers = SplitData(composers);
    }

    private async Task<IList<string>> GetVideoUrlsAsync(IPage page)
    {
        IList<string> result = new List<string>();

        IElementHandle moreVideosButton = await page.QuerySelectorAsync(".lyrics_more_videos_div button");
        if (moreVideosButton != null) await moreVideosButton.ClickAsync(new() { Force = true });

        IReadOnlyList<IElementHandle> videoElements = await page.QuerySelectorAllAsync(".vid_ratio iframe");
        string urlAttribute = "src";

        // When the link cannot be obtained from the videos above, it will be obtained from the videos below
        // In github actions this case occurs in eurovision spain 1966
        if (videoElements.Count == 0)
        {
            videoElements = await page.QuerySelectorAllAsync(".vid_ratio .video");
            urlAttribute = "data-video-iframe";
        }

        foreach (IElementHandle element in videoElements)
        {
            string videoUrl = await element.GetAttributeAsync(urlAttribute);

            int lastUrlIndex = videoUrl.IndexOf('?');
            if (lastUrlIndex != -1) videoUrl = videoUrl.Substring(0, lastUrlIndex);
            result.Add(videoUrl);
        }

        return result;
    }

    protected virtual async Task<IList<Lyrics>> GetLyricsAsync(IPage page, Dictionary<string, string> data)
    {
        IList<Lyrics> result = new List<Lyrics>();
        IReadOnlyList<IElementHandle> lyrics = await page.QuerySelectorAllAsync(".lyrics_div:not(.sticky)");

        if (!lyrics.IsNullOrEmpty())
        {
            foreach (IElementHandle lyric in lyrics)
            {
                IElementHandle title = await lyric.QuerySelectorAsync("h3");
                IReadOnlyList<IElementHandle> paragraphs = await lyric.QuerySelectorAllAsync("p");
                StringBuilder stringBuilder = new StringBuilder();

                for (int i = 0; i < paragraphs.Count; i++)
                {
                    IElementHandle paragraph = paragraphs[i];
                    string text = await paragraph.InnerTextFromHTMLAsync();

                    stringBuilder.Append(text);
                    if (i < paragraphs.Count - 1) stringBuilder.Append("\n\n");
                }

                result.Add(new Lyrics()
                {
                    Languages = (await lyric.GetAttributeAsync("data-lyrics-version")).Split(DATA_SEPARATOR),
                    Title = await (title?.InnerTextAsync()).ForAwait(),
                    Content = stringBuilder.ToString()
                });
            }
        }

        return result;
    }

    #endregion

    #region Round

    protected abstract Task<IReadOnlyList<Round>> GetRoundsAsync(PlaywrightScraper playwright, int year,
        IReadOnlyDictionary<string, string> contestData, IReadOnlyList<TContestant> contestants);

    #endregion

    #region Utils

    protected void AddData(Dictionary<string, string> data, string key, string value)
    {
        key = key.ToLower();

        if (key.EndsWith('s')) key = key.Substring(0, key.Length - 1);

        if (!data.TryAdd(key, value)) data[key] = value;
    }

    protected string[] SplitData(string data)
    {
        return data.Replace(" and ", DATA_SEPARATOR).Split(DATA_SEPARATOR);
    }

    protected (DateOnly, TimeOnly?) GetDateAndTime(IReadOnlyDictionary<string, string> contestData)
    {
        string[] data = contestData[ROUND_DATETIME_KEY].Split(DATA_SEPARATOR);
        DateOnly date = DateOnly.Parse(data[0]);
        TimeOnly? time = null;

        if (data.Length == 2)
        {
            string timeData = data[1];

            Regex endTimeRegex = new Regex(@"-.*:[0-9]+ *"); // To remove the time the contest ends
            timeData = endTimeRegex.Replace(timeData, "");

            DateTime datetimeData = DateTime.Parse(timeData.Replace("CEST", "+2")
                .Replace("CET", "+1")
                .Replace("UTC", "+0"));

            time = TimeOnly.FromDateTime(datetimeData.ToUniversalTime());
        }

        return (date, time);
    }

    #endregion
}
