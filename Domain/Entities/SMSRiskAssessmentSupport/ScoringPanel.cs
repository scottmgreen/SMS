//-----------------------------------------------------------------------
// <copyright file="ScoringPanel.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: SMS domain entity representing scoringpanel with business rules and lifecycle management.
//                  Domain entity implementing business rules and invariants
//                  following Domain-Driven Design principles.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Domain.Entities;

/// <summary>
/// Scoring Panel entity for managing risk assessment panels and evaluation processes
/// </summary>
public class ScoringPanel : BaseAuditableEntity
{
    public ScoringPanel(ScoringPanelID id) : base(id, string.Empty, DateTime.UtcNow) { }

    public string? Code { get; set; }
    public string? HazardCode { get; set; }
    public string? RiskAssessmentCode { get; set; }
    public string? SMSUserCode { get; set; }
    
    // Step 4 (Technical) properties
    public int? InitialLikelihood { get; set; } = 0;
    public int? InitialSeverity { get; set; } = 0;
    public decimal? InitialScore { get; set; }
    public string? InitialRationale { get; set; }

    // Step 5 (Residual) properties  
    public int? ResidualLikelihood { get; set; } = 0;
    public int? ResidualSeverity { get; set; } = 0;
    public decimal? ResidualScore { get; set; }
    public string? ResidualRationale { get; set; }

    // Backward compatibility properties - will be mapped based on CurrentStep
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public int? Likelihood { get; set; }
    
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public int? Severity { get; set; }
    
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public decimal? Score { get; set; }
    
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public string? Rationale { get; set; }
}
