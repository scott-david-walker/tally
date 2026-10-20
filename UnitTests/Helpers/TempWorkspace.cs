namespace UnitTests.Helpers;

/// <summary>A throwaway directory for writing YAML fixtures on disk, cleaned up on dispose.</summary>
internal sealed class TempWorkspace : IDisposable
{
    private string Root { get; } = Path.Combine(Path.GetTempPath(), "testcli-tests-" + Guid.NewGuid().ToString("N"));

    public TempWorkspace() => Directory.CreateDirectory(Root);

    public string Write(string relativePath, string content)
    {
        var full = Path.Combine(Root, relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(full)!);
        File.WriteAllText(full, content);
        return full;
    }

    public void Dispose()
    {
        try { Directory.Delete(Root, recursive: true); }
        catch { /* best effort */ }
    }
}