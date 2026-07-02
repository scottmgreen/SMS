using System.Text;

using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

using SMS_Application.Queries;
using SMS_Application.Interfaces;
using SMS_Domain.Events;
using SMS_Shared.Configuration;

using SMS3.Components.Shared.UIHelpers;

namespace SMS3.Components.Pages.SMSRiskManagement.Components;

public partial class FileViewerDialog : ComponentBase
{
    #region Injected Services
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
    [Inject] private IBaseMediator Mediator { get; set; } = default!;
    [Inject] private ILogger<FileViewerDialog> Logger { get; set; } = default!;
    [Inject] private IBaseEventBus EventBus { get; set; } = default!;
    #endregion

    #region Parameters
    [Parameter] public HazardFile? ViewingFile { get; set; }
    [Parameter] public EventCallback OnClose { get; set; }
    #endregion

    #region State Properties
    private string? FileDataUrl { get; set; }
    private string? TextContent { get; set; }
    private bool IsLoading { get; set; } = true;
    #endregion

    #region Lifecycle Methods
    protected override async Task OnParametersSetAsync()
    {
        if (ViewingFile is not null)
        {
            await LoadFileForViewing();
        }
    }
    #endregion

    #region File Loading
    private async Task LoadFileForViewing()
    {
        if (ViewingFile is null)
        {
            await ShowErrorAsyncNotification("File is not available for viewing");
            return;
        }

        TextContent = null;
        FileDataUrl = null;

        var isCloudSource = string.Equals(ViewingFile.StorageType, "Cloud", StringComparison.OrdinalIgnoreCase)
                            && !string.IsNullOrWhiteSpace(ViewingFile.FilePath);

        if (isCloudSource)
        {
            FileDataUrl = ViewingFile.FilePath;
            Logger.LogInformation("Using cloud file path for viewing: {FileName} => {FilePath}", ViewingFile.FileName, ViewingFile.FilePath);
            IsLoading = false;
            StateHasChanged();
            return;
        }

        var fileData = ViewingFile.FileData;
        if (fileData is null || fileData.Length == 0)
        {
            try
            {
                var fileDataResult = await Mediator.SendAsync(new GetHazardFileDataQuery(ViewingFile.Code), CancellationToken.None);
                if (fileDataResult.IsSuccess && fileDataResult.Value?.FileData is { Length: > 0 })
                {
                    ViewingFile.FileData = fileDataResult.Value.FileData;
                    fileData = ViewingFile.FileData;
                }
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Unable to fetch file data for file: {FileName}", ViewingFile.FileName);
            }
        }

        if (fileData is null || fileData.Length == 0)
        {
            Logger.LogWarning("File data is null or empty for file: {FileName}", ViewingFile.FileName);
            await ShowErrorAsyncNotification("File data is not available for viewing");
            return;
        }

        try
        {
            IsLoading = true;
            StateHasChanged();

            Logger.LogInformation("Loading file for viewing: {FileName} ({Size} bytes)",
                ViewingFile.FileName, fileData.Length);

            // For text files, decode the content
            if (IsTextFile(ViewingFile))
            {
                try
                {
                    TextContent = Encoding.UTF8.GetString(fileData);
                    Logger.LogInformation("Loaded text content for file: {FileName} ({Length} characters)",
                        ViewingFile.FileName, TextContent.Length);
                }
                catch (Exception ex)
                {
                    Logger.LogWarning(ex, "Failed to decode text file as UTF-8: {FileName}", ViewingFile.FileName);
                    TextContent = Encoding.Default.GetString(fileData);
                }
            }
            else
            {
                // For binary files, create data URL
                var mimeType = GetMimeType(ViewingFile);
                var base64 = Convert.ToBase64String(fileData);
                FileDataUrl = $"data:{mimeType};base64,{base64}";

                Logger.LogInformation("Created data URL for file: {FileName} with MIME type: {MimeType}",
                    ViewingFile.FileName, mimeType);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading file for viewing: {FileName}", ViewingFile.FileName);
            await ShowErrorAsyncNotification($"Error loading file for viewing: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }
    #endregion

    #region File Type Detection
    private bool IsImageFile(HazardFile file)
    {
        if (file.FileType?.ToLowerInvariant() == "image") return true;

        var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp", ".svg" };
        return imageExtensions.Any(ext => file.FileName?.ToLowerInvariant().EndsWith(ext) == true);
    }

    private bool IsPdfFile(HazardFile file)
    {
        return file.FileType?.ToLowerInvariant() == "pdf" ||
               file.FileName?.ToLowerInvariant().EndsWith(".pdf") == true;
    }

    private bool IsVideoFile(HazardFile file)
    {
        if (file.FileType?.ToLowerInvariant() == "video") return true;

        var videoExtensions = new[] { ".mp4", ".avi", ".mov", ".wmv", ".flv", ".webm", ".mkv" };
        return videoExtensions.Any(ext => file.FileName?.ToLowerInvariant().EndsWith(ext) == true);
    }

    private bool IsAudioFile(HazardFile file)
    {
        if (file.FileType?.ToLowerInvariant() == "audio") return true;

        var audioExtensions = new[] { ".mp3", ".wav", ".m4a", ".aac", ".ogg", ".wma" };
        return audioExtensions.Any(ext => file.FileName?.ToLowerInvariant().EndsWith(ext) == true);
    }

    private bool IsTextFile(HazardFile file)
    {
        if (file.FileType?.ToLowerInvariant() == "text") return true;

        var textExtensions = new[] { ".txt", ".csv", ".log", ".xml", ".json", ".html", ".css", ".js", ".sql", ".md" };
        return textExtensions.Any(ext => file.FileName?.ToLowerInvariant().EndsWith(ext) == true);
    }
    #endregion

    #region Helper Methods
    private string GetMimeType(HazardFile file)
    {
        // Use the stored ContentType if available
        if (!string.IsNullOrEmpty(file.ContentType) && file.ContentType != "application/octet-stream")
        {
            return file.ContentType;
        }

        // Fallback to extension-based detection
        var extension = Path.GetExtension(file.FileName)?.ToLowerInvariant();
        return extension switch
        {
            ".pdf" => "application/pdf",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".bmp" => "image/bmp",
            ".webp" => "image/webp",
            ".svg" => "image/svg+xml",
            ".mp4" => "video/mp4",
            ".avi" => "video/x-msvideo",
            ".mov" => "video/quicktime",
            ".wmv" => "video/x-ms-wmv",
            ".webm" => "video/webm",
            ".mp3" => "audio/mpeg",
            ".wav" => "audio/wav",
            ".m4a" => "audio/mp4",
            ".aac" => "audio/aac",
            ".ogg" => "audio/ogg",
            ".txt" => "text/plain",
            ".csv" => "text/csv",
            ".xml" => "text/xml",
            ".json" => "application/json",
            ".html" => "text/html",
            ".css" => "text/css",
            ".js" => "text/javascript",
            _ => "application/octet-stream"
        };
    }

    private string GetFileIcon(string? fileType)
    {
        return fileType?.ToLowerInvariant() switch
        {
            "pdf" => "picture_as_pdf",
            "image" => "image",
            "video" => "videocam",
            "audio" => "audiotrack",
            "text" => "text_snippet",
            "document" => "description",
            "spreadsheet" => "grid_on",
            "archive" => "archive",
            _ => "insert_drive_file"
        };
    }

    private string GetFileIconColor(string? fileType)
    {
        return fileType?.ToLowerInvariant() switch
        {
            "pdf" => "#d32f2f",
            "image" => "#388e3c",
            "video" => "#1976d2",
            "audio" => "#f57c00",
            "text" => "#616161",
            "document" => "#7b1fa2",
            "spreadsheet" => "#388e3c",
            "archive" => "#795548",
            _ => "#757575"
        };
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
    #endregion

    #region UI Event Handlers
    private async Task CloseViewer()
    {
        Logger.LogInformation("Closing file viewer for: {FileName}", ViewingFile?.FileName);
        await OnClose.InvokeAsync();
    }

    private async Task CloseIfClickedOutside(MouseEventArgs e)
    {
        // This method will be used to close when clicking outside the dialog
        // For now, we'll just close the viewer
        await CloseViewer();
    }

    private async Task OpenInNewTab()
    {
        if (ViewingFile is null || string.IsNullOrEmpty(FileDataUrl))
        {
            await ShowErrorAsyncNotification("File is not ready for viewing");
            return;
        }

        try
        {
            Logger.LogInformation("Opening file in new tab: {FileName}", ViewingFile.FileName);

            await JSRuntime.InvokeVoidAsync("open", FileDataUrl, "_blank");

            await ShowSuccessAsyncNotification($"Opened '{ViewingFile.FileName}' in new tab");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error opening file in new tab: {FileName}", ViewingFile.FileName);
            await ShowErrorAsyncNotification("Unable to open file in new tab");
        }
    }

    private async Task DownloadFile()
    {
        if (ViewingFile is null)
        {
            await ShowErrorAsyncNotification("File is not available for download");
            return;
        }

        var isCloudSource = string.Equals(ViewingFile.StorageType, "Cloud", StringComparison.OrdinalIgnoreCase)
                            && !string.IsNullOrWhiteSpace(ViewingFile.FilePath);

        if (isCloudSource)
        {
            try
            {
                await JSRuntime.InvokeVoidAsync("open", ViewingFile.FilePath, "_blank");
                await ShowSuccessAsyncNotification($"Opened '{ViewingFile.FileName}' in a new tab");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error opening cloud file for download: {FileName}", ViewingFile.FileName);
                await ShowErrorAsyncNotification("Unable to open cloud file");
            }
            return;
        }

        if (ViewingFile.FileData is null || ViewingFile.FileData.Length == 0)
        {
            try
            {
                var fileDataResult = await Mediator.SendAsync(new GetHazardFileDataQuery(ViewingFile.Code), CancellationToken.None);
                if (fileDataResult.IsSuccess && fileDataResult.Value?.FileData is { Length: > 0 })
                {
                    ViewingFile.FileData = fileDataResult.Value.FileData;
                }
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Unable to fetch file data for download: {FileName}", ViewingFile.FileName);
            }
        }

        if (ViewingFile.FileData is null || ViewingFile.FileData.Length == 0)
        {
            await ShowErrorAsyncNotification("File is not available for download");
            return;
        }

        try
        {
            Logger.LogInformation("Starting download for file: {FileName}", ViewingFile.FileName);

            // Create download link using JS interop
            var fileName = ViewingFile.FileName ?? "file";
            var mimeType = GetMimeType(ViewingFile);
            var base64 = Convert.ToBase64String(ViewingFile.FileData);

            await JSRuntime.InvokeVoidAsync("downloadFile", fileName, mimeType, base64);

            await ShowSuccessAsyncNotification($"Download started for '{fileName}'");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error downloading file: {FileName}", ViewingFile?.FileName);
            await ShowErrorAsyncNotification("Unable to download file");
        }
    }
    #endregion

    #region Notification Methods (EventBus-Driven)
    private async Task ShowSuccessAsyncNotification(string message)
    {
        await EventBus.PublishUIEventAsync(UINotificationEvent.Success("Success", message, duration: 3000));
    }

    private async Task ShowErrorAsyncNotification(string message)
    {
        await EventBus.PublishUIEventAsync(UINotificationEvent.Error("Error", message, duration: 5000));
    }
    #endregion
}