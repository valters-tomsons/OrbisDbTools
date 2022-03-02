using OrbisDbTools.PS4.Constants;
using OrbisDbTools.PS4.Models;
using OrbisDbTools.Utils.Connections;
using OrbisDbTools.Utils.Extensions;
using static OrbisDbTools.Utils.Extensions.SqlExtensions;
using Microsoft.Data.Sqlite;
using Dapper;

namespace OrbisDbTools.Lib.Providers;

/// <summary>
/// Layer for interacting with Orbis app database at <c>/system_data/priv/mms/app.db</c>
/// </summary>
public class AppDbProvider : IAsyncDisposable
{
    private SqliteConnection? _dbConnection;

    /// <summary>
    /// Opens a connection to a local database file
    /// </summary>
    /// <param name="dbPath">Local path to SQLite database</param>
    public async Task<bool> OpenDatabase(string dbPath)
    {
        _dbConnection = await SqlConnectionFactory.OpenConnection(dbPath);
        return _dbConnection != null;
    }

    public async Task<IEnumerable<string>> GetAppTables()
    {
        var tables = await _dbConnection?.EnumerateTables();
        var appTables = tables?.Where(x => x.StartsWith(OrbisSystemPaths.TblAppBrowse));
        return appTables ?? Enumerable.Empty<string>();
    }

    public async Task<IEnumerable<AppTitle>> GetInstalledTitles(string appTable)
    {
        if (string.IsNullOrWhiteSpace(appTable))
        {
            return Enumerable.Empty<AppTitle>();
        }

        var installedAppsSql = $@"SELECT titleId, titleName, contentId, contentSize, visible, canRemove, metaDataPath
                                from {appTable}
                                where contentId not NULL and metaDataPath like '{OrbisSystemPaths.UserAppMetadataPath}%'";

        var result = await _dbConnection?.QueryAsync<AppTitle>(installedAppsSql);
        return result ?? Enumerable.Empty<AppTitle>();
    }

    public async Task<IEnumerable<AppTitle>> GetAllTitles(string appTable)
    {
        if (string.IsNullOrWhiteSpace(appTable))
        {
            return Enumerable.Empty<AppTitle>();
        }

        var appsSql = $"SELECT titleId, titleName, contentId, contentSize, visible, canRemove from {appTable}";

        var result = await _dbConnection?.QueryAsync<AppTitle>(appsSql);
        return result ?? Enumerable.Empty<AppTitle>();
    }

    public async Task<int> UpdateTitleSizes(string appTable, IEnumerable<ContentSizeDto> titlesSizes)
    {
        if (string.IsNullOrWhiteSpace(appTable) || !titlesSizes.Any())
        {
            return 0;
        }

        var updateSizeSql = $"UPDATE {appTable} SET contentSize=@TotalSizeInBytes WHERE titleId=@TitleId;";
        return await _dbConnection?.ExecuteAsync(updateSizeSql, titlesSizes);
    }

    public async Task<int> EnableTitleDeletion(string appTable, IEnumerable<AppTitle> titles)
    {
        var allowDeleteSql = $"UPDATE {appTable} set canRemove=true where titleId in @titleIds";
        return await _dbConnection?.ExecuteAsync(allowDeleteSql, new { titleIds = titles.Select(x => x.TitleId) });
    }

    public async Task<int> HideTitles(string appTable, IEnumerable<AppTitle> titles)
    {
        var hideSql = $"UPDATE {appTable} set visible=false where titleId in @titleIds";
        return await _dbConnection?.ExecuteAsync(hideSql, new { titleIds = titles.Select(x => x.TitleId) });
    }

    public async ValueTask DisposeAsync()
    {
        if (_dbConnection is not null)
        {
            await _dbConnection.CloseAsync();
            await _dbConnection.DisposeAsync();
        }

        GC.SuppressFinalize(this);
    }
}