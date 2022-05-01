using LibOrbisPkg.PKG;
using LibOrbisPkg.SFO;
using OrbisDbTools.Utils.Extensions;

namespace OrbisDbTools.PS4.Models;

public record DlcPkgDataDto
{
    public DlcPkgDataDto(string titleId, string pkgPath, Header header, ParamSfo? sfo)
    {
        TitleId = titleId;
        ContentId = header.content_id;
        DirName = pkgPath.GetDirectoryNameFromDlcPkgPath() ?? string.Empty;

        Title = (sfo?["TITLE"] as Utf8Value)?.Value ?? string.Empty;
        Attribute = (sfo?["ATTRIBUTE"] as IntegerValue)?.Value.ToString() ?? string.Empty;
        Version = (sfo?["VERSION"] as Utf8Value)?.Value ?? string.Empty;
    }

	public string TitleId { get; set; }
	public string ContentId { get; set; }
	public string DirName { get; set; }

	public string Title { get; set; }
	public string Version { get; set; }
	public string Attribute { get; set; }
}