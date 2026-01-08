using SMS_Application.Common;
using SMS_Application.Interfaces;
using SMS_Domain.Entities;
using SMS_Shared.Common;

namespace SMS_Application.Messaging.Queries;

/// <summary>
/// SMS Audit Plan Queries for CQRS pattern
/// </summary>

// GET ALL SMS AUDIT PLANS
public class GetAllSMSAuditPlansQuery : BaseQueryBundle, IRequest<Result<List<SMSAuditPlan>>>
{
    public string? StatusFilter { get; set; }
    public string? AuditTypeFilter { get; set; }
    public string? DepartmentFilter { get; set; }
    public DateTime? StartDateFrom { get; set; }
    public DateTime? StartDateTo { get; set; }

    public GetAllSMSAuditPlansQuery(string? statusFilter = null, string? auditTypeFilter = null,
        string? departmentFilter = null, DateTime? startDateFrom = null, DateTime? startDateTo = null)
    {
        StatusFilter = statusFilter;
        AuditTypeFilter = auditTypeFilter;
        DepartmentFilter = departmentFilter;
        StartDateFrom = startDateFrom;
        StartDateTo = startDateTo;
    }
}

// GET SMS AUDIT PLAN BY CODE
public class GetSMSAuditPlanByCodeQuery : BaseQueryBundle, IRequest<Result<SMSAuditPlan>>
{
    public string AuditPlanCode { get; set; }
    public bool IncludeAudits { get; set; }

    public GetSMSAuditPlanByCodeQuery(string auditPlanCode, bool includeAudits = false)
    {
        AuditPlanCode = auditPlanCode ?? throw new ArgumentNullException(nameof(auditPlanCode));
        IncludeAudits = includeAudits;
    }
}

// GET SMS AUDIT PLANS BY TYPE
public class GetSMSAuditPlansByTypeQuery : BaseQueryBundle, IRequest<Result<List<SMSAuditPlan>>>
{
    public string AuditType { get; set; }
    public string? StatusFilter { get; set; }

    public GetSMSAuditPlansByTypeQuery(string auditType, string? statusFilter = null)
    {
        AuditType = auditType ?? throw new ArgumentNullException(nameof(auditType));
        StatusFilter = statusFilter;
    }
}

// GET SMS AUDIT PLANS BY DEPARTMENT
public class GetSMSAuditPlansByDepartmentQuery : BaseQueryBundle, IRequest<Result<List<SMSAuditPlan>>>
{
    public string Department { get; set; }
    public string? StatusFilter { get; set; }

    public GetSMSAuditPlansByDepartmentQuery(string department, string? statusFilter = null)
    {
        Department = department ?? throw new ArgumentNullException(nameof(department));
        StatusFilter = statusFilter;
    }
}

// GET SMS AUDIT PLANS REQUIRING APPROVAL
public class GetSMSAuditPlansRequiringApprovalQuery : BaseQueryBundle, IRequest<Result<List<SMSAuditPlan>>>
{
    public GetSMSAuditPlansRequiringApprovalQuery()
    {
        // No parameters needed - gets all plans with status "Draft"
    }
}

// GET SMS AUDIT CALENDAR DATA
public class GetSMSAuditCalendarQuery : BaseQueryBundle, IRequest<Result<SMSAuditCalendarData>>
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? DepartmentFilter { get; set; }
    public string? AuditTypeFilter { get; set; }
    public string? AuditorFilter { get; set; }

    public GetSMSAuditCalendarQuery(DateTime startDate, DateTime endDate,
        string? departmentFilter = null, string? auditTypeFilter = null, string? auditorFilter = null)
    {
        StartDate = startDate;
        EndDate = endDate;
        DepartmentFilter = departmentFilter;
        AuditTypeFilter = auditTypeFilter;
        AuditorFilter = auditorFilter;
    }
}

/// <summary>
/// SMS Audit Calendar Data for dashboard and calendar views
/// </summary>
public class SMSAuditCalendarData
{
    public List<SMSAuditCalendarEntry> ScheduledAudits { get; set; } = new();
    public List<SMSAuditCalendarEntry> InProgressAudits { get; set; } = new();
    public List<SMSAuditCalendarEntry> CompletedAudits { get; set; } = new();
    public List<SMSAuditCalendarEntry> OverdueAudits { get; set; } = new();
    public SMSAuditCalendarSummary Summary { get; set; } = new();
}

/// <summary>
/// SMS Audit Calendar Entry for calendar display
/// </summary>
public class SMSAuditCalendarEntry
{
    public string AuditCode { get; set; } = string.Empty;
    public string AuditPlanCode { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string AuditType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string LeadAuditor { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public int ProgressPercentage { get; set; }
    public string Priority { get; set; } = string.Empty; // High, Medium, Low
}

/// <summary>
/// SMS Audit Calendar Summary statistics
/// </summary>
public class SMSAuditCalendarSummary
{
    public int TotalScheduledAudits { get; set; }
    public int AuditsThisMonth { get; set; }
    public int AuditsInProgress { get; set; }
    public int OverdueAudits { get; set; }
    public int CompletedThisMonth { get; set; }
    public Dictionary<string, int> AuditsByType { get; set; } = new();
    public Dictionary<string, int> AuditsByDepartment { get; set; } = new();
    public Dictionary<string, int> AuditsByStatus { get; set; } = new();
}