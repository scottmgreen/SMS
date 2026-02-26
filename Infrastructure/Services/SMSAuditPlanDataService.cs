//-----------------------------------------------------------------------
// <copyright file="SMSAuditPlanDataService.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Data service coordinating smsauditplan repository operations supporting compliance and audit processes.
//                  Infrastructure service providing external system integration
//                  and technical functionality support.
// </copyright>
//-----------------------------------------------------------------------

using Domain.Models;

using Infrastructure.Persistence;

namespace SMS_Infrastructure.Services;

/// <summary>
/// Data service for SMS Audit Plan operations
/// Handles all database interactions for SMS audit plan management
/// </summary>
public class SMSAuditPlanDataService : BaseDataService<SMSAuditPlanDataService>
{
    private readonly ILogger<SMSAuditPlanDataService> _logger;
    private readonly string _logheader;
    private readonly SMSAuditPlanRepository _repo;

    public SMSAuditPlanDataService(
        ILogger<SMSAuditPlanDataService> logger,
        IServiceScopeFactory serviceScopeFactory,
        IConfiguration configuration,
        SMSAuditPlanRepository repo)
        : base(logger, serviceScopeFactory, configuration)
    {
        _logger = base.Logger;
        _logheader = base.LogHeader;
        _repo = repo ?? throw new ArgumentNullException(nameof(repo));

        _logger.LogInfrastructureInformation(InfrastructureEventIds.InfrastructureEvent, $"{_logheader} SMSAuditPlanDataService");
    }

    /// <summary>
    /// Creates a new SMS Audit Plan
    /// </summary>
    public async Task<Result<SMSAuditPlan>> CreateAuditPlanAsync(SMSAuditPlan auditPlan, CancellationToken ct = default)
    {
        return await _repo.CreateSMSAuditPlanAsync(auditPlan, ct);
    }

    /// <summary>
    /// Updates an existing SMS Audit Plan
    /// </summary>
    public async Task<Result<SMSAuditPlan>> UpdateAuditPlanAsync(SMSAuditPlan auditPlan, CancellationToken ct = default)
    {
        return await _repo.UpdateSMSAuditPlanAsync(auditPlan, ct);
    }

    /// <summary>
    /// Deletes an SMS Audit Plan
    /// </summary>
    public async Task<Result<bool>> DeleteAuditPlanAsync(string auditPlanCode, string deletedBy, string reason, CancellationToken ct = default)
    {
        // Get the audit plan by code first to get the ID
        var auditPlanResult = await _repo.GetSMSAuditPlanByCodeAsync(auditPlanCode, ct);
        if (auditPlanResult.IsFailure)
        {
            return Result<bool>.Failure<bool>(auditPlanResult.Error);
        }

        return await _repo.DeleteSMSAuditPlanAsync(auditPlanResult.Value.Code, ct);
    }

    /// <summary>
    /// Gets all SMS Audit Plans
    /// </summary>
    public async Task<Result<List<SMSAuditPlan>>> GetAllAuditPlansAsync(CancellationToken ct = default)
    {
        var result = await _repo.GetAllSMSAuditPlansAsync(ct);
        if (result.IsSuccess)
        {
            return Result<List<SMSAuditPlan>>.Success(result.Value.ToList());
        }
        return Result<List<SMSAuditPlan>>.Failure<List<SMSAuditPlan>>(result.Error);
    }

    /// <summary>
    /// Gets an SMS Audit Plan by Code
    /// </summary>
    public async Task<Result<SMSAuditPlan>> GetAuditPlanByCodeAsync(string auditPlanCode, CancellationToken ct = default)
    {
        return await _repo.GetSMSAuditPlanByCodeAsync(auditPlanCode, ct);
    }

    /// <summary>
    /// Gets SMS Audit Plans by type
    /// </summary>
    public async Task<Result<List<SMSAuditPlan>>> GetAuditPlansByTypeAsync(string auditType, string? statusFilter = null, CancellationToken ct = default)
    {
        // For now, get all and filter in memory - could be optimized with specific repository methods
        var allPlansResult = await _repo.GetAllSMSAuditPlansAsync(ct);
        if (allPlansResult.IsFailure)
        {
            return Result<List<SMSAuditPlan>>.Failure<List<SMSAuditPlan>>(allPlansResult.Error);
        }

        var filteredPlans = allPlansResult.Value
            .Where(ap => ap.AuditType.Equals(auditType, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrEmpty(statusFilter))
        {
            filteredPlans = filteredPlans.Where(ap => ap.Status.Equals(statusFilter, StringComparison.OrdinalIgnoreCase));
        }

        return Result<List<SMSAuditPlan>>.Success(filteredPlans.ToList());
    }

    /// <summary>
    /// Gets SMS Audit Plans by department
    /// </summary>
    public async Task<Result<List<SMSAuditPlan>>> GetAuditPlansByDepartmentAsync(string department, string? statusFilter = null, CancellationToken ct = default)
    {
        // For now, get all and filter in memory - could be optimized with specific repository methods
        var allPlansResult = await _repo.GetAllSMSAuditPlansAsync(ct);
        if (allPlansResult.IsFailure)
        {
            return Result<List<SMSAuditPlan>>.Failure<List<SMSAuditPlan>>(allPlansResult.Error);
        }

        var filteredPlans = allPlansResult.Value
            .Where(ap => ap.ResponsibleDepartment.Equals(department, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrEmpty(statusFilter))
        {
            filteredPlans = filteredPlans.Where(ap => ap.Status.Equals(statusFilter, StringComparison.OrdinalIgnoreCase));
        }

        return Result<List<SMSAuditPlan>>.Success(filteredPlans.ToList());
    }

    /// <summary>
    /// Gets SMS Audit Plans requiring approval
    /// </summary>
    public async Task<Result<List<SMSAuditPlan>>> GetAuditPlansRequiringApprovalAsync(CancellationToken ct = default)
    {
        // For now, get all and filter for "Draft" status - could be optimized with specific repository methods
        var allPlansResult = await _repo.GetAllSMSAuditPlansAsync(ct);
        if (allPlansResult.IsFailure)
        {
            return Result<List<SMSAuditPlan>>.Failure<List<SMSAuditPlan>>(allPlansResult.Error);
        }

        var plansRequiringApproval = allPlansResult.Value
            .Where(ap => ap.Status.Equals("Draft", StringComparison.OrdinalIgnoreCase) && ap.RequiresApproval)
            .ToList();

        return Result<List<SMSAuditPlan>>.Success(plansRequiringApproval);
    }

    /// <summary>
    /// Gets SMS Audit Calendar data for dashboard
    /// </summary>
    public async Task<Result<SMSAuditCalendarData>> GetAuditCalendarDataAsync(DateTime startDate, DateTime endDate,
        string? departmentFilter = null, string? auditTypeFilter = null, string? auditorFilter = null, CancellationToken ct = default)
    {
        // This method would need to be implemented based on specific business requirements
        // For now, return a placeholder implementation
        var calendarData = new SMSAuditCalendarData
        {
            Id = Guid.NewGuid().ToString(),
            Title = "Audit Calendar Data",
            Description = "Placeholder calendar data",
            EventType = "AuditPlan",
            StartDate = startDate,
            EndDate = endDate,
            Status = "Active",
            Priority = "Medium",
            LeadAuditor = "TBD",
            ResponsibleDepartment = departmentFilter ?? "All Departments"
        };

        return Result<SMSAuditCalendarData>.Success(calendarData);
    }
}
