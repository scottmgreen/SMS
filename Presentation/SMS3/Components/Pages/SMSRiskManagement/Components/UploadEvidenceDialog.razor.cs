using SMS_Domain.Entities;
using SMS_Domain.ValueObjects;
using SMS_Application.Messaging.Commands;
using SMS_Application.Interfaces;
using SMS_Shared.Common;
using Radzen;

namespace SMS3.Components.Pages.SMSRiskManagement.Components;

public partial class UploadEvidenceDialog : ComponentBase
{
    #region Injected Services
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private NotificationService NotificationService { get; set; } = default!;
    [Inject] private ILogger<UploadEvidenceDialog> Logger { get; set; } = default!;
    [Inject] private DialogService DialogService { get; set; } = default!;
    #endregion

    #region Parameters
    [Parameter] public string HazardCode { get; set; } = default!;
    [Parameter] public string InvestigationCode { get; set; } = default!;
    #endregion

    #region State
    private bool IsUploading { get; set; } = false;
    private int UploadProgress { get; set; } = 0;
    public UploadFileModel Model { get; set; } = new();
    #endregion

    #region Dropdown Options
    private readonly List<DropdownOption> CategoryOptions = new()
    {
        new() { Value = "Evidence", Text = "General Evidence" },
        new() { Value = "Photo", Text = "Photographic Evidence" },
        new() { Value = "Document", Text = "Document/Report" },
        new() { Value = "Video", Text = "Video Evidence" },
        new() { Value = "Audio", Text = "Audio Recording" },
        new() { Value = "Witness", Text = "Witness Statement" },
        new() { Value = "Technical", Text = "Technical Data" },
        new() { Value = "Other", Text = "Other" }
    };
    #endregion

    #region Methods
    private bool CanUpload()
    {
        return Model.SelectedFiles?.Any() == true && 
               !string.IsNullOrWhiteSpace(Model.Description);
    }

    private async Task UploadFile(UploadFileModel model)
    {
        try
        {
            if (!CanUpload())
            {
                ShowErrorNotification("Please select a file and provide a description");
                return;
            }

            IsUploading = true;
            UploadProgress = 0;
            StateHasChanged();

            var file = model.SelectedFiles!.First();

            // Simulate upload progress
            for (int i = 0; i <= 100; i += 10)
            {
                UploadProgress = i;
                StateHasChanged();
                await Task.Delay(100); // Simulate upload time
            }

            // Create HazardFile entity
            var hazardFileResult = HazardFile.CreateForHazard(
                HazardCode,
                file.Name,
                file.Size.ToString(),
                GetFileType(file.Name),
                "CURRENT_USER"); // TODO: Get actual current user

            if (hazardFileResult.IsFailure)
            {
                ShowErrorNotification($"Failed to create file record: {hazardFileResult.Error?.Message}");
                return;
            }

            var hazardFile = hazardFileResult.Value;
            hazardFile.Description = model.Description;
            hazardFile.Category = model.Category ?? "Evidence";
            hazardFile.IsConfidential = model.IsConfidential;
            hazardFile.UploadedDate = DateTime.UtcNow;

            // TODO: Implement actual file upload to storage
            // For now, we'll just create the database record
            
            var createCommand = new CreateHazardFileCommand(hazardFile);
            var result = await Mediator.SendAsync(createCommand, CancellationToken.None);

            if (result.IsSuccess)
            {
                Logger.LogInformation("Evidence file uploaded successfully: {FileName} for hazard {HazardCode}", 
                    file.Name, HazardCode);
                DialogService.Close(true);
            }
            else
            {
                ShowErrorNotification($"Failed to save file record: {result.Error?.Message}");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error uploading evidence file");
            ShowErrorNotification("Error uploading file");
        }
        finally
        {
            IsUploading = false;
            UploadProgress = 0;
            StateHasChanged();
        }
    }

    private string GetFileType(string fileName)
    {
        var extension = Path.GetExtension(fileName)?.ToLowerInvariant();
        return extension switch
        {
            ".pdf" => "PDF",
            ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" => "Image",
            ".mp4" or ".avi" or ".mov" or ".wmv" => "Video",
            ".mp3" or ".wav" or ".m4a" => "Audio",
            ".doc" or ".docx" => "Document",
            ".xls" or ".xlsx" => "Spreadsheet",
            ".txt" => "Text",
            ".zip" or ".rar" => "Archive",
            _ => "Other"
        };
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

    #region Models
    public class UploadFileModel
    {
        public IEnumerable<IBrowserFile>? SelectedFiles { get; set; }
        public string? Description { get; set; }
        public string? Category { get; set; } = "Evidence";
        public bool IsConfidential { get; set; } = false;
    }

    public class DropdownOption
    {
        public object Value { get; set; } = default!;
        public string Text { get; set; } = "";
    }
    #endregion
}