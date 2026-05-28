// <copyright file="EventDataSourceDiscoveryTest.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Test demonstration of dynamic event data source discovery.
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using SMS_Application.Common;
using Microsoft.AspNetCore.Components;
using Radzen;
using static SMS_Application.Common.SPIConstants;

namespace SMS3.Components.Pages.SMSSystem.EventBus;

/// <summary>
/// Test page to demonstrate dynamic event data source discovery
/// Shows how domain events implementing IEventSource are automatically discovered
/// </summary>
public partial class EventDataSourceDiscoveryTest
{
    [Inject] private ILogger<EventDataSourceDiscoveryTest> Logger { get; set; } = default!;

    private List<EventDataSourceInfo> _eventDataSources = new();
    private Dictionary<string, List<EventDataSourceInfo>> _groupedSources = new();
    private List<string> _dataSourceNames = new();
    private string? _selectedDataSource;
    private bool _isLoading = true;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            Logger.LogInformation("Discovering dynamic event data sources...");

            // Test the dynamic discovery
            _eventDataSources = SPIConstants.SPIDataSources.GetEventDrivenSources();
            _groupedSources = SPIConstants.SPIDataSources.GetEventDrivenSourcesByCategory();
            _dataSourceNames = SPIConstants.SPIDataSources.GetEventDrivenSourceNames();

            Logger.LogInformation("Discovered {Count} event data sources", _eventDataSources.Count);

            foreach (var source in _eventDataSources)
            {
                Logger.LogInformation("   ?? {DisplayName} ({EventType}) - Category: {Category}, Priority: {Priority}",
                    source.DisplayName, source.EventTypeName, source.Category, source.Priority);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to discover event data sources");
        }
        finally
        {
            _isLoading = false;
        }
    }

    private static BadgeStyle GetPriorityBadgeStyle(int priority)
    {
        return priority switch
        {
            1 => BadgeStyle.Danger,      // Highest priority
            2 => BadgeStyle.Warning,     // High priority
            3 => BadgeStyle.Primary,     // Medium priority
            4 => BadgeStyle.Info,        // Low priority
            _ => BadgeStyle.Secondary    // Default
        };
    }
}
