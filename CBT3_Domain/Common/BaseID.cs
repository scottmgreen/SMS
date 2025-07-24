
namespace CBT3_Domain.Common;
public abstract class BaseID<T> : IEquatable<BaseID<T>>
{
    public T Value { get; }

    protected BaseID(T value)
    {
        if (value is null)
        {
            throw new ArgumentNullException(nameof(value), "ID value cannot be null.");
        }
        if (value.Equals(string.Empty))
        {
            throw new ArgumentException("ID value cannot be empty.", nameof(value));
        }
        Value = value;
    }

    public bool Equals(BaseID<T>? other)
    {
        if (other is null)
        {
            return false;
        }

        return Equals(Value, other.Value);
    }

    public override bool Equals(object obj)
    {
        if (obj is null || !(obj is BaseID<T> other))
        {
            return false;
        }

        return Equals(other);
    }

    public override int GetHashCode() => Value.GetHashCode();

    public override string ToString() => Value.ToString();

    
    public static bool operator ==(BaseID<T> left, BaseID<T> right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(BaseID<T> left, BaseID<T> right)
    {
        return !Equals(left, right);
    }

    public int CompareTo(BaseID<T> other)
    {
        if (Value is IComparable<T> comparable)
        {
            return comparable.CompareTo(other.Value);
        }
        throw new InvalidOperationException("BaseId type does not support comparison.");
    }




}

