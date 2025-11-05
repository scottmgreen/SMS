using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PDXSMS.Services;
using PDXSMS.UseCases.Queries.Organization;

namespace PDXSMS_Presentation.Pages.System;

/// <summary>
/// Roles and Permissions Management - Configure user roles, permissions, and access control
/// </summary>
public class RolesAndPermissionsModel : PageModel
{
    private readonly DashboardDataService _dashboardService;
    private readonly UniversalJsonDataService _jsonDataService;

    public List<SystemRole> Roles { get; set; } = new();
    public List<Permission> AvailablePermissions { get; set; } = new();
    public List<UserRoleAssignment> UserAssignments { get; set; } = new();
    public RoleManagementStats Stats { get; set; } = new();

    public RolesAndPermissionsModel(DashboardDataService dashboardService, UniversalJsonDataService jsonDataService)
    {
        _dashboardService = dashboardService;
        _jsonDataService = jsonDataService;
    }

    public async Task OnGetAsync()
    {
        ViewData["Title"] = "Roles & Permissions - System Administration";
        await LoadRoleDataAsync();
    }

    // Create Role Handler
    public async Task<IActionResult> OnPostCreateRoleAsync(string roleName, string roleDescription, bool isActive = true)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(roleName) || string.IsNullOrWhiteSpace(roleDescription))
            {
                TempData["ErrorMessage"] = "Role name and description are required.";
                return RedirectToPage();
            }

            // Create new role in JSON data service
            var newRole = new SMSOrganizationalRoleDto
            {
                Id = Guid.NewGuid().ToString(),
                Name = roleName,
                Description = roleDescription,
                RoleType = "Custom",
                HierarchyLevel = 10,
                KeyResponsibilities = new List<string> { "Custom role responsibilities" },
                Permissions = new List<string>(),
                AssignedUsersCount = 0,
                IsActive = isActive,
                CreatedDate = DateTime.UtcNow
            };

            _jsonDataService.SaveRole(newRole);

            TempData["SuccessMessage"] = $"Role '{roleName}' created successfully.";

            return RedirectToPage();
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error creating role: {ex.Message}";
            return RedirectToPage();
        }
    }

    // Edit Role Handler
    public async Task<IActionResult> OnPostEditRoleAsync(string roleId, string roleName, string roleDescription)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(roleId) || string.IsNullOrWhiteSpace(roleName) || string.IsNullOrWhiteSpace(roleDescription))
            {
                TempData["ErrorMessage"] = "All fields are required for editing.";
                return RedirectToPage();
            }

            var role = _jsonDataService.GetRoleById(roleId);
            if (role == null)
            {
                TempData["ErrorMessage"] = "Role not found.";
                return RedirectToPage();
            }

            role.Name = roleName;
            role.Description = roleDescription;
            _jsonDataService.SaveRole(role);

            TempData["SuccessMessage"] = $"Role '{roleName}' updated successfully.";

            return RedirectToPage();
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error updating role: {ex.Message}";
            return RedirectToPage();
        }
    }

    // Delete Role Handler
    public async Task<IActionResult> OnPostDeleteRoleAsync(string roleId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(roleId))
            {
                TempData["ErrorMessage"] = "Role ID is required for deletion.";
                return RedirectToPage();
            }

            var role = _jsonDataService.GetRoleById(roleId);
            if (role == null)
            {
                TempData["ErrorMessage"] = "Role not found.";
                return RedirectToPage();
            }

            if (role.AssignedUsersCount > 0)
            {
                TempData["ErrorMessage"] = $"Cannot delete role '{role.Name}' because it is assigned to {role.AssignedUsersCount} user(s). Remove all user assignments first.";
                return RedirectToPage();
            }

            role.IsActive = false;
            _jsonDataService.SaveRole(role);

            TempData["SuccessMessage"] = $"Role '{role.Name}' deleted successfully.";

            return RedirectToPage();
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error deleting role: {ex.Message}";
            return RedirectToPage();
        }
    }

    // Update Role Permissions Handler
    public async Task<IActionResult> OnPostUpdateRolePermissionsAsync(string roleId, string[] selectedPermissions)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(roleId))
            {
                TempData["ErrorMessage"] = "Role ID is required.";
                return RedirectToPage();
            }

            selectedPermissions = selectedPermissions ?? Array.Empty<string>();

            var role = _jsonDataService.GetRoleById(roleId);
            if (role == null)
            {
                TempData["ErrorMessage"] = "Role not found.";
                return RedirectToPage();
            }

            role.Permissions = selectedPermissions.ToList();
            _jsonDataService.SaveRole(role);

            TempData["SuccessMessage"] = $"Permissions for role '{role.Name}' updated successfully. {selectedPermissions.Length} permission(s) assigned.";

            return RedirectToPage();
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error updating permissions: {ex.Message}";
            return RedirectToPage();
        }
    }

    private async Task LoadRoleDataAsync()
    {
        var jsonRoles = _jsonDataService.GetAllRoles();
        
        Roles = jsonRoles.Select(r => new SystemRole
        {
            Id = r.Id,
            Name = r.Name,
            Description = r.Description,
            IsActive = r.IsActive,
            CreatedDate = r.CreatedDate,
            UserCount = r.AssignedUsersCount,
            PermissionIds = r.Permissions.ToArray()
        }).ToList();
        
        AvailablePermissions = GetMockPermissions();
        UserAssignments = GetMockUserAssignments();
        
        // Map to local type
        var serviceStats = await _dashboardService.GetRoleManagementStatsAsync();
        Stats = new RoleManagementStats
        {
            TotalRoles = serviceStats.TotalRoles,
            ActiveRoles = serviceStats.ActiveRoles,
            TotalPermissions = serviceStats.TotalPermissions,
            UsersWithRoles = serviceStats.UsersWithRoles
        };
    }

    private List<Permission> GetMockPermissions()
    {
        return new List<Permission>
        {
            new() { Id = "1", Name = "View Dashboard", Category = "General", Description = "Access main dashboard and overview" },
            new() { Id = "2", Name = "Manage Users", Category = "Administration", Description = "Create, edit, and delete user accounts" },
            new() { Id = "3", Name = "Manage Audits", Category = "Safety Assurance", Description = "Schedule and conduct safety audits" },
            new() { Id = "4", Name = "View Reports", Category = "Reporting", Description = "Access and view system reports" },
            new() { Id = "5", Name = "Create Reports", Category = "Reporting", Description = "Generate new reports and analysis" },
            new() { Id = "6", Name = "Manage Compliance", Category = "Compliance", Description = "Handle regulatory compliance activities" },
            new() { Id = "7", Name = "Manage Hazards", Category = "Risk Management", Description = "Process and manage hazard reports" },
            new() { Id = "8", Name = "System Configuration", Category = "Administration", Description = "Configure system settings and parameters" },
            new() { Id = "9", Name = "Manage Roles", Category = "Administration", Description = "Create and modify user roles and permissions" },
            new() { Id = "10", Name = "System Monitoring", Category = "Administration", Description = "Monitor system health and performance" }
        };
    }

    private List<UserRoleAssignment> GetMockUserAssignments()
    {
        return new List<UserRoleAssignment>
        {
            new() { UserId = "1", UserName = "System Administrator", RoleId = "1", RoleName = "System Administrator", AssignedDate = DateTime.Now.AddDays(-30) },
            new() { UserId = "2", UserName = "Sarah Johnson", RoleId = "2", RoleName = "Safety Manager", AssignedDate = DateTime.Now.AddDays(-20) },
            new() { UserId = "3", UserName = "Mike Chen", RoleId = "3", RoleName = "Safety Officer", AssignedDate = DateTime.Now.AddDays(-15) }
        };
    }
}

public class SystemRole
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
    public int UserCount { get; set; }
    public string[] PermissionIds { get; set; } = Array.Empty<string>();
}

public class Permission
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class UserRoleAssignment
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string RoleId { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public DateTime AssignedDate { get; set; }
}

public class RoleManagementStats
{
    public int TotalRoles { get; set; }
    public int ActiveRoles { get; set; }
    public int TotalPermissions { get; set; }
    public int UsersWithRoles { get; set; }
}