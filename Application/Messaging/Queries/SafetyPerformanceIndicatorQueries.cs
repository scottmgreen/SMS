//-----------------------------------------------------------------------
// <copyright file="SafetyPerformanceIndicatorQueries.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Query definitions for read operations in the SMS CQRS architecture.
//                  Defines query objects for read operations in the CQRS pattern.
//                  Queries retrieve data without causing side effects.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Application.Messaging.Queries;

// =============================================
// SPI BASIC QUERIES
// =============================================

public class GetAllSafetyPerformanceIndicatorsQuery : BaseEventBundle, IRequest<Result<List<SafetyPerformanceIndicator>>>
{
    public string? StatusFilter { get; set; }
    public string? TypeFilter { get; set; }
    public string? DepartmentFilter { get; set; }
    public bool IncludeDataPoints { get; set; } = true;

    public GetAllSafetyPerformanceIndicatorsQuery(string? statusFilter = null, string? typeFilter = null,
        string? departmentFilter = null, bool includeDataPoints = true)
    {
        StatusFilter = statusFilter;
        TypeFilter = typeFilter;
        DepartmentFilter = departmentFilter;
        IncludeDataPoints = includeDataPoints;
    }
}

public class GetSafetyPerformanceIndicatorByIdQuery : BaseEventBundle, IRequest<Result<SafetyPerformanceIndicator>>
{
    public SafetyPerformanceIndicatorID SPIId { get; set; }
    public bool IncludeDataPoints { get; set; } = true;

    public GetSafetyPerformanceIndicatorByIdQuery(SafetyPerformanceIndicatorID spiId, bool includeDataPoints = true)
    {
        SPIId = spiId ?? throw new ArgumentNullException(nameof(spiId));
        IncludeDataPoints = includeDataPoints;
    }
}

public class GetSafetyPerformanceIndicatorByCodeQuery : BaseEventBundle, IRequest<Result<SafetyPerformanceIndicator>>
{
    public string Code { get; set; }
    public bool IncludeDataPoints { get; set; } = true;

    public GetSafetyPerformanceIndicatorByCodeQuery(string code, bool includeDataPoints = true)
    {
        Code = code ?? throw new ArgumentNullException(nameof(code));
        IncludeDataPoints = includeDataPoints;
    }
}





// =============================================
// SPI DASHBOARD QUERIES
// =============================================

public class GetSPIDashboardDataQuery : BaseEventBundle, IRequest<Result<SPIDashboardData>>
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public List<string>? SPIIds { get; set; }
    public List<string>? DepartmentFilters { get; set; }
    public List<string>? TypeFilters { get; set; }
    public bool IncludeTrends { get; set; } = true;
    public bool IncludeAlerts { get; set; } = true;

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
}

public class GetSPITrendAnalysisQuery : BaseEventBundle, IRequest<Result<List<SPITrendAnalysis>>>
{
    public List<string>? SPIIds { get; set; }
    public int Periods { get; set; } = 12;
    public DateTime? EndDate { get; set; }

    public GetSPITrendAnalysisQuery(List<string>? spiIds = null, int periods = 12, DateTime? endDate = null)
    {
        SPIIds = spiIds;
        Periods = periods;
        EndDate = endDate ?? DateTime.UtcNow;
    }
}

public class GetSPIPerformanceSummaryQuery : BaseEventBundle, IRequest<Result<SPIPerformanceSummary>>
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<string>? DepartmentFilters { get; set; }
    public List<string>? TypeFilters { get; set; }

    public GetSPIPerformanceSummaryQuery(DateTime startDate, DateTime endDate,
        List<string>? departmentFilters = null, List<string>? typeFilters = null)
    {
        StartDate = startDate;
        EndDate = endDate;
        DepartmentFilters = departmentFilters;
        TypeFilters = typeFilters;
    }
}

public class GetSPIAlertsQuery : BaseEventBundle, IRequest<Result<List<SPIAlert>>>
{
    public List<string>? SPIIds { get; set; }
    public bool ActiveAlertsOnly { get; set; } = true;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    public GetSPIAlertsQuery(List<string>? spiIds = null, bool activeAlertsOnly = true,
        DateTime? startDate = null, DateTime? endDate = null)
    {
        SPIIds = spiIds;
        ActiveAlertsOnly = activeAlertsOnly;
        StartDate = startDate;
        EndDate = endDate;
    }
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
    public string TrendDirection { get; set; } = string.Empty; // Using string instead of enum for simplicity
}

// =============================================
// SPI DATA ANALYSIS QUERIES
// =============================================

public class GetSPIDataPointsQuery : BaseEventBundle, IRequest<Result<List<SPIDataPoint>>>
{
    public string SPIId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int? MaxRecords { get; set; }

    public GetSPIDataPointsQuery(string spiId, DateTime? startDate = null, DateTime? endDate = null, int? maxRecords = null)
    {
        SPIId = spiId ?? throw new ArgumentNullException(nameof(spiId));
        StartDate = startDate;
        EndDate = endDate;
        MaxRecords = maxRecords;
    }
}

public class GetSPIComplianceStatusQuery : BaseEventBundle, IRequest<Result<List<SPIComplianceStatus>>>
{
    public List<string>? SPIIds { get; set; }
    public DateTime? AsOfDate { get; set; }

    public GetSPIComplianceStatusQuery(List<string>? spiIds = null, DateTime? asOfDate = null)
    {
        SPIIds = spiIds;
        AsOfDate = asOfDate ?? DateTime.UtcNow;
    }
}

public class GetSPIReviewScheduleQuery : BaseEventBundle, IRequest<Result<List<SPIReviewItem>>>
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool OverdueOnly { get; set; } = false;

    public GetSPIReviewScheduleQuery(DateTime? startDate = null, DateTime? endDate = null, bool overdueOnly = false)
    {
        StartDate = startDate;
        EndDate = endDate;
        OverdueOnly = overdueOnly;
    }
}

// =============================================
// DATA TRANSFER OBJECTS
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
    public string IndicatorType { get; set; } = string.Empty; // Using string instead of SPIType
    public string Status { get; set; } = string.Empty; // Using string instead of SPIStatus
    public string MeasurementUnit { get; set; } = string.Empty;
    public string MeasurementFrequency { get; set; } = string.Empty; // Using string instead of SPIMeasurementFrequency

    // Current Values
    public decimal? CurrentValue { get; set; }
    public decimal? TargetValue { get; set; }
    public decimal? WarningThreshold { get; set; }
    public decimal? CriticalThreshold { get; set; }

    // Performance Indicators
    public string TrendDirection { get; set; } = string.Empty; // Using string instead of SPITrendDirection
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
/// SPI trend analysis over time
/// </summary>
public class SPITrendAnalysis
{
    public string SPIId { get; set; } = string.Empty;
    public string SPIName { get; set; } = string.Empty;
    public List<SPIDataPointSummary> DataPoints { get; set; } = new();
    public string OverallTrend { get; set; } = string.Empty; // Using string instead of SPITrendDirection
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

    // By HazardCategory
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
