using SMS_Domain.Entities;
using Radzen;

namespace SMS3.Components.Pages.SMSRiskManagement.Components;

public partial class ViewFileDialog : ComponentBase
{
    #region Injected Services
    [Inject] private DialogService DialogService { get; set; } = default!;
    [Inject] private NotificationService NotificationService { get; set; } = default!;
    [Inject] private ILogger<ViewFileDialog> Logger { get; set; } = default!;
    #endregion

    #region Parameters
    [Parameter] public HazardFile HazardFile { get; set; } = default!;
    #endregion

    #region File Type Helpers
    private bool IsImageFile()
    {
        if (HazardFile?.FileType?.ToLowerInvariant().StartsWith("image") == true) return true;
        
        var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };
        return imageExtensions.Any(ext => HazardFile?.FileName?.ToLowerInvariant().EndsWith(ext) == true);
    }

    private bool IsPdfFile()
    {
        return HazardFile?.FileType?.ToLowerInvariant() == "pdf" || 
               HazardFile?.FileName?.ToLowerInvariant().EndsWith(".pdf") == true;
    }

    private bool IsVideoFile()
    {
        if (HazardFile?.FileType?.ToLowerInvariant().StartsWith("video") == true) return true;
        
        var videoExtensions = new[] { ".mp4", ".avi", ".mov", ".wmv", ".flv", ".webm" };
        return videoExtensions.Any(ext => HazardFile?.FileName?.ToLowerInvariant().EndsWith(ext) == true);
    }
    #endregion

    #region Actions
    private void DownloadFile()
    {
        try
        {
            // TODO: Implement actual file download
            // This would typically trigger a download from the server
            ShowInfoNotification($"Download functionality for '{HazardFile.FileName}' would be implemented here");
            
            Logger.LogInformation("File download requested: {FileName} (Code: {Code})", 
                HazardFile.FileName, HazardFile.Code);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error downloading file: {Code}", HazardFile?.Code);
            ShowErrorNotification("Error downloading file");
        }
    }
    #endregion

    #region Notifications
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
    #endregion
}