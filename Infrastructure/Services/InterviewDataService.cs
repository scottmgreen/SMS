//-----------------------------------------------------------------------
// <copyright file="InterviewDataService.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Interview data service providing business operations for SMS domain entities.
//                  Infrastructure layer service implementing data access patterns
//                  through repositories while maintaining clean architecture.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;
using SMS_Infrastructure.Interfaces;
using SMS_Infrastructure.Persistence;

namespace SMS_Infrastructure.Services;

/// <summary>
/// Interview Data Service providing business operations for Interview entities
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

