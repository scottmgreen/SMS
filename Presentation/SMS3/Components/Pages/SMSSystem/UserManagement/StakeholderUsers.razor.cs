using SMS_Application.Interfaces;
using SMS_Domain.Events;
using SMS_Infrastructure.Interfaces;
using SMS_Shared.Configuration;

using SMS3.Components.Shared.UIHelpers;
using SMS_Domain.Enums;

namespace SMS3.Components.Pages.SMSSystem.UserManagement;

/// <summary>
/// Code-behind for Stakeholder Users management page  
/// Handles external stakeholder user management
/// ? FIXED: Removed manual audit field assignments - pipeline handles automatically
/// </summary>
public partial class StakeholderUsers : ComponentBase
{
    private sealed class LookupOption
    {
        public string Value { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
    }

    [Inject] private IBaseMediator _mediator { get; set; } = default!;
    [Inject] private ILogger<StakeholderUsers> _logger { get; set; } = default!;
    [Inject] private NavigationManager _navigation { get; set; } = default!;
    [Inject] private DialogService _dialogService { get; set; } = default!;
    [Inject] private IBaseEventBus _eventBus { get; set; } = default!;
    [Inject] private ICurrentUserService _currentUserService { get; set; } = default!;
    [Inject] private ISMSJobTitleRepository _jobTitleRepository { get; set; } = default!;

    // Data Properties
    private List<SMSStakeholderUser> StakeholderUsersList { get; set; } = new();
    private List<SMSUserRole> UserRoles { get; set; } = new();
    private string? _successMessage { get; set; }
    private string? _errorMessage { get; set; }

    // Predefined stakeholder types
    //private static readonly string[] StakeholderTypes = { "SUT-0001", "SUT-0002", "SUT-0003" };
    private string[] StakeholderTypes { get; set; } = Array.Empty<string>();

    // Form Models
    private EditStakeholderUserModel _editUser = new();
    private CreateStakeholderUserModel _newUser = new();

    // Create Modal Properties  
    private bool _showCreateModal { get; set; }
    private bool _isSaving { get; set; }

    // Edit Modal Properties
    private bool _showEditModal { get; set; }
    private SMSStakeholderUser? _currentEditUser { get; set; }

    // Role Assignment Modal Properties
    private bool _showRoleAssignmentModal { get; set; }
    private string _roleAssignmentUserCode { get; set; } = string.Empty;
    private string _roleAssignmentUserDisplayName { get; set; } = string.Empty;
    private string? _currentUserRoleCode { get; set; }
    private string? _selectedRoleCode { get; set; }

    // Legacy properties for compatibility
    private bool _showRoleModal { get; set; }
    private string _roleUserCode { get; set; } = string.Empty;
    private string _roleUserDisplayName { get; set; } = string.Empty;
    private string _currentRoleCode { get; set; } = string.Empty;

    // Dynamically get all unique modules from available roles' permissions
    private IEnumerable<string> _smsModules =>
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


            var titleResult = await _jobTitleRepository.GetAllAsync();
            StakeholderTypes = titleResult.IsSuccess
                ? titleResult.Value
                    .Select(st => st.Value)
                    .ToArray()
                : Array.Empty<string>();


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

            // Create user entity
            var userId = new SMSStakeholderUserID($"SU-0000");
            var normalizedTitle = NormalizeTitleSelection(_newUser.Title);
            var user = new SMSStakeholderUser(userId)
            {
                Code = userId.Value,
                FirstName = FirstName.Create(_newUser.FirstName).Value,
                LastName = LastName.Create(_newUser.LastName).Value,
                UserName = UserName.Create(_newUser.UserName).Value,
                Password = Password.Create(_newUser.Password).Value,
                StakeholderType = normalizedTitle,
                Organization = _newUser.Organization,
                Company = _newUser.Company,
                Title = normalizedTitle,
                JobFunction = _newUser.JobFunction,
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

            if (user.UserRole is null)
            {
                await ShowErrorAsyncNotification("SMS User Role/Permissions is required.");
                return;
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
            _isSaving = false;
            StateHasChanged();
        }
    }

    private void CloseCreateModal()
    {
        _showCreateModal = false;
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
        _newUser.Password == _newUser.ConfirmPassword &&
        !string.IsNullOrWhiteSpace(_newUser.Title) &&
        !string.IsNullOrWhiteSpace(_newUser.Organization);

    // Transform stakeholder types for dropdown
    private IEnumerable<object> StakeholderTypesForDropdown => StakeholderTypes.Select(type => new
    {
        Value = type,
        Text = GetStakeholderTypeDisplay(type)
    });

    private List<LookupOption> CompanyOptions =>
        SMSCompany.GetAllValues()
            .OrderBy(c => c.Company)
            .Select(c => new LookupOption { Value = c.Value, Text = c.Company })
            .ToList();

    private List<LookupOption> OrganizationOptions =>
        SMSOrganization.GetAllValues()
            .OrderBy(o => o.Name)
            .Select(o => new LookupOption { Value = o.Value, Text = o.Name })
            .ToList();

    private static string NormalizeCompanySelection(string? rawValue)
    {
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            return string.Empty;
        }

        var byValue = SMSCompany.FromValue(rawValue);
        if (byValue is not null)
        {
            return byValue.Value;
        }

        var byName = SMSCompany.FromCompany(rawValue);
        return byName?.Value ?? rawValue;
    }

    private static string NormalizeOrganizationSelection(string? rawValue)
    {
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            return string.Empty;
        }

        var byValue = SMSOrganization.FromValue(rawValue);
        if (byValue is not null)
        {
            return byValue.Value;
        }

        var byName = SMSOrganization.FromName(rawValue);
        return byName?.Value ?? rawValue;
    }

    private async Task ShowEditDialog(SMSStakeholderUser user)
    {
        var normalizedTitle = NormalizeTitleSelection(
            string.IsNullOrWhiteSpace(user.StakeholderType) ? user.Title : user.StakeholderType);

        _currentEditUser = user;
        _editUser = new EditStakeholderUserModel
        {
            UserId = user.Code,
            FirstName = user.FirstName?.Value ?? "",
            LastName = user.LastName?.Value ?? "",
            StakeholderType = normalizedTitle,
            Organization = NormalizeOrganizationSelection(user.Organization),
            Company = NormalizeCompanySelection(user.Company),
            Title = normalizedTitle,
            JobFunction = user.JobFunction,
            UserRoleCode = user.UserRole?.Code ?? "",
            IsActive = user.IsActive,
            IsPOPEmployee = user.IsPOPEmployee,
            TwoFactorEnabled = user.TwoFactorEnabled
        };
        _showEditModal = true;
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
            _isSaving = true;
            StateHasChanged();

            if (_currentEditUser is null)
            {
                await ShowErrorAsyncNotification("No user selected for update.");
                return;
            }

            // ? FIXED: Only set business fields - let pipeline handle audit fields
            var normalizedTitle = NormalizeTitleSelection(_editUser.Title);
            _currentEditUser.FirstName = FirstName.Create(_editUser.FirstName).Value;
            _currentEditUser.LastName = LastName.Create(_editUser.LastName).Value;
            _currentEditUser.StakeholderType = normalizedTitle;
            _currentEditUser.Organization = _editUser.Organization;
            _currentEditUser.Company = _editUser.Company;
            _currentEditUser.Title = normalizedTitle;
            _currentEditUser.JobFunction = _editUser.JobFunction;
            _currentEditUser.IsActive = _editUser.IsActive;
            _currentEditUser.IsPOPEmployee = _editUser.IsPOPEmployee;
            _currentEditUser.TwoFactorEnabled = _editUser.TwoFactorEnabled;
            _currentEditUser.SMSUserType = SMSUserType.Stakeholder;
            
                        
            if (string.IsNullOrWhiteSpace(_editUser.UserRoleCode))
            {
                await ShowErrorAsyncNotification("SMS User Role/Permissions is required.");
                return;
            }

            var roleQuery = new GetSMSUserRoleByIdQuery(_editUser.UserRoleCode);
            var roleResult = await _mediator.SendAsync(roleQuery, CancellationToken.None);
            if (roleResult.IsFailure || roleResult.Value is null)
            {
                await ShowErrorAsyncNotification("Selected SMS User Role/Permissions was not found.");
                return;
            }

            _currentEditUser.UserRole = roleResult.Value;

            // Update user - pipeline will automatically set UpdatedBy/UpdatedDate
            var updateCommand = new UpdateSMSStakeholderUserCommand(_currentEditUser);
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
            _isSaving = false;
            StateHasChanged();
        }
    }

    private void CloseEditModal()
    {
        _showEditModal = false;
        _currentEditUser = null;
        _editUser = new EditStakeholderUserModel();
        StateHasChanged();
    }

    private bool _isEditFormValid =>
        !string.IsNullOrWhiteSpace(_editUser.FirstName) &&
        !string.IsNullOrWhiteSpace(_editUser.LastName) &&
        !string.IsNullOrWhiteSpace(_editUser.Title) &&
        !string.IsNullOrWhiteSpace(_editUser.Organization) &&
        !string.IsNullOrWhiteSpace(_editUser.UserRoleCode);

    private static string NormalizeTitleSelection(string? rawValue)
    {
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            return string.Empty;
        }

        var byValue = SMSJobTitle.FromValue(rawValue);
        if (byValue is not null)
        {
            return byValue.Value;
        }

        var byName = SMSJobTitle.FromName(rawValue);
        return byName?.Value ?? rawValue;
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
    private bool _showPasswordModal { get; set; }
    private string _passwordUserCode { get; set; } = string.Empty;
    private string _passwordUserDisplayName { get; set; } = string.Empty;

    // Group Management Properties
    private bool _showGroupsModal { get; set; } = false;
    private string _groupManagementUserCode { get; set; } = string.Empty;
    private string _groupManagementUserDisplayName { get; set; } = string.Empty;
    private List<SMSStakeholderGroup> AllStakeholderGroups { get; set; } = new();
    private List<SMSStakeholderGroup> UserCurrentGroups { get; set; } = new();
    private List<SMSStakeholderGroup> AvailableGroups { get; set; } = new();
    private Dictionary<string, bool> SelectedGroups { get; set; } = new();

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
            var userQuery = new GetSMSStakeholderUserByCodeQuery(_roleAssignmentUserCode);
            var userResult = await _mediator.SendAsync(userQuery, CancellationToken.None);

            if (userResult.IsFailure || userResult.Value is null)
            {
                await ShowErrorAsyncNotification("User not found.");
                return;
            }

            var user = userResult.Value;

            // Get the selected role
            var selectedRole = UserRoles.FirstOrDefault(r => r.Code == _selectedRoleCode);
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
            var userQuery = new GetSMSStakeholderUserByCodeQuery(_roleAssignmentUserCode);
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
        _selectedRoleCode = userRoleCode;
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
            var groupId = new SMSStakeholderGroupID(groupCode);
            var command = new RemoveUserFromStakeholderGroupCommand(_groupManagementUserCode, groupId);
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
            var groupId = new SMSStakeholderGroupID(groupCode);
            var command = new AssignUserToStakeholderGroupCommand(_groupManagementUserCode, groupId);
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
                    var groupId = new SMSStakeholderGroupID(groupCode);
                    var command = new AssignUserToStakeholderGroupCommand(_groupManagementUserCode, groupId);
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

    #region UI Helper Methods

    private async Task ExportUsers()
    {
        await ShowInfoAsyncNotification("Export functionality will be implemented soon.");
    }

    

    private string GetStakeholderTypeDisplay(string stakeholderType)
    {
        var title = SMSJobTitle.FromValue(stakeholderType);
        return title?.Name ?? stakeholderType;
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
        public string ConfirmPassword { get; set; } = "";
        public string StakeholderType { get; set; } = "";
        public string Company { get; set; } = "";
        public string Title { get; set; } = "";
        public string JobFunction { get; set; } = "";
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
        public string Company { get; set; } = "";
        public string Title { get; set; } = "";
        public string JobFunction { get; set; } = "";
        public string Organization { get; set; } = "";
        public string UserRoleCode { get; set; } = "";
        public bool IsActive { get; set; } = true;
        public bool TwoFactorEnabled { get; set; } = false;
        public bool IsPOPEmployee { get; set; } = false;
    }

    #endregion
}

