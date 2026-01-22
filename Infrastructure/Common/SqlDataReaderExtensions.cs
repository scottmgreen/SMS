namespace SMS_Infrastructure.Common;

/// <summary>
/// Extension methods for SqlDataReader to enhance functionality
/// </summary>
public static class SqlDataReaderExtensions
{
    /// <summary>
    /// Checks if a column exists in the SqlDataReader
    /// </summary>
    /// <param name="reader">The SqlDataReader instance</param>
    /// <param name="columnName">The name of the column to check</param>
    /// <returns>True if the column exists, false otherwise</returns>
    public static bool HasColumn(this SqlDataReader reader, string columnName)
    {
        try
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetName(i).Equals(columnName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }
        catch
        {
            return false;
        }
    }
}