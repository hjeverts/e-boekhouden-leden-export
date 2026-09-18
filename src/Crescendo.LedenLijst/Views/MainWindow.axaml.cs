using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Platform.Storage;
using Crescendo.LedenLijst.Models;
using Crescendo.LedenLijst.Services.Export;
using Crescendo.LedenLijst.ViewModels;

namespace Crescendo.LedenLijst.Views;

public partial class MainWindow : Window
{
    private static readonly FilePickerFileType XlsxFileType = new("Excel-bestand (*.xlsx)")
    {
        Patterns = ["*.xlsx"],
    };

    public MainWindow()
    {
        InitializeComponent();
        DataContextChanged += (_, _) =>
        {
            if (ViewModel is not null)
            {
                ViewModel.Columns.CollectionChanged += (_, _) => RebuildGridColumns();
            }
        };
    }

    private MainWindowViewModel? ViewModel => DataContext as MainWindowViewModel;

    private async void OnOpenFileClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Kies de ledenlijst-export",
            AllowMultiple = false,
            FileTypeFilter = [XlsxFileType],
        });

        var file = files.Count > 0 ? files[0] : null;
        if (file?.TryGetLocalPath() is not { } path)
        {
            return;
        }

        try
        {
            ViewModel?.LoadMembers(path);
        }
        catch (Exception ex)
        {
            if (ViewModel is not null)
            {
                ViewModel.StatusMessage = $"Kon bestand niet laden: {ex.Message}";
            }
        }
    }

    private void OnSelectAllColumnsClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        => ViewModel?.SelectAllColumns();

    private void OnDeselectAllColumnsClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        => ViewModel?.DeselectAllColumns();

    private async void OnExportCsvClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        => await ExportAsync(new CsvExporter(), "csv");

    private async void OnExportPdfClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        => await ExportAsync(new PdfExporter(), "pdf");

    private async void OnExportOdsClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        => await ExportAsync(new OdsExporter(), "ods");

    private async Task ExportAsync(IExporter exporter, string extensionLabel)
    {
        var vm = ViewModel;
        if (vm is null)
        {
            return;
        }

        var (headers, rows) = vm.GetExportData();
        if (headers.Count == 0)
        {
            vm.StatusMessage = "Selecteer minstens één kolom voordat je exporteert.";
            return;
        }

        var suggestedName = $"ledenlijst-{DateTime.Now:yyyy-MM-dd}{exporter.FileExtension}";
        var file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = $"Exporteren als {extensionLabel.ToUpperInvariant()}",
            SuggestedFileName = suggestedName,
            DefaultExtension = exporter.FileExtension.TrimStart('.'),
            FileTypeChoices = [new FilePickerFileType(extensionLabel.ToUpperInvariant()) { Patterns = [$"*{exporter.FileExtension}"] }],
        });

        var path = file?.TryGetLocalPath();
        if (path is null)
        {
            return;
        }

        try
        {
            exporter.Export(path, vm.ExportTitle, headers, rows);
            vm.StatusMessage = $"{rows.Count} rijen, {headers.Count} kolommen geëxporteerd naar {Path.GetFileName(path)}.";
        }
        catch (Exception ex)
        {
            vm.StatusMessage = $"Export mislukt: {ex.Message}";
        }
    }

    private void RebuildGridColumns()
    {
        var vm = ViewModel;
        MembersGrid.Columns.Clear();
        if (vm is null)
        {
            return;
        }

        foreach (var columnInfo in vm.Columns)
        {
            MembersGrid.Columns.Add(BuildColumn(columnInfo));
        }
    }

    private static DataGridTemplateColumn BuildColumn(ColumnInfo columnInfo)
    {
        var header = columnInfo.Header;

        return new DataGridTemplateColumn
        {
            Header = BuildHeaderContent(columnInfo),
            Width = new DataGridLength(150),
            CellTemplate = new FuncDataTemplate<Member>((member, _) => new TextBlock
            {
                Text = member?.GetValue(header) ?? string.Empty,
                Padding = new Avalonia.Thickness(6, 3),
                TextTrimming = Avalonia.Media.TextTrimming.CharacterEllipsis,
            }),
        };
    }

    private static Control BuildHeaderContent(ColumnInfo columnInfo)
    {
        var checkBox = new CheckBox { VerticalAlignment = VerticalAlignment.Center };
        checkBox.Bind(ToggleButton.IsCheckedProperty, new Binding(nameof(ColumnInfo.IsSelected)));

        var text = new TextBlock
        {
            Text = columnInfo.Header,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Avalonia.Thickness(4, 0, 0, 0),
            FontWeight = Avalonia.Media.FontWeight.SemiBold,
            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
        };

        return new StackPanel
        {
            Orientation = Orientation.Horizontal,
            DataContext = columnInfo,
            Children = { checkBox, text },
        };
    }
}
