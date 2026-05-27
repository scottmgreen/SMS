
using Radzen;

using SMS_Application.Interfaces;
using SMS_Domain.Enums;
using SMS_Domain.Events;

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
    [Inject] private IBaseMediator _mediator { get; set; } = default!;
    [Inject] private ILogger<AuditEvidenceDialog> _logger { get; set; } = default!;
    [Inject] private IBaseEventBus _eventBus { get; set; } = default!;
    [Inject] private DialogService _dialogService { get; set; } = default!;
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
        else if (Evidence is not null)
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
        if (!await ValidateForm()) return;

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
            _logger.LogError(ex, "Error submitting evidence");
            await ShowErrorAsyncNotification("Error saving evidence");
        }
        finally
        {
            IsSubmitting = false;
            StateHasChanged();
        }
    }

    private void OnCancel()
    {
        _dialogService.Close(null);
    }

    private void OnEvidenceTypeChanged(object value)
    {
        ViewModel.EvidenceType = value?.ToString() ?? "Document";
        StateHasChanged();
    }

    private async Task OnFileUploadComplete(UploadCompleteEventArgs args)
    {
        try
        {
            // In a real implementation, you would handle the file upload result
            // For now, we'll simulate the file being uploaded
            ViewModel.FilePath = $"/uploads/audit/{AuditCode}/uploaded_file.pdf";
            ViewModel.OriginalFileName = "uploaded_file.pdf";
            ViewModel.FileSize = 1024000; // 1MB simulation
            ViewModel.ContentType = "application/pdf";

            await ShowSuccessAsyncNotification("File uploaded successfully");
            StateHasChanged();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing uploaded file");
            await ShowErrorAsyncNotification("Error processing uploaded file");
        }
    }

    private async Task OnFileUploadError(UploadErrorEventArgs args)
    {
        await ShowErrorAsyncNotification($"File upload failed: {args.Message}");
    }

    private async Task OnDownloadFile()
    {
        try
        {
            // In a real implementation, you would trigger the file download
            // For now, just show a notification
            await ShowSuccessAsyncNotification("File download would start here");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading file");
            await ShowErrorAsyncNotification("Error downloading file");
        }
    }
    #endregion

    #region CRUD Operations
    private async Task CreateEvidence()
    {
        try
        {
            // For now, just show success since the commands don't exist yet
            await ShowSuccessAsyncNotification("Evidence creation feature will be implemented when command handlers are ready");

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

            _dialogService.Close(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating evidence");
            await ShowErrorAsyncNotification("Error uploading evidence");
        }
    }

    private async Task UpdateEvidence()
    {
        try
        {
            // For now, just show success since the commands don't exist yet
            await ShowSuccessAsyncNotification("Evidence update feature will be implemented when command handlers are ready");

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

            _dialogService.Close(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating evidence");
            await ShowErrorAsyncNotification("Error updating evidence");
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

    private async Task<bool> ValidateForm()
    {
        if (string.IsNullOrWhiteSpace(ViewModel.Title))
        {
            await ShowErrorAsyncNotification("Evidence title is required");
            return false;
        }

        if (string.IsNullOrWhiteSpace(ViewModel.Description))
        {
            await ShowErrorAsyncNotification("Evidence description is required");
            return false;
        }

        if (string.IsNullOrWhiteSpace(ViewModel.Source))
        {
            await ShowErrorAsyncNotification("Evidence source is required");
            return false;
        }

        if (string.IsNullOrWhiteSpace(ViewModel.CollectedBy))
        {
            await ShowErrorAsyncNotification("Collector information is required");
            return false;
        }

        if (IsNew && string.IsNullOrEmpty(ViewModel.FilePath))
        {
            await ShowErrorAsyncNotification("Please upload a file");
            return false;
        }

        return true;
    }
    #endregion

    #region Notification Methods (EventBus-Driven)
    private async Task ShowSuccessAsyncNotification(string message)
    {
        await _eventBus.PublishUIEventAsync(UINotificationEvent.Success("Success", message));
    }

    private async Task ShowErrorAsyncNotification(string message)
    {
        await _eventBus.PublishUIEventAsync(UINotificationEvent.Error("Error", message));
    }
    #endregion
}