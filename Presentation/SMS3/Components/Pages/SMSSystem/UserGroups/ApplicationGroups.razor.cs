using SMS_Application.Interfaces;
using SMS_Application.Commands;
using SMS_Application.Queries;

using SMS_Domain.Events;
using SMS_Domain.Enums;
using SMS_Domain.ValueObjects;

using SMS_Shared.Common;
using SMS_Shared.Configuration;

using SMS3.Components.Shared.UIHelpers;
using SMS3.Configuration.Extensions;
using System.ComponentModel.DataAnnotations;
using System.Net.Mail;

namespace SMS3.Components.Pages.SMSSystem.UserGroups;

public partial class ApplicationGroups : ComponentBase
{
    #region Dependency Injection

    [Inject] private IBaseMediator _mediator { get; set; } = default!;
    [Inject] private ILogger<ApplicationGroups> _logger { get; set; } = default!;
    [Inject] private NavigationManager _navigation { get; set; } = default!;
    [Inject] private IBaseEventBus _eventBus { get; set; } = default!;
    [Inject] private DialogService _dialogService { get; set; } = default!;

    [Inject] private ICurrentUserService _currentUserService { get; set; } = default!;

    #endregion

    private static bool IsEmailFormat(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return false;
        }

        try
        {
            var address = new MailAddress(email.Trim());
            return string.Equals(address.Address, email.Trim(), StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    #region Parameters

    [Parameter] public string? GroupCode { get; set; }
    [Parameter] public string? Action { get; set; } // For handling different actions like "members"

    #endregion

    #region Properties

    private List<SMSApplicationGroup> SMSApplicationGroups { get; set; } = new();
    private List<SMSApplicationUser> SMSApplicationUsers { get; set; } = new();
    private List<SMSCompany> CompanyOptions { get; set; } = new();
    private Dictionary<string, int> GroupMemberCounts { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    private List<SMSApplicationUser> GroupMembers { get; set; } = new();
    private List<SMSApplicationUser> AvailableUsers { get; set; } = new();
    private SMSApplicationGroup? _currentGroup { get; set; }
    private bool _isEditMode { get; set; }
    private bool _isManagingMembers { get; set; }
    private string? _currentGroupCode { get; set; }

    // Grid reference
    private RadzenDataGrid<SMSApplicationGroup>? _groupsGrid;

    // Selection tracking for member management
    private Dictionary<string, bool> SelectedUsers { get; set; } = new();

    private string _successMessage { get; set; } = string.Empty;
    private string _errorMessage { get; set; } = string.Empty;
    private bool _isSaving { get; set; } = false;

    #endregion

    #region Modal Properties

    private bool _showCreateModal { get; set; } = false;
    private bool _showEditModal { get; set; } = false;
    private bool _showMembersModal { get; set; } = false;
    private bool _showDeleteModal { get; set; } = false;
    private string _newGroupName { get; set; } = string.Empty;
    private string _newDescription { get; set; } = string.Empty;
    private string _newContactEmail { get; set; } = string.Empty;
    private HashSet<string> _newAllowedCompanyCodes { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    private string _editGroupName { get; set; } = string.Empty;
    private string _editDescription { get; set; } = string.Empty;
    private string _editContactEmail { get; set; } = string.Empty;
    private HashSet<string> _editAllowedCompanyCodes { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    private bool _editIsActive { get; set; } = true;
    private string _deleteGroupCode { get; set; } = string.Empty;
    private string _deleteGroupName { get; set; } = string.Empty;

    #endregion

    #region Dropdown Options

    private readonly List<StatusOption> _activeInactiveStatusOptions = StatusOptions.ActiveInactiveOptions;

    #endregion

    #region Lifecycle Methods

    protected override async Task OnInitializedAsync()
    {
        await LoadDataAsync();
    }

    protected override async Task OnParametersSetAsync()
    {
        // No longer need route parameter handling since we use modals
        await Task.CompletedTask;
    }

    #endregion

    #region Data Loading

    private async Task LoadDataAsync()
    {
        try
        {
            // Load Application Groups
            var groupsQuery = new GetAllSMSApplicationGroupsQuery();
            var groupsResult = await _mediator.SendAsync(groupsQuery, CancellationToken.None);
            SMSApplicationGroups = groupsResult.IsSuccess ?
                groupsResult.Value?.ToList() ?? new List<SMSApplicationGroup>() :
                new List<SMSApplicationGroup>();

            // Load Application Users for potential group assignments
            var usersQuery = new GetAllSMSApplicationUsersQuery();
            var usersResult = await _mediator.SendAsync(usersQuery, CancellationToken.None);
            SMSApplicationUsers = usersResult.IsSuccess ?
                usersResult.Value?.ToList() ?? new List<SMSApplicationUser>() :
                new List<SMSApplicationUser>();

            CompanyOptions = SMSCompany.GetAllValuesList()
                .Where(c => c is not null && !string.IsNullOrWhiteSpace(c.Value))
                .OrderBy(c => c.Company)
                .ToList();

            await LoadGroupMemberCountsAsync();

            _logger.LogInformation("Loaded {GroupCount} application groups and {UserCount} application users",
                SMSApplicationGroups.Count, SMSApplicationUsers.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading data");
            await ShowErrorAsyncNotification("Error loading data. Please try again.");
        }
    }

    #endregion

    private async Task LoadGroupMemberCountsAsync()
    {
        GroupMemberCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        foreach (var group in SMSApplicationGroups)
        {
            if (string.IsNullOrWhiteSpace(group.Code))
            {
                continue;
            }

            var groupMembersQuery = new GetUsersByApplicationGroupCodeQuery(group.Code);
            var membersResult = await _mediator.SendAsync(groupMembersQuery, CancellationToken.None);
            GroupMemberCounts[group.Code] = membersResult.IsSuccess
                ? membersResult.Value?.Count() ?? 0
                : 0;
        }
    }

    private int GetAssignedUsersCount(string? groupCode)
    {
        if (string.IsNullOrWhiteSpace(groupCode))
        {
            return 0;
        }

        return GroupMemberCounts.TryGetValue(groupCode, out var count) ? count : 0;
    }

    private static int GetAllowedCompaniesCount(SMSApplicationGroup? group)
    {
        return group?.AllowedCompanyCodes?.Count(c => !string.IsNullOrWhiteSpace(c)) ?? 0;
    }

    private async Task ShowAllowedCompanies(SMSApplicationGroup group)
    {
        var allowedCodes = group.AllowedCompanyCodes?
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .Select(c => c.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList() ?? new List<string>();

        if (!allowedCodes.Any())
        {
            await _dialogService.Alert(
                "No specific allowed companies configured. All companies are currently allowed.",
                $"Allowed Companies - {group.Name}",
                new AlertOptions { OkButtonText = "Close" });
            return;
        }

        var allowedCompanies = CompanyOptions
            .Where(c => allowedCodes.Contains(c.Value, StringComparer.OrdinalIgnoreCase))
            .Select(c => new AllowedCompanyDisplayItem { Code = c.Value, Name = c.Company })
            .OrderBy(c => c.Name)
            .ToList();

        await _dialogService.OpenAsync<AllowedCompaniesDialog>($"Allowed Companies - {group.Name}",
            new Dictionary<string, object?>
            {
                { "AllowedCompanies", allowedCompanies }
            },
            new DialogOptions { Width = "600px", Height = "480px", Resizable = true, Draggable = true });
    }

    private bool IsGroupDeleteDisabled(string? groupCode)
    {
        return GetAssignedUsersCount(groupCode) > 0;
    }

    private string GetDeleteTooltip(string? groupCode)
    {
        var assignedCount = GetAssignedUsersCount(groupCode);
        return assignedCount > 0
            ? $"Cannot delete. {assignedCount} user(s) assigned."
            : "Delete Group";
    }

    #region Edit Operations

    private async Task EditGroup(string groupCode)
    {
        if (string.IsNullOrWhiteSpace(groupCode))
        {
            await ShowErrorAsyncNotification("Group code is required.");
            return;
        }

        try
        {
            var getGroupQuery = new GetSMSApplicationGroupByCodeQuery(groupCode);
            var groupResult = await _mediator.SendAsync(getGroupQuery, CancellationToken.None);

            if (groupResult.IsFailure)
            {
                await ShowErrorAsyncNotification("Group not found.");
                return;
            }

            _currentGroup = groupResult.Value;

            // Set edit form values
            _editGroupName = _currentGroup.Name ?? string.Empty;
            _editDescription = _currentGroup.Description ?? string.Empty;
            _editContactEmail = _currentGroup.ContactEmail ?? string.Empty;
            _editAllowedCompanyCodes = new HashSet<string>(
                _currentGroup.AllowedCompanyCodes ?? new List<string>(),
                StringComparer.OrdinalIgnoreCase);
            _editIsActive = _currentGroup.IsActive;

            // Open edit modal
            _showEditModal = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading group for edit: {GroupCode}", groupCode);
            await ShowErrorAsyncNotification("Error loading group. Please try again.");
        }
    }

    private void CancelEdit()
    {
        _isEditMode = false;
        _currentGroup = null;
        _editGroupName = string.Empty;
        _editDescription = string.Empty;
        _editContactEmail = string.Empty;
        _editAllowedCompanyCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        _editIsActive = true;
        _navigation.NavigateToSecure("/System/UserGroups/ApplicationGroups");
    }

    private void CloseEditModal()
    {
        _showEditModal = false;
        _currentGroup = null;
        _editGroupName = string.Empty;
        _editDescription = string.Empty;
        _editContactEmail = string.Empty;
        _editAllowedCompanyCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        _editIsActive = true;
    }

    #endregion

    #region CRUD Operations

    private async Task CreateGroup()
    {
        if (string.IsNullOrWhiteSpace(_newGroupName))
        {
            await ShowErrorAsyncNotification("Group name is required.");
            return;
        }

        if (!IsValidOptionalEmail(_newContactEmail))
        {
            await ShowErrorAsyncNotification("Contact Email must be a valid email address.");
            return;
        }

        try
        {
            _isSaving = true;
            StateHasChanged();

            // Create group entity
            var groupCode = $"AG-0000";
            var groupId = new SMSApplicationGroupID(groupCode);
            var group = new SMSApplicationGroup(groupId)
            {
                Code = groupCode,
                Name = _newGroupName,
                Description = _newDescription,
                ContactEmail = string.IsNullOrWhiteSpace(_newContactEmail) ? null : _newContactEmail.Trim(),
                IsActive = true,
                AllowedCompanyCodes = _newAllowedCompanyCodes.ToList()
            };

            var command = new CreateSMSApplicationGroupCommand(group);
            var result = await _mediator.SendAsync(command, CancellationToken.None);

            if (result.IsSuccess)
            {
                await ShowSuccessAsyncNotification($"Application group '{_newGroupName}' created successfully.");
                CloseCreateModal();
                await LoadDataAsync();
                await RefreshMembersAfterCompanyAssignmentSaveAsync(result.Value?.Code);
                if (_groupsGrid != null)
                    await _groupsGrid.Reload();
            }
            else
            {
                await ShowErrorAsyncNotification(result.Error?.Message ?? "Failed to create application group.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating application group");
            await ShowErrorAsyncNotification("Error creating application group. Please try again.");
        }
        finally
        {
            _isSaving = false;
            StateHasChanged();
        }
    }

    private async Task UpdateGroup()
    {
        if (_currentGroup is null || string.IsNullOrWhiteSpace(_editGroupName))
        {
            await ShowErrorAsyncNotification("Group name is required.");
            return;
        }

        if (!IsValidOptionalEmail(_editContactEmail))
        {
            await ShowErrorAsyncNotification("Contact Email must be a valid email address.");
            return;
        }

        try
        {
            _isSaving = true;
            StateHasChanged();

            // Update group properties
            _currentGroup.Name = _editGroupName;
            _currentGroup.Description = _editDescription;
            _currentGroup.ContactEmail = string.IsNullOrWhiteSpace(_editContactEmail) ? null : _editContactEmail.Trim();
            _currentGroup.AllowedCompanyCodes = _editAllowedCompanyCodes.ToList();
            _currentGroup.IsActive = _editIsActive;
            
            var updateCommand = new UpdateSMSApplicationGroupCommand(_currentGroup);
            var result = await _mediator.SendAsync(updateCommand, CancellationToken.None);

            if (result.IsSuccess)
            {
                await ShowSuccessAsyncNotification($"Application group '{_editGroupName}' updated successfully.");
                var updatedGroupCode = _currentGroup.Code;
                CloseEditModal();
                await LoadDataAsync();
                await RefreshMembersAfterCompanyAssignmentSaveAsync(updatedGroupCode);
                if (_groupsGrid != null)
                    await _groupsGrid.Reload();
            }
            else
            {
                await ShowErrorAsyncNotification(result.Error?.Message ?? "Failed to update application group.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating application group: {GroupCode}", _currentGroup.Code);
            await ShowErrorAsyncNotification("Error updating application group. Please try again.");
        }
        finally
        {
            _isSaving = false;
            StateHasChanged();
        }
    }

    private async Task DeleteGroup()
    {
        if (string.IsNullOrWhiteSpace(_deleteGroupCode))
        {
            await ShowErrorAsyncNotification("Group code is required for deletion.");
            return;
        }

        if (IsGroupDeleteDisabled(_deleteGroupCode))
        {
            await ShowErrorAsyncNotification($"Group '{_deleteGroupName}' cannot be deleted while users are assigned.");
            return;
        }

        try
        {
            _isSaving = true;
            StateHasChanged();

            var command = new DeleteSMSApplicationGroupCommand(_deleteGroupCode);
            var result = await _mediator.SendAsync(command, CancellationToken.None);

            if (result.IsSuccess)
            {
                await ShowSuccessAsyncNotification("Application group deleted successfully.");
                CloseDeleteModal();
                await LoadDataAsync();
                if (_groupsGrid != null)
                    await _groupsGrid.Reload();

                // If we're editing the deleted group, cancel edit mode
                if (_currentGroup?.Code == _deleteGroupCode)
                {
                    CancelEdit();
                }
            }
            else
            {
                await ShowErrorAsyncNotification(result.Error?.Message ?? "Failed to delete application group.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting application group: {GroupCode}", _deleteGroupCode);
            await ShowErrorAsyncNotification("Error deleting application group. Please try again.");
        }
        finally
        {
            _isSaving = false;
            StateHasChanged();
        }
    }

    #endregion

    #region Modal Operations

    private void OpenCreateModal()
    {
        _newGroupName = string.Empty;
        _newDescription = string.Empty;
        _newContactEmail = string.Empty;
        _newAllowedCompanyCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        _showCreateModal = true;
    }

    private void CloseCreateModal()
    {
        _showCreateModal = false;
        _newGroupName = string.Empty;
        _newDescription = string.Empty;
        _newContactEmail = string.Empty;
        _newAllowedCompanyCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    }

    private async Task ConfirmDelete(string groupCode, string groupName)
    {
        if (IsGroupDeleteDisabled(groupCode))
        {
            await ShowErrorAsyncNotification($"Group '{groupName}' cannot be deleted while users are assigned.");
            return;
        }

        _deleteGroupCode = groupCode;
        _deleteGroupName = groupName;
        _showDeleteModal = true;
    }

    private void CloseDeleteModal()
    {
        _showDeleteModal = false;
        _deleteGroupCode = string.Empty;
        _deleteGroupName = string.Empty;
    }

    private bool IsCreateCompanySelected(string companyCode) =>
        _newAllowedCompanyCodes.Contains(companyCode);

    private bool IsEditCompanySelected(string companyCode) =>
        _editAllowedCompanyCodes.Contains(companyCode);

    private void SetCreateCompanySelection(string companyCode, bool isSelected)
    {
        if (isSelected)
        {
            _newAllowedCompanyCodes.Add(companyCode);
            return;
        }

        _newAllowedCompanyCodes.Remove(companyCode);
    }

    private void SetEditCompanySelection(string companyCode, bool isSelected)
    {
        if (isSelected)
        {
            _editAllowedCompanyCodes.Add(companyCode);
            return;
        }

        _editAllowedCompanyCodes.Remove(companyCode);
    }

    private async Task RefreshMembersAfterCompanyAssignmentSaveAsync(string? groupCode)
    {
        if (string.IsNullOrWhiteSpace(groupCode)
            || !_showMembersModal
            || string.IsNullOrWhiteSpace(_currentGroupCode)
            || !string.Equals(_currentGroupCode, groupCode, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        _currentGroup = SMSApplicationGroups.FirstOrDefault(g =>
            string.Equals(g.Code, groupCode, StringComparison.OrdinalIgnoreCase));

        if (_currentGroup is not null)
        {
            await LoadGroupMembersAsync(groupCode);
            StateHasChanged();
        }
    }

    #endregion

    #region Notification Methods (EventBus-Driven)

    private static bool IsValidOptionalEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return true;
        }

        return new EmailAddressAttribute().IsValid(email.Trim());
    }

    private async Task ShowErrorAsyncNotification(string message)
    {
        await _eventBus.PublishUIEventAsync(UINotificationEvent.Error("Error", message));
    }

    private async Task ShowSuccessAsyncNotification(string message)
    {
        await _eventBus.PublishUIEventAsync(UINotificationEvent.Success("Success", message));
    }

    #endregion

    #region Member Management Operations

    private async Task ManageMembers(string groupCode)
    {
        if (string.IsNullOrWhiteSpace(groupCode))
        {
            await ShowErrorAsyncNotification("Group code is required to manage members.");
            return;
        }

        try
        {
            _currentGroupCode = groupCode;
            _isManagingMembers = true;

            var groupQuery = new GetSMSApplicationGroupByCodeQuery(groupCode);
            var groupResult = await _mediator.SendAsync(groupQuery, CancellationToken.None);
            _currentGroup = groupResult.IsSuccess
                ? groupResult.Value
                : SMSApplicationGroups.FirstOrDefault(g => g.Code == groupCode);

            await LoadGroupMembersAsync(groupCode);

            // Show modal instead of navigating
            _showMembersModal = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error entering manage members mode for group: {GroupCode}", groupCode);
            await ShowErrorAsyncNotification("Error entering manage members mode. Please try again.");
        }
    }

    private async Task LoadGroupMembersAsync(string groupCode)
    {
        try
        {
            // Get users in this group using the enhanced repository method
            var groupMembersQuery = new GetUsersByApplicationGroupCodeQuery(groupCode);
            var membersResult = await _mediator.SendAsync(groupMembersQuery, CancellationToken.None);
            GroupMembers = membersResult.IsSuccess ?
                membersResult.Value?.ToList() ?? new List<SMSApplicationUser>() :
                new List<SMSApplicationUser>();

            // Load available users (users not in this group)
            if (!SMSApplicationUsers.Any())
            {
                await LoadDataAsync();
            }

            var memberCodes = GroupMembers.Select(m => m.Code).ToHashSet();
            var allowedCompanies = (_currentGroup?.AllowedCompanyCodes ?? new List<string>())
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Select(c => c.Trim())
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            AvailableUsers = SMSApplicationUsers
                .Where(u => !memberCodes.Contains(u.Code))
                .Where(u => !string.IsNullOrWhiteSpace(u.Company)
                    && allowedCompanies.Contains(u.Company.Trim()))
                .ToList();

            // Initialize selection tracking
            SelectedUsers.Clear();
            foreach (var user in AvailableUsers)
            {
                SelectedUsers[user.Code] = false;
            }

            _logger.LogInformation("Loaded {MemberCount} group members and {AvailableCount} available users for group {GroupCode}",
                GroupMembers.Count, AvailableUsers.Count, groupCode);

            GroupMemberCounts[groupCode] = GroupMembers.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading group members for group: {GroupCode}", groupCode);

            // For now, if the query fails, just load empty collections
            GroupMembers = new List<SMSApplicationUser>();
            AvailableUsers = SMSApplicationUsers?.ToList() ?? new List<SMSApplicationUser>();
            GroupMemberCounts[groupCode] = 0;
        }
    }

    private void ExitMemberManagement()
    {
        _isManagingMembers = false;
        _currentGroupCode = null;
        _currentGroup = null;
        GroupMembers.Clear();
        AvailableUsers.Clear();
        SelectedUsers.Clear();
        _navigation.NavigateToSecure("/System/UserGroups/ApplicationGroups");
    }

    private void CloseMembersModal()
    {
        _showMembersModal = false;
        _isManagingMembers = false;
        _currentGroupCode = null;
        _currentGroup = null;
        GroupMembers.Clear();
        AvailableUsers.Clear();
        SelectedUsers.Clear();
    }

    private async Task RemoveUser(string userCode)
    {
        if (string.IsNullOrWhiteSpace(userCode) || string.IsNullOrWhiteSpace(_currentGroupCode))
        {
            await ShowErrorAsyncNotification("User code and group code are required.");
            return;
        }

        try
        {
            var command = new RemoveUserFromApplicationGroupCommand(userCode, _currentGroupCode);
            var result = await _mediator.SendAsync(command, CancellationToken.None);

            if (result.IsSuccess)
            {
                await ShowSuccessAsyncNotification("User removed from group successfully.");
                await LoadGroupMembersAsync(_currentGroupCode);
                StateHasChanged(); // Refresh the modal
            }
            else
            {
                await ShowErrorAsyncNotification(result.Error?.Message ?? "Failed to remove user from group.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing user {UserCode} from group {GroupCode}", userCode, _currentGroupCode);
            await ShowErrorAsyncNotification("Error removing user from group. Please try again.");
        }
    }

    private async Task AssignMultipleUsers()
    {
        if (string.IsNullOrWhiteSpace(_currentGroupCode) || !SelectedUsers.Any(s => s.Value))
        {
            await ShowErrorAsyncNotification("Group code and at least one user must be selected.");
            return;
        }

        try
        {
            var selectedUserCodes = SelectedUsers.Where(s => s.Value).Select(s => s.Key).ToArray();
            int successCount = 0;
            int failureCount = 0;

            foreach (var userCode in selectedUserCodes)
            {
                try
                {
                    var command = new AssignUserToApplicationGroupCommand(userCode, _currentGroupCode);
                    var result = await _mediator.SendAsync(command, CancellationToken.None);

                    if (result.IsSuccess)
                        successCount++;
                    else
                        failureCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error assigning user {UserCode} to group {GroupCode}", userCode, _currentGroupCode);
                    failureCount++;
                }
            }

            if (successCount > 0)
            {
                var message = $"Successfully assigned {successCount} user(s) to group.";
                if (failureCount > 0)
                    message += $" {failureCount} assignment(s) failed.";
                await ShowSuccessAsyncNotification(message);

                await LoadGroupMembersAsync(_currentGroupCode);
                StateHasChanged(); // Refresh the modal
            }
            else
            {
                await ShowErrorAsyncNotification("Failed to assign users to group.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning multiple users to group {GroupCode}", _currentGroupCode);
            await ShowErrorAsyncNotification("Error assigning users to group. Please try again.");
        }
    }

    private async Task AssignSingleUser(string userCode)
    {
        if (string.IsNullOrWhiteSpace(userCode) || string.IsNullOrWhiteSpace(_currentGroupCode))
        {
            await ShowErrorAsyncNotification("User code and group code are required.");
            return;
        }

        try
        {
            var command = new AssignUserToApplicationGroupCommand(userCode, _currentGroupCode);
            var result = await _mediator.SendAsync(command, CancellationToken.None);

            if (result.IsSuccess)
            {
                await ShowSuccessAsyncNotification("User assigned to group successfully.");
                await LoadGroupMembersAsync(_currentGroupCode);
                StateHasChanged(); // Refresh the modal
            }
            else
            {
                await ShowErrorAsyncNotification(result.Error?.Message ?? "Failed to assign user to group.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning user {UserCode} to group {GroupCode}", userCode, _currentGroupCode);
            await ShowErrorAsyncNotification("Error assigning user to group. Please try again.");
        }
    }

    #endregion

    
}


