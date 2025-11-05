using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PDXSMS.Services;
using System.Text;

namespace PDXSMS_Presentation.Pages.System;

/// <summary>
/// User Management - Manage user accounts, roles, and permissions
/// </summary>
public class UserManagementModel : PageModel
{
    private readonly DashboardDataService _dashboardService;
    private readonly UniversalUserRepository _userRepository;
    private readonly DashboardRefreshService _refreshService;

    public List<UserAccount> Users { get; set; } = new();
    public List<UserRole> Roles { get; set; } = new();
    public UserManagementStats Stats { get; set; } = new();

    public UserManagementModel(
        DashboardDataService dashboardService, 
        UniversalUserRepository userRepository,
        DashboardRefreshService refreshService)
    {
        _dashboardService = dashboardService;
        _userRepository = userRepository;
        _refreshService = refreshService;
    }

    public async Task OnGetAsync()
    {
        ViewData["Title"] = "User Management - System Administration";
        await LoadUserDataAsync();
    }

    // Add User Handler - Fixed method name to match asp-page-handler
    public async Task<IActionResult> OnPostAddUserAsync(string fullName, string email, string department, string initialRole, bool isActive = true, string userType = "SMS")
    {
        try
        {
            if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(department))
            {
                TempData["ErrorMessage"] = "Full name, email, and department are required.";
                return RedirectToPage();
            }

            // Check if email already exists in any user type
            var appUsers = await _userRepository.GetAllApplicationUsersAsync();
            var smsUsers = await _userRepository.GetAllSMSUsersAsync();
            var stakeholderUsers = await _userRepository.GetAllStakeholderUsersAsync();

            if (appUsers.Any(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase)) ||
                smsUsers.Any(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase)) ||
                stakeholderUsers.Any(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase)))
            {
                TempData["ErrorMessage"] = $"A user with email '{email}' already exists.";
                return RedirectToPage();
            }

            // Create SMS Organizational User by default (since this is SMS management)
            var nameParts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var firstName = nameParts.FirstOrDefault() ?? fullName;
            var lastName = nameParts.Skip(1).FirstOrDefault() ?? "";
            
            var newSMSUser = new SMSOrganizationalUserData
            {
                Id = Guid.NewGuid().ToString(),
                EmployeeId = GenerateEmployeeId(department),
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                Department = department,
                JobTitle = "Staff Member", // Default job title
                IsEmployee = true,
                EmploymentStatus = "Active",
                SMSTeamMemberships = new List<string>(),
                Qualifications = new List<string>(),
                IsActive = isActive,
                CreatedDate = DateTime.UtcNow
            };

            // Add initial role if specified
            if (!string.IsNullOrWhiteSpace(initialRole))
            {
                var role = GetMockRoles().FirstOrDefault(r => r.Id == initialRole);
                if (role != null)
                {
                    newSMSUser.Qualifications.Add(role.Name); // Use role name, not ID
                }
            }

            await _userRepository.AddSMSUserAsync(newSMSUser);
            
            // Refresh dashboard data
            await _refreshService.RefreshDashboardsAfterUserOperation();

            TempData["SuccessMessage"] = $"SMS User '{fullName}' created successfully with Employee ID: {newSMSUser.EmployeeId}";
            
            return RedirectToPage();
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error creating user: {ex.Message}";
            return RedirectToPage();
        }
    }

    // Edit User Handler - Fixed method name
    public async Task<IActionResult> OnPostEditUserAsync(string userId, string fullName, string email, string department, bool isActive)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(fullName) || 
                string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(department))
            {
                TempData["ErrorMessage"] = "All fields are required for editing.";
                return RedirectToPage();
            }

            // Try to find user in both SMS and Application users
            var smsUser = await _userRepository.GetSMSUserByIdAsync(userId);
            var appUser = await _userRepository.GetApplicationUserByIdAsync(userId);

            if (smsUser != null)
            {
                // Update SMS User
                var nameParts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                smsUser.FirstName = nameParts.FirstOrDefault() ?? fullName;
                smsUser.LastName = nameParts.Skip(1).FirstOrDefault() ?? "";
                smsUser.Email = email;
                smsUser.Department = department;
                smsUser.IsActive = isActive;

                await _userRepository.UpdateSMSUserAsync(smsUser);
                TempData["SuccessMessage"] = $"SMS User '{fullName}' updated successfully.";
            }
            else if (appUser != null)
            {
                // Update Application User
                appUser.Email = email;
                appUser.Username = email;
                appUser.FirstName = fullName.Split(' ').FirstOrDefault() ?? fullName;
                appUser.LastName = fullName.Split(' ').Skip(1).FirstOrDefault() ?? "";
                appUser.Department = department;
                appUser.IsActive = isActive;

                await _userRepository.UpdateApplicationUserAsync(appUser);
                TempData["SuccessMessage"] = $"Application User '{fullName}' updated successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToPage();
            }

            // Refresh dashboard data
            await _refreshService.RefreshDashboardsAfterUserOperation();
            
            return RedirectToPage();
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error updating user: {ex.Message}";
            return RedirectToPage();
        }
    }

    // Delete User Handler - Fixed method name
    public async Task<IActionResult> OnPostDeleteUserAsync(string userId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                TempData["ErrorMessage"] = "User ID is required for deletion.";
                return RedirectToPage();
            }

            // Try to find user in both SMS and Application users
            var smsUser = await _userRepository.GetSMSUserByIdAsync(userId);
            var appUser = await _userRepository.GetApplicationUserByIdAsync(userId);

            if (smsUser != null)
            {
                await _userRepository.DeleteSMSUserAsync(userId);
                TempData["SuccessMessage"] = $"SMS User '{smsUser.DisplayName}' deleted successfully.";
            }
            else if (appUser != null)
            {
                // Check if user is the last admin
                if (appUser.ApplicationRoles.Any(r => r.Contains("Administrator")) || appUser.ApplicationRoles.Any(r => r.Contains("SystemManager")))
                {
                    var allUsers = await _userRepository.GetAllApplicationUsersAsync();
                    var adminCount = allUsers.Count(u => u.IsActive && (u.ApplicationRoles.Any(r => r.Contains("Administrator")) || u.ApplicationRoles.Any(r => r.Contains("SystemManager"))));
                    
                    if (adminCount <= 1)
                    {
                        TempData["ErrorMessage"] = "Cannot delete the last System Administrator. Assign another user as administrator first.";
                        return RedirectToPage();
                    }
                }

                await _userRepository.DeleteApplicationUserAsync(userId);
                TempData["SuccessMessage"] = $"Application User '{appUser.DisplayName}' deleted successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToPage();
            }

            // Refresh dashboard data
            await _refreshService.RefreshDashboardsAfterUserOperation();
            
            return RedirectToPage();
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error deleting user: {ex.Message}";
            return RedirectToPage();
        }
    }

    // Update User Roles Handler
    public async Task<IActionResult> OnPostUpdateUserRolesAsync(string userId, string[] selectedRoles)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                TempData["ErrorMessage"] = "User ID is required.";
                return RedirectToPage();
            }

            selectedRoles = selectedRoles ?? Array.Empty<string>();

            // Try to find user in both SMS and Application users
            var smsUser = await _userRepository.GetSMSUserByIdAsync(userId);
            var appUser = await _userRepository.GetApplicationUserByIdAsync(userId);

            if (smsUser != null)
            {
                // Update SMS User qualifications (similar to roles)
                var roleNames = selectedRoles.Select(roleId => 
                    GetMockRoles().FirstOrDefault(r => r.Id == roleId)?.Name ?? "Unknown")
                    .Where(name => name != "Unknown").ToList();

                smsUser.Qualifications = roleNames;
                await _userRepository.UpdateSMSUserAsync(smsUser);

                TempData["SuccessMessage"] = selectedRoles.Any() 
                    ? $"Qualifications updated for SMS user '{smsUser.DisplayName}'. Assigned: {string.Join(", ", roleNames)}."
                    : $"All qualifications removed from SMS user '{smsUser.DisplayName}'.";
            }
            else if (appUser != null)
            {
                // Check if removing admin role from last admin
                var wasAdmin = appUser.ApplicationRoles.Any(r => r.Contains("Administrator") || r.Contains("SystemManager"));
                var willBeAdmin = selectedRoles.Any(r => r.Contains("Administrator") || r.Contains("SystemManager"));

                if (wasAdmin && !willBeAdmin)
                {
                    var allUsers = await _userRepository.GetAllApplicationUsersAsync();
                    var adminCount = allUsers.Count(u => u.IsActive && (u.ApplicationRoles.Any(r => r.Contains("Administrator")) || u.ApplicationRoles.Any(r => r.Contains("SystemManager"))));
                    
                    if (adminCount <= 1)
                    {
                        TempData["ErrorMessage"] = "Cannot remove System Administrator role from the last administrator. Assign another user as administrator first.";
                        return RedirectToPage();
                    }
                }

                var roleNames = selectedRoles.Select(roleId => 
                    GetMockRoles().FirstOrDefault(r => r.Id == roleId)?.Name ?? "Unknown")
                    .Where(name => name != "Unknown").ToList();

                appUser.ApplicationRoles = roleNames;
                await _userRepository.UpdateApplicationUserAsync(appUser);

                TempData["SuccessMessage"] = selectedRoles.Any() 
                    ? $"Roles updated for application user '{appUser.DisplayName}'. Assigned: {string.Join(", ", roleNames)}."
                    : $"All roles removed from application user '{appUser.DisplayName}'.";
            }
            else
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToPage();
            }

            // Refresh dashboard data
            await _refreshService.RefreshDashboardsAfterUserOperation();
            
            return RedirectToPage();
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error updating user roles: {ex.Message}";
            return RedirectToPage();
        }
    }

    // Assign Role to Multiple Users Handler
    public async Task<IActionResult> OnPostAssignRoleToMultipleUsersAsync(string roleId, string[] selectedUsers)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(roleId))
            {
                TempData["ErrorMessage"] = "Role ID is required.";
                return RedirectToPage();
            }

            selectedUsers = selectedUsers ?? Array.Empty<string>();

            if (!selectedUsers.Any())
            {
                TempData["ErrorMessage"] = "Please select at least one user.";
                return RedirectToPage();
            }

            var role = GetMockRoles().FirstOrDefault(r => r.Id == roleId);
            if (role == null)
            {
                TempData["ErrorMessage"] = "Role not found.";
                return RedirectToPage();
            }

            var updatedCount = 0;
            foreach (var userId in selectedUsers)
            {
                var smsUser = await _userRepository.GetSMSUserByIdAsync(userId);
                var appUser = await _userRepository.GetApplicationUserByIdAsync(userId);

                if (smsUser != null && !smsUser.Qualifications.Contains(role.Name))
                {
                    smsUser.Qualifications.Add(role.Name);
                    await _userRepository.UpdateSMSUserAsync(smsUser);
                    updatedCount++;
                }
                else if (appUser != null && !appUser.ApplicationRoles.Contains(role.Name))
                {
                    appUser.ApplicationRoles.Add(role.Name);
                    await _userRepository.UpdateApplicationUserAsync(appUser);
                    updatedCount++;
                }
            }

            // Refresh dashboard data
            await _refreshService.RefreshDashboardsAfterUserOperation();

            TempData["SuccessMessage"] = $"Role '{role.Name}' assigned to {updatedCount} user(s).";
            
            return RedirectToPage();
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error assigning role to users: {ex.Message}";
            return RedirectToPage();
        }
    }

    private async Task LoadUserDataAsync()
    {
        // Load BOTH ApplicationUsers and SMS Users
        var appUsers = await _userRepository.GetAllApplicationUsersAsync();
        var smsUsers = await _userRepository.GetAllSMSUsersAsync();
        
        var combinedUsers = new List<UserAccount>();

        // Add Application Users
        combinedUsers.AddRange(appUsers.Select(u => new UserAccount
        {
            Id = u.Id,
            Name = u.DisplayName,
            Email = u.Email,
            AssignedRoles = u.ApplicationRoles.ToArray(),
            Department = u.Department,
            IsActive = u.IsActive,
            LastLogin = u.LastLoginDate ?? DateTime.MinValue,
            UserType = "Application"
        }));

        // Add SMS Users  
        combinedUsers.AddRange(smsUsers.Select(u => new UserAccount
        {
            Id = u.Id,
            Name = u.DisplayName,
            Email = u.Email,
            AssignedRoles = u.Qualifications.ToArray(), // SMS users use qualifications as roles
            Department = u.Department,
            IsActive = u.IsActive,
            LastLogin = u.LastActivityDate ?? DateTime.MinValue,
            UserType = "SMS"
        }));

        Users = combinedUsers.OrderBy(u => u.Name).ToList();
        
        Roles = GetMockRoles();
        
        // Map to local type
        var serviceStats = await _dashboardService.GetUserManagementStatsAsync();
        Stats = new UserManagementStats
        {
            TotalUsers = serviceStats.TotalUsers,
            ActiveUsers = serviceStats.ActiveUsers,
            InactiveUsers = serviceStats.InactiveUsers,
            AdminUsers = serviceStats.AdminUsers,
            LastLogin = serviceStats.LastLogin
        };
    }

    private List<UserRole> GetMockRoles()
    {
        return new List<UserRole>
        {
            new() { Id = "1", Name = "System Administrator", Description = "Full system access and configuration", Permissions = new[] { "All Permissions" } },
            new() { Id = "2", Name = "Safety Manager", Description = "Manage safety programs and oversight", Permissions = new[] { "View All Data", "Manage Audits", "Generate Reports", "Approve Actions" } },
            new() { Id = "3", Name = "Safety Analyst", Description = "Analyze data and support investigations", Permissions = new[] { "View Data", "Create Reports", "Edit SPIs", "Manage Hazards" } },
            new() { Id = "4", Name = "Audit Coordinator", Description = "Schedule and manage audit activities", Permissions = new[] { "View Audits", "Schedule Audits", "Assign Teams", "Track Progress" } },
            new() { Id = "5", Name = "Safety Investigator", Description = "Investigate incidents and hazards", Permissions = new[] { "View Incidents", "Create Investigations", "Update Findings", "Generate Reports" } },
            new() { Id = "6", Name = "Compliance Officer", Description = "Ensure regulatory compliance", Permissions = new[] { "View Compliance", "Generate Regulatory Reports", "Manage Documentation" } }
        };
    }

    private string GenerateEmployeeId(string department)
    {
        var prefix = department switch
        {
            "Safety" or "Safety Management" => "SMS",
            "Operations" or "Airport Operations" => "OPS", 
            "IT" or "Information Technology" => "IT",
            "Quality Assurance" => "QA",
            "Regulatory" => "REG",
            "Management" => "MGT",
            _ => "STF"
        };

        var random = new Random();
        var number = random.Next(100, 999);
        return $"{prefix}{number}";
    }

    private string HashPassword(string password)
    {
        return Convert.ToBase64String(Encoding.UTF8.GetBytes($"SALT_{password}_HASH"));
    }
}

public class UserAccount
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string[] AssignedRoles { get; set; } = Array.Empty<string>();
    public string Department { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime LastLogin { get; set; }
    public string UserType { get; set; } = string.Empty; // "Application" or "SMS"
}

public class UserRole
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string[] Permissions { get; set; } = Array.Empty<string>();
}

public class UserManagementStats
{
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int InactiveUsers { get; set; }
    public int AdminUsers { get; set; }
    public DateTime LastLogin { get; set; }
}