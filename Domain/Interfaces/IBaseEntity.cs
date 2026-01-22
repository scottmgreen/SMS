namespace SMS_Domain.Interfaces;

public interface IBaseEntity
{
    // Common property for an entity identifier
    BaseID<string> Id { get; }
}
