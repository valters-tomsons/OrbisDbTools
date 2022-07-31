using OrbisDbTools.PS4.Constants;
using OrbisDbTools.PS4.Models;
using OrbisDbTools.Utils.Connections;
using OrbisDbTools.Utils.Extensions;
using static OrbisDbTools.Utils.Extensions.SqlExtensions;
using Microsoft.Data.Sqlite;
using Dapper;
using Dapper.Contrib.Extensions;

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
    public async Task<bool> OpenDatabase(string dbPath, CancellationToken cts = default)
    {
        _dbConnection = await SqlConnectionFactory.OpenConnection(dbPath, cts);
        return _dbConnection != null;
    }

    public async Task<IEnumerable<string>> GetAppTables()
    {
        if (_dbConnection is null)
        {
            throw new Exception("Cannot query database because it's not connected.");
        }

        var tables = await _dbConnection.EnumerateTables();
        var appTables = tables?.Where(x => x.StartsWith(OrbisSystemPaths.TblAppBrowse));
        return appTables ?? Enumerable.Empty<string>();
    }

    public async Task<int> InsertAppBrowseRows(string appTable, IReadOnlyCollection<AppBrowseTblRow> rows)
    {
        if (_dbConnection is null)
        {
            throw new Exception("Cannot query database because it's not connected.");
        }

        var count = 0;
        foreach (var row in rows)
        {
            // Workaround for Dapper bug
            // Cursed, I know
            // don't care

            var sql = $@"INSERT into {appTable} 
                (titleId, contentId, titleName, metaDataPath, lastAccessTime, contentStatus, onDisc, parentalLevel, visible, sortPriority, pathInfo, lastAccessIndex, dispLocation, canRemove, category, contentType, pathInfo2, presentBoxStatus, entitlement, thumbnailUrl, lastUpdateTime, playableDate, contentSize, installDate, platform, uiCategory, skuId, disableLiveDetail, linkType, linkUri, serviceIdAddCont1, serviceIdAddCont2, serviceIdAddCont3, serviceIdAddCont4, serviceIdAddCont5, serviceIdAddCont6, serviceIdAddCont7, folderType, folderInfo, parentFolderId, positionInFolder, activeDate, entitlementTitleName, hddLocation, externalHddAppStatus, entitlementIdKamaji, mTime, freePsPlusContent, entitlementActiveFlag, sizeOtherHdd, entitlementHidden, preorderPlaceholderFlag, gatingEntitlementJson)
                values
                (@titleId, @contentId, @titleName, @metaDataPath, @lastAccessTime, @contentStatus, @onDisc, @parentalLevel, @visible, @sortPriority, @pathInfo, @lastAccessIndex, @dispLocation, @canRemove, @category, @contentType, @pathInfo2, @presentBoxStatus, @entitlement, @thumbnailUrl, @lastUpdateTime, @playableDate, @contentSize, @installDate, @platform, @uiCategory, @skuId, @disableLiveDetail, @linkType, @linkUri, @serviceIdAddCont1, @serviceIdAddCont2, @serviceIdAddCont3, @serviceIdAddCont4, @serviceIdAddCont5, @serviceIdAddCont6, @serviceIdAddCont7, @folderType, @folderInfo, @parentFolderId, @positionInFolder, @activeDate, @entitlementTitleName, @hddLocation, @externalHddAppStatus, @entitlementIdKamaji, @mTime, @freePsPlusContent, @entitlementActiveFlag, @sizeOtherHdd, @entitlementHidden, @preorderPlaceholderFlag, @gatingEntitlementJson)";

            var result = await _dbConnection.ExecuteAsync(sql, new { row.titleId, row.contentId, row.titleName, row.metaDataPath, row.lastAccessTime, row.contentStatus, row.onDisc, row.parentalLevel, row.visible, row.sortPriority, row.pathInfo, row.lastAccessIndex, row.dispLocation, row.canRemove, row.category, row.contentType, row.pathInfo2, row.presentBoxStatus, row.entitlement, row.thumbnailUrl, row.lastUpdateTime, row.playableDate, row.contentSize, row.installDate, row.platform, row.uiCategory, row.skuId, row.disableLiveDetail, row.linkType, row.linkUri, row.serviceIdAddCont1, row.serviceIdAddCont2, row.serviceIdAddCont3, row.serviceIdAddCont4, row.serviceIdAddCont5, row.serviceIdAddCont6, row.serviceIdAddCont7, row.folderType, row.folderInfo, row.parentFolderId, row.positionInFolder, row.activeDate, row.entitlementTitleName, row.hddLocation, row.externalHddAppStatus, row.entitlementIdKamaji, row.mTime, row.freePsPlusContent, row.entitlementActiveFlag, row.sizeOtherHdd, row.entitlementHidden, row.preorderPlaceholderFlag, row.gatingEntitlementJson });
            count += result;
        }

        return count;
    }

    public async Task<int> InsertAppInfoRows(IReadOnlyCollection<AppInfoTblRow> rows)
    {
        if (_dbConnection is null)
        {
            throw new Exception("Cannot query database because it's not connected.");
        }

        return await _dbConnection.InsertAsync(rows);
    }

    public async Task<IEnumerable<AppTitle>> GetInstalledTitles(string appTable)
    {
        if (_dbConnection is null)
        {
            throw new Exception("Cannot query database because it's not connected.");
        }

        if (string.IsNullOrWhiteSpace(appTable))
        {
            return Enumerable.Empty<AppTitle>();
        }

        var installedAppsSql = $@"SELECT titleId, titleName, contentId, contentSize, visible, canRemove, metaDataPath
                                from {appTable}
                                where contentId not NULL and metaDataPath like '{OrbisSystemPaths.UserAppMetadataPath}%'";

        var result = await _dbConnection.QueryAsync<AppTitle>(installedAppsSql);
        return result ?? Enumerable.Empty<AppTitle>();
    }

    public async Task<IEnumerable<AppTitle>> GetAllTitles(string appTable)
    {
        if (_dbConnection is null)
        {
            throw new Exception("Cannot query database because it's not connected.");
        }

        if (string.IsNullOrWhiteSpace(appTable))
        {
            return Enumerable.Empty<AppTitle>();
        }

        var appsSql = $"SELECT titleId, titleName, contentId, contentSize, visible, canRemove from {appTable}";

        var result = await _dbConnection.QueryAsync<AppTitle>(appsSql);
        return result ?? Enumerable.Empty<AppTitle>();
    }

    public async Task<int> UpdateTitleSizes(string appTable, IEnumerable<ContentSizeDto> titlesSizes)
    {
        if (_dbConnection is null)
        {
            throw new Exception("Cannot query database because it's not connected.");
        }

        if (string.IsNullOrWhiteSpace(appTable) || !titlesSizes.Any())
        {
            return 0;
        }

        var updateSizeSql = $"UPDATE {appTable} SET contentSize=@TotalSizeInBytes WHERE titleId=@TitleId;";
        return await _dbConnection.ExecuteAsync(updateSizeSql, titlesSizes);
    }

    public async Task<int> EnableTitleDeletion(string appTable, IEnumerable<AppTitle> titles)
    {
        if (_dbConnection is null)
        {
            throw new Exception("Cannot query database because it's not connected.");
        }

        var allowDeleteSql = $"UPDATE {appTable} set canRemove=true where titleId in @titleIds";
        return await _dbConnection.ExecuteAsync(allowDeleteSql, new { titleIds = titles.Select(x => x.TitleId) });
    }

    public async Task<int> HideTitles(string appTable, IEnumerable<AppTitle> titles)
    {
        if (_dbConnection is null)
        {
            throw new Exception("Cannot query database because it's not connected.");
        }

        var hideSql = $"UPDATE {appTable} set visible=false where titleId in @titleIds";
        return await _dbConnection.ExecuteAsync(hideSql, new { titleIds = titles.Select(x => x.TitleId) });
    }

    public async Task<int> WriteTitleChanges(string appTable, AppTitle title)
    {
        if (_dbConnection is null)
        {
            throw new Exception("Cannot query database because it's not connected.");
        }

        var sql = $"UPDATE {appTable} set canRemove=@CanRemove, titleName=@TitleName where titleId=@TitleId";
        return await _dbConnection.ExecuteAsync(sql, new { title.TitleId, title.CanRemove, title.TitleName });
    }

    public async Task<int> DeleteApp(string appTable, AppTitle title)
    {
        if (_dbConnection is null)
        {
            throw new Exception("Cannot query database because it's not connected.");
        }

        var sql = $"DELETE from {appTable} where titleId=@TitleId";
        return await _dbConnection.ExecuteAsync(sql, new { title.TitleId });
    }

    public async Task<int> DeleteAppInfo(AppTitle title)
    {
        if (_dbConnection is null)
        {
            throw new Exception("Cannot query database because it's not connected.");
        }

        const string sql = "DELETE from tbl_appinfo where titleId=@TitleId";
        return await _dbConnection.ExecuteAsync(sql, new { title.TitleId });
    }

    public async Task<long> GetExternalHddId()
    {
        if (_dbConnection is null)
        {
            throw new Exception("Cannot query database because it's not connected.");
        }

        const string sql = "SELECT status from tbl_version where category='external_hdd_id'";
        return await _dbConnection.ExecuteScalarAsync<long>(sql);
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