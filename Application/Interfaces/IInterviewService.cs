//-----------------------------------------------------------------------
// <copyright file="IInterviewService.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Application service interface for SMS interview management.
//                  Provides business logic operations for interview entities.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Application.Interfaces;

/// <summary>
/// Application service interface for Interview management and business operations
/// </summary>
public interface IInterviewService
{
    /// <summary>
    /// Creates a new interview
    /// </summary>
    Task<Result<Interview>> CreateInterviewAsync(Interview interview, CancellationToken ct = default);

    /// <summary>
    /// Gets interview by code
    /// </summary>
    Task<Result<Interview>> GetInterviewByCodeAsync(InterviewID code, CancellationToken ct = default);

    /// <summary>
    /// Gets all interviews
    /// </summary>
    Task<Result<List<Interview>>> GetAllInterviewsAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets interviews by investigation code
    /// </summary>
    Task<Result<List<Interview>>> GetInterviewsByInvestigationCodeAsync(InvestigationID investigationCode, CancellationToken ct = default);

    /// <summary>
    /// Updates an existing interview
    /// </summary>
    Task<Result<Interview>> UpdateInterviewAsync(Interview interview, CancellationToken ct = default);

    /// <summary>
    /// Deletes an interview by code
    /// </summary>
    Task<Result<bool>> DeleteInterviewAsync(InterviewID code, CancellationToken ct = default);

    /// <summary>
    /// Schedules an interview for a specific date/time
    /// </summary>
    Task<Result<Interview>> ScheduleInterviewAsync(InterviewID code, DateTime scheduledDateTime, string scheduledBy, CancellationToken ct = default);

    /// <summary>
    /// Completes an interview with notes and outcome
    /// </summary>
    Task<Result<Interview>> CompleteInterviewAsync(InterviewID code, string notes, string completedBy, CancellationToken ct = default);
}