namespace SMS3.Components.Shared;

public partial class AuditManagementGuidancePanel : ComponentBase
{
    [Inject] private NavigationManager Navigation { get; set; } = default!;

    private void CreateAnnualAuditPlan()
    {
        // Navigate to create audit plan page with annual plan template
        Navigation.NavigateTo("/SMSAssurance/AuditPlans/CreateAnnual");
    }

    private void CreateInternalAudit()
    {
        // Navigate to create internal audit page
        Navigation.NavigateTo("/SMSAssurance/AuditPlans/CreateInternal");
    }

    private void NavigateToEvidence()
    {
        Navigation.NavigateTo("/SMSAssurance/AuditEvidence");
    }

    private void NavigateToAuditReports()
    {
        Navigation.NavigateTo("/SMSAssurance/AuditReports");
    }
}