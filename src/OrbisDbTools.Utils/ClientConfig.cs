using System.Runtime.InteropServices;
namespace OrbisDbTools.Utils;

public static class ClientConfig
{
    public static Uri TempDirectory { get; }

    private const string ToolsFolderName = "orbisdbtools";

    static ClientConfig()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var tmpdir = Environment.GetEnvironmentVariable("%Temp%");
            TempDirectory = new($"{tmpdir}/{ToolsFolderName}");
        }
        else
        {
            var tmpdir = Environment.GetEnvironmentVariable("TMPDIR");
            if (!string.IsNullOrWhiteSpace(tmpdir))
            {
                TempDirectory = new($"{tmpdir}/{ToolsFolderName}");
            }

            TempDirectory = new($"/tmp/{ToolsFolderName}");
        }
    }
}