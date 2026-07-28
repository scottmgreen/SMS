//-----------------------------------------------------------------------
// <copyright file="HazardFile.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: SMS hazard entity representing hazardfile for safety management processes.
//                  Domain entity implementing business rules and invariants
//                  following Domain-Driven Design principles.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Domain.Entities;

/// <summary>
/// Hazard File entity for managing file attachments and evidence
/// </summary>
public class HazardFile : BaseAuditableEntity
{
    // Private constructor for Entity Framework
    

    public HazardFile(HazardFileID id) : base(id, string.Empty, DateTime.UtcNow) { }

    #region Properties

    public string Code { get; set; } = string.Empty;
    public string HazardCode { get; set; } = string.Empty;
    public string? ReportCode { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public string? FileSize { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string StorageType { get; set; } = "FileSystem";
    public string? FilePath { get; set; }
    public byte[]? FileData { get; set; }
    public string? Description { get; set; }
    public string? Category { get; set; }
    public bool IsConfidential { get; set; }
    public string? Tags { get; set; }
    public string UploadedBy { get; set; } = string.Empty;
    public DateTime? UploadedDate { get; set; }
    public bool IsActive { get; set; } = true;
    public string? InactiveReason { get; set; }
    public DateTime? InactiveDate { get; set; }
    public string? InactiveBy { get; set; }

    #endregion

    #region Factory Methods

    /// <summary>
    /// Create a new HazardFile for a specific hazard
    /// </summary>
    public static Result<HazardFile> CreateForHazard(string hazardCode, string fileName, string? fileSize, string? fileType, string uploadedBy)
    {
        if (string.IsNullOrWhiteSpace(hazardCode))
        {
            return Result.Failure<HazardFile>(DomainErrors.HazardError.CodeRequired);
        }

        if (string.IsNullOrWhiteSpace(fileName))
        {
            return Result.Failure<HazardFile>(DomainErrors.HazardFileError.FileNameRequired);
        }

        if (string.IsNullOrWhiteSpace(uploadedBy))
        {
            return Result.Failure<HazardFile>(DomainErrors.HazardFileError.UploaderRequired);
        }

        var code = GenerateCode();
        var id = new HazardFileID(code);

        var hazardFile = new HazardFile(id)
        {
            Code = code,
            HazardCode = hazardCode,
            FileName = fileName,
            FileSize = fileSize,
            FileType = fileType ?? GetFileTypeFromName(fileName),
            UploadedBy = uploadedBy,
            UploadedDate = DateTime.UtcNow,
            IsActive = true
        };

        return Result.Success(hazardFile);
    }

    #endregion

    #region Private Helper Methods

    private static string GenerateCode()
    {
        return $"HF-0000";
    }

    private static string GetFileTypeFromName(string fileName)
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

    #endregion
}

