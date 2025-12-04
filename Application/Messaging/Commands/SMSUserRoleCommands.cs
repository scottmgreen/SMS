using SMS_Application.Common;
using SMS_Application.Interfaces;
using SMS_Domain.Entities;
using SMS_Domain.ValueObjects;
using SMS_Shared.Common;

namespace SMS_Application.Messaging.Commands;

/// <summary>
/// Commands for SMS User Role management operations
/// </summary>

#region Create Commands

/// <summary>
/// Command to create a new SMS User Role assignment
/// </summary>
public class CreateSMSUserRoleCommand : BaseCommandBundle, IRequest<Result<SMSUserRole>>, ICreateCommand
{
    /// <summary>
    /// The SMS User Role entity to create
    /// </summary>
    public SMSUserRole SMSUserRole { get; set; }

    /// <summary>
    /// Initializes a new instance of the CreateSMSUserRoleCommand class.
    /// </summary>
    /// <param name="smsUserRole">The SMS user role to create</param>
    /// <exception cref="ArgumentNullException">Thrown when smsUserRole is null</exception>
    public CreateSMSUserRoleCommand(SMSUserRole smsUserRole)
    {
        SMSUserRole = smsUserRole ?? throw new ArgumentNullException(nameof(smsUserRole));
    }

    public void SetCreatedBy(string userId, DateTime timestamp)
    {
        // SMSUserRole doesn't have standard CreatedBy/CreatedDate fields
        // The audit trail is maintained through other properties
    }

    public void SetUpdatedBy(string userId, DateTime timestamp)
    {
        // For create commands, we typically don't set UpdatedBy
    }
}

#endregion

#region Update Commands

/// <summary>
/// Command to update an existing SMS User Role assignment
/// </summary>
public class UpdateSMSUserRoleCommand : BaseCommandBundle, IRequest<Result<SMSUserRole>>, IUpdateCommand
{
    /// <summary>
    /// The SMS User Role entity to update
    /// </summary>
    public SMSUserRole SMSUserRole { get; set; }

    /// <summary>
    /// Initializes a new instance of the UpdateSMSUserRoleCommand class.
    /// </summary>
    /// <param name="smsUserRole">The SMS user role to update</param>
    /// <exception cref="ArgumentNullException">Thrown when smsUserRole is null</exception>
    public UpdateSMSUserRoleCommand(SMSUserRole smsUserRole)
    {
        SMSUserRole = smsUserRole ?? throw new ArgumentNullException(nameof(smsUserRole));
    }

    public void SetCreatedBy(string userId, DateTime timestamp)
    {
        // For update commands, we typically don't modify CreatedBy
    }

    public void SetUpdatedBy(string userId, DateTime timestamp)
    {
        // SMSUserRole doesn't have standard UpdatedBy/UpdatedDate fields
        // The audit trail is maintained through other properties
    }
}

/// <summary>
/// Command to activate SMS User Role assignment
/// </summary>
public class ActivateSMSUserRoleCommand : BaseCommandBundle, IRequest<Result<bool>>, IHasAuditFields
{
    /// <summary>
    /// The ID of the SMS User Role to activate
    /// </summary>
    public string UserRoleId { get; set; }

    /// <summary>
    /// User who is activating the role
    /// </summary>
    public string ActivatedBy { get; set; }

    /// <summary>
    /// Initializes a new instance of the ActivateSMSUserRoleCommand class.
    /// </summary>
    /// <param name="userRoleId">The user role ID</param>
    /// <exception cref="ArgumentException">Thrown when userRoleId is null or empty</exception>
    public ActivateSMSUserRoleCommand(string userRoleId)
    {
        if (string.IsNullOrWhiteSpace(userRoleId)) throw new ArgumentException("User role ID cannot be null or empty", nameof(userRoleId));

        UserRoleId = userRoleId;
    }

    public void SetCreatedBy(string userId, DateTime timestamp)
    {
        // This is not a create operation
    }

    public void SetUpdatedBy(string userId, DateTime timestamp)
    {
        ActivatedBy = userId;
    }
}

/// <summary>
/// Command to deactivate SMS User Role assignment
/// </summary>
public class DeactivateSMSUserRoleCommand : BaseCommandBundle, IRequest<Result<bool>>, IHasAuditFields
{
    /// <summary>
    /// The ID of the SMS User Role to deactivate
    /// </summary>
    public string UserRoleId { get; set; }

    /// <summary>
    /// User who is deactivating the role
    /// </summary>
    public string DeactivatedBy { get; set; }

    /// <summary>
    /// Reason for deactivation
    /// </summary>
    public string DeactivationReason { get; set; }

    /// <summary>
    /// Initializes a new instance of the DeactivateSMSUserRoleCommand class.
    /// </summary>
    /// <param name="userRoleId">The user role ID</param>
    /// <param name="deactivationReason">Reason for deactivation</param>
    /// <exception cref="ArgumentException">Thrown when userRoleId is null or empty</exception>
    public DeactivateSMSUserRoleCommand(string userRoleId, string deactivationReason = null)
    {
        if (string.IsNullOrWhiteSpace(userRoleId)) throw new ArgumentException("User role ID cannot be null or empty", nameof(userRoleId));

        UserRoleId = userRoleId;
        DeactivationReason = deactivationReason ?? string.Empty;
    }

    public void SetCreatedBy(string userId, DateTime timestamp)
    {
        // This is not a create operation
    }

    public void SetUpdatedBy(string userId, DateTime timestamp)
    {
        DeactivatedBy = userId;
    }
}

/// <summary>
/// Command to extend SMS User Role assignment expiration
/// </summary>
public class ExtendSMSUserRoleExpirationCommand : BaseCommandBundle, IRequest<Result<bool>>, IHasAuditFields
{
    /// <summary>
    /// The ID of the SMS User Role to extend
    /// </summary>
    public string UserRoleId { get; set; }

    /// <summary>
    /// New expiration date
    /// </summary>
    public DateTime NewExpirationDate { get; set; }

    /// <summary>
    /// User who is extending the role
    /// </summary>
    public string ExtendedBy { get; set; }

    /// <summary>
    /// Initializes a new instance of the ExtendSMSUserRoleExpirationCommand class.
    /// </summary>
    /// <param name="userRoleId">The user role ID</param>
    /// <param name="newExpirationDate">New expiration date</param>
    /// <exception cref="ArgumentException">Thrown when userRoleId is null or empty</exception>
    public ExtendSMSUserRoleExpirationCommand(string userRoleId, DateTime newExpirationDate)
    {
        if (string.IsNullOrWhiteSpace(userRoleId)) throw new ArgumentException("User role ID cannot be null or empty", nameof(userRoleId));

        UserRoleId = userRoleId;
        NewExpirationDate = newExpirationDate;
    }

    public void SetCreatedBy(string userId, DateTime timestamp)
    {
        // This is not a create operation
    }

    public void SetUpdatedBy(string userId, DateTime timestamp)
    {
        ExtendedBy = userId;
    }
}

#endregion

#region Delete Commands

/// <summary>
/// Command to delete an SMS User Role assignment by ID
/// </summary>
public class DeleteSMSUserRoleCommand : BaseCommandBundle, IRequest<Result<bool>>
{
    /// <summary>
    /// The ID of the SMS User Role to delete
    /// </summary>
    public SMSUserRoleID SMSUserRoleId { get; set; }

    /// <summary>
    /// Initializes a new instance of the DeleteSMSUserRoleCommand class.
    /// </summary>
    /// <param name="smsUserRoleId">The ID of the SMS user role to delete</param>
    /// <exception cref="ArgumentNullException">Thrown when smsUserRoleId is null</exception>
    public DeleteSMSUserRoleCommand(SMSUserRoleID smsUserRoleId)
    {
        SMSUserRoleId = smsUserRoleId ?? throw new ArgumentNullException(nameof(smsUserRoleId));
    }
}

#endregion

#region Assignment Commands

/// <summary>
/// Command to assign a role to a user
/// </summary>
public class AssignRoleToUserCommand : BaseCommandBundle, IRequest<Result<SMSUserRole>>, IHasAuditFields
{
    /// <summary>
    /// User ID to assign role to
    /// </summary>
    public string UserId { get; set; }

    /// <summary>
    /// Role code to assign
    /// </summary>
    public string RoleCode { get; set; }

    /// <summary>
    /// Department for the assignment
    /// </summary>
    public string Department { get; set; }

    /// <summary>
    /// User type
    /// </summary>
    public string UserType { get; set; }

    /// <summary>
    /// Effective date for the assignment
    /// </summary>
    public DateTime EffectiveDate { get; set; }

    /// <summary>
    /// Optional expiration date
    /// </summary>
    public DateTime? ExpirationDate { get; set; }

    /// <summary>
    /// User who assigned the role
    /// </summary>
    public string AssignedBy { get; set; }

    /// <summary>
    /// Initializes a new instance of the AssignRoleToUserCommand class.
    /// </summary>
    public AssignRoleToUserCommand(
        string userId, 
        string roleCode, 
        string department, 
        string userType, 
        DateTime effectiveDate,
        DateTime? expirationDate = null)
    {
        if (string.IsNullOrWhiteSpace(userId)) throw new ArgumentException("User ID cannot be null or empty", nameof(userId));
        if (string.IsNullOrWhiteSpace(roleCode)) throw new ArgumentException("Role code cannot be null or empty", nameof(roleCode));
        if (string.IsNullOrWhiteSpace(department)) throw new ArgumentException("Department cannot be null or empty", nameof(department));
        if (string.IsNullOrWhiteSpace(userType)) throw new ArgumentException("User type cannot be null or empty", nameof(userType));

        UserId = userId;
        RoleCode = roleCode;
        Department = department;
        UserType = userType;
        EffectiveDate = effectiveDate;
        ExpirationDate = expirationDate;
    }

    public void SetCreatedBy(string userId, DateTime timestamp)
    {
        // This is not a create operation in the traditional sense
    }

    public void SetUpdatedBy(string userId, DateTime timestamp)
    {
        AssignedBy = userId;
    }
}

/// <summary>
/// Command to remove a role from a user
/// </summary>
public class RemoveRoleFromUserCommand : BaseCommandBundle, IRequest<Result<bool>>, IHasAuditFields
{
    /// <summary>
    /// User ID to remove role from
    /// </summary>
    public string UserId { get; set; }

    /// <summary>
    /// Role code to remove
    /// </summary>
    public string RoleCode { get; set; }

    /// <summary>
    /// User who removed the role
    /// </summary>
    public string RemovedBy { get; set; }

    /// <summary>
    /// Initializes a new instance of the RemoveRoleFromUserCommand class.
    /// </summary>
    public RemoveRoleFromUserCommand(string userId, string roleCode)
    {
        if (string.IsNullOrWhiteSpace(userId)) throw new ArgumentException("User ID cannot be null or empty", nameof(userId));
        if (string.IsNullOrWhiteSpace(roleCode)) throw new ArgumentException("Role code cannot be null or empty", nameof(roleCode));

        UserId = userId;
        RoleCode = roleCode;
    }

    public void SetCreatedBy(string userId, DateTime timestamp)
    {
        // This is not a create operation
    }

    public void SetUpdatedBy(string userId, DateTime timestamp)
    {
        RemovedBy = userId;
    }
}

#endregion