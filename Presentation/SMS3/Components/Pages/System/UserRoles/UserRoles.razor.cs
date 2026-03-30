using System.Security;

using SMS3.Components.Shared.UIHelpers;

namespace SMS3.Components.Pages.System.UserRoles;

public partial class UserRoles : ComponentBase
{
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ILogger<UserRoles> Logger { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Inject] private DialogService DialogService { get; set; } = default!;
    [Inject] private INotificationHelper NotificationHelper { get; set; } = default!;
    

    // Data Properties
    private List<SMSUserRole> UserRolesList { get; set; } = new();
    private string? SuccessMessage { get; set; }
    private string? ErrorMessage { get; set; }

    // Predefined SMS Modules
    private static string[] SMSModules { get; set; } = Array.Empty<string>();

    private void LoadModules()
    {
        if (!UserRolesList?.Any() == true)
        {
            SMSModules = Array.Empty<string>();
            return;
        }

        // Alternative approach if permissions have a Module property
        var allModules = UserRolesList
            .SelectMany(role => role.Permissions ?? new List<SMSUserRolePermission>())
            .Select(permission => permission.SMSModule) // Assuming Permission has a Module property
            .Where(module => !string.IsNullOrWhiteSpace(module))
            .Distinct()
            .OrderBy(module => module)
            .ToArray();

        SMSModules = allModules;
    }

    // Form Models
    private EditRoleModel editRole = new();
    private CreateRoleModel NewRole = new();

    // Create Modal Properties
    private bool ShowCreateModal { get; set; }
    private bool IsSaving { get; set; }

    // View Permissions Modal Properties
    private bool ShowPermissionsModal { get; set; }
    private SMSUserRole? ViewRole { get; set; }

    // Edit Role Modal Properties
    private bool ShowEditModal { get; set; }
    private SMSUserRole? CurrentEditRole { get; set; }

    // Component References
    private RadzenDataGrid<SMSUserRole>? rolesGrid;

    protected override async Task OnInitializedAsync()
    {
        await LoadUserRolesAsync();
        LoadModules();

    }

    #region Data Loading

    private async Task LoadUserRolesAsync()
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
            await NotificationHelper.ShowErrorAsync("Error loading data. Please refresh the page.");
        }
    }

    #endregion

    #region CRUD Operations

    private async Task ShowCreateDialog()
    {
        NewRole = new CreateRoleModel
        {
            CreatePermissions = SMSModules.ToDictionary(m => m, m => false),
            ReadPermissions = SMSModules.ToDictionary(m => m, m => false),
            UpdatePermissions = SMSModules.ToDictionary(m => m, m => false),
            DeletePermissions = SMSModules.ToDictionary(m => m, m => false)
        };
        ShowCreateModal = true;
        StateHasChanged();
    }

    private async Task CreateRole()
    {
        if (!IsCreateFormValid)
        {
            await NotificationHelper.ShowErrorAsync("Please fill in all required fields.");
            return;
        }

        try
        {
            IsSaving = true;
            StateHasChanged();

            // Create role entity
            var roleCode = $"SR-0000";
            var roleId = new SMSUserRoleID(roleCode);
            var role = new SMSUserRole(roleId)
            {
                Code = roleCode,
                Name = NewRole.RoleName,
                Permissions = new List<SMSUserRolePermission>()
            };

            // Create permissions for each module
            foreach (var module in SMSModules)
            {
                var permissionCode = $"PRM-0000";
                var permissionId = new SMSUserRolePermissionID(permissionCode);
                var permission = new SMSUserRolePermission(permissionId)
                {
                    Code = permissionCode,
                    SMSUserRoleCode = roleCode,
                    SMSModule = module,
                    Create = NewRole.CreatePermissions.ContainsKey(module) && NewRole.CreatePermissions[module],
                    Read = NewRole.ReadPermissions.ContainsKey(module) && NewRole.ReadPermissions[module],
                    Update = NewRole.UpdatePermissions.ContainsKey(module) && NewRole.UpdatePermissions[module],
                    Delete = NewRole.DeletePermissions.ContainsKey(module) && NewRole.DeletePermissions[module]
                };
                role.Permissions.Add(permission);
            }

            var command = new CreateSMSUserRoleCommand(role);
            var result = await Mediator.SendAsync(command, CancellationToken.None);

            if (result.IsSuccess)
            {
                await NotificationHelper.ShowSuccessAsync($"User role '{NewRole.RoleName}' created successfully.");
                CloseCreateModal();
                await LoadUserRolesAsync();
            }
            else
            {
                await NotificationHelper.ShowErrorAsync(result.Error?.Message ?? "Failed to create user role.");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error creating user role");
            await NotificationHelper.ShowErrorAsync("Error creating user role. Please try again.");
        }
        finally
        {
            IsSaving = false;
            StateHasChanged();
        }
    }

    private void CloseCreateModal()
    {
        ShowCreateModal = false;
        NewRole = new CreateRoleModel();
        StateHasChanged();
    }

    private bool IsCreateFormValid => !string.IsNullOrWhiteSpace(NewRole.RoleName);

    private void ToggleAllCreatePermissions(bool enable)
    {
        foreach (var module in SMSModules)
        {
            NewRole.CreatePermissions[module] = enable;
            NewRole.ReadPermissions[module] = enable;
            NewRole.UpdatePermissions[module] = enable;
            NewRole.DeletePermissions[module] = enable;
        }
        StateHasChanged();
    }

    private void ToggleCreateModulePermissions(string module, bool enable)
    {
        NewRole.CreatePermissions[module] = enable;
        NewRole.ReadPermissions[module] = enable;
        NewRole.UpdatePermissions[module] = enable;
        NewRole.DeletePermissions[module] = enable;
        StateHasChanged();
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

    private async Task EditRole(string roleCode)
    {
        try
        {
            var getRoleQuery = new GetSMSUserRoleByIdQuery(roleCode);
            var roleResult = await Mediator.SendAsync(getRoleQuery, CancellationToken.None);

            if (roleResult.IsFailure)
            {
                await NotificationHelper.ShowErrorAsync("Role not found.");
                return;
            }

            CurrentEditRole = roleResult.Value;

            // Populate edit form with permission matrices
            editRole = new EditRoleModel
            {
                RoleName = CurrentEditRole.Name,
                CreatePermissions = SMSModules.ToDictionary(m => m, m => GetPermissionValue(m, "create")),
                ReadPermissions = SMSModules.ToDictionary(m => m, m => GetPermissionValue(m, "read")),
                UpdatePermissions = SMSModules.ToDictionary(m => m, m => GetPermissionValue(m, "update")),
                DeletePermissions = SMSModules.ToDictionary(m => m, m => GetPermissionValue(m, "delete"))
            };

            ShowEditModal = true;
            StateHasChanged();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading role for edit: {RoleCode}", roleCode);
            await NotificationHelper.ShowErrorAsync("Error loading role. Please try again.");
        }
    }

    private async Task UpdateRole()
    {
        if (!IsEditFormValid)
        {
            await NotificationHelper.ShowErrorAsync("Please fill in all required fields.");
            return;
        }

        try
        {
            if (CurrentEditRole == null)
            {
                await NotificationHelper.ShowErrorAsync("No role selected for update.");
                return;
            }

            IsSaving = true;
            StateHasChanged();

            CurrentEditRole.Name = editRole.RoleName;

            // Update permissions for each module
            foreach (var module in SMSModules)
            {
                var existingPermission = CurrentEditRole.Permissions?.FirstOrDefault(p => p.SMSModule == module);
                if (existingPermission != null)
                {
                    existingPermission.Create = editRole.CreatePermissions.ContainsKey(module) && editRole.CreatePermissions[module];
                    existingPermission.Read = editRole.ReadPermissions.ContainsKey(module) && editRole.ReadPermissions[module];
                    existingPermission.Update = editRole.UpdatePermissions.ContainsKey(module) && editRole.UpdatePermissions[module];
                    existingPermission.Delete = editRole.DeletePermissions.ContainsKey(module) && editRole.DeletePermissions[module];
                }
                else
                {
                    // Create new permission if it doesn't exist
                    var permissionCode = $"PRM-0000";
                    var permissionId = new SMSUserRolePermissionID(permissionCode);
                    var permission = new SMSUserRolePermission(permissionId)
                    {
                        Code = permissionCode,
                        SMSUserRoleCode = CurrentEditRole.Code,
                        SMSModule = module,
                        Create = editRole.CreatePermissions.ContainsKey(module) && editRole.CreatePermissions[module],
                        Read = editRole.ReadPermissions.ContainsKey(module) && editRole.ReadPermissions[module],
                        Update = editRole.UpdatePermissions.ContainsKey(module) && editRole.UpdatePermissions[module],
                        Delete = editRole.DeletePermissions.ContainsKey(module) && editRole.DeletePermissions[module]
                    };

                    
                    CurrentEditRole.Permissions ??= new List<SMSUserRolePermission>();
                    CurrentEditRole.Permissions.Add(permission);
                }
            }

            var updateCommand = new UpdateSMSUserRoleCommand(CurrentEditRole);
            var result = await Mediator.SendAsync(updateCommand, CancellationToken.None);

            if (result.IsSuccess)
            {
                await NotificationHelper.ShowSuccessAsync($"User role '{editRole.RoleName}' updated successfully.");
                CloseEditModal();
                await LoadUserRolesAsync();
            }
            else
            {
                await NotificationHelper.ShowErrorAsync(result.Error?.Message ?? "Failed to update user role.");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error updating user role: {RoleCode}", CurrentEditRole?.Code);
            await NotificationHelper.ShowErrorAsync("Error updating user role. Please try again.");
        }
        finally
        {
            IsSaving = false;
            StateHasChanged();
        }
    }

    private void CloseEditModal()
    {
        ShowEditModal = false;
        CurrentEditRole = null;
        editRole = new EditRoleModel();
        StateHasChanged();
    }

    private bool IsEditFormValid => !string.IsNullOrWhiteSpace(editRole.RoleName);

    private void ToggleAllEditPermissions(bool enable)
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

    private void ToggleEditModulePermissions(string module, bool enable)
    {
        editRole.CreatePermissions[module] = enable;
        editRole.ReadPermissions[module] = enable;
        editRole.UpdatePermissions[module] = enable;
        editRole.DeletePermissions[module] = enable;
        StateHasChanged();
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
                await NotificationHelper.ShowSuccessAsync("User role deleted successfully.");
                await LoadUserRolesAsync();
            }
            else
            {
                await NotificationHelper.ShowErrorAsync(result.Error?.Message ?? "Failed to delete user role.");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error deleting user role: {RoleCode}", roleCode);
            await NotificationHelper.ShowErrorAsync("Error deleting user role. Please try again.");
        }
    }

    #endregion

    #region Permission Management

    private async Task ShowPermissionsDialog(SMSUserRole role)
    {
        ViewRole = role;
        ShowPermissionsModal = true;
        StateHasChanged();
    }

    private void ClosePermissionsModal()
    {
        ShowPermissionsModal = false;
        ViewRole = null;
        StateHasChanged();
    }

    private bool IsPermissionGranted(string module, string action)
    {
        if (ViewRole?.Permissions == null) return false;

        var permission = ViewRole.Permissions.FirstOrDefault(p => p.SMSModule == module);
        return action switch
        {
            "Create" => permission?.Create == true,
            "Read" => permission?.Read == true,
            "Update" => permission?.Update == true,
            "Delete" => permission?.Delete == true,
            _ => false
        };
    }

    private int GetViewRoleTotalPermissions()
    {
        return ViewRole?.Permissions?.Sum(p => (p.Create ? 1 : 0) + (p.Read ? 1 : 0) + (p.Update ? 1 : 0) + (p.Delete ? 1 : 0)) ?? 0;
    }

    private int GetMaxPermissions()
    {
        return SMSModules.Length * 4;
    }
    private bool GetPermissionValue(string module, string permissionType)
    {
        if (CurrentEditRole?.Permissions == null) return false;

        var permission = CurrentEditRole.Permissions.FirstOrDefault(p => p.SMSModule == module);
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
    private async Task ExportRoles()
    {
        await NotificationHelper.ShowInfoAsync("Export functionality will be implemented soon.");
    }

    private int GetTotalPermissions(SMSUserRole role)
    {
        return role.Permissions?.Sum(p => (p.Create ? 1 : 0) + (p.Read ? 1 : 0) + (p.Update ? 1 : 0) + (p.Delete ? 1 : 0)) ?? 0;
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