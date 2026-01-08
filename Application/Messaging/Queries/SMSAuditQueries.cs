using SMS_Application.Common;
using SMS_Application.Interfaces;
using SMS_Domain.Entities;
using SMS_Shared.Common;

namespace SMS_Application.Messaging.Queries;

/// <summary>
/// SMS Audit Queries for CQRS pattern
/// </summary>

// GET ALL SMS AUDITS
public class GetAllSMSAuditsQuery : BaseQueryBundle, IRequest<Result<List<SMSAudit>>>
{
    public string? StatusFilter { get; set; }
    public string? AuditTypeFilter { get; set; }
    public string? DepartmentFilter { get; set; }
    public string? AuditorFilter { get; set; }
    public DateTime? StartDateFrom { get; set; }
    public DateTime? StartDateTo { get; set; }
    public bool IncludeFindings { get; set; }
    public bool IncludeEvidence { get; set; }

    public GetAllSMSAuditsQuery(string? statusFilter = null, string? auditTypeFilter = null,
        string? departmentFilter = null, string? auditorFilter = null, DateTime? startDateFrom = null,
        DateTime? startDateTo = null, bool includeFindings = false, bool includeEvidence = false)
    {
        StatusFilter = statusFilter;
        AuditTypeFilter = auditTypeFilter;
        DepartmentFilter = departmentFilter;
        AuditorFilter = auditorFilter;
        StartDateFrom = startDateFrom;
        StartDateTo = startDateTo;
        IncludeFindings = includeFindings;
        IncludeEvidence = includeEvidence;
    }
}

// GET SMS AUDIT BY CODE
public class GetSMSAuditByCodeQuery : BaseQueryBundle, IRequest<Result<SMSAudit>>
{
    public string AuditCode { get; set; }
    public bool IncludeFindings { get; set; }
    public bool IncludeEvidence { get; set; }

    public GetSMSAuditByCodeQuery(string auditCode, bool includeFindings = true, bool includeEvidence = true)
    {
        AuditCode = auditCode ?? throw new ArgumentNullException(nameof(auditCode));
        IncludeFindings = includeFindings;
        IncludeEvidence = includeEvidence;
    }
}

// GET SMS AUDITS BY PLAN
public class GetSMSAuditsByPlanQuery : BaseQueryBundle, IRequest<Result<List<SMSAudit>>>
{
    public string AuditPlanCode { get; set; }
    public string? StatusFilter { get; set; }
    public bool IncludeFindings { get; set; }

    public GetSMSAuditsByPlanQuery(string auditPlanCode, string? statusFilter = null, bool includeFindings = false)
    {
        AuditPlanCode = auditPlanCode ?? throw new ArgumentNullException(nameof(auditPlanCode));
        StatusFilter = statusFilter;
        IncludeFindings = includeFindings;
    }
}

// GET SMS AUDITS BY STATUS
public class GetSMSAuditsByStatusQuery : BaseQueryBundle, IRequest<Result<List<SMSAudit>>>
{
    public string Status { get; set; }
    public string? DepartmentFilter { get; set; }
    public bool IncludeFindings { get; set; }

    public GetSMSAuditsByStatusQuery(string status, string? departmentFilter = null, bool includeFindings = false)
    {
        Status = status ?? throw new ArgumentNullException(nameof(status));
        DepartmentFilter = departmentFilter;
        IncludeFindings = includeFindings;
    }
}

// GET SMS AUDITS BY AUDITOR
public class GetSMSAuditsByAuditorQuery : BaseQueryBundle, IRequest<Result<List<SMSAudit>>>
{
    public string Auditor { get; set; }
    public string? StatusFilter { get; set; }
    public DateTime? StartDateFrom { get; set; }
    public DateTime? StartDateTo { get; set; }

    public GetSMSAuditsByAuditorQuery(string auditor, string? statusFilter = null,
        DateTime? startDateFrom = null, DateTime? startDateTo = null)
    {
        Auditor = auditor ?? throw new ArgumentNullException(nameof(auditor));
        StatusFilter = statusFilter;
        StartDateFrom = startDateFrom;
        StartDateTo = startDateTo;
    }
}

// GET OVERDUE SMS AUDITS
public class GetOverdueSMSAuditsQuery : BaseQueryBundle, IRequest<Result<List<SMSAudit>>>
{
    public string? DepartmentFilter { get; set; }
    public string? AuditorFilter { get; set; }

    public GetOverdueSMSAuditsQuery(string? departmentFilter = null, string? auditorFilter = null)
    {
        DepartmentFilter = departmentFilter;
        AuditorFilter = auditorFilter;
    }
}

// GET SMS AUDIT EXECUTION DASHBOARD
public class GetSMSAuditExecutionDashboardQuery : BaseQueryBundle, IRequest<Result<SMSAuditExecutionDashboard>>
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? DepartmentFilter { get; set; }
    public string? AuditorFilter { get; set; }

    public GetSMSAuditExecutionDashboardQuery(DateTime? startDate = null, DateTime? endDate = null,
        string? departmentFilter = null, string? auditorFilter = null)
    {
        StartDate = startDate ?? DateTime.UtcNow.AddMonths(-12);
        EndDate = endDate ?? DateTime.UtcNow;
        DepartmentFilter = departmentFilter;
        AuditorFilter = auditorFilter;
    }
}

/// <summary>
/// SMS Audit Execution Dashboard data
/// </summary>
public class SMSAuditExecutionDashboard
{
    public int TotalAudits { get; set; }
    public int ScheduledAudits { get; set; }
    public int InProgressAudits { get; set; }
    public int CompletedAudits { get; set; }
    public int OverdueAudits { get; set; }
    public int CancelledAudits { get; set; }
    
    // Findings Summary
    public int TotalFindings { get; set; }
    public int CriticalFindings { get; set; }
    public int MajorFindings { get; set; }
    public int MinorFindings { get; set; }
    public int Observations { get; set; }
    
    // Performance Metrics
    public decimal AverageAuditDuration { get; set; }
    public decimal OnTimeCompletionRate { get; set; }
    public decimal FindingClosureRate { get; set; }
    
    // Breakdowns
    public Dictionary<string, int> AuditsByType { get; set; } = new();
    public Dictionary<string, int> AuditsByDepartment { get; set; } = new();
    public Dictionary<string, int> AuditsByAuditor { get; set; } = new();
    public Dictionary<string, int> FindingsByType { get; set; } = new();
    
    // Recent Activity
    public List<SMSAuditActivitySummary> RecentActivities { get; set; } = new();
    public List<SMSAuditOverdueItem> OverdueItems { get; set; } = new();
    public List<SMSAuditUpcomingItem> UpcomingAudits { get; set; } = new();
}

/// <summary>
/// SMS Audit Activity Summary for dashboard
/// </summary>
public class SMSAuditActivitySummary
{
    public string ActivityType { get; set; } = string.Empty; // Started, Completed, Finding Added, etc.
    public string AuditCode { get; set; } = string.Empty;
    public string AuditName { get; set; } = string.Empty;
    public string ActivityDescription { get; set; } = string.Empty;
    public DateTime ActivityDate { get; set; }
    public string ActivityBy { get; set; } = string.Empty;
}

/// <summary>
/// SMS Audit Overdue Item for dashboard alerts
/// </summary>
public class SMSAuditOverdueItem
{
    public string AuditCode { get; set; } = string.Empty;
    public string AuditName { get; set; } = string.Empty;
    public string ItemType { get; set; } = string.Empty; // Audit, Finding, Action
    public string Description { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public int DaysOverdue { get; set; }
    public string ResponsiblePerson { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
}

/// <summary>
/// SMS Audit Upcoming Item for dashboard
/// </summary>
public class SMSAuditUpcomingItem
{
    public string AuditCode { get; set; } = string.Empty;
    public string AuditName { get; set; } = string.Empty;
    public DateTime ScheduledDate { get; set; }
    public string LeadAuditor { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public int DaysUntilDue { get; set; }
    public string PreparationStatus { get; set; } = string.Empty;
}

/// <summary>
/// SMS Audit Finding Statistics for dashboard
/// </summary>
public class SMSAuditFindingStatistics
{
    public int TotalFindings { get; set; }
    public int OpenFindings { get; set; }
    public int InProgressFindings { get; set; }
    public int ClosedFindings { get; set; }
    public int VerifiedFindings { get; set; }
    public int OverdueFindings { get; set; }
    
    public Dictionary<string, int> FindingsBySeverity { get; set; } = new();
    public Dictionary<string, int> FindingsByType { get; set; } = new();
    public Dictionary<string, int> FindingsByDepartment { get; set; } = new();
    
    public decimal AverageResolutionDays { get; set; }
    public decimal FindingClosureRate { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// SMS Audit Evidence Statistics for dashboard
/// </summary>
public class SMSAuditEvidenceStatistics
{
    public int TotalEvidence { get; set; }
    public int VerifiedEvidence { get; set; }
    public int UnverifiedEvidence { get; set; }
    public int ArchivedEvidence { get; set; }
    
    public Dictionary<string, int> EvidenceByType { get; set; } = new();
    public Dictionary<string, int> EvidenceByAudit { get; set; } = new();
    public Dictionary<string, int> EvidenceByCollector { get; set; } = new();
    
    public long TotalFileSize { get; set; }
    public decimal AverageEvidencePerAudit { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}

// GET SMS AUDIT FINDINGS BY AUDIT CODE
public class GetSMSAuditFindingsByAuditCodeQuery : BaseQueryBundle, IRequest<Result<List<SMSAuditFinding>>>
{
    public string AuditCode { get; set; }
    public string? StatusFilter { get; set; }
    public string? SeverityFilter { get; set; }

    public GetSMSAuditFindingsByAuditCodeQuery(string auditCode, string? statusFilter = null, string? severityFilter = null)
    {
        AuditCode = auditCode ?? throw new ArgumentNullException(nameof(auditCode));
        StatusFilter = statusFilter;
        SeverityFilter = severityFilter;
    }
}

// GET SMS AUDIT EVIDENCE BY AUDIT CODE
public class GetSMSAuditEvidenceByAuditCodeQuery : BaseQueryBundle, IRequest<Result<List<SMSAuditEvidence>>>
{
    public string AuditCode { get; set; }
    public string? EvidenceTypeFilter { get; set; }
    public bool IncludeArchived { get; set; }

    public GetSMSAuditEvidenceByAuditCodeQuery(string auditCode, string? evidenceTypeFilter = null, bool includeArchived = false)
    {
        AuditCode = auditCode ?? throw new ArgumentNullException(nameof(auditCode));
        EvidenceTypeFilter = evidenceTypeFilter;
        IncludeArchived = includeArchived;
    }
}

// GET SMS AUDIT CHECKLIST ITEMS BY AUDIT CODE
public class GetSMSAuditChecklistItemsByAuditCodeQuery : BaseQueryBundle, IRequest<Result<List<SMSAuditChecklistItem>>>
{
    public string AuditCode { get; set; }
    public string? StatusFilter { get; set; }
    public string? CategoryFilter { get; set; }
    public bool OnlyRequired { get; set; }

    public GetSMSAuditChecklistItemsByAuditCodeQuery(string auditCode, string? statusFilter = null, string? categoryFilter = null, bool onlyRequired = false)
    {
        AuditCode = auditCode ?? throw new ArgumentNullException(nameof(auditCode));
        StatusFilter = statusFilter;
        CategoryFilter = categoryFilter;
        OnlyRequired = onlyRequired;
    }
}