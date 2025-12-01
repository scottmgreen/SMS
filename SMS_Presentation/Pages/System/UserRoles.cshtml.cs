using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SMS_Application.Interfaces;
using SMS_Application.Messaging.Commands;
using SMS_Application.Messaging.Queries;
using SMS_Domain.Entities;
using SMS_Domain.ValueObjects;
using SMS_Shared.Common;

namespace SMS_Presentation.Pages.System;

/// <summary>
/// User Role Management - Full CRUD for SMS User Roles with Permission Management
/// </summary>
public class UserRolesModel : PageModel
{
    private readonly IMediator _mediator;
    private readonly ILogger<UserRolesModel> _logger;

    public UserRolesModel(IMediator mediator, ILogger<UserRolesModel> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region Properties

    public IEnumerable<SMSUserRole> UserRoles { get; private set; } = new List<SMSUserRole>();
    public SMSUserRole? CurrentRole { get; set; }
    public bool IsEditMode { get; set; }

    // Predefined SMS Modules
    public static readonly string[] SMSModules = 
    {
        "SMS_Assurance", "SMS_Policy", "SMS_Promotion", "SMS_RiskManagement","SMS_System"
    };

    [TempData]
    public string? SuccessMessage { get; set; }

    [TempData] 
    public string? ErrorMessage { get; set; }

    #endregion

    #region Page Handlers

    public async Task<IActionResult> OnGetAsync(string? code = null, string? action = null)
    {
        await LoadDataAsync();
        
        // If code is provided, go into edit mode
        if (!string.IsNullOrWhiteSpace(code))
        {
            return await OnGetEditAsync(code);
        }
        
        if (action == "create")
        {
            IsEditMode = false;
            CurrentRole = null;
        }
        
        return Page();
    }

    public async Task<IActionResult> OnGetEditAsync(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            ErrorMessage = "Role code is required.";
            return RedirectToPage();
        }

        try
        {
            var getRoleQuery = new GetSMSUserRoleByIdQuery(code);
            var roleResult = await _mediator.SendAsync(getRoleQuery, CancellationToken.None);
            
            if (roleResult.IsFailure)
            {
                ErrorMessage = "Role not found.";
                return RedirectToPage();
            }

            CurrentRole = roleResult.Value;
            IsEditMode = true;
            await LoadDataAsync();
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading role for edit: {RoleCode}", code);
            ErrorMessage = "Error loading role. Please try again.";
            return RedirectToPage();
        }
    }

    #endregion

    #region Create/Update Handlers

    public async Task<IActionResult> OnPostCreateAsync(
        string roleName,
        Dictionary<string, bool> createPermissions,
        Dictionary<string, bool> readPermissions,
        Dictionary<string, bool> updatePermissions,
        Dictionary<string, bool> deletePermissions)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(roleName))
            {
                ErrorMessage = "Role name is required.";
                await LoadDataAsync();
                return Page();
            }

            // Create role entity
            var roleCode = $"ROLE-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}";
            var roleId = new SMSUserRoleID(roleCode);
            var role = new SMSUserRole(roleId)
            {
                Code = roleCode,
                Name = roleName,
                Permissions = new List<SMSUserRolePermission>()
            };

            // Create permissions for each module
            foreach (var module in SMSModules)
            {
                var permissionCode = $"PERM-{roleCode}-{module.Replace(" ", "")}";
                var permissionId = new SMSUserRolePermissionID(permissionCode);
                var permission = new SMSUserRolePermission(permissionId)
                {
                    Code = permissionCode,
                    SMSUserRoleCode = roleCode,
                    SMSModule = module,
                    Create = createPermissions.ContainsKey(module) && createPermissions[module],
                    Read = readPermissions.ContainsKey(module) && readPermissions[module],
                    Update = updatePermissions.ContainsKey(module) && updatePermissions[module],
                    Delete = deletePermissions.ContainsKey(module) && deletePermissions[module]
                };
                role.Permissions.Add(permission);
            }

            var command = new CreateSMSUserRoleCommand(role);
            var result = await _mediator.SendAsync(command, CancellationToken.None);

            if (result.IsSuccess)
            {
                SuccessMessage = $"User role '{roleName}' created successfully.";
                return RedirectToPage();
            }
            else
            {
                ErrorMessage = result.Error?.Message ?? "Failed to create user role.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user role");
            ErrorMessage = "Error creating user role. Please try again.";
        }

        await LoadDataAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostUpdateAsync(
        string roleCode,
        string roleName,
        Dictionary<string, bool> createPermissions,
        Dictionary<string, bool> readPermissions,
        Dictionary<string, bool> updatePermissions,
        Dictionary<string, bool> deletePermissions)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(roleCode) || string.IsNullOrWhiteSpace(roleName))
            {
                ErrorMessage = "Role code and name are required.";
                return await OnGetEditAsync(roleCode);
            }

            var getRoleQuery = new GetSMSUserRoleByIdQuery(roleCode);
            var roleResult = await _mediator.SendAsync(getRoleQuery, CancellationToken.None);
            
            if (roleResult.IsFailure)
            {
                ErrorMessage = "Role not found.";
                return RedirectToPage();
            }

            var role = roleResult.Value;
            role.Name = roleName;

            // Update permissions for each module
            foreach (var module in SMSModules)
            {
                var existingPermission = role.Permissions?.FirstOrDefault(p => p.SMSModule == module);
                if (existingPermission != null)
                {
                    existingPermission.Create = createPermissions.ContainsKey(module) && createPermissions[module];
                    existingPermission.Read = readPermissions.ContainsKey(module) && readPermissions[module];
                    existingPermission.Update = updatePermissions.ContainsKey(module) && updatePermissions[module];
                    existingPermission.Delete = deletePermissions.ContainsKey(module) && deletePermissions[module];
                }
                else
                {
                    // Create new permission if it doesn't exist
                    var permissionCode = $"PERM-{roleCode}-{module.Replace(" ", "")}";
                    var permissionId = new SMSUserRolePermissionID(permissionCode);
                    var permission = new SMSUserRolePermission(permissionId)
                    {
                        Code = permissionCode,
                        SMSUserRoleCode = roleCode,
                        SMSModule = module,
                        Create = createPermissions.ContainsKey(module) && createPermissions[module],
                        Read = readPermissions.ContainsKey(module) && readPermissions[module],
                        Update = updatePermissions.ContainsKey(module) && updatePermissions[module],
                        Delete = deletePermissions.ContainsKey(module) && deletePermissions[module]
                    };
                    role.Permissions ??= new List<SMSUserRolePermission>();
                    role.Permissions.Add(permission);
                }
            }

            var updateCommand = new UpdateSMSUserRoleCommand(role);
            var result = await _mediator.SendAsync(updateCommand, CancellationToken.None);

            if (result.IsSuccess)
            {
                SuccessMessage = $"User role '{roleName}' updated successfully.";
                return RedirectToPage();
            }
            else
            {
                ErrorMessage = result.Error?.Message ?? "Failed to update user role.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user role: {RoleCode}", roleCode);
            ErrorMessage = "Error updating user role. Please try again.";
        }

        return await OnGetEditAsync(roleCode);
    }

    #endregion

    #region Delete Handler

    public async Task<IActionResult> OnPostDeleteAsync(string roleCode)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(roleCode))
            {
                ErrorMessage = "Role code is required for deletion.";
                return RedirectToPage();
            }

            var roleId = new SMSUserRoleID(roleCode);
            var command = new DeleteSMSUserRoleCommand(roleId);
            var result = await _mediator.SendAsync(command, CancellationToken.None);

            if (result.IsSuccess)
            {
                SuccessMessage = "User role deleted successfully.";
            }
            else
            {
                ErrorMessage = result.Error?.Message ?? "Failed to delete user role.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user role: {RoleCode}", roleCode);
            ErrorMessage = "Error deleting user role. Please try again.";
        }

        return RedirectToPage();
    }

    #endregion

    #region Helper Methods

    private async Task LoadDataAsync()
    {
        try
        {
            // Load User Roles
            var userRolesQuery = new GetAllSMSUserRolesQuery();
            var userRolesResult = await _mediator.SendAsync(userRolesQuery, CancellationToken.None);
            UserRoles = userRolesResult.IsSuccess ? userRolesResult.Value ?? new List<SMSUserRole>() : new List<SMSUserRole>();

            _logger.LogInformation("Loaded {RoleCount} user roles", UserRoles.Count());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading data");
            throw;
        }
    }

    // Helper method to get permission value for a module
    public bool GetPermissionValue(string module, string permissionType)
    {
        if (CurrentRole?.Permissions == null) return false;
        
        var permission = CurrentRole.Permissions.FirstOrDefault(p => p.SMSModule == module);
        if (permission == null) return false;

        return permissionType.ToLower() switch
        {
            "create" => permission.Create,
            "read" => permission.Read,
            "update" => permission.Update,
            "delete" => permission.Delete,
            _ => false
        };
    }

    #endregion
}