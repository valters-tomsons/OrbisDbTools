using LibOrbisPkg.SFO;

namespace OrbisDbTools.Lib.Providers
{
    public class GameDataProvider
    {
        public async Task<ParamSfo?> ReadSfo(Uri sfoPath)
        {
            var fileStream = new FileStream(sfoPath.LocalPath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 2048, true);
            return await ParamSfo.FromStreamAsync(fileStream);
        }
    }
}