using System.ComponentModel.DataAnnotations;
using SMS_Domain.Entities;

namespace SMS.Presentation.Pages.SafetyRiskManagement.Models;

/// <summary>
/// Step 2 Model - Hazard Identification
/// Handles all properties and logic specific to Step 2 of the Risk Assessment
/// </summary>
public class Step2Model
{
    #region Hazard Properties

    [Required(ErrorMessage = "At least one hazard description is required.")]
    public List<string> HazardIds { get; set; } = new();

    [Required(ErrorMessage = "Hazard descriptions are required.")]
    public List<string> HazardDescriptions { get; set; } = new();

    public List<string> HazardCategories { get; set; } = new();

    #endregion

    #region Reference Data

    public List<Hazard> AvailableHazards { get; set; } = new();
    public List<string> PredefinedCategories { get; set; } = new()
    {
        "Aircraft Operations",
        "Ground Operations", 
        "Weather Related",
        "Equipment Failure",
        "Human Factors",
        "Security Related",
        "Infrastructure",
        "Environmental"
    };

    #endregion

    #region Step 2 Specific Methods

    /// <summary>
    /// Validates Step 2 data using business rules
    /// </summary>
    public (bool isValid, string message) Validate()
    {
        var validHazards = HazardDescriptions.Where(h => !string.IsNullOrWhiteSpace(h)).Count();
        
        if (validHazards < 1)
        {
            return (false, "At least 1 hazard is required");
        }
        
        return (true, $"Step 2 validation passed with {validHazards} hazards");
    }

    /// <summary>
    /// Applies Step 2 data to the RiskAssessment entity
    /// </summary>
    public void ApplyToAssessment(RiskAssessment assessment)
    {
        // Clear existing identified hazards
        assessment.ClearIdentifiedHazards();

        // Add new hazards
        for (int i = 0; i < HazardIds.Count && i < HazardDescriptions.Count; i++)
        {
            if (!string.IsNullOrWhiteSpace(HazardDescriptions[i]))
            {
                assessment.AddIdentifiedHazard(HazardIds[i], HazardDescriptions[i]);
            }
        }

        // Mark step as completed
        assessment.CompleteStep(2);
    }

    /// <summary>
    /// Loads data from RiskAssessment entity into this model
    /// </summary>
    public void LoadFromAssessment(RiskAssessment assessment, List<Hazard> relatedHazards)
    {
        if (assessment == null) return;

        // Load identified hazard IDs
        HazardIds = assessment.IdentifiedHazardIds.ToList();

        // Load hazard descriptions from related hazards
        HazardDescriptions = relatedHazards
            .Where(h => assessment.IdentifiedHazardIds.Contains(h.Code))
            .Select(h => h.Description)
            .ToList();

        // Load hazard categories
        HazardCategories = relatedHazards
            .Where(h => assessment.IdentifiedHazardIds.Contains(h.Code))
            .Select(h => h.HazardType)
            .ToList();
    }

    /// <summary>
    /// Adds a new hazard entry
    /// </summary>
    public void AddHazard(string? hazardId = null, string? description = null, string? category = null)
    {
        HazardIds.Add(hazardId ?? Guid.NewGuid().ToString());
        HazardDescriptions.Add(description ?? string.Empty);
        HazardCategories.Add(category ?? string.Empty);
    }

    /// <summary>
    /// Removes a hazard entry at the specified index
    /// </summary>
    public void RemoveHazard(int index)
    {
        if (index >= 0 && index < HazardIds.Count)
        {
            HazardIds.RemoveAt(index);
            if (index < HazardDescriptions.Count) HazardDescriptions.RemoveAt(index);
            if (index < HazardCategories.Count) HazardCategories.RemoveAt(index);
        }
    }

    /// <summary>
    /// Checks if this step has meaningful data
    /// </summary>
    public bool HasData()
    {
        return HazardDescriptions.Any(h => !string.IsNullOrWhiteSpace(h));
    }

    /// <summary>
    /// Gets completion percentage for this step
    /// </summary>
    public int GetCompletionPercentage()
    {
        if (!HazardDescriptions.Any()) return 0;
        
        var completedHazards = HazardDescriptions.Count(h => !string.IsNullOrWhiteSpace(h) && h.Length >= 10);
        return (int)((double)completedHazards / HazardDescriptions.Count * 100);
    }

    /// <summary>
    /// Gets the count of valid hazards
    /// </summary>
    public int GetValidHazardCount()
    {
        return HazardDescriptions.Count(h => !string.IsNullOrWhiteSpace(h));
    }

    #endregion
}