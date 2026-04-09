using Microsoft.Extensions.Options;

using SMS_Shared.Configuration;

using SMS3.Components.Shared.UIHelpers;

namespace SMS3.Components.Pages.System.UserManagement;

public partial class OrganizationalUsers : ComponentBase
{
    #region Dependency Injection

    [Inject] private IMediator _mediator { get; set; } = default!;
    [Inject] private ILogger<OrganizationalUsers> _logger { get; set; } = default!;
    [Inject] private NavigationManager _navigation { get; set; } = default!;
    [Inject] private INotificationHelper  _notificationHelper { get; set; } = default!;
    [Inject] private DialogService _dialogService { get; set; } = default!;

    [Inject] private ICurrentUserService CurrentUserService { get; set; } = default!;
    #endregion

    #region Properties

    private List<SMSOrganizationalUser> OrganizationalUsersList { get; set; } = new();
    private SMSOrganizationalUser? CurrentUser { get; set; }
    private bool IsSaving { get; set; } = false;

    // Grid reference
    private RadzenDataGrid<SMSOrganizationalUser>? usersGrid;

    private string SuccessMessage { get; set; } = string.Empty;
    private string ErrorMessage { get; set; } = string.Empty;

    #endregion

    #region Modal Properties

    private bool ShowCreateModal { get; set; } = false;
    private bool ShowEditModal { get; set; } = false;
    private bool ShowPasswordModal { get; set; } = false;
    private bool ShowDeleteModal { get; set; } = false;
    private bool ShowGroupsModal { get; set; } = false;

    // Create form fields
    private string NewFirstName { get; set; } = string.Empty;
    private string NewLastName { get; set; } = string.Empty;
    private string NewUserName { get; set; } = string.Empty;
    private string NewPassword { get; set; } = string.Empty;
    private string NewDepartmentId { get; set; } = string.Empty;
    private string NewPosition { get; set; } = string.Empty;
    private string NewOrganizationLevelId { get; set; } =  string.Empty;
    private bool NewTwoFactorEnabled { get; set; } = false;
    private bool NewIsActive { get; set; } = true; // ADDED: Missing property
    private SMSUserRole? NewSMSUserRole { get; set; }

    // Update form fields to use role ID instead of role name
    
    private string NewSMSUserRoleId { get; set; } = string.Empty;
    private string EditSMSUserRoleId { get; set; } = string.Empty;


    // Edit form fields
    
    private string EditFirstName { get; set; } = string.Empty;
    private string EditLastName { get; set; } = string.Empty;
    private string EditDepartmentId { get; set; } = string.Empty;
    private string EditPosition { get; set; } = string.Empty;
    private string EditOrganizationLevelId { get; set; }  = string.Empty;
    private bool EditIsActive { get; set; } = true;
    private bool EditTwoFactorEnabled { get; set; } = false;

    // Password change fields
    private string PasswordUserId { get; set; } = string.Empty;
    private string PasswordUserDisplayName { get; set; } = string.Empty;
    private string ConfirmPassword { get; set; } = string.Empty;
    private string PasswordValidationMessage { get; set; } = string.Empty;

    // Password Modal Properties for Shared Component
    private string PasswordUserCode { get; set; } = string.Empty;

    // Delete confirmation fields
    private string DeleteUserId { get; set; } = string.Empty;
    private string DeleteUserDisplayName { get; set; } = string.Empty;

    // Group Management Properties
    private string GroupManagementUserCode { get; set; } = string.Empty;
    private string GroupManagementUserDisplayName { get; set; } = string.Empty;
    private List<SMSOrganizationalGroup> AllOrganizationalGroups { get; set; } = new();
    private List<SMSOrganizationalGroup> UserCurrentGroups { get; set; } = new();
    private List<SMSOrganizationalGroup> AvailableGroups { get; set; } = new();
    private Dictionary<string, bool> SelectedGroups { get; set; } = new();

    #endregion

    #region Dropdown Options

    private readonly List<StatusOption> ActiveInactiveStatusOptions = StatusOptions.ActiveInactiveOptions;

    private List<DropdownOption> DepartmentOptions
    {
        get
        {
            return SMSDepartment.GetAllDepartments()
                .OrderBy(dept => dept.Name)
                .Select(dept => new DropdownOption
                {
                    Text = dept.Name,
                    Value = dept.Value
                })
                .ToList();
        }
    }

    // Updated to use centralized helper for SMS Organization Level options
    private List<DropdownOption> OrganizationLevelOptions => DropdownHelper.GetOrganizationLevelOptions();

    // Update validation to check if role exists
    private bool IsValidSMSRole(string roleId)
    {
        if (string.IsNullOrWhiteSpace(roleId)) return true; // Optional field

        return SMSRoleOptions.Any(role => role.Value.Equals(roleId, StringComparison.OrdinalIgnoreCase));
    }
    

    // Helper method to get enum by value
    private SMSOrganizationalLevel GetOrganizationLevelByValue(string value)
    {
        if (string.IsNullOrEmpty(value))
            return SMSOrganizationalLevel.UnassignedLevel;

        return SMSOrganizationalLevel.GetAllValues()
            .FirstOrDefault(l => l.Value.Equals(value, StringComparison.OrdinalIgnoreCase))
            ?? SMSOrganizationalLevel.UnassignedLevel;
    }

    // Update form validation
    private bool IsValidOrganizationLevel(SMSOrganizationalLevel organizationLevel)
    {
        return organizationLevel != null && organizationLevel != SMSOrganizationalLevel.UnassignedLevel;
    }
    #endregion

    #region Form Validation Properties

    private bool IsCreateFormValid =>
        !string.IsNullOrWhiteSpace(NewFirstName) &&
        !string.IsNullOrWhiteSpace(NewLastName) &&
        !string.IsNullOrWhiteSpace(NewUserName) &&
        !string.IsNullOrWhiteSpace(NewPassword) &&
        IsValidDepartment(NewDepartmentId) &&
        IsValidOrganizationLevel(NewOrganizationLevelId);

    private bool IsEditFormValid =>
        !string.IsNullOrWhiteSpace(EditFirstName) &&
        !string.IsNullOrWhiteSpace(EditLastName) &&
        IsValidDepartment(EditDepartmentId) &&
        IsValidOrganizationLevel(EditOrganizationLevelId);

    private bool IsPasswordFormValid =>
        !string.IsNullOrWhiteSpace(NewPassword) &&
        !string.IsNullOrWhiteSpace(ConfirmPassword) &&
        NewPassword == ConfirmPassword &&
        NewPassword.Length >= 8;

    // Helper method to validate organization level
    private bool IsValidOrganizationLevel(string organizationLevel)
    {
        if (string.IsNullOrWhiteSpace(organizationLevel)) return true; // Optional field

        return SMSOrganizationalLevel.GetAllValues()
            .Any(level => level.Name.Equals(organizationLevel, StringComparison.OrdinalIgnoreCase));
    }
    private bool IsValidDepartment(string department)
    {
        if (string.IsNullOrWhiteSpace(department)) return true; // Optional field

        return SMSDepartment.GetAllValues()
            .Any(level => level.Value.Equals(department, StringComparison.OrdinalIgnoreCase));
    }
    #endregion

    #region Lifecycle Methods

    protected override async Task OnInitializedAsync()
    {
        await LoadDataAsync();
    }

    #endregion

    #region Data Loading
    // Load SMS User Roles dynamically
    private List<DropdownOption> SMSRoleOptions { get; set; } = new();

    private async Task LoadSMSRoleOptions()
    {
        try
        {
            var query = new GetAllSMSUserRolesQuery();
            var result = await _mediator.SendAsync(query, CancellationToken.None);

            if (result.IsSuccess && result.Value != null)
            {
                SMSRoleOptions = result.Value
                    .Where(role => !string.IsNullOrEmpty(role.Name)) // Only include roles with names
                    .OrderBy(role => role.Name)
                    .Select(role => new DropdownOption
                    {
                        Text = role.Name ?? role.Code ?? "Unknown Role",
                        Value = role.Code ?? string.Empty
                    })
                    .ToList();
            }
            else
            {
                // Fallback to empty list or show error
                SMSRoleOptions = new List<DropdownOption>();
                _logger.LogWarning("Failed to load SMS User Roles: {Error}", result.Error?.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading SMS User Roles");
            SMSRoleOptions = new List<DropdownOption>();
        }
    }
    private async Task LoadDataAsync()
    {
        try
        {
            // Load Organizational Users
            var organizationalUsersQuery = new GetAllSMSOrganizationalUsersQuery();
            var organizationalUsersResult = await _mediator.SendAsync(organizationalUsersQuery, CancellationToken.None);
            OrganizationalUsersList = organizationalUsersResult.IsSuccess ?
                organizationalUsersResult.Value?.ToList() ?? new List<SMSOrganizationalUser>() :
                new List<SMSOrganizationalUser>();

            // Load Organizational Groups for group management
            var groupsQuery = new GetAllSMSOrganizationalGroupsQuery();
            var groupsResult = await _mediator.SendAsync(groupsQuery, CancellationToken.None);
            AllOrganizationalGroups = groupsResult.IsSuccess ?
                groupsResult.Value?.ToList() ?? new List<SMSOrganizationalGroup>() :
                new List<SMSOrganizationalGroup>();


            // Load SMS User Roles for dropdown
            await LoadSMSRoleOptions();

            _logger.LogInformation("Loaded {UserCount} organizational users and {GroupCount} organizational groups",
                OrganizationalUsersList.Count, AllOrganizationalGroups.Count);

            StateHasChanged();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading organizational users data");
            ShowErrorAsyncNotification("Error loading data. Please refresh the page.");
        }
    }

    #endregion

    #region Create Operations

    private void OpenCreateModal()
    {
        NewFirstName = string.Empty;
        NewLastName = string.Empty;
        NewUserName = string.Empty;
        NewPassword = string.Empty;
        NewDepartmentId = string.Empty;
        NewPosition = string.Empty;
        NewOrganizationLevelId = string.Empty;
        NewTwoFactorEnabled = false;
        NewIsActive = true; // ADDED: Reset new property
        NewSMSUserRole = null;
        NewSMSUserRoleId = string.Empty; // ADDED: Reset role ID
        ShowCreateModal = true;
    }

    private void CloseCreateModal()
    {
        ShowCreateModal = false;
        NewFirstName = string.Empty;
        NewLastName = string.Empty;
        NewUserName = string.Empty;
        NewPassword = string.Empty;
        NewDepartmentId = string.Empty;
        NewPosition = string.Empty;
        NewOrganizationLevelId = string.Empty;
        NewTwoFactorEnabled = false;
        NewIsActive = true; // ADDED: Reset new property
        NewSMSUserRole = null;
        NewSMSUserRoleId = string.Empty; // ADDED: Reset role ID
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

            // Find the actual SMS User Role if one was selected
            SMSUserRole? selectedRole = null;
            if (!string.IsNullOrEmpty(NewSMSUserRoleId))
            {
                // You'll need to fetch the actual role from the database or loaded options
                var roleOption = SMSRoleOptions.FirstOrDefault(r => r.Value == NewSMSUserRoleId);
                if (roleOption != null)
                {
                    // Either fetch from database or create a minimal role object
                    selectedRole = new SMSUserRole(new SMSUserRoleID(roleOption.Value))
                    {
                        Code = roleOption.Value,
                        Name = roleOption.Text
                    };
                }
            }

            // Create user entity
            var userCode = $"OU-0000";
            var userId = new SMSOrganizationalUserID(userCode);
            var user = new SMSOrganizationalUser(userId)
            {
                Code = userCode,
                FirstName = FirstName.Create(NewFirstName).Value,
                LastName = LastName.Create(NewLastName).Value,
                UserName = UserName.Create(NewUserName).Value,
                Password = Password.Create(NewPassword).Value,
                Department =  SMSDepartment.FromValue(NewDepartmentId),
                Position = NewPosition,
                OrganizationLevel = SMSOrganizationalLevel.FromName(NewOrganizationLevelId) ?? SMSOrganizationalLevel.UnassignedLevel,
                SMSUserRole = selectedRole, // UPDATED: Use selectedRole instead of NewSMSUserRole
                TwoFactorEnabled = NewTwoFactorEnabled,
                IsActive = NewIsActive, // UPDATED: Use NewIsActive property
                SMSUserType = SMSUserType.Organizational // ADDED: Set correct user type
            };

            var command = new CreateSMSOrganizationalUserCommand(user);
            var result = await _mediator.SendAsync(command, CancellationToken.None);

            if (result.IsSuccess)
            {
                ShowSuccessAsyncNotification($"Organizational user '{NewFirstName} {NewLastName}' created successfully.");
                CloseCreateModal();
                await LoadDataAsync();
                await usersGrid?.Reload();
            }
            else
            {
                ShowErrorAsyncNotification(result.Error?.Message ?? "Failed to create organizational user.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating organizational user");
            ShowErrorAsyncNotification("Error creating organizational user. Please try again.");
        }
        finally
        {
            IsSaving = false;
            StateHasChanged();
        }
    }

    #endregion

    #region Edit Operations

    private async Task EditUser(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            ShowErrorAsyncNotification("User ID is required.");
            return;
        }

        try
        {
            var getUserQuery = new GetSMSOrganizationalUserByCodeQuery(userId);
            var userResult = await _mediator.SendAsync(getUserQuery, CancellationToken.None);

            if (userResult.IsFailure)
            {
                ShowErrorAsyncNotification("User not found.");
                return;
            }

            CurrentUser = userResult.Value;

            // Set edit form values
            EditFirstName = CurrentUser.FirstName?.Value ?? string.Empty;
            EditLastName = CurrentUser.LastName?.Value ?? string.Empty;
            EditDepartmentId = CurrentUser.Department.Value ?? string.Empty;
            EditPosition = CurrentUser.Position ?? string.Empty;
            EditOrganizationLevelId = CurrentUser.OrganizationLevel.Name ?? SMSOrganizationalLevel.UnassignedLevel;
            EditIsActive = CurrentUser.IsActive;
            EditTwoFactorEnabled = CurrentUser.TwoFactorEnabled;
            EditSMSUserRoleId = CurrentUser.UserRole?.Code ?? string.Empty;
            // Open edit modal
            ShowEditModal = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading user for edit: {UserId}", userId);
            ShowErrorAsyncNotification("Error loading user. Please try again.");
        }
    }

    private void CloseEditModal()
    {
        ShowEditModal = false;
        CurrentUser = null;
        EditFirstName = string.Empty;
        EditLastName = string.Empty;
        EditDepartmentId = string.Empty;
        EditPosition = string.Empty;
        // FIXED: Reset to empty string instead of enum object
        EditOrganizationLevelId = string.Empty;
        EditSMSUserRoleId = string.Empty; // ADDED: Reset SMS User Role
        EditIsActive = true;
        EditTwoFactorEnabled = false;
    }

    private async Task UpdateUser()
    {
        if (CurrentUser == null || !IsEditFormValid)
        {
            ShowErrorAsyncNotification("Please fill in all required fields.");
            return;
        }

        try
        {
            IsSaving = true;
            StateHasChanged();

            // Update user properties
            CurrentUser.FirstName = FirstName.Create(EditFirstName).Value;
            CurrentUser.LastName = LastName.Create(EditLastName).Value;
            CurrentUser.Department = SMSDepartment.FromValue(EditDepartmentId);
            CurrentUser.Position = EditPosition;
            CurrentUser.OrganizationLevel = SMSOrganizationalLevel.FromName(EditOrganizationLevelId) ?? SMSOrganizationalLevel.UnassignedLevel;
            CurrentUser.IsActive = EditIsActive;
            CurrentUser.TwoFactorEnabled = EditTwoFactorEnabled;
            // ADDED: Handle SMS User Role update
            if (!string.IsNullOrEmpty(EditSMSUserRoleId))
            {
                var roleOption = SMSRoleOptions.FirstOrDefault(r => r.Value == EditSMSUserRoleId);
                if (roleOption != null)
                {
                    CurrentUser.UserRole = new SMSUserRole(new SMSUserRoleID(roleOption.Value))
                    {
                        Code = roleOption.Value,
                        Name = roleOption.Text
                    };
                }
            }
            else
            {
                CurrentUser.SMSUserRole = null;
            }

            CurrentUser.IsActive = EditIsActive;
            CurrentUser.SMSUserType = SMSUserType.Organizational; // FIXED: Should be Organizational, not Stakeholder
            var updateCommand = new UpdateSMSOrganizationalUserCommand(CurrentUser);
            var result = await _mediator.SendAsync(updateCommand, CancellationToken.None);

            if (result.IsSuccess)
            {
                ShowSuccessAsyncNotification($"Organizational user '{EditFirstName} {EditLastName}' updated successfully.");
                CloseEditModal();
                await LoadDataAsync();
                await usersGrid?.Reload();
            }
            else
            {
                ShowErrorAsyncNotification(result.Error?.Message ?? "Failed to update organizational user.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating organizational user: {UserId}", CurrentUser.Code);
            ShowErrorAsyncNotification("Error updating organizational user. Please try again.");
        }
        finally
        {
            IsSaving = false;
            StateHasChanged();
        }
    }

    #endregion

    #region Password Management

    private void OpenPasswordModal(string userId, string displayName)
    {
        PasswordUserCode = userId;
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

    // Legacy methods - kept for compatibility
    private void ClosePasswordModal()
    {
        ClosePasswordChangeModal();
    }

    private async Task UpdatePassword()
    {
        // This method is no longer used with the shared component
        // but kept for compatibility if referenced elsewhere
        ShowErrorAsyncNotification("Please use the password change modal to update passwords.");
    }

    #endregion

    #region Delete Operations

    private void ConfirmDelete(String userId, string displayName)
    {
        DeleteUserId = userId;
        DeleteUserDisplayName = displayName;
        ShowDeleteModal = true;
    }

    private void CloseDeleteModal()
    {
        ShowDeleteModal = false;
        DeleteUserId = string.Empty;
        DeleteUserDisplayName = string.Empty;
    }

    private async Task DeleteUser()
    {
        if (string.IsNullOrWhiteSpace(DeleteUserId))
        {
            ShowErrorAsyncNotification("User ID is required for deletion.");
            return;
        }

        try
        {
            IsSaving = true;
            StateHasChanged();

            var organizationalUserId = new SMSOrganizationalUserID(DeleteUserId);
            var command = new DeleteSMSOrganizationalUserCommand(organizationalUserId);
            var result = await _mediator.SendAsync(command, CancellationToken.None);

            if (result.IsSuccess)
            {
                ShowSuccessAsyncNotification("Organizational user deleted successfully.");
                CloseDeleteModal();
                await LoadDataAsync();
                await usersGrid?.Reload();
            }
            else
            {
                ShowErrorAsyncNotification(result.Error?.Message ?? "Failed to delete organizational user.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting organizational user: {UserId}", DeleteUserId);
            ShowErrorAsyncNotification("Error deleting organizational user. Please try again.");
        }
        finally
        {
            IsSaving = false;
            StateHasChanged();
        }
    }

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
            var userGroupsQuery = new GetSMSOrganizationalGroupsByUserCodeQuery(userId);
            var userGroupsResult = await _mediator.SendAsync(userGroupsQuery, CancellationToken.None);
            UserCurrentGroups = userGroupsResult.IsSuccess ?
                userGroupsResult.Value?.ToList() ?? new List<SMSOrganizationalGroup>() :
                new List<SMSOrganizationalGroup>();

            // Calculate available groups (groups the user is not currently in)
            var currentGroupCodes = UserCurrentGroups.Select(g => g.Code).ToHashSet();
            AvailableGroups = AllOrganizationalGroups
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
            UserCurrentGroups = new List<SMSOrganizationalGroup>();
            AvailableGroups = AllOrganizationalGroups.Where(g => g.IsActive).ToList();

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
            var groupId = new SMSOrganizationalGroupID(groupCode);
            var command = new RemoveUserFromOrganizationalGroupCommand(GroupManagementUserCode, groupId);
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
            var groupId = new SMSOrganizationalGroupID(groupCode);
            var command = new AssignUserToOrganizationalGroupCommand(GroupManagementUserCode, groupId);
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
                    var groupId = new SMSOrganizationalGroupID(groupCode);
                    var command = new AssignUserToOrganizationalGroupCommand(GroupManagementUserCode, groupId);
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

    #region Utility Methods

    
        

    // Add method to get organization level display text with hierarchy info
    private string GetOrganizationLevelDisplayText(string organizationLevel)
    {
        var level = SMSOrganizationalLevel.GetAllValues()
            .FirstOrDefault(l => l.Name.Equals(organizationLevel, StringComparison.OrdinalIgnoreCase));

        if (level == null) return organizationLevel;

        return $"{level.Name} - {level.Category} (Authority Level {level.AuthorityLevel})";
    }

    #endregion

    #region Notification Methods

    private void ShowErrorAsyncNotification(string message)
    {
        _notificationHelper.ShowErrorAsync( message);
    }

    private void ShowSuccessAsyncNotification(string message)
    {
        _notificationHelper.ShowSuccessAsync( message);
    }

    private void ShowInfoAsyncNotification(string message)
    {
        _notificationHelper.ShowInfoAsync( message);
    }

    #endregion

    #region Helper Classes

   

  

    #endregion
}