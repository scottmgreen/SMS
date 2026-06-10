using SMS_Application.Interfaces;
using SMS_Domain.Events;
using SMS_Shared.Configuration;

using SMS3.Components.Shared.UIHelpers;

namespace SMS3.Components.Pages.SMSSystem.UserManagement;

/// <summary>
/// Code-behind for Stakeholder Users management page  
/// Handles external stakeholder user management
/// ? FIXED: Removed manual audit field assignments - pipeline handles automatically
/// </summary>
public partial class StakeholderUsers : ComponentBase
{
    [Inject] private IBaseMediator _mediator { get; set; } = default!;
    [Inject] private ILogger<StakeholderUsers> _logger { get; set; } = default!;
    [Inject] private NavigationManager _navigation { get; set; } = default!;
    [Inject] private DialogService _dialogService { get; set; } = default!;
    [Inject] private IBaseEventBus _eventBus { get; set; } = default!;
    [Inject] private ICurrentUserService _currentUserService { get; set; } = default!;

    // Data Properties
    private List<SMSStakeholderUser> StakeholderUsersList { get; set; } = new();
    private List<SMSUserRole> UserRoles { get; set; } = new();
    private string? SuccessMessage { get; set; }
    private string? ErrorMessage { get; set; }

    // Predefined stakeholder types
    //private static readonly string[] StakeholderTypes = { "SUT-0001", "SUT-0002", "SUT-0003" };
    private string[] StakeholderTypes { get; set; } = Array.Empty<string>();

    // Form Models
    private EditStakeholderUserModel _editUser = new();
    private CreateStakeholderUserModel _newUser = new();

    // Create Modal Properties  
    private bool ShowCreateModal { get; set; }
    private bool IsSaving { get; set; }

    // Edit Modal Properties
    private bool ShowEditModal { get; set; }
    private SMSStakeholderUser? CurrentEditUser { get; set; }

    // Role Assignment Modal Properties
    private bool ShowRoleAssignmentModal { get; set; }
    private string RoleAssignmentUserCode { get; set; } = string.Empty;
    private string RoleAssignmentUserDisplayName { get; set; } = string.Empty;
    private string? CurrentUserRoleCode { get; set; }
    private string? SelectedRoleCode { get; set; }

    // Legacy properties for compatibility
    private bool ShowRoleModal { get; set; }
    private string RoleUserCode { get; set; } = string.Empty;
    private string RoleUserDisplayName { get; set; } = string.Empty;
    private string CurrentRoleCode { get; set; } = string.Empty;

    // Dynamically get all unique modules from available roles' permissions
    private IEnumerable<string> SMSModules =>
        UserRoles
            .Where(role => role.Permissions is not null)
            .SelectMany(role => role.Permissions)
            .Where(permission => !string.IsNullOrWhiteSpace(permission.SMSModule))
            .Select(permission => permission.SMSModule!)
            .Distinct()
            .OrderBy(module => module);
    // Component References
    private RadzenDataGrid<SMSStakeholderUser>? _usersGrid;

    protected override async Task OnInitializedAsync()
    {
        await LoadDataAsync();
    }

    #region Data Loading

    private async Task LoadDataAsync()
    {
        try
        {
            // Load Stakeholder Users
            var stakeholderUsersQuery = new GetAllSMSStakeholderUsersQuery();
            var stakeholderUsersResult = await _mediator.SendAsync(stakeholderUsersQuery, CancellationToken.None);
            StakeholderUsersList = stakeholderUsersResult.IsSuccess ?
                stakeholderUsersResult.Value?.ToList() ?? new List<SMSStakeholderUser>() :
                new List<SMSStakeholderUser>();

            // Load User Roles
            var userRolesQuery = new GetAllSMSUserRolesQuery();
            var userRolesResult = await _mediator.SendAsync(userRolesQuery, CancellationToken.None);
            UserRoles = userRolesResult.IsSuccess ?
                userRolesResult.Value?.ToList() ?? new List<SMSUserRole>() :
                new List<SMSUserRole>();

            // Load Stakeholder Groups for group management
            var groupsQuery = new GetAllSMSStakeholderGroupsQuery();
            var groupsResult = await _mediator.SendAsync(groupsQuery, CancellationToken.None);
            AllStakeholderGroups = groupsResult.IsSuccess ?
                groupsResult.Value?.ToList() ?? new List<SMSStakeholderGroup>() :
                new List<SMSStakeholderGroup>();


            StakeholderTypes = SMSStakeholderType.GetAllValuesAsStringArray();


            _logger.LogInformation("Loaded {UserCount} stakeholder users, {RoleCount} user roles, and {GroupCount} stakeholder groups",
                StakeholderUsersList.Count, UserRoles.Count, AllStakeholderGroups.Count);

            StateHasChanged();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading stakeholder users data");
            await ShowErrorAsyncNotification("Error loading data. Please refresh the page.");
        }
    }

    #endregion

    #region CRUD Operations

    private async Task ShowCreateDialog()
    {
        _newUser = new CreateStakeholderUserModel
        {
            IsActive = true,  // ADDED: Set default value
            IsPOPEmployee = false,
            TwoFactorEnabled = false
        };
        ShowCreateModal = true;
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
            IsSaving = true;
            StateHasChanged();

            // Create user entity
            var userId = new SMSStakeholderUserID($"SU-0000");
            var user = new SMSStakeholderUser(userId)
            {
                Code = userId.Value,
                FirstName = FirstName.Create(_newUser.FirstName).Value,
                LastName = LastName.Create(_newUser.LastName).Value,
                UserName = UserName.Create(_newUser.UserName).Value,
                Password = Password.Create(_newUser.Password).Value,
                StakeholderType = _newUser.StakeholderType,
                Organization = _newUser.Organization,
                IsActive = _newUser.IsActive,
                IsPOPEmployee = _newUser.IsPOPEmployee,
                TwoFactorEnabled = _newUser.TwoFactorEnabled,
                SMSUserType = SMSUserType.Stakeholder
            };

            // Assign user role if specified
            if (!string.IsNullOrWhiteSpace(_newUser.UserRoleCode))
            {
                var roleQuery = new GetSMSUserRoleByIdQuery(_newUser.UserRoleCode);
                var roleResult = await _mediator.SendAsync(roleQuery, CancellationToken.None);
                if (roleResult.IsSuccess && roleResult.Value is not null)
                {
                    user.UserRole = roleResult.Value;
                }
            }

            var command = new CreateSMSStakeholderUserCommand(user);
            var result = await _mediator.SendAsync(command, CancellationToken.None);

            if (result.IsSuccess)
            {
                await ShowSuccessAsyncNotification($"Stakeholder user '{_newUser.FirstName} {_newUser.LastName}' created successfully.");
                CloseCreateModal();
                await LoadDataAsync();
            }
            else
            {
                await ShowErrorAsyncNotification(result.Error?.Message ?? "Failed to create stakeholder user.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating stakeholder user");
            await ShowErrorAsyncNotification("Error creating stakeholder user. Please try again.");
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
        _newUser = new CreateStakeholderUserModel
        {
            IsActive = true,  // ADDED: Set default value
            IsPOPEmployee = false,
            TwoFactorEnabled = false
        };
        StateHasChanged();
    }

    private bool _isCreateFormValid =>
        !string.IsNullOrWhiteSpace(_newUser.FirstName) &&
        !string.IsNullOrWhiteSpace(_newUser.LastName) &&
        !string.IsNullOrWhiteSpace(_newUser.UserName) &&
        !string.IsNullOrWhiteSpace(_newUser.Password) &&
        !string.IsNullOrWhiteSpace(_newUser.StakeholderType) &&
        !string.IsNullOrWhiteSpace(_newUser.Organization);

    // Transform stakeholder types for dropdown
    private IEnumerable<object> StakeholderTypesForDropdown => StakeholderTypes.Select(type => new
    {
        Value = type,
        Text = GetStakeholderTypeDisplay(type)
    });

    private async Task ShowEditDialog(SMSStakeholderUser user)
    {
        CurrentEditUser = user;
        _editUser = new EditStakeholderUserModel
        {
            UserId = user.Code,
            FirstName = user.FirstName?.Value ?? "",
            LastName = user.LastName?.Value ?? "",
            StakeholderType = user.StakeholderType,
            Organization = user.Organization,
            UserRoleCode = user.UserRole?.Code ?? "",
            IsActive = user.IsActive,
            IsPOPEmployee = user.IsPOPEmployee,
            TwoFactorEnabled = user.TwoFactorEnabled
        };
        ShowEditModal = true;
        StateHasChanged();
    }

    private async Task UpdateUser()
    {
        if (!_isEditFormValid)
        {
            await ShowErrorAsyncNotification("Please fill in all required fields.");
            return;
        }

        try
        {
            IsSaving = true;
            StateHasChanged();

            if (CurrentEditUser is null)
            {
                await ShowErrorAsyncNotification("No user selected for update.");
                return;
            }

            // ? FIXED: Only set business fields - let pipeline handle audit fields
            CurrentEditUser.FirstName = FirstName.Create(_editUser.FirstName).Value;
            CurrentEditUser.LastName = LastName.Create(_editUser.LastName).Value;
            CurrentEditUser.StakeholderType = _editUser.StakeholderType;
            CurrentEditUser.Organization = _editUser.Organization;
            CurrentEditUser.IsActive = _editUser.IsActive;
            CurrentEditUser.IsPOPEmployee = _editUser.IsPOPEmployee;
            CurrentEditUser.TwoFactorEnabled = _editUser.TwoFactorEnabled;
            CurrentEditUser.SMSUserType = SMSUserType.Stakeholder;
            
                        
            // Update user role if specified
            if (!string.IsNullOrWhiteSpace(_editUser.UserRoleCode))
            {
                var roleQuery = new GetSMSUserRoleByIdQuery(_editUser.UserRoleCode);
                var roleResult = await _mediator.SendAsync(roleQuery, CancellationToken.None);
                if (roleResult.IsSuccess && roleResult.Value is not null)
                {
                    CurrentEditUser.UserRole = roleResult.Value;
                }
            }
            // Note: If no role is specified, we keep the existing UserRole unchanged

            // Update user - pipeline will automatically set UpdatedBy/UpdatedDate
            var updateCommand = new UpdateSMSStakeholderUserCommand(CurrentEditUser);
            var result = await _mediator.SendAsync(updateCommand, CancellationToken.None);

            if (result.IsSuccess)
            {
                await ShowSuccessAsyncNotification($"Stakeholder user '{_editUser.FirstName} {_editUser.LastName}' updated successfully.");
                CloseEditModal();
                await LoadDataAsync();
            }
            else
            {
                await ShowErrorAsyncNotification(result.Error?.Message ?? "Failed to update stakeholder user.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating stakeholder user: {UserId}", _editUser.UserId);
            await ShowErrorAsyncNotification("Error updating stakeholder user. Please try again.");
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
        CurrentEditUser = null;
        _editUser = new EditStakeholderUserModel();
        StateHasChanged();
    }

    private bool _isEditFormValid =>
        !string.IsNullOrWhiteSpace(_editUser.FirstName) &&
        !string.IsNullOrWhiteSpace(_editUser.LastName) &&
        !string.IsNullOrWhiteSpace(_editUser.StakeholderType) &&
        !string.IsNullOrWhiteSpace(_editUser.Organization);

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
            var stakeholderUserId = new SMSStakeholderUserID(userId);
            var command = new DeleteSMSStakeholderUserCommand(stakeholderUserId);
            var result = await _mediator.SendAsync(command, CancellationToken.None);

            if (result.IsSuccess)
            {
                await ShowSuccessAsyncNotification("Stakeholder user deleted successfully.");
                await LoadDataAsync();
            }
            else
            {
                await ShowErrorAsyncNotification(result.Error?.Message ?? "Failed to delete stakeholder user.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting stakeholder user: {UserId}", userId);
            await ShowErrorAsyncNotification("Error deleting stakeholder user. Please try again.");
        }
    }

    #endregion

    #region Password Management

    // Password Modal Properties for Shared Component
    private bool ShowPasswordModal { get; set; }
    private string PasswordUserCode { get; set; } = string.Empty;
    private string PasswordUserDisplayName { get; set; } = string.Empty;

    // Group Management Properties
    private bool ShowGroupsModal { get; set; } = false;
    private string GroupManagementUserCode { get; set; } = string.Empty;
    private string GroupManagementUserDisplayName { get; set; } = string.Empty;
    private List<SMSStakeholderGroup> AllStakeholderGroups { get; set; } = new();
    private List<SMSStakeholderGroup> UserCurrentGroups { get; set; } = new();
    private List<SMSStakeholderGroup> AvailableGroups { get; set; } = new();
    private Dictionary<string, bool> SelectedGroups { get; set; } = new();

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
        await ShowSuccessAsyncNotification($"Password updated successfully for {PasswordUserDisplayName}.");
    }

    // Legacy password methods - kept for compatibility
    private async Task ShowPasswordDialog(string userId, string displayName)
    {
        OpenPasswordChangeModal(userId, displayName);
    }

    private async Task UpdatePassword(string userId, string newPassword)
    {
        // Legacy method - now handled by shared component
        await ShowInfoAsyncNotification("Please use the password change modal to update passwords.");
    }

    #endregion

    #region Role Assignment

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
            await ShowErrorAsyncNotification("Invalid user or role selection.");
            return;
        }

        try
        {
            IsSaving = true;
            StateHasChanged();

            // Get the user
            var userQuery = new GetSMSStakeholderUserByCodeQuery(RoleAssignmentUserCode);
            var userResult = await _mediator.SendAsync(userQuery, CancellationToken.None);

            if (userResult.IsFailure || userResult.Value is null)
            {
                await ShowErrorAsyncNotification("User not found.");
                return;
            }

            var user = userResult.Value;

            // Get the selected role
            var selectedRole = UserRoles.FirstOrDefault(r => r.Code == SelectedRoleCode);
            if (selectedRole is null)
            {
                await ShowErrorAsyncNotification("Selected role not found.");
                return;
            }

            user.SMSUserType = SMSUserType.Stakeholder;
            user.UserRole = selectedRole;

            // Update user - pipeline will automatically set UpdatedBy/UpdatedDate
            var updateCommand = new UpdateSMSStakeholderUserCommand(user);
            var updateResult = await _mediator.SendAsync(updateCommand, CancellationToken.None);

            if (updateResult.IsSuccess)
            {
                await ShowSuccessAsyncNotification($"Role '{selectedRole.Name}' successfully assigned to {RoleAssignmentUserDisplayName}.");

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
            _logger.LogError(ex, "Error assigning role to user {UserCode}", RoleAssignmentUserCode);
            await ShowErrorAsyncNotification("An error occurred while assigning the role. Please try again.");
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
            await ShowErrorAsyncNotification("Invalid user selection.");
            return;
        }

        try
        {
            IsSaving = true;
            StateHasChanged();

            // Get the user
            var userQuery = new GetSMSStakeholderUserByCodeQuery(RoleAssignmentUserCode);
            var userResult = await _mediator.SendAsync(userQuery, CancellationToken.None);

            if (userResult.IsFailure || userResult.Value is null)
            {
                await ShowErrorAsyncNotification("User not found.");
                return;
            }

            var user = userResult.Value;
            user.UserRole = null!; // Explicitly assign null with null-forgiving operator

            // Update user - pipeline will automatically set UpdatedBy/UpdatedDate
            var updateCommand = new UpdateSMSStakeholderUserCommand(user);
            var updateResult = await _mediator.SendAsync(updateCommand, CancellationToken.None);

            if (updateResult.IsSuccess)
            {
                await ShowSuccessAsyncNotification($"Role successfully removed from {RoleAssignmentUserDisplayName}.");

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
            _logger.LogError(ex, "Error removing role from user {UserCode}", RoleAssignmentUserCode);
            await ShowErrorAsyncNotification("An error occurred while removing the role. Please try again.");
        }
        finally
        {
            IsSaving = false;
            StateHasChanged();
        }
    }

    // Legacy methods for compatibility
    private async Task ShowRoleDialog(string userId, string displayName, string? currentRoleCode)
    {
        OpenRoleAssignmentModal(userId, displayName, currentRoleCode);
    }

    private void CloseRoleModal()
    {
        CloseRoleAssignmentModal();
    }

    private async Task AssignRoleFromModal(string? userRoleCode)
    {
        SelectedRoleCode = userRoleCode;
        await AssignUserRole();
    }

    private readonly List<StatusOption> _isActiveOptions = StatusOptions.ActiveInactiveOptions;

    private readonly List<StatusOption> _isPopEmployeeOptions = StatusOptions.YesNoOptions;
    
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
            await ShowErrorAsyncNotification("Error loading user groups. Please try again.");
        }
    }

    private async Task LoadUserGroups(string userId)
    {
        try
        {
            // Load groups that this user is currently assigned to
            var userGroupsQuery = new GetSMSStakeholderGroupsByUserCodeQuery(userId);
            var userGroupsResult = await _mediator.SendAsync(userGroupsQuery, CancellationToken.None);
            UserCurrentGroups = userGroupsResult.IsSuccess ?
                userGroupsResult.Value?.ToList() ?? new List<SMSStakeholderGroup>() :
                new List<SMSStakeholderGroup>();

            // Calculate available groups (groups the user is not currently in)
            var currentGroupCodes = UserCurrentGroups.Select(g => g.Code).ToHashSet();
            AvailableGroups = AllStakeholderGroups
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
            UserCurrentGroups = new List<SMSStakeholderGroup>();
            AvailableGroups = AllStakeholderGroups.Where(g => g.IsActive).ToList();

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
            await ShowErrorAsyncNotification("Group code and user code are required.");
            return;
        }

        try
        {
            var groupId = new SMSStakeholderGroupID(groupCode);
            var command = new RemoveUserFromStakeholderGroupCommand(GroupManagementUserCode, groupId);
            var result = await _mediator.SendAsync(command, CancellationToken.None);

            if (result.IsSuccess)
            {
                await ShowSuccessAsyncNotification("User removed from group successfully.");
                await LoadUserGroups(GroupManagementUserCode);
                StateHasChanged();
            }
            else
            {
                await ShowErrorAsyncNotification(result.Error?.Message ?? "Failed to remove user from group.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing user {UserCode} from group {GroupCode}", GroupManagementUserCode, groupCode);
            await ShowErrorAsyncNotification("Error removing user from group. Please try again.");
        }
    }

    private async Task AssignUserToGroup(string groupCode)
    {
        if (string.IsNullOrWhiteSpace(groupCode) || string.IsNullOrWhiteSpace(GroupManagementUserCode))
        {
            await ShowErrorAsyncNotification("Group code and user code are required.");
            return;
        }

        try
        {
            var groupId = new SMSStakeholderGroupID(groupCode);
            var command = new AssignUserToStakeholderGroupCommand(GroupManagementUserCode, groupId);
            var result = await _mediator.SendAsync(command, CancellationToken.None);

            if (result.IsSuccess)
            {
                await ShowSuccessAsyncNotification("User assigned to group successfully.");
                await LoadUserGroups(GroupManagementUserCode);
                StateHasChanged();
            }
            else
            {
                await ShowErrorAsyncNotification(result.Error?.Message ?? "Failed to assign user to group.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning user {UserCode} to group {GroupCode}", GroupManagementUserCode, groupCode);
            await ShowErrorAsyncNotification("Error assigning user to group. Please try again.");
        }
    }

    private async Task AssignMultipleGroups()
    {
        if (string.IsNullOrWhiteSpace(GroupManagementUserCode) || !SelectedGroups.Any(s => s.Value))
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
                    var groupId = new SMSStakeholderGroupID(groupCode);
                    var command = new AssignUserToStakeholderGroupCommand(GroupManagementUserCode, groupId);
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
                await ShowSuccessAsyncNotification(message);

                await LoadUserGroups(GroupManagementUserCode);
                StateHasChanged();
            }
            else
            {
                await ShowErrorAsyncNotification("Failed to assign user to groups.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning user {UserCode} to multiple groups", GroupManagementUserCode);
            await ShowErrorAsyncNotification("Error assigning user to groups. Please try again.");
        }
    }

    #endregion

    #region UI Helper Methods

    private async Task ExportUsers()
    {
        await ShowInfoAsyncNotification("Export functionality will be implemented soon.");
    }

    

    private string GetStakeholderTypeDisplay(string stakeholderType)
    {
        var stakeholderTypeEnum = SMSStakeholderType.FromValue(stakeholderType);
        return stakeholderTypeEnum?.Name ?? stakeholderType;
    }

    #endregion

    #region Notification Methods (EventBus-Driven)

    private async Task ShowErrorAsyncNotification(string message)
    {
        await _eventBus.PublishUIEventAsync(UINotificationEvent.Error("Error", message));
    }

    private async Task ShowSuccessAsyncNotification(string message)
    {
        await _eventBus.PublishUIEventAsync(UINotificationEvent.Success("Success", message));
    }

    private async Task ShowInfoAsyncNotification(string message)
    {
        await _eventBus.PublishUIEventAsync(UINotificationEvent.Info("Information", message));
    }

    #endregion

    #region Models

    public class CreateStakeholderUserModel
    {
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string UserName { get; set; } = "";
        public string Password { get; set; } = "";
        public string StakeholderType { get; set; } = "";
        public string Organization { get; set; } = "";
        public string UserRoleCode { get; set; } = "";
        public bool IsPOPEmployee { get; set; } = false;
        public bool TwoFactorEnabled { get; set; } = false;
        public bool IsActive { get; set; } = false;
    }

    public class EditStakeholderUserModel
    {
        public string UserId { get; set; } = "";
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string StakeholderType { get; set; } = "";
        public string Organization { get; set; } = "";
        public string UserRoleCode { get; set; } = "";
        public bool IsActive { get; set; } = true;
        public bool TwoFactorEnabled { get; set; } = false;
        public bool IsPOPEmployee { get; set; } = false;
    }

    #endregion
}
