namespace OrbisDbTools.Utils.Extensions;

public static class StringExtensions
{
    public static string? GetTitleIdFromGamePkgPath(this string contentPath)
    {
        var fileName = contentPath.Split('/').LastOrDefault();

        if (contentPath.EndsWith(".pkg"))
        {
            return fileName?.Replace(".pkg", string.Empty);
        }

        return fileName;
    }

    public static string? GetDirectoryNameFromDlcPkgPath(this string contentPath)
    {
        var directoryPath = contentPath.Replace("/ac.pkg", string.Empty);
        return directoryPath.Split('/').LastOrDefault();
    }
}