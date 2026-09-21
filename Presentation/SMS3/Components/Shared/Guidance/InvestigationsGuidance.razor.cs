using SMS3.Configuration.Extensions;

namespace SMS3.Components.Shared.Components;

public partial class InvestigationsGuidance : ComponentBase
{
    [Inject] private NavigationManager Navigation { get; set; } = default!;

    private bool isExpanded = false;

    private void CreateNewInvestigation()
    {
        Navigation.NavigateToSecure("/SMSRiskManagement/Investigations/Create");
    }

    private void NavigateToMyInterviews()
    {
        Navigation.NavigateToSecure("/SMSRiskManagement/MyInterviews");
    }

    private void NavigateToInvestigationList()
    {
        Navigation.NavigateToSecure("/Listings/Investigations");
    }

    private void NavigateToInvestigationReports()
    {
        Navigation.NavigateToSecure("/SMSReporting/InvestigationReports");
    }

    private void ContactSupport()
    {
        Navigation.NavigateToSecure("/Support/Contact", "topic", "investigations");
    }
}
