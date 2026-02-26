//-----------------------------------------------------------------------
// <copyright file="SafetyPerformanceIndicator.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: SMS safety performance indicator entity for measuring and tracking safety metrics.
//                  Domain entity implementing business rules and invariants
//                  following Domain-Driven Design principles.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Domain.Entities;

/// <summary>
/// Safety Performance Indicator entity for tracking safety metrics and performance targets
/// Implements 14 CFR § 139.402(a)(6) requirements for safety objective measurement
/// </summary>
public sealed class SafetyPerformanceIndicator : BaseAuditableEntity
{
    public SafetyPerformanceIndicator(SafetyPerformanceIndicatorID id, string name, string description,
        SPIType indicatorType, string createdBy)
        : base(id, createdBy, DateTime.UtcNow)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description ?? throw new ArgumentNullException(nameof(description));
        IndicatorType = indicatorType ?? throw new ArgumentNullException(nameof(indicatorType));
        Status = SPIStatus.Active;
        AlertThresholds = new List<SPIThreshold>();
        DataPoints = new List<SPIDataPoint>();
    }

    // Core Properties
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; }
    public string Description { get; set; }
    public SPIType IndicatorType { get; set; }
    public SPIStatus Status { get; set; }

    // Measurement Configuration
    public string MeasurementUnit { get; set; } = string.Empty;
    public SPIMeasurementFrequency MeasurementFrequency { get; set; } = SPIMeasurementFrequency.Monthly;
    public string CalculationMethod { get; set; } = string.Empty;
    public string DataSource { get; set; } = string.Empty;

    // Target Values
    public decimal? TargetValue { get; set; }
    public decimal? AcceptableRange { get; set; }
    public decimal? WarningThreshold { get; set; }
    public decimal? CriticalThreshold { get; set; }

    // Ownership
    public string ResponsibleDepartment { get; set; } = string.Empty;
    public string DataOwner { get; set; } = string.Empty;
    public string ReviewAuthority { get; set; } = string.Empty;

    // Review Schedule
    public DateTime? NextReviewDate { get; set; }
    public DateTime? LastReviewDate { get; set; }
    public string? LastReviewNotes { get; set; }

    // Alert Configuration
    public List<SPIThreshold> AlertThresholds { get; set; }
    public bool AlertsEnabled { get; set; } = true;
    public string? AlertRecipients { get; set; }

    // Data Points Collection
    public List<SPIDataPoint> DataPoints { get; set; }

    // Business Methods
    public Result UpdateConfiguration(string name, string description, SPIType indicatorType,
        string measurementUnit, SPIMeasurementFrequency frequency, string updatedBy)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(name))
                return Result.Failure(DomainErrors.SPIError.InvalidName);

            if (string.IsNullOrWhiteSpace(description))
                return Result.Failure(DomainErrors.SPIError.InvalidDescription);

            Name = name;
            Description = description;
            IndicatorType = indicatorType;
            MeasurementUnit = measurementUnit;
            MeasurementFrequency = frequency;
            UpdatedBy = updatedBy;
            UpdatedDate = DateTime.UtcNow;

            return Result.Success();
        }
        catch (Exception)
        {
            return Result.Failure(DomainErrors.SPIError.UpdateFailed);
        }
    }

    public Result SetTargets(decimal? targetValue, decimal? acceptableRange,
        decimal? warningThreshold, decimal? criticalThreshold, string updatedBy)
    {
        try
        {
            TargetValue = targetValue;
            AcceptableRange = acceptableRange;
            WarningThreshold = warningThreshold;
            CriticalThreshold = criticalThreshold;
            UpdatedBy = updatedBy;
            UpdatedDate = DateTime.UtcNow;

            return Result.Success();
        }
        catch (Exception)
        {
            return Result.Failure(DomainErrors.SPIError.TargetUpdateFailed);
        }
    }

    //public Result AddDataPoint(decimal value, DateTime measurementDate, string dataSource, string enteredBy)
    //{
    //    try
    //    {
    //        var dataPoint = new SPIDataPoint(new SPIDataPointID("DP-0000"); // Temporary ID - will be replaced by repository

    //        dataPoint.SPIId = this.Code,
    //            Value = value,
    //            MeasurementDate = measurementDate,
    //            DataSource = dataSource,
    //            Period = GetPeriodFromDate(measurementDate)
    //        };

    //        DataPoints.Add(dataPoint);
    //        UpdatedBy = enteredBy;
    //        UpdatedDate = DateTime.UtcNow;

    //        return Result.Success();
    //    }
    //    catch (Exception)
    //    {
    //        return Result.Failure(DomainErrors.SPIError.DataPointAddFailed);
    //    }
    //}

    public Result UpdateStatus(SPIStatus newStatus, string updatedBy)
    {
        try
        {
            if (Status == newStatus)
                return Result.Success();

            Status = newStatus;
            UpdatedBy = updatedBy;
            UpdatedDate = DateTime.UtcNow;

            return Result.Success();
        }
        catch (Exception)
        {
            return Result.Failure(DomainErrors.SPIError.StatusUpdateFailed);
        }
    }

    public Result ScheduleReview(DateTime reviewDate, string scheduledBy)
    {
        try
        {
            if (reviewDate <= DateTime.UtcNow)
                return Result.Failure(DomainErrors.SPIError.InvalidReviewDate);

            NextReviewDate = reviewDate;
            UpdatedBy = scheduledBy;
            UpdatedDate = DateTime.UtcNow;

            return Result.Success();
        }
        catch (Exception)
        {
            return Result.Failure(DomainErrors.SPIError.ReviewScheduleFailed);
        }
    }

    public Result CompleteReview(string reviewNotes, DateTime? nextReviewDate, string reviewedBy)
    {
        try
        {
            LastReviewDate = DateTime.UtcNow;
            LastReviewNotes = reviewNotes;
            NextReviewDate = nextReviewDate;
            UpdatedBy = reviewedBy;
            UpdatedDate = DateTime.UtcNow;

            return Result.Success();
        }
        catch (Exception)
        {
            return Result.Failure(DomainErrors.SPIError.ReviewCompletionFailed);
        }
    }

    // Query Methods
    public SPITrendDirection GetTrendDirection(int periods = 3)
    {
        var recentData = DataPoints
            .OrderByDescending(dp => dp.MeasurementDate)
            .Take(periods)
            .ToList();

        if (recentData.Count < 2)
            return SPITrendDirection.Stable;

        var firstValue = recentData.Last().Value;
        var lastValue = recentData.First().Value;

        if (lastValue > firstValue * 1.05m) // 5% threshold
            return SPITrendDirection.Improving;
        else if (lastValue < firstValue * 0.95m)
            return SPITrendDirection.Declining;
        else
            return SPITrendDirection.Stable;
    }

    public decimal? GetCurrentValue()
    {
        return DataPoints
            .OrderByDescending(dp => dp.MeasurementDate)
            .FirstOrDefault()?.Value;
    }

    public decimal? GetAverageValue(int periods = 12)
    {
        var recentData = DataPoints
            .OrderByDescending(dp => dp.MeasurementDate)
            .Take(periods)
            .ToList();

        return recentData.Any() ? recentData.Average(dp => dp.Value) : null;
    }

    public bool IsOverThreshold()
    {
        var currentValue = GetCurrentValue();
        if (!currentValue.HasValue || !CriticalThreshold.HasValue)
            return false;

        return currentValue.Value >= CriticalThreshold.Value;
    }

    public bool IsAtWarningLevel()
    {
        var currentValue = GetCurrentValue();
        if (!currentValue.HasValue || !WarningThreshold.HasValue)
            return false;

        return currentValue.Value >= WarningThreshold.Value;
    }

    public bool RequiresReview()
    {
        return NextReviewDate.HasValue && NextReviewDate.Value <= DateTime.UtcNow;
    }

    public List<SPIDataPoint> GetDataPointsForPeriod(DateTime startDate, DateTime endDate)
    {
        return DataPoints
            .Where(dp => dp.MeasurementDate >= startDate && dp.MeasurementDate <= endDate)
            .OrderBy(dp => dp.MeasurementDate)
            .ToList();
    }

    private string GetPeriodFromDate(DateTime date)
    {
        if (MeasurementFrequency.Value == SPIMeasurementFrequency.Daily.Value)
            return date.ToString("yyyy-MM-dd");
        else if (MeasurementFrequency.Value == SPIMeasurementFrequency.Weekly.Value)
            return $"{date.Year}-W{GetWeekNumber(date):D2}";
        else if (MeasurementFrequency.Value == SPIMeasurementFrequency.Monthly.Value)
            return date.ToString("yyyy-MM");
        else if (MeasurementFrequency.Value == SPIMeasurementFrequency.Quarterly.Value)
            return $"{date.Year}-Q{GetQuarter(date)}";
        else if (MeasurementFrequency.Value == SPIMeasurementFrequency.Annually.Value)
            return date.ToString("yyyy");
        else
            return date.ToString("yyyy-MM");
    }

    private int GetWeekNumber(DateTime date)
    {
        var culture = System.Globalization.CultureInfo.CurrentCulture;
        return culture.Calendar.GetWeekOfYear(date,
            System.Globalization.CalendarWeekRule.FirstDay, DayOfWeek.Monday);
    }

    private int GetQuarter(DateTime date)
    {
        return (date.Month - 1) / 3 + 1;
    }
}

