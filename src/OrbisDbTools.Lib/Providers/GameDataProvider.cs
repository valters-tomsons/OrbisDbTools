using LibOrbisPkg.PKG;
using LibOrbisPkg.SFO;

namespace OrbisDbTools.Lib.Providers;

public class GameDataProvider
{
    public async Task<ParamSfo?> ParseLocalSfo(Uri sfoPath)
    {
        var fileStream = new FileStream(sfoPath.LocalPath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 2048, true);
        return await ParamSfo.FromStreamAsync(fileStream);
    }

    public async Task<(Header? header, ParamSfo? sfo)> GetDlcPkgData(Stream remoteStream)
    {
		var pkgReader = new PkgReader(remoteStream);
        var pkgData = pkgReader.ReadRemotePkgData();

        var localSfoStream = new MemoryStream(pkgData.sfoBuffer);
        return (pkgData.header, await ParamSfo.FromStreamAsync(localSfoStream));
    }
}