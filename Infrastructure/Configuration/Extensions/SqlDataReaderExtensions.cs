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
    public static List<T> LoadCollection<T>(this SqlDataReader reader) where T : new()
    {
        List<T> collection = new List<T>();

        while (reader.Read())
        {
            T entity = reader.CreateEntityFromReader<T>();
            collection.Add(entity);
        }

        return collection;
    }

    private static T CreateEntityFromReader<T>(this SqlDataReader reader) where T : new()
    {
        T entity = new T();
        reader.PopulateEntityFromReader(entity);
        return entity;
    }

    private static void PopulateEntityFromReader<T>(this SqlDataReader reader, T entity) where T : new()
    {
        for (int i = 0; i < reader.FieldCount; i++)
        {
            string columnName = reader.GetName(i);
            object columnValue = reader.GetValue(i);

            PropertyInfo property = typeof(T).GetProperty(columnName);
            if (property != null && columnValue != DBNull.Value)
            {
                property.SetValue(entity, columnValue, null);
            }
        }
    }

    public static T GetValue<T>(this IDataReader reader, string columnName)
    {
        object value = reader[columnName];
        return value == DBNull.Value ? default : (T)value;
    }
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
