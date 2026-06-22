using SMS_Shared.Configuration;

using SMS3.Components.Shared.UIHelpers;

namespace SMS3.Components.Pages.SMSAssurance;

public partial class SPIConfiguration
{
    #region Injected Services
    [Inject] private IBaseMediator _mediator { get; set; } = default!;
    [Inject] private DialogService _dialogService { get; set; } = default!;
    
    [Inject] private INotificationHelper _notificationHelper { get; set; } = default!;
    [Inject] private NavigationManager _navigation { get; set; } = default!;
    #endregion

    #region Component References
    private RadzenDataGrid<SafetyPerformanceIndicator>? _spGrid;
    #endregion

    #region Data Properties
    private IEnumerable<SafetyPerformanceIndicator> _allSpis = new List<SafetyPerformanceIndicator>();
    private IEnumerable<SafetyPerformanceIndicator> _filteredSpis = new List<SafetyPerformanceIndicator>();

    // Filter properties
    private string _searchTerm = string.Empty;
    private string? _selectedType = null;
    private string? _selectedDepartment = null;
    private string? _selectedStatus = null;

    // Pagination
    private int _itemsPerPage = 20;

    // Loading state
    private bool _isLoading = true;

    // Dropdown data
    private List<SMSSafetyPerformanceIndicatorType> _availableTypes = new();
    private List<SPIStatus> _availableStatuses = new();
    private List<SPIMeasurementFrequency> _availableFrequencies = new();
    private List<string> _availableDepartments = new();
    #endregion

    #region Computed Properties
    private bool HasActiveFilters =>
        !string.IsNullOrWhiteSpace(_searchTerm) ||
        !string.IsNullOrWhiteSpace(_selectedType) ||
        !string.IsNullOrWhiteSpace(_selectedDepartment) ||
        !string.IsNullOrWhiteSpace(_selectedStatus);
    #endregion

    #region Lifecycle Methods
    protected override async Task OnInitializedAsync()
    {
        try
        {
            await LoadDropdownData();
            await LoadSPIs();
        }
        catch (Exception ex)
        {
            ShowErrorAsyncNotification($"Failed to load SPI configuration: {ex.Message}");
        }
        finally
        {
            _isLoading = false;
        }
    }
    #endregion

    #region Data Loading Methods
    private async Task LoadDropdownData()
    {
        // Load SPI types
        _availableTypes = SMSSafetyPerformanceIndicatorType.GetAllValues().ToList();

        // Load statuses using centralized helper
        _availableStatuses = DropdownHelper.GetSPIStatusOptions();

        // Load frequencies using centralized helper  
        _availableFrequencies = DropdownHelper.GetSPIMeasurementFrequencyOptions();

        // Load departments from SMSDepartment enum
        _availableDepartments = DropdownHelper.GetDepartmentNames();
    }

    private async Task LoadSPIs()
    {
        try
        {
            var query = new GetAllSafetyPerformanceIndicatorsQuery();
            var result = await _mediator.SendAsync(query, CancellationToken.None);

            if (result.IsSuccess && result.Value is not null)
            {
                _allSpis = result.Value.ToList();
                ApplyFilters();
            }
            else
            {
                ShowErrorAsyncNotification($"Failed to load SPIs: {result.Error?.Message}");
            }
        }
        catch (Exception ex)
        {
            ShowErrorAsyncNotification($"Error loading SPIs: {ex.Message}");
        }
    }

    private void ApplyFilters()
    {
        var query = _allSpis.AsQueryable();

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(_searchTerm))
        {
            var lowerSearch = _searchTerm.ToLower();
            query = query.Where(spi =>
                spi.Code.ToLower().Contains(lowerSearch) ||
                spi.Name.ToLower().Contains(lowerSearch) ||
                (spi.Description != null && spi.Description.ToLower().Contains(lowerSearch)));
        }

        // Apply type filter
        if (!string.IsNullOrWhiteSpace(_selectedType))
        {
            query = query.Where(spi => spi.IndicatorType.Value == _selectedType);
        }

        // Apply department filter
        if (!string.IsNullOrWhiteSpace(_selectedDepartment))
        {
            query = query.Where(spi => spi.ResponsibleDepartment == _selectedDepartment);
        }

        // Apply status filter
        if (!string.IsNullOrWhiteSpace(_selectedStatus))
        {
            query = query.Where(spi => spi.Status.Value == _selectedStatus);
        }

        _filteredSpis = query.OrderBy(spi => spi.Code).ToList();
        StateHasChanged();
    }
    #endregion

    #region Filter Event Handlers
    private async Task OnSearchChanged(ChangeEventArgs e)
    {
        _searchTerm = e.Value?.ToString() ?? string.Empty;
        ApplyFilters();
        await Task.CompletedTask;
    }

    private async Task OnFilterChanged(object value)
    {
        ApplyFilters();
        await Task.CompletedTask;
    }
    #endregion

    #region SPI Management Methods
    private async Task CreateNewSPI()
    {
        var newSPI = new SafetyPerformanceIndicator(
            new SafetyPerformanceIndicatorID("PI-0000"),
            "New SPI",
            "Safety Performance Indicator",
            SMSSafetyPerformanceIndicatorType.IncidentRate,
            "SYSTEM"
        );

        await OpenSPIDialog(newSPI, false);
    }

    private async Task EditSPI(SafetyPerformanceIndicator spi)
    {
        // Create a copy for editing
        var editSPI = new SafetyPerformanceIndicator(
            new SafetyPerformanceIndicatorID(spi.Id.Value),
            spi.Name,
            spi.Description ?? string.Empty,
            spi.IndicatorType,
            "SYSTEM"
        );

        // Copy all properties
        editSPI.Code = spi.Code;
        editSPI.MeasurementUnit = spi.MeasurementUnit;
        editSPI.MeasurementFrequency = spi.MeasurementFrequency;
        editSPI.CalculationMethod = spi.CalculationMethod;
        editSPI.DataSource = spi.DataSource;
        editSPI.TargetValue = spi.TargetValue;
        editSPI.AcceptableRange = spi.AcceptableRange;
        editSPI.WarningThreshold = spi.WarningThreshold;
        editSPI.CriticalThreshold = spi.CriticalThreshold;
        editSPI.ResponsibleDepartment = spi.ResponsibleDepartment;
        editSPI.DataOwner = spi.DataOwner;
        editSPI.ReviewAuthority = spi.ReviewAuthority;
        editSPI.NextReviewDate = spi.NextReviewDate;
        editSPI.AlertsEnabled = spi.AlertsEnabled;
        editSPI.AlertRecipients = spi.AlertRecipients;

        await OpenSPIDialog(editSPI, true);
    }

    private async Task DuplicateSPI(SafetyPerformanceIndicator spi)
    {
        var duplicatedSPI = new SafetyPerformanceIndicator(
            new SafetyPerformanceIndicatorID(string.Empty),
            $"Copy of {spi.Name}",
            spi.Description ?? string.Empty,
            spi.IndicatorType,
            "SYSTEM"
        );

        // Copy configuration but reset ID and code
        duplicatedSPI.Code = string.Empty; // Will be auto-generated
        duplicatedSPI.MeasurementUnit = spi.MeasurementUnit;
        duplicatedSPI.MeasurementFrequency = spi.MeasurementFrequency;
        duplicatedSPI.CalculationMethod = spi.CalculationMethod;
        duplicatedSPI.DataSource = spi.DataSource;
        duplicatedSPI.TargetValue = spi.TargetValue;
        duplicatedSPI.AcceptableRange = spi.AcceptableRange;
        duplicatedSPI.WarningThreshold = spi.WarningThreshold;
        duplicatedSPI.CriticalThreshold = spi.CriticalThreshold;
        duplicatedSPI.ResponsibleDepartment = spi.ResponsibleDepartment;
        duplicatedSPI.DataOwner = spi.DataOwner;
        duplicatedSPI.ReviewAuthority = spi.ReviewAuthority;
        duplicatedSPI.AlertsEnabled = spi.AlertsEnabled;
        duplicatedSPI.AlertRecipients = spi.AlertRecipients;

        await OpenSPIDialog(duplicatedSPI, false);
    }

    private async Task DeleteSPI(SafetyPerformanceIndicator spi)
    {
        var confirmed = await _dialogService.Confirm(
            $"Are you sure you want to delete the SPI '{spi.Code} - {spi.Name}'?\n\nThis action cannot be undone and will also delete all associated data points.",
            "Delete SPI",
            new ConfirmOptions()
            {
                OkButtonText = "Yes, Delete",
                CancelButtonText = "Cancel",
                AutoFocusFirstElement = true
            });

        if (confirmed == true)
        {
            try
            {
                var spiId = new SafetyPerformanceIndicatorID(spi.Id.Value);
                var command = new DeleteSafetyPerformanceIndicatorCommand(spiId);
                var result = await _mediator.SendAsync(command, CancellationToken.None);

                if (result.IsSuccess)
                {
                    ShowSuccessAsyncNotification($"SPI '{spi.Code}' has been deleted successfully.");
                    await LoadSPIs();
                }
                else
                {
                    ShowErrorAsyncNotification($"Failed to delete SPI: {result.Error?.Message ?? "Unknown error"}");
                }
            }
            catch (Exception ex)
            {
                ShowErrorAsyncNotification($"Error deleting SPI: {ex.Message}");
            }
        }
    }

    private async Task OpenSPIDialog(SafetyPerformanceIndicator spi, bool isEditMode)
    {
        var options = new DialogOptions()
        {
            Width = "1200px",
            Height = "auto",
            Resizable = true,
            Draggable = true,
            CloseDialogOnOverlayClick = false,
            CloseDialogOnEsc = true,
            ShowClose = true,
            ShowTitle = false, // We're using our own custom header
            CssClass = "sms-spi-dialog"
        };

        var parameters = new Dictionary<string, object?>
        {
            { "SPI", spi },
            { "IsEditMode", isEditMode },
            { "AvailableTypes", _availableTypes },
            { "AvailableStatuses", _availableStatuses },
            { "AvailableFrequencies", _availableFrequencies },
            { "AvailableDepartments", _availableDepartments }
        };

        var result = await _dialogService.OpenAsync<Components.SPIEditDialog>(
            "", // Empty title since we're using custom header
            parameters,
            options);

        if (result is SafetyPerformanceIndicator updatedSPI)
        {
            await SaveSPI(updatedSPI, isEditMode);
        }
    }

    private async Task SaveSPI(SafetyPerformanceIndicator spi, bool isEditMode)
    {
        try
        {
            Result result;

            if (!isEditMode)
            {
                // Create new SPI
                var command = new CreateSafetyPerformanceIndicatorCommand(
                    spi.Name,
                    spi.Description ?? string.Empty,
                    spi.IndicatorType,
                    spi.MeasurementUnit,
                    spi.MeasurementFrequency,
                    spi.CalculationMethod,
                    spi.DataSource,
                    spi.TargetValue,
                    spi.AcceptableRange,
                    spi.WarningThreshold,
                    spi.CriticalThreshold,
                    spi.ResponsibleDepartment,
                    spi.DataOwner,
                    spi.ReviewAuthority,
                    spi.NextReviewDate,
                    spi.AlertsEnabled,
                    spi.AlertRecipients,
                    "SYSTEM"
                );

                result = await _mediator.SendAsync(command, CancellationToken.None);

                if (result.IsSuccess)
                {
                    ShowSuccessAsyncNotification("SPI created successfully.");
                }
            }
            else
            {
                // Update existing SPI
                var spiId = new SafetyPerformanceIndicatorID(spi.Id.Value);
                var command = new UpdateSafetyPerformanceIndicatorCommand(
                    spiId,
                    spi.Code,
                    spi.Name,
                    spi.Description ?? string.Empty,
                    spi.IndicatorType,
                    spi.Status,
                    spi.MeasurementUnit,
                    spi.MeasurementFrequency,
                    spi.CalculationMethod,
                    spi.DataSource,
                    spi.TargetValue,
                    spi.AcceptableRange,
                    spi.WarningThreshold,
                    spi.CriticalThreshold,
                    spi.ResponsibleDepartment,
                    spi.DataOwner,
                    spi.ReviewAuthority,
                    spi.NextReviewDate,
                    spi.LastReviewDate,
                    spi.LastReviewNotes,
                    spi.AlertsEnabled,
                    spi.AlertRecipients,
                    "SYSTEM"
                );

                result = await _mediator.SendAsync(command, CancellationToken.None);

                if (result.IsSuccess)
                {
                    ShowSuccessAsyncNotification("SPI updated successfully.");
                }
            }

            if (result.IsSuccess)
            {
                await LoadSPIs();
            }
            else
            {
                ShowErrorAsyncNotification($"Failed to save SPI: {result.Error?.Message ?? "Unknown error"}");
            }
        }
        catch (Exception ex)
        {
            ShowErrorAsyncNotification($"Error saving SPI: {ex.Message}");
        }
    }

    private async Task ViewDataPoints(SafetyPerformanceIndicator spi)
    {
        try
        {
            // Navigate to SPI Dashboard with specific SPI filter - this works with your existing infrastructure
            var url = $"/SMSAssurance/SPIDashboard?spiCode={Uri.EscapeDataString(spi.Code)}";
            _navigation.NavigateTo(url);

            ShowInfoAsyncNotification($"Opening SPI Dashboard for {spi.Name} - View automated data points created by the system");
        }
        catch (Exception ex)
        {
            ShowErrorAsyncNotification($"Failed to navigate to data points: {ex.Message}");
        }
    }
    #endregion

    #region Filter Options Methods
    private List<FilterOption> GetTypeFilterOptions()
    {
        return _availableTypes.Select(t => new FilterOption
        {
            Text = $"{t.Name} ({t.Category})",
            Value = t.Value
        }).ToList();
    }

    private List<string> GetDepartmentFilterOptions()
    {
        return _availableDepartments;
    }

    private List<FilterOption> GetStatusFilterOptions()
    {
        return _availableStatuses.Select(s => new FilterOption
        {
            Text = s.Name,
            Value = s.Value
        }).ToList();
    }
    #endregion

    #region UI Helper Methods
    private BadgeStyle GetStatusBadgeStyle(SPIStatus status)
    {
        return status.Value switch
        {
            "ACTIVE" => BadgeStyle.Success,
            "INACTIVE" => BadgeStyle.Secondary,
            "UNDER_REVIEW" => BadgeStyle.Warning,
            "DEPRECATED" => BadgeStyle.Danger,
            _ => BadgeStyle.Light
        };
    }

    private BadgeStyle GetTypeBadgeStyle(SMSSafetyPerformanceIndicatorType type)
    {
        return type.Category switch
        {
            "Leading" => BadgeStyle.Primary,
            "Lagging" => BadgeStyle.Danger,
            "Process" => BadgeStyle.Base,
            "Compliance" => BadgeStyle.Warning,
            _ => BadgeStyle.Secondary
        };
    }

    private string GetTypeDisplayName(SMSSafetyPerformanceIndicatorType type)
    {
        return $"{type.Name} ({type.Category})";
    }

    private string TruncateText(string text, int maxLength)
    {
        if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
            return text;

        return text.Substring(0, maxLength) + "...";
    }
    #endregion

    #region Notification Methods
    private void ShowSuccessAsyncNotification(string message)
    {
        _notificationHelper.ShowSuccessAsync(message);
    }

    private void ShowErrorAsyncNotification(string message)
    {
        _notificationHelper.ShowErrorAsync(message);
    }

    private void ShowInfoAsyncNotification(string message)
    {
        _notificationHelper.ShowInfoAsync(message);
    }
    #endregion

    #region Supporting Types
    #endregion
}