//-----------------------------------------------------------------------
// <copyright file="IRiskAssessment.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Domain service contracts defining business operations for SMS domain entities.
//                  Domain service interfaces providing abstraction for business
//                  operations while maintaining domain model integrity.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;

namespace SMS_Domain.Interfaces;

/// <summary>
/// Business interface for Risk Assessment entity operations
/// </summary>
public interface IRiskAssessment
{
    RiskAssessmentID Id { get; set; }
    string? Code { get; set; }
    string? Name { get; set; }
    string? Description { get; set; }
    string? HazardCode { get; set; }
    string? AssessmentType { get; set; }
    string? Status { get; set; }
    string? Stage { get; set; }
}

