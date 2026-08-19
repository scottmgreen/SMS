using Microsoft.Extensions.Options;

using SMS_Application.Commands;
using SMS_Application.Queries;

using SMS_Application.Interfaces;
using SMS_Domain.Events;
using SMS_Shared.Configuration;
using System.Net.Mail;

using SMS3.Components.Shared.UIHelpers;
using SMS_Domain.Enums;
using SMS_Infrastructure.Interfaces;

namespace SMS3.Components.Pages.SMSSystem.UserManagement;

public partial class OrganizationalUsers : ComponentBase
{
    #region Dependency Injection

    [Inject] private IBaseMediator _mediator { get; set; } = default!;
    [Inject] private ILogger<OrganizationalUsers> _logger { get; set; } = default!;
    [Inject] private IBaseEventBus _eventBus { get; set; } = default!;
    [Inject] private NavigationManager _navigation { get; set; } = default!;

    [Inject] private DialogService _dialogService { get; set; } = default!;

    [Inject] private ICurrentUserService _currentUserService { get; set; } = default!;
    [Inject] private ISMSJobTitleRepository _jobTitleRepository { get; set; } = default!;
    #endregion

    #region Properties

    private List<SMSOrganizationalUser> OrganizationalUsersList { get; set; } = new();
    private SMSOrganizationalUser? _currentUser { get; set; }
    private bool _isSaving { get; set; } = false;

    // Grid reference
    private RadzenDataGrid<SMSOrganizationalUser>? _usersGrid;

    private string _successMessage { get; set; } = string.Empty;
    private string _errorMessage { get; set; } = string.Empty;

    #endregion

    #region Modal Properties

    private bool _showCreateModal { get; set; } = false;
    private bool _showEditModal { get; set; } = false;
    private bool _showPasswordModal { get; set; } = false;
    private bool _showDeleteModal { get; set; } = false;
    private bool _showGroupsModal { get; set; } = false;

    // Create form fields
    private string _newFirstName { get; set; } = string.Empty;
    private string _newLastName { get; set; } = string.Empty;
    private string _newUserName { get; set; } = string.Empty;
    private string _newPassword { get; set; } = string.Empty;
    private string _newDepartmentId { get; set; } = string.Empty;
    private string _newCompany { get; set; } = string.Empty;
    private string _newJobFunction { get; set; } = string.Empty;
    private string _newPosition { get; set; } = string.Empty;
    private string _newOrganizationLevelId { get; set; } =  string.Empty;
    private bool _newTwoFactorEnabled { get; set; } = false;
    private bool _newIsActive { get; set; } = true; // ADDED: Missing property
    // Update form fields to use role ID consistently with other user dialogs
    private string _newUserRoleCode { get; set; } = string.Empty;
    private string _editUserRoleCode { get; set; } = string.Empty;


    // Edit form fields
    
    private string _editFirstName { get; set; } = string.Empty;
    private string _editLastName { get; set; } = string.Empty;
    private string _editDepartmentId { get; set; } = string.Empty;
    private string _editCompany { get; set; } = string.Empty;
    private string _editJobFunction { get; set; } = string.Empty;
    private string _editPosition { get; set; } = string.Empty;
    private string _editOrganizationLevelId { get; set; }  = string.Empty;
    private bool _editIsActive { get; set; } = true;
    private bool _editTwoFactorEnabled { get; set; } = false;

    // Password change fields
    private string _passwordUserId { get; set; } = string.Empty;
    private string _passwordUserDisplayName { get; set; } = string.Empty;
    private string _confirmPassword { get; set; } = string.Empty;
    private string _passwordValidationMessage { get; set; } = string.Empty;

    // Password Modal Properties for Shared Component
    private string _passwordUserCode { get; set; } = string.Empty;

    // Delete confirmation fields
    private string _deleteUserId { get; set; } = string.Empty;
    private string _deleteUserDisplayName { get; set; } = string.Empty;

    // Role Assignment Properties
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
    private string _groupManagementUserCode { get; set; } = string.Empty;
    private string _groupManagementUserDisplayName { get; set; } = string.Empty;
    private int _groupManagementUserAuthorityLevel { get; set; }
    private List<SMSOrganizationalGroup> AllOrganizationalGroups { get; set; } = new();
    private List<SMSOrganizationalGroup> UserCurrentGroups { get; set; } = new();
    private List<SMSOrganizationalGroup> AvailableGroups { get; set; } = new();
    private Dictionary<string, bool> SelectedGroups { get; set; } = new();

    #endregion

    #region Dropdown Options

    private readonly List<StatusOption> _activeInactiveStatusOptions = StatusOptions.ActiveInactiveOptions;

    private List<DropdownOption> DepartmentOptions
    {
        get
        {
            return SMSOrganization.GetAllDepartments()
                .OrderBy(dept => dept.Name)
                .Select(dept => new DropdownOption
                {
                    Text = dept.Name,
                    Value = dept.Value
                })
                .ToList();
        }
    }

    private List<DropdownOption> OrganizationOptions => DepartmentOptions;

    private List<DropdownOption> TitleOptions { get; set; } = new();

    private List<DropdownOption> CompanyOptions =>
        SMSCompany.GetAllValues()
            .OrderBy(c => c.Company)
            .Select(c => new DropdownOption
            {
                Text = c.Company,
                Value = c.Value
            })
            .ToList();

    // Updated to use centralized helper for SMS Organization Level options
    private List<DropdownOption> _organizationLevelOptions => DropdownHelper.GetOrganizationLevelOptions();

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

    private bool _isCreateFormValid =>
        !string.IsNullOrWhiteSpace(_newFirstName) &&
        !string.IsNullOrWhiteSpace(_newLastName) &&
        !string.IsNullOrWhiteSpace(_newUserName) &&
        !string.IsNullOrWhiteSpace(_newPassword) &&
        _newPassword == _confirmPassword &&
        IsValidDepartment(_newDepartmentId) &&
        IsValidOrganizationLevel(_newOrganizationLevelId) &&
        !string.IsNullOrWhiteSpace(_newUserRoleCode);

    private bool _isEditFormValid =>
        !string.IsNullOrWhiteSpace(_editFirstName) &&
        !string.IsNullOrWhiteSpace(_editLastName) &&
        IsValidDepartment(_editDepartmentId) &&
        IsValidOrganizationLevel(_editOrganizationLevelId) &&
        !string.IsNullOrWhiteSpace(_editUserRoleCode);

    
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

        return SMSOrganization.GetAllValues()
            .Any(level => level.Value.Equals(department, StringComparison.OrdinalIgnoreCase));
    }

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

            var titleResult = await _jobTitleRepository.GetAllAsync();
            TitleOptions = titleResult.IsSuccess
                ? titleResult.Value?
                    .OrderBy(t => t.Name)
                    .Select(t => new DropdownOption
                    {
                        Text = t.Name,
                        Value = t.Value
                    })
                    .ToList() ?? new List<DropdownOption>()
                : new List<DropdownOption>();

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
        _newFirstName = string.Empty;
        _newLastName = string.Empty;
        _newUserName = string.Empty;
        _newPassword = string.Empty;
        _confirmPassword = string.Empty;
        _newDepartmentId = string.Empty;
        _newCompany = string.Empty;
        _newJobFunction = string.Empty;
        _newPosition = string.Empty;
        _newOrganizationLevelId = string.Empty;
        _newTwoFactorEnabled = false;
        _newIsActive = true; // ADDED: Reset new property
        _newUserRoleCode = string.Empty;
        _showCreateModal = true;
    }

    private void CloseCreateModal()
    {
        _showCreateModal = false;
        _newFirstName = string.Empty;
        _newLastName = string.Empty;
        _newUserName = string.Empty;
        _newPassword = string.Empty;
        _confirmPassword = string.Empty;
        _newDepartmentId = string.Empty;
        _newCompany = string.Empty;
        _newJobFunction = string.Empty;
        _newPosition = string.Empty;
        _newOrganizationLevelId = string.Empty;
        _newTwoFactorEnabled = false;
        _newIsActive = true; // ADDED: Reset new property
        _newUserRoleCode = string.Empty;
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

            // Find the actual SMS User Role if one was selected
            SMSUserRole? selectedRole = null;
            if (!string.IsNullOrEmpty(_newUserRoleCode))
            {
                var roleOption = SMSRoleOptions.FirstOrDefault(r => r.Value == _newUserRoleCode);
                if (roleOption is not null)
                {
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
                FirstName = FirstName.Create(_newFirstName).Value,
                LastName = LastName.Create(_newLastName).Value,
                UserName = UserName.Create(_newUserName).Value,
                Password = Password.Create(_newPassword).Value,
                Department = SMSOrganization.FromValue(_newDepartmentId) ?? SMSOrganization.Create(string.Empty, string.Empty, string.Empty, Array.Empty<string>()),
                Company = _newCompany,
                Organization = _newDepartmentId,
                Title = _newPosition,
                JobFunction = _newJobFunction,
                Position = _newPosition,
                OrganizationLevel = SMSOrganizationalLevel.FromName(_newOrganizationLevelId) ?? SMSOrganizationalLevel.UnassignedLevel,
                UserRole = selectedRole,
                TwoFactorEnabled = _newTwoFactorEnabled,
                IsActive = _newIsActive, // UPDATED: Use _newIsActive property
                SMSUserType = SMSUserType.Organizational // ADDED: Set correct user type
            };
            user.SyncAuthorityFromOrganizationLevel();

            var command = new CreateSMSOrganizationalUserCommand(user);
            var result = await _mediator.SendAsync(command, CancellationToken.None);

            if (result.IsSuccess)
            {
                await ShowSuccessAsyncNotification($"Organizational user '{_newFirstName} {_newLastName}' created successfully.");
                CloseCreateModal();
                await LoadDataAsync();
            await (_usersGrid?.Reload() ?? Task.CompletedTask);
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
            _isSaving = false;
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

            _currentUser = userResult.Value;

            // Set edit form values
            _editFirstName = _currentUser.FirstName?.Value ?? string.Empty;
            _editLastName = _currentUser.LastName?.Value ?? string.Empty;
            _editDepartmentId = _currentUser.Department.Value ?? string.Empty;
            _editCompany = SMSCompany.FromCompany(_currentUser.Company ?? string.Empty)?.Value ?? (_currentUser.Company ?? string.Empty);
            _editJobFunction = _currentUser.JobFunction ?? string.Empty;
            _editPosition = NormalizeTitleSelection(string.IsNullOrWhiteSpace(_currentUser.Title)
                ? (_currentUser.Position ?? string.Empty)
                : _currentUser.Title);
            _editOrganizationLevelId = _currentUser.OrganizationLevel.Name ?? SMSOrganizationalLevel.UnassignedLevel;
            _editIsActive = _currentUser.IsActive;
            _editTwoFactorEnabled = _currentUser.TwoFactorEnabled;
            _editUserRoleCode = _currentUser.UserRole?.Code ?? string.Empty;
            // Open edit modal
            _showEditModal = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading user for edit: {UserId}", userId);
            await ShowErrorAsyncNotification("Error loading user. Please try again.");
        }
    }

    private void CloseEditModal()
    {
        _showEditModal = false;
        _currentUser = null;
        _editFirstName = string.Empty;
        _editLastName = string.Empty;
        _editDepartmentId = string.Empty;
        _editCompany = string.Empty;
        _editJobFunction = string.Empty;
        _editPosition = string.Empty;
        // FIXED: Reset to empty string instead of enum object
        _editOrganizationLevelId = string.Empty;
        _editUserRoleCode = string.Empty;
        _editIsActive = true;
        _editTwoFactorEnabled = false;
    }

    private async Task UpdateUser()
    {
        if (_currentUser is null || !_isEditFormValid)
        {
            await ShowErrorAsyncNotification("Please fill in all required fields.");
            return;
        }

        try
        {
            _isSaving = true;
            StateHasChanged();

            // Update user properties
            _currentUser.FirstName = FirstName.Create(_editFirstName).Value;
            _currentUser.LastName = LastName.Create(_editLastName).Value;
            _currentUser.Department = SMSOrganization.FromValue(_editDepartmentId) ?? SMSOrganization.Create(string.Empty, string.Empty, string.Empty, Array.Empty<string>());
            _currentUser.Company = _editCompany;
            _currentUser.Organization = _editDepartmentId;
            _currentUser.Title = _editPosition;
            _currentUser.JobFunction = _editJobFunction;
            _currentUser.Position = _editPosition;
            _currentUser.OrganizationLevel = SMSOrganizationalLevel.FromName(_editOrganizationLevelId) ?? SMSOrganizationalLevel.UnassignedLevel;
            _currentUser.SyncAuthorityFromOrganizationLevel();
            _currentUser.IsActive = _editIsActive;
            _currentUser.TwoFactorEnabled = _editTwoFactorEnabled;
            // ADDED: Handle SMS User Role update
            if (!string.IsNullOrEmpty(_editUserRoleCode))
            {
                var roleOption = SMSRoleOptions.FirstOrDefault(r => r.Value == _editUserRoleCode);
                if (roleOption is not null)
                {
                    _currentUser.UserRole = new SMSUserRole(new SMSUserRoleID(roleOption.Value))
                    {
                        Code = roleOption.Value,
                        Name = roleOption.Text
                    };
                }
                else
                {
                    await ShowErrorAsyncNotification("Selected SMS User Role/Permissions was not found.");
                    return;
                }
            }
            else
            {
                await ShowErrorAsyncNotification("SMS User Role/Permissions is required.");
                return;
            }

            _currentUser.IsActive = _editIsActive;
            _currentUser.SMSUserType = SMSUserType.Organizational; // FIXED: Should be Organizational, not Stakeholder
            var updateCommand = new UpdateSMSOrganizationalUserCommand(_currentUser);
            var result = await _mediator.SendAsync(updateCommand, CancellationToken.None);

            if (result.IsSuccess)
            {
                await ShowSuccessAsyncNotification($"Organizational user '{_editFirstName} {_editLastName}' updated successfully.");
                CloseEditModal();
                await LoadDataAsync();
            await (_usersGrid?.Reload() ?? Task.CompletedTask);
            }
            else
            {
                await ShowErrorAsyncNotification(result.Error?.Message ?? "Failed to update organizational user.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating organizational user: {UserId}", _currentUser.Code);
            await ShowErrorAsyncNotification("Error updating organizational user. Please try again.");
        }
        finally
        {
            _isSaving = false;
            StateHasChanged();
        }
    }

    #endregion

    #region Password Management

    private void OpenPasswordModal(string userId, string displayName)
    {
        _passwordUserCode = userId;
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

      

    #endregion

    #region Delete Operations

    private void ConfirmDelete(String userId, string displayName)
    {
        _deleteUserId = userId;
        _deleteUserDisplayName = displayName;
        _showDeleteModal = true;
    }

    private void CloseDeleteModal()
    {
        _showDeleteModal = false;
        _deleteUserId = string.Empty;
        _deleteUserDisplayName = string.Empty;
    }

    private async Task DeleteUser()
    {
        if (string.IsNullOrWhiteSpace(_deleteUserId))
        {
            await ShowErrorAsyncNotification("User ID is required for deletion.");
            return;
        }

        try
        {
            _isSaving = true;
            StateHasChanged();

            var organizationalUserId = new SMSOrganizationalUserID(_deleteUserId);
            var command = new DeleteSMSOrganizationalUserCommand(organizationalUserId);
            var result = await _mediator.SendAsync(command, CancellationToken.None);

            if (result.IsSuccess)
            {
                await ShowSuccessAsyncNotification("Organizational user deleted successfully.");
                CloseDeleteModal();
                await LoadDataAsync();
            await (_usersGrid?.Reload() ?? Task.CompletedTask);
            }
            else
            {
                await ShowErrorAsyncNotification(result.Error?.Message ?? "Failed to delete organizational user.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting organizational user: {UserId}", _deleteUserId);
            await ShowErrorAsyncNotification("Error deleting organizational user. Please try again.");
        }
        finally
        {
            _isSaving = false;
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
            var userQuery = new GetSMSOrganizationalUserByCodeQuery(_roleAssignmentUserCode);
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

            user.SMSUserType = SMSUserType.Organizational;
            user.UserRole = selectedRole;

            // Update user - pipeline will automatically set UpdatedBy/UpdatedDate
            var updateCommand = new UpdateSMSOrganizationalUserCommand(user);
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
            var userQuery = new GetSMSOrganizationalUserByCodeQuery(_roleAssignmentUserCode);
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

    #region Group Management

    private async Task ManageGroups(string userId, string displayName)
    {
        try
        {
            _groupManagementUserCode = userId;
            _groupManagementUserDisplayName = displayName;
            _groupManagementUserAuthorityLevel = 0;

            var userResult = await _mediator.SendAsync(new GetSMSOrganizationalUserByCodeQuery(userId), CancellationToken.None);
            if (userResult.IsSuccess && userResult.Value is not null)
            {
                _groupManagementUserAuthorityLevel = userResult.Value.EffectiveAuthorityLevel;
            }

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
            var userGroupsQuery = new GetSMSOrganizationalGroupsByUserCodeQuery(userId);
            var userGroupsResult = await _mediator.SendAsync(userGroupsQuery, CancellationToken.None);
            UserCurrentGroups = userGroupsResult.IsSuccess ?
                userGroupsResult.Value?.ToList() ?? new List<SMSOrganizationalGroup>() :
                new List<SMSOrganizationalGroup>();

            // Calculate available groups (groups the user is not currently in)
            var currentGroupCodes = UserCurrentGroups.Select(g => g.Code).ToHashSet();
            AvailableGroups = AllOrganizationalGroups
                .Where(g => !currentGroupCodes.Contains(g.Code) && g.IsActive)
                .Where(g => g.EffectiveAuthorityLevel <= _groupManagementUserAuthorityLevel)
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
            var groupId = new SMSOrganizationalGroupID(groupCode);
            var command = new RemoveUserFromOrganizationalGroupCommand(_groupManagementUserCode, groupId);
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

        if (!CanAssignGroupToManagedUser(groupCode))
        {
            await ShowErrorAsyncNotification("User cannot be assigned to a group with higher authority level.");
            return;
        }

        try
        {
            var groupId = new SMSOrganizationalGroupID(groupCode);
            var command = new AssignUserToOrganizationalGroupCommand(_groupManagementUserCode, groupId);
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
                    if (!CanAssignGroupToManagedUser(groupCode))
                    {
                        failureCount++;
                        continue;
                    }

                    var groupId = new SMSOrganizationalGroupID(groupCode);
                    var command = new AssignUserToOrganizationalGroupCommand(_groupManagementUserCode, groupId);
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

    #region Utility Methods

    
        

    // Add method to get organization level display text with hierarchy info
    private string GetOrganizationLevelDisplayText(string organizationLevel)
    {
        var level = SMSOrganizationalLevel.GetAllValues()
            .FirstOrDefault(l => l.Name.Equals(organizationLevel, StringComparison.OrdinalIgnoreCase));

        if (level is null) return organizationLevel;

        return $"{level.Name} - {level.Category} (Authority Level {level.AuthorityLevel})";
    }

    private bool CanAssignGroupToManagedUser(string groupCode)
    {
        var group = AllOrganizationalGroups.FirstOrDefault(g => g.Code == groupCode);
        if (group is null)
        {
            return false;
        }

        var groupAuthorityLevel = group.EffectiveAuthorityLevel;
        return groupAuthorityLevel <= _groupManagementUserAuthorityLevel;
    }

    private static bool IsEmailFormat(string? userName)
    {
        if (string.IsNullOrWhiteSpace(userName))
        {
            return false;
        }

        try
        {
            var addr = new MailAddress(userName.Trim());
            return string.Equals(addr.Address, userName.Trim(), StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
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

