using Microsoft.Extensions.Options;

using SMS_Application.Messaging.Commands;
using SMS_Application.Messaging.Queries;

using SMS_Application.Interfaces;
using SMS_Domain.Events;
using SMS_Shared.Configuration;

using SMS3.Components.Shared.UIHelpers;

namespace SMS3.Components.Pages.SMSSystem.UserManagement;

public partial class OrganizationalUsers : ComponentBase
{
    #region Dependency Injection

    [Inject] private IBaseMediator _mediator { get; set; } = default!;
    [Inject] private ILogger<OrganizationalUsers> _logger { get; set; } = default!;
    [Inject] private IBaseEventBus _eventBus { get; set; } = default!;
    [Inject] private NavigationManager _navigation { get; set; } = default!;

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

    // Role Assignment Properties
    private bool ShowRoleAssignmentModal { get; set; }
    private string RoleAssignmentUserCode { get; set; } = string.Empty;
    private string RoleAssignmentUserDisplayName { get; set; } = string.Empty;
    private string? CurrentUserRoleCode { get; set; }
    private string? SelectedRoleCode { get; set; }
    private List<SMSUserRole> AvailableRoles { get; set; } = new();

    // Dynamically get all unique modules from available roles' permissions
    private IEnumerable<string> SMSModules =>
        AvailableRoles
            .Where(role => role.Permissions is not null)
            .SelectMany(role => role.Permissions)
            .Where(permission => !string.IsNullOrWhiteSpace(permission.SMSModule))
            .Select(permission => permission.SMSModule!)
            .Distinct()
            .OrderBy(module => module);

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
        return organizationLevel is not null && organizationLevel != SMSOrganizationalLevel.UnassignedLevel;
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

            if (result.IsSuccess && result.Value is not null)
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

            // Load User Roles for role assignment
            var userRolesQuery = new GetAllSMSUserRolesQuery();
            var userRolesResult = await _mediator.SendAsync(userRolesQuery, CancellationToken.None);
            AvailableRoles = userRolesResult.IsSuccess ?
                userRolesResult.Value?.ToList() ?? new List<SMSUserRole>() :
                new List<SMSUserRole>();


            // Load SMS User Roles for dropdown
            await LoadSMSRoleOptions();

            _logger.LogInformation("Loaded {UserCount} organizational users, {GroupCount} organizational groups, and {RoleCount} user roles",
                OrganizationalUsersList.Count, AllOrganizationalGroups.Count, AvailableRoles.Count);

            StateHasChanged();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading organizational users data");
            await ShowErrorAsyncNotification("Error loading data. Please refresh the page.");
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
            await ShowErrorAsyncNotification("Please fill in all required fields.");
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
                if (roleOption is not null)
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
                Department = SMSDepartment.FromValue(NewDepartmentId) ?? SMSDepartment.AirportOperations,
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
                await ShowSuccessAsyncNotification($"Organizational user '{NewFirstName} {NewLastName}' created successfully.");
                CloseCreateModal();
                await LoadDataAsync();
                await (usersGrid?.Reload() ?? Task.CompletedTask);
            }
            else
            {
                await ShowErrorAsyncNotification(result.Error?.Message ?? "Failed to create organizational user.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating organizational user");
            await ShowErrorAsyncNotification("Error creating organizational user. Please try again.");
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
            await ShowErrorAsyncNotification("User ID is required.");
            return;
        }

        try
        {
            var getUserQuery = new GetSMSOrganizationalUserByCodeQuery(userId);
            var userResult = await _mediator.SendAsync(getUserQuery, CancellationToken.None);

            if (userResult.IsFailure)
            {
                await ShowErrorAsyncNotification("User not found.");
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
            await ShowErrorAsyncNotification("Error loading user. Please try again.");
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
        if (CurrentUser is null || !IsEditFormValid)
        {
            await ShowErrorAsyncNotification("Please fill in all required fields.");
            return;
        }

        try
        {
            IsSaving = true;
            StateHasChanged();

            // Update user properties
            CurrentUser.FirstName = FirstName.Create(EditFirstName).Value;
            CurrentUser.LastName = LastName.Create(EditLastName).Value;
            CurrentUser.Department = SMSDepartment.FromValue(EditDepartmentId) ?? SMSDepartment.AirportOperations;
            CurrentUser.Position = EditPosition;
            CurrentUser.OrganizationLevel = SMSOrganizationalLevel.FromName(EditOrganizationLevelId) ?? SMSOrganizationalLevel.UnassignedLevel;
            CurrentUser.IsActive = EditIsActive;
            CurrentUser.TwoFactorEnabled = EditTwoFactorEnabled;
            // ADDED: Handle SMS User Role update
            if (!string.IsNullOrEmpty(EditSMSUserRoleId))
            {
                var roleOption = SMSRoleOptions.FirstOrDefault(r => r.Value == EditSMSUserRoleId);
                if (roleOption is not null)
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
                await ShowSuccessAsyncNotification($"Organizational user '{EditFirstName} {EditLastName}' updated successfully.");
                CloseEditModal();
                await LoadDataAsync();
                await (usersGrid?.Reload() ?? Task.CompletedTask);
            }
            else
            {
                await ShowErrorAsyncNotification(result.Error?.Message ?? "Failed to update organizational user.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating organizational user: {UserId}", CurrentUser.Code);
            await ShowErrorAsyncNotification("Error updating organizational user. Please try again.");
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
        await ShowSuccessAsyncNotification($"Password updated successfully for {PasswordUserDisplayName}.");
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
            await ShowErrorAsyncNotification("User ID is required for deletion.");
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
                await ShowSuccessAsyncNotification("Organizational user deleted successfully.");
                CloseDeleteModal();
                await LoadDataAsync();
                await (usersGrid?.Reload() ?? Task.CompletedTask);
            }
            else
            {
                await ShowErrorAsyncNotification(result.Error?.Message ?? "Failed to delete organizational user.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting organizational user: {UserId}", DeleteUserId);
            await ShowErrorAsyncNotification("Error deleting organizational user. Please try again.");
        }
        finally
        {
            IsSaving = false;
            StateHasChanged();
        }
    }

    #endregion

    #region Role Assignment Methods

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
            var userQuery = new GetSMSOrganizationalUserByCodeQuery(RoleAssignmentUserCode);
            var userResult = await _mediator.SendAsync(userQuery, CancellationToken.None);

            if (userResult.IsFailure || userResult.Value is null)
            {
                await ShowErrorAsyncNotification("User not found.");
                return;
            }

            var user = userResult.Value;

            // Get the selected role
            var selectedRole = AvailableRoles.FirstOrDefault(r => r.Code == SelectedRoleCode);
            if (selectedRole is null)
            {
                await ShowErrorAsyncNotification("Selected role not found.");
                return;
            }

            user.SMSUserType = SMSUserType.Organizational;
            user.UserRole = selectedRole;

            // Update user - pipeline will automatically set UpdatedBy/UpdatedDate
            var updateCommand = new UpdateSMSOrganizationalUserCommand(user);
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
            var userQuery = new GetSMSOrganizationalUserByCodeQuery(RoleAssignmentUserCode);
            var userResult = await _mediator.SendAsync(userQuery, CancellationToken.None);

            if (userResult.IsFailure || userResult.Value is null)
            {
                await ShowErrorAsyncNotification("User not found.");
                return;
            }

            var user = userResult.Value;
            user.UserRole = null!; // Explicitly assign null

            // Update user - pipeline will automatically set UpdatedBy/UpdatedDate
            var updateCommand = new UpdateSMSOrganizationalUserCommand(user);
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
            await ShowErrorAsyncNotification("Group code and user code are required.");
            return;
        }

        try
        {
            var groupId = new SMSOrganizationalGroupID(groupCode);
            var command = new RemoveUserFromOrganizationalGroupCommand(GroupManagementUserCode, groupId);
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
            var groupId = new SMSOrganizationalGroupID(groupCode);
            var command = new AssignUserToOrganizationalGroupCommand(GroupManagementUserCode, groupId);
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

    #region Utility Methods

    
        

    // Add method to get organization level display text with hierarchy info
    private string GetOrganizationLevelDisplayText(string organizationLevel)
    {
        var level = SMSOrganizationalLevel.GetAllValues()
            .FirstOrDefault(l => l.Name.Equals(organizationLevel, StringComparison.OrdinalIgnoreCase));

        if (level is null) return organizationLevel;

        return $"{level.Name} - {level.Category} (Authority Level {level.AuthorityLevel})";
    }

    #endregion

    #region Notification Methods (EventBus-driven)

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

    #region Helper Classes

   

  

    #endregion
}
