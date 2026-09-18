using EBoekhouden.Ledenexport.Models;

namespace EBoekhouden.Ledenexport.ViewModels;

/// <summary>
/// A way to narrow the grid/export down by lid vs. donateur. The lidnummer threshold
/// that decides the split is not baked in here — it comes from the user-editable
/// "Donateur vanaf lidnummer" field, so the same option is re-evaluated as that changes.
/// </summary>
public sealed record FilterOption(string Label, Func<Member, int, bool> Matches)
{
    public static readonly FilterOption Alle = new(
        "Alle (leden + donateurs)", (_, _) => true);

    public static readonly FilterOption Leden = new(
        "Alleen leden", (m, donateurGrens) => !m.IsDonateur(donateurGrens));

    public static readonly FilterOption Donateurs = new(
        "Alleen donateurs", (m, donateurGrens) => m.IsDonateur(donateurGrens));

    public static IReadOnlyList<FilterOption> All { get; } = [Alle, Leden, Donateurs];

    public override string ToString() => Label;
}
