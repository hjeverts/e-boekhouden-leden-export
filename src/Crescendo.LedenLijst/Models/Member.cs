namespace Crescendo.LedenLijst.Models;

/// <summary>
/// One row from the ledenlijst. Values are kept as the formatted strings shown in the
/// source spreadsheet, keyed by column header, so the grid and exporters need no per-column code.
/// </summary>
public sealed class Member
{
    public IReadOnlyDictionary<string, string> Fields { get; }

    public int Lidnummer { get; }

    public Member(IReadOnlyDictionary<string, string> fields)
    {
        Fields = fields;
        Lidnummer = fields.TryGetValue("Lidnummer", out var raw) && int.TryParse(raw, out var n) ? n : 0;
    }

    /// <summary>True when this member's lidnummer is at or above the (user-configurable) donateur threshold.</summary>
    public bool IsDonateur(int donateurGrens) => Lidnummer >= donateurGrens;

    public string GetValue(string columnHeader)
        => Fields.TryGetValue(columnHeader, out var value) ? value : string.Empty;
}
