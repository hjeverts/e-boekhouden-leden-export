using ClosedXML.Excel;
using Crescendo.LedenLijst.Models;

namespace Crescendo.LedenLijst.Services;

public sealed record LoadResult(IReadOnlyList<string> Headers, IReadOnlyList<Member> Members);

/// <summary>
/// Reads a ledenlijst export (.xlsx) into plain rows. Understands both a real Excel
/// table (as exported by the membership software) and a plain sheet where the first
/// non-empty row is the header row.
/// </summary>
public sealed class MemberDataService
{
    public LoadResult Load(string path)
    {
        using var workbook = new XLWorkbook(path);
        var worksheet = workbook.Worksheets.First();

        var table = worksheet.Tables.FirstOrDefault();
        return table is not null ? LoadFromTable(table) : LoadFromPlainSheet(worksheet);
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
        var usedRows = worksheet.RangeUsed()?.RowsUsed().ToList() ?? [];
        if (usedRows.Count == 0)
        {
            return new LoadResult([], []);
        }

        var headerRow = usedRows[0];
        var headers = headerRow.Cells()
            .Select(c => c.GetString().Trim())
            .ToList();

        var members = new List<Member>();
        foreach (var row in usedRows.Skip(1))
        {
            members.Add(BuildMember(headers, row.Cells(1, headers.Count)));
        }

        return new LoadResult(headers, members);
    }

    private static Member BuildMember(IReadOnlyList<string> headers, IEnumerable<IXLCell> cells)
    {
        var fields = new Dictionary<string, string>(headers.Count);
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
}
