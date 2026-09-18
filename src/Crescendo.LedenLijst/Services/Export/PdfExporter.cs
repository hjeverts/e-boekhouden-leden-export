using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Crescendo.LedenLijst.Services.Export;

/// <summary>
/// Renders the selected columns/rows as a landscape, paginated PDF table via QuestPDF.
/// </summary>
public sealed class PdfExporter : IExporter
{
    public string FileExtension => ".pdf";

    public void Export(string filePath, string title, IReadOnlyList<string> headers, IReadOnlyList<string[]> rows)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(24);
                page.DefaultTextStyle(x => x.FontSize(8));

                page.Header().Column(column =>
                {
                    column.Item().Text(title).FontSize(16).Bold();
                    column.Item().Text($"Gegenereerd op {DateTime.Now:dd-MM-yyyy HH:mm}  ·  {rows.Count} rijen").FontSize(9).FontColor(Colors.Grey.Darken1);
                    column.Item().PaddingTop(6).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
                });

                page.Content().PaddingTop(10).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        foreach (var _ in headers)
                        {
                            columns.RelativeColumn();
                        }
                    });

                    table.Header(header =>
                    {
                        foreach (var headerText in headers)
                        {
                            header.Cell().Element(HeaderCellStyle).Text(headerText).Bold();
                        }
                    });

                    var alternate = false;
                    foreach (var row in rows)
                    {
                        var background = alternate ? Colors.Grey.Lighten4 : Colors.White;
                        alternate = !alternate;

                        foreach (var cellValue in row)
                        {
                            table.Cell().Element(container => DataCellStyle(container, background)).Text(cellValue);
                        }
                    }
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.CurrentPageNumber();
                    x.Span(" / ");
                    x.TotalPages();
                });
            });
        });

        document.GeneratePdf(filePath);
    }

    private static IContainer HeaderCellStyle(IContainer container) =>
        container
            .Background(Colors.Grey.Darken2)
            .PaddingVertical(4)
            .PaddingHorizontal(3)
            .DefaultTextStyle(x => x.FontColor(Colors.White));

    private static IContainer DataCellStyle(IContainer container, string background) =>
        container
            .Background(background)
            .BorderBottom(0.5f)
            .BorderColor(Colors.Grey.Lighten2)
            .PaddingVertical(3)
            .PaddingHorizontal(3);
}
