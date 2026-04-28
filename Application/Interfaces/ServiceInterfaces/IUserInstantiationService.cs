//-----------------------------------------------------------------------
// <copyright file="IUserInstantiationService.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Service interface for complete user instantiation with roles and permissions
//                  across all authentication methods using CQRS pattern via Mediator.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;

using SMS_Domain.Enums;

namespace SMS_Application.Interfaces;

/// <summary>
/// Service for complete user instantiation with roles and permissions
/// Ensures users are fully populated regardless of authentication method
/// Uses CQRS pattern - NO direct repository access
/// </summary>
public interface IUserInstantiationService
{
    /// <summary>
    /// Get fully instantiated user by code with complete roles and permissions
    /// Uses CQRS queries via Mediator for all user types
    /// </summary>
    Task<Result<(BaseUser User, SMSUserType UserType)>> GetCompleteUserAsync(string userCode, SMSUserType userType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get fully instantiated user by username with complete roles and permissions
    /// Uses CQRS queries via Mediator for all user types
    /// </summary>
    Task<Result<(BaseUser User, SMSUserType UserType)>> GetCompleteUserByUsernameAsync(string username, SMSUserType userType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Serialize complete user data for storage (Session/Circuit/Context)
    /// Preserves all role and permission information
    /// </summary>
    Dictionary<string, string> SerializeCompleteUser(BaseUser user, SMSUserType userType);

    /// <summary>
    /// Deserialize and fully instantiate user from stored data
    /// Will refetch from database via CQRS if stored data is incomplete
    /// </summary>
    Task<Result<(BaseUser User, SMSUserType UserType)>> DeserializeCompleteUserAsync(Dictionary<string, string> userData, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validate user completeness (has all required roles/permissions)
    /// </summary>
    bool IsUserComplete(BaseUser user);

    /// <summary>
    /// Get list of missing components for incomplete user
    /// </summary>
    List<string> GetMissingComponents(BaseUser user);

    /// <summary>
    /// Ensure user has complete role and permission data
    /// Will refetch via CQRS if incomplete
    /// </summary>
    Task<Result<BaseUser>> EnsureUserCompletenessAsync(BaseUser user, SMSUserType userType, CancellationToken cancellationToken = default);
}