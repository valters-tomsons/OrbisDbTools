using LibOrbisPkg.SFO;
using OrbisDbTools.PS4.Constants;

namespace OrbisDbTools.PS4.Models;

public record FsTitle
{
    public FsTitle(string titleId, string contentPath) { TitleId = titleId; ContentPath = contentPath; }

    public string TitleId { get; init; }
    public string ContentPath { get; init; }
    public ParamSfo? SFO { get; set; }

    public bool ExternalStorage => ContentPath.StartsWith(OrbisSystemPaths.UserExternalAppPath);
}