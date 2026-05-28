//-----------------------------------------------------------------------
// <copyright file="SMSAuditPlanQueries.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Query definitions for SMS audit data retrieval and reporting operations.
//                  Defines query objects for read operations in the CQRS pattern.
//                  Queries retrieve data without causing side effects.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;

namespace SMS_Application.Queries;

/// <summary>
/// SMS Audit Plan Queries for CQRS pattern - WITH AUDIT TRACKING
/// </summary>

// GET ALL SMS AUDIT PLANS WITH AUDIT TRACKING
public class GetAllSMSAuditPlansQuery : BaseQueryBundle, IRequest<Result<List<SMSAuditPlan>>>, IReadQuery
{
    public string? StatusFilter { get; set; }
    public string? AuditTypeFilter { get; set; }
    public string? DepartmentFilter { get; set; }
    public DateTime? StartDateFrom { get; set; }
    public DateTime? StartDateTo { get; set; }

    // Audit properties for query tracking
    public string AccessedBy { get; private set; } = string.Empty;
    public DateTime? AccessedDate { get; private set; }

    public GetAllSMSAuditPlansQuery(string? statusFilter = null, string? auditTypeFilter = null,
        string? departmentFilter = null, DateTime? startDateFrom = null, DateTime? startDateTo = null)
    {
        StatusFilter = statusFilter;
        AuditTypeFilter = auditTypeFilter;
        DepartmentFilter = departmentFilter;
        StartDateFrom = startDateFrom;
        StartDateTo = startDateTo;
    }

    // IReadQuery implementation
    public void SetAccessedBy(string userId, DateTime timestamp)
    {
        AccessedBy = userId;
        AccessedDate = timestamp;
    }

    public string GetResourceIdentifier()
    {
        return "SMSAuditPlan:All" + 
               (StatusFilter != null ? $":Status:{StatusFilter}" : "") +
               (AuditTypeFilter != null ? $":Type:{AuditTypeFilter}" : "");
    }

    public string GetAccessType()
    {
        return "GetAll";
    }
}

// GET SMS AUDIT PLAN BY CODE WITH AUDIT TRACKING
public class GetSMSAuditPlanByCodeQuery : BaseQueryBundle, IRequest<Result<SMSAuditPlan>>, IReadQuery
{
    public string AuditPlanCode { get; set; }
    public bool IncludeAudits { get; set; }

    // Audit properties for query tracking
    public string AccessedBy { get; private set; } = string.Empty;
    public DateTime? AccessedDate { get; private set; }

    public GetSMSAuditPlanByCodeQuery(string auditPlanCode, bool includeAudits = false)
    {
        AuditPlanCode = auditPlanCode ?? throw new ArgumentNullException(nameof(auditPlanCode));
        IncludeAudits = includeAudits;
    }

    // IReadQuery implementation
    public void SetAccessedBy(string userId, DateTime timestamp)
    {
        AccessedBy = userId;
        AccessedDate = timestamp;
    }

    public string GetResourceIdentifier()
    {
        return $"SMSAuditPlan:Code:{AuditPlanCode}";
    }

    public string GetAccessType()
    {
        return "GetByCode";
    }
}

// GET SMS AUDIT PLANS BY TYPE WITH AUDIT TRACKING
public class GetSMSAuditPlansByTypeQuery : BaseQueryBundle, IRequest<Result<List<SMSAuditPlan>>>, IReadQuery
{
    public string AuditType { get; set; }
    public string? StatusFilter { get; set; }

    // Audit properties for query tracking
    public string AccessedBy { get; private set; } = string.Empty;
    public DateTime? AccessedDate { get; private set; }

    public GetSMSAuditPlansByTypeQuery(string auditType, string? statusFilter = null)
    {
        AuditType = auditType ?? throw new ArgumentNullException(nameof(auditType));
        StatusFilter = statusFilter;
    }

    // IReadQuery implementation
    public void SetAccessedBy(string userId, DateTime timestamp)
    {
        AccessedBy = userId;
        AccessedDate = timestamp;
    }

    public string GetResourceIdentifier()
    {
        return $"SMSAuditPlan:Type:{AuditType}" + (StatusFilter != null ? $":Status:{StatusFilter}" : "");
    }

    public string GetAccessType()
    {
        return "GetByType";
    }
}

// GET SMS AUDIT PLANS BY DEPARTMENT WITH AUDIT TRACKING
public class GetSMSAuditPlansByDepartmentQuery : BaseQueryBundle, IRequest<Result<List<SMSAuditPlan>>>, IReadQuery
{
    public string Department { get; set; }
    public string? StatusFilter { get; set; }

    // Audit properties for query tracking
    public string AccessedBy { get; private set; } = string.Empty;
    public DateTime? AccessedDate { get; private set; }

    public GetSMSAuditPlansByDepartmentQuery(string department, string? statusFilter = null)
    {
        Department = department ?? throw new ArgumentNullException(nameof(department));
        StatusFilter = statusFilter;
    }

    // IReadQuery implementation
    public void SetAccessedBy(string userId, DateTime timestamp)
    {
        AccessedBy = userId;
        AccessedDate = timestamp;
    }

    public string GetResourceIdentifier()
    {
        return $"SMSAuditPlan:Department:{Department}" + (StatusFilter != null ? $":Status:{StatusFilter}" : "");
    }

    public string GetAccessType()
    {
        return "GetByDepartment";
    }
}

// GET SMS AUDIT PLANS REQUIRING APPROVAL WITH AUDIT TRACKING
public class GetSMSAuditPlansRequiringApprovalQuery : BaseQueryBundle, IRequest<Result<List<SMSAuditPlan>>>, IReadQuery
{
    // Audit properties for query tracking
    public string AccessedBy { get; private set; } = string.Empty;
    public DateTime? AccessedDate { get; private set; }

    public GetSMSAuditPlansRequiringApprovalQuery()
    {
        // No parameters needed - gets all plans with status "Draft"
    }

    // IReadQuery implementation
    public void SetAccessedBy(string userId, DateTime timestamp)
    {
        AccessedBy = userId;
        AccessedDate = timestamp;
    }

    public string GetResourceIdentifier()
    {
        return "SMSAuditPlan:RequiringApproval";
    }

    public string GetAccessType()
    {
        return "GetRequiringApproval";
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
