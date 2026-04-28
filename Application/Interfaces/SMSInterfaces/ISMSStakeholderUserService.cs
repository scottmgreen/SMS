//-----------------------------------------------------------------------
// <copyright file="ISMSStakeholderUserService.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: SMS User management service handling user lifecycle and authentication operations.
//                  Provides business logic operations and coordinates domain entities
//                  through the CQRS pattern via Mediator services.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;

namespace SMS_Application.Interfaces;

public interface ISMSStakeholderUserService
{
    Task<Result<bool>> AuthenticateSMSStakeholderUserAsync(string userName, string plainTextPassword, CancellationToken ct = default);
    Task<Result<SMSStakeholderUser>> CreateSMSStakeholderUserAsync(SMSStakeholderUser user, CancellationToken ct = default);
    
    Task<Result<IEnumerable<SMSStakeholderUser>>> GetAllSMSStakeholderUsersAsync(CancellationToken ct = default);
    Task<Result<SMSStakeholderUser>> GetSMSStakeholderUserByCodeAsync(string id, CancellationToken ct = default);
    Task<Result<IEnumerable<SMSStakeholderUser>>> GetSMSStakeholderUsersByTypeAsync(string stakeholderType, CancellationToken ct = default);
    Task<Result<Dictionary<string, int>>> GetStakeholderTypeStatisticsAsync(CancellationToken ct = default);
    Task<Result<IEnumerable<SMSStakeholderUser>>> GetUsersRequiringAOAAccessAsync(CancellationToken ct = default);
    Task<Result<SMSStakeholderUser>> UpdateSMSStakeholderUserAsync(SMSStakeholderUser user, CancellationToken ct = default);
}
