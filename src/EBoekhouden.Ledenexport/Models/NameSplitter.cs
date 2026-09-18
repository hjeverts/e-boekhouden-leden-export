namespace EBoekhouden.Ledenexport.Models;

/// <summary>
/// Splits a single "Naam" field into voornaam/achternaam for sorting and export.
///
/// The membership export already encodes a tussenvoegsel with a comma, e.g.
/// "H.J. Barge, ten" (= "ten Barge") or "A.B. Berg, van den" (= "van den Berg"); when
/// that comma is present we trust it directly. Without a comma (e.g. a plain
/// "Gerard van Ommeren") we detect the tussenvoegsel ourselves from a fixed word list.
/// Either way, achternaam is written as "Stam, tussenvoegsel" (e.g. "Ommeren, van") so a
/// plain string sort already groups people by surname stem, ignoring the prefix, which
/// is the normal Dutch phone-book convention.
///
/// A hyphenated last token (e.g. "Bos-Goorhorst") is always kept whole as the achternaam
/// stem and never split on the hyphen. The one case this heuristic does not unravel is a
/// double-barrelled surname that embeds a tussenvoegsel without a comma and without a
/// hyphen on the final token, e.g. "J.E. Bruntink-ten Nijenhuis" — there is no reliable
/// signal to tell that apart from "voornaam tussenvoegsel achternaam", so it is treated
/// as a normal name with achternaam "Nijenhuis".
/// </summary>
public static class NameSplitter
{
    private static readonly HashSet<string> Tussenvoegsels = new(StringComparer.OrdinalIgnoreCase)
    {
        "van", "de", "der", "den", "het", "'t", "ten", "ter", "te",
        "in", "op", "aan", "uit", "onder", "over", "voor",
        "af", "bij", "toe", "tot", "gen", "von", "vom", "vor",
        "la", "le", "les", "di", "del", "della", "da",
    };

    public static (string Voornaam, string Achternaam) Split(string naam)
    {
        if (string.IsNullOrWhiteSpace(naam))
        {
            return (string.Empty, string.Empty);
        }

        naam = naam.Trim();

        var commaIndex = naam.IndexOf(',');
        if (commaIndex >= 0)
        {
            var beforeComma = naam[..commaIndex].Trim();
            var tussenvoegsel = naam[(commaIndex + 1)..].Trim();
            var (voornaam, stam) = SplitOffLastToken(beforeComma);
            return (voornaam, FormatAchternaam(stam, tussenvoegsel));
        }

        var tokens = naam.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (tokens.Length <= 1)
        {
            return (string.Empty, naam);
        }

        var stemIndex = tokens.Length - 1;
        var prefixStart = stemIndex;
        while (prefixStart - 1 >= 1 && Tussenvoegsels.Contains(tokens[prefixStart - 1]))
        {
            prefixStart--;
        }

        var voornaamTokens = tokens[..prefixStart];
        var prefixTokens = tokens[prefixStart..stemIndex];

        return (string.Join(' ', voornaamTokens), FormatAchternaam(tokens[stemIndex], string.Join(' ', prefixTokens)));
    }

    private static string FormatAchternaam(string stam, string tussenvoegsel)
        => tussenvoegsel.Length > 0 ? $"{stam}, {tussenvoegsel}" : stam;

    private static (string Voornaam, string Stam) SplitOffLastToken(string text)
    {
        var lastSpace = text.LastIndexOf(' ');
        return lastSpace < 0 ? (string.Empty, text) : (text[..lastSpace].Trim(), text[(lastSpace + 1)..].Trim());
    }
}
