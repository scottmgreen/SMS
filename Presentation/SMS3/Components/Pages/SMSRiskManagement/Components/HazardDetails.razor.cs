using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;
using SMS3.Components.Shared;
using SMS_Application.Interfaces;
using SMS_Application.Queries;
using SMS_Domain.Entities;
using SMS_Shared.Common;
namespace SMS3.Components.Pages.SMSRiskManagement.Components;

public partial class HazardDetails
{

[Parameter] public string ReportCode { get; set; } = string.Empty;
    [Parameter] public EventCallback<Hazard> OnViewHazardCallback { get; set; }
    [Parameter] public EventCallback<Hazard> OnEditHazardCallback { get; set; }

    private List<Hazard> Hazards = new();
    private bool IsLoading = true;
    private string? ErrorMessage = null;

    protected override async Task OnInitializedAsync()
    {
        await LoadHazardsAsync();
    }

    protected override async Task OnParametersSetAsync()
    {
        if (!string.IsNullOrEmpty(ReportCode))
        {
            await LoadHazardsAsync();
        }
    }

    private async Task LoadHazardsAsync()
    {
        if (string.IsNullOrEmpty(ReportCode))
        {
            IsLoading = false;
            return;
        }

        try
        {
            IsLoading = true;
            ErrorMessage = null;

            Logger.LogInformation("Loading hazards for report: {ReportCode}", ReportCode);

            // Execute GetHazardsByReportCodeQuery via Mediator
            var query = new GetHazardsByReportCodeQuery(new ReportID(ReportCode));
            var result = await Mediator.SendAsync(query, CancellationToken.None);

            if (result.IsSuccess && result.Value != null)
            {
                Hazards = result.Value.ToList();
                Logger.LogInformation("Successfully loaded {Count} hazards for report {ReportCode}", Hazards.Count, ReportCode);
            }
            else
            {
                Hazards = new List<Hazard>();
                ErrorMessage = result.Error?.Message ?? "Failed to load hazards";
                Logger.LogWarning("Failed to load hazards for report {ReportCode}: {Error}", ReportCode, ErrorMessage);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Exception occurred while loading hazards for report {ReportCode}", ReportCode);
            Hazards = new List<Hazard>();
            ErrorMessage = "An unexpected error occurred while loading hazards.";
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }

    private async Task OnViewHazard(Hazard hazard)
    {
        if (OnViewHazardCallback.HasDelegate)
        {
            await OnViewHazardCallback.InvokeAsync(hazard);
        }
    }

    private async Task OnEditHazard(Hazard hazard)
    {
        if (OnEditHazardCallback.HasDelegate)
        {
            await OnEditHazardCallback.InvokeAsync(hazard);
        }
    }

    private BadgeStyle GetHazardStatusBadge(string status)
    {
        return status?.ToLower() switch
        {
            "active" => BadgeStyle.Success,
            "pending" => BadgeStyle.Warning,
            "closed" => BadgeStyle.Secondary,
            "cancelled" => BadgeStyle.Danger,
            "under_review" => BadgeStyle.Info,
            "escalated" => BadgeStyle.Warning,
            _ => BadgeStyle.Light
        };
    }

    private string TruncateText(string text, int maxLength)
    {
        if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
        {
            return text ?? "";
        }
        
        return text.Substring(0, maxLength) + "...";
    }

    /// <summary>
    /// Check if hazard has any location data to display
    /// </summary>
    private bool HasLocationData(Hazard hazard)
    {
        if (hazard is null) return false;

        // Check if we have coordinates in LocationArea
        if (!string.IsNullOrEmpty(hazard.LocationArea))
        {
            var hasCoords = Regex.IsMatch(
                hazard.LocationArea, 
                @"Lat:\s*(-?\d+\.?\d*)", 
                RegexOptions.IgnoreCase);
            
            if (hasCoords) return true;
        }

        // Check if we have any location description
        return !string.IsNullOrEmpty(hazard.LocationArea) || 
               !string.IsNullOrEmpty(hazard.LocationSubArea) || 
               hazard.HazardLocation is not null;
    }
}


