using System.Text;
using Microsoft.Data.Sqlite;

namespace OrbisDbTools.Utils.Connections;

public static class SqlConnectionFactory
{
    public static async Task<SqliteConnection?> OpenConnection(string dataSource)
    {
        await AssertFileIntegrity(dataSource);

        var conn = new SqliteConnection($"Data Source={dataSource}");
        await conn.OpenAsync();
        return conn;
    }

    /// <summary>
    /// Throws an exception if target file fails validation
    /// </summary>
    private static async Task AssertFileIntegrity(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new Exception("Failed to locate file");
        }

        await ValidateFileSignature(filePath);
    }

    private static async Task ValidateFileSignature(string filePath)
    {
        using var reader = new FileStream(filePath, FileMode.Open);
        var signatureBuffer = new byte[32];
        await reader.ReadAsync(signatureBuffer);

        var signatureString = Encoding.ASCII.GetString(signatureBuffer);
        if (!signatureString.StartsWith("SQLite format 3"))
        {
            throw new Exception("File is not a SQLite 3 database");
        }
    }
}
