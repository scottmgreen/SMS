using SMS_Infrastructure.Services;
using SMS_Domain.Entities;
using SMS_Shared.Common;
using Microsoft.Extensions.Logging;
using SMS_Application.Messaging.Queries;
using SMS_Application.Common;

namespace SMS_Application.Services;

/// <summary>
/// SMS Audit Planning Service for business logic and orchestration
/// </summary>
public class SMSAuditPlanService
{
    private readonly SMSAuditPlanDataService _auditPlanDataService;
    private readonly ILogger<SMSAuditPlanService> _logger;

    public SMSAuditPlanService(
        SMSAuditPlanDataService auditPlanDataService,
        ILogger<SMSAuditPlanService> logger)
    {
        _auditPlanDataService = auditPlanDataService ?? throw new ArgumentNullException(nameof(auditPlanDataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Creates a new audit plan
    /// </summary>
    public async Task<Result<SMSAuditPlan>> CreateAuditPlanAsync(SMSAuditPlan auditPlan, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Creating audit plan: {AuditPlanCode}", auditPlan.Code);
            return await _auditPlanDataService.CreateAuditPlanAsync(auditPlan, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating audit plan: {AuditPlanCode}", auditPlan?.Code);
            return Result<SMSAuditPlan>.Failure<SMSAuditPlan>(new Error("CREATE_FAILED", "Failed to create audit plan"));
        }
    }

    /// <summary>
    /// Updates an existing audit plan
    /// </summary>
    public async Task<Result<SMSAuditPlan>> UpdateAuditPlanAsync(SMSAuditPlan auditPlan, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Updating audit plan: {AuditPlanCode}", auditPlan.Code);
            return await _auditPlanDataService.UpdateAuditPlanAsync(auditPlan, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating audit plan: {AuditPlanCode}", auditPlan?.Code);
            return Result<SMSAuditPlan>.Failure<SMSAuditPlan>(new Error("UPDATE_FAILED", "Failed to update audit plan"));
        }
    }

    /// <summary>
    /// Gets audit plan by code
    /// </summary>
    public async Task<Result<SMSAuditPlan>> GetAuditPlanByCodeAsync(string auditPlanCode, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting audit plan by code: {AuditPlanCode}", auditPlanCode);
            return await _auditPlanDataService.GetAuditPlanByCodeAsync(auditPlanCode, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting audit plan by code: {AuditPlanCode}", auditPlanCode);
            return Result<SMSAuditPlan>.Failure<SMSAuditPlan>(new Error("GET_FAILED", "Failed to get audit plan"));
        }
    }

    /// <summary>
    /// Gets all audit plans
    /// </summary>
    public async Task<Result<List<SMSAuditPlan>>> GetAllAuditPlansAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting all audit plans");
            return await _auditPlanDataService.GetAllAuditPlansAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all audit plans");
            return Result<List<SMSAuditPlan>>.Failure<List<SMSAuditPlan>>(new Error("GET_ALL_FAILED", "Failed to get audit plans"));
        }
    }

    /// <summary>
    /// Deletes an audit plan
    /// </summary>
    public async Task<Result<bool>> DeleteAuditPlanAsync(string auditPlanCode, string deletedBy, string reason, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Deleting audit plan: {AuditPlanCode} by {DeletedBy}", auditPlanCode, deletedBy);
            return await _auditPlanDataService.DeleteAuditPlanAsync(auditPlanCode, deletedBy, reason, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting audit plan: {AuditPlanCode}", auditPlanCode);
            return Result<bool>.Failure<bool>(new Error("DELETE_FAILED", "Failed to delete audit plan"));
        }
    }

    /// <summary>
    /// Gets comprehensive audit calendar data with analytics
    /// </summary>
    public async Task<Result<SMSAuditCalendarData>> GetAuditCalendarDataAsync(
        DateTime startDate, DateTime endDate, string? departmentFilter = null,
        string? auditTypeFilter = null, string? auditorFilter = null, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Getting SMS audit calendar data for date range: {StartDate} to {EndDate}", 
                startDate, endDate);

            var result = await _auditPlanDataService.GetAuditCalendarDataAsync(
                startDate, endDate, departmentFilter, auditTypeFilter, auditorFilter, ct);

            if (result.IsSuccess)
            {
                // Convert Domain model to Application model
                var calendarData = result.Value.ToApplicationModel();
                
                // Apply additional business logic or calculations here if needed
                // For example: calculate workload distribution, resource conflicts, etc.
                
                _logger.LogInformation("Successfully retrieved SMS audit calendar data with {ScheduledAudits} scheduled audits", 
                    calendarData.ScheduledAudits?.Count ?? 0);

                return Result<SMSAuditCalendarData>.Success(calendarData);
            }

            return Result<SMSAuditCalendarData>.Failure<SMSAuditCalendarData>(result.Error);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting SMS audit calendar data");
            return Result<SMSAuditCalendarData>.Failure<SMSAuditCalendarData>(new Error("CALENDAR_DATA_FAILED", "Failed to get audit calendar data"));
        }
    }

    /// <summary>
    /// Gets audit plans by type with business filtering
    /// </summary>
    public async Task<Result<List<SMSAuditPlan>>> GetAuditPlansByTypeAsync(string auditType, 
        string? statusFilter = null, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Getting audit plans by type: {AuditType}", auditType);
            return await _auditPlanDataService.GetAuditPlansByTypeAsync(auditType, statusFilter, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting audit plans by type: {AuditType}", auditType);
            return Result<List<SMSAuditPlan>>.Failure<List<SMSAuditPlan>>(new Error("GET_BY_TYPE_FAILED", "Failed to get audit plans by type"));
        }
    }

    /// <summary>
    /// Gets audit plans by department with business filtering
    /// </summary>
    public async Task<Result<List<SMSAuditPlan>>> GetAuditPlansByDepartmentAsync(string department, 
        string? statusFilter = null, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Getting audit plans by department: {Department}", department);
            return await _auditPlanDataService.GetAuditPlansByDepartmentAsync(department, statusFilter, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting audit plans by department: {Department}", department);
            return Result<List<SMSAuditPlan>>.Failure<List<SMSAuditPlan>>(new Error("GET_BY_DEPARTMENT_FAILED", "Failed to get audit plans by department"));
        }
    }

    /// <summary>
    /// Gets audit plans requiring approval with business logic
    /// </summary>
    public async Task<Result<List<SMSAuditPlan>>> GetAuditPlansRequiringApprovalAsync(CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Getting audit plans requiring approval");
            
            var result = await _auditPlanDataService.GetAuditPlansRequiringApprovalAsync(ct);
            
            if (result.IsSuccess)
            {
                var plans = result.Value;
                
                // Sort by priority: Regulatory > External > Management > Internal
                var prioritizedPlans = plans
                    .OrderBy(p => GetApprovalPriority(p.AuditType))
                    .ThenBy(p => p.PlannedStartDate)
                    .ToList();
                
                return Result<List<SMSAuditPlan>>.Success(prioritizedPlans);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting audit plans requiring approval");
            return Result<List<SMSAuditPlan>>.Failure<List<SMSAuditPlan>>(new Error("GET_APPROVAL_FAILED", "Failed to get audit plans requiring approval"));
        }
    }

    /// <summary>
    /// Validates audit plan business rules
    /// </summary>
    public Result ValidateAuditPlan(SMSAuditPlan auditPlan)
    {
        try
        {
            if (auditPlan == null)
                return Result.Failure(new Error("AUDIT_PLAN_NULL", "Audit plan cannot be null"));

            if (string.IsNullOrWhiteSpace(auditPlan.Name))
                return Result.Failure(new Error("AUDIT_PLAN_NAME_REQUIRED", "Audit plan name is required"));

            if (string.IsNullOrWhiteSpace(auditPlan.AuditType))
                return Result.Failure(new Error("AUDIT_TYPE_REQUIRED", "Audit type is required"));

            if (string.IsNullOrWhiteSpace(auditPlan.LeadAuditor))
                return Result.Failure(new Error("LEAD_AUDITOR_REQUIRED", "Lead auditor is required"));

            if (auditPlan.PlannedStartDate >= auditPlan.PlannedEndDate)
                return Result.Failure(new Error("INVALID_DATE_RANGE", "Planned end date must be after start date"));

            if (auditPlan.PlannedStartDate < DateTime.UtcNow.Date)
                return Result.Failure(new Error("PAST_START_DATE", "Planned start date cannot be in the past"));

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating audit plan: {AuditPlanCode}", auditPlan?.Code);
            return Result.Failure(new Error("VALIDATION_ERROR", "Error occurred during audit plan validation"));
        }
    }

    /// <summary>
    /// Generates audit plan code based on naming convention
    /// </summary>
    
    /// <summary>
    /// Calculates recommended audit duration based on scope and type
    /// </summary>
    public int CalculateRecommendedDuration(string auditType, string auditScope)
    {
        try
        {
            var baseDuration = auditType switch
            {
                "Internal" => 8, // 1 day
                "External" => 16, // 2 days
                "Regulatory" => 24, // 3 days
                "Management" => 4, // Half day
                _ => 8
            };

            // Adjust based on scope complexity
            var scopeFactor = auditScope.Length switch
            {
                > 500 => 1.5,
                > 200 => 1.2,
                _ => 1.0
            };

            return (int)(baseDuration * scopeFactor);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating recommended duration for audit type: {AuditType}", auditType);
            return 8; // Default to 1 day
        }
    }

    /// <summary>
    /// Gets next recommended review date based on audit type and frequency
    /// </summary>
    public DateTime GetNextRecommendedDate(string auditType, string recurrencePattern, DateTime lastAuditDate)
    {
        try
        {
            return recurrencePattern switch
            {
                "Annual" => lastAuditDate.AddYears(1),
                "Quarterly" => lastAuditDate.AddMonths(3),
                "Monthly" => lastAuditDate.AddMonths(1),
                "Semi-Annual" => lastAuditDate.AddMonths(6),
                _ => auditType switch
                {
                    "Internal" => lastAuditDate.AddYears(1),
                    "Regulatory" => lastAuditDate.AddYears(1),
                    "Management" => lastAuditDate.AddMonths(6),
                    _ => lastAuditDate.AddYears(1)
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating next recommended date");
            return DateTime.UtcNow.AddYears(1); // Default to 1 year
        }
    }

    /// <summary>
    /// Validates audit scheduling constraints
    /// </summary>
    public Result ValidateAuditScheduling(SMSAuditPlan auditPlan, DateTime proposedDate, 
        List<SMSAudit> existingAudits)
    {
        try
        {
            if (auditPlan.Status != "Approved")
                return Result.Failure(new Error("PLAN_NOT_APPROVED", "Audit plan must be approved before scheduling"));

            if (proposedDate < DateTime.UtcNow.Date)
                return Result.Failure(new Error("PAST_DATE", "Cannot schedule audit in the past"));

            // Check for auditor availability (simplified check)
            var conflictingAudits = existingAudits?.Where(a => 
                a.LeadAuditor == auditPlan.LeadAuditor &&
                a.Status == "Scheduled" &&
                Math.Abs((a.ScheduledStartDate - proposedDate).TotalDays) < 1
            ).ToList();

            if (conflictingAudits?.Any() == true)
                return Result.Failure(new Error("AUDITOR_CONFLICT", 
                    $"Lead auditor {auditPlan.LeadAuditor} has conflicting audit scheduled"));

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating audit scheduling");
            return Result.Failure(new Error("VALIDATION_ERROR", "Error occurred during scheduling validation"));
        }
    }

    /// <summary>
    /// Validates audit plan approval authority
    /// </summary>
    public Result ValidateApprovalAuthority(SMSAuditPlan auditPlan, string approverRole)
    {
        try
        {
            var requiredRole = auditPlan.AuditType switch
            {
                "Regulatory" => "QualityManager",
                "Management" => "AuditManager", 
                "External" => "QualityManager",
                _ => "AuditSupervisor"
            };

            // This would typically check against actual user roles
            // For now, simplified validation
            if (string.IsNullOrWhiteSpace(approverRole))
                return Result.Failure(new Error("INVALID_APPROVER", "Approver role not specified"));

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating approval authority");
            return Result.Failure(new Error("VALIDATION_ERROR", "Error occurred during approval validation"));
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
    /// Gets approval priority for sorting
    /// </summary>
    private int GetApprovalPriority(string auditType)
    {
        return auditType switch
        {
            "Regulatory" => 1,
            "External" => 2,
            "Management" => 3,
            "Internal" => 4,
            _ => 5
        };
    }

    #endregion
}