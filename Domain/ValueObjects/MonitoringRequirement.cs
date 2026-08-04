//-----------------------------------------------------------------------
// <copyright file="RiskAssessmentValueObjects.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Value object representing riskassessmentvalueobjects with immutable properties and business validation.
//                  Immutable value object encapsulating domain concepts with
//                  business logic and validation rules.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Domain.ValueObjects;

/// <summary>
/// Monitoring Requirement Value Object
/// This stays as Value Object since it's just configuration data, not an entity with lifecycle
/// </summary>
public sealed class MonitoringRequirement
{
    public string MonitoringFrequency { get; private set; }
    public string ResponsibleDepartment { get; private set; }
    public string ReviewTrigger { get; private set; }
    public DateTime EstablishedDate { get; private set; }

    private MonitoringRequirement(string frequency, string department, string trigger)
    {
        MonitoringFrequency = frequency;
        ResponsibleDepartment = department;
        ReviewTrigger = trigger;
        EstablishedDate = DateTime.UtcNow;
    }

    public static Result<MonitoringRequirement> Create(string frequency, string department, string trigger)
    {
        if (string.IsNullOrWhiteSpace(frequency))
        {
            return Result<MonitoringRequirement>.Failure<MonitoringRequirement>(DomainErrors.RiskAssessmentError.InvalidAssessmentType);
        }

        return Result<MonitoringRequirement>.Success(new MonitoringRequirement(
            frequency,
            department ?? "Safety Department",
            trigger ?? "Annual"));
    }
}
