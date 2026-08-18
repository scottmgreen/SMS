//-----------------------------------------------------------------------
// <copyright file="ISMSOrganizationRepository.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Infrastructure repository contract for SQL-backed SMS organization lookup values.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Enums;
using SMS_Domain.Interfaces;

namespace SMS_Infrastructure.Interfaces;

/// <summary>
/// SMS Organization repository interface.
/// </summary>
public interface ISMSOrganizationRepository
{
    Task<Result<IEnumerable<SMSOrganization>>> GetAllAsync();
    Task<Result<SMSOrganization>> GetByCodeAsync(string code, CancellationToken ct = default);
    Task<Result<SMSOrganization>> CreateAsync(SMSOrganization organization, string createdBy, CancellationToken ct = default);
    Task<Result<SMSOrganization>> UpdateAsync(string code, SMSOrganization organization, string updatedBy, CancellationToken ct = default);
    Task<Result<bool>> DeleteAsync(string code, string deletedBy, CancellationToken ct = default);
}
