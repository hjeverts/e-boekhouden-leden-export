using Crescendo.LedenLijst.Models;

namespace Crescendo.LedenLijst.ViewModels;

public sealed record FilterOption(string Label, Func<Member, bool> Matches)
{
    public static readonly FilterOption Alle = new(
        "Alle (leden + donateurs)", _ => true);

    public static readonly FilterOption Leden = new(
        $"Leden (lidnummer < {Member.DonateurGrensLidnummer})", m => m.Type == MemberType.Lid);

    public static readonly FilterOption Donateurs = new(
        $"Donateurs (lidnummer >= {Member.DonateurGrensLidnummer})", m => m.Type == MemberType.Donateur);

    public static IReadOnlyList<FilterOption> All { get; } = [Alle, Leden, Donateurs];

    public override string ToString() => Label;
}
