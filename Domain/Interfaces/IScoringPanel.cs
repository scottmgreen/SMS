//-----------------------------------------------------------------------
// <copyright file="IScoringPanel.cs" company="SMS Safety Management System">
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
/// Business interface for Scoring Panel entity operations
/// </summary>
public interface IScoringPanel
{
    ScoringPanelID Id { get; set; }
    string? Code { get; set; }
    string? HazardCode { get; set; }
    string? SMSUserCode { get; set; }
    string? Likelihood { get; set; }
    string? Severity { get; set; }
    string? Score { get; set; }
}
