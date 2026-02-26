//-----------------------------------------------------------------------
// <copyright file="Hazard.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: SMS hazard entity representing hazard for safety management processes.
//                  Domain entity implementing business rules and invariants
//                  following Domain-Driven Design principles.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Domain.Entities;

/// <summary>
/// Hazard Domain Entity - Mission Critical
/// Represents identified hazards within the SMS system with comprehensive properties
/// to support the Risk Assessment Wizard and hazard management workflows
/// </summary>
public sealed class Hazard : BaseAuditableEntity
{
    // Public constructor for domain usage
    public Hazard(HazardID id) : base(id, "SYSTEM", DateTime.UtcNow) { }

    // Private constructor for creation with validation
    //private Hazard(HazardID id, string code, string description, string category)
    //    : base(id, "SYSTEM", DateTime.UtcNow)
    //{
    //    Code = code;
    //    Name = HazardType;
    //    Description = description;
    //    HazardCategory = category;
    //    Status = HazardStatus.Active;
    //    Priority = HazardPriority.Medium;
    //    CreatedDate = DateTime.UtcNow;
    //    UpdatedDate = DateTime.UtcNow;
    //    HazardLocation = new HazardLocation(new HazardLocationID("HL-0000"));
    //}

    #region Core Properties

    public string Code { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string Description { get; set; } = string.Empty;

    public RiskLevel HazardRiskLevel { get; set; } = RiskLevel.Unkonwn;
    
    public HazardStatus Status { get; set; } = HazardStatus.InitialRiskAssessment;
    
    public bool IsInitialHazard { get; set; }
    #endregion

    #region Hazard Classification Properties
    public string HazardCategory { get; set; } = string.Empty; // Aircraft Operations, Ground Operations, etc.
    public string? HazardType { get; set; }              // Type/category of hazard (e.g., "Operational", "Equipment", etc.)

    #endregion

   
    #region Location and Context Properties

    public HazardLocation Location { get; set; }
    public string? LocationArea { get; set; }
    public string? LocationSubArea { get; set; }

    // Hazard Location Entity Reference
    public HazardLocation? HazardLocation { get; set; }

    #endregion

    #region Related Entity References (IDs Only)

    // Related HazardFiles (IDs only - load separately when needed)
    private readonly List<HazardFileID> _hazardFileIds = new();
    public IReadOnlyList<HazardFileID> HazardFileIds => _hazardFileIds.AsReadOnly();

    // Related Mitigations (IDs only - load separately when needed) 
    private readonly List<MitigationID> _mitigationIds = new();
    public IReadOnlyList<MitigationID> MitigationIds => _mitigationIds.AsReadOnly();

    #endregion

    #region Assessment and Risk Properties

    public string ReportCode { get; set; } = string.Empty;
    public string? InitialRiskMatrixCode { get; set; }
    public decimal? InitialAverageScore { get; set; }

    public string? ResidualRiskMatrixCode { get; set; }
    public decimal? ResidualAverageScore { get; set; }


    public string? InitialWorstCredibleOutcome { get; set; }
    public string? InitialRootCause { get; set; }

    #endregion

    #region Additional Assessment Context

    public string? AdditionalComments { get; set; }
    public bool RequiresInvestigation { get; set; } = false;
    public DateTime? InvestigationCompletedDate { get; set; }
    public string? InvestigationNotes { get; set; }

    #endregion



     

    #region Related Entity Management

    /// <summary>
    /// Add a HazardFile reference to this hazard
    /// </summary>
    public void AddHazardFile(HazardFileID hazardFileId)
    {
        if (hazardFileId != null && !_hazardFileIds.Contains(hazardFileId))
        {
            _hazardFileIds.Add(hazardFileId);
            UpdatedDate = DateTime.UtcNow;
        }
    }




    #endregion


    /// <summary>
    /// Analyze and suggest Five M component based on hazard description
    /// </summary>
    






}

