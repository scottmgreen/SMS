//-----------------------------------------------------------------------
// <copyright file="SafetyPerformanceIndicatorCommands.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Command definitions for write operations in the SMS CQRS architecture.
//                  Defines command objects for write operations in the CQRS pattern.
//                  Commands represent business intentions and trigger state changes.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;

namespace SMS_Application.Commands;

// =============================================
// SPI CRUD COMMANDS
// =============================================

public class CreateSafetyPerformanceIndicatorCommand : BaseCommandBundle, IRequest<Result<SafetyPerformanceIndicator>>, ICreateCommand
{
    public string Name { get; set; }
    public string Description { get; set; }
    public SMSSafetyPerformanceIndicatorType IndicatorType { get; set; }
    public string MeasurementUnit { get; set; }
    public SPIMeasurementFrequency MeasurementFrequency { get; set; }
    public string CalculationMethod { get; set; }
    public string DataSource { get; set; }
    public decimal? TargetValue { get; set; }
    public decimal? AcceptableRange { get; set; }
    public decimal? WarningThreshold { get; set; }
    public decimal? CriticalThreshold { get; set; }
    public string ResponsibleDepartment { get; set; }
    public string DataOwner { get; set; }
    public string ReviewAuthority { get; set; }
    public DateTime? NextReviewDate { get; set; }
    public bool AlertsEnabled { get; set; }
    public string? AlertRecipients { get; set; }
    public string CreatedBy { get; set; }

    public CreateSafetyPerformanceIndicatorCommand(
        string name, string description, SMSSafetyPerformanceIndicatorType indicatorType, string measurementUnit,
        SPIMeasurementFrequency frequency, string calculationMethod, string dataSource,
        decimal? targetValue, decimal? acceptableRange, decimal? warningThreshold, decimal? criticalThreshold,
        string responsibleDepartment, string dataOwner, string reviewAuthority, DateTime? nextReviewDate,
        bool alertsEnabled, string? alertRecipients, string createdBy)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description ?? string.Empty;
        IndicatorType = indicatorType ?? throw new ArgumentNullException(nameof(indicatorType));
        MeasurementUnit = measurementUnit ?? string.Empty;
        MeasurementFrequency = frequency ?? throw new ArgumentNullException(nameof(frequency));
        CalculationMethod = calculationMethod ?? string.Empty;
        DataSource = dataSource ?? string.Empty;
        TargetValue = targetValue;
        AcceptableRange = acceptableRange;
        WarningThreshold = warningThreshold;
        CriticalThreshold = criticalThreshold;
        ResponsibleDepartment = responsibleDepartment ?? string.Empty;
        DataOwner = dataOwner ?? string.Empty;
        ReviewAuthority = reviewAuthority ?? string.Empty;
        NextReviewDate = nextReviewDate;
        AlertsEnabled = alertsEnabled;
        AlertRecipients = alertRecipients;
        CreatedBy = createdBy ?? throw new ArgumentNullException(nameof(createdBy));
    }

    public void SetCreatedBy(string userId, DateTime timestamp)
    {
        CreatedBy = userId;
    }

    public void SetUpdatedBy(string userId, DateTime timestamp)
    {
        // For create commands, we typically don't set UpdatedBy
    }
}

public class UpdateSafetyPerformanceIndicatorCommand : BaseCommandBundle, IRequest<Result<SafetyPerformanceIndicator>>, IUpdateCommand
{
    public SafetyPerformanceIndicatorID Id { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public SMSSafetyPerformanceIndicatorType IndicatorType { get; set; }
    public SPIStatus Status { get; set; }
    public string MeasurementUnit { get; set; }
    public SPIMeasurementFrequency MeasurementFrequency { get; set; }
    public string CalculationMethod { get; set; }
    public string DataSource { get; set; }
    public decimal? TargetValue { get; set; }
    public decimal? AcceptableRange { get; set; }
    public decimal? WarningThreshold { get; set; }
    public decimal? CriticalThreshold { get; set; }
    public string ResponsibleDepartment { get; set; }
    public string DataOwner { get; set; }
    public string ReviewAuthority { get; set; }
    public DateTime? NextReviewDate { get; set; }
    public DateTime? LastReviewDate { get; set; }
    public string? LastReviewNotes { get; set; }
    public bool AlertsEnabled { get; set; }
    public string? AlertRecipients { get; set; }
    public string UpdatedBy { get; set; }

    public UpdateSafetyPerformanceIndicatorCommand(
        SafetyPerformanceIndicatorID id, string code, string name, string description, SMSSafetyPerformanceIndicatorType indicatorType, SPIStatus status,
        string measurementUnit, SPIMeasurementFrequency frequency, string calculationMethod, string dataSource,
        decimal? targetValue, decimal? acceptableRange, decimal? warningThreshold, decimal? criticalThreshold,
        string responsibleDepartment, string dataOwner, string reviewAuthority, DateTime? nextReviewDate,
        DateTime? lastReviewDate, string? lastReviewNotes, bool alertsEnabled, string? alertRecipients, string updatedBy)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
        Code = code ?? throw new ArgumentNullException(nameof(code));
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description ?? string.Empty;
        IndicatorType = indicatorType ?? throw new ArgumentNullException(nameof(indicatorType));
        Status = status ?? throw new ArgumentNullException(nameof(status));
        MeasurementUnit = measurementUnit ?? string.Empty;
        MeasurementFrequency = frequency ?? throw new ArgumentNullException(nameof(frequency));
        CalculationMethod = calculationMethod ?? string.Empty;
        DataSource = dataSource ?? string.Empty;
        TargetValue = targetValue;
        AcceptableRange = acceptableRange;
        WarningThreshold = warningThreshold;
        CriticalThreshold = criticalThreshold;
        ResponsibleDepartment = responsibleDepartment ?? string.Empty;
        DataOwner = dataOwner ?? string.Empty;
        ReviewAuthority = reviewAuthority ?? string.Empty;
        NextReviewDate = nextReviewDate;
        LastReviewDate = lastReviewDate;
        LastReviewNotes = lastReviewNotes;
        AlertsEnabled = alertsEnabled;
        AlertRecipients = alertRecipients;
        UpdatedBy = updatedBy ?? throw new ArgumentNullException(nameof(updatedBy));
    }

    public void SetCreatedBy(string userId, DateTime timestamp)
    {
        // For update commands, we typically don't modify CreatedBy
    }

    public void SetUpdatedBy(string userId, DateTime timestamp)
    {
        UpdatedBy = userId;
    }
}

public class DeleteSafetyPerformanceIndicatorCommand : BaseCommandBundle, IRequest<Result<bool>>
{
    public SafetyPerformanceIndicatorID SafetyPerformanceIndicatorId { get; set; }

    public DeleteSafetyPerformanceIndicatorCommand(SafetyPerformanceIndicatorID spiId)
    {
        SafetyPerformanceIndicatorId = spiId ?? throw new ArgumentNullException(nameof(spiId));
    }
}

// =============================================
// SPI CONFIGURATION COMMANDS
// =============================================

public class UpdateSPIConfigurationCommand : BaseCommandBundle, IRequest<Result<SafetyPerformanceIndicator>>
{
    public string SPIId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string IndicatorType { get; set; }
    public string MeasurementUnit { get; set; }
    public string MeasurementFrequency { get; set; }
    public string CalculationMethod { get; set; }
    public string DataSource { get; set; }
    public string UpdatedBy { get; set; }

    public UpdateSPIConfigurationCommand(string spiId, string name, string description,
        string indicatorType, string measurementUnit, string frequency,
        string calculationMethod, string dataSource, string updatedBy)
    {
        SPIId = spiId ?? throw new ArgumentNullException(nameof(spiId));
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description ?? throw new ArgumentNullException(nameof(description));
        IndicatorType = indicatorType ?? throw new ArgumentNullException(nameof(indicatorType));
        MeasurementUnit = measurementUnit ?? throw new ArgumentNullException(nameof(measurementUnit));
        MeasurementFrequency = frequency ?? throw new ArgumentNullException(nameof(frequency));
        CalculationMethod = calculationMethod ?? string.Empty;
        DataSource = dataSource ?? throw new ArgumentNullException(nameof(dataSource));
        UpdatedBy = updatedBy ?? throw new ArgumentNullException(nameof(updatedBy));
    }
}

public class SetSPITargetsCommand : BaseCommandBundle, IRequest<Result<SafetyPerformanceIndicator>>
{
    public string SPIId { get; set; }
    public decimal? TargetValue { get; set; }
    public decimal? AcceptableRange { get; set; }
    public decimal? WarningThreshold { get; set; }
    public decimal? CriticalThreshold { get; set; }
    public string UpdatedBy { get; set; }

    public SetSPITargetsCommand(string spiId, decimal? targetValue, decimal? acceptableRange,
        decimal? warningThreshold, decimal? criticalThreshold, string updatedBy)
    {
        SPIId = spiId ?? throw new ArgumentNullException(nameof(spiId));
        TargetValue = targetValue;
        AcceptableRange = acceptableRange;
        WarningThreshold = warningThreshold;
        CriticalThreshold = criticalThreshold;
        UpdatedBy = updatedBy ?? throw new ArgumentNullException(nameof(updatedBy));
    }
}

public class UpdateSPIStatusCommand : BaseCommandBundle, IRequest<Result<SafetyPerformanceIndicator>>
{
    public string SPIId { get; set; }
    public string Status { get; set; }
    public string UpdatedBy { get; set; }

    public UpdateSPIStatusCommand(string spiId, string status, string updatedBy)
    {
        SPIId = spiId ?? throw new ArgumentNullException(nameof(spiId));
        Status = status ?? throw new ArgumentNullException(nameof(status));
        UpdatedBy = updatedBy ?? throw new ArgumentNullException(nameof(updatedBy));
    }
}

// =============================================
// SPI DATA POINT COMMANDS
// =============================================

public class AddSPIDataPointCommand : BaseCommandBundle, IRequest<Result<SafetyPerformanceIndicator>>
{
    public SPIDataPoint DataPoint { get; set; }

    public AddSPIDataPointCommand(SPIDataPoint dataPoint)
    {
        DataPoint = dataPoint;
    }
}

public class UpdateSPIDataPointCommand : BaseCommandBundle, IRequest<Result<SafetyPerformanceIndicator>>
{
    public SPIDataPoint DataPoint { get; set; }

    public UpdateSPIDataPointCommand(SPIDataPoint dataPoint)
    {
        DataPoint = dataPoint;
    }
}

public class DeleteSPIDataPointCommand : BaseCommandBundle, IRequest<Result<SafetyPerformanceIndicator>>
{
    public SPIDataPoint DataPoint { get; set; }

    public DeleteSPIDataPointCommand(SPIDataPoint dataPoint)
    {
        DataPoint = dataPoint;
    }
}

// =============================================
// SPI REVIEW COMMANDS
// =============================================

public class ScheduleSPIReviewCommand : BaseCommandBundle, IRequest<Result<SafetyPerformanceIndicator>>
{
    public string SPIId { get; set; }
    public DateTime ReviewDate { get; set; }
    public string ScheduledBy { get; set; }

    public ScheduleSPIReviewCommand(string spiId, DateTime reviewDate, string scheduledBy)
    {
        SPIId = spiId ?? throw new ArgumentNullException(nameof(spiId));
        ReviewDate = reviewDate;
        ScheduledBy = scheduledBy ?? throw new ArgumentNullException(nameof(scheduledBy));
    }
}

public class CompleteSPIReviewCommand : BaseCommandBundle, IRequest<Result<SafetyPerformanceIndicator>>
{
    public string SPIId { get; set; }
    public string ReviewNotes { get; set; }
    public DateTime? NextReviewDate { get; set; }
    public string ReviewedBy { get; set; }

    public CompleteSPIReviewCommand(string spiId, string reviewNotes, DateTime? nextReviewDate, string reviewedBy)
    {
        SPIId = spiId ?? throw new ArgumentNullException(nameof(spiId));
        ReviewNotes = reviewNotes ?? throw new ArgumentNullException(nameof(reviewNotes));
        NextReviewDate = nextReviewDate;
        ReviewedBy = reviewedBy ?? throw new ArgumentNullException(nameof(reviewedBy));
    }
}

// =============================================
// DASHBOARD SPECIFIC COMMANDS
// =============================================

public class RecalculateSPIDashboardCommand : BaseCommandBundle, IRequest<Result<bool>>
{
    public DateTime? CalculationDate { get; set; }
    public List<string>? SPIIds { get; set; }
    public string RequestedBy { get; set; }

    public RecalculateSPIDashboardCommand(string requestedBy, DateTime? calculationDate = null, List<string>? spiIds = null)
    {
        RequestedBy = requestedBy ?? throw new ArgumentNullException(nameof(requestedBy));
        CalculationDate = calculationDate ?? DateTime.UtcNow;
        SPIIds = spiIds;
    }
}

public class GenerateSPIAlertsCommand : BaseCommandBundle, IRequest<Result<List<SPIAlertResult>>>
{
    public List<string>? SPIIds { get; set; }
    public DateTime? CheckDate { get; set; }
    public string RequestedBy { get; set; }

    public GenerateSPIAlertsCommand(string requestedBy, List<string>? spiIds = null, DateTime? checkDate = null)
    {
        RequestedBy = requestedBy ?? throw new ArgumentNullException(nameof(requestedBy));
        SPIIds = spiIds;
        CheckDate = checkDate ?? DateTime.UtcNow;
    }
}

/// <summary>
/// SPI Alert result object for commands
/// </summary>
public class SPIAlertResult
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
/// Archive old SPI data command for data retention
/// </summary>
public class ArchiveOldSPIDataCommand : BaseCommandBundle, IRequest<Result<int>>
{
    public DateTime CutoffDate { get; set; }
    public List<string>? SPIIds { get; set; }
    public string RequestedBy { get; set; }

    public ArchiveOldSPIDataCommand(DateTime cutoffDate, string requestedBy, List<string>? spiIds = null)
    {
        CutoffDate = cutoffDate;
        RequestedBy = requestedBy ?? throw new ArgumentNullException(nameof(requestedBy));
        SPIIds = spiIds;
    }
}
