
namespace SMS3.Components.Pages.SMSRiskManagement.Models;

/// <summary>
/// Step 2: Hazard Identification
/// </summary>
public class Step2Model
{
    public List<string> HazardIds { get; set; } = new();
    public List<string> HazardDescriptions { get; set; } = new();
    public List<string> HazardCategories { get; set; } = new();

    public (bool isValid, string message) Validate(List<Hazard>? availableHazards = null)
    {
        var hazardCount = availableHazards?.Count ?? HazardIds.Count(id => !string.IsNullOrWhiteSpace(id));

        if (hazardCount < 1)
        {
            return (false, "At least 1 hazard must be identified before proceeding to Step 3");
        }

        return (true, $"Step 2 validation passed with {hazardCount} hazard(s)");
    }

    
    public void LoadFromAssessment(RiskAssessment assessment)
    {
        if (assessment is null) return;

        if (assessment.IdentifiedHazardIds?.Any() == true)
        {
            HazardIds = assessment.IdentifiedHazardIds.ToList();
            HazardDescriptions = new List<string>(new string[HazardIds.Count]);
            HazardCategories = new List<string>(new string[HazardIds.Count]);

            for (int i = 0; i < HazardIds.Count; i++)
            {
                HazardDescriptions[i] = $"Hazard {HazardIds[i]}";
                HazardCategories[i] = "General";
            }
        }
        else
        {
            HazardIds = new List<string>();
            HazardDescriptions = new List<string>();
            HazardCategories = new List<string>();
        }
    }

    public void ApplyToAssessment(RiskAssessment assessment)
    {
        assessment.ClearIdentifiedHazards();

        foreach (var hazardId in HazardIds)
        {
            if (!string.IsNullOrEmpty(hazardId))
            {
                var hazardDescription = HazardDescriptions.Count > HazardIds.IndexOf(hazardId)
                    ? HazardDescriptions[HazardIds.IndexOf(hazardId)]
                    : $"Hazard {hazardId}";

                assessment.AddIdentifiedHazard(hazardId, hazardDescription);
            }
        }

        assessment.CompleteStep(2);
    }
}
