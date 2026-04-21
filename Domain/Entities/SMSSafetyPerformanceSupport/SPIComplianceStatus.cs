//-----------------------------------------------------------------------
// <copyright file="SPIComplianceStatus.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Domain entity representing SPI compliance status for reporting and monitoring.
//                  Core domain model for SPI compliance tracking and status management.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Domain.Entities;

/// <summary>
/// Domain entity representing SPI compliance status for reporting and monitoring
/// Used for tracking compliance against established thresholds and targets
/// </summary>
public class SPIComplianceStatus
{
    #region Core Properties
    public string SPIId { get; set; } = string.Empty;
    public string SPIName { get; set; } = string.Empty;
    public bool InCompliance { get; set; }
    public string ComplianceStatus { get; set; } = string.Empty; // Compliant, Warning, Critical, No Data
    public decimal? CurrentValue { get; set; }
    public decimal? ComplianceThreshold { get; set; }
    public DateTime? LastMeasurementDate { get; set; }
    public int DaysWithoutData { get; set; }
    #endregion

    #region Business Rules
    /// <summary>
    /// Determines if the SPI is within acceptable variance
    /// </summary>
    public bool IsWithinAcceptableVariance(decimal acceptableVariance)
    {
        if (!CurrentValue.HasValue || !ComplianceThreshold.HasValue)
            return false;

        var variance = Math.Abs(CurrentValue.Value - ComplianceThreshold.Value);
        return variance <= acceptableVariance;
    }

    /// <summary>
    /// Gets the days since last measurement
    /// </summary>
    public int GetDaysSinceLastMeasurement()
    {
        if (!LastMeasurementDate.HasValue)
            return int.MaxValue;

        return (DateTime.UtcNow - LastMeasurementDate.Value).Days;
    }

    /// <summary>
    /// Determines compliance status based on business rules
    /// </summary>
    public string DetermineComplianceStatus(decimal acceptableVariance, decimal unacceptableVariance)
    {
        if (!CurrentValue.HasValue || !ComplianceThreshold.HasValue)
            return "No Data";

        if (IsWithinAcceptableVariance(acceptableVariance))
            return "Compliant";

        var variance = Math.Abs(CurrentValue.Value - ComplianceThreshold.Value);
        return variance <= unacceptableVariance ? "Warning" : "Critical";
    }
    #endregion

    #region Constructors
    public SPIComplianceStatus() { }

    public SPIComplianceStatus(string spiId, string spiName)
    {
        SPIId = spiId ?? throw new ArgumentNullException(nameof(spiId));
        SPIName = spiName ?? throw new ArgumentNullException(nameof(spiName));
    }
    #endregion
}