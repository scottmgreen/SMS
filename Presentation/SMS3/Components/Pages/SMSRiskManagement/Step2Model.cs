namespace SMS3.Components.Pages.SMSRiskManagement;

/// <summary>
/// Step 2: Hazard Identification
/// </summary>
public class Step2Model
{
    public List<string> HazardIds { get; set; } = new();
    public List<string> HazardDescriptions { get; set; } = new();
    public List<string> HazardCategories { get; set; } = new();

    
    public void LoadFromAssessment(RiskAssessment assessment)
    {
        if (assessment == null) return;

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
