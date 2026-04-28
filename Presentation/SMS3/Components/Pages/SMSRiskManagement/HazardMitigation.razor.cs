
using SMS3.Components.Shared.UIHelpers;
using SMS3.Configuration.Extensions;

namespace SMS3.Components.Pages.SMSRiskManagement;

/// <summary>
/// Code-behind for HazardMitigation creation/editing page
/// Handles comprehensive mitigation creation and update for SMS compliance
/// </summary>
public partial class HazardMitigation : ComponentBase
{
    #region Injected Services
    [Inject] private IBaseMediator _mediator { get; set; } = default!;
    [Inject] private NavigationManager _navigation { get; set; } = default!;
    [Inject] private INotificationHelper _notificationHelper { get; set; } = default!;
    [Inject] private ILogger<HazardMitigation> _logger { get; set; } = default!;

    [Inject] private ICurrentUserService CurrentUserService { get; set; } = default!;

    #endregion

    #region Parameters
    [Parameter] public string MitigationCode { get; set; } = default!;
    #endregion

    #region State Properties
    private bool IsLoading { get; set; } = true;
    private bool IsSaving { get; set; } = false;
    private bool IsEditMode => !string.IsNullOrWhiteSpace(MitigationCode);

    // Use the Domain Entity directly - NO MODELS!
    public Mitigation CurrentMitigation { get; set; } = new(new MitigationID(Guid.NewGuid().ToString()));

    public string PageTitle => IsEditMode ? "Edit Hazard Mitigation" : "Create Hazard Mitigation";
    public string PageSubtitle => IsEditMode ? $"Modify hazard mitigation strategy {MitigationCode}" : "Create new hazard mitigation strategy";
    #endregion

    #region Dropdown Options
    private readonly List<string> ControlTypes = new()
    {
        "Eliminate", "Engineering Control", "Administrative Control", "Personal Protective Equipment"
    };

    

    

    // ? UPDATED: Replace hardcoded department list with SMSDepartment enum
    private List<string> Departments => SMSDepartment.GetAllDepartments()
        .Select(d => d.Name)
        .OrderBy(name => name)
        .ToList();
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
                    Status = MitigationStatus.PendingApproval,
                    Type = "Administrative Control",
                    Progress = 0,
                    TargetDate = DateTime.Now.AddMonths(3)
                };

                // ?? AUDIT: Set creation audit fields
                CurrentMitigation.CreatedBy = GetCurrentUserId();
                CurrentMitigation.CreatedDate = DateTime.UtcNow;
            }

            _logger.LogInformation("Loaded hazard mitigation {Mode} page for Code: {Code} by user: {UserId}",
                IsEditMode ? "edit" : "creation", MitigationCode ?? "New", GetCurrentUserId());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading hazard mitigation page for Code: {Code}", MitigationCode);
            await _notificationHelper.ShowErrorAsync("Error loading hazard mitigation page");
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

            _logger.LogInformation("Loading existing hazard mitigation: {Code}", MitigationCode);

            var mitigationQuery = new GetMitigationByCodeQuery(new MitigationID(MitigationCode));
            var mitigationResult = await _mediator.SendAsync(mitigationQuery, CancellationToken.None);

            if (mitigationResult.IsSuccess && mitigationResult.Value is not null)
            {
                CurrentMitigation = mitigationResult.Value;
                _logger.LogInformation("Successfully loaded existing hazard mitigation: {Code}", MitigationCode);
            }
            else
            {
                await _notificationHelper.ShowErrorAsync($"Hazard mitigation '{MitigationCode}' not found");
                _logger.LogError("Failed to load hazard mitigation {Code}: {Error}", MitigationCode, mitigationResult.Error?.Message);
                _navigation.NavigateToSecure("/Listings/Mitigations");
                return;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading existing hazard mitigation: {Code}", MitigationCode);
            await _notificationHelper.ShowErrorAsync("Error loading existing hazard mitigation");
            _navigation.NavigateToSecure("/Listings/Mitigations");
        }
    }
    #endregion

    #region Form Actions
    private async Task HandleFormSubmit(Mitigation mitigation)
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
            if (!await ValidateForm())
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
            _navigation.NavigateToSecure("/Listings/Mitigations");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving hazard mitigation for Code: {Code}", MitigationCode);
            await _notificationHelper.ShowErrorAsync("Error saving hazard mitigation");
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

        // ?? AUDIT: Set creation audit fields with current user
        CurrentMitigation.CreatedBy = GetCurrentUserId();
        CurrentMitigation.CreatedDate = DateTime.UtcNow;

        // Save using CREATE command
        var createCommand = new CreateMitigationCommand(CurrentMitigation);
        var result = await _mediator.SendAsync(createCommand, CancellationToken.None);

        if (result.IsSuccess)
        {
            await _notificationHelper.ShowSuccessAsync($"Hazard mitigation {CurrentMitigation.Code} created successfully!");
            _logger.LogInformation("Created hazard mitigation: {Code} by user: {UserId}",
                CurrentMitigation.Code, GetCurrentUserId());
        }
        else
        {
            await _notificationHelper.ShowErrorAsync($"Failed to create hazard mitigation: {result.Error?.Message}");
            _logger.LogError("Failed to create hazard mitigation: {Error} by user: {UserId}",
                result.Error?.Message, GetCurrentUserId());
        }
    }

    private async Task UpdateExistingMitigation()
    {
        // ?? AUDIT: Set update audit fields with current user
        CurrentMitigation.UpdatedBy = GetCurrentUserId();
        CurrentMitigation.UpdatedDate = DateTime.UtcNow;

        // Save using UPDATE command
        var updateCommand = new UpdateMitigationCommand(CurrentMitigation);
        var result = await _mediator.SendAsync(updateCommand, CancellationToken.None);

        if (result.IsSuccess)
        {
            await _notificationHelper.ShowSuccessAsync($"Hazard mitigation {CurrentMitigation.Code} updated successfully!");
            _logger.LogInformation("Updated hazard mitigation: {Code} by user: {UserId}",
                CurrentMitigation.Code, GetCurrentUserId());
        }
        else
        {
            await _notificationHelper.ShowErrorAsync($"Failed to update hazard mitigation: {result.Error?.Message}");
            _logger.LogError("Failed to update hazard mitigation {Code}: {Error} by user: {UserId}",
                CurrentMitigation.Code, result.Error?.Message, GetCurrentUserId());
        }
    }

    private async Task CancelAndReturn()
    {
        _navigation.NavigateToSecure("/Listings/Mitigations");
    }
    #endregion

    #region Validation
    private async Task<bool> ValidateForm()
    {
        if (string.IsNullOrWhiteSpace(CurrentMitigation.Name))
        {
            await _notificationHelper.ShowErrorAsync("Hazard mitigation name is required");
            return false;
        }

        if (string.IsNullOrWhiteSpace(CurrentMitigation.Description))
        {
            await _notificationHelper.ShowErrorAsync("Description is required");
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

    #region Authentication Helpers
    /// <summary>
    /// Get the current authenticated user ID for audit fields
    /// Returns actual user ID instead of "SYSTEM" hardcoding
    /// </summary>
    protected string GetCurrentUserId()
    {
        return CurrentUserService?.UserCode ?? "SYSTEM";
    }

    
    #endregion
}