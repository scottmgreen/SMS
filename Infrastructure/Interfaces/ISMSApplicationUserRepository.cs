//-----------------------------------------------------------------------
// <copyright file="ISMSApplicationUserRepository.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Repository implementing data access operations for SMS ismsapplicationuser entities with CRUD operations and business queries.
//                  Infrastructure service contract defining data access operations
//                  and external system integration interfaces.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Interfaces;

namespace Infrastructure.Interfaces;
public interface ISMSApplicationUserRepository
{
    Task<Result<SMSApplicationUser>> AddAsync(SMSApplicationUser user);
    Task<Result<bool>> DeleteAsync(BaseUserID userId);
    Task<Result<IEnumerable<SMSApplicationUser>>> GetActiveUsersAsync();
    Task<Result<IEnumerable<SMSApplicationUser>>> GetAllAsync();
    Task<Result<IEnumerable<SMSApplicationUser>>> GetByApplicationRoleAsync(string applicationRole);
    Task<Result<SMSApplicationUser>> GetByCodeAsync(string code);
    Task<Result<SMSApplicationUser>> GetByIdAsync(SMSApplicationUserID id);
    Task<Result<SMSApplicationUser>> GetByIdAsync(string id);
    Task<Result<IEnumerable<SMSApplicationUser>>> GetBySMSApplicationUserRoleAsync(string applicationRole);
    Task<Result<SMSApplicationUser>> GetByUserNameAsync(string userName);
    Task<Result<UserStatistics>> GetUserStatisticsAsync();
    Task<Result<bool>> RecordLoginAsync(BaseUserID userId, DateTime loginDate);
    Task<Result<bool>> UpdateAsync(SMSApplicationUser user);
    Task<Result<bool>> UpdatePasswordAsync(BaseUserID userId, string hashedPassword);
    Task<Result<bool>> UserNameExistsAsync(string userName);
}
