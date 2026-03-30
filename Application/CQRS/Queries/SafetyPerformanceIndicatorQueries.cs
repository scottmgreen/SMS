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

public class GetSPIDashboardDataQuery : BaseEventBundle, IRequest<Result<SPIDashboardData>>, IReadQuery
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
// MISSING DTO CLASSES THAT WERE REMOVED - RESTORED
// =============================================

/// <summary>
/// Complete SPI Dashboard data aggregation
/// </summary>
public class SPIDashboardData
{
    public List<SPIDashboardCard> SPICards { get; set; } = new();
    public List<SPITrendAnalysis> TrendAnalysis { get; set; } = new();
    public List<SPIAlert> ActiveAlerts { get; set; } = new();
    public SPIPerformanceSummary PerformanceSummary { get; set; } = new();
    public DateTime LastUpdateDate { get; set; }
    public int TotalSPIs { get; set; }
    public int ActiveSPIs { get; set; }
    public int SPIsOverThreshold { get; set; }
    public int SPIsRequiringReview { get; set; }
}

/// <summary>
/// Individual SPI dashboard card data
/// </summary>
public class SPIDashboardCard
{
    public string SPIId { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string IndicatorType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string MeasurementUnit { get; set; } = string.Empty;
    public string MeasurementFrequency { get; set; } = string.Empty;

    // Current Values
    public decimal? CurrentValue { get; set; }
    public decimal? TargetValue { get; set; }
    public decimal? WarningThreshold { get; set; }
    public decimal? CriticalThreshold { get; set; }

    // Performance Indicators
    public string TrendDirection { get; set; } = string.Empty;
    public decimal? PercentageToTarget { get; set; }
    public bool IsOverThreshold { get; set; }
    public bool IsAtWarningLevel { get; set; }
    public bool RequiresReview { get; set; }

    // Timing
    public DateTime? LastMeasurementDate { get; set; }
    public DateTime? NextReviewDate { get; set; }

    // Ownership
    public string ResponsibleDepartment { get; set; } = string.Empty;
    public string DataOwner { get; set; } = string.Empty;
}

/// <summary>
/// SPI Alert for queries
/// </summary>
public class SPIAlert
{
    public string SPIId { get; set; } = string.Empty;
    public string SPIName { get; set; } = string.Empty;
    public string AlertType { get; set; } = string.Empty; // Warning, Critical, Target
    public decimal CurrentValue { get; set; }
    public decimal? ThresholdValue { get; set; }
    public string AlertMessage { get; set; } = string.Empty;
    public DateTime AlertDate { get; set; }
    public string TrendDirection { get; set; } = string.Empty;
}

/// <summary>
/// SPI trend analysis over time
/// </summary>
public class SPITrendAnalysis
{
    public string SPIId { get; set; } = string.Empty;
    public string SPIName { get; set; } = string.Empty;
    public List<SPIDataPointSummary> DataPoints { get; set; } = new();
    public string OverallTrend { get; set; } = string.Empty;
    public decimal? TrendSlope { get; set; }
    public decimal? VariabilityIndex { get; set; }
    public int ConsecutivePeriodsAboveTarget { get; set; }
    public int ConsecutivePeriodsBelowTarget { get; set; }
}

/// <summary>
/// Summarized data point for trending
/// </summary>
public class SPIDataPointSummary
{
    public string Period { get; set; } = string.Empty;
    public DateTime MeasurementDate { get; set; }
    public decimal Value { get; set; }
    public decimal? Target { get; set; }
    public bool IsAboveWarning { get; set; }
    public bool IsAboveCritical { get; set; }
}

/// <summary>
/// Performance summary across multiple SPIs
/// </summary>
public class SPIPerformanceSummary
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalSPIs { get; set; }
    public int SPIsMeetingTarget { get; set; }
    public int SPIsAboveWarning { get; set; }
    public int SPIsAboveCritical { get; set; }
    public decimal OverallComplianceRate { get; set; }

    // By Type and Department
    public Dictionary<string, int> SPIsByType { get; set; } = new();
    public Dictionary<string, int> SPIsByDepartment { get; set; } = new();
    public Dictionary<string, decimal> AverageValuesByType { get; set; } = new();

    // Trends
    public int SPIsImproving { get; set; }
    public int SPIsStable { get; set; }
    public int SPIsDeclining { get; set; }
}

/// <summary>
/// SPI compliance status
/// </summary>
public class SPIComplianceStatus
{
    public string SPIId { get; set; } = string.Empty;
    public string SPIName { get; set; } = string.Empty;
    public bool InCompliance { get; set; }
    public string ComplianceStatus { get; set; } = string.Empty; // Compliant, Warning, Critical, No Data
    public decimal? CurrentValue { get; set; }
    public decimal? ComplianceThreshold { get; set; }
    public DateTime? LastMeasurementDate { get; set; }
    public int DaysWithoutData { get; set; }
}

/// <summary>
/// SPI review schedule item
/// </summary>
public class SPIReviewItem
{
    public string SPIId { get; set; } = string.Empty;
    public string SPIName { get; set; } = string.Empty;
    public string ResponsibleDepartment { get; set; } = string.Empty;
    public string ReviewAuthority { get; set; } = string.Empty;
    public DateTime? NextReviewDate { get; set; }
    public DateTime? LastReviewDate { get; set; }
    public bool IsOverdue { get; set; }
    public int DaysOverdue { get; set; }
    public string Priority { get; set; } = string.Empty; // High, Medium, Low
}
