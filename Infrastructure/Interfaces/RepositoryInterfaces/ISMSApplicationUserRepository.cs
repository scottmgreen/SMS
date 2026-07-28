//-----------------------------------------------------------------------
// <copyright file="ISMSApplicationUserRepository.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Repository implementing data access operations for SMS ismsapplicationuser entities with safety management integration.
//                  Infrastructure service contract defining data access operations
//                  and external system integration interfaces.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;
using SMS_Domain.Interfaces;

namespace SMS_Infrastructure.Interfaces;

/// <summary>
/// SMS Application User Repository Interface
/// Extends the base user repository with SMS-specific operations
/// </summary>
public interface ISMSApplicationUserRepository : IBaseUserRepository<SMSApplicationUser>
{
    // Additional SMS-specific methods not covered by base interface
    Task<Result<IEnumerable<SMSApplicationUser>>> GetByApplicationRoleAsync(string applicationRole);
    Task<Result<IEnumerable<SMSApplicationUser>>> GetBySMSApplicationUserRoleAsync(string applicationRole);

    // 🔐 Two-Factor Authentication Methods
    Task<Result<bool>> Setup2FAAsync(string userCode, string secretKey, string? backupCodes = null, string updatedBy = "");
    Task<Result<bool>> Update2FAFailedAttemptsAsync(string userCode, int failedAttempts, DateTime? lockoutUntil = null, string updatedBy = "");
    Task<Result<bool>> Reset2FAFailedAttemptsAsync(string userCode, string updatedBy = "");
    Task<Result<bool>> Disable2FAAsync(string userCode, string updatedBy = "");
}
