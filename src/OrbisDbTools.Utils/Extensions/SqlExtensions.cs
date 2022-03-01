using Dapper;
using Microsoft.Data.Sqlite;

namespace OrbisDbTools.Utils.Extensions;

public static class SqlExtensions
{
    public async static Task<IEnumerable<string>> EnumerateTables(this SqliteConnection? conn)
    {
        if (conn is null)
        {
            return Enumerable.Empty<string>();
        }

        const string tablesQuerySql = @"
                            SELECT name
                            FROM sqlite_schema
                            WHERE 
                                type ='table' AND 
                                name NOT LIKE 'sqlite_%';";

        return await conn.QueryAsync<string>(tablesQuerySql);
    }
}