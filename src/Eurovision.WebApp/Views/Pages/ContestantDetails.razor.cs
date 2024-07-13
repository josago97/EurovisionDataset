using System.Data;
using Eurovision.WebApp.Models;
using Eurovision.WebApp.Utilities;
using Eurovision.WebApp.Views.Components;
using Microsoft.AspNetCore.Components;
using Sharplus.System;
using Sharplus.System.Linq;

namespace Eurovision.WebApp.Views.Pages;

public partial class ContestantDetails
{
    private const int LYRICS_COLUMNS = 2;

    [Parameter]
    public int Year { get; set; }
    [Parameter]
    public int ContestantId { get; set; }

    private ContestData Contest { get; set; }
    private ContestantData Contestant { get; set; }
    private IList<RoundData> Rounds { get; set; }
    private int LyricsSelectedIndex { get; set; }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        Contest contest = GetContest(Year);
        Contest = GetContestData(contest);
        Contestant = GetContestantData(contest, ContestantId);
        Rounds = GetRounds(contest, ContestantId);
    }

    private ContestData GetContestData(Contest contest)
    {
        ContestData result = new ContestData()
        {
            Year = contest.Year,
            Country = contest.Country
        };

        return result;
    }

    private IList<RoundData> GetRounds(Contest contest, int contestantId)
    {
        List<RoundData> result = new List<RoundData>();

        for (int i = 0; i < contest.Rounds.Count; i++)
        {
            Round round = contest.Rounds[i];
            Performance performance = round.Performances?.FirstOrDefault(p => p.ContestantId == contestantId);

            if (performance != null)
            {
                result.Add(new RoundData()
                {
                    Name = Utils.GetDisplayRoundName(round.Name),
                    Place = performance.Place,
                    ContestantsCount = round.Performances.Count,
                    Points = performance.Scores.Count > 0
                        ? performance.Scores.Sum(s => s.Points)
                        : null,
                    Running = performance.Running
                });
            }
        }
        
        return result;
    }

    private ContestantData GetContestantData(Contest contest, int contestantId)
    {
        Contestant contestant = contest.Contestants[contestantId];

        ContestantData result = new ContestantData()
        {
            Artist = contestant.Artist,
            Bpm = contestant.Bpm >= 0 ? contestant.Bpm : null,
            Broadcaster = contestant.Broadcaster,
            Conductor = contestant.Conductor,
            CountryCode = contestant.Country,
            CountryName = Repository.Countries[contestant.Country],
            Lyrics = GetLyrics(contestant),
            MusicSheet = GetMusicSheet(contestant),
            Song = contestant.Song,
            Spokesperson = contestant.Spokesperson,
            StageDirector = contestant.StageDirector,
            Tone = contestant.Tone,
            Videos = contestant.VideoUrls
        };

        if (!contestant.Backings.IsNullOrEmpty())
            result.Backings = string.Join(", ", contestant.Backings);

        if (!contestant.Commentators.IsNullOrEmpty())
            result.Commentators = string.Join(", ", contestant.Commentators);

        if (!contestant.Composers.IsNullOrEmpty())
            result.Composers = string.Join(", ", contestant.Composers);

        if (!contestant.Dancers.IsNullOrEmpty())
            result.Dancers = string.Join(", ", contestant.Dancers);

        if (!contestant.Writers.IsNullOrEmpty())
            result.Writers = string.Join(", ", contestant.Writers);

        if (contest.Rounds != null)
        {
            result.Disqualified = contest.Rounds.Any(round =>
                round.Disqualifieds != null && round.Disqualifieds.Contains(contestantId));
        }

        return result;
    }

    private IReadOnlyList<Lyrics> GetLyrics(Contestant contestant)
    {
        List<Lyrics> result = new List<Lyrics>();

        foreach (Lyrics lyrics in contestant.Lyrics)
        {
            if (!string.IsNullOrEmpty(lyrics.Content))
            {
                if (string.IsNullOrEmpty(lyrics.Title)) lyrics.Title = contestant.Song;
                result.Add(lyrics);
            }
        }

        return result;
    }

    private MusicSheet GetMusicSheet(Contestant contestant)
    {
        MusicSheet result = null;

        if (!string.IsNullOrEmpty(contestant.Tone))
        {
            string[] toneAndScaleName = contestant.Tone.Split();
            string toneName = toneAndScaleName[0].Replace("b", "Flat").Replace("#", "Sharp");
            ArmorMusicSheet.Notes tone = Enum.Parse<ArmorMusicSheet.Notes>(toneName, true);
            ArmorMusicSheet.Scales scale = toneAndScaleName[1].Equals("minor", StringComparison.OrdinalIgnoreCase)
                ? ArmorMusicSheet.Scales.Minor
                : ArmorMusicSheet.Scales.Major;
            string toneDisplay = toneAndScaleName[0];
            string scaleDisplay = toneAndScaleName[1];
            if (scale == ArmorMusicSheet.Scales.Major) scaleDisplay = scaleDisplay.ToTitleCase(); 

            result = new MusicSheet()
            {
                Tone = $"{toneDisplay} {scaleDisplay}",
                Bpm = contestant.Bpm,
                ArmorMusicSheet = new RenderFragment(builder =>
                {
                    builder.OpenComponent(0, typeof(ArmorMusicSheet));
                    builder.AddAttribute(1, nameof(ArmorMusicSheet.Tempo), contestant.Bpm);
                    builder.AddAttribute(2, nameof(ArmorMusicSheet.Tone), tone);
                    builder.AddAttribute(3, nameof(ArmorMusicSheet.Scale), scale);
                    builder.CloseComponent();
                }),
            };
        }

        return result;
    }

    // Columns / Paragraph / Line
    private string[][][] GetLyricsColumns()
    {
        string[][][] result = new string[LYRICS_COLUMNS][][];

        string lyrics = Contestant.Lyrics[LyricsSelectedIndex].Content;
        string[] paragraphGroups = lyrics.Split("\n\n");
        string[][] paragraphs = paragraphGroups.Select(s => s.Split('\n')).ToArray();

        int totalLines = paragraphs.Sum(p => p.Length + 1); // Lines break (+1)
        int middleLines = (int)Math.Ceiling(totalLines / 2.0);
        int middle = 0;

        while (middleLines > 0)
        {
            int lines = paragraphs[middle].Length + 1; // Line break (+1)

            // Si lo que me queda es menor que la mitad del párrafo,
            // entonces lo añado mejor a la otra mitad, ya que está
            // más cerca de la segunda mitad que de la primera
            if (middleLines < lines / 2)
                middleLines = 0;
            else
            {
                middleLines -= lines;
                middle++;
            }
        }

        List<string[]> columnParagraphs = new List<string[]>();
        int start, end;

        for (int i = 0; i < result.Length; i++)
        {
            start = i == 0 ? 0 : middle;
            end = i == 0 ? middle : paragraphs.Length;

            for (int j = start; j < end; j++)
                columnParagraphs.Add(paragraphs[j]);

            result[i] = columnParagraphs.ToArray();
            columnParagraphs.Clear();
        }

        return result;
    }

    private class ContestData
    {
        public int Year { get; set; }
        public string Country { get; set; }
        public string CountryName { get; set; }
    }

    private class RoundData
    {
        public string Name { get; set; }
        public int Place { get; set; }
        public int ContestantsCount { get; set; }
        public int? Points { get; set; }
        public int? Running { get; set; }
    }

    private class ContestantData
    {
        public string Artist { get; set; }
        public string Backings { get; set; }
        public int? Bpm { get; set; }
        public string Broadcaster { get; set; }
        public string Commentators { get; set; }
        public string Composers { get; set; }
        public string Conductor { get; set; }
        public string CountryCode { get; set; }
        public string CountryName { get; set; }
        public string Dancers { get; set; }
        public IReadOnlyList<Lyrics> Lyrics { get; set; }
        public MusicSheet MusicSheet { get; set; }
        public int? Place { get; set; }
        public int? Points { get; set; }
        public int? Running { get; set; }
        public string Song { get; set; }
        public string Spokesperson { get; set; }
        public string StageDirector { get; set; }
        public string Tone { get; set; }
        public IReadOnlyList<string> Videos { get; set; }
        public string Writers { get; set; }
        public bool Disqualified { get; set; }
    }

    private class MusicSheet
    {
        public string Tone { get; set; }
        public int? Bpm { get; set; }
        public RenderFragment ArmorMusicSheet { get; set; }
    }
}