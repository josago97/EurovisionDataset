﻿using EurovisionDataset.Entities;
using EurovisionDataset.Utilities;
using Contest = EurovisionDataset.Entities.Senior.Contest;
using Contestant = EurovisionDataset.Entities.Senior.Contestant;

namespace EurovisionDataset.Scrapers.Senior;

public class SeniorScraper : BaseScraper<Contest, Contestant>
{
    protected override int FirstYear => 1956;
    protected override EurovisionWorld EurovisionWorld { get; } = new EurovisionWorld();

    protected override async Task ScrapContests(IReadOnlyList<Contest> contests)
    {
        Eschome eschome = new Eschome();
        await eschome.ScrapContestsAsync(contests);

        EurovisionLod eurovisionLod = new EurovisionLod();
        await eurovisionLod.ScrapContestsAsync(contests);
    }

    protected override void InsertUnavailableData(Contest contest)
    {
        Contestant contestant;

        switch (contest.Year)
        {
            case 2023:
                contest.IntendedCountry = CountryCollection.GetCountryCode("Ukraine");
                break;

            case 2022:
                contest.LogoUrl = "https://upload.wikimedia.org/wikipedia/en/thumb/0/01/Eurovision_2022_Official_Logo.jpg/250px-Eurovision_2022_Official_Logo.jpg";
                break;

            case 2020:
                contest.LogoUrl = "https://upload.wikimedia.org/wikipedia/en/thumb/6/6f/Eurovision_Song_Contest_2020.svg/188px-Eurovision_Song_Contest_2020.svg.png";
                contestant = (Contestant)contest.Contestants.First(c => c.Country == CountryCollection.GetCountryCode("Armenia"));
                contestant.Broadcaster = "AMPTV";
                contestant = (Contestant)contest.Contestants.First(c => c.Country == CountryCollection.GetCountryCode("Belarus"));
                contestant.Broadcaster = "BTRC";
                contest.Rounds =
                [
                    new Round() { Name = "semifinal1", Date = new DateOnly(2020, 5, 12), Time = new TimeOnly(19, 0) },
                    new Round() { Name = "semifinal2", Date = new DateOnly(2020, 5, 14), Time = new TimeOnly(19, 0) },
                    new Round() { Name = "final", Date = new DateOnly(2020, 5, 16), Time = new TimeOnly(19, 0) }
                ];
                break;

            case 2015:
                contestant = (Contestant)contest.Contestants.First(c => c.Country == CountryCollection.GetCountryCode("Russia"));
                contestant.Lyrics = GetLyrics("English", contestant.Song, "2015_russia_lyrics");
                contestant.VideoUrls = ["https://www.youtube.com/embed/jBVY7Glcd84"];
                break;

            case 2005:
                contestant = (Contestant)contest.Contestants.First(c => c.Country == CountryCollection.GetCountryCode("Russia"));
                contestant.Lyrics = GetLyrics("English", contestant.Song, "2005_russia_lyrics");
                contestant.VideoUrls = ["https://www.youtube.com/embed/HQhgevOeh1E"];
                break;

            case 1995:
                contestant = (Contestant)contest.Contestants.First(c => c.Country == CountryCollection.GetCountryCode("Russia"));
                contestant.Lyrics = GetLyrics("Russian", contestant.Song, "1995_russia_lyrics");
                contestant.VideoUrls = ["https://www.youtube.com/embed/mZTZPE1mV2s"];
                break;
        }
    }

    protected override void CheckUnvailableData(Contestant contestant, List<string> noAvailable)
    {
        base.CheckUnvailableData(contestant, noAvailable);

        if (string.IsNullOrEmpty(contestant.Broadcaster))
            noAvailable.Add("Broadcaster");
    }
}
