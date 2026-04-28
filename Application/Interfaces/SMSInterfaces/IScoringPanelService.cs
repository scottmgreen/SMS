//-----------------------------------------------------------------------
// <copyright file="IScoringPanelService.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Application service interface for SMS scoring panel management.
//                  Provides business logic operations for scoring panel entities
//                  through clean architecture patterns.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;

namespace SMS_Application.Interfaces;

/// <summary>
/// Application service interface for Scoring Panel management and business operations
/// </summary>
public interface IScoringPanelService
{
    /// <summary>
    /// Creates a new scoring panel for a hazard
    /// </summary>
    Task<Result<ScoringPanel>> CreateScoringPanelAsync(ScoringPanel panel, CancellationToken ct = default);

    /// <summary>
    /// Gets scoring panel by ID
    /// </summary>
    Task<Result<ScoringPanel>> GetScoringPanelByIdAsync(ScoringPanelID id, CancellationToken ct = default);

    /// <summary>
    /// Gets all scoring panels
    /// </summary>
    Task<Result<List<ScoringPanel>>> GetAllScoringPanelsAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets all scoring panels for a specific hazard
    /// </summary>
    Task<Result<List<ScoringPanel>>> GetScoringPanelsByHazardCodeAsync(string hazardCode, CancellationToken ct = default);

    /// <summary>
    /// Updates an existing scoring panel
    /// </summary>
    Task<Result<ScoringPanel>> UpdateScoringPanelAsync(ScoringPanel panel, CancellationToken ct = default);

    /// <summary>
    /// Deletes a scoring panel
    /// </summary>
    Task<Result<bool>> DeleteScoringPanelAsync(ScoringPanelID id, CancellationToken ct = default);
}