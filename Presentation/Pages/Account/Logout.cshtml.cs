using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PDXSMS_Presentation.Pages.Account;

public class LogoutModel : PageModel
{
    public IActionResult OnGet()
    {
        // Clear session
        HttpContext.Session.Clear();
        
        // Clear remember me cookie if it exists
        if (Request.Cookies.ContainsKey("RememberMe"))
        {
            Response.Cookies.Delete("RememberMe");
        }
        
        // Clear any authentication cookies
        foreach (var cookie in Request.Cookies.Keys)
        {
            if (cookie.StartsWith("AspNetCore") || cookie.StartsWith("Auth"))
            {
                Response.Cookies.Delete(cookie);
            }
        }
        
        return Page();
    }

    public IActionResult OnPost()
    {
        return OnGet();
    }
}