using Dapper;
using Microsoft.Data.Sqlite;
using OrbisDbTools.PS4.Models;
using OrbisDbTools.Utils.Connections;

namespace OrbisDbTools.Lib.Providers;

/// <summary>
/// Layer for interacting with Orbis additional content database at <c>/system_data/priv/mms/addcont.db</c>
/// </summary>
public class AddContDbProvider : IAsyncDisposable
{
	private SqliteConnection? _dbConnection;

	public async ValueTask DisposeAsync()
	{
        if (_dbConnection is not null)
        {
            await _dbConnection.CloseAsync();
            await _dbConnection.DisposeAsync();
        }

        GC.SuppressFinalize(this);
	}

    /// <summary>
    /// Opens a connection to a local database file
    /// </summary>
    /// <param name="dbPath">Local path to SQLite database</param>
    public async Task<bool> OpenDatabase(string dbPath)
    {
        _dbConnection = await SqlConnectionFactory.OpenConnection(dbPath);
        return _dbConnection != null;
    }

    public async Task<IEnumerable<AddContTblRow>> GetInstalledContent()
    {
        if (_dbConnection is null)
        {
            throw new Exception("Cannot query database because it's not connected.");
        }

        const string sql =
            @"select 
                id, title_id, dir_name, content_id, title, version, attribute, status 
            from addcont
            where status != 2";
        return await _dbConnection.QueryAsync<AddContTblRow>(sql);
    }
}