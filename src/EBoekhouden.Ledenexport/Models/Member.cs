namespace EBoekhouden.Ledenexport.Models;

/// <summary>
/// One row from the ledenlijst. Values are kept as the formatted strings shown in the
/// source spreadsheet, keyed by column header, so the grid and exporters need no per-column code.
/// </summary>
public sealed class Member
{
    public IReadOnlyDictionary<string, string> Fields { get; }

    public int Lidnummer { get; }

    /// <summary>Derived from "Naam" — see <see cref="NameSplitter"/> for the splitting rules.</summary>
    public string Voornaam { get; }

    /// <summary>Formatted as "Stam, tussenvoegsel" (e.g. "Voorbeeld, van") so a plain string
    /// sort already follows the normal Dutch phone-book convention.</summary>
    public string Achternaam { get; }

    public Member(IReadOnlyDictionary<string, string> fields)
    {
        Lidnummer = fields.TryGetValue("Lidnummer", out var raw) && int.TryParse(raw, out var n) ? n : 0;

        var naam = fields.TryGetValue("Naam", out var naamValue) ? naamValue : string.Empty;
        (Voornaam, Achternaam) = NameSplitter.Split(naam);

        var extended = new Dictionary<string, string>(fields, StringComparer.OrdinalIgnoreCase)
        {
            ["Voornaam"] = Voornaam,
            ["Achternaam"] = Achternaam,
        };
        Fields = extended;
    }

    /// <summary>True when this member's lidnummer is at or above the (user-configurable) donateur threshold.</summary>
    public bool IsDonateur(int donateurGrens) => Lidnummer >= donateurGrens;

    public string GetValue(string columnHeader)
        => Fields.TryGetValue(columnHeader, out var value) ? value : string.Empty;
}
