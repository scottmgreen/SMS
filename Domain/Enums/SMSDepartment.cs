//-----------------------------------------------------------------------
// <copyright file="SMSDepartment.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Enumeration defining valid values and classifications for SMS smsdepartment domain concepts.
//                  Domain enumeration defining valid states and classifications
//                  for business entities and processes.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Domain.Enums;

public sealed class SMSDepartment : IEquatable<SMSDepartment>
{
    private static readonly object _syncLock = new();

    private static IReadOnlyDictionary<string, SMSDepartment> _departmentsByCode =
        new Dictionary<string, SMSDepartment>(StringComparer.OrdinalIgnoreCase);

    private SMSDepartment(string value, string name, string description, string[] responsibilities)
    {
        Value = value?.Trim() ?? string.Empty;
        Name = name?.Trim() ?? string.Empty;
        Description = description;
        Responsibilities = responsibilities ?? Array.Empty<string>();
    }

    public string Value { get; }
    public string Name { get; }
    public string Description { get; }
    public string[] Responsibilities { get; }

    public static SMSDepartment Create(string value, string name, string description, string[] responsibilities)
    {
        return new SMSDepartment(value, name, description, responsibilities);
    }

    public static void SetDepartments(IEnumerable<SMSDepartment> departments)
    {
        if (departments is null)
        {
            return;
        }

        var map = departments
            .Where(d => d is not null && !string.IsNullOrWhiteSpace(d.Value))
            .GroupBy(d => d.Value, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First())
            .ToDictionary(d => d.Value, StringComparer.OrdinalIgnoreCase);

        lock (_syncLock)
        {
            _departmentsByCode = map;
        }
    }

    public static SMSDepartment? FromValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return _departmentsByCode.TryGetValue(value.Trim(), out var department) ? department : null;
    }

    public static SMSDepartment? FromName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        return _departmentsByCode.Values.FirstOrDefault(d => d.Name.Equals(name.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    public static IEnumerable<SMSDepartment> GetAllValues() => _departmentsByCode.Values;

    public static List<SMSDepartment> GetAllValuesList() => _departmentsByCode.Values.ToList();

    /// <summary>
    /// Checks if this department has responsibility for a specific area
    /// </summary>
    public bool HasResponsibility(string responsibility)
    {
        return Responsibilities.Contains(responsibility, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gets departments by responsibility area
    /// </summary>
    public static IEnumerable<SMSDepartment> GetDepartmentsByResponsibility(string responsibility)
    {
        return GetAllDepartments().Where(dept => dept.HasResponsibility(responsibility));
    }

    /// <summary>
    /// Gets all available departments
    /// </summary>
    public static IEnumerable<SMSDepartment> GetAllDepartments()
    {
        return _departmentsByCode.Values;
    }

    public bool Equals(SMSDepartment? other)
    {
        return other is not null && Value.Equals(other.Value, StringComparison.OrdinalIgnoreCase);
    }

    public override bool Equals(object? obj)
    {
        return obj is SMSDepartment other && Equals(other);
    }

    public override int GetHashCode()
    {
        return StringComparer.OrdinalIgnoreCase.GetHashCode(Value);
    }

    public override string ToString() => Name;
}
