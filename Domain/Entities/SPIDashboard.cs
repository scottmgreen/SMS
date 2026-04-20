//-----------------------------------------------------------------------
// <copyright file="SPIDashboard.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Domain entities for Safety Performance Indicator dashboard components
//                  including dashboard aggregation, performance summaries, alerts, and trends.
//                  Following Domain-Driven Design principles for SMS performance monitoring.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Domain.Entities;

/// <summary>
/// Complete dashboard data aggregate for Safety Performance Indicators
/// Domain entity representing comprehensive SPI performance monitoring data
/// </summary>
public class SPIDashboard
{
    public int TotalSPIs { get; set; }
    public int ActiveSPIs { get; set; }
    public List<SPIDashboardCard> SPICards { get; set; } = new();
    public SPIPerformanceSummary PerformanceSummary { get; set; } = new();
    public List<SPIAlert> ActiveAlerts { get; set; } = new();
    public List<SPITrendAnalysis> TrendAnalysis { get; set; } = new();
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    // UI Compatibility Properties
    public int SPIsRequiringReview => SPICards.Count(c => c.RequiresReview);

    public SPIDashboard()
    {
        SPICards = new List<SPIDashboardCard>();
        PerformanceSummary = new SPIPerformanceSummary();
        ActiveAlerts = new List<SPIAlert>();
        TrendAnalysis = new List<SPITrendAnalysis>();
    }

    /// <summary>
    /// Calculate overall system health score based on SPI performance
    /// </summary>
    public decimal CalculateSystemHealthScore()
    {
        if (!SPICards.Any()) return 0m;

        var performanceWeights = SPICards
            .Where(c => !string.IsNullOrEmpty(c.PerformanceStatus))
            .GroupBy(c => c.PerformanceStatus)
            .ToDictionary(g => g.Key, g => g.Count());

        var totalCards = performanceWeights.Values.Sum();
        if (totalCards == 0) return 0m;

        var score = (performanceWeights.GetValueOrDefault("Meeting", 0) * 100m +
                    performanceWeights.GetValueOrDefault("Warning", 0) * 60m +
                    performanceWeights.GetValueOrDefault("Critical", 0) * 20m) / totalCards;

        return Math.Round(score, 1);
    }

    /// <summary>
    /// Get SPIs requiring immediate attention
    /// </summary>
    public List<SPIDashboardCard> GetSPIsRequiringAttention()
    {
        return SPICards
            .Where(c => c.PerformanceStatus == "Critical" || c.HasActiveAlerts)
            .OrderByDescending(c => c.AlertCount)
            .ThenBy(c => c.LastMeasurementDate)
            .ToList();
    }
}

/// <summary>
/// Individual SPI card representation for dashboard display
/// Domain value object containing SPI performance and status information
/// </summary>
public class SPIDashboardCard
{
    // Core SPI Information
    public string SPIId { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty; // Added for UI compatibility
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // Leading, Lagging, Process, Compliance
    public string IndicatorType { get; set; } = string.Empty; // Added for UI compatibility (same as Type)
    public string Category { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // Active, Inactive, Under Review

    // Performance Metrics
    public decimal? CurrentValue { get; set; }
    public decimal? TargetValue { get; set; }
    public decimal? WarningThreshold { get; set; }
    public decimal? CriticalThreshold { get; set; }
    public string MeasurementUnit { get; set; } = string.Empty;

    // Status Information
    public string PerformanceStatus { get; set; } = string.Empty; // Meeting, Warning, Critical
    public DateTime? LastMeasurementDate { get; set; }
    public string ResponsibleDepartment { get; set; } = string.Empty;
    public string DataOwner { get; set; } = string.Empty;

    // Trend Information
    public string TrendDirection { get; set; } = string.Empty; // Improving, Stable, Declining
    public decimal? TrendPercentage { get; set; }
    public int DataPointCount { get; set; }

    // Alert Information
    public bool HasActiveAlerts { get; set; }
    public int AlertCount { get; set; }

    // UI Compatibility Properties (computed properties for legacy UI)
    public bool IsOverThreshold => PerformanceStatus == "Critical";
    public bool IsAtWarningLevel => PerformanceStatus == "Warning"; 
    public bool RequiresReview => GetDaysSinceLastMeasurement() > 30 || IsOverThreshold;

    /// <summary>
    /// Calculate performance percentage compared to target
    /// </summary>
    public decimal CalculatePerformancePercentage()
    {
        if (!CurrentValue.HasValue || !TargetValue.HasValue || TargetValue.Value == 0)
            return 0m;

        return Math.Round((CurrentValue.Value / TargetValue.Value) * 100m, 1);
    }

    /// <summary>
    /// Determine if SPI is within acceptable performance range
    /// </summary>
    public bool IsPerformanceAcceptable()
    {
        return PerformanceStatus == "Meeting";
    }

    /// <summary>
    /// Get days since last measurement
    /// </summary>
    public int GetDaysSinceLastMeasurement()
    {
        if (!LastMeasurementDate.HasValue) return int.MaxValue;
        return (DateTime.UtcNow - LastMeasurementDate.Value).Days;
    }
}

/// <summary>
/// Performance summary aggregate for dashboard overview
/// Domain value object providing high-level SPI performance metrics
/// </summary>
public class SPIPerformanceSummary
{
    public int SPIsMeetingTarget { get; set; }
    public int SPIsInWarning { get; set; }
    public int SPIsInCritical { get; set; }
    public decimal OverallComplianceRate { get; set; }
    public Dictionary<string, int> SPIsByType { get; set; } = new();
    public Dictionary<string, int> SPIsByDepartment { get; set; } = new();
    public List<string> TopPerformingDepartments { get; set; } = new();
    public List<string> DepartmentsNeedingAttention { get; set; } = new();

    /// <summary>
    /// Calculate total number of SPIs being tracked
    /// </summary>
    public int GetTotalSPICount()
    {
        return SPIsMeetingTarget + SPIsInWarning + SPIsInCritical;
    }

    /// <summary>
    /// Get compliance status description
    /// </summary>
    public string GetComplianceStatusDescription()
    {
        return OverallComplianceRate switch
        {
            >= 95m => "Excellent",
            >= 80m => "Good", 
            >= 65m => "Fair",
            >= 50m => "Poor",
            _ => "Critical"
        };
    }
}

/// <summary>
/// SPI alert domain entity for performance threshold monitoring
/// Domain entity representing automated SPI performance alerts
/// </summary>
public class SPIAlert
{
    public string SPIId { get; set; } = string.Empty;
    public string SPIName { get; set; } = string.Empty;
    public string AlertType { get; set; } = string.Empty; // Warning, Critical, Threshold Exceeded
    public string AlertMessage { get; set; } = string.Empty;
    public DateTime AlertDate { get; set; }
    public decimal CurrentValue { get; set; }
    public decimal ThresholdValue { get; set; }
    public string Severity { get; set; } = string.Empty; // Low, Medium, High, Critical
    public string TrendDirection { get; set; } = string.Empty; // UI compatibility
    public bool IsAcknowledged { get; set; }
    public string? AcknowledgedBy { get; set; }
    public DateTime? AcknowledgedDate { get; set; }

    /// <summary>
    /// Acknowledge the alert
    /// </summary>
    public void Acknowledge(string acknowledgedBy)
    {
        IsAcknowledged = true;
        AcknowledgedBy = acknowledgedBy;
        AcknowledgedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Determine if alert is overdue for acknowledgment
    /// </summary>
    public bool IsOverdueForAcknowledgment()
    {
        if (IsAcknowledged) return false;

        var hoursOld = (DateTime.UtcNow - AlertDate).TotalHours;

        return Severity.ToUpper() switch
        {
            "CRITICAL" => hoursOld > 1,  // Critical alerts need acknowledgment within 1 hour
            "HIGH" => hoursOld > 4,      // High alerts within 4 hours
            "MEDIUM" => hoursOld > 24,   // Medium alerts within 24 hours
            _ => hoursOld > 72           // Low alerts within 72 hours
        };
    }
}

/// <summary>
/// SPI trend analysis domain value object
/// Domain entity providing trend analysis and forecasting for SPIs
/// </summary>
public class SPITrendAnalysis
{
    public string SPIId { get; set; } = string.Empty;
    public string SPIName { get; set; } = string.Empty;
    public List<SPIDataPointSummary> DataPoints { get; set; } = new();
    public string TrendDirection { get; set; } = string.Empty; // Improving, Stable, Declining
    public decimal TrendSlope { get; set; }
    public decimal? ProjectedValue { get; set; }
    public string TrendAnalysisPeriod { get; set; } = string.Empty;
    public decimal TrendConfidence { get; set; } // 0-100 confidence in trend analysis

    /// <summary>
    /// Calculate trend strength (how strong the trend is)
    /// </summary>
    public decimal CalculateTrendStrength()
    {
        if (DataPoints.Count < 2) return 0m;

        var values = DataPoints.OrderBy(d => d.MeasurementDate).Select(d => d.Value).ToList();
        var mean = values.Average();
        var variance = values.Sum(v => Math.Pow((double)(v - mean), 2)) / values.Count;
        var stdDev = (decimal)Math.Sqrt(variance);

        return stdDev == 0 ? 100m : Math.Min(100m, Math.Abs(TrendSlope) / stdDev * 100m);
    }

    /// <summary>
    /// Predict future value based on current trend
    /// </summary>
    public decimal? PredictFutureValue(int periodsAhead)
    {
        if (DataPoints.Count < 2 || TrendConfidence < 50m) return null;

        var latestValue = DataPoints.OrderByDescending(d => d.MeasurementDate).First().Value;
        return latestValue + (TrendSlope * periodsAhead);
    }
}

/// <summary>
/// Individual data point summary for trending and analysis
/// Domain value object representing a single SPI measurement point
/// </summary>
public class SPIDataPointSummary
{
    public string Period { get; set; } = string.Empty; // "2024-01-15", "2024-Q1", etc.
    public DateTime MeasurementDate { get; set; }
    public decimal Value { get; set; }
    public decimal? Target { get; set; }
    public string DataSource { get; set; } = string.Empty;
    public bool IsVerified { get; set; }
    public string Notes { get; set; } = string.Empty;
    public string? VerifiedBy { get; set; }
    public DateTime? VerifiedDate { get; set; }

    /// <summary>
    /// Calculate variance from target
    /// </summary>
    public decimal? CalculateVarianceFromTarget()
    {
        if (!Target.HasValue) return null;
        return Value - Target.Value;
    }

    /// <summary>
    /// Calculate percentage variance from target
    /// </summary>
    public decimal? CalculatePercentageVarianceFromTarget()
    {
        if (!Target.HasValue || Target.Value == 0) return null;
        return Math.Round(((Value - Target.Value) / Target.Value) * 100m, 1);
    }

    /// <summary>
    /// Determine if measurement meets target
    /// </summary>
    public bool MeetsTarget()
    {
        return Target.HasValue && Math.Abs(Value - Target.Value) <= (Target.Value * 0.05m); // 5% tolerance
    }
}