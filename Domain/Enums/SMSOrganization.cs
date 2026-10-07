//-----------------------------------------------------------------------
// <copyright file="SMSOrganization.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Enumeration defining valid values and classifications for SMS organization domain concepts.
//                  Domain enumeration defining valid states and classifications
//                  for business entities and processes.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Domain.Enums;

public sealed class SMSOrganization : IEquatable<SMSOrganization>
{
    private static readonly object _syncLock = new();

    private static IReadOnlyDictionary<string, SMSOrganization> _organizationsByCode =
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

    public static void SetOrganizations(IEnumerable<SMSOrganization> organizations)
    {
        if (organizations is null)
        {
            return;
        }

        var map = organizations
            .Where(o => o is not null && !string.IsNullOrWhiteSpace(o.Value))
            .GroupBy(o => o.Value, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First())
            .ToDictionary(d => d.Value, StringComparer.OrdinalIgnoreCase);

        lock (_syncLock)
        {
            _organizationsByCode = map;
        }
    }

    public static void SetDepartments(IEnumerable<SMSOrganization> departments)
    {
        SetOrganizations(departments);
    }

    public static SMSOrganization? FromValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return _organizationsByCode.TryGetValue(value.Trim(), out var organization) ? organization : null;
    }

    public static SMSOrganization? FromName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        return _organizationsByCode.Values.FirstOrDefault(o => o.Name.Equals(name.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    public static IEnumerable<SMSOrganization> GetAllValues() => _organizationsByCode.Values;

    public static List<SMSOrganization> GetAllValuesList() => _organizationsByCode.Values.ToList();

    /// <summary>
    /// Checks if this organization has responsibility for a specific area
    /// </summary>
    public bool HasResponsibility(string responsibility)
    {
        return Responsibilities.Contains(responsibility, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gets organizations by responsibility area
    /// </summary>
    public static IEnumerable<SMSOrganization> GetOrganizationsByResponsibility(string responsibility)
    {
        return GetAllOrganizations().Where(org => org.HasResponsibility(responsibility));
    }

    /// <summary>
    /// Gets all available organizations
    /// </summary>
    public static IEnumerable<SMSOrganization> GetAllOrganizations()
    {
        return _organizationsByCode.Values;
    }

    public static IEnumerable<SMSOrganization> GetDepartmentsByResponsibility(string responsibility)
    {
        return GetOrganizationsByResponsibility(responsibility);
    }

    public static IEnumerable<SMSOrganization> GetAllDepartments()
    {
        return GetAllOrganizations();
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
