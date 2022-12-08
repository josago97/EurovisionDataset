using System.Text.RegularExpressions;
using EurovisionDataset.Data.National;
using Microsoft.Playwright;

namespace EurovisionDataset.Scrapers.National;

public class Natfinals
{
    private const string URL = "https://natfinals.50webs.com/";

    public async Task<IList<Contest>> GetNationalsAsync(int start, int end)
    {
        List<Contest> contests = new List<Contest>();

        for (int i = start; i <= end; i++)
        {
            contests.Add(await GetContestAsync(i));
        }

        return contests;
    }

    private async Task<bool> LoadPageAsync(PlaywrightScraper playwright, string realtiveUrl)
    {
        string url = $"{URL}{realtiveUrl}";
        IResponse response = await playwright.LoadPageAsync(url, WaitUntilState.DOMContentLoaded);

        return response.Ok;
    }

    private async Task<IReadOnlyList<IElementHandle>> GetElementsAsync(PlaywrightScraper playwright)
    {
        return await playwright.Page.QuerySelectorAllAsync("td a");
    }

    private async Task<Contest> GetContestAsync(int year)
    {
        Contest result = new Contest() { Year = year };
        using PlaywrightScraper playwright = new PlaywrightScraper();

        string decade = year switch
        {
            < 1970 => "50s_60s/",
            < 1990 => "70s_80s/",
            < 2010 => "90s_00s/",
            < 2023 => "10s_20s/",
            _ => string.Empty
        };

        if (await LoadPageAsync(playwright, $"{decade}{year}.html"))
        {
            List<Selection> nationals = new List<Selection>();
            IReadOnlyList<IElementHandle> finals = await GetElementsAsync(playwright);

            foreach (IElementHandle final in finals)
            {
                string href = await final.GetAttributeAsync("href");
                string nationalUrl = $"{decade}/{href}";
                nationals.Add(await GetNationalAsync(nationalUrl));
            }

            result.Selections = nationals.ToArray();
        }

        return result;
    }

    private async Task<Selection> GetNationalAsync(string url)
    {
        Selection result = null;
        using PlaywrightScraper playwright = new PlaywrightScraper();

        if (await LoadPageAsync(playwright, url))
        {
            result = new Selection();
            result.Country = await GetCountryCodeAsync(playwright);
            result.Contestants = await GetContestantAsync(playwright);
            //result.WinnersId = await GetWinnersAsync(playwright, result.Contestants);
        }

        return result;
    }

    private async Task<int[]> GetWinnersAsync(PlaywrightScraper playwright, Contestant[] contestants)
    {
        IElementHandle winnersElement = await playwright.Page.QuerySelectorAsync("p i");
        Regex regex = new Regex("\".*\""); // Todo lo que haya entre comillas
        string[] winnersSong = (await winnersElement.InnerTextAsync())
            .Split("/")
            .SelectMany(s => regex.Matches(s).Select(m => m.Value.Trim('\"')))
            .ToArray();

        return contestants.Where(c => winnersSong.Any(s => c.Song.Equals(s, StringComparison.OrdinalIgnoreCase)))
            .Select(c => c.Id)
            .ToArray();
    }

    private async Task<string> GetCountryCodeAsync(PlaywrightScraper playwright)
    {
        IElementHandle countryElement = await playwright.Page.QuerySelectorAsync("table u"); 
        /*await playwright.Page.QuerySelectorAsync("big u")
            ?? await playwright.Page.QuerySelectorAsync("table b u");*/
        //?? await playwright.Page.QuerySelectorAsync("table u big");

        string countryName = await countryElement.InnerTextAsync();
        Regex regex = new Regex(@"[a-zA-Z\s]+"); //Solo letras y espacios
        countryName = regex.Match(countryName).Value;

        var t = Utils.GetCountryCode(countryName);

        if (string.IsNullOrEmpty(t))
        {

        }

        return t;
    }

    /*
    private async Task<Contestant[]> GetContestantAsync(PlaywrightScraper playwright)
    {
        List<Contestant> result = new List<Contestant>();
        IElementHandle table = (await playwright.Page.QuerySelectorAllAsync("tbody"))[1];

        string[][] contestantTableData = await GetContestantTableDataAsync(table);

        if (contestantTableData != null)
        {
            ContestantTableHeader[] headers = await GetContestantTableHeadersAsync(table);

            for (int i = 0; i < contestantTableData.Length; i++)
            {
                //Cogemos la fila
                string[] data = contestantTableData[i];

                result.Add(GetContestant(headers, data));
            }
        }

        return result.ToArray();
    }

    private Contestant GetContestant(ContestantTableHeader[] headers, string[] rowData)
    {
        Contestant result = new Contestant();

        for (int i = 0; i < headers.Length; i++)
        {
            try
            {
                ContestantTableHeader header = headers[i];
                string data = rowData[i];


                if (header == ContestantTableHeader.Id)
                    result.Id = int.Parse(data);
                else if (header == ContestantTableHeader.Song)
                    result.Song = data;
                else if (header == ContestantTableHeader.EnglishSong)
                    result.EnglishSong = data;
                else if (header == ContestantTableHeader.Artist)
                    result.Artist = data;
                else if (header == ContestantTableHeader.Score && data != "-")
                    result.Score = int.Parse(data);
                else if (header == ContestantTableHeader.Rank && data != "?")
                    result.Rank = int.Parse(data.Where(char.IsDigit).ToArray());
            }
            catch
            {

            }
        }

        return result;
    }*/

    /*
    private async Task<ContestantTableHeader[]> GetContestantTableHeadersAsync(IElementHandle table)
    {
        List<ContestantTableHeader> result = new List<ContestantTableHeader>();

        string[][] tableData = await GetContestantTableDataAsync(table);

        if (tableData != null)
        {
            result.Add(ContestantTableHeader.Id);

            int columns = tableData[0].Length;

            if (columns == 6)
            {
                result.Add(ContestantTableHeader.Song);
                result.Add(ContestantTableHeader.EnglishSong);
                result.Add(ContestantTableHeader.Artist);
                result.Add(ContestantTableHeader.Score);
                result.Add(ContestantTableHeader.Rank);
            }
            else if (columns == 5)
            {
                result.Add(ContestantTableHeader.Song);
                result.Add(ContestantTableHeader.EnglishSong);
                result.Add(ContestantTableHeader.Artist);
                result.Add(ContestantTableHeader.Rank);
            }
            else if (columns == 4)
            {
                result.Add(ContestantTableHeader.Song);
                result.Add(ContestantTableHeader.EnglishSong);
                result.Add(ContestantTableHeader.Artist);
            }
            else
            {

            }
        }
        /*
        if (tableData.Length == 6)
        {
            result.Add(ContestantTableHeader.Song);
            result.Add(ContestantTableHeader.EnglishSong);
            result.Add(ContestantTableHeader.Artist);
            result.Add(ContestantTableHeader.Score);
            result.Add(ContestantTableHeader.Rank);
        }
        else if (tableData.Length == 5)
        {

        }
        else if (tableData.Length == 4)
        {
            result.Add(ContestantTableHeader.Song);
            result.Add(ContestantTableHeader.EnglishSong);

            if (tableData.Last().Any(s => s == "?" || s == "1st"))
                result.Add(ContestantTableHeader.Rank);
            else
                result.Add(ContestantTableHeader.Artist);
        }
        else if (tableData.Length == 3)
        {
            result.Add(ContestantTableHeader.Artist);
            result.Add(ContestantTableHeader.Rank);
        }*/
    /*
        return result.ToArray();
    }



    //Devuelve las columnas y dentro las filas
    private async Task<string[][]> GetContestantTableDataAsync(IElementHandle table)
    {
        string[][] result = null;

        IReadOnlyList<IElementHandle> rows = await table.QuerySelectorAllAsync("tr");

        if (rows.Count > 0)
        {
            result = new string[rows.Count][];

            for (int i = 0; i < result.Length; i++)
            {
                IReadOnlyList<IElementHandle> columns = await rows[i].QuerySelectorAllAsync("td");
                result[i] = await Task.WhenAll(columns.Select(c => c.InnerTextAsync()));
            }
        }

        return result;

        var tableData = await table.QuerySelectorAllAsync("tr")
            .ContinueWithResult(e => e.Select(r => r.QuerySelectorAllAsync("td")));

        /*
    var tableData = await Task.WhenAll((await table.QuerySelectorAllAsync("tr"))
        .Select(async r => await r.QuerySelectorAllAsync("td")
        .ContinueWithResult(async e => await e.Select(async x => x.InnerTextAsync()));

    if (tableData.Length > 0) 
    {
        result = new string[tableData.Length][];
    }

    result = new string[tableData[0].Count][];

    for (int i = 0; i < result.Length; i++)
    {
        List<string> columns = new List<string>();

        for (int j = 0; j < tableData[i].Count; j++)
        {
            try
            {
                columns.Add(await tableData[i][j].InnerTextAsync());
            }
            catch
            {

            }
        }

        result[i] = columns.ToArray();
    }

    return result;*/
    /*}*/





    private async Task<Contestant[]> GetContestantAsync(PlaywrightScraper playwright)
    {
        List<Contestant> result = new List<Contestant>();

        ContestantTable[] contestantTables = await GetContestantTablesAsync(playwright);
        int id = 0;

        foreach (ContestantTable contestantTable in contestantTables)
        {
            ContestantTableHeader[] headers = contestantTable.Headers;

            for (int i = 0; i < contestantTable.Content.Length; i++)
            {
                //Cogemos la fila
                string[] data = contestantTable.Content[i];

                result.Add(GetContestant(headers, id, data));
                id++;
            }
        }

        return result.ToArray();
    }

    private Contestant GetContestant(ContestantTableHeader[] headers, int id, string[] rowData)
    {
        Contestant result = new Contestant() { Id = id };

        for (int i = 0; i < headers.Length; i++)
        {
            ContestantTableHeader header = headers[i];
            string data = rowData[i];

            try
            {

                /*
                if (header == ContestantTableHeader.Song)
                    result.Song = data;
               // else if (header == ContestantTableHeader.EnglishSong)
                    //result.EnglishSong = data;
                else if (header == ContestantTableHeader.Artist)
                    result.Artist = data;
                else if (header == ContestantTableHeader.Score && data != "-")
                    result.Score = int.Parse(data);
                else if (header == ContestantTableHeader.Rank)
                {
                    string rank = new string(data.Where(char.IsDigit).ToArray());
                    if (!string.IsNullOrEmpty(rank)) result.Rank = int.Parse(rank);
                }*/
            }
            catch (Exception e)
            {

            }
        }

        return result;
    }

    private async Task<ContestantTable[]> GetContestantTablesAsync(PlaywrightScraper playwright)
    {
        List<ContestantTable> result = new List<ContestantTable>();

        //Cogemos todas las tablas que no tengan imágenes
        var tables = await playwright.Page.QuerySelectorAllAsync("table:not(:has(img))");

        foreach (IElementHandle table in tables)
        {
            ContestantTable contestantTable = await GetContestantTableAsync(table);
            result.Add(contestantTable);
        }

        return result.ToArray();
    }

    private async Task<ContestantTable> GetContestantTableAsync(IElementHandle table)
    {
        string[][] content = await GetContentContestantTableAsync(table);

        return new ContestantTable()
        {
            Headers = GetContestantTableHeaders(content),
            Content = content
        };
    }

    private async Task<string[][]> GetContentContestantTableAsync(IElementHandle table)
    {
        IReadOnlyList<IElementHandle> rows = await table.QuerySelectorAllAsync("tr");
        string[][] result = new string[rows.Count][];

        for (int i = 0; i < result.Length; i++)
        {
            IReadOnlyList<IElementHandle> columns = await rows[i].QuerySelectorAllAsync("td");
            result[i] = await Task.WhenAll(columns.Skip(1).Select(c => c.InnerTextAsync()));
        }

        return result;
    }


    private ContestantTableHeader[] GetContestantTableHeaders(string[][] table)
    {
        ContestantTableHeader[] result = new ContestantTableHeader[table[0].Length];

        for (int i = 0; i < result.Length; i++)
        {
            ContestantTableHeader header = ContestantTableHeader.None;

            if (i == 0)
                header = ContestantTableHeader.Song;
            else if (i == 1)
                header = ContestantTableHeader.EnglishSong;
            else if (i == 2)
                header = ContestantTableHeader.Artist;
            else
            {
                string[] columns = table.Select(r => r[i]).ToArray();
                Regex regex = new Regex(@"[1-9]+[a-z]+");

                if (columns.Any(regex.IsMatch))
                    header = ContestantTableHeader.Rank;
            }

            result[i] = header;
        }

        return result.ToArray();
    }


    private enum ContestantTableHeader { None, Song, EnglishSong, Artist, Score, Rank };

    private class ContestantTable
    {
        public ContestantTableHeader[] Headers { get; set; }
        public string[][] Content { get; set; }
    }
}




/*

private async Task<Contestant[]> GetContestantAsync(PlaywrightScraper playwright)
{
    List<Contestant> result = new List<Contestant>();
    IElementHandle table = (await playwright.Page.QuerySelectorAllAsync("tbody"))[1];

    string[][] contestantTableData = await GetContestantTableDataAsync(table);

    if (contestantTableData != null)
    {
        ContestantTableHeader[] headers = await GetContestantTableHeadersAsync(table);

        for (int i = 0; i < contestantTableData[0].Length; i++)
        {
            //Cogemos la fila
            string[] data = contestantTableData[i];

            result.Add(GetContestant(headers, data));
        }
    }

    return result.ToArray();
}

private Contestant GetContestant(ContestantTableHeader[] headers, string[] rowData)
{
    Contestant result = new Contestant();

    for (int i = 0; i < headers.Length; i++)
    {
        ContestantTableHeader header = headers[i];
        string data = rowData[i];

        try
        {
            if (header == ContestantTableHeader.Id)
                result.Id = int.Parse(data);
            else if (header == ContestantTableHeader.Song)
                result.Song = data;
            else if (header == ContestantTableHeader.EnglishSong)
                result.EnglishSong = data;
            else if (header == ContestantTableHeader.Artist)
                result.Artist = data;
            else if (header == ContestantTableHeader.Score && data != "-")
                result.Score = int.Parse(data);
            else if (header == ContestantTableHeader.Rank && data != "?")
                result.Rank = int.Parse(data.Where(char.IsDigit).ToArray());
        }
        catch
        {

        }
    }

    return result;
}

private async Task<ContestantTableHeader[]> GetContestantTableHeadersAsync(IElementHandle table)
{
    List<ContestantTableHeader> result = new List<ContestantTableHeader>()
    {
        ContestantTableHeader.Id
    };

    string[][] tableData = await GetContestantTableDataAsync(table);

    if (tableData.Length == 6)
    {
        result.Add(ContestantTableHeader.Song);
        result.Add(ContestantTableHeader.EnglishSong);
        result.Add(ContestantTableHeader.Artist);
        result.Add(ContestantTableHeader.Score);
        result.Add(ContestantTableHeader.Rank);
    }
    else if (tableData.Length == 5)
    {

    }
    else if (tableData.Length == 4)
    {
        result.Add(ContestantTableHeader.Song);
        result.Add(ContestantTableHeader.EnglishSong);

        if (tableData.Last().Any(s => s == "?" || s == "1st"))
            result.Add(ContestantTableHeader.Rank);
        else
            result.Add(ContestantTableHeader.Artist);
    }
    else if(tableData.Length == 3) 
    {
        result.Add(ContestantTableHeader.Artist);
        result.Add(ContestantTableHeader.Rank);
    }

    return result.ToArray();
}

//Devuelve las columnas y dentro las filas
private async Task<string[][]> GetContestantTableDataAsync(IElementHandle table)
{
    string[][] result;

    var tableData = await Task.WhenAll((await table.QuerySelectorAllAsync("tr"))
        .Select(async r => await r.QuerySelectorAllAsync("td")));

    result = new string[tableData[0].Count][];

    for (int i = 0; i < result.Length; i++)
    {
        List<string> columns = new List<string>();

        for (int j = 0; j < tableData[i].Count; j++)
        {
            try
            {
                columns.Add(await tableData[i][j].InnerTextAsync());
            }
            catch
            {

            }
        }

        result[i] = columns.ToArray();
    }

    return result;
}
}
*/