using System.Reflection;

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
        return Enumerations.TryGetValue(value, out TEnum? enumeration) ? enumeration : default;
    }
    public static TEnum? FromName(string name)
    {
        return Enumerations.Values.SingleOrDefault(key => key.Name == name);
    }
    public bool Equals(BaseEnum<TEnum>? other)
    {
        if (other is null)
        { return false; }
        return GetType() == other.GetType() && Value == other.Value;
    }
    public override bool Equals(object? obj)
    {
        return obj is BaseEnum<TEnum> other && Equals(obj);
    }
    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }
    public override string ToString()
    {
        return Value;
    }


}
