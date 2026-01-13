using SMS_Domain.Entities;
using SMS_Domain.ValueObjects;
using SMS_Application.Messaging.Queries;
using SMS_Application.Messaging.Commands;
using SMS_Application.Interfaces;
using SMS_Shared.Common;
using Radzen;
using Microsoft.JSInterop;

namespace SMS3.Components.Pages.SMSRiskManagement.Components;

public partial class EvidenceFilesManager : ComponentBase
{
    #region Injected Services
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private NotificationService NotificationService { get; set; } = default!;
    [Inject] private ILogger<EvidenceFilesManager> Logger { get; set; } = default!;
    [Inject] private DialogService DialogService { get; set; } = default!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
    #endregion

    #region Parameters
    [Parameter] public string HazardCode { get; set; } = default!;
    [Parameter] public string InvestigationCode { get; set; } = default!;
    #endregion

    #region State Properties
    private bool IsLoading { get; set; } = true;
    private int selectedFileTypeIndex { get; set; } = 0;
    private bool ShowFileViewer { get; set; } = false;
    private HazardFile? ViewingFile { get; set; }
    public List<HazardFile> EvidenceFiles { get; set; } = new();
    #endregion

    #region Lifecycle Methods
    protected override async Task OnInitializedAsync()
    {
        await LoadEvidenceFiles();
    }

    protected override async Task OnParametersSetAsync()
    {
        if (!string.IsNullOrEmpty(HazardCode))
        {
            await LoadEvidenceFiles();
            // Force UI update
            await InvokeAsync(StateHasChanged);
        }
    }
    #endregion

    #region Public Methods
    public async Task RefreshFiles()
    {
        await LoadEvidenceFiles();
        await InvokeAsync(StateHasChanged);
    }

    /// <summary>
    /// Force component to refresh - useful for debugging
    /// </summary>
    public async Task ForceRefresh()
    {
        await InvokeAsync(() =>
        {
            Logger.LogInformation("?? ForceRefresh called - Files count: {Count}", EvidenceFiles.Count);
            StateHasChanged();
        });
    }
    #endregion

    #region Data Loading
    private async Task LoadEvidenceFiles()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(HazardCode))
            {
                Logger.LogWarning("HazardCode is null or empty, cannot load evidence files");
                return;
            }

            IsLoading = true;
            
            Logger.LogInformation("?? Loading evidence files for HazardCode: {HazardCode}", HazardCode);
            
            // Load ALL files for this hazard, not just those with HazardCategory = "Evidence"
            // This will include both files uploaded during initial reporting and investigation
            var query = new GetHazardFilesByHazardCodeQuery(HazardCode, false, null); // null removes category filter
            var result = await Mediator.SendAsync(query, CancellationToken.None);

            if (result.IsSuccess && result.Value != null)
            {
                var allFiles = result.Value.ToList();
                Logger.LogInformation("?? Retrieved {Count} total files from database for HazardCode: {HazardCode}", allFiles.Count, HazardCode);

                EvidenceFiles = allFiles
                    .Where(f => f.IsActive) // Only show active files
                    .OrderByDescending(f => f.UploadedDate)
                    .ToList();
                
                Logger.LogInformation("? Filtered to {Count} active evidence files for hazard {HazardCode}", 
                    EvidenceFiles.Count, HazardCode);
                
                // Log details about each file for debugging
                foreach (var file in EvidenceFiles)
                {
                    Logger.LogInformation("?? File: {FileName} | Category: {Category} | Size: {Size} | Type: {Type} | Active: {Active} | Uploaded: {Date}",
                        file.FileName, file.Category ?? "NULL", file.FileSizeBytes, file.FileType, file.IsActive, file.UploadedDate);
                }
            }
            else
            {
                Logger.LogError("? Failed to load evidence files for HazardCode {HazardCode}: {Error}", HazardCode, result.Error?.Message);
                EvidenceFiles = new List<HazardFile>();
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "? Exception loading evidence files for hazard: {HazardCode}", HazardCode);
            ShowErrorNotification("Error loading evidence files");
            EvidenceFiles = new List<HazardFile>();
        }
        finally
        {
            IsLoading = false;
            Logger.LogInformation("?? LoadEvidenceFiles completed. Final count: {Count}", EvidenceFiles.Count);
            StateHasChanged();
        }
    }
    #endregion

    #region File Actions
    private async Task ShowUploadDialog()
    {
        var options = new DialogOptions() 
        { 
            Width = "1098px", 
            Height = "584px",
            Resizable = true,
            Draggable = true,
            CloseDialogOnOverlayClick = false,
            CloseDialogOnEsc = true
        };

        var parameters = new Dictionary<string, object> 
        { 
            { "HazardCode", HazardCode },
            { "InvestigationCode", InvestigationCode }
        };

        var result = await DialogService.OpenAsync<UploadEvidenceDialog>(
            "Upload Evidence File", 
            parameters,
            options);

        if (result == true)
        {
            await RefreshFiles();
            ShowSuccessNotification("Evidence file uploaded successfully");
        }
    }

    private async Task ViewFile(HazardFile file)
    {
        try
        {
            Logger.LogInformation("Opening file viewer for: {FileName}", file.FileName);
            
            ViewingFile = file;
            ShowFileViewer = true;
            StateHasChanged();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error opening file viewer for: {FileName}", file.FileName);
            ShowErrorNotification($"Error opening file: {ex.Message}");
        }
    }

    private async Task CloseFileViewer()
    {
        ShowFileViewer = false;
        ViewingFile = null;
        StateHasChanged();
    }

    private async Task DownloadFile(HazardFile file)
    {
        try
        {
            if (file.FileData == null || file.FileData.Length == 0)
            {
                ShowErrorNotification("File data is not available for download");
                return;
            }

            Logger.LogInformation("Starting download for file: {FileName}", file.FileName);
            
            var fileName = file.FileName ?? "file";
            var mimeType = GetMimeType(file);
            var base64 = Convert.ToBase64String(file.FileData);
            
            await JSRuntime.InvokeVoidAsync("downloadFile", fileName, mimeType, base64);
            
            ShowInfoNotification($"Download started for '{fileName}'");
            
            Logger.LogInformation("Download initiated for file: {FileName} (Code: {Code})", 
                file.FileName, file.Code);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error downloading file: {Code}", file.Code);
            ShowErrorNotification("Error downloading file");
        }
    }

    private async Task DeleteFile(HazardFile file)
    {
        try
        {
            var confirmed = await DialogService.Confirm(
                $"Are you sure you want to delete '{file.FileName}'? This action cannot be undone.",
                "Delete Evidence File",
                new ConfirmOptions() { OkButtonText = "Delete", CancelButtonText = "Cancel" });

            if (confirmed == true)
            {
                // Use a default reason since Radzen doesn't have a built-in prompt
                var reason = "File no longer relevant to investigation";

                // Use the file code instead of Id since HazardFile uses string codes
                var deleteCommand = new DeactivateHazardFileCommand(file.Code.GetHashCode(), reason);
                var result = await Mediator.SendAsync(deleteCommand, CancellationToken.None);

                if (result.IsSuccess)
                {
                    await RefreshFiles();
                    ShowSuccessNotification($"File '{file.FileName}' removed successfully");
                }
                else
                {
                    ShowErrorNotification($"Failed to remove file: {result.Error?.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error removing file: {Code}", file.Code);
            ShowErrorNotification("Error removing file");
        }
    }
    #endregion

    #region File Type Helpers
    private IEnumerable<HazardFile> GetFilesByType(string fileType)
    {
        return fileType switch
        {
            "PDF" => EvidenceFiles.Where(f => IsPdfFile(f)),
            "Image" => EvidenceFiles.Where(f => IsImageFile(f)),
            "Video" => EvidenceFiles.Where(f => IsVideoFile(f)),
            "Other" => EvidenceFiles.Where(f => !IsPdfFile(f) && !IsImageFile(f) && !IsVideoFile(f)),
            _ => EvidenceFiles
        };
    }

    private bool IsPdfFile(HazardFile file)
    {
        return file.FileType?.ToLowerInvariant() == "pdf" || 
               file.FileName?.ToLowerInvariant().EndsWith(".pdf") == true;
    }

    private bool IsImageFile(HazardFile file)
    {
        var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };
        return file.FileType?.ToLowerInvariant().StartsWith("image") == true ||
               imageExtensions.Any(ext => file.FileName?.ToLowerInvariant().EndsWith(ext) == true);
    }

    private bool IsVideoFile(HazardFile file)
    {
        var videoExtensions = new[] { ".mp4", ".avi", ".mov", ".wmv", ".flv", ".webm" };
        return file.FileType?.ToLowerInvariant().StartsWith("video") == true ||
               videoExtensions.Any(ext => file.FileName?.ToLowerInvariant().EndsWith(ext) == true);
    }

    private string GetFileIcon(HazardFile file)
    {
        if (IsPdfFile(file)) return "picture_as_pdf";
        if (IsImageFile(file)) return "image";
        if (IsVideoFile(file)) return "videocam";
        
        var extension = Path.GetExtension(file.FileName)?.ToLowerInvariant();
        return extension switch
        {
            ".doc" or ".docx" => "description",
            ".xls" or ".xlsx" => "grid_on",
            ".ppt" or ".pptx" => "slideshow",
            ".txt" => "text_snippet",
            ".zip" or ".rar" => "archive",
            _ => "insert_drive_file"
        };
    }

    private string GetFileIconColor(HazardFile file)
    {
        if (IsPdfFile(file)) return "#d32f2f";
        if (IsImageFile(file)) return "#388e3c";
        if (IsVideoFile(file)) return "#1976d2";
        
        return "#757575";
    }

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
    #endregion

    #region Notifications
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
            Duration = 4000
        });
    }
    #endregion
}