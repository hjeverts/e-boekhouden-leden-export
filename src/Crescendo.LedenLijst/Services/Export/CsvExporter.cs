using System.Text;

namespace Crescendo.LedenLijst.Services.Export;

/// <summary>
/// Writes semicolon-separated CSV (the delimiter Excel/LibreOffice on a Dutch locale
/// expects by default) with UTF-8 BOM so accented characters render correctly.
/// </summary>
public sealed class CsvExporter : IExporter
{
    private const char Delimiter = ';';

    public string FileExtension => ".csv";

    public void Export(string filePath, string title, IReadOnlyList<string> headers, IReadOnlyList<string[]> rows)
    {
        using var writer = new StreamWriter(filePath, false, new UTF8Encoding(true));

        writer.WriteLine(string.Join(Delimiter, headers.Select(Escape)));
        foreach (var row in rows)
        {
            writer.WriteLine(string.Join(Delimiter, row.Select(Escape)));
        }
    }

    private static string Escape(string value)
    {
        if (value.Contains(Delimiter) || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        return value;
    }
}
