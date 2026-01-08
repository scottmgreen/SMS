using SMS_Infrastructure.Services;
using SMS_Domain.Entities;
using SMS_Shared.Common;
using Microsoft.Extensions.Logging;
using SMS_Application.Messaging.Queries;
using SMS_Application.Common;

namespace SMS_Application.Services;

/// <summary>
/// SMS Audit Execution Service for business logic and orchestration
/// </summary>
public class SMSAuditService
{
    private readonly SMSAuditDataService _auditDataService;
    private readonly ILogger<SMSAuditService> _logger;

    public SMSAuditService(
        SMSAuditDataService auditDataService,
        ILogger<SMSAuditService> logger)
    {
        _auditDataService = auditDataService ?? throw new ArgumentNullException(nameof(auditDataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Creates a new audit
    /// </summary>
    public async Task<Result<SMSAudit>> CreateAuditAsync(SMSAudit audit, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Creating audit: {AuditCode}", audit.Code);
            return await _auditDataService.CreateAuditAsync(audit, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating audit: {AuditCode}", audit?.Code);
            return Result<SMSAudit>.Failure<SMSAudit>(new Error("CREATE_FAILED", "Failed to create audit"));
        }
    }

    /// <summary>
    /// Updates an existing audit
    /// </summary>
    public async Task<Result<SMSAudit>> UpdateAuditAsync(SMSAudit audit, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Updating audit: {AuditCode}", audit.Code);
            return await _auditDataService.UpdateAuditAsync(audit, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating audit: {AuditCode}", audit?.Code);
            return Result<SMSAudit>.Failure<SMSAudit>(new Error("UPDATE_FAILED", "Failed to update audit"));
        }
    }

    /// <summary>
    /// Gets audit by code
    /// </summary>
    public async Task<Result<SMSAudit>> GetAuditByCodeAsync(string auditCode, CancellationToken cancellationToken = default, bool includeFindings = false, bool includeEvidence = false)
    {
        try
        {
            _logger.LogInformation("Getting audit by code: {AuditCode}", auditCode);
            return await _auditDataService.GetAuditByCodeAsync(auditCode, includeFindings, includeEvidence, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting audit by code: {AuditCode}", auditCode);
            return Result<SMSAudit>.Failure<SMSAudit>(new Error("GET_FAILED", "Failed to get audit"));
        }
    }

    /// <summary>
    /// Gets all audits
    /// </summary>
    public async Task<Result<List<SMSAudit>>> GetAllAuditsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting all audits");
            return await _auditDataService.GetAllAuditsAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all audits");
            return Result<List<SMSAudit>>.Failure<List<SMSAudit>>(new Error("GET_ALL_FAILED", "Failed to get audits"));
        }
    }

    /// <summary>
    /// Deletes an audit
    /// </summary>
    public async Task<Result<bool>> DeleteAuditAsync(string auditCode, string deletedBy, string reason, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Deleting audit: {AuditCode} by {DeletedBy}", auditCode, deletedBy);
            return await _auditDataService.DeleteAuditAsync(auditCode, deletedBy, reason, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting audit: {AuditCode}", auditCode);
            return Result<bool>.Failure<bool>(new Error("DELETE_FAILED", "Failed to delete audit"));
        }
    }

    /// <summary>
    /// Gets comprehensive audit execution dashboard data with analytics
    /// </summary>
    public async Task<Result<SMSAuditExecutionDashboard>> GetAuditExecutionDashboardAsync(
        DateTime? startDate = null, DateTime? endDate = null, string? departmentFilter = null,
        string? auditorFilter = null, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Getting SMS audit execution dashboard data");

            var result = await _auditDataService.GetAuditExecutionDashboardAsync(
                startDate ?? DateTime.UtcNow.AddMonths(-12),
                endDate ?? DateTime.UtcNow,
                departmentFilter,
                auditorFilter,
                ct);

            if (result.IsSuccess)
            {
                // Convert Domain model to Application model and apply business logic
                var dashboardData = result.Value.ToApplicationModel();
                
                // Apply additional business logic calculations here
                CalculatePerformanceMetrics(dashboardData);
                
                _logger.LogInformation("Successfully retrieved SMS audit execution dashboard with {TotalAudits} audits", 
                    dashboardData.TotalAudits);

                return Result<SMSAuditExecutionDashboard>.Success(dashboardData);
            }

            return Result<SMSAuditExecutionDashboard>.Failure<SMSAuditExecutionDashboard>(result.Error);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting SMS audit execution dashboard");
            return Result<SMSAuditExecutionDashboard>.Failure<SMSAuditExecutionDashboard>(new Error("DASHBOARD_FAILED", "Failed to get audit execution dashboard"));
        }
    }

    /// <summary>
    /// Gets audits by plan with business filtering
    /// </summary>
    public async Task<Result<List<SMSAudit>>> GetAuditsByPlanAsync(string auditPlanCode, 
        string? statusFilter = null, bool includeFindings = false, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Getting audits by plan: {AuditPlanCode}", auditPlanCode);
            return await _auditDataService.GetAuditsByPlanAsync(auditPlanCode, statusFilter, includeFindings, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting audits by plan: {AuditPlanCode}", auditPlanCode);
            return Result<List<SMSAudit>>.Failure<List<SMSAudit>>(new Error("GET_BY_PLAN_FAILED", "Failed to get audits by plan"));
        }
    }

    /// <summary>
    /// Gets audits by status with business filtering
    /// </summary>
    public async Task<Result<List<SMSAudit>>> GetAuditsByStatusAsync(string status, 
        string? departmentFilter = null, bool includeFindings = false, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Getting audits by status: {Status}", status);
            return await _auditDataService.GetAuditsByStatusAsync(status, departmentFilter, includeFindings, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting audits by status: {Status}", status);
            return Result<List<SMSAudit>>.Failure<List<SMSAudit>(new Error("GET_BY_STATUS_FAILED", "Failed to get audits by status"));
        }
    }

    /// <summary>
    /// Gets audits by auditor with business filtering
    /// </summary>
    public async Task<Result<List<SMSAudit>>> GetAuditsByAuditorAsync(string auditor, 
        string? statusFilter = null, DateTime? startDateFrom = null, DateTime? startDateTo = null, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Getting audits by auditor: {Auditor}", auditor);
            return await _auditDataService.GetAuditsByAuditorAsync(auditor, statusFilter, startDateFrom, startDateTo, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting audits by auditor: {Auditor}", auditor);
            return Result<List<SMSAudit>>.Failure<List<SMSAudit>(new Error("GET_BY_AUDITOR_FAILED", "Failed to get audits by auditor"));
        }
    }

    /// <summary>
    /// Gets overdue audits with prioritization
    /// </summary>
    public async Task<Result<List<SMSAudit>>> GetOverdueAuditsAsync(
        string? departmentFilter = null, string? auditorFilter = null, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Getting overdue audits");
            
            var result = await _auditDataService.GetOverdueAuditsAsync(departmentFilter, auditorFilter, ct);
            
            if (result.IsSuccess)
            {
                var audits = result.Value;
                
                // Sort by priority: Critical findings > Regulatory > Days overdue
                var prioritizedAudits = audits
                    .OrderByDescending(a => a.CriticalFindings)
                    .ThenBy(a => GetAuditTypePriority(a.AuditType))
                    .ThenByDescending(a => (DateTime.UtcNow - a.ScheduledEndDate).TotalDays)
                    .ToList();
                
                return Result<List<SMSAudit>>.Success(prioritizedAudits);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting overdue audits");
            return Result<List<SMSAudit>>.Failure<List<SMSAudit>(new Error("GET_OVERDUE_FAILED", "Failed to get overdue audits"));
        }
    }

    /// <summary>
    /// Schedules an audit from an audit plan
    /// </summary>
    public async Task<Result<SMSAudit>> ScheduleAuditFromPlanAsync(string auditPlanCode, DateTime scheduledDate,
        string scheduledBy, string contactPerson = "", string auditLocation = "", CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Scheduling audit from plan: {AuditPlanCode} for date: {ScheduledDate}", 
                auditPlanCode, scheduledDate);
            
            return await _auditDataService.ScheduleAuditFromPlanAsync(
                auditPlanCode, scheduledDate, scheduledBy, contactPerson, auditLocation, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scheduling audit from plan: {AuditPlanCode}", auditPlanCode);
            return Result<SMSAudit>.Failure<SMSAudit>(new Error("SCHEDULE_FAILED", "Failed to schedule audit from plan"));
        }
    }

    /// <summary>
    /// Validates audit business rules
    /// </summary>
    public Result ValidateAudit(SMSAudit audit)
    {
        try
        {
            if (audit == null)
                return Result.Failure(new Error("AUDIT_NULL", "Audit cannot be null"));

            if (string.IsNullOrWhiteSpace(audit.Name))
                return Result.Failure(new Error("AUDIT_NAME_REQUIRED", "Audit name is required"));

            if (string.IsNullOrWhiteSpace(audit.AuditType))
                return Result.Failure(new Error("AUDIT_TYPE_REQUIRED", "Audit type is required"));

            if (string.IsNullOrWhiteSpace(audit.LeadAuditor))
                return Result.Failure(new Error("LEAD_AUDITOR_REQUIRED", "Lead auditor is required"));

            if (audit.ScheduledStartDate >= audit.ScheduledEndDate)
                return Result.Failure(new Error("INVALID_DATE_RANGE", "Scheduled end date must be after start date"));

            if (audit.ScheduledStartDate < DateTime.UtcNow.Date)
                return Result.Failure(new Error("PAST_START_DATE", "Scheduled start date cannot be in the past"));

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating audit: {AuditCode}", audit?.Code);
            return Result.Failure(new Error("VALIDATION_ERROR", "Error occurred during audit validation"));
        }
    }

    /// <summary>
    /// Generates audit code based on naming convention
    /// </summary>
    public string GenerateAuditCode(string auditType, DateTime scheduledStartDate)
    {
        try
        {
            var typeCode = GetAuditTypeCode(auditType);
            var yearMonth = scheduledStartDate.ToString("yyyyMM");
            var sequence = DateTime.UtcNow.ToString("HHmmss");
            
            return $"AUD-{typeCode}-{yearMonth}-{sequence}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating audit code for type: {AuditType}", auditType);
            return $"AUD-GEN-{DateTime.UtcNow:yyyyMMddHHmmss}";
        }
    }

    #region Private Helper Methods

    /// <summary>
    /// Gets audit type code for code generation
    /// </summary>
    private string GetAuditTypeCode(string auditType)
    {
        return auditType switch
        {
            "Internal" => "INT",
            "External" => "EXT",
            "Regulatory" => "REG",
            "Management" => "MGT",
            "Compliance" => "COM",
            "Quality" => "QUA",
            "Safety" => "SAF",
            _ => "GEN"
        };
    }

    /// <summary>
    /// Gets audit type priority for sorting overdue audits
    /// </summary>
    private int GetAuditTypePriority(string auditType)
    {
        return auditType switch
        {
            "Regulatory" => 1,
            "External" => 2,
            "Safety" => 3,
            "Quality" => 4,
            "Compliance" => 5,
            "Management" => 6,
            "Internal" => 7,
            _ => 8
        };
    }

    /// <summary>
    /// Calculates additional performance metrics for dashboard
    /// </summary>
    private void CalculatePerformanceMetrics(SMSAuditExecutionDashboard dashboardData)
    {
        try
        {
            // Calculate average audit duration if not already set
            if (dashboardData.AverageAuditDuration == 0 && dashboardData.CompletedAudits > 0)
            {
                // This would typically be calculated from actual audit data
                dashboardData.AverageAuditDuration = GetEstimatedAverageDuration();
            }

            // Calculate on-time completion rate
            if (dashboardData.TotalAudits > 0)
            {
                var onTimeAudits = dashboardData.CompletedAudits - dashboardData.OverdueAudits;
                dashboardData.OnTimeCompletionRate = Math.Max(0, 
                    (decimal)onTimeAudits / dashboardData.TotalAudits * 100);
            }

            // Calculate finding closure rate
            if (dashboardData.TotalFindings > 0)
            {
                // This would be calculated from actual finding closure data
                dashboardData.FindingClosureRate = 75; // Placeholder calculation
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating performance metrics for dashboard");
            // Don't fail the entire operation for metric calculation errors
        }
    }

    /// <summary>
    /// Gets estimated average duration based on audit types
    /// </summary>
    private decimal GetEstimatedAverageDuration()
    {
        // This would typically query actual audit durations from the database
        // For now, return a reasonable default
        return 2.5m; // 2.5 days average
    }

    #endregion
}