//-----------------------------------------------------------------------
// <copyright file="SMSAuditDataService.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: SMS Audit data service providing business operations for SMS domain entities.
//                  Infrastructure layer service implementing data access patterns
//                  through repositories while maintaining clean architecture.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;
using SMS_Infrastructure.Persistence;

namespace SMS_Infrastructure.Services;

/// <summary>
/// SMS Audit Data Service providing business operations for SMSAudit entities
/// </summary>
public class SMSAuditDataService : BaseDataService<SMSAuditDataService>
{
    private readonly ILogger<SMSAuditDataService> _logger;
    private readonly string _logheader;
    private readonly SMSAuditRepository _repo;

    public SMSAuditDataService(
        ILogger<SMSAuditDataService> logger,
        IServiceScopeFactory serviceScopeFactory,
        IConfiguration configuration,
        SMSAuditRepository repo)
        : base(logger, serviceScopeFactory, configuration)
    {
        _logger = base.Logger;
        _logheader = base.LogHeader;
        _repo = repo ?? throw new ArgumentNullException(nameof(repo));

        _logger.LogInfrastructureInformation(InfrastructureEventIds.InfrastructureEvent, $"{_logheader} SMSAuditDataService");
    }

    /// <summary>
    /// Creates a new SMS Audit
    /// </summary>
    public async Task<Result<SMSAudit>> CreateAuditAsync(SMSAudit audit, CancellationToken ct = default)
    {
        return await _repo.CreateSMSAuditAsync(audit, ct);
    }

    /// <summary>
    /// Updates an existing SMS Audit
    /// </summary>
    public async Task<Result<SMSAudit>> UpdateAuditAsync(SMSAudit audit, CancellationToken ct = default)
    {
        return await _repo.UpdateSMSAuditAsync(audit, ct);
    }

    /// <summary>
    /// Deletes an SMS Audit
    /// </summary>
    public async Task<Result<bool>> DeleteAuditAsync(string auditCode, string deletedBy, string reason, CancellationToken ct = default)
    {
        // Get the audit by code first to get the ID
        var auditResult = await _repo.DeleteSMSAuditAsync(auditCode, ct);
        if (auditResult.IsFailure)
        {
            return Result<bool>.Failure<bool>(auditResult.Error);
        }

        return auditResult.Value;
    }

    /// <summary>
    /// Gets all SMS Audits
    /// </summary>
    public async Task<Result<List<SMSAudit>>> GetAllAuditsAsync(CancellationToken ct = default)
    {
        var result = await _repo.GetAllSMSAuditsAsync(ct);
        if (result.IsSuccess)
        {
            return Result<List<SMSAudit>>.Success(result.Value.ToList());
        }
        return Result<List<SMSAudit>>.Failure<List<SMSAudit>>(result.Error);
    }

    /// <summary>
    /// Gets an SMS Audit by Code
    /// </summary>
    public async Task<Result<SMSAudit>> GetAuditByCodeAsync(string auditCode, bool includeFindings = false, bool includeEvidence = false, CancellationToken ct = default)
    {
        return await _repo.GetSMSAuditByCodeAsync(auditCode, ct);
    }

    /// <summary>
    /// Gets SMS Audits by plan code
    /// </summary>
    public async Task<Result<List<SMSAudit>>> GetAuditsByPlanAsync(string auditPlanCode, string? statusFilter = null, bool includeFindings = false, CancellationToken ct = default)
    {
        // For now, get all and filter in memory - could be optimized with specific repository methods
        var allAuditsResult = await _repo.GetAllSMSAuditsAsync(ct);
        if (allAuditsResult.IsFailure)
        {
            return Result<List<SMSAudit>>.Failure<List<SMSAudit>>(allAuditsResult.Error);
        }

        var filteredAudits = allAuditsResult.Value
            .Where(a => a.AuditPlanCode != null && a.AuditPlanCode.Equals(auditPlanCode, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrEmpty(statusFilter))
        {
            filteredAudits = filteredAudits.Where(a => a.Status.Equals(statusFilter, StringComparison.OrdinalIgnoreCase));
        }

        return Result<List<SMSAudit>>.Success(filteredAudits.ToList());
    }

    /// <summary>
    /// Gets SMS Audits by status
    /// </summary>
    public async Task<Result<List<SMSAudit>>> GetAuditsByStatusAsync(string status, string? departmentFilter = null, bool includeFindings = false, CancellationToken ct = default)
    {
        // For now, get all and filter in memory - could be optimized with specific repository methods
        var allAuditsResult = await _repo.GetAllSMSAuditsAsync(ct);
        if (allAuditsResult.IsFailure)
        {
            return Result<List<SMSAudit>>.Failure<List<SMSAudit>>(allAuditsResult.Error);
        }

        var filteredAudits = allAuditsResult.Value
            .Where(a => a.Status.Equals(status, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrEmpty(departmentFilter))
        {
            filteredAudits = filteredAudits.Where(a => a.ResponsibleDepartment.Equals(departmentFilter, StringComparison.OrdinalIgnoreCase));
        }

        return Result<List<SMSAudit>>.Success(filteredAudits.ToList());
    }

    /// <summary>
    /// Gets SMS Audits by auditor
    /// </summary>
    public async Task<Result<List<SMSAudit>>> GetAuditsByAuditorAsync(string auditor, string? statusFilter = null,
        DateTime? startDateFrom = null, DateTime? startDateTo = null, CancellationToken ct = default)
    {
        // For now, get all and filter in memory - could be optimized with specific repository methods
        var allAuditsResult = await _repo.GetAllSMSAuditsAsync(ct);
        if (allAuditsResult.IsFailure)
        {
            return Result<List<SMSAudit>>.Failure<List<SMSAudit>>(allAuditsResult.Error);
        }

        var filteredAudits = allAuditsResult.Value
            .Where(a => a.LeadAuditor.Equals(auditor, StringComparison.OrdinalIgnoreCase) ||
                       (a.AuditorTeam != null && a.AuditorTeam.Contains(auditor, StringComparison.OrdinalIgnoreCase)));

        if (!string.IsNullOrEmpty(statusFilter))
        {
            filteredAudits = filteredAudits.Where(a => a.Status.Equals(statusFilter, StringComparison.OrdinalIgnoreCase));
        }

        if (startDateFrom.HasValue)
        {
            filteredAudits = filteredAudits.Where(a => a.ScheduledStartDate >= startDateFrom.Value);
        }

        if (startDateTo.HasValue)
        {
            filteredAudits = filteredAudits.Where(a => a.ScheduledStartDate <= startDateTo.Value);
        }

        return Result<List<SMSAudit>>.Success(filteredAudits.ToList());
    }

    /// <summary>
    /// Gets overdue SMS Audits
    /// </summary>
    public async Task<Result<List<SMSAudit>>> GetOverdueAuditsAsync(string? departmentFilter = null, string? auditorFilter = null, CancellationToken ct = default)
    {
        // For now, get all and filter in memory - could be optimized with specific repository methods
        var allAuditsResult = await _repo.GetAllSMSAuditsAsync(ct);
        if (allAuditsResult.IsFailure)
        {
            return Result<List<SMSAudit>>.Failure<List<SMSAudit>>(allAuditsResult.Error);
        }

        var currentDate = DateTime.UtcNow;
        var overdueAudits = allAuditsResult.Value
                .Where(a => a.ScheduledEndDate < currentDate &&
               a.Status != "Completed" && a.Status != "Cancelled");
        if (!string.IsNullOrEmpty(departmentFilter))
        {
            overdueAudits = overdueAudits.Where(a => a.ResponsibleDepartment.Equals(departmentFilter, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrEmpty(auditorFilter))
        {
            overdueAudits = overdueAudits.Where(a => a.LeadAuditor.Equals(auditorFilter, StringComparison.OrdinalIgnoreCase));
        }

        return Result<List<SMSAudit>>.Success(overdueAudits.ToList());
    }

    /// <summary>
    /// Gets SMS Audit execution dashboard data
    /// </summary>
    public async Task<Result<SMSAuditExecutionDashboard>> GetAuditExecutionDashboardAsync(DateTime? startDate = null, DateTime? endDate = null,
        string? departmentFilter = null, string? auditorFilter = null, CancellationToken ct = default)
    {
        // Get all audits and calculate dashboard metrics
        var allAuditsResult = await _repo.GetAllSMSAuditsAsync(ct);
        if (allAuditsResult.IsFailure)
        {
            return Result<SMSAuditExecutionDashboard>.Failure<SMSAuditExecutionDashboard>(allAuditsResult.Error);
        }

        var audits = allAuditsResult.Value.ToList();

        // Filter by date range if specified
        if (startDate.HasValue)
        {
            audits = audits.Where(a => a.ScheduledStartDate >= startDate.Value).ToList();
        }
        if (endDate.HasValue)
        {
            audits = audits.Where(a => a.ScheduledStartDate <= endDate.Value).ToList();
        }

        // Filter by department if specified
        if (!string.IsNullOrEmpty(departmentFilter))
        {
            audits = audits.Where(a => a.ResponsibleDepartment.Equals(departmentFilter, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        // Filter by auditor if specified
        if (!string.IsNullOrEmpty(auditorFilter))
        {
            audits = audits.Where(a => a.LeadAuditor.Equals(auditorFilter, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        // Calculate dashboard metrics
        var dashboard = new SMSAuditExecutionDashboard
        {
            StartDate = startDate ?? DateTime.UtcNow.AddMonths(-12),
            EndDate = endDate ?? DateTime.UtcNow,
            TotalAuditPlans = 0, // Would need audit plan count
            TotalAuditsScheduled = audits.Count,
            AuditsCompleted = audits.Count(a => a.Status == "Completed"),
            AuditsInProgress = audits.Count(a => a.Status == "In Progress"),
            AuditsOverdue = audits.Count(a => a.ScheduledEndDate < DateTime.UtcNow && a.Status != "Completed" && a.Status != "Cancelled"),
            AuditsCancelled = audits.Count(a => a.Status == "Cancelled"),
            TotalFindings = audits.Sum(a => a.TotalFindings),
            CriticalFindings = audits.Sum(a => a.CriticalFindings),
            MajorFindings = audits.Sum(a => a.MajorFindings),
            MinorFindings = audits.Sum(a => a.MinorFindings),
            Observations = audits.Sum(a => a.Observations),
            FindingsResolved = 0, // Would need to calculate from findings
            FindingsOpen = audits.Sum(a => a.TotalFindings), // Simplified
            FindingsOverdue = 0, // Would need finding details
            AverageResolutionTimeDays = 0, // Would need finding resolution data
            OnTimeCompletionRate = audits.Any() ? (double)audits.Count(a => a.ActualEndDate <= a.ScheduledEndDate) / audits.Count * 100 : 0,
            AverageAuditDurationHours = 0, // Would need to calculate from actual vs scheduled times
            TotalEvidence = 0, // Would need evidence count
            EvidenceVerified = 0, // Would need evidence details
            EvidenceArchived = 0, // Would need evidence details
            AuditsByType = audits.GroupBy(a => a.AuditType).ToDictionary(g => g.Key, g => g.Count()),
            AuditsByDepartment = audits.GroupBy(a => a.ResponsibleDepartment).ToDictionary(g => g.Key, g => g.Count()),
            MonthlyTrends = new(), // Would need time-series data
            TopAuditors = new(), // Would need auditor performance data
            RecentActivities = new() // Would need activity tracking
        };

        return Result<SMSAuditExecutionDashboard>.Success(dashboard);
    }

    /// <summary>
    /// Schedules an audit from an audit plan
    /// </summary>
    public async Task<Result<SMSAudit>> ScheduleAuditFromPlanAsync(string auditPlanCode, DateTime scheduledDate, string scheduledBy,
        string contactPerson = "", string auditLocation = "", CancellationToken ct = default)
    {
        // Create a new audit based on the audit plan
        var auditId = new SMSAuditID($"AUD-{DateTime.UtcNow:yyyyMMddHHmmss}");
        var audit = new SMSAudit(auditId, scheduledBy)
        {
            AuditPlanCode = auditPlanCode,
            Name = $"Scheduled Audit - {auditPlanCode}",
            Description = "Audit scheduled from plan",
            ScheduledStartDate = scheduledDate,
            ScheduledEndDate = scheduledDate.AddDays(1), // Default 1 day duration
            Status = "Scheduled",
            ContactPerson = contactPerson,
            AuditLocation = auditLocation
        };

        return await _repo.CreateSMSAuditAsync(audit, ct);
    }
}
