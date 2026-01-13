using Microsoft.AspNetCore.Components;

namespace SMS3.Components.Shared;

public partial class InvestigationsGuidance : ComponentBase
{
    [Inject] private NavigationManager Navigation { get; set; } = default!;

    private void CreateNewInvestigation()
    {
        // Navigate to new investigation creation page or form
        Navigation.NavigateTo("/SMSRiskManagement/Investigations/Create");
    }

    private void NavigateToMyInterviews()
    {
        // Navigate to the user's assigned interviews
        Navigation.NavigateTo("/SMSRiskManagement/MyInterviews");
    }

    private void NavigateToInvestigationList()
    {
        // Navigate to investigations listing page
        Navigation.NavigateTo("/Listings/Investigations");
    }

    private void NavigateToInvestigationReports()
    {
        // Navigate to investigation reports and analytics
        Navigation.NavigateTo("/SMSReporting/InvestigationReports");
    }

    private void ContactSupport()
    {
        // Could open a modal, navigate to support page, or trigger email
        Navigation.NavigateTo("/Support/Contact?topic=investigations");
    }
}