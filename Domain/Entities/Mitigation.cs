//-----------------------------------------------------------------------
// <copyright file="Mitigation.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: SMS mitigation entity representing mitigation for risk mitigation strategies.
//                  Domain entity implementing business rules and invariants
//                  following Domain-Driven Design principles.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Domain.Entities;

/// <summary>
/// Mitigation Domain Entity - Comprehensive implementation matching tbld_Mitigations schema
/// Represents a risk mitigation strategy with complete lifecycle management
/// </summary>
public sealed class Mitigation : BaseAuditableEntity
{
    #region Constructors

    // Public constructor following domain pattern
    public Mitigation(MitigationID id) : base(id, "SYSTEM", DateTime.UtcNow) {}

    #endregion

    #region Core Properties (Required fields)

    /// <summary>Business identifier for the mitigation</summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>Associated hazard code - links to Hazard entity</summary>
    public string HazardCode { get; set; } = string.Empty;

    /// <summary>Descriptive name of the mitigation</summary>
    public string? Name { get; set; }

    /// <summary>Detailed description of the mitigation strategy</summary>
    public string? Description { get; set; }

    #endregion

    #region Classification Properties

    /// <summary>Type of mitigation (Engineering, Administrative, PPE, etc.)</summary>
    public string? Type { get; set; }

    /// <summary>Current status (Proposed, Approved, InProgress, Completed, Cancelled, OnHold)</summary>
    public MitigationStatus Status { get; set; } = MitigationStatus.PendingApproval;

    
    /// <summary>Associated risk assessment code</summary>
    public string? RiskAssessmentCode { get; set; }

    #endregion

    #region Timeline Properties

    /// <summary>Target completion date</summary>
    public DateTime? TargetDate { get; set; }

   
    #endregion

    #region Assignment Properties

    /// <summary>Department responsible for implementation</summary>
    public string? AssignedDepartment { get; set; }

    /// <summary>Person assigned to implement the mitigation</summary>
    public string? AssignedTo { get; set; }

   
    #endregion

    #region Progress Tracking

    /// <summary>Implementation progress percentage (0-100)</summary>
    public int Progress { get; set; } = 0;

    
    #endregion

    #region Cost and Resource Properties

    /// <summary>Estimated cost for implementation</summary>
    public decimal? EstimatedCost { get; set; }

    /// <summary>Resource requirements description</summary>
    public string? ResourceRequirements { get; set; }

    /// <summary>Estimated hours for implementation</summary>
    public int? EstimatedHours { get; set; }

  

    #endregion


  
}

