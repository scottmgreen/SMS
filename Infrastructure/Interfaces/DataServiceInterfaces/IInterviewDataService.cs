//-----------------------------------------------------------------------
// <copyright file="IInterviewDataService.cs" company="SMS Safety Management System">
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
/// Interview Data Service Interface
/// </summary>
public interface IInterviewDataService
{
    // Core CRUD Operations
    Task<Result<Interview>> CreateInterviewAsync(Interview interview, CancellationToken ct = default);
    Task<Result<Interview>> GetInterviewByCodeAsync(InterviewID code, CancellationToken ct = default);
    Task<Result<List<Interview>>> GetAllInterviewsAsync(CancellationToken ct = default);
    Task<Result<Interview>> UpdateInterviewAsync(Interview interview, CancellationToken ct = default);
    Task<Result<bool>> DeleteInterviewAsync(InterviewID id, CancellationToken ct = default);

    // Query Operations
    Task<Result<IEnumerable<Interview>>> GetByInvestigationAsync(string investigationCode, CancellationToken ct = default);
    Task<Result<IEnumerable<Interview>>> GetByInvestigatorAsync(string investigatorCode, CancellationToken ct = default);
    Task<Result<IEnumerable<Interview>>> GetByStatusAsync(InterviewStatus status, CancellationToken ct = default);

    // Interview Workflow Operations
    Task<Result<bool>> UpdateStatusAsync(string interviewCode, InterviewStatus status, CancellationToken ct = default);
    Task<Result<bool>> ScheduleInterviewAsync(string interviewCode, DateTime interviewDate, string location,
        int? durationMinutes = null, CancellationToken ct = default);
    Task<Result<bool>> CompleteInterviewAsync(string interviewCode, string? personNotes, string? investigatorNotes,
        string? keyFindings = null, CancellationToken ct = default);
}
