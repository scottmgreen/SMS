//-----------------------------------------------------------------------
// <copyright file="IHazardService.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Application service interface for Hazard management and business operations.
//                  Provides business logic operations for hazard entities.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;
using SMS_Domain.Entities;

namespace SMS_Application.Interfaces;

/// <summary>
/// Application service interface for Hazard management and business operations
/// </summary>
public interface IHazardService
{
    /// <summary>
    /// Creates a new hazard
    /// </summary>
    Task<Result<Hazard>> CreateHazardAsync(Hazard hazard, CancellationToken ct = default);

    /// <summary>
    /// Gets hazard by ID
    /// </summary>
    Task<Result<Hazard>> GetHazardByIdAsync(HazardID id, CancellationToken ct = default);

    /// <summary>
    /// Gets hazard by code
    /// </summary>
    Task<Result<Hazard>> GetHazardByCodeAsync(string code, CancellationToken ct = default);

    /// <summary>
    /// Gets all hazards
    /// </summary>
    Task<Result<List<Hazard>>> GetAllHazardsAsync(CancellationToken ct = default);

    /// <summary>
    /// Updates an existing hazard
    /// </summary>
    Task<Result<Hazard>> UpdateHazardAsync(Hazard hazard, CancellationToken ct = default);

    /// <summary>
    /// Deletes a hazard
    /// </summary>
    Task<Result<bool>> DeleteHazardAsync(HazardID id, CancellationToken ct = default);

    /// <summary>
    /// Gets hazards by report code (string)
    /// </summary>
    Task<Result<List<Hazard>>> GetHazardsByReportCodeAsync(string reportCode, CancellationToken ct = default);

    /// <summary>
    /// Gets hazards by report ID
    /// </summary>
    Task<Result<List<Hazard>>> GetHazardsByReportCodeAsync(ReportID reportId, CancellationToken ct = default);
}