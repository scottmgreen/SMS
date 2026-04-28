//-----------------------------------------------------------------------
// <copyright file="IHazardLocationRepository.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Repository implementing data access operations for SMS ihazardlocation entities with safety management integration.
//                  Infrastructure service contract defining data access operations
//                  and external system integration interfaces.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;

namespace SMS_Infrastructure.Interfaces;

/// <summary>
/// Hazard Location Repository Interface
/// </summary>
public interface IHazardLocationRepository
{
    Task<Result<HazardLocation>> GetByCodeAsync(HazardLocationID code);
    Task<Result<HazardLocation>> AddAsync(HazardLocation hazardLocation);
    Task<Result<bool>> UpdateAsync(HazardLocation hazardLocation);
    Task<Result<bool>> DeleteAsync(HazardLocationID code);
    Task<Result<IEnumerable<HazardLocation>>> GetAllAsync();
    Task<Result<IEnumerable<HazardLocation>>> GetByHazardCodeAsync(string hazardCode);
}
