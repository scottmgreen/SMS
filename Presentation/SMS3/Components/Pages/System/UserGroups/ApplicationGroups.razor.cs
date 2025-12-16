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

public partial class ApplicationGroups : ComponentBase
{
    #region Dependency Injection

    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ILogger<ApplicationGroups> Logger { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Inject] private NotificationService NotificationService { get; set; } = default!;
    [Inject] private DialogService DialogService { get; set; } = default!;

    #endregion

    #region Parameters

    [Parameter] public string? GroupCode { get; set; }

    #endregion

    #region Properties

    private List<SMSApplicationGroup> SMSApplicationGroups { get; set; } = new();
    private List<SMSApplicationUser> SMSApplicationUsers { get; set; } = new();
    private SMSApplicationGroup? CurrentGroup { get; set; }
    private bool IsEditMode { get; set; }

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
        
        if (!string.IsNullOrEmpty(GroupCode))
        {
            await EditGroup(GroupCode);
        }
    }

    protected override async Task OnParametersSetAsync()
    {
        if (!string.IsNullOrEmpty(GroupCode) && CurrentGroup?.Code != GroupCode)
        {
            await EditGroup(GroupCode);
        }
        else if (string.IsNullOrEmpty(GroupCode) && IsEditMode)
        {
            CancelEdit();
        }
    }

    #endregion

    #region Data Loading

    private async Task LoadDataAsync()
    {
        try
        {
            // Load Application Groups
            var groupsQuery = new GetAllSMSApplicationGroupsQuery();
            var groupsResult = await Mediator.SendAsync(groupsQuery, CancellationToken.None);
            SMSApplicationGroups = groupsResult.IsSuccess ? 
                groupsResult.Value?.ToList() ?? new List<SMSApplicationGroup>() : 
                new List<SMSApplicationGroup>();

            // Load Application Users for potential group assignments
            var usersQuery = new GetAllSMSApplicationUsersQuery();
            var usersResult = await Mediator.SendAsync(usersQuery, CancellationToken.None);
            SMSApplicationUsers = usersResult.IsSuccess ? 
                usersResult.Value?.ToList() ?? new List<SMSApplicationUser>() : 
                new List<SMSApplicationUser>();

            Logger.LogInformation("Loaded {GroupCount} application groups and {UserCount} application users", 
                SMSApplicationGroups.Count, SMSApplicationUsers.Count);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading data");
            ShowErrorNotification("Error loading data. Please try again.");
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
            var getGroupQuery = new GetSMSApplicationGroupByCodeQuery(groupCode);
            var groupResult = await Mediator.SendAsync(getGroupQuery, CancellationToken.None);
            
            if (groupResult.IsFailure)
            {
                ShowErrorNotification("Group not found.");
                Navigation.NavigateTo("/System/UserGroups/ApplicationGroups");
                return;
            }

            CurrentGroup = groupResult.Value;
            IsEditMode = true;
            
            // Set edit form values
            EditGroupName = CurrentGroup.Name ?? string.Empty;
            EditDescription = CurrentGroup.Description ?? string.Empty;
            EditIsActive = CurrentGroup.IsActive;
            
            // Update URL
            if (GroupCode != groupCode)
            {
                Navigation.NavigateTo($"/System/UserGroups/ApplicationGroups/Edit/{groupCode}");
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
        Navigation.NavigateTo("/System/UserGroups/ApplicationGroups");
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
            var groupCode = $"AG-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}";
            var groupId = new SMSApplicationGroupID(groupCode);
            var group = new SMSApplicationGroup(groupId)
            {
                Code = groupCode,
                Name = NewGroupName,
                Description = NewDescription,
                IsActive = true
            };

            var command = new CreateSMSApplicationGroupCommand(group);
            var result = await Mediator.SendAsync(command, CancellationToken.None);

            if (result.IsSuccess)
            {
                ShowSuccessNotification($"Application group '{NewGroupName}' created successfully.");
                CloseCreateModal();
                await LoadDataAsync();
            }
            else
            {
                ShowErrorNotification(result.Error?.Message ?? "Failed to create application group.");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error creating application group");
            ShowErrorNotification("Error creating application group. Please try again.");
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

            var updateCommand = new UpdateSMSApplicationGroupCommand(CurrentGroup);
            var result = await Mediator.SendAsync(updateCommand, CancellationToken.None);

            if (result.IsSuccess)
            {
                ShowSuccessNotification($"Application group '{EditGroupName}' updated successfully.");
                CancelEdit();
                await LoadDataAsync();
            }
            else
            {
                ShowErrorNotification(result.Error?.Message ?? "Failed to update application group.");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error updating application group: {GroupCode}", CurrentGroup.Code);
            ShowErrorNotification("Error updating application group. Please try again.");
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

            var command = new DeleteSMSApplicationGroupCommand(DeleteGroupCode);
            var result = await Mediator.SendAsync(command, CancellationToken.None);

            if (result.IsSuccess)
            {
                ShowSuccessNotification("Application group deleted successfully.");
                CloseDeleteModal();
                await LoadDataAsync();
                
                // If we're editing the deleted group, cancel edit mode
                if (CurrentGroup?.Code == DeleteGroupCode)
                {
                    CancelEdit();
                }
            }
            else
            {
                ShowErrorNotification(result.Error?.Message ?? "Failed to delete application group.");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error deleting application group: {GroupCode}", DeleteGroupCode);
            ShowErrorNotification("Error deleting application group. Please try again.");
        }
        finally
        {
            IsSaving = false;
            StateHasChanged();
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