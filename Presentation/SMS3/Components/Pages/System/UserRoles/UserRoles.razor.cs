using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;
using SMS_Application.Messaging.Commands;
using SMS_Application.Messaging.Queries;
using SMS_Application.Interfaces;
using SMS_Domain.Entities;
using SMS_Domain.ValueObjects;
using SMS_Shared.Common;
using SMS3.Components.Pages.System.Components;

namespace SMS3.Components.Pages.System.UserRoles;

public partial class UserRoles : ComponentBase
{
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ILogger<UserRoles> Logger { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Inject] private DialogService DialogService { get; set; } = default!;
    [Inject] private NotificationService NotificationService { get; set; } = default!;

    [Parameter] public string? Code { get; set; }

    // Data Properties
    private List<SMSUserRole> UserRolesList { get; set; } = new();
    private SMSUserRole? CurrentRole { get; set; }
    private bool IsEditMode { get; set; }
    private string? SuccessMessage { get; set; }
    private string? ErrorMessage { get; set; }

    // Predefined SMS Modules
    private static readonly string[] SMSModules = 
    {
        "SMS_Assurance", "SMS_Policy", "SMS_Promotion", "SMS_RiskManagement", "SMS_System"
    };

    // Form Models
    private EditRoleModel editRole = new();

    // Component References
    private RadzenDataGrid<SMSUserRole>? rolesGrid;

    protected override async Task OnInitializedAsync()
    {
        await LoadDataAsync();
        
        // Check if we're in edit mode
        if (!string.IsNullOrWhiteSpace(Code))
        {
            await LoadRoleForEdit(Code);
        }
    }

    protected override async Task OnParametersSetAsync()
    {
        if (!string.IsNullOrWhiteSpace(Code) && !IsEditMode)
        {
            await LoadRoleForEdit(Code);
        }
        else if (string.IsNullOrWhiteSpace(Code) && IsEditMode)
        {
            CancelEdit();
        }
    }

    #region Data Loading

    private async Task LoadDataAsync()
    {
        try
        {
            // Load User Roles
            var userRolesQuery = new GetAllSMSUserRolesQuery();
            var userRolesResult = await Mediator.SendAsync(userRolesQuery, CancellationToken.None);
            UserRolesList = userRolesResult.IsSuccess ? 
                userRolesResult.Value?.ToList() ?? new List<SMSUserRole>() : 
                new List<SMSUserRole>();

            Logger.LogInformation("Loaded {RoleCount} user roles", UserRolesList.Count);

            StateHasChanged();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading user roles data");
            ShowErrorNotification("Error loading data. Please refresh the page.");
        }
    }

    private async Task LoadRoleForEdit(string code)
    {
        try
        {
            var getRoleQuery = new GetSMSUserRoleByIdQuery(code);
            var roleResult = await Mediator.SendAsync(getRoleQuery, CancellationToken.None);
            
            if (roleResult.IsFailure)
            {
                ShowErrorNotification("Role not found.");
                Navigation.NavigateTo("/System/UserRoles/UserRoles");
                return;
            }

            CurrentRole = roleResult.Value;
            IsEditMode = true;
            
            // Populate edit form with permission matrices
            editRole = new EditRoleModel
            {
                RoleName = CurrentRole.Name,
                CreatePermissions = SMSModules.ToDictionary(m => m, m => GetPermissionValue(m, "create")),
                ReadPermissions = SMSModules.ToDictionary(m => m, m => GetPermissionValue(m, "read")),
                UpdatePermissions = SMSModules.ToDictionary(m => m, m => GetPermissionValue(m, "update")),
                DeletePermissions = SMSModules.ToDictionary(m => m, m => GetPermissionValue(m, "delete"))
            };

            StateHasChanged();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading role for edit: {RoleCode}", code);
            ShowErrorNotification("Error loading role. Please try again.");
        }
    }

    #endregion

    #region CRUD Operations

    private async Task ShowCreateDialog()
    {
        var createRole = new CreateRoleModel
        {
            CreatePermissions = SMSModules.ToDictionary(m => m, m => false),
            ReadPermissions = SMSModules.ToDictionary(m => m, m => false),
            UpdatePermissions = SMSModules.ToDictionary(m => m, m => false),
            DeletePermissions = SMSModules.ToDictionary(m => m, m => false)
        };
        
        var result = await DialogService.OpenAsync<CreateUserRoleDialog>("Create User Role",
            new Dictionary<string, object>
            {
                { "Model", createRole },
                { "SMSModules", SMSModules }
            },
            new DialogOptions { Width = "1000px", Height = "700px", Resizable = true, Draggable = true });

        if (result is CreateRoleModel model && model != null)
        {
            await CreateRole(model);
        }
    }

    private async Task CreateRole(CreateRoleModel model)
    {
        try
        {
            // Create role entity
            var roleCode = $"ROLE-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}";
            var roleId = new SMSUserRoleID(roleCode);
            var role = new SMSUserRole(roleId)
            {
                Code = roleCode,
                Name = model.RoleName,
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
                    Create = model.CreatePermissions.ContainsKey(module) && model.CreatePermissions[module],
                    Read = model.ReadPermissions.ContainsKey(module) && model.ReadPermissions[module],
                    Update = model.UpdatePermissions.ContainsKey(module) && model.UpdatePermissions[module],
                    Delete = model.DeletePermissions.ContainsKey(module) && model.DeletePermissions[module]
                };
                role.Permissions.Add(permission);
            }

            var command = new CreateSMSUserRoleCommand(role);
            var result = await Mediator.SendAsync(command, CancellationToken.None);

            if (result.IsSuccess)
            {
                ShowSuccessNotification($"User role '{model.RoleName}' created successfully.");
                await LoadDataAsync();
            }
            else
            {
                ShowErrorNotification(result.Error?.Message ?? "Failed to create user role.");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error creating user role");
            ShowErrorNotification("Error creating user role. Please try again.");
        }
    }

    private async Task EditRole(string roleCode)
    {
        Navigation.NavigateTo($"/System/UserRoles/UserRoles/Edit/{roleCode}");
    }

    private async Task UpdateRole(EditRoleModel model)
    {
        try
        {
            if (CurrentRole == null)
            {
                ShowErrorNotification("No role selected for update.");
                return;
            }

            CurrentRole.Name = model.RoleName;

            // Update permissions for each module
            foreach (var module in SMSModules)
            {
                var existingPermission = CurrentRole.Permissions?.FirstOrDefault(p => p.SMSModule == module);
                if (existingPermission != null)
                {
                    existingPermission.Create = model.CreatePermissions.ContainsKey(module) && model.CreatePermissions[module];
                    existingPermission.Read = model.ReadPermissions.ContainsKey(module) && model.ReadPermissions[module];
                    existingPermission.Update = model.UpdatePermissions.ContainsKey(module) && model.UpdatePermissions[module];
                    existingPermission.Delete = model.DeletePermissions.ContainsKey(module) && model.DeletePermissions[module];
                }
                else
                {
                    // Create new permission if it doesn't exist
                    var permissionCode = $"PERM-{CurrentRole.Code}-{module.Replace(" ", "")}";
                    var permissionId = new SMSUserRolePermissionID(permissionCode);
                    var permission = new SMSUserRolePermission(permissionId)
                    {
                        Code = permissionCode,
                        SMSUserRoleCode = CurrentRole.Code,
                        SMSModule = module,
                        Create = model.CreatePermissions.ContainsKey(module) && model.CreatePermissions[module],
                        Read = model.ReadPermissions.ContainsKey(module) && model.ReadPermissions[module],
                        Update = model.UpdatePermissions.ContainsKey(module) && model.UpdatePermissions[module],
                        Delete = model.DeletePermissions.ContainsKey(module) && model.DeletePermissions[module]
                    };
                    CurrentRole.Permissions ??= new List<SMSUserRolePermission>();
                    CurrentRole.Permissions.Add(permission);
                }
            }

            var updateCommand = new UpdateSMSUserRoleCommand(CurrentRole);
            var result = await Mediator.SendAsync(updateCommand, CancellationToken.None);

            if (result.IsSuccess)
            {
                ShowSuccessNotification($"User role '{model.RoleName}' updated successfully.");
                Navigation.NavigateTo("/System/UserRoles/UserRoles");
            }
            else
            {
                ShowErrorNotification(result.Error?.Message ?? "Failed to update user role.");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error updating user role: {RoleCode}", CurrentRole?.Code);
            ShowErrorNotification("Error updating user role. Please try again.");
        }
    }

    private async Task ShowDeleteDialog(string roleCode, string roleName)
    {
        var result = await DialogService.Confirm($"Are you sure you want to delete the role '{roleName}'?\n\nThis action cannot be undone and may affect users assigned to this role.", 
            "Confirm Delete", 
            new ConfirmOptions 
            { 
                OkButtonText = "Delete", 
                CancelButtonText = "Cancel",
                AutoFocusFirstElement = true
            });

        if (result == true)
        {
            await DeleteRole(roleCode);
        }
    }

    private async Task DeleteRole(string roleCode)
    {
        try
        {
            var roleId = new SMSUserRoleID(roleCode);
            var command = new DeleteSMSUserRoleCommand(roleId);
            var result = await Mediator.SendAsync(command, CancellationToken.None);

            if (result.IsSuccess)
            {
                ShowSuccessNotification("User role deleted successfully.");
                await LoadDataAsync();
            }
            else
            {
                ShowErrorNotification(result.Error?.Message ?? "Failed to delete user role.");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error deleting user role: {RoleCode}", roleCode);
            ShowErrorNotification("Error deleting user role. Please try again.");
        }
    }

    #endregion

    #region Permission Management

    private async Task ShowPermissionsDialog(SMSUserRole role)
    {
        await DialogService.OpenAsync<ViewPermissionsDialog>("View Permissions",
            new Dictionary<string, object>
            {
                { "Role", role },
                { "SMSModules", SMSModules }
            },
            new DialogOptions { Width = "800px", Height = "600px", Resizable = true, Draggable = true });
    }

    private void ToggleAllPermissions(bool enable)
    {
        foreach (var module in SMSModules)
        {
            editRole.CreatePermissions[module] = enable;
            editRole.ReadPermissions[module] = enable;
            editRole.UpdatePermissions[module] = enable;
            editRole.DeletePermissions[module] = enable;
        }
        StateHasChanged();
    }

    private void ToggleModulePermissions(string module, bool enable)
    {
        editRole.CreatePermissions[module] = enable;
        editRole.ReadPermissions[module] = enable;
        editRole.UpdatePermissions[module] = enable;
        editRole.DeletePermissions[module] = enable;
        StateHasChanged();
    }

    #endregion

    #region UI Helper Methods

    private void CancelEdit()
    {
        IsEditMode = false;
        CurrentRole = null;
        editRole = new EditRoleModel();
        Navigation.NavigateTo("/System/UserRoles/UserRoles");
    }

    private async Task ExportRoles()
    {
        ShowInfoNotification("Export functionality will be implemented soon.");
    }

    private int GetTotalPermissions(SMSUserRole role)
    {
        return role.Permissions?.Sum(p => (p.Create ? 1 : 0) + (p.Read ? 1 : 0) + (p.Update ? 1 : 0) + (p.Delete ? 1 : 0)) ?? 0;
    }

    // Helper method to get permission value for a module
    private bool GetPermissionValue(string module, string permissionType)
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

    #region Notifications

    private void ShowSuccessNotification(string message)
    {
        NotificationService.Notify(new NotificationMessage
        {
            Severity = NotificationSeverity.Success,
            Summary = "Success",
            Detail = message,
            Duration = 4000
        });
    }

    private void ShowErrorNotification(string message)
    {
        NotificationService.Notify(new NotificationMessage
        {
            Severity = NotificationSeverity.Error,
            Summary = "Error",
            Detail = message,
            Duration = 6000
        });
    }

    private void ShowInfoNotification(string message)
    {
        NotificationService.Notify(new NotificationMessage
        {
            Severity = NotificationSeverity.Info,
            Summary = "Information",
            Detail = message,
            Duration = 4000
        });
    }

    #endregion

    #region Models

    public class EditRoleModel
    {
        public string RoleName { get; set; } = "";
        public Dictionary<string, bool> CreatePermissions { get; set; } = new();
        public Dictionary<string, bool> ReadPermissions { get; set; } = new();
        public Dictionary<string, bool> UpdatePermissions { get; set; } = new();
        public Dictionary<string, bool> DeletePermissions { get; set; } = new();
    }

    public class CreateRoleModel
    {
        public string RoleName { get; set; } = "";
        public Dictionary<string, bool> CreatePermissions { get; set; } = new();
        public Dictionary<string, bool> ReadPermissions { get; set; } = new();
        public Dictionary<string, bool> UpdatePermissions { get; set; } = new();
        public Dictionary<string, bool> DeletePermissions { get; set; } = new();
    }

    #endregion
}