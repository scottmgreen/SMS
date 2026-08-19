
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

public partial class StakeholderGroups : ComponentBase
{
    #region Dependency Injection
    [Inject] private IBaseMediator Mediator { get; set; } = default!;
    [Inject] private ILogger<StakeholderGroups> Logger { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Inject] private IBaseEventBus EventBus { get; set; } = default!;
    [Inject] private DialogService DialogService { get; set; } = default!;
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

    #endregion

    #region Properties

    private List<SMSStakeholderGroup> SMSStakeholderGroups { get; set; } = new();
    private List<SMSStakeholderUser> SMSStakeholderUsers { get; set; } = new();
    private List<SMSCompany> CompanyOptions { get; set; } = new();
    private Dictionary<string, int> GroupMemberCounts { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    private List<SMSStakeholderUser> GroupMembers { get; set; } = new();
    private List<SMSStakeholderUser> AvailableUsers { get; set; } = new();
    private SMSStakeholderGroup? _currentGroup { get; set; }
    private bool _isEditMode { get; set; }
    private bool _isManagingMembers { get; set; }
    private string? _currentGroupCode { get; set; }

    // Grid reference
    private RadzenDataGrid<SMSStakeholderGroup>? _groupsGrid;

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

    private readonly List<StatusOption> _activeStatusOptions = StatusOptions.ActiveInactiveOptions;

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
            // Load Stakeholder Groups
            var groupsQuery = new GetAllSMSStakeholderGroupsQuery();
            var groupsResult = await Mediator.SendAsync(groupsQuery, CancellationToken.None);
            SMSStakeholderGroups = groupsResult.IsSuccess ?
                groupsResult.Value?.ToList() ?? new List<SMSStakeholderGroup>() :
                new List<SMSStakeholderGroup>();

            // Load Stakeholder Users for potential group assignments
            var usersQuery = new GetAllSMSStakeholderUsersQuery();
            var usersResult = await Mediator.SendAsync(usersQuery, CancellationToken.None);
            SMSStakeholderUsers = usersResult.IsSuccess ?
                usersResult.Value?.ToList() ?? new List<SMSStakeholderUser>() :
                new List<SMSStakeholderUser>();

            CompanyOptions = SMSCompany.GetAllValuesList()
                .Where(c => c is not null && !string.IsNullOrWhiteSpace(c.Value))
                .OrderBy(c => c.Company)
                .ToList();

            await LoadGroupMemberCountsAsync();

            Logger.LogInformation("Loaded {GroupCount} stakeholder groups and {UserCount} stakeholder users",
                SMSStakeholderGroups.Count, SMSStakeholderUsers.Count);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading data");
            await ShowErrorAsyncNotification("Error loading data. Please try again.");
        }
    }

    #endregion

    private async Task LoadGroupMemberCountsAsync()
    {
        GroupMemberCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        foreach (var group in SMSStakeholderGroups)
        {
            if (string.IsNullOrWhiteSpace(group.Code))
            {
                continue;
            }

            var groupMembersQuery = new GetUsersByStakeholderGroupCodeQuery(group.Code);
            var membersResult = await Mediator.SendAsync(groupMembersQuery, CancellationToken.None);
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

    private static int GetAllowedCompaniesCount(SMSStakeholderGroup? group)
    {
        return group?.AllowedCompanyCodes?.Count(c => !string.IsNullOrWhiteSpace(c)) ?? 0;
    }

    private async Task ShowAllowedCompanies(SMSStakeholderGroup group)
    {
        var allowedCodes = group.AllowedCompanyCodes?
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .Select(c => c.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList() ?? new List<string>();

        if (!allowedCodes.Any())
        {
            await DialogService.Alert(
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

        await DialogService.OpenAsync<AllowedCompaniesDialog>($"Allowed Companies - {group.Name}",
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
            var getGroupQuery = new GetSMSStakeholderGroupByCodeQuery(groupCode);
            var groupResult = await Mediator.SendAsync(getGroupQuery, CancellationToken.None);

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
            Logger.LogError(ex, "Error loading group for edit: {GroupCode}", groupCode);
            await ShowErrorAsyncNotification("Error loading group. Please try again.");
        }
    }

    private async Task CancelEdit()
    {
        _isEditMode = false;
        _currentGroup = null;
        _editGroupName = string.Empty;
        _editDescription = string.Empty;
        _editContactEmail = string.Empty;
        _editAllowedCompanyCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        _editIsActive = true;
        Logger.LogInformation("Group edit cancelled");
        await EventBus.PublishUIEventAsync(UINotificationEvent.Info("Information", "Edit cancelled"));
        Navigation.NavigateToSecure("/System/UserGroups/StakeholderGroups");
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
            var groupCode = $"SG-0000";
            var groupId = new SMSStakeholderGroupID(groupCode);
            var group = new SMSStakeholderGroup(groupId)
            {
                Code = groupCode,
                Name = _newGroupName,
                Description = _newDescription,
                ContactEmail = string.IsNullOrWhiteSpace(_newContactEmail) ? null : _newContactEmail.Trim(),
                IsActive = true,
                AllowedCompanyCodes = _newAllowedCompanyCodes.ToList()
            };

            var command = new CreateSMSStakeholderGroupCommand(group);
            var result = await Mediator.SendAsync(command, CancellationToken.None);

            if (result.IsSuccess)
            {
                await ShowSuccessAsyncNotification($"Stakeholder group '{_newGroupName}' created successfully.");
                CloseCreateModal();
                await LoadDataAsync();
                await RefreshMembersAfterCompanyAssignmentSaveAsync(result.Value?.Code);
                if (_groupsGrid != null)
                    await _groupsGrid.Reload();
            }
            else
            {
                await ShowErrorAsyncNotification(result.Error?.Message ?? "Failed to create stakeholder group.");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error creating stakeholder group");
            await ShowErrorAsyncNotification("Error creating stakeholder group. Please try again.");
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

            var updateCommand = new UpdateSMSStakeholderGroupCommand(_currentGroup);
            var result = await Mediator.SendAsync(updateCommand, CancellationToken.None);

            if (result.IsSuccess)
            {
                await ShowSuccessAsyncNotification($"Stakeholder group '{_editGroupName}' updated successfully.");
                var updatedGroupCode = _currentGroup.Code;
                CloseEditModal();
                await LoadDataAsync();
                await RefreshMembersAfterCompanyAssignmentSaveAsync(updatedGroupCode);
                if (_groupsGrid != null)
                    await _groupsGrid.Reload();
            }
            else
            {
                await ShowErrorAsyncNotification(result.Error?.Message ?? "Failed to update stakeholder group.");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error updating stakeholder group: {GroupCode}", _currentGroup.Code);
            await ShowErrorAsyncNotification("Error updating stakeholder group. Please try again.");
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

            // Get existing group to pass to delete command
            var getGroupQuery = new GetSMSStakeholderGroupByCodeQuery(_deleteGroupCode);
            var groupResult = await Mediator.SendAsync(getGroupQuery, CancellationToken.None);

            if (groupResult.IsFailure)
            {
                await ShowErrorAsyncNotification("Group not found.");
                return;
            }

            var deleteCommand = new DeleteSMSStakeholderGroupCommand(groupResult.Value);
            var result = await Mediator.SendAsync(deleteCommand, CancellationToken.None);

            if (result.IsSuccess)
            {
                await ShowSuccessAsyncNotification("Stakeholder group deleted successfully.");
                CloseDeleteModal();
                await LoadDataAsync();
                if (_groupsGrid != null)
                    await _groupsGrid.Reload();

                // If we're editing the deleted group, cancel edit mode
                if (_currentGroup?.Code == _deleteGroupCode)
                {
                    await CancelEdit();
                }
            }
            else
            {
                await ShowErrorAsyncNotification(result.Error?.Message ?? "Failed to delete stakeholder group.");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error deleting stakeholder group: {GroupCode}", _deleteGroupCode);
            await ShowErrorAsyncNotification("Error deleting stakeholder group. Please try again.");
        }
        finally
        {
            _isSaving = false;
            StateHasChanged();
        }
    }

    #endregion

    #region Modal Operations
    

    #region Filtering Methods

    private bool _filterPopEmployeesOnly { get; set; } = false;

    private List<SMSStakeholderUser> GetFilteredAvailableUsers()
    {
        return _filterPopEmployeesOnly ?
            AvailableUsers.Where(u => u.IsPOPEmployee).ToList() :
            AvailableUsers;
    }



    #endregion
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

        _currentGroup = SMSStakeholderGroups.FirstOrDefault(g =>
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
        await EventBus.PublishUIEventAsync(UINotificationEvent.Error("Error", message));
    }

    private async Task ShowSuccessAsyncNotification(string message)
    {
        await EventBus.PublishUIEventAsync(UINotificationEvent.Success("Success", message));
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

            // Find the current group
            _currentGroup = SMSStakeholderGroups.FirstOrDefault(g => g.Code == groupCode);

            await LoadGroupMembersAsync(groupCode);

            // Show modal instead of navigating
            _showMembersModal = true;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error entering manage members mode for group: {GroupCode}", groupCode);
            await ShowErrorAsyncNotification("Error entering manage members mode. Please try again.");
        }
    }

    private async Task LoadGroupMembersAsync(string groupCode)
    {
        try
        {
            // Get users in this group
            var groupMembersQuery = new GetUsersByStakeholderGroupCodeQuery(groupCode);
            var membersResult = await Mediator.SendAsync(groupMembersQuery, CancellationToken.None);
            GroupMembers = membersResult.IsSuccess ?
                membersResult.Value?.ToList() ?? new List<SMSStakeholderUser>() :
                new List<SMSStakeholderUser>();

            // Load available users (users not in this group)
            if (!SMSStakeholderUsers.Any())
            {
                await LoadDataAsync();
            }

            var memberCodes = GroupMembers.Select(m => m.Code).ToHashSet();
            var allowedCompanies = _currentGroup?.AllowedCompanyCodes ?? new List<string>();

            AvailableUsers = SMSStakeholderUsers
                .Where(u => !memberCodes.Contains(u.Code))
                .Where(u => allowedCompanies.Count == 0
                    || (!string.IsNullOrWhiteSpace(u.Company)
                        && allowedCompanies.Any(c => c.Equals(u.Company, StringComparison.OrdinalIgnoreCase))))
                .ToList();

            // Initialize selection tracking
            SelectedUsers.Clear();
            foreach (var user in AvailableUsers)
            {
                SelectedUsers[user.Code] = false;
            }

            Logger.LogInformation("Loaded {MemberCount} group members and {AvailableCount} available users for group {GroupCode}",
                GroupMembers.Count, AvailableUsers.Count, groupCode);

            GroupMemberCounts[groupCode] = GroupMembers.Count;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading group members for group: {GroupCode}", groupCode);

            // For now, if the query fails, just load empty collections
            GroupMembers = new List<SMSStakeholderUser>();
            AvailableUsers = SMSStakeholderUsers?.ToList() ?? new List<SMSStakeholderUser>();
            GroupMemberCounts[groupCode] = 0;
        }
    }

    private async Task ExitMemberManagement()
    {
        // Reset member management state
        _isManagingMembers = false;
        _currentGroupCode = null;
        _currentGroup = null;
        GroupMembers.Clear();
        AvailableUsers.Clear();
        SelectedUsers.Clear();
        Logger.LogInformation("Exited member management view");
        await EventBus.PublishUIEventAsync(UINotificationEvent.Info("Information", "Returned to group management"));

        Navigation.NavigateToSecure("/System/UserGroups/StakeholderGroups");
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
            var groupId = new SMSStakeholderGroupID(_currentGroupCode);
            var command = new RemoveUserFromStakeholderGroupCommand(userCode, groupId);
            var result = await Mediator.SendAsync(command, CancellationToken.None);

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
            Logger.LogError(ex, "Error removing user {UserCode} from group {GroupCode}", userCode, _currentGroupCode);
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
                    var groupId = new SMSStakeholderGroupID(_currentGroupCode);
                    var command = new AssignUserToStakeholderGroupCommand(userCode, groupId);
                    var result = await Mediator.SendAsync(command, CancellationToken.None);

                    if (result.IsSuccess)
                        successCount++;
                    else
                        failureCount++;
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Error assigning user {UserCode} to group {GroupCode}", userCode, _currentGroupCode);
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
            Logger.LogError(ex, "Error assigning multiple users to group {GroupCode}", _currentGroupCode);
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
            var groupId = new SMSStakeholderGroupID(_currentGroupCode);
            var command = new AssignUserToStakeholderGroupCommand(userCode, groupId);
            var result = await Mediator.SendAsync(command, CancellationToken.None);

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
            Logger.LogError(ex, "Error assigning user {UserCode} to group {GroupCode}", userCode, _currentGroupCode);
            await ShowErrorAsyncNotification("Error assigning user to group. Please try again.");
        }
    }

    #endregion

    
}


