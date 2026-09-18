using System.IO.Compression;
using System.Text;
using System.Xml.Linq;

namespace EBoekhouden.Ledenexport.Services.Export;

/// <summary>
/// Hand-rolled, minimal OpenDocument Spreadsheet (.ods) writer. An .ods file is a zip
/// with a fixed "mimetype" entry stored first and uncompressed, plus a manifest and a
/// content.xml describing a single sheet. Everything is written as a text cell, which
/// keeps the format simple and is enough for exporting a member list.
/// </summary>
public sealed class OdsExporter : IExporter
{
    private const string MimeType = "application/vnd.oasis.opendocument.spreadsheet";

    public string FileExtension => ".ods";

    public void Export(string filePath, string title, IReadOnlyList<string> headers, IReadOnlyList<string[]> rows)
    {
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        using var stream = new FileStream(filePath, FileMode.CreateNew);
        using var archive = new ZipArchive(stream, ZipArchiveMode.Create);

        var mimetypeEntry = archive.CreateEntry("mimetype", CompressionLevel.NoCompression);
        using (var entryStream = mimetypeEntry.Open())
        using (var writer = new StreamWriter(entryStream, new UTF8Encoding(false)))
        {
            writer.Write(MimeType);
        }

        WriteTextEntry(archive, "META-INF/manifest.xml", BuildManifest());
        WriteTextEntry(archive, "content.xml", BuildContent(title, headers, rows).ToString(SaveOptions.DisableFormatting));
    }

    private static void WriteTextEntry(ZipArchive archive, string entryName, string content)
    {
        var entry = archive.CreateEntry(entryName, CompressionLevel.Optimal);
        using var entryStream = entry.Open();
        using var writer = new StreamWriter(entryStream, new UTF8Encoding(false));
        writer.Write(content);
    }

    private static string BuildManifest() =>
        $"""
         <?xml version="1.0" encoding="UTF-8"?>
         <manifest:manifest xmlns:manifest="urn:oasis:names:tc:opendocument:xmlns:manifest:1.0" manifest:version="1.2">
           <manifest:file-entry manifest:full-path="/" manifest:version="1.2" manifest:media-type="{MimeType}"/>
           <manifest:file-entry manifest:full-path="content.xml" manifest:media-type="text/xml"/>
         </manifest:manifest>
         """;

    private static XDocument BuildContent(string title, IReadOnlyList<string> headers, IReadOnlyList<string[]> rows)
    {
        XNamespace office = "urn:oasis:names:tc:opendocument:xmlns:office:1.0";
        XNamespace table = "urn:oasis:names:tc:opendocument:xmlns:table:1.0";
        XNamespace text = "urn:oasis:names:tc:opendocument:xmlns:text:1.0";

        XElement TextCell(string value) =>
            new(table + "table-cell",
                new XAttribute(office + "value-type", "string"),
                new XElement(text + "p", value));

        var headerRow = new XElement(table + "table-row", headers.Select(TextCell));
        var dataRows = rows.Select(row => new XElement(table + "table-row", row.Select(TextCell)));

        var sheet = new XElement(table + "table",
            new XAttribute(table + "name", SanitizeSheetName(title)),
            headers.Select(_ => new XElement(table + "table-column")),
            headerRow,
            dataRows);

        var content = new XElement(office + "document-content",
            new XAttribute(XNamespace.Xmlns + "office", office),
            new XAttribute(XNamespace.Xmlns + "table", table),
            new XAttribute(XNamespace.Xmlns + "text", text),
            new XAttribute(office + "version", "1.2"),
            new XElement(office + "body",
                new XElement(office + "spreadsheet", sheet)));

        return new XDocument(new XDeclaration("1.0", "UTF-8", null), content);
    }

    private static string SanitizeSheetName(string title)
    {
        var name = string.IsNullOrWhiteSpace(title) ? "Ledenlijst" : title;
        foreach (var invalid in new[] { '[', ']', '*', '?', ':', '/', '\\' })
        {
            name = name.Replace(invalid, '-');
        }

        return name.Length > 31 ? name[..31] : name;
    }
}
