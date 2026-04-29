//-----------------------------------------------------------------------
// <copyright file="IUserCompletenessValidator.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Service interface for validating user completeness with roles and permissions.
//                  Ensures users have all required data for proper authentication.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;

using SMS_Domain.Enums;

namespace SMS_Application.Interfaces;

/// <summary>
/// Service for validating user completeness with roles and permissions
/// Ensures users have all required data for proper authentication
/// </summary>
public interface IUserCompletenessValidator
{
    /// <summary>
    /// Check if user has all required components for complete authentication
    /// </summary>
    bool IsUserComplete(BaseUser user);

    /// <summary>
    /// Get detailed list of missing components for incomplete user
    /// </summary>
    List<string> GetMissingComponents(BaseUser user);

    /// <summary>
    /// Validate user basic information (Code, Names, Email)
    /// </summary>
    bool HasValidBasicInfo(BaseUser user);

    /// <summary>
    /// Validate user has proper role assignment
    /// </summary>
    bool HasValidRole(BaseUser user);

    /// <summary>
    /// Validate user role has permissions
    /// </summary>
    bool HasValidPermissions(BaseUser user);

    /// <summary>
    /// Validate user type-specific information
    /// </summary>
    bool HasValidTypeSpecificInfo(BaseUser user, SMSUserType userType);

    /// <summary>
    /// Get completeness score (0-100) for user
    /// </summary>
    int GetCompletenessScore(BaseUser user, SMSUserType userType);

    /// <summary>
    /// Generate completeness report for logging/debugging
    /// </summary>
    string GenerateCompletenessReport(BaseUser user, SMSUserType userType);

    /// <summary>
    /// Validate minimum required permissions for user type
    /// </summary>
    bool HasMinimumRequiredPermissions(BaseUser user, SMSUserType userType);
}