using System.Text;
using Microsoft.AspNetCore.Components;

namespace Eurovision.WebApp.Views.Components;

public partial class ArmorMusicSheet
{
    public enum Notes { CFlat, C, CSharp, DFlat, D, DSharp, EFlat, E, ESharp, FFlat, F, FSharp, GFlat, G, GSharp, AFlat, A, ASharp, BFlat, B, BSharp };
    public enum Scales { Major, Minor };

    private const int ACCORD_NOTES_LENGTH = 3;
    private const int ALTERATION_SPACE = 8;
    private const double MARGIN_KEY = 3;
    private const double MARGIN_ALTERATIONS = 20;
    private const double MARGIN_ACCORD = 10;
    private const string MUSIC_FONT = "Polihymnia";
    private const int TEMPO_POSITION_Y = 10;
    private const int STAFF_POSITION_Y = TEMPO_POSITION_Y + 15;
    private const int STAFF_WIDTH = 110;
    private const int STAFF_SPACE = 6;
    private const int POSITION_C_NOTE = STAFF_POSITION_Y + STAFF_SPACE * 5;
    private const int TEMPO_SIZE = 15;
    private const int PENTAGRAM_ITEM_SIZE = 30;
    private const double OVERLINE_SIZE = 15.75;
    private static readonly Notes[] SHARPS_ORDER = { Notes.C, Notes.G, Notes.D, Notes.A, Notes.E, Notes.B, Notes.FSharp, Notes.CSharp };
    private static readonly double[] SHARP_POSITIONS = { 0, 1.5, -0.5, 1, 2.5, 0.5, 2 }; // F C G D A E B
    private static readonly Notes[] FLATS_ORDER = { Notes.C, Notes.F, Notes.BFlat, Notes.EFlat, Notes.AFlat, Notes.DFlat, Notes.GFlat, Notes.CFlat };
    private static readonly double[] FLAT_POSITIONS = { 2, 0.5, 2.5, 1, 3, 1.5, 3.5 }; // B E A D G C F
    
    private static readonly Dictionary<Notes, Notes> FIFTH_CIRCLE = new Dictionary<Notes, Notes>() // Minor -> Major
    {
        // Minor    Major
        // 0
        { Notes.A, Notes.C },
        // 1
        { Notes.E, Notes.G },
        { Notes.D, Notes.F },
        // 2
        { Notes.G, Notes.BFlat },
        { Notes.B, Notes.D },
        // 3
        { Notes.C, Notes.EFlat },
        { Notes.FSharp, Notes.A },
        // 4
        { Notes.F, Notes.AFlat },
        { Notes.CSharp, Notes.E },
        // 5
        { Notes.BFlat, Notes.DFlat },
        { Notes.GSharp, Notes.B },
        // 6
        { Notes.DSharp, Notes.FSharp },
        { Notes.EFlat, Notes.GFlat },
        // 7
        { Notes.AFlat, Notes.CFlat },
        { Notes.ASharp, Notes.CSharp },
    };

    [Parameter]
    public Notes Tone { get; set; }
    [Parameter]
    public Scales Scale { get; set; }
    [Parameter]
    public int? Tempo { get; set; }

    private string Html { get; set; }
    private double PositionX { get; set; }

    protected override void OnParametersSet()
    {
        PositionX = 0;
        StringBuilder htmlBuilder = new StringBuilder();
        if (Tempo.HasValue) DrawTempo(htmlBuilder, Tempo.Value);
        DrawStaff(htmlBuilder);
        DrawArmor(htmlBuilder);
        DrawAccord(htmlBuilder);

        Html = htmlBuilder.ToString();
    }

    private void DrawTempo(StringBuilder builder, int tempo)
    {
        builder.AppendLine(DrawText(0, TEMPO_POSITION_Y, TEMPO_SIZE, MUSIC_FONT, "qj"));
        builder.AppendLine(DrawText(11, TEMPO_POSITION_Y, TEMPO_SIZE - 4, "Open Sans", $"= {tempo}"));
    }

    private void DrawStaff(StringBuilder stringBuilder)
    {
        int y = STAFF_POSITION_Y;

        for (int i = 0; i < 5; i++)
        {
            stringBuilder.AppendLine(DrawLine(0, y, STAFF_WIDTH, y));
            y += STAFF_SPACE;
        }

        PositionX += MARGIN_KEY;
        stringBuilder.AppendLine(DrawText(PositionX, STAFF_POSITION_Y + 16, PENTAGRAM_ITEM_SIZE, MUSIC_FONT, "Gj"));
    }

    private void DrawArmor(StringBuilder builder)
    {
        Notes tone = Tone;

        // Find relative major
        if (Scale == Scales.Minor && !FIFTH_CIRCLE.TryGetValue(tone, out tone))
        {
            throw new InvalidDataException("Not exist scale");
        }

        string alteration;
        double[] alterationPositions;
        int alterationCount = Array.IndexOf(SHARPS_ORDER, tone);

        if (alterationCount >= 0) // Sharp
        {
            alteration = "Xj";
            alterationPositions = SHARP_POSITIONS;
        }
        else
        {
            alterationCount = Array.IndexOf(FLATS_ORDER, tone);

            if (alterationCount >= 0) // Flat
            {
                alteration = "bj";
                alterationPositions = FLAT_POSITIONS;
            }
            else
                throw new InvalidDataException("Not exist scale");
        }

        PositionX += MARGIN_ALTERATIONS;

        for (int i = 0; i < alterationCount; i++)
        {
            int y = (int)(STAFF_POSITION_Y + alterationPositions[i] * STAFF_SPACE);
            
            builder.AppendLine(DrawText(PositionX, y, PENTAGRAM_ITEM_SIZE, MUSIC_FONT, alteration));
            PositionX += ALTERATION_SPACE;
        }
    }

    private void DrawAccord(StringBuilder builder)
    {
        int startNote = (int)Tone / 3; // flat + nature + sharp
        double positionY = POSITION_C_NOTE - (STAFF_SPACE / 2) * startNote;

        PositionX += MARGIN_ACCORD;

        for (int i = 0; i < ACCORD_NOTES_LENGTH; i++)
        {
            string note = DrawText(PositionX, positionY, PENTAGRAM_ITEM_SIZE, MUSIC_FONT, "wj");
            builder.AppendLine(note);
            positionY -= STAFF_SPACE;
        }

        // If start Note is C then we must draw the overline
        if (startNote == 0)
        {
            string overline = DrawLine(PositionX, POSITION_C_NOTE, PositionX + OVERLINE_SIZE, POSITION_C_NOTE);
            builder.AppendLine(overline);
        }
    }

    private string DrawLine(double x1, double y1, double x2, double y2)
    {
        return $"<line x1=\"{x1}\" y1=\"{y1}\" x2=\"{x2}\" y2=\"{y2}\" style=\"fill:none;stroke:rgb(0,0,0);stroke-width:0.5\"></line>";
    }

    private string DrawText(double x, double y, double fontSize, string fontFamily, string innerHtml)
    {
        return $"<text x=\"{x}\" y=\"{y}\" style=\"fill:rgb(0,0,0); font-size:{fontSize}px; font-family: {fontFamily};\">{innerHtml}</text>";
    }
}