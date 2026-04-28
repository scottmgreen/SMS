//-----------------------------------------------------------------------
// <copyright file="ISMSInvestigationWorkflowService.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: SMS Investigation workflow service managing safety investigation processes.
//                  Provides business logic operations and coordinates domain entities
//                  through the CQRS pattern via Mediator services.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;

namespace SMS_Application.Interfaces;

/// <summary>
/// Interface for SMS Investigation Workflow Service
/// Defines the contract for investigation workflow operations
/// </summary>
public interface ISMSInvestigationWorkflowService
{
    /// <summary>
    /// Create a new investigation for a hazard
    /// </summary>
    Task<Result<Investigation>> CreateInvestigationAsync(
        string hazardCode,
        string assignedInvestigatorId,
        string investigationNotes,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get investigation by ID
    /// </summary>
    Task<Result<Investigation>> GetInvestigationAsync(string investigationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get investigation by hazard code
    /// </summary>
    Task<Result<Investigation>> GetInvestigationByHazardCodeAsync(string hazardCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update investigation notes
    /// </summary>
    Task<Result<Investigation>> UpdateInvestigationNotesAsync(
        string investigationId,
        string investigationNotes,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Complete investigation and return decision
    /// </summary>
    Task<Result<Investigation>> CompleteInvestigationAsync(
        string investigationId,
        InvestigationDecisionType decisionType,
        string decisionRationale,
        string decisionMaker,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all investigations
    /// </summary>
    Task<Result<List<Investigation>>> GetAllInvestigationsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete investigation
    /// </summary>
    Task<Result<bool>> DeleteInvestigationAsync(string investigationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get available investigators
    /// </summary>
    //Task<Result<List<SMSApplicationUser>>> GetAvailableInvestigatorsAsync(CancellationToken cancellationToken = default);
}
