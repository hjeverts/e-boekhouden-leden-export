using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Crescendo.LedenLijst.Models;
using Crescendo.LedenLijst.Services;

namespace Crescendo.LedenLijst.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly MemberDataService _dataService = new();
    private List<Member> _allMembers = [];

    [ObservableProperty]
    private string? _sourceFilePath;

    [ObservableProperty]
    private string _statusMessage = "Nog geen ledenlijst geladen. Klik op \"Bestand openen\" om een export (.xlsx) te laden.";

    [ObservableProperty]
    private FilterOption _selectedFilterOption = FilterOption.Alle;

    public ObservableCollection<ColumnInfo> Columns { get; } = [];

    public ObservableCollection<Member> FilteredMembers { get; } = [];

    public IReadOnlyList<FilterOption> FilterOptions { get; } = FilterOption.All;

    partial void OnSelectedFilterOptionChanged(FilterOption value) => ApplyFilter();

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

        var leden = _allMembers.Count(m => m.Type == MemberType.Lid);
        var donateurs = _allMembers.Count(m => m.Type == MemberType.Donateur);
        StatusMessage = $"{_allMembers.Count} rijen geladen uit {Path.GetFileName(path)} " +
                         $"({leden} leden, {donateurs} donateurs).";
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
        foreach (var member in _allMembers.Where(SelectedFilterOption.Matches))
        {
            FilteredMembers.Add(member);
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

    public string ExportTitle => $"Crescendo ledenlijst – {SelectedFilterOption.Label}";
}
