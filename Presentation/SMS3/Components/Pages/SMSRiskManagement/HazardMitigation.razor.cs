using Microsoft.AspNetCore.Components;
using SMS_Domain.Entities;
using SMS_Domain.ValueObjects;
using SMS_Application.Messaging.Queries;
using SMS_Application.Messaging.Commands;
using SMS_Application.Interfaces;
using SMS_Shared.Common;
using Radzen;
using SMS3.Components.Shared;

namespace SMS3.Components.Pages.SMSRiskManagement;

/// <summary>
/// Code-behind for HazardMitigation creation/editing page
/// Handles comprehensive mitigation creation and update for SMS compliance
/// </summary>
public partial class HazardMitigation : ComponentBase
{
    #region Injected Services
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Inject] private NotificationService NotificationService { get; set; } = default!;
    [Inject] private ILogger<HazardMitigation> Logger { get; set; } = default!;
    
    [CascadingParameter(Name = "AuthService")]
    public AuthenticationService AuthService { get; set; } = default!;
    #endregion

    #region Parameters
    [Parameter] public string MitigationCode { get; set; } = default!;
    #endregion

    #region State Properties
    private bool IsLoading { get; set; } = true;
    private bool IsSaving { get; set; } = false;
    private bool IsEditMode => !string.IsNullOrWhiteSpace(MitigationCode);
    
    // Use the Domain Entity directly - NO MODELS!
    public SMS_Domain.Entities.Mitigation CurrentMitigation { get; set; } = new(new MitigationID(Guid.NewGuid().ToString()));
    
    public string PageTitle => IsEditMode ? "Edit Hazard Mitigation" : "Create Hazard Mitigation";
    public string PageSubtitle => IsEditMode ? $"Modify hazard mitigation strategy {MitigationCode}" : "Create new hazard mitigation strategy";
    #endregion

    #region Dropdown Options
    private readonly List<string> ControlTypes = new()
    {
        "Eliminate", "Engineering Control", "Administrative Control", "Personal Protective Equipment"
    };

    private readonly List<string> PriorityLevels = new()
    {
        "Critical", "High", "Medium", "Low"
    };

    private readonly List<string> StatusLevels = new()
    {
        "Proposed", "Approved", "InProgress", "Completed", "Cancelled", "OnHold"
    };

    private readonly List<string> Departments = new()
    {
        "Airport Operations", "Safety Management", "Maintenance & Engineering", 
        "Aircraft Rescue & Firefighting", "Airport Security", "Air Traffic Control", 
        "Ground Handling Services", "Cargo Operations", "External Contractor"
    };
    #endregion

    #region Lifecycle Methods
    protected override async Task OnInitializedAsync()
    {
        await LoadDataAsync();
    }
    #endregion

    #region Data Loading
    private async Task LoadDataAsync()
    {
        try
        {
            IsLoading = true;

            if (IsEditMode)
            {
                // Edit mode: Load existing mitigation
                await LoadExistingMitigation();
            }
            else
            {
                // Create mode: Initialize new entity with defaults
                var mitigationId = new MitigationID(Guid.NewGuid().ToString());
                CurrentMitigation = new SMS_Domain.Entities.Mitigation(mitigationId)
                {
                    Status = "Proposed",
                    Priority = "Medium",
                    Type = "Administrative Control",
                    Progress = 0,
                    TargetDate = DateTime.Now.AddMonths(3)
                };
                
                // 📋 AUDIT: Set creation audit fields
                CurrentMitigation.CreatedBy = GetCurrentUserId();
                CurrentMitigation.CreatedDate = DateTime.UtcNow;
            }

            Logger.LogInformation("Loaded hazard mitigation {Mode} page for Code: {Code} by user: {UserId}", 
                IsEditMode ? "edit" : "creation", MitigationCode ?? "New", GetCurrentUserId());
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading hazard mitigation page for Code: {Code}", MitigationCode);
            ShowErrorNotification("Error loading hazard mitigation page");
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }

    private async Task LoadExistingMitigation()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(MitigationCode))
                return;

            Logger.LogInformation("Loading existing hazard mitigation: {Code}", MitigationCode);

            var mitigationQuery = new GetMitigationByCodeQuery(new MitigationID(MitigationCode));
            var mitigationResult = await Mediator.SendAsync(mitigationQuery, CancellationToken.None);

            if (mitigationResult.IsSuccess && mitigationResult.Value != null)
            {
                CurrentMitigation = mitigationResult.Value;
                Logger.LogInformation("Successfully loaded existing hazard mitigation: {Code}", MitigationCode);
            }
            else
            {
                ShowErrorNotification($"Hazard mitigation '{MitigationCode}' not found");
                Logger.LogError("Failed to load hazard mitigation {Code}: {Error}", MitigationCode, mitigationResult.Error?.Message);
                Navigation.NavigateTo("/Listings/Mitigations");
                return;
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading existing hazard mitigation: {Code}", MitigationCode);
            ShowErrorNotification("Error loading existing hazard mitigation");
            Navigation.NavigateTo("/Listings/Mitigations");
        }
    }
    #endregion

    #region Form Actions
    private async Task HandleFormSubmit(SMS_Domain.Entities.Mitigation mitigation)
    {
        await SaveMitigation();
    }

    private async Task SaveMitigation()
    {
        try
        {
            IsSaving = true;
            StateHasChanged();

            // Validate required fields
            if (!ValidateForm())
            {
                return;
            }

            if (IsEditMode)
            {
                await UpdateExistingMitigation();
            }
            else
            {
                await CreateNewMitigation();
            }

            // Navigate back to listings
            Navigation.NavigateTo("/Listings/Mitigations");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error saving hazard mitigation for Code: {Code}", MitigationCode);
            ShowErrorNotification("Error saving hazard mitigation");
        }
        finally
        {
            IsSaving = false;
            StateHasChanged();
        }
    }

    private async Task CreateNewMitigation()
    {
        // Generate new code and update entity
        var mitigationCode = $"MIT-{DateTime.Now:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}";
        CurrentMitigation.Code = mitigationCode;

        // 📋 AUDIT: Set creation audit fields with current user
        CurrentMitigation.CreatedBy = GetCurrentUserId();
        CurrentMitigation.CreatedDate = DateTime.UtcNow;

        // Save using CREATE command
        var createCommand = new CreateMitigationCommand(CurrentMitigation);
        var result = await Mediator.SendAsync(createCommand, CancellationToken.None);

        if (result.IsSuccess)
        {
            ShowSuccessNotification($"Hazard mitigation {CurrentMitigation.Code} created successfully!");
            Logger.LogInformation("Created hazard mitigation: {Code} by user: {UserId}", 
                CurrentMitigation.Code, GetCurrentUserId());
        }
        else
        {
            ShowErrorNotification($"Failed to create hazard mitigation: {result.Error?.Message}");
            Logger.LogError("Failed to create hazard mitigation: {Error} by user: {UserId}", 
                result.Error?.Message, GetCurrentUserId());
        }
    }

    private async Task UpdateExistingMitigation()
    {
        // 📋 AUDIT: Set update audit fields with current user
        CurrentMitigation.UpdatedBy = GetCurrentUserId();
        CurrentMitigation.UpdatedDate = DateTime.UtcNow;

        // Save using UPDATE command
        var updateCommand = new UpdateMitigationCommand(CurrentMitigation);
        var result = await Mediator.SendAsync(updateCommand, CancellationToken.None);

        if (result.IsSuccess)
        {
            ShowSuccessNotification($"Hazard mitigation {CurrentMitigation.Code} updated successfully!");
            Logger.LogInformation("Updated hazard mitigation: {Code} by user: {UserId}", 
                CurrentMitigation.Code, GetCurrentUserId());
        }
        else
        {
            ShowErrorNotification($"Failed to update hazard mitigation: {result.Error?.Message}");
            Logger.LogError("Failed to update hazard mitigation {Code}: {Error} by user: {UserId}", 
                CurrentMitigation.Code, result.Error?.Message, GetCurrentUserId());
        }
    }

    private async Task CancelAndReturn()
    {
        Navigation.NavigateTo("/Listings/Mitigations");
    }
    #endregion

    #region Validation
    private bool ValidateForm()
    {
        if (string.IsNullOrWhiteSpace(CurrentMitigation.Name))
        {
            ShowErrorNotification("Hazard mitigation name is required");
            return false;
        }

        if (string.IsNullOrWhiteSpace(CurrentMitigation.Description))
        {
            ShowErrorNotification("Description is required");
            return false;
        }

        return true;
    }
    #endregion

    #region Helper Methods
    // Get the appropriate button text based on mode
    public string GetSaveButtonText()
    {
        return IsEditMode ? "Update Hazard Mitigation" : "Create Hazard Mitigation";
    }

    // Get the appropriate button icon based on mode
    public string GetSaveButtonIcon()
    {
        return IsEditMode ? "save" : "add";
    }
    #endregion

    #region Notifications
    private void ShowSuccessNotification(string message)
    {
        NotificationService.Notify(new NotificationMessage
        {
            Severity = NotificationSeverity.Success,
            Summary = "Success",
            Detail = message,
            Duration = 4000
        });
    }

    private void ShowErrorNotification(string message)
    {
        NotificationService.Notify(new NotificationMessage
        {
            Severity = NotificationSeverity.Error,
            Summary = "Error",
            Detail = message,
            Duration = 6000
        });
    }
    #endregion

    #region Authentication Helpers
    /// <summary>
    /// Get the current authenticated user ID for audit fields
    /// Returns actual user ID instead of "SYSTEM" hardcoding
    /// </summary>
    protected string GetCurrentUserId()
    {
        return AuthService?.CurrentUserId ?? "SYSTEM";
    }

    /// <summary>
    /// Get the current authenticated user display name
    /// </summary>
    protected string GetCurrentUserDisplayName()
    {
        return AuthService?.CurrentUserDisplayName ?? "System User";
    }

    /// <summary>
    /// Check if user is authenticated
    /// </summary>
    protected bool IsUserAuthenticated()
    {
        return AuthService?.IsAuthenticated ?? false;
    }
    #endregion
}