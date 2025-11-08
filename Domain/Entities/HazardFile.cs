using SMS_Shared.Common;

namespace SMS_Domain.Entities;

/// <summary>
/// HazardFile Domain Entity - Mission Critical
/// Represents supporting documentation files for hazards including photos, documents, videos
/// Supports multiple storage strategies (database, file system, cloud) for flexibility
/// </summary>
public sealed class HazardFile : BaseAuditableEntity
{
    // Private constructor for Entity Framework
    private HazardFile() : base(new HazardFileID(Guid.NewGuid().ToString()), "SYSTEM", DateTime.UtcNow) { }

    // Public constructor for domain usage
    public HazardFile(HazardFileID id) : base(id, "SYSTEM", DateTime.UtcNow) { }

    // Private constructor for creation with validation
    private HazardFile(HazardFileID id, string code, string hazardCode, string fileName, string fileType, long fileSizeBytes, string uploadedBy)
        : base(id, "SYSTEM", DateTime.UtcNow)
    {
        Code = code;
        HazardCode = hazardCode;
        FileName = fileName;
        FileType = fileType.ToLowerInvariant();
        FileSizeBytes = fileSizeBytes;
        UploadedBy = uploadedBy;
        UploadedDate = DateTime.UtcNow;
        StorageType = HazardFileStorageType.FileSystem; // Default to file system
        IsActive = true;
        IsConfidential = false;
        ContentType = DetermineContentType(fileType);
    }

    #region Core Properties

    public string Code { get; private set; } = string.Empty;
    public string HazardCode { get; private set; } = string.Empty; // FK to Hazard
    public string? ReportCode { get; private set; } // FK to Report (optional)
    public string FileName { get; private set; } = string.Empty;
    public string FileType { get; private set; } = string.Empty; // Extension: pdf, jpg, png, mp4, etc.
    public string ContentType { get; private set; } = string.Empty; // MIME type
    public long FileSizeBytes { get; private set; }

    #endregion

    #region Storage Properties

    public HazardFileStorageType StorageType { get; private set; } = HazardFileStorageType.FileSystem;
    public string? FilePath { get; private set; } // For file system storage
    public byte[]? FileData { get; private set; } // For database storage
    public string? FileHash { get; private set; } // SHA-256 hash for integrity

    #endregion

    #region Metadata Properties

    public string? Description { get; private set; }
    public HazardFileCategory? Category { get; private set; }
    public bool IsConfidential { get; private set; }
    public string? Tags { get; private set; } // Comma-separated searchable tags

    #endregion

    #region Upload Tracking

    public string UploadedBy { get; private set; } = string.Empty;
    public DateTime UploadedDate { get; private set; } = DateTime.UtcNow;

    #endregion

    #region Status Properties

    public bool IsActive { get; private set; } = true;
    public string? InactiveReason { get; private set; }
    public DateTime? InactiveDate { get; private set; }
    public string? InactiveBy { get; private set; }

    #endregion

    #region Factory Methods

    /// <summary>
    /// Create a new hazard file with file system storage
    /// </summary>
    public static Result<HazardFile> CreateForFileSystem(string hazardCode, string fileName, string fileType, 
        long fileSizeBytes, string filePath, string uploadedBy, string? reportCode = null)
    {
        var validationResult = ValidateCreateParameters(hazardCode, fileName, fileType, fileSizeBytes, uploadedBy);
        if (validationResult.IsFailure)
        {
            return Result<HazardFile>.Failure<HazardFile>(validationResult.Error);
        }

        var code = GenerateCode();
        var id = new HazardFileID(code);
        var file = new HazardFile(id, code, hazardCode, fileName, fileType, fileSizeBytes, uploadedBy)
        {
            ReportCode = reportCode,
            StorageType = HazardFileStorageType.FileSystem,
            FilePath = filePath
        };

        return Result<HazardFile>.Success(file);
    }

    /// <summary>
    /// Create a new hazard file with database storage
    /// </summary>
    public static Result<HazardFile> CreateForDatabase(string hazardCode, string fileName, string fileType, 
        byte[] fileData, string uploadedBy, string? reportCode = null)
    {
        if (fileData == null || fileData.Length == 0)
        {
            return Result<HazardFile>.Failure<HazardFile>(DomainErrors.HazardFileError.FileDataRequired);
        }

        var validationResult = ValidateCreateParameters(hazardCode, fileName, fileType, fileData.Length, uploadedBy);
        if (validationResult.IsFailure)
        {
            return Result<HazardFile>.Failure<HazardFile>(validationResult.Error);
        }

        var code = GenerateCode();
        var id = new HazardFileID(code);
        var file = new HazardFile(id, code, hazardCode, fileName, fileType, fileData.Length, uploadedBy)
        {
            ReportCode = reportCode,
            StorageType = HazardFileStorageType.Database,
            FileData = fileData,
            FileHash = ComputeFileHash(fileData)
        };

        return Result<HazardFile>.Success(file);
    }

    /// <summary>
    /// Create a new hazard file with cloud storage
    /// </summary>
    public static Result<HazardFile> CreateForCloud(string hazardCode, string fileName, string fileType, 
        long fileSizeBytes, string cloudPath, string uploadedBy, string? reportCode = null)
    {
        var validationResult = ValidateCreateParameters(hazardCode, fileName, fileType, fileSizeBytes, uploadedBy);
        if (validationResult.IsFailure)
        {
            return Result<HazardFile>.Failure<HazardFile>(validationResult.Error);
        }

        var code = GenerateCode();
        var id = new HazardFileID(code);
        var file = new HazardFile(id, code, hazardCode, fileName, fileType, fileSizeBytes, uploadedBy)
        {
            ReportCode = reportCode,
            StorageType = HazardFileStorageType.Cloud,
            FilePath = cloudPath
        };

        return Result<HazardFile>.Success(file);
    }

    #endregion

    #region Domain Behavior Methods

    /// <summary>
    /// Update file metadata
    /// </summary>
    public Result<bool> UpdateMetadata(string? description, HazardFileCategory? category, bool? isConfidential, string? tags)
    {
        if (!IsActive)
        {
            return Result<bool>.Failure<bool>(DomainErrors.HazardFileError.CannotModifyInactive);
        }

        Description = description;
        Category = category;
        if (isConfidential.HasValue)
        {
            IsConfidential = isConfidential.Value;
        }
        Tags = tags;
        UpdatedDate = DateTime.UtcNow;

        return Result<bool>.Success(true);
    }

    /// <summary>
    /// Update file name
    /// </summary>
    public Result<bool> UpdateFileName(string newFileName)
    {
        if (!IsActive)
        {
            return Result<bool>.Failure<bool>(DomainErrors.HazardFileError.CannotModifyInactive);
        }

        if (string.IsNullOrWhiteSpace(newFileName))
        {
            return Result<bool>.Failure<bool>(DomainErrors.HazardFileError.FileNameRequired);
        }

        FileName = newFileName.Trim();
        UpdatedDate = DateTime.UtcNow;

        return Result<bool>.Success(true);
    }

    /// <summary>
    /// Set file as confidential
    /// </summary>
    public Result<bool> SetConfidential(bool isConfidential, string? reason = null)
    {
        if (!IsActive)
        {
            return Result<bool>.Failure<bool>(DomainErrors.HazardFileError.CannotModifyInactive);
        }

        IsConfidential = isConfidential;
        
        if (!string.IsNullOrWhiteSpace(reason))
        {
            var currentTags = Tags ?? "";
            Tags = string.IsNullOrWhiteSpace(currentTags) ? $"confidential:{reason}" : $"{currentTags},confidential:{reason}";
        }

        UpdatedDate = DateTime.UtcNow;
        return Result<bool>.Success(true);
    }

    /// <summary>
    /// Deactivate the file (soft delete)
    /// </summary>
    public Result<bool> Deactivate(string reason, string deactivatedBy)
    {
        if (!IsActive)
        {
            return Result<bool>.Failure<bool>(DomainErrors.HazardFileError.AlreadyInactive);
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            return Result<bool>.Failure<bool>(DomainErrors.HazardFileError.DeactivationReasonRequired);
        }

        if (string.IsNullOrWhiteSpace(deactivatedBy))
        {
            return Result<bool>.Failure<bool>(DomainErrors.HazardFileError.DeactivatedByRequired);
        }

        IsActive = false;
        InactiveReason = reason;
        InactiveDate = DateTime.UtcNow;
        InactiveBy = deactivatedBy;
        UpdatedDate = DateTime.UtcNow;

        return Result<bool>.Success(true);
    }

    /// <summary>
    /// Reactivate the file
    /// </summary>
    public Result<bool> Reactivate()
    {
        if (IsActive)
        {
            return Result<bool>.Failure<bool>(DomainErrors.HazardFileError.AlreadyActive);
        }

        IsActive = true;
        InactiveReason = null;
        InactiveDate = null;
        InactiveBy = null;
        UpdatedDate = DateTime.UtcNow;

        return Result<bool>.Success(true);
    }

    /// <summary>
    /// Update file hash (for integrity verification)
    /// </summary>
    public Result<bool> UpdateFileHash(string fileHash)
    {
        if (string.IsNullOrWhiteSpace(fileHash))
        {
            return Result<bool>.Failure<bool>(DomainErrors.HazardFileError.InvalidFileHash);
        }

        FileHash = fileHash;
        UpdatedDate = DateTime.UtcNow;

        return Result<bool>.Success(true);
    }

    #endregion

    #region Query Methods

    /// <summary>
    /// Check if file is an image
    /// </summary>
    public bool IsImage()
    {
        var imageTypes = new[] { "jpg", "jpeg", "png", "gif", "bmp", "tiff", "webp" };
        return imageTypes.Contains(FileType.ToLowerInvariant());
    }

    /// <summary>
    /// Check if file is a video
    /// </summary>
    public bool IsVideo()
    {
        var videoTypes = new[] { "mp4", "avi", "mov", "wmv", "flv", "webm", "mkv" };
        return videoTypes.Contains(FileType.ToLowerInvariant());
    }

    /// <summary>
    /// Check if file is a document
    /// </summary>
    public bool IsDocument()
    {
        var documentTypes = new[] { "pdf", "doc", "docx", "xls", "xlsx", "ppt", "pptx", "txt", "rtf" };
        return documentTypes.Contains(FileType.ToLowerInvariant());
    }

    /// <summary>
    /// Get file size in human readable format
    /// </summary>
    public string GetFormattedFileSize()
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = FileSizeBytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }

    /// <summary>
    /// Check if file exceeds size limit
    /// </summary>
    public bool ExceedsSizeLimit(long maxSizeBytes)
    {
        return FileSizeBytes > maxSizeBytes;
    }

    /// <summary>
    /// Get display category
    /// </summary>
    public string GetDisplayCategory()
    {
        if (Category.HasValue)
            return Category.Value.ToString();

        if (IsImage()) return "Photo";
        if (IsVideo()) return "Video";
        if (IsDocument()) return "Document";
        return "Other";
    }

    #endregion

    #region Private Helper Methods

    private static Result<bool> ValidateCreateParameters(string hazardCode, string fileName, string fileType, long fileSizeBytes, string uploadedBy)
    {
        if (string.IsNullOrWhiteSpace(hazardCode))
        {
            return Result<bool>.Failure<bool>(DomainErrors.HazardFileError.HazardCodeRequired);
        }

        if (string.IsNullOrWhiteSpace(fileName))
        {
            return Result<bool>.Failure<bool>(DomainErrors.HazardFileError.FileNameRequired);
        }

        if (string.IsNullOrWhiteSpace(fileType))
        {
            return Result<bool>.Failure<bool>(DomainErrors.HazardFileError.FileTypeRequired);
        }

        if (fileSizeBytes <= 0)
        {
            return Result<bool>.Failure<bool>(DomainErrors.HazardFileError.InvalidFileSize);
        }

        if (string.IsNullOrWhiteSpace(uploadedBy))
        {
            return Result<bool>.Failure<bool>(DomainErrors.HazardFileError.UploadedByRequired);
        }

        return Result<bool>.Success(true);
    }

    private static string GenerateCode()
    {
        return $"HF-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}";
    }

    private static string DetermineContentType(string fileType)
    {
        return fileType.ToLowerInvariant() switch
        {
            "pdf" => "application/pdf",
            "doc" => "application/msword",
            "docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            "xls" => "application/vnd.ms-excel",
            "xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "jpg" or "jpeg" => "image/jpeg",
            "png" => "image/png",
            "gif" => "image/gif",
            "bmp" => "image/bmp",
            "tiff" => "image/tiff",
            "mp4" => "video/mp4",
            "avi" => "video/avi",
            "mov" => "video/quicktime",
            "wmv" => "video/x-ms-wmv",
            "txt" => "text/plain",
            _ => "application/octet-stream"
        };
    }

    private static string ComputeFileHash(byte[] fileData)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var hashBytes = sha256.ComputeHash(fileData);
        return Convert.ToHexString(hashBytes);
    }

    #endregion
}

/// <summary>
/// HazardFile ID Value Object
/// </summary>
public sealed class HazardFileID : BaseID<string>
{
    public HazardFileID(string id) : base(id) { }
}