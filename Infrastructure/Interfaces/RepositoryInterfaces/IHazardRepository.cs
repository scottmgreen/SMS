//-----------------------------------------------------------------------
// <copyright file="IHazardRepository.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Repository implementing data access operations for SMS ihazard entities with safety management integration.
//                  Infrastructure service contract defining data access operations
//                  and external system integration interfaces.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;

namespace SMS_Infrastructure.Interfaces;

/// <summary>
/// Hazard Repository Interface
/// </summary>
public interface IHazardRepository
{
    Task<Result<Hazard>> GetByIdAsync(HazardID id);
    Task<Result<Hazard>> AddAsync(Hazard hazard);
    Task<Result<bool>> UpdateAsync(Hazard hazard);
    Task<Result<bool>> DeleteAsync(HazardID id);
    Task<Result<IEnumerable<Hazard>>> GetAllAsync();
    Task<Result<IEnumerable<Hazard>>> GetByReportCodeAsync(string reportCode);
}

