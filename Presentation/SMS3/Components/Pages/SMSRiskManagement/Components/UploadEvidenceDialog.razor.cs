using SMS3.Components.Shared.UIHelpers;

namespace SMS3.Components.Pages.SMSRiskManagement.Components;

public partial class UploadEvidenceDialog : ComponentBase
{
    #region Injected Services
    [Inject] private IBaseMediator Mediator { get; set; } = default!;
    [Inject] private INotificationHelper NotificationHelper { get; set; } = default!;
    [Inject] private ILogger<UploadEvidenceDialog> Logger { get; set; } = default!;
    [Inject] private DialogService DialogService { get; set; } = default!;
    [Inject] private ISMSSessionService SessionService { get; set; } = default!;
    #endregion

    #region Parameters
    [Parameter] public string HazardCode { get; set; } = default!;
    [Parameter] public string InvestigationCode { get; set; } = default!;
    #endregion

    #region State Properties
    private bool IsUploading { get; set; } = false;
    private int UploadProgress { get; set; } = 0;
    private int FileProgress { get; set; } = 0;
    private int CurrentFileIndex { get; set; } = 0;
    private string CurrentFileName { get; set; } = string.Empty;
    private string CurrentUploadStatus { get; set; } = string.Empty;
    private bool ShowConfidentialInfo { get; set; } = false;

    public UploadFileModel Model { get; set; } = new();
    public List<AttachedFile> AttachedFiles { get; set; } = new();
    #endregion

    #region Computed Properties
    public int DescriptionCharacterCount => Model.Description?.Length ?? 0;

    private long TotalSize => AttachedFiles.Sum(f => f.Size);

    private string GetTotalSizeDisplay() => FormatFileSize(TotalSize);

    private string GetFileTypeSummary()
    {
        var typeGroups = AttachedFiles
            .GroupBy(f => GetFileTypeCategory(f.FileName))
            .Select(g => $"{g.Count()} {g.Key}")
            .ToList();

        return typeGroups.Any() ? string.Join(", ", typeGroups) : "None";
    }
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

    #region Lifecycle Methods
    protected override Task OnInitializedAsync()
    {
        InitializeModel();
        return Task.CompletedTask;
    }

    protected override Task OnParametersSetAsync()
    {
        // React to parameter changes
        if (Model.SelectedFiles?.Any() == true && !AttachedFiles.Any())
        {
            return ProcessAttachedFiles();
        }
        return Task.CompletedTask;
    }
    #endregion

    #region Initialization
    private void InitializeModel()
    {
        var currentUser = SessionService.GetCurrentUserDisplayName() ?? "System User";

        Model = new UploadFileModel
        {
            Category = "Evidence",
            IsConfidential = false
        };
    }
    #endregion

    #region File Processing
    /// <summary>
    /// Handle InputFile change event - this will accumulate files properly
    /// </summary>
    public async Task OnInputFileChange(InputFileChangeEventArgs e)
    {
        var newFiles = e.GetMultipleFiles(10); // Allow up to 10 files at once
        Logger.LogInformation("?? OnInputFileChange called with {Count} new files", newFiles?.Count() ?? 0);

        if (newFiles?.Any() == true)
        {
            // Process files immediately to avoid the "file list may have changed" error
            var successfullyProcessedFiles = new List<AttachedFile>();
            var failedFiles = new List<string>();

            foreach (var newFile in newFiles)
            {
                try
                {
                    // Check for duplicate first (before processing)
                    var isDuplicate = AttachedFiles.Any(existing =>
                        existing.FileName.Equals(newFile.Name, StringComparison.OrdinalIgnoreCase) &&
                        existing.Size == newFile.Size);

                    if (isDuplicate)
                    {
                        Logger.LogInformation("?? Skipped duplicate file: {FileName}", newFile.Name);
                        continue;
                    }

                    // Check file size (50MB limit)
                    if (newFile.Size > 52428800)
                    {
                        Logger.LogWarning("? File {FileName} exceeds 50MB limit", newFile.Name);
                        await NotificationHelper.ShowWarningAsync($"File '{newFile.Name}' exceeds 50MB limit and will be skipped");
                        failedFiles.Add(newFile.Name);
                        continue;
                    }

                    // Read file data immediately to avoid JavaScript interop issues
                    byte[] fileData;
                    using (var stream = newFile.OpenReadStream(maxAllowedSize: 52428800))
                    using (var memoryStream = new MemoryStream())
                    {
                        await stream.CopyToAsync(memoryStream);
                        fileData = memoryStream.ToArray();
                    }

                    // Create the attached file object with cached data
                    var attachedFile = new AttachedFile
                    {
                        FileName = newFile.Name,
                        ContentType = newFile.ContentType ?? "application/octet-stream",
                        Size = newFile.Size,
                        Data = fileData,
                        SizeDisplay = FormatFileSize(newFile.Size)
                    };

                    successfullyProcessedFiles.Add(attachedFile);
                    Logger.LogInformation("? Successfully processed file: {FileName} ({Size} bytes)", newFile.Name, newFile.Size);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "? Error processing file: {FileName}", newFile.Name);
                    failedFiles.Add(newFile.Name);
                    await NotificationHelper.ShowErrorAsync($"Error processing file '{newFile.Name}': {ex.Message}");
                }
            }

            // Add successfully processed files to the collection
            if (successfullyProcessedFiles.Any())
            {
                AttachedFiles.AddRange(successfullyProcessedFiles);
            }

            // Show notification about results
            if (successfullyProcessedFiles.Any() && failedFiles.Any())
            {
                await NotificationHelper.ShowWarningAsync($"Added {successfullyProcessedFiles.Count} file(s). Failed to process {failedFiles.Count} file(s). Total: {AttachedFiles.Count} files queued.");
            }
            else if (successfullyProcessedFiles.Any())
            {
                await NotificationHelper.ShowSuccessAsync($"Added {successfullyProcessedFiles.Count} file(s) to the queue. Total: {AttachedFiles.Count} files ready for upload.");
            }
            else if (failedFiles.Any())
            {
                await NotificationHelper.ShowErrorAsync($"Failed to process {failedFiles.Count} file(s). This may be due to file size limits or browser restrictions.");
            }

            Logger.LogInformation("?? File processing completed: {Success} successful, {Failed} failed. Total queued: {Total}",
                successfullyProcessedFiles.Count, failedFiles.Count, AttachedFiles.Count);
        }
        else
        {
            Logger.LogInformation("?? No files provided to OnInputFileChange");
        }

        StateHasChanged();
    }

    /// <summary>
    /// Handle file selection with proper event callback signature
    /// </summary>
    public async Task OnFilesSelected(IReadOnlyList<IBrowserFile> files)
    {
        Model.SelectedFiles = files;
        await ProcessAttachedFiles();
        StateHasChanged();
    }

    /// <summary>
    /// Wrapper method for RadzenFileInput Change event
    /// </summary>
    public async Task OnFilesSelectedWrapper(object files)
    {
        if (files is IReadOnlyList<IBrowserFile> browserFiles)
        {
            await OnFilesSelected(browserFiles);
        }
    }

    private async Task ProcessAttachedFiles()
    {
        // This method is now redundant since we process files directly in OnInputFileChange
        // But keeping it for backward compatibility with any existing calls
        Logger.LogInformation("ProcessAttachedFiles called - files are now processed immediately in OnInputFileChange");
    }
    #endregion

    #region Validation
    private bool CanUpload()
    {
        return AttachedFiles.Any() &&
               !string.IsNullOrWhiteSpace(Model.Description) &&
               DescriptionCharacterCount <= 1000;
    }

    private string GetValidationMessage()
    {
        if (!AttachedFiles.Any()) return "Please select at least one file";
        if (string.IsNullOrWhiteSpace(Model.Description)) return "Evidence description is required";
        if (DescriptionCharacterCount > 1000) return "Description exceeds character limit";

        return "Ready to upload";
    }
    #endregion

    #region Upload Processing
    private async Task UploadFiles(UploadFileModel model)
    {
        try
        {
            if (!CanUpload())
            {
                await NotificationHelper.ShowErrorAsync(GetValidationMessage());
                return;
            }

            IsUploading = true;
            UploadProgress = 0;
            CurrentUploadStatus = "Preparing upload...";
            StateHasChanged();

            Logger.LogInformation("?? Starting upload of {Count} evidence files for Hazard: {HazardCode}",
                AttachedFiles.Count, HazardCode);

            var uploadedFileIds = new List<string>();
            var totalFiles = AttachedFiles.Count;
            var currentUser = SessionService.GetCurrentUserDisplayName() ?? "Unknown User";

            for (int i = 0; i < totalFiles; i++)
            {
                var file = AttachedFiles[i];
                CurrentFileIndex = i;
                CurrentFileName = file.FileName;
                CurrentUploadStatus = $"Uploading {file.FileName}...";
                FileProgress = 0;
                StateHasChanged();

                try
                {
                    // Simulate file processing progress
                    for (int progress = 0; progress <= 100; progress += 20)
                    {
                        FileProgress = progress;
                        UploadProgress = (int)((i * 100 + progress) / (double)totalFiles);
                        StateHasChanged();
                        await Task.Delay(50); // Simulate processing time
                    }

                    // Generate unique file code
                    var fileCode = "HF-0000";

                    // Create HazardFile entity using only existing properties
                    var hazardFile = new HazardFile(new HazardFileID(fileCode))
                    {
                        Code = fileCode,
                        HazardCode = HazardCode,
                        ReportCode = string.Empty,
                        FileName = file.FileName,
                        FileType = GetFileTypeFromExtension(file.FileName),
                        ContentType = file.ContentType ?? "application/octet-stream",
                        FileSizeBytes = file.Size,
                        FileSize = FormatFileSize(file.Size),
                        StorageType = "Database",
                        FileData = file.Data,
                        UploadedBy = currentUser,
                        UploadedDate = DateTime.UtcNow,
                        IsActive = true,
                        IsConfidential = Model.IsConfidential,
                        Description = Model.Description,
                        Category = Model.Category ?? "Evidence"
                    };

                    // Send CreateHazardFileCommand
                    Logger.LogInformation("?? Creating HazardFile: {FileName} with Code: {FileCode} for Evidence",
                        file.FileName, fileCode);

                    var createCommand = new CreateHazardFileCommand(hazardFile);
                    var result = await Mediator.SendAsync(createCommand, CancellationToken.None);

                    if (result.IsSuccess)
                    {
                        var createdFileId = result.Value.Code;
                        uploadedFileIds.Add(createdFileId);

                        Logger.LogInformation("? Successfully created evidence file: {FileName} with ID: {FileId}",
                            file.FileName, createdFileId);
                    }
                    else
                    {
                        Logger.LogError("? Failed to create evidence file: {FileName}. Error: {Error}",
                            file.FileName, result.Error?.Message);

                        await NotificationHelper.ShowErrorAsync($"Failed to upload '{file.FileName}': {result.Error?.Message}");
                    }
                }
                catch (Exception fileEx)
                {
                    Logger.LogError(fileEx, "? Exception uploading evidence file: {FileName}", file.FileName);
                    await NotificationHelper.ShowErrorAsync($"Error uploading '{file.FileName}': {fileEx.Message}");
                }

                // Update overall progress
                UploadProgress = (int)(((i + 1) * 100.0) / totalFiles);
                StateHasChanged();
            }

            // Final status update
            CurrentUploadStatus = "Upload completed!";
            CurrentFileName = string.Empty;
            UploadProgress = 100;
            StateHasChanged();

            // Show completion message
            if (uploadedFileIds.Count == totalFiles)
            {
                Logger.LogInformation("? All evidence files uploaded successfully: {SuccessCount}/{TotalCount} files",
                    uploadedFileIds.Count, totalFiles);

                await NotificationHelper.ShowSuccessAsync($"Successfully uploaded {uploadedFileIds.Count} evidence file(s)");

                // Close dialog with success
                await Task.Delay(1000); // Brief delay to show completion
                DialogService.Close(true);
            }
            else
            {
                var failedCount = totalFiles - uploadedFileIds.Count;
                Logger.LogWarning("?? Partial upload success: {SuccessCount}/{TotalCount} files uploaded, {FailedCount} failed",
                    uploadedFileIds.Count, totalFiles, failedCount);

                await NotificationHelper.ShowWarningAsync($"Uploaded {uploadedFileIds.Count} of {totalFiles} files. {failedCount} file(s) failed.");

                if (uploadedFileIds.Any())
                {
                    // Close dialog as we had some success
                    await Task.Delay(1500);
                    DialogService.Close(true);
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "? Critical error during evidence upload process");
            await NotificationHelper.ShowErrorAsync("Critical error during upload process. Please try again.");
        }
        finally
        {
            IsUploading = false;
            UploadProgress = 0;
            FileProgress = 0;
            CurrentFileName = string.Empty;
            CurrentUploadStatus = string.Empty;
            CurrentFileIndex = 0;
            StateHasChanged();
        }
    }

    private string GetUploadButtonText()
    {
        if (IsUploading) return $"Uploading... ({UploadProgress}%)";
        if (!AttachedFiles.Any()) return "Upload Evidence";
        return $"Upload {AttachedFiles.Count} File(s)";
    }
    #endregion

    #region File Type Helpers
    private string GetFileTypeFromExtension(string fileName)
    {
        var extension = Path.GetExtension(fileName)?.ToLowerInvariant();
        return extension switch
        {
            ".pdf" => "PDF",
            ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" or ".webp" => "Image",
            ".mp4" or ".avi" or ".mov" or ".wmv" or ".flv" or ".webm" => "Video",
            ".mp3" or ".wav" or ".m4a" or ".aac" or ".ogg" => "Audio",
            ".doc" or ".docx" => "Document",
            ".xls" or ".xlsx" => "Spreadsheet",
            ".txt" or ".rtf" => "Text",
            ".zip" or ".rar" or ".7z" => "Archive",
            _ => "Other"
        };
    }

    private string GetFileTypeCategory(string fileName)
    {
        return GetFileTypeFromExtension(fileName).ToLowerInvariant() switch
        {
            "image" => "images",
            "video" => "videos",
            "audio" => "audio files",
            "pdf" => "PDFs",
            "document" => "documents",
            "spreadsheet" => "spreadsheets",
            "text" => "text files",
            "archive" => "archives",
            _ => "other files"
        };
    }

    private string GetFileIcon(string fileName)
    {
        return GetFileTypeFromExtension(fileName) switch
        {
            "PDF" => "picture_as_pdf",
            "Image" => "image",
            "Video" => "videocam",
            "Audio" => "audiotrack",
            "Document" => "description",
            "Spreadsheet" => "grid_on",
            "Text" => "text_snippet",
            "Archive" => "archive",
            _ => "insert_drive_file"
        };
    }

    private string GetFileIconColor(string fileName)
    {
        return GetFileTypeFromExtension(fileName) switch
        {
            "PDF" => "#d32f2f",
            "Image" => "#388e3c",
            "Video" => "#1976d2",
            "Audio" => "#f57c00",
            "Document" => "#7b1fa2",
            "Spreadsheet" => "#388e3c",
            "Text" => "#616161",
            "Archive" => "#795548",
            _ => "#757575"
        };
    }

    private string GetFileTypeDisplay(string fileName)
    {
        return GetFileTypeFromExtension(fileName);
    }

    private string GetFileStatusBadge(string fileName)
    {
        return "Ready";
    }

    private BadgeStyle GetFileStatusBadgeStyle(string fileName)
    {
        return BadgeStyle.Success;
    }
    #endregion

    #region UI Event Handlers
    private void ToggleConfidentialInfo()
    {
        ShowConfidentialInfo = !ShowConfidentialInfo;
        StateHasChanged();
    }
    #endregion

    #region Utility Methods
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

    #region Data Models
    public class UploadFileModel
    {
        public IReadOnlyList<IBrowserFile>? SelectedFiles { get; set; }
        public string? Description { get; set; }
        public string? Category { get; set; } = "Evidence";
        public bool IsConfidential { get; set; } = false;
    }

  

    public class AttachedFile
    {
        public string FileName { get; set; } = string.Empty;
        public long FileSizeBytes { get; set; }
        public string SizeDisplay { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public byte[] Data { get; set; } = Array.Empty<byte>();
        public long Size { get; set; }
    }
    #endregion

    #region File Management Methods

    /// <summary>
    /// Remove a specific file from the queue
    /// </summary>
    public async Task RemoveFile(int index)
    {
        if (index >= 0 && index < AttachedFiles.Count)
        {
            var fileToRemove = AttachedFiles[index];
            AttachedFiles.RemoveAt(index);

            Logger.LogInformation("Removed file: {FileName} from upload queue", fileToRemove.FileName);
            StateHasChanged();
        }
    }

    /// <summary>
    /// Clear all queued files
    /// </summary>
    public async Task ClearAllFiles()
    {
        AttachedFiles.Clear();
        Model.SelectedFiles = null;

        Logger.LogInformation("Cleared all files from upload queue");
        StateHasChanged();
    }

    #endregion
}