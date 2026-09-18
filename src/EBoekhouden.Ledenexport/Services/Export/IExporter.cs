namespace EBoekhouden.Ledenexport.Services.Export;

/// <summary>
/// Writes a selected set of columns and rows to a file. All exporters share this
/// shape so the UI can call whichever one matches the format the user picked.
/// </summary>
public interface IExporter
{
    string FileExtension { get; }

    void Export(string filePath, string title, IReadOnlyList<string> headers, IReadOnlyList<string[]> rows);
}
