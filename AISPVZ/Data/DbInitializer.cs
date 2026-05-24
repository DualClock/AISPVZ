using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace AISPVZ.Data.Context;

public static class DbInitializer
{
    private static readonly string LogFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "AISPVZ", "Logs");

    private static void Log(string message)
    {
        try
        {
            Directory.CreateDirectory(LogFolder);
            var logFile = Path.Combine(LogFolder, "dbinit.log");
            File.AppendAllText(logFile, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}{Environment.NewLine}");
        }
        catch { /* silent */ }
    }

   
    public static void EnsureDecimalColumns()
    {
        try
        {
            using var db = new AppDbContext();
            var connection = db.Database.GetDbConnection();
            connection.Open();

           
            FixColumn(connection, "OrderItems", "Price", "DECIMAL(18,2)");

            
            FixColumn(connection, "IssueOperations", "TotalAmount", "DECIMAL(18,2)");

            Log("DbInitializer completed successfully.");
        }
        catch (Exception ex)
        {
            Log("DbInitializer critical error: " + ex);
          
            System.Diagnostics.Debug.WriteLine("[DbInitializer] " + ex);
        }
    }

    private static void FixColumn(System.Data.Common.DbConnection connection, string tableName, string columnName, string newType)
    {
        
        var checkSql = $@"
            SELECT COUNT(*) FROM sys.columns c
            JOIN sys.types t ON c.user_type_id = t.user_type_id
            WHERE c.object_id = OBJECT_ID('{tableName}')
              AND c.name = '{columnName}'
              AND t.name IN ('float', 'real');";

        using (var checkCmd = connection.CreateCommand())
        {
            checkCmd.CommandText = checkSql;
            var count = Convert.ToInt32(checkCmd.ExecuteScalar());
            if (count == 0)
            {
                Log($"Column {tableName}.{columnName} is already correct or does not exist.");
                return;
            }
        }

        Log($"Fixing {tableName}.{columnName} to {newType}...");

        
        var dropDefaultSql = $@"
            DECLARE @sql NVARCHAR(MAX);
            SELECT @sql = 'ALTER TABLE [{tableName}] DROP CONSTRAINT ' + QUOTENAME(d.name)
            FROM sys.default_constraints d
            JOIN sys.columns c ON d.parent_object_id = c.object_id AND d.parent_column_id = c.column_id
            WHERE c.object_id = OBJECT_ID('{tableName}') AND c.name = '{columnName}';
            IF @sql IS NOT NULL EXEC sp_executesql @sql;";

        using (var dropCmd = connection.CreateCommand())
        {
            dropCmd.CommandText = dropDefaultSql;
            dropCmd.ExecuteNonQuery();
        }

       
        var alterSql = $"ALTER TABLE [{tableName}] ALTER COLUMN [{columnName}] {newType} NOT NULL;";
        using (var alterCmd = connection.CreateCommand())
        {
            alterCmd.CommandText = alterSql;
            alterCmd.ExecuteNonQuery();
        }

        
        var addDefaultSql = $"ALTER TABLE [{tableName}] ADD DEFAULT 0 FOR [{columnName}];";
        using (var addCmd = connection.CreateCommand())
        {
            addCmd.CommandText = addDefaultSql;
            try
            {
                addCmd.ExecuteNonQuery();
            }
            catch (SqlException ex)
            {
                Log($"Could not re-add default on {tableName}.{columnName}: {ex.Message}");
            }
        }

        Log($"Column {tableName}.{columnName} fixed to {newType}.");
    }
}
