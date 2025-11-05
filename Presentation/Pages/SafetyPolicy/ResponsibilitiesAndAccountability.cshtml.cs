using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PDXSMS.Services;
using System.Text;
using System.Text.Json;

namespace PDXSMS_Presentation.Pages.SafetyPolicy;

// Updated stats class to focus on SMS and Stakeholder users only
public class UserManagementStats
{
    public int TotalSMSUsers { get; set; }
    public int TotalStakeholderUsers { get; set; }
    public int TotalGroups { get; set; }
    public int ActiveSMSUsers { get; set; }
    public int ActiveStakeholderUsers { get; set; }
    public int ActiveGroups { get; set; }
    public int SMSTeams { get; set; }
    public int StakeholderGroups { get; set; }
}

// Input models for create forms (removed Application User inputs)
public class CreateSMSUserInput
{
    public string DisplayName { get; set; } = string.Empty;
    public string EmployeeId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string SelectedRoles { get; set; } = string.Empty;
}

public class CreateStakeholderUserInput
{
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string OrganizationName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string StakeholderType { get; set; } = string.Empty;
    public string StakeholderCategory { get; set; } = string.Empty;
    public string SelectedGroups { get; set; } = string.Empty;
}

// ?? NEW: Input model for creating stakeholder groups
public class CreateStakeholderGroupInput
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string StakeholderType { get; set; } = string.Empty;
    public string Purpose { get; set; } = string.Empty;
}

/// <summary>
/// SMS Organizational Management - Focus on SMS Users and Stakeholders only
/// Application Users are managed under System ? User Management
/// </summary>
public class ResponsibilitiesAndAccountabilityModel : PageModel
{
    private readonly DashboardDataService _dashboardService;
    private readonly UniversalUserRepository _userRepository;
    private readonly DashboardRefreshService _refreshService;

    public UserManagementStats Stats { get; set; } = new();
    
    // User collections for display (removed ApplicationUsers)
    public List<SMSOrganizationalUserData> SMSUsers { get; set; } = new();
    public List<StakeholderUserData> StakeholderUsers { get; set; } = new();
    public List<GroupData> AllGroups { get; set; } = new();

    // Input models for binding
    [BindProperty]
    public CreateSMSUserInput CreateSMSUser { get; set; } = new();

    [BindProperty]
    public CreateStakeholderUserInput CreateStakeholderUser { get; set; } = new();

    // ?? NEW: Binding property for stakeholder group creation
    [BindProperty]
    public CreateStakeholderGroupInput CreateStakeholderGroup { get; set; } = new();

    public ResponsibilitiesAndAccountabilityModel(
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
        ViewData["Title"] = "SMS Organizational Management";
        await LoadAllDataAsync();
    }

    #region SMS User CRUD Handlers

    public async Task<IActionResult> OnPostCreateSMSUserAsync()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(CreateSMSUser.DisplayName) || 
                string.IsNullOrWhiteSpace(CreateSMSUser.EmployeeId) || 
                string.IsNullOrWhiteSpace(CreateSMSUser.Email))
            {
                TempData["ErrorMessage"] = "Display name, employee ID, and email are required.";
                return RedirectToPage();
            }

            // Check if employee ID or email already exists
            var smsUsers = await _userRepository.GetAllSMSUsersAsync();
            if (smsUsers.Any(u => u.EmployeeId == CreateSMSUser.EmployeeId || u.Email.Equals(CreateSMSUser.Email, StringComparison.OrdinalIgnoreCase)))
            {
                TempData["ErrorMessage"] = $"A user with employee ID '{CreateSMSUser.EmployeeId}' or email '{CreateSMSUser.Email}' already exists.";
                return RedirectToPage();
            }

            var displayNameParts = CreateSMSUser.DisplayName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var firstName = displayNameParts.FirstOrDefault() ?? CreateSMSUser.DisplayName;
            var lastName = displayNameParts.Skip(1).FirstOrDefault() ?? "";

            var newUser = new SMSOrganizationalUserData
            {
                Id = Guid.NewGuid().ToString(),
                EmployeeId = CreateSMSUser.EmployeeId,
                Email = CreateSMSUser.Email,
                FirstName = firstName,
                LastName = lastName,
                Phone = CreateSMSUser.Phone,
                Department = CreateSMSUser.Department,
                JobTitle = CreateSMSUser.JobTitle,
                IsEmployee = true,
                EmploymentStatus = "Active",
                SMSTeamMemberships = new List<string>(),
                Qualifications = new List<string>(),
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };

            await _userRepository.AddSMSUserAsync(newUser);
            await _refreshService.RefreshDashboardsAfterUserOperation();

            TempData["SuccessMessage"] = $"SMS user '{CreateSMSUser.DisplayName}' created successfully.";
            return RedirectToPage();
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error creating SMS user: {ex.Message}";
            return RedirectToPage();
        }
    }

    public async Task<IActionResult> OnPostEditSMSUserAsync(string userId, string displayName, string email, string department, string jobTitle, string employeeId, string? phone)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(displayName) || 
                string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(employeeId))
            {
                TempData["ErrorMessage"] = "User ID, display name, email, and employee ID are required.";
                return RedirectToPage();
            }

            var user = await _userRepository.GetSMSUserByIdAsync(userId);
            if (user == null)
            {
                TempData["ErrorMessage"] = "SMS user not found.";
                return RedirectToPage();
            }

            var nameParts = displayName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            user.FirstName = nameParts.FirstOrDefault() ?? displayName;
            user.LastName = nameParts.Skip(1).FirstOrDefault() ?? "";
            user.Email = email;
            user.Department = department;
            user.JobTitle = jobTitle;
            user.EmployeeId = employeeId;
            user.Phone = phone;

            await _userRepository.UpdateSMSUserAsync(user);
            await _refreshService.RefreshDashboardsAfterUserOperation();

            TempData["SuccessMessage"] = $"SMS user '{displayName}' updated successfully.";
            return RedirectToPage();
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error updating SMS user: {ex.Message}";
            return RedirectToPage();
        }
    }

    public async Task<IActionResult> OnPostUpdateSMSUserTeamsAsync(string userId, string[] selectedTeams)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                TempData["ErrorMessage"] = "User ID is required.";
                return RedirectToPage();
            }

            selectedTeams = selectedTeams ?? Array.Empty<string>();

            var user = await _userRepository.GetSMSUserByIdAsync(userId);
            if (user == null)
            {
                TempData["ErrorMessage"] = "SMS user not found.";
                return RedirectToPage();
            }

            user.SMSTeamMemberships = selectedTeams.ToList();
            await _userRepository.UpdateSMSUserAsync(user);
            await _refreshService.RefreshDashboardsAfterUserOperation();

            TempData["SuccessMessage"] = selectedTeams.Any() 
                ? $"Teams updated for SMS user '{user.DisplayName}'. Assigned to: {string.Join(", ", selectedTeams)}."
                : $"All team assignments removed from SMS user '{user.DisplayName}'.";
            
            return RedirectToPage();
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error updating SMS user teams: {ex.Message}";
            return RedirectToPage();
        }
    }

    public async Task<IActionResult> OnPostDeleteSMSUserAsync(string userId)
    {
        try
        {
            var user = await _userRepository.GetSMSUserByIdAsync(userId);
            if (user == null)
            {
                TempData["ErrorMessage"] = "SMS user not found.";
                return RedirectToPage();
            }

            await _userRepository.DeleteSMSUserAsync(userId);
            await _refreshService.RefreshDashboardsAfterUserOperation();

            TempData["SuccessMessage"] = $"SMS user '{user.DisplayName}' deleted successfully.";
            return RedirectToPage();
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error deleting SMS user: {ex.Message}";
            return RedirectToPage();
        }
    }

    #endregion

    #region Stakeholder User Handlers

    public async Task<IActionResult> OnPostCreateStakeholderUserAsync()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(CreateStakeholderUser.DisplayName) || 
                string.IsNullOrWhiteSpace(CreateStakeholderUser.Email))
            {
                TempData["ErrorMessage"] = "Display name and email are required.";
                return RedirectToPage();
            }

            // Check if email already exists
            var stakeholderUsers = await _userRepository.GetAllStakeholderUsersAsync();
            if (stakeholderUsers.Any(u => u.Email.Equals(CreateStakeholderUser.Email, StringComparison.OrdinalIgnoreCase)))
            {
                TempData["ErrorMessage"] = $"A stakeholder user with email '{CreateStakeholderUser.Email}' already exists.";
                return RedirectToPage();
            }

            var displayNameParts = CreateStakeholderUser.DisplayName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var firstName = displayNameParts.FirstOrDefault() ?? CreateStakeholderUser.DisplayName;
            var lastName = displayNameParts.Skip(1).FirstOrDefault() ?? "";

            var newUser = new StakeholderUserData
            {
                Id = Guid.NewGuid().ToString(),
                Email = CreateStakeholderUser.Email,
                FirstName = firstName,
                LastName = lastName,
                Phone = CreateStakeholderUser.Phone,
                OrganizationName = CreateStakeholderUser.OrganizationName,
                StakeholderType = CreateStakeholderUser.StakeholderType,
                StakeholderCategory = CreateStakeholderUser.StakeholderCategory,
                PreferredContactMethod = "Email",
                StakeholderGroups = ParseRoles(CreateStakeholderUser.SelectedGroups),
                InterestAreas = new List<string>(),
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };

            await _userRepository.AddStakeholderUserAsync(newUser);
            await _refreshService.RefreshDashboardsAfterUserOperation();

            TempData["SuccessMessage"] = $"Stakeholder user '{CreateStakeholderUser.DisplayName}' created successfully.";
            return RedirectToPage();
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error creating stakeholder user: {ex.Message}";
            return RedirectToPage();
        }
    }

    public async Task<IActionResult> OnPostDeleteStakeholderUserAsync(string userId)
    {
        try
        {
            var user = await _userRepository.GetStakeholderUserByIdAsync(userId);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Stakeholder user not found.";
                return RedirectToPage();
            }

            await _userRepository.DeleteStakeholderUserAsync(userId);
            await _refreshService.RefreshDashboardsAfterUserOperation();

            TempData["SuccessMessage"] = $"Stakeholder user '{user.DisplayName}' deleted successfully.";
            return RedirectToPage();
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error deleting stakeholder user: {ex.Message}";
            return RedirectToPage();
        }
    }

    public async Task<IActionResult> OnPostUpdateStakeholderUserGroupsAsync(string userId, string[] selectedGroups)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                TempData["ErrorMessage"] = "User ID is required.";
                return RedirectToPage();
            }

            selectedGroups = selectedGroups ?? Array.Empty<string>();

            var user = await _userRepository.GetStakeholderUserByIdAsync(userId);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Stakeholder user not found.";
                return RedirectToPage();
            }

            user.StakeholderGroups = selectedGroups.ToList();
            await _userRepository.UpdateStakeholderUserAsync(user);
            await _refreshService.RefreshDashboardsAfterUserOperation();

            TempData["SuccessMessage"] = selectedGroups.Any() 
                ? $"Groups updated for stakeholder user '{user.DisplayName}'. Assigned to: {string.Join(", ", selectedGroups)}."
                : $"All group assignments removed from stakeholder user '{user.DisplayName}'.";
            
            return RedirectToPage();
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error updating stakeholder user groups: {ex.Message}";
            return RedirectToPage();
        }
    }

    public async Task<IActionResult> OnPostEditStakeholderUserAsync(string userId, string displayName, string email, string organizationName, string? phone, string stakeholderType, string stakeholderCategory, string preferredContactMethod)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(displayName) || 
                string.IsNullOrWhiteSpace(email))
            {
                TempData["ErrorMessage"] = "User ID, display name, and email are required.";
                return RedirectToPage();
            }

            var user = await _userRepository.GetStakeholderUserByIdAsync(userId);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Stakeholder user not found.";
                return RedirectToPage();
            }

            var nameParts = displayName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            user.FirstName = nameParts.FirstOrDefault() ?? displayName;
            user.LastName = nameParts.Skip(1).FirstOrDefault() ?? "";
            user.Email = email;
            user.OrganizationName = organizationName ?? "";
            user.Phone = phone;
            user.StakeholderType = stakeholderType ?? "";
            user.StakeholderCategory = stakeholderCategory ?? "";
            user.PreferredContactMethod = preferredContactMethod ?? "Email";

            await _userRepository.UpdateStakeholderUserAsync(user);
            await _refreshService.RefreshDashboardsAfterUserOperation();

            TempData["SuccessMessage"] = $"Stakeholder user '{displayName}' updated successfully.";
            return RedirectToPage();
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error updating stakeholder user: {ex.Message}";
            return RedirectToPage();
        }
    }

    #endregion

    #region ?? NEW: Stakeholder Group Management Handlers

    public async Task<IActionResult> OnPostCreateStakeholderGroupAsync()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(CreateStakeholderGroup.Name) || 
                string.IsNullOrWhiteSpace(CreateStakeholderGroup.Description) ||
                string.IsNullOrWhiteSpace(CreateStakeholderGroup.Category))
            {
                TempData["ErrorMessage"] = "Group name, description, and category are required.";
                return RedirectToPage();
            }

            // Check if group name already exists
            var groups = await _userRepository.GetAllGroupsAsync();
            if (groups.Any(g => g.Name.Equals(CreateStakeholderGroup.Name, StringComparison.OrdinalIgnoreCase)))
            {
                TempData["ErrorMessage"] = $"A group with name '{CreateStakeholderGroup.Name}' already exists.";
                return RedirectToPage();
            }

            var newGroup = new GroupData
            {
                Id = Guid.NewGuid().ToString(),
                Name = CreateStakeholderGroup.Name,
                Description = CreateStakeholderGroup.Description,
                Type = "StakeholderGroup",
                Category = CreateStakeholderGroup.Category,
                Purpose = CreateStakeholderGroup.Purpose ?? "Stakeholder engagement and consultation",
                IsActive = true,
                MemberCount = 0,
                CreatedDate = DateTime.UtcNow
            };

            await _userRepository.AddGroupAsync(newGroup);
            await _refreshService.RefreshDashboardsAfterUserOperation();

            TempData["SuccessMessage"] = $"Stakeholder group '{CreateStakeholderGroup.Name}' created successfully.";
            return RedirectToPage();
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error creating stakeholder group: {ex.Message}";
            return RedirectToPage();
        }
    }

    public async Task<IActionResult> OnPostEditStakeholderGroupAsync(string groupId, string groupName, string groupDescription, string groupCategory)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(groupId) || string.IsNullOrWhiteSpace(groupName) || 
                string.IsNullOrWhiteSpace(groupDescription) || string.IsNullOrWhiteSpace(groupCategory))
            {
                TempData["ErrorMessage"] = "Group ID, name, description, and category are required.";
                return RedirectToPage();
            }

            var groups = await _userRepository.GetAllGroupsAsync();
            var group = groups.FirstOrDefault(g => g.Id == groupId);
            
            if (group == null)
            {
                TempData["ErrorMessage"] = "Stakeholder group not found.";
                return RedirectToPage();
            }

            group.Name = groupName;
            group.Description = groupDescription;
            group.Category = groupCategory;

            await _userRepository.UpdateGroupAsync(group);
            await _refreshService.RefreshDashboardsAfterUserOperation();

            TempData["SuccessMessage"] = $"Stakeholder group '{groupName}' updated successfully.";
            return RedirectToPage();
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error updating stakeholder group: {ex.Message}";
            return RedirectToPage();
        }
    }

    public async Task<IActionResult> OnPostDeleteStakeholderGroupAsync(string groupId)
    {
        try
        {
            var groups = await _userRepository.GetAllGroupsAsync();
            var group = groups.FirstOrDefault(g => g.Id == groupId);
            
            if (group == null)
            {
                TempData["ErrorMessage"] = "Stakeholder group not found.";
                return RedirectToPage();
            }

            // Check if any stakeholders are assigned to this group
            var stakeholders = await _userRepository.GetAllStakeholderUsersAsync();
            var affectedStakeholders = stakeholders.Where(s => s.StakeholderGroups.Contains(group.Name)).ToList();
            
            if (affectedStakeholders.Any())
            {
                // Remove group from affected stakeholders
                foreach (var stakeholder in affectedStakeholders)
                {
                    stakeholder.StakeholderGroups.Remove(group.Name);
                    await _userRepository.UpdateStakeholderUserAsync(stakeholder);
                }
            }

            await _userRepository.DeleteGroupAsync(groupId);
            await _refreshService.RefreshDashboardsAfterUserOperation();

            var message = affectedStakeholders.Any() 
                ? $"Stakeholder group '{group.Name}' deleted. {affectedStakeholders.Count} stakeholder(s) were unassigned from this group."
                : $"Stakeholder group '{group.Name}' deleted successfully.";

            TempData["SuccessMessage"] = message;
            return RedirectToPage();
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error deleting stakeholder group: {ex.Message}";
            return RedirectToPage();
        }
    }

    public async Task<IActionResult> OnGetGetStakeholderGroupMembersAsync(string groupId)
    {
        try
        {
            var groups = await _userRepository.GetAllGroupsAsync();
            var group = groups.FirstOrDefault(g => g.Id == groupId);
            
            if (group == null)
            {
                return new JsonResult(new { success = false, message = "Group not found" });
            }

            var stakeholders = await _userRepository.GetStakeholderUsersByGroupAsync(group.Name);
            
            var members = stakeholders.Select(s => new
            {
                id = s.Id,
                displayName = s.DisplayName,
                email = s.Email,
                phone = s.Phone,
                organizationName = s.OrganizationName,
                stakeholderType = s.StakeholderType,
                stakeholderCategory = s.StakeholderCategory,
                isActive = s.IsActive
            }).ToList();

            return new JsonResult(new { success = true, members });
        }
        catch (Exception ex)
        {
            return new JsonResult(new { success = false, message = ex.Message });
        }
    }

    #endregion

    #region AJAX Handlers

    public async Task<IActionResult> OnGetUserStatsAsync()
    {
        try
        {
            var unifiedStats = await _dashboardService.GetUnifiedUserArchitectureStatsAsync();
            var groups = await _userRepository.GetAllGroupsAsync();

            var stats = new
            {
                smsUsers = unifiedStats.TotalSMSUsers,
                stakeholderUsers = unifiedStats.TotalStakeholderUsers,
                totalGroups = unifiedStats.TotalGroups,
                smsTeams = groups.Count(g => g.Type == "SMSTeam" && g.IsActive),
                stakeholderGroups = groups.Count(g => g.Type == "StakeholderGroup" && g.IsActive)
            };

            return new JsonResult(new { success = true, stats });
        }
        catch (Exception ex)
        {
            return new JsonResult(new { success = false, error = ex.Message });
        }
    }

    #endregion

    #region Private Methods

    private async Task LoadAllDataAsync()
    {
        // Load only SMS and Stakeholder users (Application Users managed in System menu)
        SMSUsers = await _userRepository.GetAllSMSUsersAsync();
        StakeholderUsers = await _userRepository.GetAllStakeholderUsersAsync();
        AllGroups = await _userRepository.GetAllGroupsAsync();

        // Load statistics
        var unifiedStats = await _dashboardService.GetUnifiedUserArchitectureStatsAsync();
        Stats = new UserManagementStats
        {
            TotalSMSUsers = unifiedStats.TotalSMSUsers,
            TotalStakeholderUsers = unifiedStats.TotalStakeholderUsers,
            TotalGroups = unifiedStats.TotalGroups,
            ActiveSMSUsers = unifiedStats.ActiveSMSUsers,
            ActiveStakeholderUsers = unifiedStats.ActiveStakeholderUsers,
            ActiveGroups = unifiedStats.ActiveGroups,
            SMSTeams = AllGroups.Count(g => g.Type == "SMSTeam" && g.IsActive),
            StakeholderGroups = AllGroups.Count(g => g.Type == "StakeholderGroup" && g.IsActive)
        };
    }

    private List<string> ParseRoles(string rolesString)
    {
        if (string.IsNullOrWhiteSpace(rolesString))
            return new List<string>();

        return rolesString.Split(',', StringSplitOptions.RemoveEmptyEntries)
                         .Select(r => r.Trim())
                         .Where(r => !string.IsNullOrEmpty(r))
                         .ToList();
    }

    #endregion
}