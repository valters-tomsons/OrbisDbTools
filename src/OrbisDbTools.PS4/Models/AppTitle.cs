using OrbisDbTools.PS4.Constants;

namespace OrbisDbTools.PS4.Models;

public record AppTitle : IEquatable<AppTitle>
{
    private string titleName = string.Empty;

    public AppTitle() { }
    public AppTitle(string titleId, string titleName) { TitleId = titleId; TitleName = titleName; }

    public string TitleId { get; init; } = string.Empty;

    public string TitleName
    {
        get { return titleName; }
        set { if (value.Length > 1) titleName = value; }
    }

    public string ContentId { get; init; } = string.Empty;
    public string MetaDataPath { get; init; } = string.Empty;
    public string? Category { get; init; }
    public ulong ContentSize { get; init; }
    public bool Visible { get; init; }
    public bool CanRemove { get; init; }

    public bool ExternalStorage => MetaDataPath.StartsWith(OrbisSystemPaths.UserExternalAppMetadata);
}