using System.Text;
using SMS_Domain.Entities;
using SMS3.Components.Shared.UIHelpers;

using Microsoft.JSInterop;

namespace SMS3.Components.Pages.Listings;

public partial class HazardFileListing : ComponentBase
{
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ILogger<HazardFileListing> Logger { get; set; } = default!;
    [Inject] private NotificationService NotificationService { get; set; } = default!;
    [Inject] private DialogService DialogService { get; set; } = default!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;

    private RadzenDataGrid<HazardFile>? filesGrid;
    private IEnumerable<HazardFile> files = new List<HazardFile>();
    private int totalCount;
    private bool isLoading = false;

    // File viewing properties
    private bool showFileModal = false;
    private bool isLoadingFile = false;
    private HazardFile? selectedFile = null;
    private string fileDataUrl = string.Empty;
    private string fileTextContent = string.Empty;
    private string fileViewError = string.Empty;

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

    // ===============================
    // READ FILE FUNCTIONALITY
    // ===============================
    private async Task ReadFile(HazardFile file)
    {
        try
        {
            Logger.LogInformation("Reading file: {Code} - {FileName}", file.Code, file.FileName);

            selectedFile = file;
            showFileModal = true;
            isLoadingFile = true;
            fileViewError = string.Empty;
            fileDataUrl = string.Empty;
            fileTextContent = string.Empty;
            StateHasChanged();

            // Get file data using correct CQRS query
            var query = new GetHazardFileDataQuery(file.Code);
            var result = await Mediator.SendAsync(query, CancellationToken.None);

            if (result.IsFailure || result.Value?.FileData == null)
            {
                fileViewError = "Could not load file data. File may be stored externally or corrupted.";
                Logger.LogWarning("Failed to load file data for: {Code}", file.Code);
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

            Logger.LogInformation("Successfully loaded file data for viewing: {Code}", file.Code);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error reading file: {Code}", file.Code);
            fileViewError = "An error occurred while loading the file.";
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
            var confirmed = await DialogService.Confirm(
                $"Are you sure you want to delete '{file.FileName}'?\n\nThis action cannot be undone.",
                "Delete File",
                new ConfirmOptions()
                {
                    OkButtonText = "Yes, Delete",
                    CancelButtonText = "Cancel"
                });

            if (confirmed == true)
            {
                Logger.LogInformation("Deleting file: {Code} - {FileName}", file.Code, file.FileName);

                // Use correct CQRS command for deactivation (soft delete)
                var command = new DeactivateHazardFileCommand(
                    ExtractIdFromCode(file.Code),
                    "Deleted by user from HazardFileListing"
                );

                var result = await Mediator.SendAsync(command, CancellationToken.None);

                if (result.IsSuccess)
                {
                    ShowSuccessNotification($"File '{file.FileName}' has been deleted successfully.");

                    // Reload the data to reflect changes
                    await LoadInitialData();
                    if (filesGrid != null)
                    {
                        await filesGrid.Reload();
                    }

                    Logger.LogInformation("Successfully deleted file: {Code}", file.Code);
                }
                else
                {
                    ShowErrorNotification($"Failed to delete file: {result.Error?.Message}");
                    Logger.LogError("Failed to delete file {Code}: {Error}", file.Code, result.Error?.Message);
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error deleting file: {Code}", file.Code);
            ShowErrorNotification("An error occurred while deleting the file.");
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
        if (selectedFile == null) return;

        try
        {
            // Get file data if we don't have it
            if (string.IsNullOrEmpty(fileDataUrl) && string.IsNullOrEmpty(fileTextContent))
            {
                var query = new GetHazardFileDataQuery(selectedFile.Code);
                var result = await Mediator.SendAsync(query, CancellationToken.None);

                if (result.IsFailure || result.Value?.FileData == null)
                {
                    ShowErrorNotification("Could not download file - file data not available.");
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

            ShowSuccessNotification($"Downloaded '{selectedFile.FileName}' successfully.");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error downloading file: {Code}", selectedFile.Code);
            ShowErrorNotification("An error occurred while downloading the file.");
        }
    }

    // ===============================
    // HELPER METHODS
    // ===============================
    private string CreateDataUrl(byte[] data, string mimeType)
    {
        var base64 = Convert.ToBase64String(data);
        return $"data:{mimeType};base64,{base64}";
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

    private static bool IsImageFile(string? fileType) =>
        !string.IsNullOrEmpty(fileType) && (fileType.ToLower() is "jpg" or "jpeg" or "png" or "gif" or "bmp" or "webp");

    private static bool IsPdfFile(string? fileType) =>
        !string.IsNullOrEmpty(fileType) && fileType.ToLower() == "pdf";

    private static bool IsTextFile(string? fileType) =>
        !string.IsNullOrEmpty(fileType) && (fileType.ToLower() is "txt" or "csv" or "log" or "xml" or "json");

    private static bool IsVideoFile(string? fileType) =>
        !string.IsNullOrEmpty(fileType) && (fileType.ToLower() is "mp4" or "avi" or "mov" or "wmv" or "webm");

    private static string GetMimeType(string? fileType)
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

    private void ShowErrorNotification(string message)
    {
        NotificationHelper.ShowError(NotificationService, message);
    }

    private void ShowSuccessNotification(string message)
    {
        NotificationHelper.ShowSuccess(NotificationService, message);
    }
}