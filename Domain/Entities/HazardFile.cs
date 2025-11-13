using SMS_Shared.Common;

namespace SMS_Domain.Entities;

/// <summary>
/// HazardFile Domain Entity
/// Represents supporting documentation files for hazards including photos, documents, videos
/// </summary>
public sealed class HazardFile : BaseAuditableEntity
{
    public HazardFile(HazardFileID id) : base(id, "SYSTEM", DateTime.UtcNow) { }

    #region Properties

    public string Code { get; set; } = string.Empty;
    public string HazardCode { get; set; } = string.Empty;
    public string? ReportCode { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string StorageType { get; set; } = "FileSystem";
    public string? FilePath { get; set; }
    public byte[]? FileData { get; set; }
    public string? FileHash { get; set; }
    public string? Description { get; set; }
    public string? Category { get; set; }
    public bool IsConfidential { get; set; }
    public string? Tags { get; set; }
    public string UploadedBy { get; set; } = string.Empty;
    public DateTime UploadedDate { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    public string? InactiveReason { get; set; }
    public DateTime? InactiveDate { get; set; }
    public string? InactiveBy { get; set; }

    #endregion
}
