using OrbisDbTools.PS4.Discovery;
using OrbisDbTools.Utils;

namespace OrbisDbTools.PS4.AppDb
{
    public class AppDbController
    {
        private readonly AppDbProvider _dbProvider;
        private readonly DiscoveryService _discovery;

        private Uri? _localAppDb;

        public AppDbController(DiscoveryService discoveryService, AppDbProvider dbProvider)
        {
            _discovery = discoveryService;
            _dbProvider = dbProvider;
        }

        public async Task<bool> DownloadAndConnect(string consoleIp)
        {
            if (!string.IsNullOrWhiteSpace(consoleIp))
            {
                _localAppDb = await _discovery.DownloadAppDb(consoleIp);

                if (_localAppDb is not null)
                {
                    File.Copy(_localAppDb.LocalPath, $"{ClientConfig.TempDirectory.LocalPath}/app.db.{DateTimeOffset.Now.ToUnixTimeSeconds()}");
                    return await _dbProvider.OpenDatabase(_localAppDb.LocalPath);
                }
            }

            return false;
        }

        public async Task DisconnectFromConsole()
        {
            await _dbProvider.DisposeAsync();
            await _discovery.DisposeAsync();
            Console.WriteLine("Finished disconnect");
        }

        public async Task<int> HideAllKnownPsnApps()
        {
            var userAppTables = await _dbProvider.GetAppTables();

            var count = 0;

            foreach (var appTable in userAppTables)
            {
                var installedTitles = await _dbProvider.GetAllTitles(appTable);

                var knownPsnIds = KnownContent.KnownPsnApps.Select(x => x.TitleId);
                var installedPsnApps = installedTitles.Where(x => knownPsnIds.Contains(x.TitleId));

                var hidden = await _dbProvider.HideTitles(appTable, installedPsnApps);
                Console.WriteLine($"Hidden {hidden} apps in {appTable}");
                count += hidden;
            }

            return count;
        }

        public async Task<int> ReCalculateInstalledAppSizes()
        {
            var userAppTables = await _dbProvider.GetAppTables();

            var count = 0;

            foreach (var appTable in userAppTables)
            {
                var installedTitles = await _dbProvider.GetInstalledTitles(appTable);

                var titleSizes = await _discovery.CalculateTitleSize(installedTitles);

                var updatedSizes = await _dbProvider.UpdateTitleSizes(appTable, titleSizes);
                Console.WriteLine($"Updated content size for {updatedSizes} titles in {appTable}");
                count += updatedSizes;
            }

            return count;
        }

        public async Task<int> AllowDeleteInstalledApps()
        {
            var userAppTables = await _dbProvider.GetAppTables();

            var count = 0;

            foreach (var appTable in userAppTables)
            {
                var installedTitles = await _dbProvider.GetInstalledTitles(appTable);
                var allowedDelete = await _dbProvider.EnableTitleDeletion(appTable, installedTitles);
                Console.WriteLine($"Enabled deletion for {allowedDelete} apps in {appTable}");
                count += allowedDelete;
            }

            return count;
        }
    }
}