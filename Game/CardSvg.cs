using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Components;
using SpiderBlazor.Localization;

namespace SpiderBlazor.Game;

// Builds (and caches) the SVG face of a card; suit shapes come from <symbol id="suit-N"> in index.html
public static class CardSvg
{
    private static readonly Dictionary<(int, Suit, Lang), MarkupString> Cache = [];

    private const double Top = 40, Bottom = 100, Mid = 70;
    private const double Left = 28, Center = 50, Right = 72;
    private const double PipSize = 18;

    // Four-colour deck: robe colour per suit (spades, hearts, clubs, diamonds)
    private static readonly string[] Robes = ["#23345c", "#b3122e", "#1d6b35", "#1450c8"];

    public static MarkupString Face(int rank, Suit suit, Lang lang)
    {
        if (Cache.TryGetValue((rank, suit, lang), out var svg)) return svg;

        var sb = new StringBuilder(2048);
        sb.Append("<svg class=\"face\" viewBox=\"0 0 100 140\" xmlns=\"http://www.w3.org/2000/svg\">");

        var label = Text.RankLabel(lang, rank);
        sb.Append(Corner(label, (int)suit));
        sb.Append(BottomCorner(label, (int)suit));

        // Number cards: one large suit in the centre; court cards: picture
        if (rank <= 10) Use(sb, (int)suit, Center, Mid, 56, false);
        else Figure(sb, rank, suit);

        sb.Append("</svg>");
        svg = new MarkupString(sb.ToString());
        Cache[(rank, suit, lang)] = svg;
        return svg;
    }

    private static string Corner(string label, int suit)
    {
        // Large sans-serif index; "10" slightly condensed to fit
        var size = label.Length > 1 ? 27 : 30;
        var spacing = label.Length > 1 ? " letter-spacing=\"-2\"" : "";
        return $"<text class=\"rank\" x=\"5\" y=\"27\" font-size=\"{size}\"{spacing}>{label}</text>"
             + $"<use href=\"#suit-{suit}\" x=\"73\" y=\"4\" width=\"23\" height=\"23\"/>";
    }

    // Upright bottom index: rank bottom-right, suit bottom-left
    private static string BottomCorner(string label, int suit)
    {
        var size = label.Length > 1 ? 27 : 30;
        var spacing = label.Length > 1 ? " letter-spacing=\"-2\"" : "";
        return $"<text class=\"rank\" x=\"95\" y=\"134\" text-anchor=\"end\" font-size=\"{size}\"{spacing}>{label}</text>"
             + $"<use href=\"#suit-{suit}\" x=\"4\" y=\"113\" width=\"23\" height=\"23\"/>";
    }

    private static void Use(StringBuilder sb, int suit, double cx, double cy, double size, bool flip)
    {
        var x = F(cx - size / 2);
        var y = F(cy - size / 2);
        sb.Append($"<use href=\"#suit-{suit}\" x=\"{x}\" y=\"{y}\" width=\"{F(size)}\" height=\"{F(size)}\"");
        if (flip) sb.Append($" transform=\"rotate(180 {F(cx)} {F(cy)})\"");
        sb.Append("/>");
    }

    // Classic pip layouts
    private static IEnumerable<(double X, double Y)> Pips(int rank)
    {
        var r1 = Top + (Bottom - Top) / 3;
        var r2 = Top + 2 * (Bottom - Top) / 3;
        (double, double)[] sides6 = [(Left, Top), (Right, Top), (Left, Mid), (Right, Mid), (Left, Bottom), (Right, Bottom)];
        (double, double)[] sides8 = [(Left, Top), (Right, Top), (Left, r1), (Right, r1), (Left, r2), (Right, r2), (Left, Bottom), (Right, Bottom)];

        return rank switch
        {
            2 => [(Center, Top), (Center, Bottom)],
            3 => [(Center, Top), (Center, Mid), (Center, Bottom)],
            4 => [(Left, Top), (Right, Top), (Left, Bottom), (Right, Bottom)],
            5 => [(Left, Top), (Right, Top), (Center, Mid), (Left, Bottom), (Right, Bottom)],
            6 => sides6,
            7 => [.. sides6, (Center, (Top + Mid) / 2)],
            8 => [.. sides6, (Center, (Top + Mid) / 2), (Center, (Mid + Bottom) / 2)],
            9 => [.. sides8, (Center, Mid)],
            _ => [.. sides8, (Center, (Top + r1) / 2), (Center, (r2 + Bottom) / 2)]
        };
    }

    // Original double-ended court figure: top half drawn, then mirrored
    private static void Figure(StringBuilder sb, int rank, Suit suit)
    {
        var robe = Robes[(int)suit];
        const string gold = "#d9a92e", skin = "#f2cfae";
        var s = (int)suit;

        var half = new StringBuilder();
        if (rank == 12)
            half.Append("<path d=\"M40 44 Q40 32 50 32 Q60 32 60 44 L62 56 L38 56 Z\" fill=\"#b9792a\"/>");
        half.Append($"<path d=\"M24 70 Q26 57 38 54 L62 54 Q74 57 76 70 Z\" fill=\"{robe}\"/>");
        half.Append($"<path d=\"M38 54 L50 64 L62 54\" stroke=\"{gold}\" stroke-width=\"2\" fill=\"none\"/>");
        half.Append($"<g style=\"color:{gold}\"><use href=\"#suit-{s}\" x=\"27\" y=\"58\" width=\"8\" height=\"8\"/></g>");
        half.Append($"<rect x=\"47\" y=\"49\" width=\"6\" height=\"6\" fill=\"{skin}\"/>");
        half.Append($"<circle cx=\"50\" cy=\"44\" r=\"8\" fill=\"{skin}\"/>");
        half.Append("<circle cx=\"47\" cy=\"43\" r=\"0.9\" fill=\"#333\"/><circle cx=\"53\" cy=\"43\" r=\"0.9\" fill=\"#333\"/>");

        switch (rank)
        {
            case 13: // King: beard, crown, sceptre
                half.Append("<path d=\"M42 45 Q43 55 50 57 Q57 55 58 45 Q54 50 50 50 Q46 50 42 45 Z\" fill=\"#6b4423\"/>");
                half.Append($"<path d=\"M41 37 L41 27 L45.5 31.5 L50 25 L54.5 31.5 L59 27 L59 37 Z\" fill=\"{gold}\"/>");
                half.Append("<circle cx=\"50\" cy=\"33\" r=\"1.3\" fill=\"#c8102e\"/>");
                half.Append($"<path d=\"M68 70 L66 53\" stroke=\"{gold}\" stroke-width=\"2\"/><circle cx=\"66\" cy=\"51\" r=\"2.2\" fill=\"{gold}\"/>");
                break;
            case 12: // Queen: tiara with pearls, flower
                half.Append("<path d=\"M47.5 47.5 Q50 49 52.5 47.5\" stroke=\"#a33\" stroke-width=\"0.8\" fill=\"none\"/>");
                half.Append($"<path d=\"M42 37 L43 29 L46.5 32 L50 27 L53.5 32 L57 29 L58 37 Z\" fill=\"{gold}\"/>");
                half.Append("<circle cx=\"43\" cy=\"29\" r=\"1.3\" fill=\"#fff\"/><circle cx=\"50\" cy=\"27\" r=\"1.3\" fill=\"#fff\"/><circle cx=\"57\" cy=\"29\" r=\"1.3\" fill=\"#fff\"/>");
                half.Append("<path d=\"M67 70 L66 58\" stroke=\"#3a7d3a\" stroke-width=\"1.2\"/><circle cx=\"66\" cy=\"56\" r=\"2.6\" fill=\"#e05a8a\"/>");
                break;
            default: // Jack: short hair, cap with feather
                half.Append("<path d=\"M47.5 47.5 Q50 49 52.5 47.5\" stroke=\"#a33\" stroke-width=\"0.8\" fill=\"none\"/>");
                half.Append("<path d=\"M42 40 Q44 34 50 34 Q56 34 58 40 Q54 37 50 37 Q46 37 42 40 Z\" fill=\"#4a2f1a\"/>");
                half.Append($"<path d=\"M40 38 Q41 28 51 28 Q60 29 61 38 Z\" fill=\"{robe}\"/>");
                half.Append($"<path d=\"M58 32 Q65 25 69 26 Q64 28 60 34 Z\" fill=\"{gold}\"/>");
                break;
        }

        // Shrink the figure so it fits inside the frame between the large corners
        var h = $"<g transform=\"translate(50 70) scale(0.85) translate(-50 -70)\">{half}</g>";
        sb.Append("<rect x=\"22\" y=\"31\" width=\"56\" height=\"78\" rx=\"3\" fill=\"#fbf3df\" stroke=\"currentColor\" stroke-width=\"1.2\"/>");
        sb.Append(h);
        sb.Append("<g transform=\"rotate(180 50 70)\">").Append(h).Append("</g>");
        sb.Append("<line x1=\"22\" y1=\"70\" x2=\"78\" y2=\"70\" stroke=\"currentColor\" stroke-opacity=\".35\" stroke-width=\"0.6\"/>");
    }

    private static string F(double v) => v.ToString("0.##", CultureInfo.InvariantCulture);
}
