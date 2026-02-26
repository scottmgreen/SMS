//-----------------------------------------------------------------------
// <copyright file="BaseEnum.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Enumeration defining valid values and classifications for SMS baseenum domain concepts.
//                  Shared domain infrastructure providing base classes
//                  and common functionality for Domain-Driven Design.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Domain.Common;

public abstract class BaseEnum<TEnum> : IEquatable<BaseEnum<TEnum>> where TEnum : BaseEnum<TEnum>
{
    private static readonly Dictionary<string, TEnum> Enumerations = CreateEnumerations();

    private static Dictionary<string, TEnum> CreateEnumerations()
    {
        var enumerationType = typeof(TEnum);
        var fieldsForType = enumerationType
            .GetFields(
                BindingFlags.Public |
                BindingFlags.Static |
                BindingFlags.FlattenHierarchy)
            .Where(fieldsInfo => enumerationType.IsAssignableFrom(fieldsInfo.FieldType))
            .Select(FieldInfo => (TEnum)FieldInfo.GetValue(default)!);

        return fieldsForType.ToDictionary(key => key.Value);
    }

    protected BaseEnum(string value, string name)
    {
        Value = value;
        Name = name;
    }

    public string Value { get; protected init; } = string.Empty;
    public string Name { get; protected init; } = string.Empty;

    public static TEnum? FromValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return default;
        return Enumerations.TryGetValue(value, out TEnum? enumeration) ? enumeration : default;
    }

    public static TEnum? FromName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return default;
        return Enumerations.Values.SingleOrDefault(key => key.Name == name);
    }

    /// <summary>
    /// Get all enumeration values
    /// </summary>
    public static IEnumerable<TEnum> GetAllValues()
    {
        return Enumerations.Values;
    }

    /// <summary>
    /// Get all enumeration values as a list
    /// </summary>
    public static List<TEnum> GetAllValuesList()
    {
        return Enumerations.Values.ToList();
    }

    public bool Equals(BaseEnum<TEnum>? other)
    {
        if (other is null)
            return false;
        return GetType() == other.GetType() && Value == other.Value;
    }

    public override bool Equals(object? obj)
    {
        return obj is BaseEnum<TEnum> other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public override string ToString()
    {
        return Name; // Return Name instead of Value for better display
    }

    /// <summary>
    /// Implicit conversion to string (returns Value)
    /// </summary>
    public static implicit operator string(BaseEnum<TEnum> enumeration)
    {
        return enumeration?.Value ?? string.Empty;
    }
}

