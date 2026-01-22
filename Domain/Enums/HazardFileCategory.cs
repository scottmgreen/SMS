namespace SMS_Domain.Enums;

/// <summary>
/// Hazard file category enumeration for file classification and organization
/// Provides rich behavior and metadata for file management
/// </summary>
public abstract class HazardFileCategory : BaseEnum<HazardFileCategory>
{
    protected HazardFileCategory(string value, string name, string description, string iconClass, bool requiresApproval, int displayOrder) : base(value, name)
    {
        Description = description;
        IconClass = iconClass;
        RequiresApproval = requiresApproval;
        DisplayOrder = displayOrder;
    }

    public string Description { get; }
    public string IconClass { get; }
    public bool RequiresApproval { get; }
    public int DisplayOrder { get; }

    #region Hazard File Category Types

    /// <summary>Photo/image files for visual documentation</summary>
    public static readonly HazardFileCategory Photo = new PhotoCategory();

    /// <summary>Document files for formal documentation</summary>
    public static readonly HazardFileCategory Document = new DocumentCategory();

    /// <summary>Video files for dynamic documentation</summary>
    public static readonly HazardFileCategory Video = new VideoCategory();

    /// <summary>Audio files for recorded information</summary>
    public static readonly HazardFileCategory Audio = new AudioCategory();

    /// <summary>Evidence files for investigation support</summary>
    public static readonly HazardFileCategory Evidence = new EvidenceCategory();

    /// <summary>Report attachments and official documents</summary>
    public static readonly HazardFileCategory Report = new ReportCategory();

    /// <summary>Other miscellaneous file types</summary>
    public static readonly HazardFileCategory Other = new OtherCategory();

    #endregion

    #region Implementations

    private sealed class PhotoCategory : HazardFileCategory
    {
        public PhotoCategory() : base("PHOTO", "Photo",
            "Image files for visual documentation of hazards and conditions", "fas fa-image", false, 1)
        {
        }
    }

    private sealed class DocumentCategory : HazardFileCategory
    {
        public DocumentCategory() : base("DOCUMENT", "Document",
            "Document files including PDFs, Word documents, and spreadsheets", "fas fa-file-alt", false, 2)
        {
        }
    }

    private sealed class VideoCategory : HazardFileCategory
    {
        public VideoCategory() : base("VIDEO", "Video",
            "Video files for dynamic documentation and training materials", "fas fa-video", false, 3)
        {
        }
    }

    private sealed class AudioCategory : HazardFileCategory
    {
        public AudioCategory() : base("AUDIO", "Audio",
            "Audio files for recorded interviews, instructions, or documentation", "fas fa-volume-up", false, 4)
        {
        }
    }

    private sealed class EvidenceCategory : HazardFileCategory
    {
        public EvidenceCategory() : base("EVIDENCE", "Evidence",
            "Evidence files supporting investigations and formal proceedings", "fas fa-search", true, 5)
        {
        }
    }

    private sealed class ReportCategory : HazardFileCategory
    {
        public ReportCategory() : base("REPORT", "Report",
            "Official report attachments and formal documentation", "fas fa-file-pdf", true, 6)
        {
        }
    }

    private sealed class OtherCategory : HazardFileCategory
    {
        public OtherCategory() : base("OTHER", "Other",
            "Other file types not covered by standard categories", "fas fa-file", false, 7)
        {
        }
    }

    #endregion

    /// <summary>
    /// Gets all available file category values
    /// </summary>
    public static IEnumerable<HazardFileCategory> GetAllValues()
    {
        return typeof(HazardFileCategory)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(f => f.FieldType == typeof(HazardFileCategory))
            .Select(f => (HazardFileCategory)f.GetValue(null)!)
            .Where(hfc => hfc != null)
            .OrderBy(hfc => hfc.DisplayOrder);
    }

    /// <summary>
    /// Gets categories that require approval
    /// </summary>
    public static IEnumerable<HazardFileCategory> GetApprovalRequiredCategories()
    {
        return GetAllValues().Where(hfc => hfc.RequiresApproval);
    }

    /// <summary>
    /// Gets category based on file extension
    /// </summary>
    public static HazardFileCategory FromFileExtension(string extension)
    {
        var ext = extension.ToLowerInvariant().TrimStart('.');

        return ext switch
        {
            "jpg" or "jpeg" or "png" or "gif" or "bmp" or "tiff" or "webp" => Photo,
            "pdf" or "doc" or "docx" or "xls" or "xlsx" or "ppt" or "pptx" or "txt" or "rtf" => Document,
            "mp4" or "avi" or "mov" or "wmv" or "flv" or "webm" or "mkv" => Video,
            "mp3" or "wav" or "aac" or "flac" or "ogg" => Audio,
            _ => Other
        };
    }

    /// <summary>
    /// Determines if this category supports the file type
    /// </summary>
    public bool SupportsFileType(string fileExtension)
    {
        var category = FromFileExtension(fileExtension);
        return category == this || (category == Other && this == Other);
    }

    /// <summary>
    /// Gets the maximum file size allowed for this category (in bytes)
    /// </summary>
    public long GetMaxFileSizeBytes()
    {
        return this switch
        {
            var c when c == Photo => 10 * 1024 * 1024,      // 10 MB
            var c when c == Document => 50 * 1024 * 1024,   // 50 MB
            var c when c == Video => 500 * 1024 * 1024,     // 500 MB
            var c when c == Audio => 100 * 1024 * 1024,     // 100 MB
            var c when c == Evidence => 100 * 1024 * 1024,  // 100 MB
            var c when c == Report => 50 * 1024 * 1024,     // 50 MB
            _ => 10 * 1024 * 1024                            // 10 MB default
        };
    }

    /// <summary>
    /// Gets the recommended storage type for this category
    /// </summary>
    public string GetRecommendedStorageType()
    {
        return this switch
        {
            var c when c == Photo => "FileSystem",
            var c when c == Document => "FileSystem",
            var c when c == Video => "Cloud",
            var c when c == Audio => "FileSystem",
            var c when c == Evidence => "Database", // High security
            var c when c == Report => "Database",   // High security
            _ => "FileSystem"
        };
    }

    /// <summary>
    /// Determines if files in this category should be indexed for search
    /// </summary>
    public bool ShouldIndexForSearch => this == Document || this == Report;

    /// <summary>
    /// Determines if files in this category contain sensitive information
    /// </summary>
    public bool IsSensitive => this == Evidence || this == Report;

    /// <summary>
    /// Gets the MIME type pattern for this category
    /// </summary>
    public string GetMimeTypePattern()
    {
        return this switch
        {
            var c when c == Photo => "image/*",
            var c when c == Document => "application/*,text/*",
            var c when c == Video => "video/*",
            var c when c == Audio => "audio/*",
            var c when c == Evidence => "*/*",
            var c when c == Report => "application/*",
            _ => "*/*"
        };
    }
}