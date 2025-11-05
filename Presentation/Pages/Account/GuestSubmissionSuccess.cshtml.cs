using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PDXSMS_Presentation.Pages.Account;

public class GuestSubmissionSuccessModel : PageModel
{
    public string TrackingId { get; set; } = string.Empty;

    public IActionResult OnGet(string trackingId)
    {
        if (string.IsNullOrEmpty(trackingId))
        {
            return RedirectToPage("/Account/Login");
        }

        TrackingId = trackingId;
        
        // Ensure session is cleared for security
        HttpContext.Session.Clear();
        
        return Page();
    }
}