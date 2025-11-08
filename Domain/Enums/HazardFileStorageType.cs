using System.Reflection;
using SMS_Domain.Common;

namespace SMS_Domain.Enums;

/// <summary>
/// Hazard file storage type enumeration for flexible file storage strategies
/// Provides rich behavior and metadata for storage management
/// </summary>
public abstract class HazardFileStorageType : BaseEnum<HazardFileStorageType>
{
    protected HazardFileStorageType(string value, string name, string description, bool supportsLargeFiles, bool requiresExternalDependency, int performanceRating) : base(value, name)
    {
        Description = description;
        SupportsLargeFiles = supportsLargeFiles;
        RequiresExternalDependency = requiresExternalDependency;
        PerformanceRating = performanceRating;
    }

    public string Description { get; }
    public bool SupportsLargeFiles { get; }
    public bool RequiresExternalDependency { get; }
    public int PerformanceRating { get; } // 1-5 scale

    #region Storage Type Values

    /// <summary>Database storage for secure, small files</summary>
    public static readonly HazardFileStorageType Database = new DatabaseStorageType();

    /// <summary>File system storage for most file types</summary>
    public static readonly HazardFileStorageType FileSystem = new FileSystemStorageType();

    /// <summary>Cloud storage for scalable, large file storage</summary>
    public static readonly HazardFileStorageType Cloud = new CloudStorageType();

    #endregion

    #region Implementations

    private sealed class DatabaseStorageType : HazardFileStorageType
    {
        public DatabaseStorageType() : base("DATABASE", "Database",
            "Files stored directly in database as binary data with high security and transactional integrity", false, false, 3)
        {
        }
    }

    private sealed class FileSystemStorageType : HazardFileStorageType
    {
        public FileSystemStorageType() : base("FILESYSTEM", "File System",
            "Files stored on local or network file system with good performance and moderate security", true, false, 4)
        {
        }
    }

    private sealed class CloudStorageType : HazardFileStorageType
    {
        public CloudStorageType() : base("CLOUD", "Cloud",
            "Files stored in cloud storage services with high scalability and availability", true, true, 5)
        {
        }
    }

    #endregion

    /// <summary>
    /// Gets all available storage type values
    /// </summary>
    public static IEnumerable<HazardFileStorageType> GetAllValues()
    {
        return typeof(HazardFileStorageType)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(f => f.FieldType == typeof(HazardFileStorageType))
            .Select(f => (HazardFileStorageType)f.GetValue(null)!)
            .Where(hst => hst != null);
    }

    /// <summary>
    /// Gets storage types that support large files
    /// </summary>
    public static IEnumerable<HazardFileStorageType> GetLargeFileStorageTypes()
    {
        return GetAllValues().Where(hst => hst.SupportsLargeFiles);
    }

    /// <summary>
    /// Gets storage types that don't require external dependencies
    /// </summary>
    public static IEnumerable<HazardFileStorageType> GetSelfContainedStorageTypes()
    {
        return GetAllValues().Where(hst => !hst.RequiresExternalDependency);
    }

    /// <summary>
    /// Recommends storage type based on file size and category
    /// </summary>
    public static HazardFileStorageType RecommendForFile(long fileSizeBytes, HazardFileCategory category)
    {
        // Large files (>50MB) should use cloud or file system
        if (fileSizeBytes > 50 * 1024 * 1024)
        {
            return Cloud;
        }

        // Sensitive files (Evidence, Reports) should use database for security
        if (category.IsSensitive)
        {
            return Database;
        }

        // Medium files (5-50MB) should use file system
        if (fileSizeBytes > 5 * 1024 * 1024)
        {
            return FileSystem;
        }

        // Small files can use database
        return Database;
    }

    /// <summary>
    /// Gets the maximum recommended file size for this storage type
    /// </summary>
    public long GetMaxRecommendedFileSize()
    {
        return this switch
        {
            var s when s == Database => 10 * 1024 * 1024,    // 10 MB
            var s when s == FileSystem => 500 * 1024 * 1024, // 500 MB
            var s when s == Cloud => long.MaxValue,          // No practical limit
            _ => 10 * 1024 * 1024
        };
    }

    /// <summary>
    /// Determines if this storage type is suitable for the file size
    /// </summary>
    public bool IsSuitableForFileSize(long fileSizeBytes)
    {
        return fileSizeBytes <= GetMaxRecommendedFileSize();
    }

    /// <summary>
    /// Gets the relative cost factor for this storage type
    /// </summary>
    public decimal GetCostFactor()
    {
        return this switch
        {
            var s when s == Database => 1.0m,     // Base cost
            var s when s == FileSystem => 0.5m,   // Lower cost
            var s when s == Cloud => 2.0m,        // Higher cost but scalable
            _ => 1.0m
        };
    }

    /// <summary>
    /// Determines if this storage type provides high availability
    /// </summary>
    public bool ProvidesHighAvailability => this == Cloud || this == Database;

    /// <summary>
    /// Determines if this storage type supports automatic backup
    /// </summary>
    public bool SupportsAutomaticBackup => this == Database || this == Cloud;

    /// <summary>
    /// Gets the security level for this storage type
    /// </summary>
    public string GetSecurityLevel()
    {
        return this switch
        {
            var s when s == Database => "High",
            var s when s == FileSystem => "Medium",
            var s when s == Cloud => "High",
            _ => "Medium"
        };
    }

    /// <summary>
    /// Gets configuration requirements for this storage type
    /// </summary>
    public List<string> GetConfigurationRequirements()
    {
        return this switch
        {
            var s when s == Database => new List<string> { "Database connection string", "VARBINARY(MAX) support" },
            var s when s == FileSystem => new List<string> { "File system path", "Appropriate permissions" },
            var s when s == Cloud => new List<string> { "Cloud provider credentials", "Storage container/bucket" },
            _ => new List<string>()
        };
    }
}