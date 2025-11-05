using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace PDXSMS_Presentation.Pages.Account;

public class ProfileModel : PageModel
{
    [BindProperty]
    public UserProfileDto UserProfile { get; set; } = new();

    [BindProperty]
    public PasswordChangeDto PasswordChange { get; set; } = new();

    public ProfileStatsDto ProfileStats { get; set; } = new();
    public List<SelectListItem> DepartmentOptions { get; set; } = new();

    public void OnGet()
    {
        LoadUserProfile();
        LoadDepartmentOptions();
        LoadProfileStats();
    }

    public async Task<IActionResult> OnPostUpdateProfileAsync()
    {
        if (!ModelState.IsValid)
        {
            LoadDepartmentOptions();
            LoadProfileStats();
            return Page();
        }

        try
        {
            // Simulate updating user profile
            await UpdateUserProfileAsync(UserProfile);
            
            // Update session with new name if changed
            HttpContext.Session.SetString("UserName", UserProfile.FullName);
            
            TempData["SuccessMessage"] = "Profile updated successfully!";
            return RedirectToPage();
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error updating profile: {ex.Message}";
            LoadDepartmentOptions();
            LoadProfileStats();
            return Page();
        }
    }

    public async Task<IActionResult> OnPostChangePasswordAsync()
    {
        // Clear profile validation errors for password change
        ModelState.Remove("UserProfile.FirstName");
        ModelState.Remove("UserProfile.LastName");
        ModelState.Remove("UserProfile.Email");

        if (!ModelState.IsValid)
        {
            LoadUserProfile();
            LoadDepartmentOptions();
            LoadProfileStats();
            return Page();
        }

        try
        {
            // Simulate password change
            var isCurrentPasswordValid = await ValidateCurrentPasswordAsync(PasswordChange.CurrentPassword);
            
            if (!isCurrentPasswordValid)
            {
                TempData["ErrorMessage"] = "Current password is incorrect.";
                LoadUserProfile();
                LoadDepartmentOptions();
                LoadProfileStats();
                return Page();
            }

            await ChangePasswordAsync(PasswordChange.NewPassword);
            TempData["SuccessMessage"] = "Password changed successfully!";
            
            // Clear the form
            PasswordChange = new PasswordChangeDto();
            
            return RedirectToPage();
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error changing password: {ex.Message}";
            LoadUserProfile();
            LoadDepartmentOptions();
            LoadProfileStats();
            return Page();
        }
    }

    private void LoadUserProfile()
    {
        var userId = HttpContext.Session.GetString("UserId") ?? "";
        var userEmail = HttpContext.Session.GetString("UserEmail") ?? "";
        var userName = HttpContext.Session.GetString("UserName") ?? "";
        var userRole = HttpContext.Session.GetString("UserRole") ?? "";
        var department = HttpContext.Session.GetString("Department") ?? "";

        // Parse full name
        var nameParts = userName.Split(' ');
        var firstName = nameParts.Length > 0 ? nameParts[0] : "";
        var lastName = nameParts.Length > 1 ? string.Join(" ", nameParts.Skip(1)) : "";

        UserProfile = new UserProfileDto
        {
            UserId = userId,
            FirstName = firstName,
            LastName = lastName,
            Email = userEmail,
            Department = department,
            Role = userRole,
            PhoneNumber = GetMockPhoneNumber(userEmail),
            JobTitle = GetMockJobTitle(userRole),
            Bio = GetMockBio(userName),
            CreatedDate = DateTime.Now.AddDays(-Random.Shared.Next(30, 365)),
            LastLoginDate = DateTime.Now.AddHours(-Random.Shared.Next(1, 24))
        };
    }

    private void LoadDepartmentOptions()
    {
        DepartmentOptions = new List<SelectListItem>
        {
            new("Safety Department", "Safety Department"),
            new("Operations", "Operations"),
            new("IT Department", "IT Department"),
            new("Quality Assurance", "Quality Assurance"),
            new("Regulatory Affairs", "Regulatory Affairs"),
            new("Airport Management", "Airport Management"),
            new("Ground Operations", "Ground Operations"),
            new("Air Traffic Control", "Air Traffic Control")
        };
    }

    private void LoadProfileStats()
    {
        ProfileStats = new ProfileStatsDto
        {
            LoginCount = Random.Shared.Next(50, 500),
            DaysActive = Random.Shared.Next(30, 200)
        };
    }

    private string GetMockPhoneNumber(string email)
    {
        var phoneNumbers = new Dictionary<string, string>
        {
            { "admin@flypdx.com", "(503) 555-0101" },
            { "sarah.johnson@flypdx.com", "(503) 555-0102" },
            { "mike.chen@flypdx.com", "(503) 555-0103" },
            { "lisa.rodriguez@flypdx.com", "(503) 555-0104" },
            { "john.smith@flypdx.com", "(503) 555-0105" }
        };

        return phoneNumbers.ContainsKey(email.ToLower()) ? phoneNumbers[email.ToLower()] : "(503) 555-0100";
    }

    private string GetMockJobTitle(string role)
    {
        return role switch
        {
            "Administrator" => "System Administrator",
            "Safety Manager" => "Senior Safety Manager",
            "Operations Manager" => "Operations Supervisor",
            "Safety Auditor" => "Lead Safety Auditor",
            "Safety Officer" => "Safety Officer",
            _ => "Staff Member"
        };
    }

    private string GetMockBio(string userName)
    {
        return userName switch
        {
            "System Administrator" => "Responsible for maintaining and configuring the PDXSMS system. Ensuring data security and system performance.",
            "Sarah Johnson" => "Experienced safety professional with over 10 years in aviation safety management. Specialized in risk assessment and safety culture development.",
            "Mike Chen" => "Operations expert focused on efficient airport operations and safety compliance. Background in air traffic management.",
            "Lisa Rodriguez" => "Certified safety auditor with expertise in regulatory compliance and safety assurance programs.",
            "John Smith" => "Front-line safety officer committed to identifying and mitigating operational hazards. Active in safety reporting initiatives.",
            _ => "Dedicated team member contributing to PDX safety management objectives."
        };
    }

    private async Task<bool> ValidateCurrentPasswordAsync(string currentPassword)
    {
        // Mock validation - in real implementation, hash and compare
        await Task.Delay(100);
        
        var userEmail = HttpContext.Session.GetString("UserEmail") ?? "";
        var mockPasswords = new Dictionary<string, string>
        {
            { "admin@flypdx.com", "admin123" },
            { "sarah.johnson@flypdx.com", "safety123" },
            { "mike.chen@flypdx.com", "ops123" },
            { "lisa.rodriguez@flypdx.com", "audit123" },
            { "john.smith@flypdx.com", "user123" }
        };

        return mockPasswords.ContainsKey(userEmail.ToLower()) && mockPasswords[userEmail.ToLower()] == currentPassword;
    }

    private async Task UpdateUserProfileAsync(UserProfileDto profile)
    {
        // Mock update - in real implementation, save to database
        await Task.Delay(200);
        
        // Update session if email changed (though typically email changes require verification)
        if (!string.IsNullOrEmpty(profile.Email))
        {
            HttpContext.Session.SetString("UserEmail", profile.Email);
        }
    }

    private async Task ChangePasswordAsync(string newPassword)
    {
        // Mock password change - in real implementation, hash and save
        await Task.Delay(200);
    }
}

public class UserProfileDto
{
    public string UserId { get; set; } = "";
    
    [Required(ErrorMessage = "First name is required")]
    public string FirstName { get; set; } = "";
    
    [Required(ErrorMessage = "Last name is required")]
    public string LastName { get; set; } = "";
    
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address")]
    public string Email { get; set; } = "";
    
    [Phone(ErrorMessage = "Please enter a valid phone number")]
    public string PhoneNumber { get; set; } = "";
    
    public string Department { get; set; } = "";
    public string JobTitle { get; set; } = "";
    public string Bio { get; set; } = "";
    public string Role { get; set; } = "";
    public DateTime CreatedDate { get; set; }
    public DateTime LastLoginDate { get; set; }
    
    public string FullName => $"{FirstName} {LastName}".Trim();
    public string Initials => $"{FirstName.FirstOrDefault()}{LastName.FirstOrDefault()}".ToUpper();
}

public class PasswordChangeDto
{
    [Required(ErrorMessage = "Current password is required")]
    public string CurrentPassword { get; set; } = "";
    
    [Required(ErrorMessage = "New password is required")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
    public string NewPassword { get; set; } = "";
    
    [Required(ErrorMessage = "Please confirm your new password")]
    [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
    public string ConfirmPassword { get; set; } = "";
}

public class ProfileStatsDto
{
    public int LoginCount { get; set; }
    public int DaysActive { get; set; }
}