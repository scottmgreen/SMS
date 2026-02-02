namespace SMS_Domain.Interfaces;

/// <summary>
/// Hazard interface defining the contract for SMS hazard entities
/// Includes all properties required for comprehensive hazard management
/// </summary>
public interface IHazard
{
    #region Core Properties
    HazardID Id { get; set; }
    string Code { get; set; }
    string? Name { get; set; }
    string Description { get; set; }
    string ReportCode { get; set; }
    #endregion

    #region Hazard Classification
    string? HazardType { get; set; }           // Type/category of hazard
    string Category { get; set; }              // Aircraft Operations, Ground Operations, etc.
    SMS_Domain.Enums.HazardStatus Status { get; set; }          // Active, UnderInvestigation, etc.
    SMS_Domain.Enums.HazardPriority Priority { get; set; }     // Low, Medium, High, Critical
    #endregion

    #region Reporting Information
    string ReportedBy { get; set; }           // Who reported the hazard
    DateTime ReportedOn { get; set; }         // When hazard was reported (renamed from ReportedDate)
    string? ReportingDepartment { get; set; } // Department that reported the hazard
    #endregion

    #region Privacy and Confidentiality
    bool IsConfidential { get; set; }         // Confidential hazard - restricted access
    bool IsAnonymous { get; set; }            // Anonymous reporting - protect reporter identity
    #endregion

    #region Location Properties
    string? Location { get; set; }            // Legacy location field
    string? LocationArea { get; set; }        // General area
    string? LocationSubArea { get; set; }     // Sub-area
    HazardLocation? HazardLocation { get; }   // Full geospatial location entity
    #endregion

    #region Risk Assessment Properties
    string? ScoringPanelCode { get; set; }
    string? AverageScore { get; set; }
    string? RiskLevel { get; set; }           // Very Low, Low, Medium, High, Very High
    //string? WorstCredibleOutcome { get; set; }
    //string? RootCause { get; set; }
    #endregion

    #region Mitigation Properties
    string? CurrentMitigations { get; set; }
    string? ProposedMitigations { get; set; }
    DateTime? MitigationTargetDate { get; set; }
    string? MitigationOwner { get; set; }
    #endregion

    #region Investigation Properties
    bool RequiresInvestigation { get; set; }
    DateTime? InvestigationCompletedDate { get; set; }
    string? InvestigationNotes { get; set; }
    #endregion

    #region Additional Properties
    string? AdditionalComments { get; set; }
    #endregion

    #region Behavior Methods
    /// <summary>
    /// Set hazard location
    /// </summary>
    Result<bool> SetLocation(HazardLocation hazardLocation);

    /// <summary>
    /// Create and set a new hazard location
    /// </summary>
    Result<bool> CreateLocation(decimal? latitude = null, decimal? longitude = null,
        string? locationArea = null, string? locationSubArea = null, string? description = null);

    /// <summary>
    /// Update location coordinates
    /// </summary>
    Result<bool> UpdateLocationCoordinates(decimal latitude, decimal longitude,
        decimal? accuracyMeters = null, string source = "Manual");

    /// <summary>
    /// Mark hazard as requiring investigation
    /// </summary>
    Result<bool> RequireInvestigation(string investigationNotes);

    /// <summary>
    /// Complete investigation
    /// </summary>
    Result<bool> CompleteInvestigation(string investigationNotes);

    /// <summary>
    /// Update risk assessment information
    /// </summary>
    Result<bool> UpdateRiskAssessment(string? worstCredibleOutcome, string? rootCause, string? riskLevel);

    /// <summary>
    /// Update mitigation information
    /// </summary>
    Result<bool> UpdateMitigations(string? currentMitigations, string? proposedMitigations,
        DateTime? targetDate, string? mitigationOwner);

    /// <summary>
    /// Check if hazard has location information
    /// </summary>
    bool HasLocation();

    /// <summary>
    /// Check if hazard has GPS coordinates
    /// </summary>
    bool HasCoordinates();

    /// <summary>
    /// Check if hazard is high priority
    /// </summary>
    bool IsHighPriority();

    /// <summary>
    /// Check if hazard needs attention
    /// </summary>
    bool NeedsAttention();

    /// <summary>
    /// Get hazard age in days
    /// </summary>
    int GetAgeInDays();
    #endregion
}
