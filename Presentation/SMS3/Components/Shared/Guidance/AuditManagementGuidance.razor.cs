using SMS3.Configuration.Extensions;

namespace SMS3.Components.Shared.Components;

public partial class AuditManagementGuidance : ComponentBase
{
    [Inject] private NavigationManager Navigation { get; set; } = default!;

    private bool isExpanded = false;

    private void CreateAnnualAuditPlan()
    {
        Navigation.NavigateToSecure("/SMSAssurance/AuditPlans/CreateAnnual");
    }

    private void CreateInternalAudit()
    {
        Navigation.NavigateToSecure("/SMSAssurance/AuditPlans/CreateInternal");
    }

    private void NavigateToEvidence()
    {
        Navigation.NavigateToSecure("/SMSAssurance/AuditEvidence");
    }

    private void NavigateToAuditReports()
    {
        Navigation.NavigateToSecure("/SMSAssurance/AuditReports");
    }
}
