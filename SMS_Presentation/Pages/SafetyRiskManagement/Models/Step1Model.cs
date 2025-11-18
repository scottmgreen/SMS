using System.ComponentModel.DataAnnotations;
using SMS_Domain.Entities;

namespace SMS.Presentation.Pages.SafetyRiskManagement.Models;

/// <summary>
/// Step 1 Model - System Description & 5M Framework
/// Handles all properties and logic specific to Step 1 of the Risk Assessment
/// </summary>
public class Step1Model
{
    #region System Overview Properties

    [Required(ErrorMessage = "Lead Assessor is required.")]
    [Display(Name = "Lead Assessor")]
    public string LeadAssessor { get; set; } = string.Empty;

    [Required(ErrorMessage = "System Description is required.")]
    [StringLength(1000, MinimumLength = 10, ErrorMessage = "System Description must be between 10 and 1000 characters.")]
    [Display(Name = "System Description")]
    public string SystemDescription { get; set; } = string.Empty;

    [Required(ErrorMessage = "System Boundaries are required.")]
    [StringLength(1000, MinimumLength = 10, ErrorMessage = "System Boundaries must be between 10 and 1000 characters.")]
    [Display(Name = "System Boundaries")]
    public string SystemBoundaries { get; set; } = string.Empty;

    [Required(ErrorMessage = "System Purpose is required.")]
    [StringLength(1000, MinimumLength = 10, ErrorMessage = "System Purpose must be between 10 and 1000 characters.")]
    [Display(Name = "System Purpose")]
    public string SystemPurpose { get; set; } = string.Empty;

    #endregion

    #region 5M Framework Properties

    [Required(ErrorMessage = "Personnel Factors (5M People) are required.")]
    [StringLength(1000, MinimumLength = 10, ErrorMessage = "Personnel Factors must be between 10 and 1000 characters.")]
    [Display(Name = "Personnel Factors")]
    public string FiveMPersonnel { get; set; } = string.Empty;

    [Required(ErrorMessage = "Equipment Factors (5M Equipment) are required.")]
    [StringLength(1000, MinimumLength = 10, ErrorMessage = "Equipment Factors must be between 10 and 1000 characters.")]
    [Display(Name = "Equipment Factors")]
    public string FiveMEquipment { get; set; } = string.Empty;

    [Required(ErrorMessage = "Procedure Factors (5M Procedures) are required.")]
    [StringLength(1000, MinimumLength = 10, ErrorMessage = "Procedure Factors must be between 10 and 1000 characters.")]
    [Display(Name = "Procedure Factors")]
    public string FiveMProcedures { get; set; } = string.Empty;

    [Required(ErrorMessage = "Resource Factors (5M Resources) are required.")]
    [StringLength(1000, MinimumLength = 10, ErrorMessage = "Resource Factors must be between 10 and 1000 characters.")]
    [Display(Name = "Resource Factors")]
    public string FiveMResources { get; set; } = string.Empty;

    [Required(ErrorMessage = "Physical Environment Factors (5M Environment) are required.")]
    [StringLength(1000, MinimumLength = 10, ErrorMessage = "Physical Environment must be between 10 and 1000 characters.")]
    [Display(Name = "Physical Environment")]
    public string FiveMPhysicalEnvironment { get; set; } = string.Empty;

    [Required(ErrorMessage = "Operational Environment Factors are required.")]
    [StringLength(1000, MinimumLength = 10, ErrorMessage = "Operational Environment must be between 10 and 1000 characters.")]
    [Display(Name = "Operational Environment")]
    public string FiveMOperationalEnvironment { get; set; } = string.Empty;

    #endregion

    #region Stakeholder Properties

    [Display(Name = "Stakeholder Groups")]
    public string StakeholderGroups { get; set; } = string.Empty;

    [Display(Name = "Individual Stakeholders")]
    public string SelectedIndividualStakeholders { get; set; } = string.Empty;

    #endregion

    #region Reference Data

    public List<SMSApplicationUser> AvailableAssessors { get; set; } = new();
    public List<SMSStakeholderUser> AvailableStakeholders { get; set; } = new();

    #endregion

    #region Step 1 Specific Methods

    /// <summary>
    /// Validates Step 1 data using business rules
    /// </summary>
    public (bool isValid, string message) Validate()
    {
        var step1Fields = new Dictionary<string, string>
        {
            { nameof(LeadAssessor), LeadAssessor },
            { nameof(SystemDescription), SystemDescription },
            { nameof(SystemBoundaries), SystemBoundaries },
            { nameof(SystemPurpose), SystemPurpose },
            { nameof(FiveMPersonnel), FiveMPersonnel },
            { nameof(FiveMEquipment), FiveMEquipment },
            { nameof(FiveMProcedures), FiveMProcedures },
            { nameof(FiveMResources), FiveMResources },
            { nameof(FiveMPhysicalEnvironment), FiveMPhysicalEnvironment },
            { nameof(FiveMOperationalEnvironment), FiveMOperationalEnvironment }
        };
        
        int fieldsWithData = 0;
        var missingFields = new List<string>();
        
        foreach (var field in step1Fields)
        {
            var value = field.Value?.Trim() ?? string.Empty;
            
            if (string.IsNullOrEmpty(value))
            {
                missingFields.Add(field.Key);
            }
            else if (value.Length >= 10)
            {
                fieldsWithData++;
            }
        }
        
        if (fieldsWithData < 4)
        {
            return (false, $"Need at least 4 complete fields (found {fieldsWithData}). Missing: {string.Join(", ", missingFields)}");
        }
        
        return (true, $"Step 1 validation passed with {fieldsWithData} complete fields");
    }

    /// <summary>
    /// Applies Step 1 data to the RiskAssessment entity
    /// </summary>
    public void ApplyToAssessment(RiskAssessment assessment)
    {
        // Use the new overload that handles both physical and operational environment
        //var updateResult = assessment.UpdateSystemDescription(
        assessment.SystemDescription.Trim();
        assessment.SystemBoundaries.Trim();
        assessment.SystemPurpose.Trim();
        assessment.FiveMPersonnel.Trim();
        assessment.FiveMEquipment.Trim();
        assessment.FiveMProcedures.Trim();
        assessment.FiveMResources.Trim();
        assessment.FiveMPhysicalEnvironment.Trim();
        assessment.FiveMOperationalEnvironment.Trim();
        //);

        //if (updateResult.IsFailure)
        //{
        //    throw new InvalidOperationException(updateResult.Error.Message);
        //}

        // Update lead assessor
        assessment.LeadAssessorId = LeadAssessor.Trim();

        // Update stakeholders if provided
        if (!string.IsNullOrEmpty(StakeholderGroups.Trim()))
        {
            // Simple comma-separated parsing
            var groups = StakeholderGroups.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(g => g.Trim())
                .Where(g => !string.IsNullOrEmpty(g));

            foreach (var group in groups)
            {
                assessment.AddStakeholder(group);
            }
        }

        // Mark step as completed
        assessment.CompleteStep(1);
    }

    /// <summary>
    /// Loads data from RiskAssessment entity into this model (only if fields are empty)
    /// </summary>
    public void LoadFromAssessment(RiskAssessment assessment)
    {
        if (assessment == null) return;

        // Only populate if fields are currently empty (don't overwrite user input)
        if (string.IsNullOrEmpty(LeadAssessor)) LeadAssessor = assessment.LeadAssessorId ?? string.Empty;
        if (string.IsNullOrEmpty(SystemDescription)) SystemDescription = assessment.SystemDescription ?? string.Empty;
        if (string.IsNullOrEmpty(SystemBoundaries)) SystemBoundaries = assessment.SystemBoundaries ?? string.Empty;
        if (string.IsNullOrEmpty(SystemPurpose)) SystemPurpose = assessment.SystemPurpose ?? string.Empty;
        
        // CORRECTED: Use the proper 5M property names
        if (string.IsNullOrEmpty(FiveMPersonnel)) FiveMPersonnel = assessment.FiveMPersonnel ?? string.Empty;
        if (string.IsNullOrEmpty(FiveMEquipment)) FiveMEquipment = assessment.FiveMEquipment ?? string.Empty;
        if (string.IsNullOrEmpty(FiveMProcedures)) FiveMProcedures = assessment.FiveMProcedures ?? string.Empty;
        if (string.IsNullOrEmpty(FiveMResources)) FiveMResources = assessment.FiveMResources ?? string.Empty;
        if (string.IsNullOrEmpty(FiveMPhysicalEnvironment)) FiveMPhysicalEnvironment = assessment.FiveMPhysicalEnvironment ?? string.Empty;
        if (string.IsNullOrEmpty(FiveMOperationalEnvironment)) FiveMOperationalEnvironment = assessment.FiveMOperationalEnvironment ?? string.Empty;
    }

    /// <summary>
    /// Checks if this step has meaningful data
    /// </summary>
    public bool HasData()
    {
        return !string.IsNullOrEmpty(SystemDescription) ||
               !string.IsNullOrEmpty(FiveMPersonnel) ||
               !string.IsNullOrEmpty(FiveMEquipment) ||
               !string.IsNullOrEmpty(FiveMProcedures);
    }

    /// <summary>
    /// Gets completion percentage for this step
    /// </summary>
    public int GetCompletionPercentage()
    {
        var fields = new[] { SystemDescription, SystemBoundaries, SystemPurpose, FiveMPersonnel, FiveMEquipment, FiveMProcedures, FiveMResources, FiveMPhysicalEnvironment, FiveMOperationalEnvironment };
        var completedFields = fields.Count(f => !string.IsNullOrWhiteSpace(f) && f.Length >= 10);
        return (int)((double)completedFields / fields.Length * 100);
    }

    #endregion
}