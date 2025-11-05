using SMS_Domain.Common;

namespace SMS_Domain.Entities;

/// <summary>
/// Represents a unique identifier for BaseUser entities
/// </summary>
public sealed class BaseUserID : BaseID<string>
{
    public BaseUserID(string value) : base(value) { }
    
    public static implicit operator string(BaseUserID id) => id.Value;
    public static implicit operator BaseUserID(string value) => new(value);
}