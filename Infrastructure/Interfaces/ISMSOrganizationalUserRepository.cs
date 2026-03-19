//-----------------------------------------------------------------------
// <copyright file="ISMSOrganizationalUserRepository.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Repository implementing data access operations for SMS ismsorganizationaluser entities with CRUD operations and business queries.
//                  Infrastructure service contract defining data access operations
//                  and external system integration interfaces.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Interfaces;

namespace Infrastructure.Interfaces;
public interface ISMSOrganizationalUserRepository
{
    Task<Result<SMSOrganizationalUser>> AddAsync(SMSOrganizationalUser user);
    Task<Result<bool>> DeleteAsync(SMSOrganizationalUserID userId);
    Task<Result<IEnumerable<SMSOrganizationalUser>>> GetActiveUsersAsync();
    Task<Result<IEnumerable<SMSOrganizationalUser>>> GetAllAsync();
    Task<Result<IEnumerable<SMSOrganizationalUser>>> GetByDepartmentAsync(string department);
    Task<Result<SMSOrganizationalUser>> GetByCodeAsync(SMSOrganizationalUserID id);
    Task<Result<IEnumerable<SMSOrganizationalUser>>> GetByPositionAsync(string position);
    Task<Result<IEnumerable<SMSOrganizationalUser>>> GetBySMSOrganizationalUserLevelAsync(string organizationLevel);
    Task<Result<SMSOrganizationalUser>> GetByUserNameAsync(string userName);
    Task<Result<IEnumerable<SMSOrganizationalUser>>> GetUsersAtOrAboveLevelAsync(string minimumLevel);
    Task<Result<UserStatistics>> GetUserStatisticsAsync();
    Task<Result<bool>> RecordLoginAsync(SMSOrganizationalUserID userId, DateTime loginDate);
    Task<Result<bool>> RecordLoginAsync(string userId, DateTime loginDate);
    Task<Result<bool>> UpdateAsync(SMSOrganizationalUser user);
    Task<Result<bool>> UpdatePasswordAsync(SMSOrganizationalUserID userId, string hashedPassword);
    Task<Result<bool>> UpdatePasswordAsync(string userId, string hashedPassword);
    Task<Result<bool>> UserNameExistsAsync(string userName);

    // 🔐 Two-Factor Authentication Methods
    Task<Result<bool>> Setup2FAAsync(string userCode, string secretKey, string? backupCodes = null, string updatedBy = "SYSTEM-2FA");
    Task<Result<bool>> Update2FAFailedAttemptsAsync(string userCode, int failedAttempts, DateTime? lockoutUntil = null, string updatedBy = "SYSTEM-2FA");
    Task<Result<bool>> Reset2FAFailedAttemptsAsync(string userCode, string updatedBy = "SYSTEM-2FA");
    Task<Result<bool>> Disable2FAAsync(string userCode, string updatedBy = "SYSTEM-2FA");
}
