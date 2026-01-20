using Microsoft.AspNetCore.Components;
using SMS_Domain.Entities;
using SMS_Domain.ValueObjects;
using SMS_Application.Messaging.Queries;
using SMS_Application.Messaging.Commands;
using SMS_Application.Interfaces;
using SMS_Shared.Common;
using Radzen;
using Radzen.Blazor;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using SMS3.Components.Shared;

namespace SMS3.Components.Pages.Listings;

public partial class RiskAssessmentListing : ComponentBase
{
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ILogger<RiskAssessmentListing> Logger { get; set; } = default!;
    [Inject] private NotificationService NotificationService { get; set; } = default!;
    [Inject] private DialogService DialogService { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;

    private RadzenDataGrid<RiskAssessment>? assessmentsGrid;
    private IEnumerable<RiskAssessment> assessments = new List<RiskAssessment>();
    private int totalCount;
    private bool isLoading = false;
    private bool ShowViewDialog = false;
    private RiskAssessment? SelectedAssessment = null;

    protected override async Task OnInitializedAsync()
    {
        await LoadInitialData();
    }

    private async Task LoadInitialData()
    {
        try
        {
            var query = new GetAllRiskAssessmentsQuery();
            var result = await Mediator.SendAsync(query, CancellationToken.None);

            if (result.IsSuccess && result.Value != null)
            {
                assessments = result.Value;
                totalCount = assessments.Count();
                Logger.LogInformation("Loaded {Count} risk assessments", totalCount);
            }
            else
            {
                ShowErrorNotification("Failed to load risk assessments");
                Logger.LogError("Failed to load risk assessments: {Error}", result.Error?.Message);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading risk assessments");
            ShowErrorNotification("Error loading risk assessments");
        }
    }

    private async Task LoadData(LoadDataArgs args)
    {
        try
        {
            isLoading = true;
            StateHasChanged();

            await LoadInitialData();

            var query = assessments.AsQueryable();
            
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

            assessments = query.ToList();
            totalCount = assessments.Count();
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

    private static Expression<Func<RiskAssessment, object>> GetPropertyExpression(string propertyName)
    {
        var parameter = Expression.Parameter(typeof(RiskAssessment), "x");
        var property = Expression.Property(parameter, propertyName);
        var conversion = Expression.Convert(property, typeof(object));
        return Expression.Lambda<Func<RiskAssessment, object>>(conversion, parameter);
    }

    private async Task ViewAssessment(RiskAssessment assessment)
    {
        try
        {
            Logger.LogInformation("Viewing risk assessment: {Code}", assessment.Code);
            SelectedAssessment = assessment;
            ShowViewDialog = true;
            StateHasChanged();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error viewing risk assessment {Code}", assessment.Code);
            ShowErrorNotification("Error viewing risk assessment");
        }
    }

    private async Task EditAssessment(RiskAssessment assessment)
    {
        try
        {
            Logger.LogInformation("Editing risk assessment: {Code} - Type: {Type}", assessment.Code, assessment.AssessmentType);
            
            // Smart navigation based on assessment type and category
            string navigationUrl = DetermineEditUrl(assessment);
            
            Logger.LogInformation("Navigating to: {Url}", navigationUrl);
            Navigation.NavigateTo(navigationUrl);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error editing risk assessment {Code}", assessment.Code);
            ShowErrorNotification("Error opening risk assessment editor");
        }
    }

    private string DetermineEditUrl(RiskAssessment assessment)
    {
        // Determine the correct edit URL based on assessment type and category
        var assessmentType = assessment.AssessmentType?.Name?.ToLowerInvariant() ?? "";
        var category = assessment.RiskAssessmentCategory?.Name?.ToLowerInvariant() ?? "";
        var hazardCode = assessment.HazardCode ?? "";
        
        // Check if it's a preliminary assessment
        if (assessmentType.Contains("preliminary") || category.Contains("preliminary"))
        {
            // Navigate to Preliminary Risk Assessment
            if (!string.IsNullOrEmpty(hazardCode))
            {
                return $"/SMSRiskManagement/PreliminaryRiskAssessment/{assessment.Code}/{hazardCode}";
            }
            else
            {
                return $"/SMSRiskManagement/PreliminaryRiskAssessment/{assessment.Code}";
            }
        }
        // Check if it's a technical assessment
        else if (assessmentType.Contains("technical") || category.Contains("technical"))
        {
            // Navigate to Technical Assessment
            // For technical assessments, we need to determine the report ID
            var reportId = ExtractReportIdFromAssessment(assessment);
            
            if (!string.IsNullOrEmpty(reportId) && !string.IsNullOrEmpty(hazardCode))
            {
                return $"/SMSRiskManagement/TechnicalAssessment/{reportId}/{hazardCode}/1";
            }
            else if (!string.IsNullOrEmpty(reportId))
            {
                return $"/SMSRiskManagement/TechnicalAssessment/{reportId}";
            }
            else if (!string.IsNullOrEmpty(hazardCode))
            {
                return $"/SMSRiskManagement/TechnicalAssessment/{assessment.Code}/{hazardCode}/1";
            }
            else
            {
                return $"/SMSRiskManagement/TechnicalAssessment/{assessment.Code}";
            }
        }
        // Default to preliminary if type is unclear
        else
        {
            Logger.LogWarning("Could not determine assessment type for {Code}, defaulting to Preliminary", assessment.Code);
            
            if (!string.IsNullOrEmpty(hazardCode))
            {
                return $"/SMSRiskManagement/PreliminaryRiskAssessment/{assessment.Code}/{hazardCode}";
            }
            else
            {
                return $"/SMSRiskManagement/PreliminaryRiskAssessment/{assessment.Code}";
            }
        }
    }

    private string ExtractReportIdFromAssessment(RiskAssessment assessment)
    {
        // Try to extract report ID from assessment code or description
        // Risk assessment codes often follow patterns like RS-0269 (from report RP-0269)
        if (assessment.Code?.StartsWith("RS-") == true)
        {
            return assessment.Code.Replace("RS-", "RP-");
        }
        
        // Check if there's report information in the description
        if (!string.IsNullOrEmpty(assessment.Description) && assessment.Description.Contains("Report"))
        {
            // Try to extract report ID from description like "Created from Report RP-0269"
            var reportMatch = Regex.Match(assessment.Description, @"RP-\d+");
            if (reportMatch.Success)
            {
                return reportMatch.Value;
            }
        }
        
        // Fallback: use the assessment code as-is
        return assessment.Code ?? assessment.Id.Value;
    }

    private async Task DuplicateAssessment(RiskAssessment assessment)
    {
        try
        {
            Logger.LogInformation("Duplicating risk assessment: {Code}", assessment.Code);
            ShowInfoNotification($"Duplicate functionality for assessment {assessment.Code} will be available in a future update");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error duplicating risk assessment {Code}", assessment.Code);
            ShowErrorNotification("Error duplicating risk assessment");
        }
    }

    private async Task ViewHistory(RiskAssessment assessment)
    {
        try
        {
            Logger.LogInformation("Viewing history for risk assessment: {Code}", assessment.Code);
            ShowInfoNotification($"History functionality for assessment {assessment.Code} will be available in a future update");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error viewing history for risk assessment {Code}", assessment.Code);
            ShowErrorNotification("Error viewing risk assessment history");
        }
    }

    private async Task DeleteAssessment(RiskAssessment assessment)
    {
        try
        {
            var confirmResult = await DialogService.Confirm(
                message: $"Are you sure you want to delete risk assessment '{assessment.Name}' ({assessment.Code})?\n\nThis action cannot be undone.",
                title: "Confirm Deletion",
                options: new ConfirmOptions
                {
                    OkButtonText = "Yes, Delete",
                    CancelButtonText = "Cancel",
                    Width = "400px"
                });

            if (confirmResult == true)
            {
                Logger.LogInformation("Deleting risk assessment: {Code}", assessment.Code);

                var deleteCommand = new DeleteRiskAssessmentCommand(new RiskAssessmentID(assessment.Id.Value));
                var result = await Mediator.SendAsync(deleteCommand, CancellationToken.None);

                if (result.IsSuccess)
                {
                    ShowSuccessNotification($"Risk assessment '{assessment.Name}' deleted successfully");
                    Logger.LogInformation("Successfully deleted risk assessment: {Code}", assessment.Code);
                    
                    // Refresh the data grid
                    await LoadInitialData();
                    if (assessmentsGrid != null)
                    {
                        await assessmentsGrid.Reload();
                    }
                    StateHasChanged();
                }
                else
                {
                    ShowErrorNotification($"Failed to delete risk assessment: {result.Error?.Message}");
                    Logger.LogError("Failed to delete risk assessment {Code}: {Error}", assessment.Code, result.Error?.Message);
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error deleting risk assessment {Code}", assessment.Code);
            ShowErrorNotification("Error deleting risk assessment");
        }
    }

    private BadgeStyle GetStatusBadgeStyle(string status)
    {
        return status switch
        {
            "Completed" => BadgeStyle.Success,
            "InProgress" => BadgeStyle.Info,
            "Approved" => BadgeStyle.Primary,
            "OnHold" => BadgeStyle.Warning,
            "Cancelled" => BadgeStyle.Danger,
            "Draft" => BadgeStyle.Secondary,
            _ => BadgeStyle.Secondary
        };
    }

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

    private void ShowInfoNotification(string message)
    {
        NotificationService.Notify(new NotificationMessage
        {
            Severity = NotificationSeverity.Info,
            Summary = "Information",
            Detail = message,
            Duration = 5000
        });
    }
}