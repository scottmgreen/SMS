//-----------------------------------------------------------------------
// <copyright file="SPIDataPoint.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: SMS safety performance indicator entity for measuring and tracking safety metrics.
//                  Domain entity implementing business rules and invariants
//                  following Domain-Driven Design principles.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Domain.Entities;

/// <summary>
/// SPI Data Point value object for individual measurements
/// </summary>
public class SPIDataPoint : BaseAuditableEntity
{
    public SPIDataPoint(SPIDataPointID id) : base(id, "SYSTEM", DateTime.UtcNow)
    {
        Code = id.Value;

    }


    public string Code { get; set; }
    public string SPIId { get; set; }
    public decimal Value { get; set; }
    public DateTime MeasurementDate { get; set; }
    public string Period { get; set; } = string.Empty;
    public string DataSource { get; set; } = string.Empty;
    //public string EnteredBy { get; set; } = string.Empty;
    //public DateTime EnteredDate { get; set; }
    public string? Notes { get; set; }
    public bool IsVerified { get; set; } = false;
    public string? VerifiedBy { get; set; }
    public DateTime? VerifiedDate { get; set; }
}

