//-----------------------------------------------------------------------
// <copyright file="IMitigationService.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Application service interface for SMS mitigation management.
//                  Provides business logic operations for mitigation entities.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;

namespace SMS_Application.Interfaces;

/// <summary>
/// Application service interface for Mitigation management and business operations
/// </summary>
public interface IMitigationService
{
    /// <summary>
    /// Creates a new mitigation
    /// </summary>
    Task<Result<Mitigation>> CreateMitigationAsync(Mitigation mitigation, CancellationToken ct = default);

    /// <summary>
    /// Gets mitigation by code
    /// </summary>
    Task<Result<Mitigation>> GetMitigationByCodeAsync(MitigationID code, CancellationToken ct = default);

    /// <summary>
    /// Gets all mitigations
    /// </summary>
    Task<Result<List<Mitigation>>> GetAllMitigationsAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets mitigations by hazard code
    /// </summary>
    Task<Result<List<Mitigation>>> GetMitigationsByHazardCodeAsync(string hazardCode, CancellationToken ct = default);

    /// <summary>
    /// Gets mitigations by status
    /// </summary>
    Task<Result<List<Mitigation>>> GetMitigationsByStatusAsync(MitigationStatus status, CancellationToken ct = default);

    /// <summary>
    /// Updates an existing mitigation
    /// </summary>
    Task<Result<Mitigation>> UpdateMitigationAsync(Mitigation mitigation, CancellationToken ct = default);

    /// <summary>
    /// Deletes a mitigation by code
    /// </summary>
    Task<Result<bool>> DeleteMitigationAsync(MitigationID code, CancellationToken ct = default);

    /// <summary>
    /// Approves a mitigation for implementation
    /// </summary>
    Task<Result<Mitigation>> ApproveMitigationAsync(MitigationID code, string approvedBy, string approvalNotes, CancellationToken ct = default);

    /// <summary>
    /// Implements a mitigation (marks as implemented)
    /// </summary>
    Task<Result<Mitigation>> ImplementMitigationAsync(MitigationID code, string implementedBy, DateTime implementationDate, string implementationNotes, CancellationToken ct = default);

    /// <summary>
    /// Closes a mitigation (marks as completed)
    /// </summary>
    Task<Result<Mitigation>> CloseMitigationAsync(MitigationID code, string closedBy, string closureNotes, CancellationToken ct = default);
}