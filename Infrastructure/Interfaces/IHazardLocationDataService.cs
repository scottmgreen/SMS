//-----------------------------------------------------------------------
// <copyright file="IHazardLocationDataService.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Data service coordinating ihazardlocation repository operations with safety management workflows.
//                  Infrastructure service contract defining data access operations
//                  and external system integration interfaces.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Infrastructure.Interfaces;

/// <summary>
/// HazardLocation Data Service Interface
/// </summary>
public interface IHazardLocationDataService
{
    Task<Result<HazardLocation>> CreateHazardLocationAsync(HazardLocation hazardLocation, CancellationToken ct = default);
    Task<Result<HazardLocation>> GetHazardLocationByCodeAsync(HazardLocationID code, CancellationToken ct = default);
    Task<Result<List<HazardLocation>>> GetAllHazardLocationsAsync(CancellationToken ct = default);
    Task<Result<List<HazardLocation>>> GetHazardLocationsByHazardCodeAsync(string hazardCode, CancellationToken ct = default);
    Task<Result<HazardLocation>> UpdateHazardLocationAsync(HazardLocation hazardLocation, CancellationToken ct = default);
    Task<Result<bool>> DeleteHazardLocationAsync(HazardLocationID id, CancellationToken ct = default);
}
