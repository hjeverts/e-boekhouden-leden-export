namespace EBoekhouden.Ledenexport.Models;

/// <summary>
/// Splits a single "Naam" field into voornaam/achternaam for sorting and export.
///
/// The membership export already encodes a tussenvoegsel with a comma, e.g.
/// "H.J. Voorbeeld, ten" (= "ten Voorbeeld") or "A.B. Proef, van den" (= "van den Proef");
/// when that comma is present we trust it directly. Without a comma (e.g. a plain
/// "Piet van Voorbeeld") we detect the tussenvoegsel ourselves from a fixed word list.
/// In both of those cases achternaam is written as "Stam, tussenvoegsel" (e.g.
/// "Voorbeeld, van") so a plain string sort already groups people by surname stem,
/// ignoring the prefix, which is the normal Dutch phone-book convention.
///
/// A double-barrelled surname (from a marriage) can itself carry a tussenvoegsel on its
/// second half, glued on with a hyphen and no comma, e.g. "J.E. Klaassen-ten Voorbeeld"
/// (= Klaassen + ten Voorbeeld). That whole tail — from the hyphenated token to the end
/// of the field — is always the achternaam here, so it is kept exactly as written
/// (tussenvoegsel left in place, not moved into a trailing ", tussenvoegsel") rather than
/// reparsed as "voornaam tussenvoegsel achternaam".
///
/// A plain hyphenated last token with no such embedded tussenvoegsel (e.g.
/// "Bakker-Jansen") is always kept whole as the achternaam stem and never split on the
/// hyphen.
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

        var doubleSurnameStart = FindDoubleSurnameWithEmbeddedTussenvoegsel(tokens);
        if (doubleSurnameStart >= 0)
        {
            return (string.Join(' ', tokens[..doubleSurnameStart]), string.Join(' ', tokens[doubleSurnameStart..]));
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

    /// <summary>Finds a token like "Klaassen-ten" (word-hyphen-tussenvoegsel) that starts a
    /// double-barrelled surname; the surname always runs to the end of the field. Returns
    /// -1 when no such token exists. At least one token is always left for voornaam.</summary>
    private static int FindDoubleSurnameWithEmbeddedTussenvoegsel(string[] tokens)
    {
        for (var i = 1; i < tokens.Length; i++)
        {
            var hyphenIndex = tokens[i].IndexOf('-');
            if (hyphenIndex < 0 || hyphenIndex == tokens[i].Length - 1)
            {
                continue;
            }

            var suffix = tokens[i][(hyphenIndex + 1)..];
            if (Tussenvoegsels.Contains(suffix))
            {
                return i;
            }
        }

        return -1;
    }

    private static string FormatAchternaam(string stam, string tussenvoegsel)
        => tussenvoegsel.Length > 0 ? $"{stam}, {tussenvoegsel}" : stam;

    private static (string Voornaam, string Stam) SplitOffLastToken(string text)
    {
        var lastSpace = text.LastIndexOf(' ');
        return lastSpace < 0 ? (string.Empty, text) : (text[..lastSpace].Trim(), text[(lastSpace + 1)..].Trim());
    }
}
