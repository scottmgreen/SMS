using System.Security;

using SMS3.Components.Shared.UIHelpers;

namespace SMS3.Components.Pages.SMSSystem.UserRoles;

public partial class UserRoles : ComponentBase
{
    [Inject] private IBaseMediator _mediator { get; set; } = default!;
    [Inject] private ILogger<UserRoles> _logger { get; set; } = default!;
    [Inject] private NavigationManager _navigation { get; set; } = default!;
    [Inject] private DialogService _dialogService { get; set; } = default!;
    [Inject] private INotificationHelper _notificationHelper { get; set; } = default!;
    

    // Data Properties
    private List<SMSUserRole> UserRolesList { get; set; } = new();
    private string? _successMessage { get; set; }
    private string? _errorMessage { get; set; }

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
        var allModules = (UserRolesList ?? new List<SMSUserRole>())
            .SelectMany(role => role.Permissions ?? new List<SMSUserRolePermission>())
            .Select(permission => permission.SMSModule) // Assuming Permission has a Module property
            .Where(module => !string.IsNullOrWhiteSpace(module))
            .Distinct()
            .OrderBy(module => module)
            .OfType<string>() // Ensure non-nullable string array
            .ToArray();

        SMSModules = allModules;
    }

    // Form Models
    private EditRoleModel _editRole = new();
    private CreateRoleModel _newRole = new();

    // Create Modal Properties
    private bool _showCreateModal { get; set; }
    private bool _isSaving { get; set; }

    // View Permissions Modal Properties
    private bool _showPermissionsModal { get; set; }
    private SMSUserRole? _viewRole { get; set; }

    // Edit Role Modal Properties
    private bool _showEditModal { get; set; }
    private SMSUserRole? _currentEditRole { get; set; }

    // Component References
    private RadzenDataGrid<SMSUserRole>? _rolesGrid;

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
            var userRolesResult = await _mediator.SendAsync(userRolesQuery, CancellationToken.None);
            UserRolesList = userRolesResult.IsSuccess ?
                userRolesResult.Value?.ToList() ?? new List<SMSUserRole>() :
                new List<SMSUserRole>();

            _logger.LogInformation("Loaded {RoleCount} user roles", UserRolesList.Count);

            StateHasChanged();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading user roles data");
            await _notificationHelper.ShowErrorAsync("Error loading data. Please refresh the page.");
        }
    }

    #endregion

    #region CRUD Operations

    private async Task ShowCreateDialog()
    {
        _newRole = new CreateRoleModel
        {
            CreatePermissions = SMSModules.ToDictionary(m => m, m => false),
            ReadPermissions = SMSModules.ToDictionary(m => m, m => false),
            UpdatePermissions = SMSModules.ToDictionary(m => m, m => false),
            DeletePermissions = SMSModules.ToDictionary(m => m, m => false)
        };
        _showCreateModal = true;
        StateHasChanged();
    }

    private async Task CreateRole()
    {
        if (!_isCreateFormValid)
        {
            await _notificationHelper.ShowErrorAsync("Please fill in all required fields.");
            return;
        }

        try
        {
            _isSaving = true;
            StateHasChanged();

            // Create role entity
            var roleCode = $"SR-0000";
            var roleId = new SMSUserRoleID(roleCode);
            var role = new SMSUserRole(roleId)
            {
                Code = roleCode,
                Name = _newRole.RoleName,
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
                    Create = _newRole.CreatePermissions.ContainsKey(module) && _newRole.CreatePermissions[module],
                    Read = _newRole.ReadPermissions.ContainsKey(module) && _newRole.ReadPermissions[module],
                    Update = _newRole.UpdatePermissions.ContainsKey(module) && _newRole.UpdatePermissions[module],
                    Delete = _newRole.DeletePermissions.ContainsKey(module) && _newRole.DeletePermissions[module]
                };
                role.Permissions.Add(permission);
            }

            var command = new CreateSMSUserRoleCommand(role);
            var result = await _mediator.SendAsync(command, CancellationToken.None);

            if (result.IsSuccess)
            {
                await _notificationHelper.ShowSuccessAsync($"User role '{_newRole.RoleName}' created successfully.");
                CloseCreateModal();
                await LoadUserRolesAsync();
            }
            else
            {
                await _notificationHelper.ShowErrorAsync(result.Error?.Message ?? "Failed to create user role.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user role");
            await _notificationHelper.ShowErrorAsync("Error creating user role. Please try again.");
        }
        finally
        {
            _isSaving = false;
            StateHasChanged();
        }
    }

    private void CloseCreateModal()
    {
        _showCreateModal = false;
        _newRole = new CreateRoleModel();
        StateHasChanged();
    }

    private bool _isCreateFormValid => !string.IsNullOrWhiteSpace(_newRole.RoleName);

    private void ToggleAllCreatePermissions(bool enable)
    {
        foreach (var module in SMSModules)
        {
            _newRole.CreatePermissions[module] = enable;
            _newRole.ReadPermissions[module] = enable;
            _newRole.UpdatePermissions[module] = enable;
            _newRole.DeletePermissions[module] = enable;
        }
        StateHasChanged();
    }

    private void ToggleCreateModulePermissions(string module, bool enable)
    {
        _newRole.CreatePermissions[module] = enable;
        _newRole.ReadPermissions[module] = enable;
        _newRole.UpdatePermissions[module] = enable;
        _newRole.DeletePermissions[module] = enable;
        StateHasChanged();
    }

    private void ToggleAllPermissions(bool enable)
    {
        foreach (var module in SMSModules)
        {
            _editRole.CreatePermissions[module] = enable;
            _editRole.ReadPermissions[module] = enable;
            _editRole.UpdatePermissions[module] = enable;
            _editRole.DeletePermissions[module] = enable;
        }
        StateHasChanged();
    }

    private void ToggleModulePermissions(string module, bool enable)
    {
        _editRole.CreatePermissions[module] = enable;
        _editRole.ReadPermissions[module] = enable;
        _editRole.UpdatePermissions[module] = enable;
        _editRole.DeletePermissions[module] = enable;
        StateHasChanged();
    }

    private async Task EditRole(string roleCode)
    {
        try
        {
            var getRoleQuery = new GetSMSUserRoleByIdQuery(roleCode);
            var roleResult = await _mediator.SendAsync(getRoleQuery, CancellationToken.None);

            if (roleResult.IsFailure)
            {
                await _notificationHelper.ShowErrorAsync("Role not found.");
                return;
            }

            _currentEditRole = roleResult.Value;

            // Populate edit form with permission matrices
            _editRole = new EditRoleModel
            {
                RoleName = _currentEditRole?.Name ?? "",
                CreatePermissions = SMSModules.ToDictionary(m => m, m => GetPermissionValue(m, "create")),
                ReadPermissions = SMSModules.ToDictionary(m => m, m => GetPermissionValue(m, "read")),
                UpdatePermissions = SMSModules.ToDictionary(m => m, m => GetPermissionValue(m, "update")),
                DeletePermissions = SMSModules.ToDictionary(m => m, m => GetPermissionValue(m, "delete"))
            };

            _showEditModal = true;
            StateHasChanged();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading role for edit: {RoleCode}", roleCode);
            await _notificationHelper.ShowErrorAsync("Error loading role. Please try again.");
        }
    }

    private async Task UpdateRole()
    {
        if (!_isEditFormValid)
        {
            await _notificationHelper.ShowErrorAsync("Please fill in all required fields.");
            return;
        }

        try
        {
            if (_currentEditRole is null)
            {
                await _notificationHelper.ShowErrorAsync("No role selected for update.");
                return;
            }

            _isSaving = true;
            StateHasChanged();

            _currentEditRole.Name = _editRole.RoleName;

            // Update permissions for each module
            foreach (var module in SMSModules)
            {
                var existingPermission = _currentEditRole.Permissions?.FirstOrDefault(p => p.SMSModule == module);
                if (existingPermission is not null)
                {
                    existingPermission.Create = _editRole.CreatePermissions.ContainsKey(module) && _editRole.CreatePermissions[module];
                    existingPermission.Read = _editRole.ReadPermissions.ContainsKey(module) && _editRole.ReadPermissions[module];
                    existingPermission.Update = _editRole.UpdatePermissions.ContainsKey(module) && _editRole.UpdatePermissions[module];
                    existingPermission.Delete = _editRole.DeletePermissions.ContainsKey(module) && _editRole.DeletePermissions[module];
                }
                else
                {
                    // Create new permission if it doesn't exist
                    var permissionCode = $"PRM-0000";
                    var permissionId = new SMSUserRolePermissionID(permissionCode);
                    var permission = new SMSUserRolePermission(permissionId)
                    {
                        Code = permissionCode,
                        SMSUserRoleCode = _currentEditRole.Code,
                        SMSModule = module,
                        Create = _editRole.CreatePermissions.ContainsKey(module) && _editRole.CreatePermissions[module],
                        Read = _editRole.ReadPermissions.ContainsKey(module) && _editRole.ReadPermissions[module],
                        Update = _editRole.UpdatePermissions.ContainsKey(module) && _editRole.UpdatePermissions[module],
                        Delete = _editRole.DeletePermissions.ContainsKey(module) && _editRole.DeletePermissions[module]
                    };

                    
                    _currentEditRole.Permissions ??= new List<SMSUserRolePermission>();
                    _currentEditRole.Permissions.Add(permission);
                }
            }

            var updateCommand = new UpdateSMSUserRoleCommand(_currentEditRole);
            var result = await _mediator.SendAsync(updateCommand, CancellationToken.None);

            if (result.IsSuccess)
            {
                await _notificationHelper.ShowSuccessAsync($"User role '{_editRole.RoleName}' updated successfully.");
                CloseEditModal();
                await LoadUserRolesAsync();
            }
            else
            {
                await _notificationHelper.ShowErrorAsync(result.Error?.Message ?? "Failed to update user role.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user role: {RoleCode}", _currentEditRole?.Code);
            await _notificationHelper.ShowErrorAsync("Error updating user role. Please try again.");
        }
        finally
        {
            _isSaving = false;
            StateHasChanged();
        }
    }

    private void CloseEditModal()
    {
        _showEditModal = false;
        _currentEditRole = null;
        _editRole = new EditRoleModel();
        StateHasChanged();
    }

    private bool _isEditFormValid => !string.IsNullOrWhiteSpace(_editRole.RoleName);

    private void ToggleAllEditPermissions(bool enable)
    {
        foreach (var module in SMSModules)
        {
            _editRole.CreatePermissions[module] = enable;
            _editRole.ReadPermissions[module] = enable;
            _editRole.UpdatePermissions[module] = enable;
            _editRole.DeletePermissions[module] = enable;
        }
        StateHasChanged();
    }

    private void ToggleEditModulePermissions(string module, bool enable)
    {
        _editRole.CreatePermissions[module] = enable;
        _editRole.ReadPermissions[module] = enable;
        _editRole.UpdatePermissions[module] = enable;
        _editRole.DeletePermissions[module] = enable;
        StateHasChanged();
    }

    private async Task ShowDeleteDialog(string roleCode, string roleName)
    {
        var result = await _dialogService.Confirm($"Are you sure you want to delete the role '{roleName}'?\n\nThis action cannot be undone and may affect users assigned to this role.",
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
            var result = await _mediator.SendAsync(command, CancellationToken.None);

            if (result.IsSuccess)
            {
                await _notificationHelper.ShowSuccessAsync("User role deleted successfully.");
                await LoadUserRolesAsync();
            }
            else
            {
                await _notificationHelper.ShowErrorAsync(result.Error?.Message ?? "Failed to delete user role.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user role: {RoleCode}", roleCode);
            await _notificationHelper.ShowErrorAsync("Error deleting user role. Please try again.");
        }
    }

    #endregion

    #region Permission Management

    private async Task ShowPermissionsDialog(SMSUserRole role)
    {
        _viewRole = role;
        _showPermissionsModal = true;
        StateHasChanged();
    }

    private void ClosePermissionsModal()
    {
        _showPermissionsModal = false;
        _viewRole = null;
        StateHasChanged();
    }

    private bool IsPermissionGranted(string module, string action)
    {
        if (_viewRole?.Permissions is null) return false;

        var permission = _viewRole.Permissions.FirstOrDefault(p => p.SMSModule == module);
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
        return _viewRole?.Permissions?.Sum(p => (p.Create ? 1 : 0) + (p.Read ? 1 : 0) + (p.Update ? 1 : 0) + (p.Delete ? 1 : 0)) ?? 0;
    }

    private int GetMaxPermissions()
    {
        return SMSModules.Length * 4;
    }
    private bool GetPermissionValue(string module, string permissionType)
    {
        if (_currentEditRole?.Permissions is null) return false;

        var permission = _currentEditRole.Permissions.FirstOrDefault(p => p.SMSModule == module);
        if (permission is null) return false;

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
        await _notificationHelper.ShowInfoAsync("Export functionality will be implemented soon.");
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


