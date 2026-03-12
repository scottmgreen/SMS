using SMS3.Extensions;

namespace SMS3.Components.Shared;

public partial class AuditManagementGuidancePanel : ComponentBase
{
    [Inject] private NavigationManager Navigation { get; set; } = default!;

    private void CreateAnnualAuditPlan()
    {
        // ?? SECURE NAVIGATION - Navigate to create audit plan page with encrypted URL
        Navigation.NavigateToSecure("/SMSAssurance/AuditPlans/CreateAnnual");
    }

    private void CreateInternalAudit()
    {
        // ?? SECURE NAVIGATION - Navigate to create internal audit page with encrypted URL
        Navigation.NavigateToSecure("/SMSAssurance/AuditPlans/CreateInternal");
    }

    private void NavigateToEvidence()
    {
        // ?? SECURE NAVIGATION - Navigate to audit evidence with encrypted URL
        Navigation.NavigateToSecure("/SMSAssurance/AuditEvidence");
    }

    private void NavigateToAuditReports()
    {
        // ?? SECURE NAVIGATION - Navigate to audit reports with encrypted URL
        Navigation.NavigateToSecure("/SMSAssurance/AuditReports");
    }
}