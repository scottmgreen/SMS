using Microsoft.AspNetCore.Components;
using SMS_Domain.Entities;
using SMS_Domain.ValueObjects;
using SMS_Application.Messaging.Queries;
using SMS_Application.Interfaces;
using SMS_Shared.Common;
using Radzen;
using Radzen.Blazor;
using System.Linq.Expressions;

namespace SMS3.Components.Pages.Listings;

public partial class InvestigationListing : ComponentBase
{
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ILogger<InvestigationListing> Logger { get; set; } = default!;
    [Inject] private NotificationService NotificationService { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;

    private RadzenDataGrid<Investigation>? investigationsGrid;
    private IEnumerable<Investigation> investigations = new List<Investigation>();
    private int totalCount;
    private bool isLoading = false;

    protected override async Task OnInitializedAsync()
    {
        await LoadInitialData();
    }

    private async Task LoadInitialData()
    {
        try
        {
            var query = new GetAllInvestigationsQuery();
            var result = await Mediator.SendAsync(query, CancellationToken.None);

            if (result.IsSuccess && result.Value != null)
            {
                investigations = result.Value;
                totalCount = investigations.Count();
                Logger.LogInformation("Loaded {Count} investigations", totalCount);
            }
            else
            {
                ShowErrorNotification("Failed to load investigations");
                Logger.LogError("Failed to load investigations: {Error}", result.Error?.Message);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading investigations");
            ShowErrorNotification("Error loading investigations");
        }
    }

    private async Task LoadData(LoadDataArgs args)
    {
        try
        {
            isLoading = true;
            StateHasChanged();

            await LoadInitialData();

            var query = investigations.AsQueryable();
            
            if (!string.IsNullOrEmpty(args.OrderBy))
            {
                query = args.OrderBy.Contains("desc") 
                    ? query.OrderByDescending(GetPropertyExpression(args.OrderBy.Replace(" desc", "")))
                    : query.OrderBy(GetPropertyExpression(args.OrderBy));
            }

            if (args.Skip.HasValue)
            {
                query = query.Skip(args.Skip.Value);
            }

            if (args.Top.HasValue)
            {
                query = query.Take(args.Top.Value);
            }

            investigations = query.ToList();
            totalCount = investigations.Count();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error in LoadData");
            ShowErrorNotification("Error loading data");
        }
        finally
        {
            isLoading = false;
            StateHasChanged();
        }
    }

    private static Expression<Func<Investigation, object>> GetPropertyExpression(string propertyName)
    {
        var parameter = Expression.Parameter(typeof(Investigation), "x");
        var property = Expression.Property(parameter, propertyName);
        var conversion = Expression.Convert(property, typeof(object));
        return Expression.Lambda<Func<Investigation, object>>(conversion, parameter);
    }

    private void ShowActions(Investigation investigation)
    {
        Logger.LogInformation("Actions requested for investigation: {Code}", investigation.Code);
    }

    private void ViewInvestigation(Investigation investigation)
    {
        if (investigation == null) return;
        
        // Navigate to Investigation with HazardCode if available
        var navigationUrl = string.IsNullOrWhiteSpace(investigation.HazardCode) 
            ? $"/SMSRiskManagement/Investigations/{investigation.Code}"
            : $"/SMSRiskManagement/Investigations/{investigation.Code}/{investigation.HazardCode}";
            
        Logger.LogInformation("Navigating to investigation: {Code} with URL: {Url}", investigation.Code, navigationUrl);
        Navigation.NavigateTo(navigationUrl);
    }

    private void EditInvestigation(Investigation investigation)
    {
        if (investigation == null) return;
        
        // Navigate to Investigation edit mode with HazardCode if available
        var navigationUrl = string.IsNullOrWhiteSpace(investigation.HazardCode) 
            ? $"/SMSRiskManagement/Investigations/{investigation.Code}"
            : $"/SMSRiskManagement/Investigations/{investigation.Code}/{investigation.HazardCode}";
            
        Logger.LogInformation("Navigating to edit investigation: {Code} with URL: {Url}", investigation.Code, navigationUrl);
        Navigation.NavigateTo(navigationUrl);
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
}