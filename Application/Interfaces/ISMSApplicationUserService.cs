//-----------------------------------------------------------------------
// <copyright file="ISMSApplicationUserService.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: SMS User management service handling user lifecycle and authentication operations.
//                  Provides business logic operations and coordinates domain entities
//                  through the CQRS pattern via Mediator services.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;

namespace SMS_Application.Interfaces;

public interface ISMSApplicationUserService
{
    Task<Result<bool>> AuthenticateSMSApplicationUserAsync(string userName, string plainTextPassword, CancellationToken ct = default);
    Task<Result<bool>> ChangePasswordAsync(string userId, string currentPassword, string newPassword, CancellationToken ct = default);
    Task<Result<SMSApplicationUser>> CreateSMSApplicationUserAsync(SMSApplicationUser user, CancellationToken ct = default);
    Task<Result<bool>> DeleteSMSApplicationUserAsync(string userId, CancellationToken ct = default);
    Task<Result<IEnumerable<SMSApplicationUser>>> GetActiveSMSApplicationUsersAsync(CancellationToken ct = default);
    Task<Result<IEnumerable<SMSApplicationUser>>> GetAllSMSApplicationUsersAsync(CancellationToken ct = default);
    Task<Result<SMSApplicationUser>> GetSMSApplicationUserByCodeAsync(string id, CancellationToken ct = default);
    Task<Result<SMSApplicationUser>> GetSMSApplicationUserByUserNameAsync(string userName, CancellationToken ct = default);
    Task<Result<IEnumerable<SMSApplicationUser>>> GetSMSApplicationUsersByRoleAsync(string applicationRole, CancellationToken ct = default);
    Task<Result<UserStatistics>> GetSMSApplicationUserStatisticsAsync(CancellationToken ct = default);
    Task<Result<SMSApplicationUser>> UpdateSMSApplicationUserAsync(SMSApplicationUser user, CancellationToken ct = default);
    Task<Result<bool>> UserNameExistsAsync(string userName, CancellationToken ct = default);
    Task<Result<SMSApplicationUser>> GetByUserNameAsync(string userName, CancellationToken ct = default);
    Task<Result<UserStatistics>> GetUserStatisticsAsync(CancellationToken ct = default);
}
