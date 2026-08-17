//-----------------------------------------------------------------------
// <copyright file="SMSOrganization.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Enumeration defining valid values and classifications for SMS smsdepartment domain concepts.
//                  Domain enumeration defining valid states and classifications
//                  for business entities and processes.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Domain.Enums;

public sealed class SMSOrganization : IEquatable<SMSOrganization>
{
    private static readonly object _syncLock = new();

    private static IReadOnlyDictionary<string, SMSOrganization> _departmentsByCode =
        new Dictionary<string, SMSOrganization>(StringComparer.OrdinalIgnoreCase);

    private SMSOrganization(string value, string name, string description, string[] responsibilities)
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

    public static SMSOrganization Create(string value, string name, string description, string[] responsibilities)
    {
        return new SMSOrganization(value, name, description, responsibilities);
    }

    public static void SetDepartments(IEnumerable<SMSOrganization> departments)
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

    public static SMSOrganization? FromValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return _departmentsByCode.TryGetValue(value.Trim(), out var department) ? department : null;
    }

    public static SMSOrganization? FromName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        return _departmentsByCode.Values.FirstOrDefault(d => d.Name.Equals(name.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    public static IEnumerable<SMSOrganization> GetAllValues() => _departmentsByCode.Values;

    public static List<SMSOrganization> GetAllValuesList() => _departmentsByCode.Values.ToList();

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
    public static IEnumerable<SMSOrganization> GetDepartmentsByResponsibility(string responsibility)
    {
        return GetAllDepartments().Where(dept => dept.HasResponsibility(responsibility));
    }

    /// <summary>
    /// Gets all available departments
    /// </summary>
    public static IEnumerable<SMSOrganization> GetAllDepartments()
    {
        return _departmentsByCode.Values;
    }

    public bool Equals(SMSOrganization? other)
    {
        return other is not null && Value.Equals(other.Value, StringComparison.OrdinalIgnoreCase);
    }

    public override bool Equals(object? obj)
    {
        return obj is SMSOrganization other && Equals(other);
    }

    public override int GetHashCode()
    {
        return StringComparer.OrdinalIgnoreCase.GetHashCode(Value);
    }

    public override string ToString() => Name;
}
