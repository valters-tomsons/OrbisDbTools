namespace OrbisDbTools.PS4.Models;

public record AppTblRow
{
    public string TitleId { get; init; } = string.Empty;
    public string TitleName { get; init; } = string.Empty;
    public string ContentId { get; init; } = string.Empty;
    public string MetaDataPath { get; init; } = string.Empty;
    public string? Category { get; init; }
    public ulong ContentSize { get; init; }
    public bool Visible { get; init; }
    public bool CanRemove { get; init; }
}