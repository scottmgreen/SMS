//-----------------------------------------------------------------------
// <copyright file="SMSJobTitle.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: SQL-driven smart-enum model for SMS job title lookup values.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Domain.Enums;

public sealed class SMSJobTitle : IEquatable<SMSJobTitle>
{
    private static readonly object _syncLock = new();

    private static IReadOnlyDictionary<string, SMSJobTitle> _titlesByCode =
        new Dictionary<string, SMSJobTitle>(StringComparer.OrdinalIgnoreCase);

    private SMSJobTitle(string value, string name)
    {
        Value = value?.Trim() ?? string.Empty;
        Name = name?.Trim() ?? string.Empty;
    }

    public string Value { get; }
    public string Name { get; }
    public string Title => Name;

    public static SMSJobTitle Create(string value, string name)
    {
        return new SMSJobTitle(value, name);
    }

    public static void SetTitles(IEnumerable<SMSJobTitle> titles)
    {
        if (titles is null)
        {
            return;
        }

        var map = titles
            .Where(t => t is not null && !string.IsNullOrWhiteSpace(t.Value))
            .GroupBy(t => t.Value, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First())
            .ToDictionary(t => t.Value, StringComparer.OrdinalIgnoreCase);

        lock (_syncLock)
        {
            _titlesByCode = map;
        }
    }

    public static SMSJobTitle? FromValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return _titlesByCode.TryGetValue(value.Trim(), out var title) ? title : null;
    }

    public static SMSJobTitle? FromName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        return _titlesByCode.Values.FirstOrDefault(t => t.Name.Equals(name.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    public static IEnumerable<SMSJobTitle> GetAllValues() => _titlesByCode.Values;

    public bool Equals(SMSJobTitle? other)
    {
        return other is not null && Value.Equals(other.Value, StringComparison.OrdinalIgnoreCase);
    }

    public override bool Equals(object? obj)
    {
        return obj is SMSJobTitle other && Equals(other);
    }

    public override int GetHashCode()
    {
        return StringComparer.OrdinalIgnoreCase.GetHashCode(Value);
    }

    public override string ToString() => Name;
}