namespace CBT3_Infrastructure.Common;
public static class SQLCommandFactory
{
    public static (SqlConnection sql, SqlCommand cmd) CreateStoredProcCommand(string connectionString, string storedProcName)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string must be provided", nameof(connectionString));


        SqlConnection sql = new();
        sql.ConnectionString = connectionString;
        SqlCommand cmd = new(storedProcName, sql)
        {
            CommandType = CommandType.StoredProcedure
        };
        return (sql, cmd);
    }
}
