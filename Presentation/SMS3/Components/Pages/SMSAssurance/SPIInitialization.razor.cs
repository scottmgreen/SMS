//-----------------------------------------------------------------------
// <copyright file="SPIInitialization.razor.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: SPI initialization page for setting up default Safety Performance
//                  Indicators with automated calculation configurations.
// </copyright>
//-----------------------------------------------------------------------

using SMS3.Components.Shared.UIHelpers;
using SMS_Application.Services;

namespace SMS3.Components.Pages.SMSAssurance;

public partial class SPIInitialization : ComponentBase
{
    #region Injected Services
    [Inject] private SPIInitializationService _initService { get; set; } = default!;
    [Inject] private ILogger<SPIInitialization> _logger { get; set; } = default!;
    [Inject] private INotificationHelper _notificationHelper { get; set; } = default!;
    [Inject] private NavigationManager _navigation { get; set; } = default!;
    #endregion

    #region Component State
    private bool IsInitializing { get; set; } = false;
    private bool InitializationComplete { get; set; } = false;
    private string StatusMessage { get; set; } = string.Empty;
    #endregion

    /// <summary>
    /// Initialize default SPIs
    /// </summary>
    private async Task InitializeDefaultSPIs()
    {
        try
        {
            IsInitializing = true;
            StatusMessage = "Initializing default Safety Performance Indicators...";
            StateHasChanged();

            _logger.LogInformation("Starting SPI initialization from UI");

            var result = await _initService.InitializeDefaultSPIsAsync();

            if (result.IsSuccess)
            {
                StatusMessage = "Default SPIs initialized successfully!";
                InitializationComplete = true;
                await _notificationHelper.ShowSuccessAsync("Default SPIs have been initialized successfully!");

                _logger.LogInformation("SPI initialization completed successfully");
            }
            else
            {
                StatusMessage = $"Failed to initialize SPIs: {result.Error?.Message}";
                await _notificationHelper.ShowErrorAsync($"Failed to initialize SPIs: {result.Error?.Message}");

                _logger.LogError("SPI initialization failed: {Error}", result.Error?.Message);
            }
        }
        catch (Exception ex)
        {
            StatusMessage = "Error during SPI initialization";
            await _notificationHelper.ShowErrorAsync("An error occurred during SPI initialization");

            _logger.LogError(ex, "Exception during SPI initialization");
        }
        finally
        {
            IsInitializing = false;
            StateHasChanged();
        }
    }

    /// <summary>
    /// Navigate to SPI Dashboard
    /// </summary>
    private void NavigateToDashboard()
    {
        _navigation.NavigateTo("/SMSAssurance/SPIDashboard");
    }
}
