using ClosedXML.Excel;
using EBoekhouden.Ledenexport.Models;

namespace EBoekhouden.Ledenexport.Services;

public sealed record LoadResult(IReadOnlyList<string> Headers, IReadOnlyList<Member> Members);

/// <summary>
/// Reads a ledenlijst export (.xlsx) into plain rows. Understands both a real Excel
/// table (as some membership software exports) and a plain sheet with no table markup
/// at all — which is what a raw e-Boekhouden export looks like: a few title/date rows,
/// then the header row, then the data. For the latter we locate the header row by
/// looking for the "Lidnummer" and "Naam" columns rather than assuming it's row 1.
/// </summary>
public sealed class MemberDataService
{
    public LoadResult Load(string path)
    {
        using var workbook = new XLWorkbook(path);
        var worksheet = workbook.Worksheets.First();

        var table = worksheet.Tables.FirstOrDefault();
        var result = table is not null ? LoadFromTable(table) : LoadFromPlainSheet(worksheet);
        return result with { Headers = WithNameColumns(result.Headers) };
    }

    private static LoadResult LoadFromTable(IXLTable table)
    {
        var headers = table.HeadersRow().Cells()
            .Select(c => c.GetString().Trim())
            .ToList();

        var members = new List<Member>();
        foreach (var row in table.DataRange.Rows())
        {
            members.Add(BuildMember(headers, row.Cells()));
        }

        return new LoadResult(headers, members);
    }

    private static LoadResult LoadFromPlainSheet(IXLWorksheet worksheet)
    {
        var rows = worksheet.RangeUsed()?.RowsUsed().ToList() ?? [];
        if (rows.Count == 0)
        {
            return new LoadResult([], []);
        }

        var headerRowIndex = rows.FindIndex(IsHeaderRow);
        if (headerRowIndex < 0)
        {
            throw new InvalidOperationException(
                "Kon geen header-rij vinden met kolommen \"Lidnummer\" en \"Naam\" in dit bestand. " +
                "Is dit een ledenlijst-export?");
        }

        var headers = rows[headerRowIndex].Cells()
            .Select(c => c.GetString().Trim())
            .ToList();

        var members = new List<Member>();
        foreach (var row in rows.Skip(headerRowIndex + 1))
        {
            members.Add(BuildMember(headers, row.Cells(1, headers.Count)));
        }

        return new LoadResult(headers, members);
    }

    /// <summary>A header row is one that has both a "Lidnummer" and a "Naam" cell — the
    /// two columns this app cannot function without, regardless of export format.</summary>
    private static bool IsHeaderRow(IXLRangeRow row)
    {
        var values = row.Cells()
            .Select(c => c.GetString().Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return values.Contains("Lidnummer") && values.Contains("Naam");
    }

    private static Member BuildMember(IReadOnlyList<string> headers, IEnumerable<IXLCell> cells)
    {
        var fields = new Dictionary<string, string>(headers.Count, StringComparer.OrdinalIgnoreCase);
        var i = 0;
        foreach (var cell in cells)
        {
            if (i >= headers.Count)
            {
                break;
            }

            var header = headers[i];
            if (!string.IsNullOrEmpty(header))
            {
                fields[header] = cell.GetString().Trim();
            }

            i++;
        }

        return new Member(fields);
    }

    /// <summary>Inserts the derived "Voornaam"/"Achternaam" columns right after "Naam" so
    /// they show up next to it in the grid, without disturbing the raw per-row parsing
    /// above (which stays aligned to the sheet's actual columns).</summary>
    private static IReadOnlyList<string> WithNameColumns(IReadOnlyList<string> headers)
    {
        var naamIndex = headers.ToList().FindIndex(h => string.Equals(h, "Naam", StringComparison.OrdinalIgnoreCase));
        if (naamIndex < 0)
        {
            return headers;
        }

        var result = new List<string>(headers);
        result.Insert(naamIndex + 1, "Voornaam");
        result.Insert(naamIndex + 2, "Achternaam");
        return result;
    }
}
