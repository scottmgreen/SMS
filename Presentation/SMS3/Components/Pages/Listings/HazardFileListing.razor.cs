using Microsoft.AspNetCore.Components;
using SMS_Domain.Entities;
using SMS_Domain.ValueObjects;
using SMS_Application.Messaging.Queries;
using SMS_Application.Interfaces;
using SMS_Shared.Common;
using Radzen;
using Radzen.Blazor;

namespace SMS3.Components.Pages.Listings;

public partial class HazardFileListing : ComponentBase
{
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ILogger<HazardFileListing> Logger { get; set; } = default!;
    [Inject] private NotificationService NotificationService { get; set; } = default!;

    private RadzenDataGrid<HazardFile>? filesGrid;
    private IEnumerable<HazardFile> files = new List<HazardFile>();
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
            var query = new GetActiveHazardFilesQuery();
            var result = await Mediator.SendAsync(query, CancellationToken.None);

            if (result.IsSuccess && result.Value != null)
            {
                files = result.Value;
                totalCount = files.Count();
                Logger.LogInformation("Loaded {Count} hazard files", totalCount);
            }
            else
            {
                ShowErrorNotification("Failed to load hazard files");
                Logger.LogError("Failed to load hazard files: {Error}", result.Error?.Message);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading hazard files");
            ShowErrorNotification("Error loading hazard files");
        }
    }

    private async Task LoadData(LoadDataArgs args)
    {
        try
        {
            isLoading = true;
            StateHasChanged();

            await LoadInitialData();

            var query = files.AsQueryable();
            
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

            files = query.ToList();
            totalCount = files.Count();
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

    private static Expression<Func<HazardFile, object>> GetPropertyExpression(string propertyName)
    {
        var parameter = Expression.Parameter(typeof(HazardFile), "x");
        var property = Expression.Property(parameter, propertyName);
        var conversion = Expression.Convert(property, typeof(object));
        return Expression.Lambda<Func<HazardFile, object>>(conversion, parameter);
    }

    private void ShowActions(HazardFile file)
    {
        Logger.LogInformation("Actions requested for hazard file: {Code}", file.Code);
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