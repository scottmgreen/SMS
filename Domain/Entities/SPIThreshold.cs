//-----------------------------------------------------------------------
// <copyright file="SPIThreshold.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: SMS safety performance indicator entity for measuring and tracking safety metrics.
//                  Domain entity implementing business rules and invariants
//                  following Domain-Driven Design principles.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Domain.Entities;

/// <summary>
/// SPI Threshold configuration for alerts
/// </summary>
public class SPIThreshold
{
    public string ThresholdType { get; set; } = string.Empty; // Warning, Critical, Target
    public decimal Value { get; set; }
    public string ComparisonOperator { get; set; } = string.Empty; // >=, <=, =, !=
    public string AlertMessage { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public string? AlertRecipients { get; set; }
}
