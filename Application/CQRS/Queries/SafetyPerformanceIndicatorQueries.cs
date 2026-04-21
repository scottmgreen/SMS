//-----------------------------------------------------------------------
// <copyright file="SafetyPerformanceIndicatorQueries.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Query definitions for read operations in the SMS CQRS architecture.
//                  Defines query objects for read operations in the CQRS pattern.
//                  Queries retrieve data without causing side effects.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;

namespace SMS_Application.Messaging.Queries;

// =============================================
// SPI BASIC QUERIES WITH AUDIT TRACKING
// =============================================

public class GetAllSafetyPerformanceIndicatorsQuery : BaseEventBundle, IRequest<Result<List<SafetyPerformanceIndicator>>>, IReadQuery
{
    public string? StatusFilter { get; set; }
    public string? TypeFilter { get; set; }
    public string? DepartmentFilter { get; set; }
    public bool IncludeDataPoints { get; set; } = true;
    
    // Audit properties for query tracking
    public string AccessedBy { get; private set; } = string.Empty;
    public DateTime? AccessedDate { get; private set; }

    public GetAllSafetyPerformanceIndicatorsQuery(string? statusFilter = null, string? typeFilter = null,
        string? departmentFilter = null, bool includeDataPoints = true)
    {
        StatusFilter = statusFilter;
        TypeFilter = typeFilter;
        DepartmentFilter = departmentFilter;
        IncludeDataPoints = includeDataPoints;
    }

    // IReadQuery implementation
    public void SetAccessedBy(string userId, DateTime timestamp)
    {
        AccessedBy = userId;
        AccessedDate = timestamp;
    }

    public string GetResourceIdentifier()
    {
        return "SPI:All" + (StatusFilter != null ? $":Status:{StatusFilter}" : "") + 
               (TypeFilter != null ? $":Type:{TypeFilter}" : "") + 
               (DepartmentFilter != null ? $":Dept:{DepartmentFilter}" : "");
    }

    public string GetAccessType()
    {
        return this.GetType().Name.Replace("Query", ""); // GetAllSafetyPerformanceIndicators
    }
}

public class GetSafetyPerformanceIndicatorByIdQuery : BaseEventBundle, IRequest<Result<SafetyPerformanceIndicator>>, IReadQuery
{
    public SafetyPerformanceIndicatorID SPIId { get; set; }
    public bool IncludeDataPoints { get; set; } = true;
    
    // Audit properties for query tracking
    public string AccessedBy { get; private set; } = string.Empty;
    public DateTime? AccessedDate { get; private set; }

    public GetSafetyPerformanceIndicatorByIdQuery(SafetyPerformanceIndicatorID spiId, bool includeDataPoints = true)
    {
        SPIId = spiId ?? throw new ArgumentNullException(nameof(spiId));
        IncludeDataPoints = includeDataPoints;
    }

    // IReadQuery implementation
    public void SetAccessedBy(string userId, DateTime timestamp)
    {
        AccessedBy = userId;
        AccessedDate = timestamp;
    }

    public string GetResourceIdentifier()
    {
        return $"SPI:{SPIId?.Value ?? "Unknown"}";
    }

    public string GetAccessType()
    {
        return this.GetType().Name.Replace("Query", ""); // GetSafetyPerformanceIndicatorById
    }
}

public class GetSafetyPerformanceIndicatorByCodeQuery : BaseEventBundle, IRequest<Result<SafetyPerformanceIndicator>>, IReadQuery
{
    public string Code { get; set; }
    public bool IncludeDataPoints { get; set; } = true;
    
    // Audit properties for query tracking
    public string AccessedBy { get; private set; } = string.Empty;
    public DateTime? AccessedDate { get; private set; }

    public GetSafetyPerformanceIndicatorByCodeQuery(string code, bool includeDataPoints = true)
    {
        Code = code ?? throw new ArgumentNullException(nameof(code));
        IncludeDataPoints = includeDataPoints;
    }

    // IReadQuery implementation
    public void SetAccessedBy(string userId, DateTime timestamp)
    {
        AccessedBy = userId;
        AccessedDate = timestamp;
    }

    public string GetResourceIdentifier()
    {
        return $"SPI:Code:{Code}";
    }

    public string GetAccessType()
    {
        return this.GetType().Name.Replace("Query", ""); // GetSafetyPerformanceIndicatorByCode
    }
}

// =============================================
// SPI DASHBOARD QUERIES WITH AUDIT TRACKING
// =============================================

public class GetSPIDashboardDataQuery : BaseEventBundle, IRequest<Result<SPIDashboard>>, IReadQuery
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public List<string>? SPIIds { get; set; }
    public List<string>? DepartmentFilters { get; set; }
    public List<string>? TypeFilters { get; set; }
    public bool IncludeTrends { get; set; } = true;
    public bool IncludeAlerts { get; set; } = true;
    
    // Audit properties for query tracking
    public string AccessedBy { get; private set; } = string.Empty;
    public DateTime? AccessedDate { get; private set; }

    public GetSPIDashboardDataQuery(DateTime? startDate = null, DateTime? endDate = null,
        List<string>? spiIds = null, List<string>? departmentFilters = null,
        List<string>? typeFilters = null, bool includeTrends = true, bool includeAlerts = true)
    {
        StartDate = startDate;
        EndDate = endDate;
        SPIIds = spiIds;
        DepartmentFilters = departmentFilters;
        TypeFilters = typeFilters;
        IncludeTrends = includeTrends;
        IncludeAlerts = includeAlerts;
    }

    // IReadQuery implementation
    public void SetAccessedBy(string userId, DateTime timestamp)
    {
        AccessedBy = userId;
        AccessedDate = timestamp;
    }

    public string GetResourceIdentifier()
    {
        return "SPI:Dashboard" + 
               (StartDate.HasValue ? $":From:{StartDate:yyyy-MM-dd}" : "") + 
               (EndDate.HasValue ? $":To:{EndDate:yyyy-MM-dd}" : "");
    }

    public string GetAccessType()
    {
        return this.GetType().Name.Replace("Query", ""); // GetSPIDashboardData
    }
}

public class GetSPITrendAnalysisQuery : BaseEventBundle, IRequest<Result<List<SPITrendAnalysis>>>, IReadQuery
{
    public List<string>? SPIIds { get; set; }
    public int Periods { get; set; } = 12;
    public DateTime? EndDate { get; set; }
    
    // Audit properties for query tracking
    public string AccessedBy { get; private set; } = string.Empty;
    public DateTime? AccessedDate { get; private set; }

    public GetSPITrendAnalysisQuery(List<string>? spiIds = null, int periods = 12, DateTime? endDate = null)
    {
        SPIIds = spiIds;
        Periods = periods;
        EndDate = endDate ?? DateTime.UtcNow;
    }

    // IReadQuery implementation
    public void SetAccessedBy(string userId, DateTime timestamp)
    {
        AccessedBy = userId;
        AccessedDate = timestamp;
    }

    public string GetResourceIdentifier()
    {
        return "SPI:TrendAnalysis";
    }

    public string GetAccessType()
    {
        return this.GetType().Name.Replace("Query", ""); // GetSPITrendAnalysis
    }
}

public class GetSPIPerformanceSummaryQuery : BaseEventBundle, IRequest<Result<SPIPerformanceSummary>>, IReadQuery
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<string>? DepartmentFilters { get; set; }
    public List<string>? TypeFilters { get; set; }
    
    // Audit properties for query tracking
    public string AccessedBy { get; private set; } = string.Empty;
    public DateTime? AccessedDate { get; private set; }

    public GetSPIPerformanceSummaryQuery(DateTime startDate, DateTime endDate,
        List<string>? departmentFilters = null, List<string>? typeFilters = null)
    {
        StartDate = startDate;
        EndDate = endDate;
        DepartmentFilters = departmentFilters;
        TypeFilters = typeFilters;
    }

    // IReadQuery implementation
    public void SetAccessedBy(string userId, DateTime timestamp)
    {
        AccessedBy = userId;
        AccessedDate = timestamp;
    }

    public string GetResourceIdentifier()
    {
        return $"SPI:PerformanceSummary:{StartDate:yyyy-MM-dd}:{EndDate:yyyy-MM-dd}";
    }

    public string GetAccessType()
    {
        return this.GetType().Name.Replace("Query", ""); // GetSPIPerformanceSummary
    }
}

public class GetSPIDataPointsBySPICodeQuery : BaseEventBundle, IRequest<Result<List<SPIDataPoint>>>, IReadQuery
{
    public string SPICode { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int? MaxResults { get; set; }

    // Audit properties for query tracking
    public string AccessedBy { get; private set; } = string.Empty;
    public DateTime? AccessedDate { get; private set; }

    public GetSPIDataPointsBySPICodeQuery(string spiCode)
    {
        SPICode = spiCode ?? throw new ArgumentNullException(nameof(spiCode));
    }

    // IReadQuery implementation
    public void SetAccessedBy(string userId, DateTime timestamp)
    {
        AccessedBy = userId;
        AccessedDate = timestamp;
    }

    public string GetResourceIdentifier()
    {
        return $"SPI:DataPoints:{SPICode}";
    }

    public string GetAccessType()
    {
        return this.GetType().Name.Replace("Query", ""); // GetSPIDataPointsBySPICode
    }
}

public class GetSPIAlertsQuery : BaseEventBundle, IRequest<Result<List<SPIAlert>>>, IReadQuery
{
    public List<string>? SPIIds { get; set; }
    public bool ActiveAlertsOnly { get; set; } = true;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    
    // Audit properties for query tracking
    public string AccessedBy { get; private set; } = string.Empty;
    public DateTime? AccessedDate { get; private set; }

    public GetSPIAlertsQuery(List<string>? spiIds = null, bool activeAlertsOnly = true,
        DateTime? startDate = null, DateTime? endDate = null)
    {
        SPIIds = spiIds;
        ActiveAlertsOnly = activeAlertsOnly;
        StartDate = startDate;
        EndDate = endDate;
    }

    // IReadQuery implementation
    public void SetAccessedBy(string userId, DateTime timestamp)
    {
        AccessedBy = userId;
        AccessedDate = timestamp;
    }

    public string GetResourceIdentifier()
    {
        return "SPI:Alerts";
    }

    public string GetAccessType()
    {
        return this.GetType().Name.Replace("Query", ""); // GetSPIAlerts
    }
}

public class GetSPIDataPointsQuery : BaseEventBundle, IRequest<Result<List<SPIDataPoint>>>, IReadQuery
{
    public string SPIId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int? MaxRecords { get; set; }
    
    // Audit properties for query tracking
    public string AccessedBy { get; private set; } = string.Empty;
    public DateTime? AccessedDate { get; private set; }

    public GetSPIDataPointsQuery(string spiId, DateTime? startDate = null, DateTime? endDate = null, int? maxRecords = null)
    {
        SPIId = spiId ?? throw new ArgumentNullException(nameof(spiId));
        StartDate = startDate;
        EndDate = endDate;
        MaxRecords = maxRecords;
    }

    // IReadQuery implementation
    public void SetAccessedBy(string userId, DateTime timestamp)
    {
        AccessedBy = userId;
        AccessedDate = timestamp;
    }

    public string GetResourceIdentifier()
    {
        return $"SPI:DataPoints:{SPIId}";
    }

    public string GetAccessType()
    {
        return this.GetType().Name.Replace("Query", ""); // GetSPIDataPoints
    }
}

public class GetSPIComplianceStatusQuery : BaseEventBundle, IRequest<Result<List<SPIComplianceStatus>>>, IReadQuery
{
    public List<string>? SPIIds { get; set; }
    public DateTime? AsOfDate { get; set; }
    
    // Audit properties for query tracking
    public string AccessedBy { get; private set; } = string.Empty;
    public DateTime? AccessedDate { get; private set; }

    public GetSPIComplianceStatusQuery(List<string>? spiIds = null, DateTime? asOfDate = null)
    {
        SPIIds = spiIds;
        AsOfDate = asOfDate ?? DateTime.UtcNow;
    }

    // IReadQuery implementation
    public void SetAccessedBy(string userId, DateTime timestamp)
    {
        AccessedBy = userId;
        AccessedDate = timestamp;
    }

    public string GetResourceIdentifier()
    {
        return "SPI:ComplianceStatus";
    }

    public string GetAccessType()
    {
        return this.GetType().Name.Replace("Query", ""); // GetSPIComplianceStatus
    }
}

public class GetSPIReviewScheduleQuery : BaseEventBundle, IRequest<Result<List<SPIReviewItem>>>, IReadQuery
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool OverdueOnly { get; set; } = false;
    
    // Audit properties for query tracking
    public string AccessedBy { get; private set; } = string.Empty;
    public DateTime? AccessedDate { get; private set; }

    public GetSPIReviewScheduleQuery(DateTime? startDate = null, DateTime? endDate = null, bool overdueOnly = false)
    {
        StartDate = startDate;
        EndDate = endDate;
        OverdueOnly = overdueOnly;
    }

    // IReadQuery implementation
    public void SetAccessedBy(string userId, DateTime timestamp)
    {
        AccessedBy = userId;
        AccessedDate = timestamp;
    }

    public string GetResourceIdentifier()
    {
        return "SPI:ReviewSchedule";
    }

    public string GetAccessType()
    {
        return this.GetType().Name.Replace("Query", ""); // GetSPIReviewSchedule
    }
}

// =============================================
// END OF QUERY DEFINITIONS
// =============================================
//
// Note: SPIComplianceStatus and SPIReviewItem classes have been moved to 
// Domain\Entities\SPIManagementSupport\ following Domain-Driven Design principles
// These are domain entities, not DTOs, and belong in the domain layer
//
