//-----------------------------------------------------------------------
// <copyright file="SMSStakeholderType.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Enumeration defining classification types for SMS smsstakeholder entities.
//                  Domain enumeration defining valid states and classifications
//                  for business entities and processes.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Domain.Enums;

/// <summary>
/// SMS stakeholder user titles for authentication and authorization management
/// </summary>
public sealed class SMSStakeholderUserTitle : IEquatable<SMSStakeholderUserTitle>
{
    private static readonly object _syncLock = new();

    private static IReadOnlyDictionary<string, SMSStakeholderUserTitle> _titlesByCode =
        new Dictionary<string, SMSStakeholderUserTitle>(StringComparer.OrdinalIgnoreCase);

    private SMSStakeholderUserTitle(string code, string name, string description)
    {
        Value = code?.Trim() ?? string.Empty;
        Name = name?.Trim() ?? string.Empty;
        Description = description?.Trim() ?? string.Empty;
    }

    public string Value { get; }
    public string Name { get; }
    public string Description { get; }

    /// <summary>
    /// Factory for SQL-backed stakeholder user titles.
    /// </summary>
    public static SMSStakeholderUserTitle Create(string code, string name, string description)
    {
        return new SMSStakeholderUserTitle(code, name, description);
    }

    /// <summary>
    /// Initializes runtime stakeholder title values from repository data.
    /// </summary>
    public static void SetTitles(IEnumerable<SMSStakeholderUserTitle> stakeholderTitles)
    {
        if (stakeholderTitles is null)
        {
            return;
        }

        var map = stakeholderTitles
            .Where(t => t is not null && !string.IsNullOrWhiteSpace(t.Value))
            .GroupBy(t => t.Value, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First())
            .ToDictionary(t => t.Value, StringComparer.OrdinalIgnoreCase);

        lock (_syncLock)
        {
            _titlesByCode = map;
        }
    }

    public static SMSStakeholderUserTitle? FromValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return _titlesByCode.TryGetValue(value.Trim(), out var title) ? title : null;
    }

    public static SMSStakeholderUserTitle? FromName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        return _titlesByCode.Values.FirstOrDefault(t => t.Name.Equals(name.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    public static IEnumerable<SMSStakeholderUserTitle> GetAllValues() => _titlesByCode.Values;

    public static string[] GetAllValuesAsStringArray() => _titlesByCode.Keys.ToArray();

    public bool Equals(SMSStakeholderUserTitle? other)
    {
        return other is not null && Value.Equals(other.Value, StringComparison.OrdinalIgnoreCase);
    }

    public override bool Equals(object? obj)
    {
        return obj is SMSStakeholderUserTitle other && Equals(other);
    }

    public override int GetHashCode()
    {
        return StringComparer.OrdinalIgnoreCase.GetHashCode(Value);
    }

    public override string ToString() => Name;
}
