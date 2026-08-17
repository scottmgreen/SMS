//-----------------------------------------------------------------------
// <copyright file="ISMSCompanyRepository.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Infrastructure repository contract for SQL-backed SMS company lookup values.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Enums;
using SMS_Domain.Interfaces;

namespace SMS_Infrastructure.Interfaces;

public interface ISMSCompanyRepository
{
    Task<Result<IEnumerable<SMSCompany>>> GetAllAsync();
    Task<Result<SMSCompany>> GetByCodeAsync(string code, CancellationToken ct = default);
    Task<Result<SMSCompany>> CreateAsync(SMSCompany company, string createdBy, CancellationToken ct = default);
    Task<Result<SMSCompany>> UpdateAsync(SMSCompany company, string updatedBy, CancellationToken ct = default);
    Task<Result<bool>> DeleteAsync(string code, string deletedBy, CancellationToken ct = default);
}
