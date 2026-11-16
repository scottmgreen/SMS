//-----------------------------------------------------------------------
// <copyright file="ISMSUserService.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Generic SMS user service contract for shared user lifecycle and authentication operations.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;

namespace SMS_Application.Interfaces;

public interface ISMSUserService<TUser> where TUser : BaseUser
{
    Task<Result<bool>> AuthenticateAsync(string userName, string plainTextPassword, CancellationToken ct = default);
    Task<Result<TUser>> CreateAsync(TUser user, CancellationToken ct = default);
    Task<Result<IEnumerable<TUser>>> GetAllAsync(CancellationToken ct = default);
    Task<Result<TUser>> GetByCodeAsync(string id, CancellationToken ct = default);
    Task<Result<TUser>> UpdateAsync(TUser user, CancellationToken ct = default);
}