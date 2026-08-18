//-----------------------------------------------------------------------
// <copyright file="ISMSJobTitleRepository.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Infrastructure repository contract for SQL-backed SMS job title lookup values.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Enums;
using SMS_Domain.Interfaces;

namespace SMS_Infrastructure.Interfaces;

public interface ISMSJobTitleRepository
{
    Task<Result<IEnumerable<SMSJobTitle>>> GetAllAsync();
    Task<Result<SMSJobTitle>> GetByCodeAsync(string code, CancellationToken ct = default);
    Task<Result<SMSJobTitle>> CreateAsync(SMSJobTitle title, string createdBy, CancellationToken ct = default);
    Task<Result<SMSJobTitle>> UpdateAsync(string code, SMSJobTitle title, string updatedBy, CancellationToken ct = default);
    Task<Result<bool>> DeleteAsync(string code, string deletedBy, CancellationToken ct = default);
}