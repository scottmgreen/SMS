using System.Linq.Expressions;
using System.Text;

using SMS_Domain.Entities;
using SMS_Domain.Events;

using Microsoft.JSInterop;

using Radzen;

using SMS_Application.Interfaces;
using SMS_Application.Commands;
using SMS_Application.Queries;

using SMS_Domain.Enums;
using SMS_Domain.Errors;
using SMS_Domain.ValueObjects;

using SMS_Shared.Configuration;

using SMS3.Components.Shared;
using SMS3.Components.Shared.UIHelpers;

namespace SMS3.Components.Pages.SMSListings;

/// <summary>
/// Hazard File Listing Component - Enhanced with full CRUD operations and advanced filtering
/// Provides comprehensive data grid listing with view details, edit navigation, and delete functionality
/// </summary>
public partial class HazardFileListing : ComponentBase
{
    #region Dependencies
    [Inject] private IBaseMediator _mediator { get; set; } = default!;
    [Inject] private ILogger<HazardFileListing> _logger { get; set; } = default!;
    [Inject] private IBaseEventBus _eventBus { get; set; } = default!;
    [Inject] private DialogService _dialogService { get; set; } = default!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
    #endregion

    #region Properties
    private RadzenDataGrid<HazardFile>? filesGrid;
    private IEnumerable<HazardFile> files = new List<HazardFile>();
    private List<HazardFile> allFiles = new List<HazardFile>(); // Store all files for client-side filtering
    private int totalCount;
    private bool isLoading = false;

    // File viewing properties
    private bool showFileModal = false;
    private bool isLoadingFile = false;
    private HazardFile? selectedFile = null;
    private string fileDataUrl = string.Empty;
    private string fileTextContent = string.Empty;
    private string fileViewError = string.Empty;
    #endregion
    
    #region Lifecycle Methods
    protected override async Task OnInitializedAsync()
    {
        await LoadInitialData();
    }
    #endregion

    #region Data Loading Methods
    private async Task LoadInitialData()
    {
        try
        {
            isLoading = true;
            StateHasChanged();

            _logger.LogInformation("Loading hazard files for listing view");

            var query = new GetActiveHazardFilesQuery();
            var result = await _mediator.SendAsync(query, CancellationToken.None);

            if (result.IsSuccess && result.Value is not null)
            {
                allFiles = result.Value.ToList(); // Store all files for filtering/sorting
                files = allFiles; // Initially show all files
                totalCount = allFiles.Count();
                _logger.LogInformation("Loaded {Count} hazard files for listing", totalCount);

                // Show success notification if we have data
                if (totalCount > 0)
                {
                    await ShowSuccessAsyncNotification($"Successfully loaded {totalCount} hazard files");
                }
                else
                {
                    await ShowInfoAsyncNotification("No hazard files found");
                }
            }
            else
            {
                // Initialize with empty lists to prevent null reference issues
                allFiles = new List<HazardFile>();
                files = allFiles;
                totalCount = 0;
                
                await ShowErrorAsyncNotification("Failed to load hazard files");
                _logger.LogError("Failed to load hazard files: {Error}", result.Error?.Message);
            }
        }
        catch (Exception ex)
        {
            // Ensure we always have valid collections even if an error occurs
            allFiles = new List<HazardFile>();
            files = allFiles;
            totalCount = 0;

            _logger.LogError(ex, "Error loading hazard files");
            await ShowErrorAsyncNotification($"Error loading hazard files: {ex.Message}");
        }
        finally
        {
            isLoading = false;
            StateHasChanged();
        }
    }

    private async Task LoadData(LoadDataArgs args)
    {
        try
        {
            isLoading = true;
            StateHasChanged();

            _logger.LogInformation("LoadData called with Skip: {Skip}, Top: {Top}, OrderBy: {OrderBy}, Filter: {Filter}", 
                args.Skip, args.Top, args.OrderBy, args.Filter);

            // If we don't have all files yet, load them first
            if (allFiles is null || !allFiles.Any())
            {
                _logger.LogInformation("No files cached, loading initial data");
                await LoadInitialData();
                return;
            }

            // Start with all files
            var query = allFiles.AsQueryable();
            _logger.LogInformation("Starting with {Count} total files", query.Count());

            // Apply filtering
            if (!string.IsNullOrEmpty(args.Filter))
            {
                _logger.LogInformation("Applying filter: {Filter}", args.Filter);
                query = ApplyFiltering(query, args);
                _logger.LogInformation("After filtering: {Count} files", query.Count());
            }

            // Get total count after filtering but before paging
            totalCount = query.Count();

            // Apply sorting
            if (!string.IsNullOrEmpty(args.OrderBy))
            {
                _logger.LogInformation("Applying sorting: {OrderBy}", args.OrderBy);
                query = ApplySorting(query, args.OrderBy);
                _logger.LogInformation("Sorting applied successfully");
            }
            else
            {
                // Default sorting by UploadedDate descending
                _logger.LogInformation("Applying default sort by UploadedDate");
                query = query.OrderByDescending(f => f.UploadedDate ?? DateTime.MinValue);
            }

            // Apply paging
            if (args.Skip.HasValue && args.Skip > 0)
            {
                _logger.LogInformation("Applying skip: {Skip}", args.Skip);
                query = query.Skip(args.Skip.Value);
            }

            if (args.Top.HasValue && args.Top > 0)
            {
                _logger.LogInformation("Applying take: {Top}", args.Top);
                query = query.Take(args.Top.Value);
            }

            files = query.ToList();

            _logger.LogInformation("Applied filtering/sorting/paging. Showing {Count} of {Total} files", 
                files.Count(), totalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in LoadData with args: Skip={Skip}, Top={Top}, OrderBy={OrderBy}, Filter={Filter}", 
                args.Skip, args.Top, args.OrderBy, args.Filter);
            await ShowErrorAsyncNotification($"Error loading data: {ex.Message}");
            
            // Fallback to show all data without filtering/sorting
            try
            {
                files = allFiles ?? new List<HazardFile>();
                totalCount = files.Count();
            }
            catch (Exception fallbackEx)
            {
                _logger.LogError(fallbackEx, "Error in LoadData fallback");
                files = new List<HazardFile>();
                totalCount = 0;
            }
        }
        finally
        {
            isLoading = false;
            StateHasChanged();
        }
    }

    /// <summary>
    /// Apply filtering based on Radzen DataGrid filter arguments
    /// </summary>
    private IQueryable<HazardFile> ApplyFiltering(IQueryable<HazardFile> query, LoadDataArgs args)
    {
        try
        {
            _logger.LogInformation("ApplyFiltering called with Filter: {Filter}, Filters count: {FilterCount}", 
                args.Filter, args.Filters?.Count() ?? 0);

            // Handle simple string filter (when user types in the general filter)
            if (!string.IsNullOrEmpty(args.Filter) && !args.Filter.Contains("("))
            {
                var filterValue = args.Filter.ToLower();
                _logger.LogInformation("Applying simple string filter: {FilterValue}", filterValue);
                
                query = query.Where(f => 
                    (!string.IsNullOrEmpty(f.Code) && f.Code.ToLower().Contains(filterValue)) ||
                    (!string.IsNullOrEmpty(f.HazardCode) && f.HazardCode.ToLower().Contains(filterValue)) ||
                    (!string.IsNullOrEmpty(f.ReportCode) && f.ReportCode.ToLower().Contains(filterValue)) ||
                    (!string.IsNullOrEmpty(f.FileName) && f.FileName.ToLower().Contains(filterValue)) ||
                    (!string.IsNullOrEmpty(f.FileType) && f.FileType.ToLower().Contains(filterValue)) ||
                    (!string.IsNullOrEmpty(f.Description) && f.Description.ToLower().Contains(filterValue)) ||
                    (!string.IsNullOrEmpty(f.UploadedBy) && f.UploadedBy.ToLower().Contains(filterValue)) ||
                    (!string.IsNullOrEmpty(f.Category) && f.Category.ToLower().Contains(filterValue))
                );
                return query;
            }

            // Handle advanced column-specific filters
            if (args.Filters is not null && args.Filters.Any())
            {
                _logger.LogInformation("Applying {Count} advanced filters", args.Filters.Count());
                
                foreach (var filter in args.Filters)
                {
                    var columnName = filter.Property?.ToLower();
                    var filterValue = filter.FilterValue?.ToString()?.ToLower();
                    var filterOperator = filter.FilterOperator;

                    _logger.LogInformation("Processing filter - Column: {Column}, Value: {Value}, Operator: {Operator}", 
                        columnName, filterValue, filterOperator);

                    if (string.IsNullOrEmpty(filterValue)) continue;

                    switch (columnName)
                    {
                        case "code":
                            query = ApplyStringFilter(query, f => f.Code, filterValue, filterOperator);
                            break;
                        case "hazardcode":
                            query = ApplyStringFilter(query, f => f.HazardCode, filterValue, filterOperator);
                            break;
                        case "reportcode":
                            query = ApplyStringFilter(query, f => f.ReportCode, filterValue, filterOperator);
                            break;
                        case "filename":
                            query = ApplyStringFilter(query, f => f.FileName, filterValue, filterOperator);
                            break;
                        case "filetype":
                            query = ApplyStringFilter(query, f => f.FileType, filterValue, filterOperator);
                            break;
                        case "description":
                            query = ApplyStringFilter(query, f => f.Description, filterValue, filterOperator);
                            break;
                        case "uploadedby":
                            query = ApplyStringFilter(query, f => f.UploadedBy, filterValue, filterOperator);
                            break;
                        case "category":
                            query = ApplyStringFilter(query, f => f.Category, filterValue, filterOperator);
                            break;
                        case "filesizebytes":
                            if (long.TryParse(filter.FilterValue?.ToString(), out var sizeValue))
                            {
                                query = ApplyNumericFilter(query, f => f.FileSizeBytes, sizeValue, filterOperator);
                            }
                            break;
                        case "uploadeddate":
                            if (DateTime.TryParse(filter.FilterValue?.ToString(), out var uploadedDateValue))
                            {
                                query = ApplyDateFilter(query, f => f.UploadedDate, uploadedDateValue, filterOperator);
                            }
                            break;
                        case "createddate":
                            if (DateTime.TryParse(filter.FilterValue?.ToString(), out var createdDateValue))
                            {
                                query = ApplyDateFilter(query, f => f.CreatedDate, createdDateValue, filterOperator);
                            }
                            break;
                        default:
                            _logger.LogWarning("Unknown filter column: {ColumnName}", columnName);
                            break;
                    }
                }
            }

            return query;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying filters - Filter: {Filter}, Filters: {@Filters}", 
                args.Filter, args.Filters?.Select(f => new { f.Property, f.FilterValue, f.FilterOperator }));
            return query; // Return unfiltered query if filtering fails
        }
    }

    /// <summary>
    /// Apply string-based filtering with different operators
    /// </summary>
    private IQueryable<HazardFile> ApplyStringFilter(IQueryable<HazardFile> query, Expression<Func<HazardFile, string?>> propertySelector, string filterValue, FilterOperator filterOperator)
    {
        return filterOperator switch
        {
            FilterOperator.Contains => query.Where(CombineExpressions(propertySelector, value => !string.IsNullOrEmpty(value) && value.ToLower().Contains(filterValue))),
            FilterOperator.StartsWith => query.Where(CombineExpressions(propertySelector, value => !string.IsNullOrEmpty(value) && value.ToLower().StartsWith(filterValue))),
            FilterOperator.EndsWith => query.Where(CombineExpressions(propertySelector, value => !string.IsNullOrEmpty(value) && value.ToLower().EndsWith(filterValue))),
            FilterOperator.Equals => query.Where(CombineExpressions(propertySelector, value => !string.IsNullOrEmpty(value) && value.ToLower() == filterValue)),
            FilterOperator.NotEquals => query.Where(CombineExpressions(propertySelector, value => string.IsNullOrEmpty(value) || value.ToLower() != filterValue)),
            _ => query.Where(CombineExpressions(propertySelector, value => !string.IsNullOrEmpty(value) && value.ToLower().Contains(filterValue)))
        };
    }

    /// <summary>
    /// Apply numeric filtering (for FileSizeBytes, etc.)
    /// </summary>
    private IQueryable<HazardFile> ApplyNumericFilter(IQueryable<HazardFile> query, Expression<Func<HazardFile, long>> propertySelector, long filterValue, FilterOperator filterOperator)
    {
        return filterOperator switch
        {
            FilterOperator.Equals => query.Where(CombineExpressions(propertySelector, value => value == filterValue)),
            FilterOperator.NotEquals => query.Where(CombineExpressions(propertySelector, value => value != filterValue)),
            FilterOperator.LessThan => query.Where(CombineExpressions(propertySelector, value => value < filterValue)),
            FilterOperator.LessThanOrEquals => query.Where(CombineExpressions(propertySelector, value => value <= filterValue)),
            FilterOperator.GreaterThan => query.Where(CombineExpressions(propertySelector, value => value > filterValue)),
            FilterOperator.GreaterThanOrEquals => query.Where(CombineExpressions(propertySelector, value => value >= filterValue)),
            _ => query.Where(CombineExpressions(propertySelector, value => value == filterValue))
        };
    }

    /// <summary>
    /// Apply date-based filtering with different operators
    /// </summary>
    private IQueryable<HazardFile> ApplyDateFilter(IQueryable<HazardFile> query, Expression<Func<HazardFile, DateTime?>> propertySelector, DateTime filterValue, FilterOperator filterOperator)
    {
        return filterOperator switch
        {
            FilterOperator.Equals => query.Where(CombineExpressions(propertySelector, value => value.HasValue && value.Value.Date == filterValue.Date)),
            FilterOperator.NotEquals => query.Where(CombineExpressions(propertySelector, value => !value.HasValue || value.Value.Date != filterValue.Date)),
            FilterOperator.LessThan => query.Where(CombineExpressions(propertySelector, value => value.HasValue && value.Value.Date < filterValue.Date)),
            FilterOperator.LessThanOrEquals => query.Where(CombineExpressions(propertySelector, value => value.HasValue && value.Value.Date <= filterValue.Date)),
            FilterOperator.GreaterThan => query.Where(CombineExpressions(propertySelector, value => value.HasValue && value.Value.Date > filterValue.Date)),
            FilterOperator.GreaterThanOrEquals => query.Where(CombineExpressions(propertySelector, value => value.HasValue && value.Value.Date >= filterValue.Date)),
            _ => query.Where(CombineExpressions(propertySelector, value => value.HasValue && value.Value.Date == filterValue.Date))
        };
    }

    /// <summary>
    /// Combine property selector with condition expression
    /// </summary>
    private Expression<Func<HazardFile, bool>> CombineExpressions<T>(Expression<Func<HazardFile, T>> propertySelector, Expression<Func<T, bool>> condition)
    {
        var parameter = propertySelector.Parameters[0];
        var property = propertySelector.Body;
        var conditionBody = condition.Body;
        var conditionParameter = condition.Parameters[0];

        // Replace the condition parameter with the property expression
        var visitor = new ParameterReplacementVisitor(conditionParameter, property);
        var newConditionBody = visitor.Visit(conditionBody);

        return Expression.Lambda<Func<HazardFile, bool>>(newConditionBody, parameter);
    }

    /// <summary>
    /// Apply sorting based on OrderBy parameter from Radzen DataGrid
    /// </summary>
    private IQueryable<HazardFile> ApplySorting(IQueryable<HazardFile> query, string orderBy)
    {
        try
        {
            if (string.IsNullOrEmpty(orderBy)) return query;

            var parts = orderBy.Split(' ');
            var propertyName = parts[0].ToLower();
            var isDescending = parts.Length > 1 && parts[1].ToLower() == "desc";

            _logger.LogInformation("Applying sorting: Property={PropertyName}, Descending={IsDescending}", propertyName, isDescending);

            return propertyName switch
            {
                "code" => isDescending ? query.OrderByDescending(f => f.Code ?? "") : query.OrderBy(f => f.Code ?? ""),
                "hazardcode" => isDescending ? query.OrderByDescending(f => f.HazardCode ?? "") : query.OrderBy(f => f.HazardCode ?? ""),
                "reportcode" => isDescending ? query.OrderByDescending(f => f.ReportCode ?? "") : query.OrderBy(f => f.ReportCode ?? ""),
                "filename" => isDescending ? query.OrderByDescending(f => f.FileName ?? "") : query.OrderBy(f => f.FileName ?? ""),
                "filetype" => isDescending ? query.OrderByDescending(f => f.FileType ?? "") : query.OrderBy(f => f.FileType ?? ""),
                "description" => isDescending ? query.OrderByDescending(f => f.Description ?? "") : query.OrderBy(f => f.Description ?? ""),
                "uploadedby" => isDescending ? query.OrderByDescending(f => f.UploadedBy ?? "") : query.OrderBy(f => f.UploadedBy ?? ""),
                "category" => isDescending ? query.OrderByDescending(f => f.Category ?? "") : query.OrderBy(f => f.Category ?? ""),
                "filesizebytes" => isDescending ? query.OrderByDescending(f => f.FileSizeBytes) : query.OrderBy(f => f.FileSizeBytes),
                "uploadeddate" => isDescending ? query.OrderByDescending(f => f.UploadedDate) : query.OrderBy(f => f.UploadedDate),
                "createddate" => isDescending ? query.OrderByDescending(f => f.CreatedDate) : query.OrderBy(f => f.CreatedDate),
                "updateddate" => isDescending ? query.OrderByDescending(f => f.UpdatedDate) : query.OrderBy(f => f.UpdatedDate),
                _ => query.OrderByDescending(f => f.UploadedDate ?? DateTime.MinValue) // Default sort with null handling
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying sorting for OrderBy: {OrderBy}", orderBy);
            return query.OrderByDescending(f => f.UploadedDate ?? DateTime.MinValue); // Fallback to default sort
        }
    }
    #endregion

    #region Helper Methods - Keep existing functionality but improved
    private static Expression<Func<HazardFile, object>> GetPropertyExpression(string propertyName)
    {
        var parameter = Expression.Parameter(typeof(HazardFile), "x");
        var property = Expression.Property(parameter, propertyName);
        var conversion = Expression.Convert(property, typeof(object));
        return Expression.Lambda<Func<HazardFile, object>>(conversion, parameter);
    }

    private string FormatFileSize(long bytes)
    {
        const int scale = 1024;
        string[] orders = { "GB", "MB", "KB", "Bytes" };
        long max = (long)Math.Pow(scale, orders.Length - 1);

        foreach (string order in orders)
        {
            if (bytes > max)
                return $"{decimal.Divide(bytes, max):##.##} {order}";
            max /= scale;
        }
        return "0 Bytes";
    }

    public static bool IsImageFile(string? fileType) =>
        !string.IsNullOrEmpty(fileType) && (fileType.ToLower() is "jpg" or "jpeg" or "png" or "gif" or "bmp" or "webp");

    public static bool IsPdfFile(string? fileType) =>
        !string.IsNullOrEmpty(fileType) && fileType.ToLower() == "pdf";

    public static bool IsTextFile(string? fileType) =>
        !string.IsNullOrEmpty(fileType) && (fileType.ToLower() is "txt" or "csv" or "log" or "xml" or "json");

    public static bool IsVideoFile(string? fileType) =>
        !string.IsNullOrEmpty(fileType) && (fileType.ToLower() is "mp4" or "avi" or "mov" or "wmv" or "webm");

    public static string GetMimeType(string? fileType)
    {
        if (string.IsNullOrEmpty(fileType)) return "application/octet-stream";

        return fileType.ToLower() switch
        {
            "jpg" or "jpeg" => "image/jpeg",
            "png" => "image/png",
            "gif" => "image/gif",
            "bmp" => "image/bmp",
            "webp" => "image/webp",
            "pdf" => "application/pdf",
            "txt" => "text/plain",
            "csv" => "text/csv",
            "xml" => "text/xml",
            "json" => "application/json",
            "mp4" => "video/mp4",
            "avi" => "video/avi",
            "mov" => "video/quicktime",
            "wmv" => "video/x-ms-wmv",
            "webm" => "video/webm",
            "doc" => "application/msword",
            "docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            _ => "application/octet-stream"
        };
    }

    private int ExtractIdFromCode(string code)
    {
        // This is a placeholder implementation
        // You'll need to implement this based on your ID extraction strategy
        // For now, return 1 as a test - the actual implementation will depend on your code format
        return 1;
    }

    /// <summary>
    /// Shows error notification to user via EventBus
    /// </summary>
    private async Task ShowErrorAsyncNotification(string message)
    {
        await _eventBus.PublishUIEventAsync(UINotificationEvent.Error("Error", message, duration: 7000));
    }

    /// <summary>
    /// Shows success notification to user via EventBus
    /// </summary>
    private async Task ShowSuccessAsyncNotification(string message)
    {
        await _eventBus.PublishUIEventAsync(UINotificationEvent.Success("Success", message, duration: 5000));
    }

    /// <summary>
    /// Shows info notification to user via EventBus
    /// </summary>
    private async Task ShowInfoAsyncNotification(string message)
    {
        await _eventBus.PublishUIEventAsync(UINotificationEvent.Info("Information", message, duration: 5000));
    }
    #endregion

    #region File Action Methods - Enhanced with better error handling
    // ===============================
    // READ FILE FUNCTIONALITY
    // ===============================
    private async Task ReadFile(HazardFile file)
    {
        try
        {
            _logger.LogInformation("Reading file: {Code} - {FileName}", file.Code, file.FileName);

            selectedFile = file;
            showFileModal = true;
            isLoadingFile = true;
            fileViewError = string.Empty;
            fileDataUrl = string.Empty;
            fileTextContent = string.Empty;
            StateHasChanged();

            // Get file data using correct CQRS query
            var query = new GetHazardFileDataQuery(file.Code);
            var result = await _mediator.SendAsync(query, CancellationToken.None);

            if (result.IsFailure || result.Value?.FileData is null)
            {
                fileViewError = "Could not load file data. File may be stored externally or corrupted.";
                _logger.LogWarning("Failed to load file data for: {Code}", file.Code);
                return;
            }

            var fileWithData = result.Value;

            // Process based on file type
            if (IsImageFile(file.FileType))
            {
                fileDataUrl = CreateDataUrl(fileWithData.FileData, GetMimeType(file.FileType));
            }
            else if (IsPdfFile(file.FileType))
            {
                fileDataUrl = CreateDataUrl(fileWithData.FileData, "application/pdf");
            }
            else if (IsTextFile(file.FileType))
            {
                fileTextContent = Encoding.UTF8.GetString(fileWithData.FileData);
            }
            else if (IsVideoFile(file.FileType))
            {
                fileDataUrl = CreateDataUrl(fileWithData.FileData, GetMimeType(file.FileType));
            }
            else
            {
                fileViewError = $"Preview not supported for {file.FileType} files. You can download the file instead.";
            }

            _logger.LogInformation("Successfully loaded file data for viewing: {Code}", file.Code);
            await ShowInfoAsyncNotification($"Opened file viewer for {file.FileName}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading file: {Code}", file.Code);
            fileViewError = "An error occurred while loading the file.";
            await ShowErrorAsyncNotification("Error reading file");
        }
        finally
        {
            isLoadingFile = false;
            StateHasChanged();
        }
    }

    // ===============================
    // DELETE FILE FUNCTIONALITY  
    // ===============================
    private async Task DeleteFile(HazardFile file)
    {
        try
        {
            var confirmed = await _dialogService.Confirm(
                $"Are you sure you want to delete '{file.FileName}'?\n\nThis action cannot be undone.",
                "Delete File",
                new ConfirmOptions()
                {
                    OkButtonText = "Yes, Delete",
                    CancelButtonText = "Cancel"
                });

            if (confirmed == true)
            {
                _logger.LogInformation("Deleting file: {Code} - {FileName}", file.Code, file.FileName);

                // Use correct CQRS command for deactivation (soft delete)
                var command = new DeactivateHazardFileCommand(
                    ExtractIdFromCode(file.Code),
                    "Deleted by user from HazardFileListing"
                );

                var result = await _mediator.SendAsync(command, CancellationToken.None);

                if (result.IsSuccess)
                {
                    await ShowSuccessAsyncNotification($"File '{file.FileName}' has been deleted successfully.");

                    // Reload the data to reflect changes
                    await LoadInitialData();
                    StateHasChanged();

                    _logger.LogInformation("Successfully deleted file: {Code}", file.Code);
                }
                else
                {
                    await ShowErrorAsyncNotification($"Failed to delete file: {result.Error?.Message}");
                    _logger.LogError("Failed to delete file {Code}: {Error}", file.Code, result.Error?.Message);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file: {Code}", file.Code);
            await ShowErrorAsyncNotification("An error occurred while deleting the file.");
        }
    }

    // ===============================
    // FILE MODAL METHODS
    // ===============================
    private void CloseFileModal()
    {
        showFileModal = false;
        selectedFile = null;
        fileDataUrl = string.Empty;
        fileTextContent = string.Empty;
        fileViewError = string.Empty;
        StateHasChanged();
    }

    private async Task DownloadFile()
    {
        if (selectedFile is null) return;

        try
        {
            _logger.LogInformation("Downloading file: {Code} - {FileName}", selectedFile.Code, selectedFile.FileName);

            // Get file data if we don't have it
            if (string.IsNullOrEmpty(fileDataUrl) && string.IsNullOrEmpty(fileTextContent))
            {
                var query = new GetHazardFileDataQuery(selectedFile.Code);
                var result = await _mediator.SendAsync(query, CancellationToken.None);

                if (result.IsFailure || result.Value?.FileData is null)
                {
                    await ShowErrorAsyncNotification("Could not download file - file data not available.");
                    return;
                }

                var fileData = Convert.ToBase64String(result.Value.FileData);
                await JSRuntime.InvokeVoidAsync("downloadFileFromBase64", selectedFile.FileName, fileData, GetMimeType(selectedFile.FileType));
            }
            else if (!string.IsNullOrEmpty(fileDataUrl))
            {
                // Extract base64 from data URL
                var base64Data = fileDataUrl.Split(',')[1];
                await JSRuntime.InvokeVoidAsync("downloadFileFromBase64", selectedFile.FileName, base64Data, GetMimeType(selectedFile.FileType));
            }
            else if (!string.IsNullOrEmpty(fileTextContent))
            {
                var textBytes = Encoding.UTF8.GetBytes(fileTextContent);
                var base64Data = Convert.ToBase64String(textBytes);
                await JSRuntime.InvokeVoidAsync("downloadFileFromBase64", selectedFile.FileName, base64Data, "text/plain");
            }

            await ShowSuccessAsyncNotification($"Downloaded '{selectedFile.FileName}' successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading file: {Code}", selectedFile.Code);
            await ShowErrorAsyncNotification("An error occurred while downloading the file.");
        }
    }

    private string CreateDataUrl(byte[] data, string mimeType)
    {
        var base64 = Convert.ToBase64String(data);
        return $"data:{mimeType};base64,{base64}";
    }
    #endregion
}
