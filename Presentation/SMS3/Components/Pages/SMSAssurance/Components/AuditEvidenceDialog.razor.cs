
using Radzen;

using SMS_Domain.Enums;

using SMS_Shared.Configuration;

using SMS3.Components.Shared.UIHelpers;

namespace SMS3.Components.Pages.SMSAssurance.Components;

public partial class AuditEvidenceDialog : ComponentBase
{
    #region Parameters
    [Parameter] public string AuditCode { get; set; } = string.Empty;
    [Parameter] public string? FindingCode { get; set; }
    [Parameter] public SMSAuditEvidence? Evidence { get; set; }
    [Parameter] public bool IsNew { get; set; } = true;
    #endregion

    #region Injected Services
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ILogger<AuditEvidenceDialog> Logger { get; set; } = default!;
    [Inject] private INotificationHelper  NotificationHelper { get; set; } = default!;
    
    [Inject] private DialogService DialogService { get; set; } = default!;
    #endregion

    #region Component State
    private bool IsSubmitting { get; set; } = false;
    private EvidenceViewModel ViewModel { get; set; } = new();
    #endregion

    #region Lifecycle Methods
    protected override void OnInitialized()
    {
        if (IsNew)
        {
            ViewModel = new EvidenceViewModel
            {
                CollectionDate = DateTime.Today,
                EvidenceType = "Document",
                StorageLocation = "Local",
                ConfidentialityLevel = "Internal",
                RetentionPeriodMonths = 84,
                CollectedBy = "CURRENT_USER" // TODO: Get from auth context
            };
        }
        else if (Evidence != null)
        {
            ViewModel = new EvidenceViewModel
            {
                Code = Evidence.Code!,
                Title = Evidence.Title!,
                Description = Evidence.Description!,
                EvidenceType = Evidence.EvidenceType!,
                Source = Evidence.Source!,
                CollectedBy = Evidence.CollectedBy!,
                CollectionDate = Evidence.CollectionDate,
                FilePath = Evidence.FilePath,
                FileSize = Evidence.FileSize,
                ContentType = Evidence.ContentType,
                StorageLocation = Evidence.StorageLocation!,
                ConfidentialityLevel = Evidence.ConfidentialityLevel!,
                RetentionPeriodMonths = Evidence.RetentionPeriodMonths,
                RetentionReason = Evidence.RetentionReason,
                IsVerified = Evidence.IsVerified,
                VerifiedBy = Evidence.VerifiedBy,
                VerificationDate = Evidence.VerificationDate,
                Notes = Evidence.Notes
            };

            // Extract original filename from file path
            if (!string.IsNullOrEmpty(Evidence.FilePath))
            {
                ViewModel.OriginalFileName = Path.GetFileName(Evidence.FilePath);
            }
        }
    }
    #endregion

    #region Event Handlers
    private async Task OnSubmit()
    {
        if (!ValidateForm()) return;

        try
        {
            IsSubmitting = true;
            StateHasChanged();

            if (IsNew)
            {
                await CreateEvidence();
            }
            else
            {
                await UpdateEvidence();
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error submitting evidence");
            ShowErrorAsyncNotification("Error saving evidence");
        }
        finally
        {
            IsSubmitting = false;
            StateHasChanged();
        }
    }

    private void OnCancel()
    {
        DialogService.Close(null);
    }

    private void OnEvidenceTypeChanged(object value)
    {
        ViewModel.EvidenceType = value?.ToString() ?? "Document";
        StateHasChanged();
    }

    private void OnFileUploadComplete(UploadCompleteEventArgs args)
    {
        try
        {
            // In a real implementation, you would handle the file upload result
            // For now, we'll simulate the file being uploaded
            ViewModel.FilePath = $"/uploads/audit/{AuditCode}/uploaded_file.pdf";
            ViewModel.OriginalFileName = "uploaded_file.pdf";
            ViewModel.FileSize = 1024000; // 1MB simulation
            ViewModel.ContentType = "application/pdf";

            ShowSuccessAsyncNotification("File uploaded successfully");
            StateHasChanged();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error processing uploaded file");
            ShowErrorAsyncNotification("Error processing uploaded file");
        }
    }

    private void OnFileUploadError(UploadErrorEventArgs args)
    {
        ShowErrorAsyncNotification($"File upload failed: {args.Message}");
    }

    private async Task OnDownloadFile()
    {
        try
        {
            // In a real implementation, you would trigger the file download
            // For now, just show a notification
            ShowSuccessAsyncNotification("File download would start here");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error downloading file");
            ShowErrorAsyncNotification("Error downloading file");
        }
    }
    #endregion

    #region CRUD Operations
    private async Task CreateEvidence()
    {
        try
        {
            // For now, just show success since the commands don't exist yet
            ShowSuccessAsyncNotification("Evidence creation feature will be implemented when command handlers are ready");

            // TODO: Implement when CreateSMSAuditEvidenceCommand is available
            /*
            var command = new CreateSMSAuditEvidenceCommand(
                AuditCode,
                ViewModel.Title,
                ViewModel.Description,
                ViewModel.EvidenceType,
                ViewModel.Source,
                ViewModel.CollectedBy,
                ViewModel.CollectionDate,
                "CURRENT_USER"
            );

            var result = await Mediator.SendAsync(command, CancellationToken.None);

            if (result.IsSuccess)
            {
                ShowSuccessAsyncNotification("Evidence uploaded successfully");
                DialogService.Close(result.Value);
            }
            else
            {
                ShowErrorAsyncNotification($"Failed to upload evidence: {result.Error?.Message}");
            }
            */

            DialogService.Close(true);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error creating evidence");
            ShowErrorAsyncNotification("Error uploading evidence");
        }
    }

    private async Task UpdateEvidence()
    {
        try
        {
            // For now, just show success since the commands don't exist yet
            ShowSuccessAsyncNotification("Evidence update feature will be implemented when command handlers are ready");

            // TODO: Implement when UpdateSMSAuditEvidenceCommand is available
            /*
            var command = new UpdateSMSAuditEvidenceCommand(
                ViewModel.Code,
                AuditCode,
                ViewModel.Title,
                ViewModel.Description,
                ViewModel.EvidenceType,
                ViewModel.Source,
                ViewModel.CollectedBy,
                ViewModel.CollectionDate,
                "CURRENT_USER"
            );

            var result = await Mediator.SendAsync(command, CancellationToken.None);

            if (result.IsSuccess)
            {
                ShowSuccessAsyncNotification("Evidence updated successfully");
                DialogService.Close(result.Value);
            }
            else
            {
                ShowErrorAsyncNotification($"Failed to update evidence: {result.Error?.Message}");
            }
            */

            DialogService.Close(true);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error updating evidence");
            ShowErrorAsyncNotification("Error updating evidence");
        }
    }
    #endregion

    #region Helper Methods
    private string GetUploadUrl()
    {
        return $"/api/audit-evidence/upload/{AuditCode}";
    }

    private string GetAcceptedFileTypes()
    {
        return ViewModel.EvidenceType switch
        {
            "Photo" => "image/*",
            "Video" => "video/*",
            "Audio" => "audio/*",
            "Document" => ".pdf,.doc,.docx,.txt,.rtf",
            _ => "*"
        };
    }

    private string GetFileFormatsDescription()
    {
        return ViewModel.EvidenceType switch
        {
            "Photo" => "JPEG, PNG, GIF, BMP, WebP",
            "Video" => "MP4, AVI, MOV, WMV, WebM",
            "Audio" => "MP3, WAV, M4A, OGG",
            "Document" => "PDF, Word, Text, RTF",
            _ => "All file types"
        };
    }

    private string GetFileIcon()
    {
        return ViewModel.EvidenceType switch
        {
            "Photo" => "image",
            "Video" => "movie",
            "Audio" => "audiotrack",
            "Document" => "description",
            _ => "insert_drive_file"
        };
    }

    private string GetContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName)?.ToLowerInvariant();
        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".mp4" => "video/mp4",
            ".mp3" => "audio/mpeg",
            _ => "application/octet-stream"
        };
    }

    private string FormatFileSize(long bytes)
    {
        string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
        int counter = 0;
        decimal number = (decimal)bytes;
        while (Math.Round(number / 1024) >= 1)
        {
            number = number / 1024;
            counter++;
        }
        return string.Format("{0:n1} {1}", number, suffixes[counter]);
    }

    private bool ValidateForm()
    {
        if (string.IsNullOrWhiteSpace(ViewModel.Title))
        {
            ShowErrorAsyncNotification("Evidence title is required");
            return false;
        }

        if (string.IsNullOrWhiteSpace(ViewModel.Description))
        {
            ShowErrorAsyncNotification("Evidence description is required");
            return false;
        }

        if (string.IsNullOrWhiteSpace(ViewModel.Source))
        {
            ShowErrorAsyncNotification("Evidence source is required");
            return false;
        }

        if (string.IsNullOrWhiteSpace(ViewModel.CollectedBy))
        {
            ShowErrorAsyncNotification("Collector information is required");
            return false;
        }

        if (IsNew && string.IsNullOrEmpty(ViewModel.FilePath))
        {
            ShowErrorAsyncNotification("Please upload a file");
            return false;
        }

        return true;
    }
    #endregion

    #region Notification Methods
    private void ShowSuccessAsyncNotification(string message)
    {
        NotificationHelper.ShowSuccessAsync( message);
    }

    private void ShowErrorAsyncNotification(string message)
    {
        NotificationHelper.ShowErrorAsync( message);
    }
    #endregion
}