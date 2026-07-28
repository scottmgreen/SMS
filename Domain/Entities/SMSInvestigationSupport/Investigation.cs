//-----------------------------------------------------------------------
// <copyright file="Investigation.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: SMS investigation entity representing investigation for safety investigation workflows.
//                  Domain entity implementing business rules and invariants
//                  following Domain-Driven Design principles.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Domain.Entities;

/// <summary>
/// Investigation Domain Entity
/// Maps to tbld_Investigations table
/// </summary>
public sealed class Investigation : BaseAuditableEntity
{
    // Public constructor for instantiation
    public Investigation(InvestigationID id) : base(id, string.Empty, DateTime.UtcNow) { }

      

    #region Properties

    public string Code { get; set; } = string.Empty;
    public string? ReportCode { get; set; }
    public string? InvestigationNotes { get; set; }
    public string HazardCode { get; set; } = "HAZ-UNKNOWN";
    public string AssignedInvestigatorId { get; set; } = "UNASSIGNED";
    public InvestigationStatus Status { get; set; } = InvestigationStatus.InvestigatorAssigned;
    public DateTime? CompletedDate { get; set; }
    public string? InvestigationPlan { get; set; }
    public string? InvestigationObjectives { get; set; }
    public string? DecisionType { get; set; } = string.Empty;
    public string? DecisionRationale { get; set; } = string.Empty;
    public string? DecisionMaker { get; set; } = string.Empty;
    public DateTime? DecisionDate { get; set; }
    public string? NextSteps { get; set; }
    public string? ReferralDetails { get; set; }

    #endregion

    #region Factory Methods

   

    #endregion

    #region Domain Methods


    
    

    

    #endregion

    #region Query Properties

    public bool HasDecision => !string.IsNullOrWhiteSpace(DecisionType);
    

    

    
    

    #endregion
}

