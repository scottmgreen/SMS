using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Data.SqlClient;
using System.Data;

namespace PDXSMS_UnitTests.Utilities;

/// <summary>
/// Database cleanup utilities for SMS integration tests
/// Provides methods to clean up test data and reset database state
/// </summary>
public class DatabaseCleanupUtility
{
    private readonly string _connectionString;
    private readonly ILogger<DatabaseCleanupUtility> _logger;

    public DatabaseCleanupUtility(IConfiguration configuration, ILogger<DatabaseCleanupUtility> logger)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnectionString") 
            ?? throw new InvalidOperationException("Database connection string is missing.");
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Truncates all SMS tables using direct SQL commands
    /// This provides a clean slate for integration tests
    /// </summary>
    /// <returns>True if successful, false if there was an error</returns>
    public async Task<bool> TruncateAllTablesAsync()
    {
        try
        {
            _logger.LogInformation("Starting database cleanup - truncating all SMS tables...");

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            // Disable foreign key constraints to avoid referential integrity issues
            var disableConstraints = "EXEC sp_MSforeachtable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL'";
            using (var cmd = new SqlCommand(disableConstraints, connection))
            {
                await cmd.ExecuteNonQueryAsync();
            }

            // Truncate tables in dependency order (child tables first)
            var tablesToTruncate = new[]
            {
                "tbls_AuditLog",
                "tbld_ScoringPanel", 
                "tbld_MitigationAssignments",
                "tbld_Interviews",
                "tbld_ReportValidations",
                "tbld_RiskAssessments",
                "tbld_RiskAnalysis", 
                "tbld_Mitigations",
                "tbld_Investigations",
                "tbld_AirportSharedDataset",
                "tbld_Hazards",
                "tbld_Reports"
            };

            foreach (var tableName in tablesToTruncate)
            {
                try
                {
                    var truncateQuery = $"TRUNCATE TABLE [dbo].[{tableName}]";
                    using var cmd = new SqlCommand(truncateQuery, connection);
                    await cmd.ExecuteNonQueryAsync();
                    _logger.LogDebug("Truncated table: {TableName}", tableName);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("Could not truncate table {TableName}: {Error}", tableName, ex.Message);
                    // Continue with other tables - some might not exist or have data
                }
            }

            // Re-enable foreign key constraints
            var enableConstraints = "EXEC sp_MSforeachtable 'ALTER TABLE ? CHECK CONSTRAINT ALL'";
            using (var cmd = new SqlCommand(enableConstraints, connection))
            {
                await cmd.ExecuteNonQueryAsync();
            }

            _logger.LogInformation("Database cleanup completed successfully at {Timestamp}", DateTime.UtcNow);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred during database cleanup");
            return false;
        }
    }

    /// <summary>
    /// Truncates specific SMS tables in the correct dependency order
    /// Useful for targeted cleanup of specific entities
    /// </summary>
    /// <param name="tableNames">List of table names to truncate (without schema prefix)</param>
    /// <returns>True if successful, false if there was an error</returns>
    public async Task<bool> TruncateSpecificTablesAsync(params string[] tableNames)
    {
        try
        {
            _logger.LogInformation("Starting targeted database cleanup for tables: {Tables}", 
                string.Join(", ", tableNames));

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            // Disable constraints temporarily
            var disableConstraints = "EXEC sp_MSforeachtable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL'";
            using (var cmd = new SqlCommand(disableConstraints, connection))
            {
                await cmd.ExecuteNonQueryAsync();
            }

            // Truncate specified tables
            foreach (var tableName in tableNames)
            {
                var truncateQuery = $"TRUNCATE TABLE [dbo].[{tableName}]";
                using var cmd = new SqlCommand(truncateQuery, connection);
                await cmd.ExecuteNonQueryAsync();
                _logger.LogDebug("Truncated table: {TableName}", tableName);
            }

            // Re-enable constraints
            var enableConstraints = "EXEC sp_MSforeachtable 'ALTER TABLE ? CHECK CONSTRAINT ALL'";
            using (var cmd = new SqlCommand(enableConstraints, connection))
            {
                await cmd.ExecuteNonQueryAsync();
            }

            _logger.LogInformation("Targeted database cleanup completed successfully");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred during targeted database cleanup");
            return false;
        }
    }

    /// <summary>
    /// Gets the count of records in all SMS tables
    /// Useful for verifying cleanup operations
    /// </summary>
    /// <returns>Dictionary with table names and their record counts</returns>
    public async Task<Dictionary<string, int>> GetTableRecordCountsAsync()
    {
        var tableCounts = new Dictionary<string, int>();
        
        var smsTableNames = new[]
        {
            "tbld_AirportSharedDataset",
            "tbld_Hazards", 
            "tbld_Interviews",
            "tbld_Investigations",
            "tbld_MitigationAssignments",
            "tbld_Mitigations",
            "tbld_Reports",
            "tbld_ReportValidations", 
            "tbld_RiskAnalysis",
            "tbld_RiskAssessments",
            "tbld_ScoringPanel",
            "tbls_AuditLog"
        };

        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            foreach (var tableName in smsTableNames)
            {
                var countQuery = $"SELECT COUNT(*) FROM [dbo].[{tableName}]";
                using var cmd = new SqlCommand(countQuery, connection);
                var count = (int)await cmd.ExecuteScalarAsync();
                tableCounts[tableName] = count;
            }

            _logger.LogInformation("Retrieved record counts for {TableCount} SMS tables", tableCounts.Count);
            return tableCounts;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while getting table record counts");
            throw;
        }
    }

    /// <summary>
    /// Verifies that all SMS tables are empty
    /// Useful for test setup validation
    /// </summary>
    /// <returns>True if all tables are empty, false otherwise</returns>
    public async Task<bool> VerifyTablesAreEmptyAsync()
    {
        try
        {
            var tableCounts = await GetTableRecordCountsAsync();
            var nonEmptyTables = tableCounts.Where(kvp => kvp.Value > 0).ToList();

            if (nonEmptyTables.Any())
            {
                _logger.LogWarning("Found {Count} non-empty tables: {Tables}",
                    nonEmptyTables.Count,
                    string.Join(", ", nonEmptyTables.Select(t => $"{t.Key}({t.Value})")));
                return false;
            }

            _logger.LogInformation("All SMS tables are empty - database is ready for testing");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while verifying table emptiness");
            return false;
        }
    }
}