namespace OrbisDbTools.PS4.Models;

public record FsTitle
{
    public FsTitle(string titleId, string contentPath) { TitleId = titleId; ContentPath = contentPath; }

    public string TitleId { get; init; }
    public string ContentPath { get; init; }
}