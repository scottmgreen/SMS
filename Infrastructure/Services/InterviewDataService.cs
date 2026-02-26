//-----------------------------------------------------------------------
// <copyright file="InterviewDataService.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Data service coordinating interview repository operations with transaction management and business validation.
//                  Infrastructure service providing external system integration
//                  and technical functionality support.
// </copyright>
//-----------------------------------------------------------------------

// =============================================
// SMS DATA SERVICES - COMPLETE SET
// All 10 SMS Data Services for easy copy/paste
// =============================================

// 1. HazardDataService.cs
namespace SMS_Infrastructure.Services;

using SMS_Infrastructure.Interfaces;
using SMS_Infrastructure.Persistence;

/// <summary>
/// Interview Data Service - follows the exact same pattern as HazardDataService
/// Provides business logic layer between CQRS handlers and repository
/// </summary>
public class InterviewDataService : BaseDataService<InterviewDataService>, IInterviewDataService
{
    private readonly ILogger<InterviewDataService> _logger;
    private readonly string _logheader;
    private readonly InterviewRepository _repo;

    public InterviewDataService(ILogger<InterviewDataService> logger, IServiceScopeFactory serviceScopeFactory, IConfiguration configuration, InterviewRepository repo)
        : base(logger, serviceScopeFactory, configuration)
    {
        _logger = base.Logger;
        _logheader = base.LogHeader;
        _repo = repo;

        _logger.LogInfrastructureInformation(InfrastructureEventIds.InfrastructureEvent, $"{_logheader} {repo.GetType().Name}");
    }

    // Core CRUD Operations
    public Task<Result<Interview>> CreateInterviewAsync(Interview interview, CancellationToken ct = default)
    {
        return _repo.CreateInterviewAsync(interview, ct);
    }

    public Task<Result<Interview>> GetInterviewByCodeAsync(InterviewID code, CancellationToken ct = default)
    {
        return _repo.GetInterviewByCodeAsync(code, ct);
    }



    public Task<Result<List<Interview>>> GetAllInterviewsAsync(CancellationToken ct = default)
    {
        return _repo.GetAllInterviewsAsync(ct);
    }

    public Task<Result<Interview>> UpdateInterviewAsync(Interview interview, CancellationToken ct = default)
    {
        return _repo.UpdateInterviewAsync(interview, ct);
    }

    public Task<Result<bool>> DeleteInterviewAsync(InterviewID id, CancellationToken ct = default)
    {
        return _repo.DeleteInterviewAsync(id, ct);
    }

    // Query Operations
    public Task<Result<IEnumerable<Interview>>> GetByInvestigationAsync(string investigationCode, CancellationToken ct = default)
    {
        return _repo.GetByInvestigationAsync(investigationCode, ct);
    }

    public Task<Result<IEnumerable<Interview>>> GetByInvestigatorAsync(string investigatorCode, CancellationToken ct = default)
    {
        return _repo.GetByInvestigatorAsync(investigatorCode, ct);
    }

    public Task<Result<IEnumerable<Interview>>> GetByStatusAsync(InterviewStatus status, CancellationToken ct = default)
    {
        return _repo.GetByStatusAsync(status, ct);
    }

    // Interview Workflow Operations
    public Task<Result<bool>> UpdateStatusAsync(string interviewCode, InterviewStatus status, CancellationToken ct = default)
    {
        return _repo.UpdateStatusAsync(interviewCode, status, ct);
    }

    public Task<Result<bool>> ScheduleInterviewAsync(string interviewCode, DateTime interviewDate, string location,
        int? durationMinutes = null, CancellationToken ct = default)
    {
        return _repo.ScheduleInterviewAsync(interviewCode, interviewDate, location, durationMinutes, ct);
    }

    public Task<Result<bool>> CompleteInterviewAsync(string interviewCode, string? personNotes, string? investigatorNotes,
        string? keyFindings = null, CancellationToken ct = default)
    {
        return _repo.CompleteInterviewAsync(interviewCode, personNotes, investigatorNotes, keyFindings, ct);
    }
}

