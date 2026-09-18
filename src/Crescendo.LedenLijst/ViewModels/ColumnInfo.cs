using CommunityToolkit.Mvvm.ComponentModel;

namespace Crescendo.LedenLijst.ViewModels;

/// <summary>
/// UI state for one column of the grid: its header text and whether the checkbox
/// above that column is ticked, which decides if it is included in an export.
/// </summary>
public partial class ColumnInfo : ObservableObject
{
    public string Header { get; }

    [ObservableProperty]
    private bool _isSelected = true;

    public ColumnInfo(string header)
    {
        Header = header;
    }
}
