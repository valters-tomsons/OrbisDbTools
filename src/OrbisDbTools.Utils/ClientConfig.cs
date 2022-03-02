namespace OrbisDbTools.Utils;

public static class ClientConfig
{
    public static Uri TempDirectory { get; }

    private const string ToolsFolderName = "OrbisDbTools";
    private const string DefaultTmpPath = "/tmp/";

    static ClientConfig()
    {
        var tmpdir = Path.GetTempPath() ?? DefaultTmpPath;
        if (string.IsNullOrWhiteSpace(tmpdir))
        {
            tmpdir = DefaultTmpPath;
        }

        TempDirectory = new(Path.Combine(tmpdir, ToolsFolderName));
    }
}