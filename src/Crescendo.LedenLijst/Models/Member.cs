namespace Crescendo.LedenLijst.Models;

public enum MemberType
{
    Lid,
    Donateur
}

/// <summary>
/// One row from the ledenlijst. Values are kept as the formatted strings shown in the
/// source spreadsheet, keyed by column header, so the grid and exporters need no per-column code.
/// </summary>
public sealed class Member
{
    public const int DonateurGrensLidnummer = 10000;

    public IReadOnlyDictionary<string, string> Fields { get; }

    public int Lidnummer { get; }

    public MemberType Type => Lidnummer >= DonateurGrensLidnummer ? MemberType.Donateur : MemberType.Lid;

    public Member(IReadOnlyDictionary<string, string> fields)
    {
        Fields = fields;
        Lidnummer = fields.TryGetValue("Lidnummer", out var raw) && int.TryParse(raw, out var n) ? n : 0;
    }

    public string GetValue(string columnHeader)
        => Fields.TryGetValue(columnHeader, out var value) ? value : string.Empty;
}
