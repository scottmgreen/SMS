using SMS_Application.Interfaces;
using SMS_Application.Messaging.Commands;
using SMS_Application.Messaging.Queries;

using SMS_Domain.ValueObjects;

using SMS_Shared.Common;
using SMS_Shared.Configuration;

using SMS3.Components.Shared.UIHelpers;
using SMS3.Configuration.Extensions;

namespace SMS3.Components.Pages.System.UserManagement;

/// <summary>
/// Code-behind for Application Users management page
/// Provides comprehensive user management capabilities for SMS Application Users
/// ? FIXED: Removed manual audit field assignments - pipeline handles automatically
/// </summary>
public partial class ApplicationUsers : ComponentBase
{
    [Inject] private ICurrentUserService CurrentUserService { get; set; } = default!;
    [Inject] private IMediator _mediator { get; set; } = default!;
    [Inject] private ILogger<ApplicationUsers> _logger { get; set; } = default!;
    [Inject] private NavigationManager _navigation { get; set; } = default!;
    [Inject] private DialogService _dialogService { get; set; } = default!;
    [Inject] private INotificationHelper _notificationHelper { get; set; } = default!;

    // Route parameter for edit mode
    [Parameter] public string? Id { get; set; }

    // Data Properties - renamed to avoid conflict
    private List<SMSApplicationUser> ApplicationUsersList { get; set; } = new();
    private List<SMSUserRole> UserRoles { get; set; } = new();
    private SMSApplicationUser? CurrentUser { get; set; }
    private bool IsEditMode { get; set; }
    private string? SuccessMessage { get; set; }
    private string? ErrorMessage { get; set; }

    // Password Modal Properties for Shared Component
    private bool ShowPasswordModal { get; set; }
    private string PasswordUserCode { get; set; } = string.Empty;
    private string PasswordUserDisplayName { get; set; } = string.Empty;

    // ?? NEW: Role Assignment Properties
    private bool ShowRoleAssignmentModal { get; set; }
    private string RoleAssignmentUserCode { get; set; } = string.Empty;
    private string RoleAssignmentUserDisplayName { get; set; } = string.Empty;
    private string? CurrentUserRoleCode { get; set; }
    private string? SelectedRoleCode { get; set; }
    private List<SMSUserRole> AvailableRoles { get; set; } = new();

    // Dynamically get all unique modules from available roles' permissions
    private IEnumerable<string> SMSModules =>
        AvailableRoles
            .Where(role => role.Permissions != null)
            .SelectMany(role => role.Permissions)
            .Where(permission => !string.IsNullOrWhiteSpace(permission.SMSModule))
            .Select(permission => permission.SMSModule!)
            .Distinct()
            .OrderBy(module => module);

    // Group Management Properties
    private bool ShowGroupsModal { get; set; } = false;
    private string GroupManagementUserCode { get; set; } = string.Empty;
    private string GroupManagementUserDisplayName { get; set; } = string.Empty;
    private List<SMSApplicationGroup> AllApplicationGroups { get; set; } = new();
    private List<SMSApplicationGroup> UserCurrentGroups { get; set; } = new();
    private List<SMSApplicationGroup> AvailableGroups { get; set; } = new();
    private Dictionary<string, bool> SelectedGroups { get; set; } = new();

    // Form Models
    private EditUserModel editUser = new();
    private CreateUserModel NewUser = new();
    private string? EditUserRoleCode { get; set; }

    // Create Modal Properties
    private bool ShowCreateModal { get; set; }
    private bool IsSaving { get; set; }

    // Component References
    private RadzenDataGrid<SMSApplicationUser>? usersGrid;

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
            Id ?? "NULL", IsEditMode);
            
        if (!string.IsNullOrWhiteSpace(Id) && !IsEditMode)
        {
            _logger.LogInformation("Calling LoadUserForEdit from OnParametersSetAsync with Id: {Id}", Id);
            await LoadUserForEdit(Id);
        }
        else if (string.IsNullOrWhiteSpace(Id) && IsEditMode)
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
            ShowErrorAsyncNotification("Error loading data. Please refresh the page.");
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
                userResult.IsSuccess, userResult.Value != null);

            if (userResult.IsFailure || userResult.Value == null)
            {
                _logger.LogWarning("User not found for ID: {Id}", id);
                ShowErrorAsyncNotification($"User not found: {id}");
                _navigation.NavigateTo("/System/UserManagement/ApplicationUsers");
                return;
            }

            CurrentUser = userResult.Value;
            IsEditMode = true;

            _logger.LogInformation("Edit mode set - CurrentUser: {UserCode}, IsEditMode: {IsEditMode}", 
                CurrentUser?.Code, IsEditMode);

            // Populate edit form
            editUser = new EditUserModel
            {
                FirstName = CurrentUser.FirstName?.Value ?? "",
                LastName = CurrentUser.LastName?.Value ?? ""
            };

            // Set the role code for dropdown binding
            EditUserRoleCode = CurrentUser.UserRole?.Code;

            _logger.LogInformation("Edit form populated - FirstName: {FirstName}, LastName: {LastName}, RoleCode: {RoleCode}", 
                editUser.FirstName, editUser.LastName, EditUserRoleCode);

            StateHasChanged();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading user for edit: {UserId}", id);
            ShowErrorAsyncNotification("An error occurred while loading the user for editing.");
            
            // Navigate back to main list on error
            _navigation.NavigateTo("/System/UserManagement/ApplicationUsers");
        }
    }

    #endregion

    #region ?? NEW: Role Assignment Methods
    private bool IsPermissionGranted(SMSUserRole role, string module, string action)
    {
        if (role?.Permissions == null) return false;

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
        RoleAssignmentUserCode = userCode;
        RoleAssignmentUserDisplayName = userDisplayName;
        CurrentUserRoleCode = currentRoleCode;
        SelectedRoleCode = currentRoleCode;
        ShowRoleAssignmentModal = true;
        StateHasChanged();
    }

    private void CloseRoleAssignmentModal()
    {
        ShowRoleAssignmentModal = false;
        RoleAssignmentUserCode = string.Empty;
        RoleAssignmentUserDisplayName = string.Empty;
        CurrentUserRoleCode = null;
        SelectedRoleCode = null;
        StateHasChanged();
    }

    private async Task AssignUserRole()
    {
        if (string.IsNullOrEmpty(RoleAssignmentUserCode) || string.IsNullOrEmpty(SelectedRoleCode))
        {
            ShowErrorAsyncNotification("Invalid user or role selection.");
            return;
        }

        try
        {
            IsSaving = true;
            StateHasChanged();

            // Get the user
            var userQuery = new GetSMSApplicationUserByCodeQuery(RoleAssignmentUserCode);
            var userResult = await _mediator.SendAsync(userQuery, CancellationToken.None);

            if (userResult.IsFailure || userResult.Value == null)
            {
                ShowErrorAsyncNotification("User not found.");
                return;
            }

            var user = userResult.Value;

            // Get the selected role
            var selectedRole = AvailableRoles.FirstOrDefault(r => r.Code == SelectedRoleCode);
            if (selectedRole == null)
            {
                ShowErrorAsyncNotification("Selected role not found.");
                return;
            }
            user.SMSUserType = SMSUserType.Application;
            
            // ? FIXED: Only set business fields - let pipeline handle audit fields
            user.UserRole = selectedRole;
            // ? REMOVED: user.UpdatedBy = CurrentUserService?.UserDisplayName;
            // ? REMOVED: user.UpdatedDate = DateTime.UtcNow;

            // Update user - pipeline will automatically set UpdatedBy/UpdatedDate
            var updateCommand = new UpdateSMSApplicationUserCommand(user);
            var updateResult = await _mediator.SendAsync(updateCommand, CancellationToken.None);

            if (updateResult.IsSuccess)
            {
                ShowSuccessAsyncNotification($"Role '{selectedRole.Name}' successfully assigned to {RoleAssignmentUserDisplayName}.");

                // Refresh data and close modal
                await LoadDataAsync();
                CloseRoleAssignmentModal();
            }
            else
            {
                ShowErrorAsyncNotification($"Failed to assign role: {updateResult.Error?.Message}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning role to user {UserCode}", RoleAssignmentUserCode);
            ShowErrorAsyncNotification("An error occurred while assigning the role. Please try again.");
        }
        finally
        {
            IsSaving = false;
            StateHasChanged();
        }
    }

    private async Task RemoveUserRole()
    {
        if (string.IsNullOrEmpty(RoleAssignmentUserCode))
        {
            ShowErrorAsyncNotification("Invalid user selection.");
            return;
        }

        try
        {
            IsSaving = true;
            StateHasChanged();

            // Get the user
            var userQuery = new GetSMSApplicationUserByCodeQuery(RoleAssignmentUserCode);
            var userResult = await _mediator.SendAsync(userQuery, CancellationToken.None);

            if (userResult.IsFailure || userResult.Value == null)
            {
                ShowErrorAsyncNotification("User not found.");
                return;
            }

            var user = userResult.Value;

            // ? FIXED: Only set business fields - let pipeline handle audit fields
            user.UserRole = null;
            // ? REMOVED: user.UpdatedBy = CurrentUserService?.UserDisplayName;
            // ? REMOVED: user.UpdatedDate = DateTime.UtcNow;

            // Update user - pipeline will automatically set UpdatedBy/UpdatedDate
            var updateCommand = new UpdateSMSApplicationUserCommand(user);
            var updateResult = await _mediator.SendAsync(updateCommand, CancellationToken.None);

            if (updateResult.IsSuccess)
            {
                ShowSuccessAsyncNotification($"Role successfully removed from {RoleAssignmentUserDisplayName}.");

                // Refresh data and close modal
                await LoadDataAsync();
                CloseRoleAssignmentModal();
            }
            else
            {
                ShowErrorAsyncNotification($"Failed to remove role: {updateResult.Error?.Message}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing role from user {UserCode}", RoleAssignmentUserCode);
            ShowErrorAsyncNotification("An error occurred while removing the role. Please try again.");
        }
        finally
        {
            IsSaving = false;
            StateHasChanged();
        }
    }

    #endregion

    #region CRUD Operations

    private async Task ShowCreateDialog()
    {
        NewUser = new CreateUserModel
        {
            TwoFactorEnabled = false
        };
        NewIsActive = true; // ADDED: Initialize the property
        ShowCreateModal = true;
        StateHasChanged();
    }

    private async Task CreateUser()
    {
        if (!IsCreateFormValid)
        {
            ShowErrorAsyncNotification("Please fill in all required fields.");
            return;
        }

        try
        {
            IsSaving = true;
            StateHasChanged();

            // ?? NEW: Get selected role if provided
            SMSUserRole? selectedRole = null;
            if (!string.IsNullOrEmpty(NewUser.UserRoleCode))
            {
                selectedRole = AvailableRoles.FirstOrDefault(r => r.Code == NewUser.UserRoleCode);
            }

            // Create user entity
            var userId = new SMSApplicationUserID($"AU-0000");
            var user = new SMSApplicationUser(userId)
            {
                Code = userId.Value,
                FirstName = FirstName.Create(NewUser.FirstName).Value,
                LastName = LastName.Create(NewUser.LastName).Value,
                UserName = UserName.Create(NewUser.UserName).Value,
                Password = Password.Create(NewUser.Password).Value,
                UserRole = selectedRole, // ?? NEW: Assign role during creation
                TwoFactorEnabled = NewUser.TwoFactorEnabled, // ?? NEW: Set 2FA requirement
                IsActive = NewIsActive, // UPDATED: Use NewIsActive property
                SMSUserType = SMSUserType.Application
                // ? FIXED: Removed manual audit field assignments
                // ? REMOVED: CreatedBy = CurrentUserService?.UserDisplayName,
                // ? REMOVED: CreatedDate = DateTime.UtcNow
            };

            // Create user - pipeline will automatically set CreatedBy/CreatedDate
            var command = new CreateSMSApplicationUserCommand(user);
            var result = await _mediator.SendAsync(command, CancellationToken.None);

            if (result.IsSuccess)
            {
                var roleText = selectedRole != null ? $" with role '{selectedRole.Name}'" : "";
                ShowSuccessAsyncNotification($"Application user '{NewUser.FirstName} {NewUser.LastName}' created successfully{roleText}!");
                CloseCreateModal();
                await LoadDataAsync();
            }
            else
            {
                ShowErrorAsyncNotification(result.Error?.Message ?? "Failed to create application user.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating application user");
            ShowErrorAsyncNotification("Error creating application user. Please try again.");
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
        NewUser = new CreateUserModel
        {
            TwoFactorEnabled = false
        };
        NewIsActive = true; // ADDED: Reset the property
        StateHasChanged();
    }

    private bool IsCreateFormValid =>
        !string.IsNullOrWhiteSpace(NewUser.FirstName) &&
        !string.IsNullOrWhiteSpace(NewUser.LastName) &&
        !string.IsNullOrWhiteSpace(NewUser.UserName) &&
        !string.IsNullOrWhiteSpace(NewUser.Password);

    private void EditUser(string userId)
    {
        try
        {
            _logger.LogInformation("Editing user: {UserId}", userId);
            
            // Use regular navigation for edit routes since secure navigation has issues with route parameters
            // TODO: Fix SecureNavigation to properly handle route parameters
            _navigation.NavigateTo($"/System/UserManagement/ApplicationUsers/Edit/{userId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error navigating to edit user: {UserId}", userId);
            ShowErrorAsyncNotification("Error opening user editor");
        }
    }

    private async Task UpdateUser(EditUserModel model)
    {
        try
        {
            if (CurrentUser == null)
            {
                ShowErrorAsyncNotification("No user selected for update.");
                return;
            }

            // ? FIXED: Only update business fields - let pipeline handle audit fields
            CurrentUser.FirstName = FirstName.Create(model.FirstName).Value;
            CurrentUser.LastName = LastName.Create(model.LastName).Value;
            CurrentUser.SMSUserType = SMSUserType.Application;
            
            // Update role if changed
            if (!string.IsNullOrEmpty(EditUserRoleCode))
            {
                var selectedRole = AvailableRoles.FirstOrDefault(r => r.Code == EditUserRoleCode);
                CurrentUser.UserRole = selectedRole;
            }
            else
            {
                CurrentUser.UserRole = null;
            }
            
            // ? REMOVED: Manual audit field assignments
            // CurrentUser.UpdatedBy = CurrentUserService?.UserDisplayName;
            // CurrentUser.UpdatedDate = DateTime.UtcNow;

            // Update user - pipeline will automatically set UpdatedBy/UpdatedDate
            var updateCommand = new UpdateSMSApplicationUserCommand(CurrentUser);
            var result = await _mediator.SendAsync(updateCommand, CancellationToken.None);

            if (result.IsSuccess)
            {
                ShowSuccessAsyncNotification($"User '{CurrentUser.UserName.Value}' has been updated successfully!");
                
                // Reset form state
                IsEditMode = false;
                CurrentUser = null;
                
                // Navigate back to main list with success
                _navigation.NavigateToSecure("/System/UserManagement/ApplicationUsers");
            }
            else
            {
                ShowErrorAsyncNotification(result.Error?.Message ?? "Failed to update application user.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating application user: {UserId}", CurrentUser?.Code);
            ShowErrorAsyncNotification("Error updating application user. Please try again.");
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
                ShowSuccessAsyncNotification("Application user deleted successfully.");
                await LoadDataAsync();
            }
            else
            {
                ShowErrorAsyncNotification(result.Error?.Message ?? "Failed to delete application user.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting application user: {UserId}", userId);
            ShowErrorAsyncNotification("Error deleting application user. Please try again.");
        }
    }

    #endregion

    #region Password Management

    private void OpenPasswordChangeModal(string userCode, string displayName)
    {
        PasswordUserCode = userCode;
        PasswordUserDisplayName = displayName;
        ShowPasswordModal = true;
        StateHasChanged();
    }

    private void ClosePasswordChangeModal()
    {
        ShowPasswordModal = false;
        PasswordUserCode = string.Empty;
        PasswordUserDisplayName = string.Empty;
        StateHasChanged();
    }

    private async Task OnPasswordChangedSuccess()
    {
        // Password was changed successfully by the modal
        ShowSuccessAsyncNotification($"Password updated successfully for {PasswordUserDisplayName}.");
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
        IsEditMode = false;
        CurrentUser = null;
        EditUserRoleCode = null;
        editUser = new EditUserModel();
        _navigation.NavigateToSecure("/System/UserManagement/ApplicationUsers");
    }

    private void NavigateToUserManagement()
    {
        _navigation.NavigateToSecure("/System/UserManagement");
    }

    private async Task ExportUsers()
    {
        // TODO: Implement export functionality
        ShowInfoAsyncNotification("Export functionality will be implemented soon.");
    }

    #endregion

    #region Notifications

    private void ShowSuccessAsyncNotification(string message)
    {
        _notificationHelper.ShowSuccessAsync( message);
    }

    private void ShowErrorAsyncNotification(string message)
    {
        _notificationHelper.ShowErrorAsync( message);
    }

    private void ShowInfoAsyncNotification(String message)
    {
        _notificationHelper.ShowInfoAsync( message);
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
    private bool NewIsActive { get; set; } = true;

    #endregion

    #region Group Management

    private async Task ManageGroups(string userId, string displayName)
    {
        try
        {
            GroupManagementUserCode = userId;
            GroupManagementUserDisplayName = displayName;

            await LoadUserGroups(userId);
            ShowGroupsModal = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error opening group management for user: {UserId}", userId);
            ShowErrorAsyncNotification("Error loading user groups. Please try again.");
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
        ShowGroupsModal = false;
        GroupManagementUserCode = string.Empty;
        GroupManagementUserDisplayName = string.Empty;
        UserCurrentGroups.Clear();
        AvailableGroups.Clear();
        SelectedGroups.Clear();
    }

    private async Task RemoveUserFromGroup(string groupCode)
    {
        if (string.IsNullOrWhiteSpace(groupCode) || string.IsNullOrWhiteSpace(GroupManagementUserCode))
        {
            ShowErrorAsyncNotification("Group code and user code are required.");
            return;
        }

        try
        {
            var command = new RemoveUserFromApplicationGroupCommand(GroupManagementUserCode, groupCode);
            var result = await _mediator.SendAsync(command, CancellationToken.None);

            if (result.IsSuccess)
            {
                ShowSuccessAsyncNotification("User removed from group successfully.");
                await LoadUserGroups(GroupManagementUserCode);
                StateHasChanged();
            }
            else
            {
                ShowErrorAsyncNotification(result.Error?.Message ?? "Failed to remove user from group.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing user {UserCode} from group {GroupCode}", GroupManagementUserCode, groupCode);
            ShowErrorAsyncNotification("Error removing user from group. Please try again.");
        }
    }

    private async Task AssignUserToGroup(string groupCode)
    {
        if (string.IsNullOrWhiteSpace(groupCode) || string.IsNullOrWhiteSpace(GroupManagementUserCode))
        {
            ShowErrorAsyncNotification("Group code and user code are required.");
            return;
        }

        try
        {
            var command = new AssignUserToApplicationGroupCommand(GroupManagementUserCode, groupCode);
            var result = await _mediator.SendAsync(command, CancellationToken.None);

            if (result.IsSuccess)
            {
                ShowSuccessAsyncNotification("User assigned to group successfully.");
                await LoadUserGroups(GroupManagementUserCode);
                StateHasChanged();
            }
            else
            {
                ShowErrorAsyncNotification(result.Error?.Message ?? "Failed to assign user to group.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning user {UserCode} to group {GroupCode}", GroupManagementUserCode, groupCode);
            ShowErrorAsyncNotification("Error assigning user to group. Please try again.");
        }
    }

    private async Task AssignMultipleGroups()
    {
        if (string.IsNullOrWhiteSpace(GroupManagementUserCode) || !SelectedGroups.Any(s => s.Value))
        {
            ShowErrorAsyncNotification("User code and at least one group must be selected.");
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
                    var command = new AssignUserToApplicationGroupCommand(GroupManagementUserCode, groupCode);
                    var result = await _mediator.SendAsync(command, CancellationToken.None);

                    if (result.IsSuccess)
                        successCount++;
                    else
                        failureCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error assigning user {UserCode} to group {GroupCode}", GroupManagementUserCode, groupCode);
                    failureCount++;
                }
            }

            if (successCount > 0)
            {
                var message = $"Successfully assigned user to {successCount} group(s).";
                if (failureCount > 0)
                    message += $" {failureCount} assignment(s) failed.";
                ShowSuccessAsyncNotification(message);

                await LoadUserGroups(GroupManagementUserCode);
                StateHasChanged();
            }
            else
            {
                ShowErrorAsyncNotification("Failed to assign user to groups.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning user {UserCode} to multiple groups", GroupManagementUserCode);
            ShowErrorAsyncNotification("Error assigning user to groups. Please try again.");
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