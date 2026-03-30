//-----------------------------------------------------------------------
// <copyright file="ISMSStakeholderUserRepository.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Repository implementing data access operations for SMS ismsstakeholderuser entities with safety management integration.
//                  Infrastructure service contract defining data access operations
//                  and external system integration interfaces.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;
using SMS_Domain.Interfaces;

namespace SMS_Infrastructure.Interfaces;

/// <summary>
/// SMS Stakeholder User Repository Interface
/// </summary>
public interface ISMSStakeholderUserRepository
{
    Task<Result<SMSStakeholderUser>> AddAsync(SMSStakeholderUser user);
    Task<Result<bool>> DeleteAsync(SMSStakeholderUserID userId);
    Task<Result<bool>> DeleteAsync(string userId);
    Task<Result<IEnumerable<SMSStakeholderUser>>> GetActiveUsersAsync();
    
    Task<Result<IEnumerable<SMSStakeholderUser>>> GetAllAsync();
    //Task<Result<IEnumerable<SMSStakeholderUser>>> GetByAccessLevelAsync(string accessLevel);
    Task<Result<SMSStakeholderUser>> GetByCodeAsync(BaseUserID id);
    Task<Result<IEnumerable<SMSStakeholderUser>>> GetByOrganizationAsync(string organization);
    Task<Result<IEnumerable<SMSStakeholderUser>>> GetByStakeholderTypeAsync(string stakeholderType);
    Task<Result<SMSStakeholderUser>> GetByUserNameAsync(string userName);
   
    Task<Result<Dictionary<string, int>>> GetOrganizationStatisticsAsync();
    Task<Result<Dictionary<string, int>>> GetStakeholderTypeStatisticsAsync();
    Task<Result<IEnumerable<SMSStakeholderUser>>> GetUsersByGroupCodeAsync(string groupCode);
    Task<Result<UserStatistics>> GetUserStatisticsAsync();
    Task<Result<IEnumerable<SMSStakeholderUser>>> GetUsersWithMinimumAccessAsync(string minimumAccessLevel);
    Task<Result<bool>> RecordLoginAsync(SMSStakeholderUserID userId, DateTime loginDate);
    Task<Result<bool>> UpdateAsync(SMSStakeholderUser user);
    //Task<Result<bool>> UpdatePasswordAsync(SMSStakeholderUserID userId, string hashedPassword);*/
    Task<Result<bool>> UpdatePasswordAsync(SMSStakeholderUserID userId, string hashedPassword);
    Task<Result<bool>> UserNameExistsAsync(string userName);

    // 🔐 Two-Factor Authentication Methods
    Task<Result<bool>> Setup2FAAsync(string userCode, string secretKey, string? backupCodes = null, string updatedBy = "SYSTEM-2FA");
    Task<Result<bool>> Update2FAFailedAttemptsAsync(string userCode, int failedAttempts, DateTime? lockoutUntil = null, string updatedBy = "SYSTEM-2FA");
    Task<Result<bool>> Reset2FAFailedAttemptsAsync(string userCode, string updatedBy = "SYSTEM-2FA");
    Task<Result<bool>> Disable2FAAsync(string userCode, string updatedBy = "SYSTEM-2FA");
}
