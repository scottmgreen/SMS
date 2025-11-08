using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PDXSMS_Presentation.Pages.SafetyRiskManagement;

/// <summary>
/// Integrated SMS Workflow demonstration page
/// Shows the complete flow from hazard reporting through risk assessment to mitigation
/// </summary>
public class IntegratedWorkflowModel : PageModel
{
    public void OnGet()
    {
        ViewData["Title"] = "Integrated SMS Workflow";
    }
}
