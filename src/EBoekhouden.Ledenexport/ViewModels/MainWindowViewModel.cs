using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using EBoekhouden.Ledenexport.Models;
using EBoekhouden.Ledenexport.Services;

namespace EBoekhouden.Ledenexport.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    public const int DefaultDonateurGrens = 10000;

    private readonly MemberDataService _dataService = new();
    private List<Member> _allMembers = [];

    [ObservableProperty]
    private string? _sourceFilePath;

    [ObservableProperty]
    private string _statusMessage = "Nog geen ledenlijst geladen. Klik op \"Bestand openen\" om een export (.xlsx) te laden.";

    [ObservableProperty]
    private FilterOption _selectedFilterOption = FilterOption.Alle;

    /// <summary>Lidnummer vanaf waar iemand als donateur geldt in plaats van als lid.</summary>
    [ObservableProperty]
    private int _donateurGrens = DefaultDonateurGrens;

    public ObservableCollection<ColumnInfo> Columns { get; } = [];

    public ObservableCollection<Member> FilteredMembers { get; } = [];

    public IReadOnlyList<FilterOption> FilterOptions { get; } = FilterOption.All;

    partial void OnSelectedFilterOptionChanged(FilterOption value) => ApplyFilter();

    partial void OnDonateurGrensChanged(int value) => ApplyFilter();

    public void LoadMembers(string path)
    {
        var result = _dataService.Load(path);
        _allMembers = [.. result.Members];
        SourceFilePath = path;

        Columns.Clear();
        foreach (var header in result.Headers)
        {
            Columns.Add(new ColumnInfo(header));
        }

        ApplyFilter();
    }

    public void SelectAllColumns()
    {
        foreach (var column in Columns)
        {
            column.IsSelected = true;
        }
    }

    public void DeselectAllColumns()
    {
        foreach (var column in Columns)
        {
            column.IsSelected = false;
        }
    }

    private void ApplyFilter()
    {
        FilteredMembers.Clear();
        foreach (var member in _allMembers.Where(m => SelectedFilterOption.Matches(m, DonateurGrens)))
        {
            FilteredMembers.Add(member);
        }

        if (_allMembers.Count > 0)
        {
            var leden = _allMembers.Count(m => !m.IsDonateur(DonateurGrens));
            var donateurs = _allMembers.Count(m => m.IsDonateur(DonateurGrens));
            StatusMessage = $"{_allMembers.Count} rijen geladen uit {Path.GetFileName(SourceFilePath)} " +
                             $"({leden} leden, {donateurs} donateurs bij grens {DonateurGrens}).";
        }
    }

    /// <summary>
    /// Builds the export payload: only the columns whose header checkbox is ticked,
    /// for the rows matching the currently selected leden/donateurs filter.
    /// </summary>
    public (IReadOnlyList<string> Headers, IReadOnlyList<string[]> Rows) GetExportData()
    {
        var selectedHeaders = Columns.Where(c => c.IsSelected).Select(c => c.Header).ToList();
        var rows = FilteredMembers
            .Select(member => selectedHeaders.Select(member.GetValue).ToArray())
            .ToList();

        return (selectedHeaders, rows);
    }

    public string ExportTitle => $"E-Boekhouden ledenexport – {SelectedFilterOption.Label}";
}
