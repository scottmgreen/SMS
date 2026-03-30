//-----------------------------------------------------------------------
// <copyright file="IInvestigationDataService.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Data service contracts defining business-focused data operations for SMS domain entities.
//                  Infrastructure service contract defining data access operations
//                  and external system integration interfaces.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;

namespace SMS_Infrastructure.Interfaces;

/// <summary>
/// Investigation Data Service Interface
/// </summary>
public interface IInvestigationDataService
{
    // Core CRUD Operations
    Task<Result<Investigation>> CreateInvestigationAsync(Investigation investigation, CancellationToken ct = default);
    Task<Result<Investigation>> GetInvestigationByCodeAsync(string code, CancellationToken ct = default);
    Task<Result<List<Investigation>>> GetAllInvestigationsAsync(CancellationToken ct = default);
    Task<Result<Investigation>> UpdateInvestigationAsync(Investigation investigation, CancellationToken ct = default);
    Task<Result<bool>> DeleteInvestigationAsync(InvestigationID id, CancellationToken ct = default);

    // Query Operations
    Task<Result<IEnumerable<Investigation>>> GetByHazardCodeAsync(string hazardCode, CancellationToken ct = default);
    Task<Result<IEnumerable<Investigation>>> GetByInvestigatorAsync(string investigatorId, CancellationToken ct = default);
    Task<Result<IEnumerable<Investigation>>> GetByStatusAsync(InvestigationStatus status, CancellationToken ct = default);

    // Status Management Operations
    Task<Result<bool>> UpdateStatusAsync(string investigationCode, InvestigationStatus status, CancellationToken ct = default);
    Task<Result<bool>> RecordDecisionAsync(string investigationCode, string decisionType, string rationale,
        string decisionMaker, string? nextSteps = null, string? referralDetails = null, CancellationToken ct = default);
    Task<Result<bool>> CompleteInvestigationAsync(string investigationCode, CancellationToken ct = default);
}
