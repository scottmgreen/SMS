//-----------------------------------------------------------------------
// <copyright file="SMSAuditQueries.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Query definitions for SMS audit data retrieval and reporting operations.
//                  Defines query objects for read operations in the CQRS pattern.
//                  Queries retrieve data without causing side effects.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;

namespace SMS_Application.Messaging.Queries;

/// <summary>
/// SMS Audit Queries for CQRS pattern
/// </summary>

// GET ALL SMS AUDITS - WITH AUDIT TRACKING
public class GetAllSMSAuditsQuery : BaseQueryBundle, IRequest<Result<List<SMSAudit>>>, IReadQuery
{
    public string? StatusFilter { get; set; }
    public string? AuditTypeFilter { get; set; }
    public string? DepartmentFilter { get; set; }
    public string? AuditorFilter { get; set; }
    public DateTime? StartDateFrom { get; set; }
    public DateTime? StartDateTo { get; set; }
    public bool IncludeFindings { get; set; }
    public bool IncludeEvidence { get; set; }

    // Audit properties for query tracking
    public string AccessedBy { get; private set; } = string.Empty;
    public DateTime? AccessedDate { get; private set; }

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

    // IReadQuery implementation
    public void SetAccessedBy(string userId, DateTime timestamp)
    {
        AccessedBy = userId;
        AccessedDate = timestamp;
    }

    public string GetResourceIdentifier()
    {
        return "SMSAudit:All" + 
               (StatusFilter != null ? $":Status:{StatusFilter}" : "") +
               (AuditTypeFilter != null ? $":Type:{AuditTypeFilter}" : "");
    }

    public string GetAccessType()
    {
        return "GetAll";
    }
}

// GET SMS AUDIT BY CODE - WITH AUDIT TRACKING
public class GetSMSAuditByCodeQuery : BaseQueryBundle, IRequest<Result<SMSAudit>>, IReadQuery
{
    public string AuditCode { get; set; }
    public bool IncludeFindings { get; set; }
    public bool IncludeEvidence { get; set; }

    // Audit properties for query tracking
    public string AccessedBy { get; private set; } = string.Empty;
    public DateTime? AccessedDate { get; private set; }

    public GetSMSAuditByCodeQuery(string auditCode, bool includeFindings = true, bool includeEvidence = true)
    {
        AuditCode = auditCode ?? throw new ArgumentNullException(nameof(auditCode));
        IncludeFindings = includeFindings;
        IncludeEvidence = includeEvidence;
    }

    // IReadQuery implementation
    public void SetAccessedBy(string userId, DateTime timestamp)
    {
        AccessedBy = userId;
        AccessedDate = timestamp;
    }

    public string GetResourceIdentifier()
    {
        return $"SMSAudit:Code:{AuditCode}";
    }

    public string GetAccessType()
    {
        return "GetByCode";
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

// GET ALL SMS AUDIT FINDINGS
public class GetAllSMSAuditFindingsQuery : BaseQueryBundle, IRequest<Result<List<SMSAuditFinding>>>
{
    public GetAllSMSAuditFindingsQuery()
    {
    }
}

// GET SMS AUDIT FINDING BY CODE
public class GetSMSAuditFindingByCodeQuery : BaseQueryBundle, IRequest<Result<SMSAuditFinding>>
{
    public string FindingCode { get; set; }

    public GetSMSAuditFindingByCodeQuery(string findingCode)
    {
        FindingCode = findingCode ?? throw new ArgumentNullException(nameof(findingCode));
    }
}

// GET OVERDUE SMS AUDIT FINDINGS
public class GetOverdueSMSAuditFindingsQuery : BaseQueryBundle, IRequest<Result<List<SMSAuditFinding>>>
{
    public GetOverdueSMSAuditFindingsQuery()
    {
    }
}

// GET ALL SMS AUDIT EVIDENCE
public class GetAllSMSAuditEvidenceQuery : BaseQueryBundle, IRequest<Result<List<SMSAuditEvidence>>>
{
    public GetAllSMSAuditEvidenceQuery()
    {
    }
}

// GET SMS AUDIT EVIDENCE BY CODE
public class GetSMSAuditEvidenceByCodeQuery : BaseQueryBundle, IRequest<Result<SMSAuditEvidence>>
{
    public string EvidenceCode { get; set; }

    public GetSMSAuditEvidenceByCodeQuery(string evidenceCode)
    {
        EvidenceCode = evidenceCode ?? throw new ArgumentNullException(nameof(evidenceCode));
    }
}

// GET SMS AUDIT EVIDENCE BY FINDING CODE
public class GetSMSAuditEvidenceByFindingCodeQuery : BaseQueryBundle, IRequest<Result<List<SMSAuditEvidence>>>
{
    public string FindingCode { get; set; }
    public bool IncludeArchived { get; set; } = false;

    public GetSMSAuditEvidenceByFindingCodeQuery(string findingCode, bool includeArchived = false)
    {
        FindingCode = findingCode ?? throw new ArgumentNullException(nameof(findingCode));
        IncludeArchived = includeArchived;
    }
}
