//-----------------------------------------------------------------------
// <copyright file="SqlDataReaderExtensions.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Extension methods providing enhanced functionality for sqldatareader operations and data manipulation.
//                  Infrastructure utility providing shared functionality
//                  for data access and external system integration.
// </copyright>
//-----------------------------------------------------------------------

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
