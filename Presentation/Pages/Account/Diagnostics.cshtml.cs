using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PDXSMS_Presentation.Pages.Account;

public class DiagnosticsModel : PageModel
{
    public string EnvironmentName { get; set; } = "";
    public bool SessionAvailable { get; set; }
    public string SessionId { get; set; } = "";
    public List<TestUser> TestUsers { get; set; } = new();

    public void OnGet()
    {
        EnvironmentName = global::System.Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown";
        SessionAvailable = HttpContext.Session.IsAvailable;
        SessionId = HttpContext.Session.Id ?? "None";
        
        TestUsers = new List<TestUser>
        {
            new() { Email = "admin@flypdx.com", Password = "admin123", Role = "Administrator" },
            new() { Email = "sarah.johnson@flypdx.com", Password = "safety123", Role = "Safety Manager" },
            new() { Email = "mike.chen@flypdx.com", Password = "ops123", Role = "Operations Manager" },
            new() { Email = "lisa.rodriguez@flypdx.com", Password = "audit123", Role = "Safety Auditor" },
            new() { Email = "john.smith@flypdx.com", Password = "user123", Role = "Safety Officer" }
        };
    }

    public async Task<IActionResult> OnPostTestLoginAsync(string testEmail, string testPassword)
    {
        try
        {
            var mockUsers = new Dictionary<string, string>
            {
                { "admin@flypdx.com", "admin123" },
                { "sarah.johnson@flypdx.com", "safety123" },
                { "mike.chen@flypdx.com", "ops123" },
                { "lisa.rodriguez@flypdx.com", "audit123" },
                { "john.smith@flypdx.com", "user123" }
            };

            await Task.Delay(100); // Simulate async validation

            var emailLower = testEmail?.ToLower() ?? "";
            var isValid = mockUsers.ContainsKey(emailLower) && mockUsers[emailLower] == testPassword;
            
            TempData["TestSuccess"] = isValid;
            TempData["TestResult"] = isValid 
                ? $"? Login successful for {testEmail}" 
                : $"? Login failed for {testEmail}. Check email/password.";
        }
        catch (Exception ex)
        {
            TempData["TestSuccess"] = false;
            TempData["TestResult"] = $"? Error during test: {ex.Message}";
        }

        return Page();
    }
}

public class TestUser
{
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
    public string Role { get; set; } = "";
}