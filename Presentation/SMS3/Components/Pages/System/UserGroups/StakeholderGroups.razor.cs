using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Radzen;
using SMS_Application.Messaging.Commands;
using SMS_Application.Messaging.Queries;
using SMS_Application.Interfaces;
using SMS_Domain.Entities;
using SMS_Domain.ValueObjects;
using SMS_Shared.Common;

namespace SMS3.Components.Pages.System.UserGroups;

public partial class StakeholderGroups : ComponentBase
{
    #region Dependency Injection

    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ILogger<StakeholderGroups> Logger { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Inject] private NotificationService NotificationService { get; set; } = default!;
    [Inject] private DialogService DialogService { get; set; } = default!;

    #endregion

    #region Parameters

    [Parameter] public string? GroupCode { get; set; }

    #endregion

    #region Properties

    private List<SMSStakeholderGroup> SMSStakeholderGroups { get; set; } = new();
    private List<SMSStakeholderUser> SMSStakeholderUsers { get; set; } = new();
    private List<SMSStakeholderUser> GroupMembers { get; set; } = new();
    private List<SMSStakeholderUser> AvailableUsers { get; set; } = new();
    private SMSStakeholderGroup? CurrentGroup { get; set; }
    private bool IsEditMode { get; set; }
    private bool IsManagingMembers { get; set; }
    private string? CurrentGroupCode { get; set; }

    private string SuccessMessage { get; set; } = string.Empty;
    private string ErrorMessage { get; set; } = string.Empty;
    private bool IsSaving { get; set; } = false;

    #endregion

    #region Modal Properties

    private bool ShowCreateModal { get; set; } = false;
    private bool ShowDeleteModal { get; set; } = false;
    private string NewGroupName { get; set; } = string.Empty;
    private string NewDescription { get; set; } = string.Empty;
    private string EditGroupName { get; set; } = string.Empty;
    private string EditDescription { get; set; } = string.Empty;
    private bool EditIsActive { get; set; } = true;
    private string DeleteGroupCode { get; set; } = string.Empty;
    private string DeleteGroupName { get; set; } = string.Empty;

    #endregion

    #region Member Management

    private Dictionary<string, bool> SelectedUsers { get; set; } = new();

    #endregion

    #region Dropdown Options

    private readonly List<StatusOption> StatusOptions = new()
    {
        new() { Text = "Active", Value = true },
        new() { Text = "Inactive", Value = false }
    };

    #endregion

    #region Lifecycle Methods

    protected override async Task OnInitializedAsync()
    {
        await LoadDataAsync();
        
        // Determine the current mode based on the URL
        var uri = Navigation.ToAbsoluteUri(Navigation.Uri);
        var segments = uri.Segments;
        
        if (segments.Length > 3)
        {
            var action = segments[3].TrimEnd('/');
            var code = segments.Length > 4 ? segments[4].TrimEnd('/') : GroupCode;
            
            switch (action.ToLower())
            {
                case "edit":
                    if (!string.IsNullOrEmpty(code))
                    {
                        await EditGroup(code);
                    }
                    break;
                case "members":
                    if (!string.IsNullOrEmpty(code))
                    {
                        await ManageMembers(code);
                    }
                    break;
            }
        }
    }

    protected override async Task OnParametersSetAsync()
    {
        if (!string.IsNullOrEmpty(GroupCode))
        {
            var uri = Navigation.ToAbsoluteUri(Navigation.Uri);
            if (uri.AbsolutePath.Contains("/Edit/"))
            {
                if (CurrentGroup?.Code != GroupCode)
                {
                    await EditGroup(GroupCode);
                }
            }
            else if (uri.AbsolutePath.Contains("/Members/"))
            {
                if (CurrentGroupCode != GroupCode)
                {
                    await ManageMembers(GroupCode);
                }
            }
        }
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
            var usersQuery = new GetActiveSMSStakeholderUsersQuery();
            var usersResult = await Mediator.SendAsync(usersQuery, CancellationToken.None);
            SMSStakeholderUsers = usersResult.IsSuccess ? 
                usersResult.Value?.ToList() ?? new List<SMSStakeholderUser>() : 
                new List<SMSStakeholderUser>();

            Logger.LogInformation("Loaded {GroupCount} stakeholder groups and {UserCount} stakeholder users", 
                SMSStakeholderGroups.Count, SMSStakeholderUsers.Count);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading data");
            ShowErrorNotification("Error loading data. Please try again.");
        }
    }

    private async Task LoadGroupMembersAsync(string groupCode)
    {
        try
        {
            // Use the existing stored procedure to get users by group code
            var membersQuery = new GetSMSStakeholderUsersByGroupCodeQuery(groupCode);
            var membersResult = await Mediator.SendAsync(membersQuery, CancellationToken.None);
            GroupMembers = membersResult.IsSuccess ? 
                membersResult.Value?.ToList() ?? new List<SMSStakeholderUser>() : 
                new List<SMSStakeholderUser>();

            // Load available users (users not in this group)
            if (!SMSStakeholderUsers.Any())
            {
                await LoadDataAsync();
            }

            var memberCodes = GroupMembers.Select(m => m.Code).ToHashSet();
            AvailableUsers = SMSStakeholderUsers.Where(u => !memberCodes.Contains(u.Code)).ToList();

            // Initialize selection dictionary
            SelectedUsers = AvailableUsers.ToDictionary(u => u.Code, u => false);

            Logger.LogInformation("Loaded {MemberCount} group members and {AvailableCount} available users for group {GroupCode}", 
                GroupMembers.Count, AvailableUsers.Count, groupCode);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading group members for {GroupCode}", groupCode);
            // For now, if the query fails, just load empty collections
            GroupMembers = new List<SMSStakeholderUser>();
            AvailableUsers = SMSStakeholderUsers?.ToList() ?? new List<SMSStakeholderUser>();
            SelectedUsers = new Dictionary<string, bool>();
        }
    }

    #endregion

    #region Edit Operations

    private async Task EditGroup(string groupCode)
    {
        if (string.IsNullOrWhiteSpace(groupCode))
        {
            ShowErrorNotification("Group code is required.");
            return;
        }

        try
        {
            var getGroupQuery = new GetSMSStakeholderGroupByCodeQuery(groupCode);
            var groupResult = await Mediator.SendAsync(getGroupQuery, CancellationToken.None);
            
            if (groupResult.IsFailure)
            {
                ShowErrorNotification("Group not found.");
                Navigation.NavigateTo("/System/UserGroups/StakeholderGroups");
                return;
            }

            CurrentGroup = groupResult.Value;
            IsEditMode = true;
            IsManagingMembers = false;
            
            // Set edit form values
            EditGroupName = CurrentGroup.Name ?? string.Empty;
            EditDescription = CurrentGroup.Description ?? string.Empty;
            EditIsActive = CurrentGroup.IsActive;
            
            // Update URL if needed
            var expectedUrl = $"/System/UserGroups/StakeholderGroups/Edit/{groupCode}";
            if (Navigation.Uri != Navigation.ToAbsoluteUri(expectedUrl).ToString())
            {
                Navigation.NavigateTo(expectedUrl);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading group for edit: {GroupCode}", groupCode);
            ShowErrorNotification("Error loading group. Please try again.");
        }
    }

    private void CancelEdit()
    {
        IsEditMode = false;
        CurrentGroup = null;
        EditGroupName = string.Empty;
        EditDescription = string.Empty;
        EditIsActive = true;
        Navigation.NavigateTo("/System/UserGroups/StakeholderGroups");
    }

    private async Task ManageMembers(string groupCode)
    {
        if (string.IsNullOrWhiteSpace(groupCode))
        {
            ShowErrorNotification("Group code is required to manage members.");
            return;
        }

        try
        {
            // Load group info
            var getGroupQuery = new GetSMSStakeholderGroupByCodeQuery(groupCode);
            var groupResult = await Mediator.SendAsync(getGroupQuery, CancellationToken.None);
            
            if (groupResult.IsSuccess)
            {
                CurrentGroup = groupResult.Value;
                CurrentGroupCode = groupCode;
                IsManagingMembers = true;
                IsEditMode = false;
                
                await LoadGroupMembersAsync(groupCode);
                
                // Update URL if needed
                var expectedUrl = $"/System/UserGroups/StakeholderGroups/Members/{groupCode}";
                if (Navigation.Uri != Navigation.ToAbsoluteUri(expectedUrl).ToString())
                {
                    Navigation.NavigateTo(expectedUrl);
                }
            }
            else
            {
                ShowErrorNotification("Group not found.");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error entering manage members mode for group: {GroupCode}", groupCode);
            ShowErrorNotification("Error entering manage members mode. Please try again.");
        }
    }

    #endregion

    #region CRUD Operations

    private async Task CreateGroup()
    {
        if (string.IsNullOrWhiteSpace(NewGroupName))
        {
            ShowErrorNotification("Group name is required.");
            return;
        }

        try
        {
            IsSaving = true;
            StateHasChanged();

            // Create group entity
            var groupCode = $"SG-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}";
            var groupId = new SMSStakeholderGroupID(groupCode);
            var group = new SMSStakeholderGroup(groupId)
            {
                Code = groupCode,
                Name = NewGroupName,
                Description = NewDescription,
                IsActive = true
            };

            var command = new CreateSMSStakeholderGroupCommand(group);
            var result = await Mediator.SendAsync(command, CancellationToken.None);

            if (result.IsSuccess)
            {
                ShowSuccessNotification($"Stakeholder group '{NewGroupName}' created successfully.");
                CloseCreateModal();
                await LoadDataAsync();
            }
            else
            {
                ShowErrorNotification(result.Error?.Message ?? "Failed to create stakeholder group.");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error creating stakeholder group");
            ShowErrorNotification("Error creating stakeholder group. Please try again.");
        }
        finally
        {
            IsSaving = false;
            StateHasChanged();
        }
    }

    private async Task UpdateGroup()
    {
        if (CurrentGroup == null || string.IsNullOrWhiteSpace(EditGroupName))
        {
            ShowErrorNotification("Group name is required.");
            return;
        }

        try
        {
            IsSaving = true;
            StateHasChanged();

            // Update group properties
            CurrentGroup.Name = EditGroupName;
            CurrentGroup.Description = EditDescription;
            CurrentGroup.IsActive = EditIsActive;

            var updateCommand = new UpdateSMSStakeholderGroupCommand(CurrentGroup);
            var result = await Mediator.SendAsync(updateCommand, CancellationToken.None);

            if (result.IsSuccess)
            {
                ShowSuccessNotification($"Stakeholder group '{EditGroupName}' updated successfully.");
                CancelEdit();
                await LoadDataAsync();
            }
            else
            {
                ShowErrorNotification(result.Error?.Message ?? "Failed to update stakeholder group.");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error updating stakeholder group: {GroupCode}", CurrentGroup.Code);
            ShowErrorNotification("Error updating stakeholder group. Please try again.");
        }
        finally
        {
            IsSaving = false;
            StateHasChanged();
        }
    }

    private async Task DeleteGroup()
    {
        if (string.IsNullOrWhiteSpace(DeleteGroupCode))
        {
            ShowErrorNotification("Group code is required for deletion.");
            return;
        }

        try
        {
            IsSaving = true;
            StateHasChanged();

            // Get existing group to pass to delete command
            var getGroupQuery = new GetSMSStakeholderGroupByCodeQuery(DeleteGroupCode);
            var groupResult = await Mediator.SendAsync(getGroupQuery, CancellationToken.None);
            
            if (groupResult.IsFailure)
            {
                ShowErrorNotification("Group not found.");
                return;
            }

            var deleteCommand = new DeleteSMSStakeholderGroupCommand(groupResult.Value);
            var result = await Mediator.SendAsync(deleteCommand, CancellationToken.None);

            if (result.IsSuccess)
            {
                ShowSuccessNotification("Stakeholder group deleted successfully.");
                CloseDeleteModal();
                await LoadDataAsync();
                
                // If we're managing/editing the deleted group, return to main view
                if (CurrentGroup?.Code == DeleteGroupCode)
                {
                    CancelEdit();
                    IsManagingMembers = false;
                    Navigation.NavigateTo("/System/UserGroups/StakeholderGroups");
                }
            }
            else
            {
                ShowErrorNotification(result.Error?.Message ?? "Failed to delete stakeholder group.");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error deleting stakeholder group: {GroupCode}", DeleteGroupCode);
            ShowErrorNotification("Error deleting stakeholder group. Please try again.");
        }
        finally
        {
            IsSaving = false;
            StateHasChanged();
        }
    }

    private async Task CreateDefaultGroups()
    {
        try
        {
            IsSaving = true;
            StateHasChanged();

            var defaultGroups = new[]
            {
                new { Name = "Airlines", Description = "Commercial airline operators at PDX" },
                new { Name = "Ground Handlers", Description = "Ground handling service providers" },
                new { Name = "Contractors", Description = "Airport contractors and service providers" },
                new { Name = "Cargo Operations", Description = "Cargo handling and logistics providers" },
                new { Name = "Fixed Base Operators", Description = "FBO and general aviation services" },
                new { Name = "Security Providers", Description = "Security and screening service providers" },
                new { Name = "Maintenance Providers", Description = "Aircraft and facility maintenance providers" }
            };

            int createdCount = 0;
            foreach (var defaultGroup in defaultGroups)
            {
                try
                {
                    // Check if group with this name already exists
                    if (SMSStakeholderGroups.Any(g => g.Name.Equals(defaultGroup.Name, StringComparison.OrdinalIgnoreCase)))
                    {
                        continue; // Skip if already exists
                    }

                    var groupCode = $"SG-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}";
                    var groupId = new SMSStakeholderGroupID(groupCode);
                    
                    var group = new SMSStakeholderGroup(groupId)
                    {
                        Code = groupCode,
                        Name = defaultGroup.Name,
                        Description = defaultGroup.Description,
                        IsActive = true
                    };

                    var command = new CreateSMSStakeholderGroupCommand(group);
                    var result = await Mediator.SendAsync(command, CancellationToken.None);

                    if (result.IsSuccess)
                    {
                        createdCount++;
                    }
                    else
                    {
                        Logger.LogWarning("Failed to create default group {GroupName}: {Error}", 
                            defaultGroup.Name, result.Error?.Message);
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogWarning(ex, "Error creating default group {GroupName}", defaultGroup.Name);
                }
            }

            if (createdCount > 0)
            {
                ShowSuccessNotification($"Successfully created {createdCount} default stakeholder groups.");
                await LoadDataAsync();
            }
            else
            {
                ShowErrorNotification("No new default groups were created. They may already exist.");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error creating default stakeholder groups");
            ShowErrorNotification("Error creating default groups. Please try again.");
        }
        finally
        {
            IsSaving = false;
            StateHasChanged();
        }
    }

    #endregion

    #region Member Management Operations

    private async Task RemoveUser(string userCode)
    {
        if (string.IsNullOrWhiteSpace(userCode) || string.IsNullOrWhiteSpace(CurrentGroupCode))
        {
            ShowErrorNotification("User code and group code are required.");
            return;
        }

        try
        {
            var groupId = new SMSStakeholderGroupID(CurrentGroupCode);
            var command = new RemoveUserFromStakeholderGroupCommand(userCode, groupId);
            var result = await Mediator.SendAsync(command, CancellationToken.None);

            if (result.IsSuccess)
            {
                ShowSuccessNotification("User removed from group successfully.");
                await LoadGroupMembersAsync(CurrentGroupCode);
            }
            else
            {
                ShowErrorNotification(result.Error?.Message ?? "Failed to remove user from group.");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error removing user {UserCode} from group {GroupCode}", userCode, CurrentGroupCode);
            ShowErrorNotification("Error removing user from group. Please try again.");
        }
    }

    private async Task AssignMultipleUsers()
    {
        if (string.IsNullOrWhiteSpace(CurrentGroupCode) || !SelectedUsers.Any(s => s.Value))
        {
            ShowErrorNotification("Group code and at least one user must be selected.");
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
                    var groupId = new SMSStakeholderGroupID(CurrentGroupCode);
                    var command = new AssignUserToStakeholderGroupCommand(userCode, groupId);
                    var result = await Mediator.SendAsync(command, CancellationToken.None);

                    if (result.IsSuccess)
                        successCount++;
                    else
                        failureCount++;
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Error assigning user {UserCode} to group {GroupCode}", userCode, CurrentGroupCode);
                    failureCount++;
                }
            }

            if (successCount > 0)
            {
                var message = $"Successfully assigned {successCount} user(s) to group.";
                if (failureCount > 0)
                    message += $" {failureCount} assignment(s) failed.";
                ShowSuccessNotification(message);
                
                await LoadGroupMembersAsync(CurrentGroupCode);
            }
            else
            {
                ShowErrorNotification("Failed to assign any users to group.");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error assigning multiple users to group {GroupCode}", CurrentGroupCode);
            ShowErrorNotification("Error assigning users to group. Please try again.");
        }
    }

    #endregion

    #region Selection Management

    private void ToggleUserSelection(string userCode)
    {
        if (SelectedUsers.ContainsKey(userCode))
        {
            SelectedUsers[userCode] = !SelectedUsers[userCode];
        }
    }

    private void SelectAll(bool select)
    {
        var keys = SelectedUsers.Keys.ToList();
        foreach (var key in keys)
        {
            SelectedUsers[key] = select;
        }
    }

    private void SelectUsersByType(string userType)
    {
        // Clear all selections first
        SelectAll(false);
        
        // Select users of specific type
        foreach (var user in AvailableUsers.Where(u => u.StakeholderType?.Equals(userType, StringComparison.OrdinalIgnoreCase) == true))
        {
            if (SelectedUsers.ContainsKey(user.Code))
            {
                SelectedUsers[user.Code] = true;
            }
        }
    }

    #endregion

    #region Modal Operations

    private void OpenCreateModal()
    {
        NewGroupName = string.Empty;
        NewDescription = string.Empty;
        ShowCreateModal = true;
    }

    private void CloseCreateModal()
    {
        ShowCreateModal = false;
        NewGroupName = string.Empty;
        NewDescription = string.Empty;
    }

    private void ConfirmDelete(string groupCode, string groupName)
    {
        DeleteGroupCode = groupCode;
        DeleteGroupName = groupName;
        ShowDeleteModal = true;
    }

    private void CloseDeleteModal()
    {
        ShowDeleteModal = false;
        DeleteGroupCode = string.Empty;
        DeleteGroupName = string.Empty;
    }

    #endregion

    #region Notification Methods

    private void ShowErrorNotification(string message)
    {
        NotificationService.Notify(new NotificationMessage
        {
            Severity = NotificationSeverity.Error,
            Summary = "Error",
            Detail = message
        });
    }

    private void ShowSuccessNotification(string message)
    {
        NotificationService.Notify(new NotificationMessage
        {
            Severity = NotificationSeverity.Success,
            Summary = "Success",
            Detail = message
        });
    }

    #endregion

    #region Helper Classes

    public class StatusOption
    {
        public string Text { get; set; } = string.Empty;
        public bool Value { get; set; }
    }

    #endregion
}