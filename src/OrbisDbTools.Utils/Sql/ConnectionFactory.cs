using Microsoft.Data.Sqlite;

namespace OrbisDbTools.Utils;

public class ConnectionFactory
{
    public async Task<SqliteConnection?> OpenConnection(string dataSource = Constants.AppDbFileName)
    {
        if (!File.Exists(dataSource))
        {
            return null;
        }

        var conn = new SqliteConnection($"Data Source={dataSource}");
        await conn.OpenAsync();
        return conn;
    }
}
