using SMS_Application.Interfaces;
using SMS_Application.Commands;
using SMS_Application.Queries;

using SMS_Domain.Events;
using SMS_Domain.ValueObjects;

using SMS_Shared.Common;
using SMS_Shared.Configuration;

using SMS3.Components.Shared.UIHelpers;
using SMS3.Configuration.Extensions;

namespace SMS3.Components.Pages.SMSSystem.UserManagement;

/// <summary>
/// Code-behind for Application Users management page
/// Provides comprehensive user management capabilities for SMS Application Users
/// ? FIXED: Removed manual audit field assignments - pipeline handles automatically
/// </summary>
public partial class ApplicationUsers : ComponentBase
{
    [Inject] private ICurrentUserService _currentUserService { get; set; } = default!;
    [Inject] private IBaseMediator _mediator { get; set; } = default!;
    [Inject] private ILogger<ApplicationUsers> _logger { get; set; } = default!;
    [Inject] private NavigationManager _navigation { get; set; } = default!;
    [Inject] private DialogService _dialogService { get; set; } = default!;
    [Inject] private IBaseEventBus _eventBus { get; set; } = default!;

    // Route parameter for edit mode
    [Parameter] public string? Id { get; set; }

    // Data Properties - renamed to avoid conflict
    private List<SMSApplicationUser> ApplicationUsersList { get; set; } = new();
    private List<SMSUserRole> UserRoles { get; set; } = new();
    private SMSApplicationUser? _currentUser { get; set; }
    private bool _isEditMode { get; set; }
    private string? _successMessage { get; set; }
    private string? _errorMessage { get; set; }

    // Password Modal Properties for Shared Component
    private bool _showPasswordModal { get; set; }
    private string _passwordUserCode { get; set; } = string.Empty;
    private string _passwordUserDisplayName { get; set; } = string.Empty;

    // ?? NEW: Role Assignment Properties
    private bool _showRoleAssignmentModal { get; set; }
    private string _roleAssignmentUserCode { get; set; } = string.Empty;
    private string _roleAssignmentUserDisplayName { get; set; } = string.Empty;
    private string? _currentUserRoleCode { get; set; }
    private string? _selectedRoleCode { get; set; }
    private List<SMSUserRole> AvailableRoles { get; set; } = new();

    // Dynamically get all unique modules from available roles' permissions
    private IEnumerable<string> _smsModules =>
        AvailableRoles
            .Where(role => role.Permissions is not null)
            .SelectMany(role => role.Permissions)
            .Where(permission => !string.IsNullOrWhiteSpace(permission.SMSModule))
            .Select(permission => permission.SMSModule!)
            .Distinct()
            .OrderBy(module => module);

    // Group Management Properties
    private bool _showGroupsModal { get; set; } = false;
    private string _groupManagementUserCode { get; set; } = string.Empty;
    private string _groupManagementUserDisplayName { get; set; } = string.Empty;
    private List<SMSApplicationGroup> AllApplicationGroups { get; set; } = new();
    private List<SMSApplicationGroup> UserCurrentGroups { get; set; } = new();
    private List<SMSApplicationGroup> AvailableGroups { get; set; } = new();
    private Dictionary<string, bool> SelectedGroups { get; set; } = new();

    // Form Models
    private EditUserModel _editUser = new();
    private CreateUserModel _newUser = new();
    private string? _editUserRoleCode { get; set; }

    // Create Modal Properties
    private bool _showCreateModal { get; set; }
    private bool _isSaving { get; set; }

    // Component References
    private RadzenDataGrid<SMSApplicationUser>? _usersGrid;

    protected override async Task OnInitializedAsync()
    {
        _logger.LogInformation("OnInitializedAsync called with Id: {Id}", Id ?? "NULL");
        
        await LoadDataAsync();

        // Check if we're in edit mode
        if (!string.IsNullOrWhiteSpace(Id))
        {
            _logger.LogInformation("Calling LoadUserForEdit from OnInitializedAsync with Id: {Id}", Id);
            await LoadUserForEdit(Id);
        }
        else
        {
            _logger.LogInformation("No Id parameter, staying in list mode");
        }
    }

    protected override async Task OnParametersSetAsync()
    {
        _logger.LogInformation("OnParametersSetAsync called with Id: {Id}, IsEditMode: {IsEditMode}", 
            Id ?? "NULL", _isEditMode);
            
        if (!string.IsNullOrWhiteSpace(Id) && !_isEditMode)
        {
            _logger.LogInformation("Calling LoadUserForEdit from OnParametersSetAsync with Id: {Id}", Id);
            await LoadUserForEdit(Id);
        }
        else if (string.IsNullOrWhiteSpace(Id) && _isEditMode)
        {
            _logger.LogInformation("Cancelling edit mode from OnParametersSetAsync");
            CancelEdit();
        }
    }

    #region Data Loading

    private async Task LoadDataAsync()
    {
        try
        {
            // Load Application Users
            var applicationUsersQuery = new GetAllSMSApplicationUsersQuery();
            var applicationUsersResult = await _mediator.SendAsync(applicationUsersQuery, CancellationToken.None);
            ApplicationUsersList = applicationUsersResult.IsSuccess ?
                applicationUsersResult.Value?.ToList() ?? new List<SMSApplicationUser>() :
                new List<SMSApplicationUser>();
            // Load Application Groups for group management
            var groupsQuery = new GetAllSMSApplicationGroupsQuery();
            var groupsResult = await _mediator.SendAsync(groupsQuery, CancellationToken.None);
            AllApplicationGroups = groupsResult.IsSuccess ?
                groupsResult.Value?.ToList() ?? new List<SMSApplicationGroup>() :
                new List<SMSApplicationGroup>();

            // ?? NEW: Load User Roles for assignment
            var userRolesQuery = new GetAllSMSUserRolesQuery();
            var userRolesResult = await _mediator.SendAsync(userRolesQuery, CancellationToken.None);
            AvailableRoles = userRolesResult.IsSuccess ?
                userRolesResult.Value?.ToList() ?? new List<SMSUserRole>() :
                new List<SMSUserRole>();

            _logger.LogInformation("Loaded {UserCount} application users, {GroupCount} groups, and {RoleCount} roles",
                ApplicationUsersList.Count, AllApplicationGroups.Count, AvailableRoles.Count);

            StateHasChanged();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading application users data");
            await ShowErrorAsyncNotification("Error loading data. Please refresh the page.");
        }
    }

    private async Task LoadUserForEdit(string id)
    {
        try
        {
            _logger.LogInformation("LoadUserForEdit called with ID: {Id}", id);
            
            var getUserQuery = new GetSMSApplicationUserByCodeQuery(id);
            var userResult = await _mediator.SendAsync(getUserQuery, CancellationToken.None);

            _logger.LogInformation("Query result - Success: {IsSuccess}, User found: {UserFound}", 
                userResult.IsSuccess, userResult.Value is not null);

            if (userResult.IsFailure || userResult.Value is null)
            {
                _logger.LogWarning("User not found for ID: {Id}", id);
                await ShowErrorAsyncNotification($"User not found: {id}");
                _navigation.NavigateTo("/SMSSystem/UserManagement/ApplicationUsers");
                return;
            }

            _currentUser = userResult.Value;
            _isEditMode = true;

            _logger.LogInformation("Edit mode set - CurrentUser: {UserCode}, IsEditMode: {IsEditMode}", 
                _currentUser?.Code, _isEditMode);

            // Populate edit form
            _editUser = new EditUserModel
            {
                FirstName = _currentUser?.FirstName?.Value ?? "",
                LastName = _currentUser?.LastName?.Value ?? ""
            };

            // Set the role code for dropdown binding
            _editUserRoleCode = _currentUser?.UserRole?.Code;

            _logger.LogInformation("Edit form populated - FirstName: {FirstName}, LastName: {LastName}, RoleCode: {RoleCode}", 
                _editUser.FirstName, _editUser.LastName, _editUserRoleCode);

            StateHasChanged();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading user for edit: {UserId}", id);
            await ShowErrorAsyncNotification("An error occurred while loading the user for editing.");
            
            // Navigate back to main list on error
            _navigation.NavigateTo("/SMSSystem/UserManagement/ApplicationUsers");
        }
    }

    #endregion

    #region ?? NEW: Role Assignment Methods
    private bool IsPermissionGranted(SMSUserRole role, string module, string action)
    {
        if (role?.Permissions is null) return false;

        var permission = role.Permissions.FirstOrDefault(p => p.SMSModule == module);
        return action switch
        {
            "Create" => permission?.Create == true,
            "Read" => permission?.Read == true,
            "Update" => permission?.Update == true,
            "Delete" => permission?.Delete == true,
            _ => false
        };
    }
    private void OpenRoleAssignmentModal(string userCode, string userDisplayName, string? currentRoleCode = null)
    {
        _roleAssignmentUserCode = userCode;
        _roleAssignmentUserDisplayName = userDisplayName;
        _currentUserRoleCode = currentRoleCode;
        _selectedRoleCode = currentRoleCode;
        _showRoleAssignmentModal = true;
        StateHasChanged();
    }

    private void CloseRoleAssignmentModal()
    {
        _showRoleAssignmentModal = false;
        _roleAssignmentUserCode = string.Empty;
        _roleAssignmentUserDisplayName = string.Empty;
        _currentUserRoleCode = null;
        _selectedRoleCode = null;
        StateHasChanged();
    }

    private async Task AssignUserRole()
    {
        if (string.IsNullOrEmpty(_roleAssignmentUserCode) || string.IsNullOrEmpty(_selectedRoleCode))
        {
            await ShowErrorAsyncNotification("Invalid user or role selection.");
            return;
        }

        try
        {
            _isSaving = true;
            StateHasChanged();

            // Get the user
            var userQuery = new GetSMSApplicationUserByCodeQuery(_roleAssignmentUserCode);
            var userResult = await _mediator.SendAsync(userQuery, CancellationToken.None);

            if (userResult.IsFailure || userResult.Value is null)
            {
                await ShowErrorAsyncNotification("User not found.");
                return;
            }

            var user = userResult.Value;

            // Get the selected role
            var selectedRole = AvailableRoles.FirstOrDefault(r => r.Code == _selectedRoleCode);
            if (selectedRole is null)
            {
                await ShowErrorAsyncNotification("Selected role not found.");
                return;
            }
            user.SMSUserType = SMSUserType.Application;
            
            // ? FIXED: Only set business fields - let pipeline handle audit fields
            user.UserRole = selectedRole;
            // ? REMOVED: user.UpdatedBy = _currentUserService?.UserDisplayName;
            // ? REMOVED: user.UpdatedDate = DateTime.UtcNow;

            // Update user - pipeline will automatically set UpdatedBy/UpdatedDate
            var updateCommand = new UpdateSMSApplicationUserCommand(user);
            var updateResult = await _mediator.SendAsync(updateCommand, CancellationToken.None);

            if (updateResult.IsSuccess)
            {
                await ShowSuccessAsyncNotification($"Role '{selectedRole.Name}' successfully assigned to {_roleAssignmentUserDisplayName}.");

                // Refresh data and close modal
                await LoadDataAsync();
                CloseRoleAssignmentModal();
            }
            else
            {
                await ShowErrorAsyncNotification($"Failed to assign role: {updateResult.Error?.Message}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning role to user {UserCode}", _roleAssignmentUserCode);
            await ShowErrorAsyncNotification("An error occurred while assigning the role. Please try again.");
        }
        finally
        {
            _isSaving = false;
            StateHasChanged();
        }
    }

    private async Task RemoveUserRole()
    {
        if (string.IsNullOrEmpty(_roleAssignmentUserCode))
        {
            await ShowErrorAsyncNotification("Invalid user selection.");
            return;
        }

        try
        {
            _isSaving = true;
            StateHasChanged();

            // Get the user
            var userQuery = new GetSMSApplicationUserByCodeQuery(_roleAssignmentUserCode);
            var userResult = await _mediator.SendAsync(userQuery, CancellationToken.None);

            if (userResult.IsFailure || userResult.Value is null)
            {
                await ShowErrorAsyncNotification("User not found.");
                return;
            }

            var user = userResult.Value;

            // ? FIXED: Only set business fields - let pipeline handle audit fields
            user.UserRole = null!; // Explicitly assign null with null-forgiving operator
            // ? REMOVED: user.UpdatedBy = _currentUserService?.UserDisplayName;
            // ? REMOVED: user.UpdatedDate = DateTime.UtcNow;

            // Update user - pipeline will automatically set UpdatedBy/UpdatedDate
            var updateCommand = new UpdateSMSApplicationUserCommand(user);
            var updateResult = await _mediator.SendAsync(updateCommand, CancellationToken.None);

            if (updateResult.IsSuccess)
            {
                await ShowSuccessAsyncNotification($"Role successfully removed from {_roleAssignmentUserDisplayName}.");

                // Refresh data and close modal
                await LoadDataAsync();
                CloseRoleAssignmentModal();
            }
            else
            {
                await ShowErrorAsyncNotification($"Failed to remove role: {updateResult.Error?.Message}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing role from user {UserCode}", _roleAssignmentUserCode);
            await ShowErrorAsyncNotification("An error occurred while removing the role. Please try again.");
        }
        finally
        {
            _isSaving = false;
            StateHasChanged();
        }
    }

    #endregion

    #region CRUD Operations

    private async Task ShowCreateDialog()
    {
            _newUser = new CreateUserModel
        {
            TwoFactorEnabled = false
        };
        _newIsActive = true; // ADDED: Initialize the property
        _showCreateModal = true;
        StateHasChanged();
    }

    private async Task CreateUser()
    {
        if (!_isCreateFormValid)
        {
            await ShowErrorAsyncNotification("Please fill in all required fields.");
            return;
        }

        try
        {
            _isSaving = true;
            StateHasChanged();

            // ?? NEW: Get selected role if provided
            SMSUserRole? selectedRole = null;
            if (!string.IsNullOrEmpty(_newUser.UserRoleCode))
            {
                selectedRole = AvailableRoles.FirstOrDefault(r => r.Code == _newUser.UserRoleCode);
            }

            // Create user entity
            var userId = new SMSApplicationUserID($"AU-0000");
            var user = new SMSApplicationUser(userId)
            {
                Code = userId.Value,
                FirstName = FirstName.Create(_newUser.FirstName).Value,
                LastName = LastName.Create(_newUser.LastName).Value,
                UserName = UserName.Create(_newUser.UserName).Value,
                Password = Password.Create(_newUser.Password).Value,
                UserRole = selectedRole ?? new SMSUserRole(new SMSUserRoleID("ROLE-UNASSIGNED")) { Name = "Unassigned" }, // ?? NEW: Assign role during creation
                TwoFactorEnabled = _newUser.TwoFactorEnabled, // ?? NEW: Set 2FA requirement
                IsActive = _newIsActive, // UPDATED: Use NewIsActive property
                SMSUserType = SMSUserType.Application
                // ? FIXED: Removed manual audit field assignments
                // ? REMOVED: CreatedBy = _currentUserService?.UserDisplayName,
                // ? REMOVED: CreatedDate = DateTime.UtcNow
            };

            // Create user - pipeline will automatically set CreatedBy/CreatedDate
            var command = new CreateSMSApplicationUserCommand(user);
            var result = await _mediator.SendAsync(command, CancellationToken.None);

            if (result.IsSuccess)
            {
                var roleText = selectedRole is not null ? $" with role '{selectedRole.Name}'" : "";
            await ShowSuccessAsyncNotification($"Application user '{_newUser.FirstName} {_newUser.LastName}' created successfully{roleText}!");
                CloseCreateModal();
                await LoadDataAsync();
            }
            else
            {
                await ShowErrorAsyncNotification(result.Error?.Message ?? "Failed to create application user.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating application user");
            await ShowErrorAsyncNotification("Error creating application user. Please try again.");
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
            _newUser = new CreateUserModel
        {
            TwoFactorEnabled = false
        };
        _newIsActive = true; // ADDED: Reset the property
        StateHasChanged();
    }

    private bool _isCreateFormValid =>
        !string.IsNullOrWhiteSpace(_newUser.FirstName) &&
        !string.IsNullOrWhiteSpace(_newUser.LastName) &&
        !string.IsNullOrWhiteSpace(_newUser.UserName) &&
        !string.IsNullOrWhiteSpace(_newUser.Password);

    private void EditUser(string userId)
    {
        try
        {
            _logger.LogInformation("Editing user: {UserId}", userId);
            
            // Use regular navigation for edit routes since secure navigation has issues with route parameters
            // TODO: Fix SecureNavigation to properly handle route parameters
            _navigation.NavigateTo($"/SMSSystem/UserManagement/ApplicationUsers/Edit/{userId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error navigating to edit user: {UserId}", userId);
            _ = ShowErrorAsyncNotification("Error opening user editor");
        }
    }

    private async Task UpdateUser(EditUserModel model)
    {
        try
        {
            if (_currentUser is null)
            {
                await ShowErrorAsyncNotification("No user selected for update.");
                return;
            }

            // ? FIXED: Only update business fields - let pipeline handle audit fields
            _currentUser.FirstName = FirstName.Create(model.FirstName).Value;
            _currentUser.LastName = LastName.Create(model.LastName).Value;
            _currentUser.SMSUserType = SMSUserType.Application;
            
            // Update role if changed
            if (!string.IsNullOrEmpty(_editUserRoleCode))
            {
                var selectedRole = AvailableRoles.FirstOrDefault(r => r.Code == _editUserRoleCode);
                _currentUser.UserRole = selectedRole ?? new SMSUserRole(new SMSUserRoleID("ROLE-UNASSIGNED")) { Name = "Unassigned" };
            }
            else
            {
                _currentUser.UserRole = new SMSUserRole(new SMSUserRoleID("ROLE-UNASSIGNED")) { Name = "Unassigned" };
            }
            
            // ? REMOVED: Manual audit field assignments
            // CurrentUser.UpdatedBy = _currentUserService?.UserDisplayName;
            // CurrentUser.UpdatedDate = DateTime.UtcNow;

            // Update user - pipeline will automatically set UpdatedBy/UpdatedDate
            var updateCommand = new UpdateSMSApplicationUserCommand(_currentUser);
            var result = await _mediator.SendAsync(updateCommand, CancellationToken.None);

            if (result.IsSuccess)
            {
                await ShowSuccessAsyncNotification($"User '{_currentUser.UserName.Value}' has been updated successfully!");
                
                // Reset form state
                _isEditMode = false;
                _currentUser = null;
                
                // Navigate back to main list with success
                _navigation.NavigateToSecure("/SMSSystem/UserManagement/ApplicationUsers");
            }
            else
            {
                await ShowErrorAsyncNotification(result.Error?.Message ?? "Failed to update application user.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating application user: {UserId}", _currentUser?.Code);
            await ShowErrorAsyncNotification("Error updating application user. Please try again.");
        }
    }

    private async Task ShowDeleteDialog(string userId, string displayName)
    {
        var result = await _dialogService.Confirm($"Are you sure you want to delete the user '{displayName}'?",
            "Confirm Delete",
            new ConfirmOptions
            {
                OkButtonText = "Delete",
                CancelButtonText = "Cancel",
                AutoFocusFirstElement = true
            });

        if (result == true)
        {
            await DeleteUser(userId);
        }
    }

    private async Task DeleteUser(string userId)
    {
        try
        {
            var applicationUserId = new SMSApplicationUserID(userId);
            var command = new DeleteSMSApplicationUserCommand(applicationUserId);
            var result = await _mediator.SendAsync(command, CancellationToken.None);

            if (result.IsSuccess)
            {
                await ShowSuccessAsyncNotification("Application user deleted successfully.");
                await LoadDataAsync();
            }
            else
            {
                await ShowErrorAsyncNotification(result.Error?.Message ?? "Failed to delete application user.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting application user: {UserId}", userId);
            await ShowErrorAsyncNotification("Error deleting application user. Please try again.");
        }
    }

    #endregion

    #region Password Management

    private void OpenPasswordChangeModal(string userCode, string displayName)
    {
        _passwordUserCode = userCode;
        _passwordUserDisplayName = displayName;
        _showPasswordModal = true;
        StateHasChanged();
    }

    private void ClosePasswordChangeModal()
    {
        _showPasswordModal = false;
        _passwordUserCode = string.Empty;
        _passwordUserDisplayName = string.Empty;
        StateHasChanged();
    }

    private async Task OnPasswordChangedSuccess()
    {
        // Password was changed successfully by the modal
        await ShowSuccessAsyncNotification($"Password updated successfully for {_passwordUserDisplayName}.");
    }

    private async Task ShowPasswordDialog(string userId, string displayName)
    {
        // Legacy method - replaced with OpenPasswordChangeModal
        OpenPasswordChangeModal(userId, displayName);
    }

    #endregion

    #region _navigation & UI

    private void CancelEdit()
    {
        _isEditMode = false;
        _currentUser = null;
        _editUserRoleCode = null;
        _editUser = new EditUserModel();
        _navigation.NavigateToSecure("/SMSSystem/UserManagement/ApplicationUsers");
    }

    private void NavigateToUserManagement()
    {
        _navigation.NavigateToSecure("/SMSSystem/UserManagement");
    }

    private async Task ExportUsers()
    {
        // TODO: Implement export functionality
        await ShowInfoAsyncNotification("Export functionality will be implemented soon.");
    }

    #endregion

    #region Notifications (EventBus-Driven)

    private async Task ShowSuccessAsyncNotification(string message)
    {
        await _eventBus.PublishUIEventAsync(UINotificationEvent.Success("Success", message));
    }

    private async Task ShowErrorAsyncNotification(string message)
    {
        await _eventBus.PublishUIEventAsync(UINotificationEvent.Error("Error", message));
    }

    private async Task ShowInfoAsyncNotification(String message)
    {
        await _eventBus.PublishUIEventAsync(UINotificationEvent.Info("Information", message));
    }

    #endregion

    #region Models

    public class EditUserModel
    {
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
    }

    // ?? UPDATED: CreateUserModel with role assignment support
    public class CreateUserModel
    {
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string UserName { get; set; } = "";
        public string Password { get; set; } = "";
        public string? UserRoleCode { get; set; } // NEW: Role assignment during creation
        public bool TwoFactorEnabled { get; set; } = false; // NEW: 2FA requirement during creation
    }

    // ADDED: Missing property for NewIsActive binding
    private bool _newIsActive { get; set; } = true;

    #endregion

    #region Group Management

    private async Task ManageGroups(string userId, string displayName)
    {
        try
        {
            _groupManagementUserCode = userId;
            _groupManagementUserDisplayName = displayName;

            await LoadUserGroups(userId);
            _showGroupsModal = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error opening group management for user: {UserId}", userId);
            await ShowErrorAsyncNotification("Error loading user groups. Please try again.");
        }
    }

    private async Task LoadUserGroups(string userId)
    {
        try
        {
            // Load groups that this user is currently assigned to
            var userGroupsQuery = new GetSMSApplicationGroupsByUserCodeQuery(userId);
            var userGroupsResult = await _mediator.SendAsync(userGroupsQuery, CancellationToken.None);
            UserCurrentGroups = userGroupsResult.IsSuccess ?
                userGroupsResult.Value?.ToList() ?? new List<SMSApplicationGroup>() :
                new List<SMSApplicationGroup>();

            // Calculate available groups (groups the user is not currently in)
            var currentGroupCodes = UserCurrentGroups.Select(g => g.Code).ToHashSet();
            AvailableGroups = AllApplicationGroups
                .Where(g => !currentGroupCodes.Contains(g.Code) && g.IsActive)
                .OrderBy(g => g.Name)
                .ToList();

            // Initialize selection tracking
            SelectedGroups.Clear();
            foreach (var group in AvailableGroups)
            {
                SelectedGroups[group.Code] = false;
            }

            _logger.LogInformation("Loaded {CurrentGroupCount} current groups and {AvailableGroupCount} available groups for user {UserId}",
                UserCurrentGroups.Count, AvailableGroups.Count, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading groups for user: {UserId}", userId);
            UserCurrentGroups = new List<SMSApplicationGroup>();
            AvailableGroups = AllApplicationGroups.Where(g => g.IsActive).ToList();

            // Initialize selection tracking even on error
            SelectedGroups.Clear();
            foreach (var group in AvailableGroups)
            {
                SelectedGroups[group.Code] = false;
            }
        }
    }

    private void CloseGroupsModal()
    {
        _showGroupsModal = false;
        _groupManagementUserCode = string.Empty;
        _groupManagementUserDisplayName = string.Empty;
        UserCurrentGroups.Clear();
        AvailableGroups.Clear();
        SelectedGroups.Clear();
    }

    private async Task RemoveUserFromGroup(string groupCode)
    {
        if (string.IsNullOrWhiteSpace(groupCode) || string.IsNullOrWhiteSpace(_groupManagementUserCode))
        {
            await ShowErrorAsyncNotification("Group code and user code are required.");
            return;
        }

        try
        {
            var command = new RemoveUserFromApplicationGroupCommand(_groupManagementUserCode, groupCode);
            var result = await _mediator.SendAsync(command, CancellationToken.None);

            if (result.IsSuccess)
            {
                await ShowSuccessAsyncNotification("User removed from group successfully.");
            await LoadUserGroups(_groupManagementUserCode);
                StateHasChanged();
            }
            else
            {
                await ShowErrorAsyncNotification(result.Error?.Message ?? "Failed to remove user from group.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing user {UserCode} from group {GroupCode}", _groupManagementUserCode, groupCode);
            await ShowErrorAsyncNotification("Error removing user from group. Please try again.");
        }
    }

    private async Task AssignUserToGroup(string groupCode)
    {
        if (string.IsNullOrWhiteSpace(groupCode) || string.IsNullOrWhiteSpace(_groupManagementUserCode))
        {
            await ShowErrorAsyncNotification("Group code and user code are required.");
            return;
        }

        try
        {
            var command = new AssignUserToApplicationGroupCommand(_groupManagementUserCode, groupCode);
            var result = await _mediator.SendAsync(command, CancellationToken.None);

            if (result.IsSuccess)
            {
                await ShowSuccessAsyncNotification("User assigned to group successfully.");
            await LoadUserGroups(_groupManagementUserCode);
                StateHasChanged();
            }
            else
            {
                await ShowErrorAsyncNotification(result.Error?.Message ?? "Failed to assign user to group.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning user {UserCode} to group {GroupCode}", _groupManagementUserCode, groupCode);
            await ShowErrorAsyncNotification("Error assigning user to group. Please try again.");
        }
    }

    private async Task AssignMultipleGroups()
    {
        if (string.IsNullOrWhiteSpace(_groupManagementUserCode) || !SelectedGroups.Any(s => s.Value))
        {
            await ShowErrorAsyncNotification("User code and at least one group must be selected.");
            return;
        }

        try
        {
            var selectedGroupCodes = SelectedGroups.Where(s => s.Value).Select(s => s.Key).ToArray();
            int successCount = 0;
            int failureCount = 0;

            foreach (var groupCode in selectedGroupCodes)
            {
                try
                {
                var command = new AssignUserToApplicationGroupCommand(_groupManagementUserCode, groupCode);
                    var result = await _mediator.SendAsync(command, CancellationToken.None);

                    if (result.IsSuccess)
                        successCount++;
                    else
                        failureCount++;
                }
                catch (Exception ex)
                {
                _logger.LogError(ex, "Error assigning user {UserCode} to group {GroupCode}", _groupManagementUserCode, groupCode);
                    failureCount++;
                }
            }

            if (successCount > 0)
            {
                var message = $"Successfully assigned user to {successCount} group(s).";
                if (failureCount > 0)
                    message += $" {failureCount} assignment(s) failed.";
                await ShowSuccessAsyncNotification(message);

            await LoadUserGroups(_groupManagementUserCode);
                StateHasChanged();
            }
            else
            {
                await ShowErrorAsyncNotification("Failed to assign user to groups.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning user {UserCode} to multiple groups", _groupManagementUserCode);
            await ShowErrorAsyncNotification("Error assigning user to groups. Please try again.");
        }
    }

    #endregion

    #region Helper Methods

    private void InitializeGroupSelection()
    {
        // Called after loading user groups to initialize the selection dictionary
        SelectedGroups.Clear();
        foreach (var group in AvailableGroups)
        {
            SelectedGroups[group.Code] = false;
        }
    }

    #endregion
}
