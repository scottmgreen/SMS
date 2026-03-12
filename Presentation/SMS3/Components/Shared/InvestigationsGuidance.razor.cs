using SMS3.Extensions;

namespace SMS3.Components.Shared;

public partial class InvestigationsGuidance : ComponentBase
{
    [Inject] private NavigationManager Navigation { get; set; } = default!;

    private void CreateNewInvestigation()
    {
        // ?? SECURE NAVIGATION - Navigate to new investigation creation with encrypted URL
        Navigation.NavigateToSecure("/SMSRiskManagement/Investigations/Create");
    }

    private void NavigateToMyInterviews()
    {
        // ?? SECURE NAVIGATION - Navigate to user's assigned interviews with encrypted URL
        Navigation.NavigateToSecure("/SMSRiskManagement/MyInterviews");
    }

    private void NavigateToInvestigationList()
    {
        // ?? SECURE NAVIGATION - Navigate to investigations listing with encrypted URL
        Navigation.NavigateToSecure("/Listings/Investigations");
    }

    private void NavigateToInvestigationReports()
    {
        // ?? SECURE NAVIGATION - Navigate to investigation reports with encrypted URL
        Navigation.NavigateToSecure("/SMSReporting/InvestigationReports");
    }

    private void ContactSupport()
    {
        // ?? SECURE NAVIGATION - Navigate to support page with encrypted URL
        Navigation.NavigateToSecure("/Support/Contact", "topic", "investigations");
    }
}