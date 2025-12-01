using SMS_Domain.Common;

namespace SMS_Domain.ValueObjects;

/// <summary>
/// Represents a user role assignment with effective dates and context
/// </summary>
//public sealed class UserRoleAssignment : BaseValueObject
//{
//    public string RoleValue { get; }
//    public string RoleName { get; }
//    public string Department { get; }
//    public DateTime EffectiveDate { get; }
//    public DateTime? ExpirationDate { get; }
//    public bool IsActive { get; }
//    public string AssignedBy { get; }
//    public string? Notes { get; }

//    private UserRoleAssignment(
//        string roleValue, 
//        string roleName, 
//        string department, 
//        DateTime effectiveDate, 
//        DateTime? expirationDate, 
//        bool isActive, 
//        string assignedBy, 
//        string? notes)
//    {
//        RoleValue = roleValue;
//        RoleName = roleName;
//        Department = department;
//        EffectiveDate = effectiveDate;
//        ExpirationDate = expirationDate;
//        IsActive = isActive;
//        AssignedBy = assignedBy;
//        Notes = notes;
//    }

//    public static UserRoleAssignment Create(
//        string roleValue,
//        string roleName,
//        string department,
//        DateTime effectiveDate,
//        string assignedBy,
//        DateTime? expirationDate = null,
//        string? notes = null)
//    {
//        return new UserRoleAssignment(
//            roleValue,
//            roleName,
//            department,
//            effectiveDate,
//            expirationDate,
//            true,
//            assignedBy,
//            notes);
//    }

//    /// <summary>
//    /// Checks if the role assignment is currently active
//    /// </summary>
//    public bool IsCurrentlyActive()
//    {
//        var now = DateTime.UtcNow;
//        return IsActive && 
//               EffectiveDate <= now && 
//               (ExpirationDate == null || ExpirationDate > now);
//    }

//    /// <summary>
//    /// Deactivates the role assignment
//    /// </summary>
//    public UserRoleAssignment Deactivate()
//    {
//        return new UserRoleAssignment(
//            RoleValue,
//            RoleName,
//            Department,
//            EffectiveDate,
//            ExpirationDate,
//            false,
//            AssignedBy,
//            Notes);
//    }

//    /// <summary>
//    /// Extends the role assignment with a new expiration date
//    /// </summary>
//    public UserRoleAssignment ExtendAssignment(DateTime newExpirationDate)
//    {
//        return new UserRoleAssignment(
//            RoleValue,
//            RoleName,
//            Department,
//            EffectiveDate,
//            newExpirationDate,
//            IsActive,
//            AssignedBy,
//            Notes);
//    }

//    protected override IEnumerable<object> GetAtomicValues()
//    {
//        yield return RoleValue;
//        yield return Department;
//        yield return EffectiveDate;
//        yield return ExpirationDate ?? DateTime.MaxValue;
//        yield return AssignedBy;
//    }
//}