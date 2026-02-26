//-----------------------------------------------------------------------
// <copyright file="BaseEntity.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Base entity class providing common functionality for SMS domain entities with identity and lifecycle management.
//                  Shared domain infrastructure providing base classes
//                  and common functionality for Domain-Driven Design.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Domain.Common;

//public class EntityId<T> : BaseID<T>
//{
//    public EntityId(T value) : base(value) { }
//}

public abstract class BaseEntity : IBaseEntity, IEquatable<BaseEntity>
{
    public BaseEntity(BaseID<string> id)
    {
        Id = id;
    }
    //private readonly List<BaseDomainEvent> _domainEvents = new();

    public BaseID<string> Id { get; private set; }

    public static bool operator ==(BaseEntity a, BaseEntity b)
    {
        if (a is null && b is null)
        {
            return true;
        }

        if (a is null || b is null)
        {
            return false;
        }

        return a.Equals(b);
    }

    public static bool operator !=(BaseEntity a, BaseEntity b) => !(a == b);

    public bool Equals(BaseEntity other)
    {
        if (other is null)
        {
            return false;
        }

        if (other.GetType() != GetType())
        {
            return false;
        }

        return ReferenceEquals(this, other) || Id == other.Id;
    }

    public override bool Equals(object obj)
    {
        if (obj is null)
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj is not BaseEntity other)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(Id.Value) || string.IsNullOrWhiteSpace(other.Id.Value))
        {
            return false;
        }

        return Id == other.Id;
    }

    public override int GetHashCode() => Id.Value.GetHashCode(StringComparison.InvariantCulture) * 41;

    //[NotMapped]
    //public IReadOnlyCollection<BaseDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    //public void AddDomainEvent(BaseDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
    //public void RemoveDomainEvent(BaseDomainEvent domainEvent) => _domainEvents.Remove(domainEvent);
    //public void ClearDomainEvents() => _domainEvents.Clear();


}




/// <summary>
/// Represents the base class that all entities derive from.
/// </summary>
public abstract class Entity : IEquatable<Entity>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Entity"/> class.
    /// </summary>
    /// <param name="id">The entity identifier.</param>
    protected Entity(string id)
        : this()
    {
        if (id == string.Empty)
        {
            throw new ArgumentException("The entity identifier is required.", nameof(id));
        }

        Id = id.ToString();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Entity"/> class.
    /// </summary>
    /// <remarks>
    /// Required for deserialization.
    /// </remarks>
    protected Entity()
    {
    }

    /// <summary>
    /// Gets the entity identifier.
    /// </summary>
    public string Id { get; private set; }

    public static bool operator ==(Entity a, Entity b)
    {
        if (a is null && b is null)
        {
            return true;
        }

        if (a is null || b is null)
        {
            return false;
        }

        return a.Equals(b);
    }

    public static bool operator !=(Entity a, Entity b) => !(a == b);

    /// <inheritdoc />
    public bool Equals(Entity other)
    {
        if (other is null)
        {
            return false;
        }

        if (other.GetType() != GetType())
        {
            return false;
        }

        return ReferenceEquals(this, other) || Id == other.Id;
    }

    /// <inheritdoc />
    public override bool Equals(object obj)
    {
        if (obj is null)
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj is not Entity other)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(Id) || string.IsNullOrWhiteSpace(other.Id))
        {
            return false;
        }

        return Id == other.Id;
    }

    /// <inheritdoc />
    public override int GetHashCode() => Id.GetHashCode(StringComparison.InvariantCulture) * 41;
}



