//-----------------------------------------------------------------------
// <copyright file="SMSAuditEvidenceStatistics.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Domain model representing statistical data and metrics for SMS smsauditevidence reporting.
//                  Domain model representing complex data structures
//                  for business reporting and analytics.
// </copyright>
//-----------------------------------------------------------------------

namespace Domain.Models;

/// <summary>
/// Statistical data model for SMS Audit Evidence analysis and reporting
/// Provides comprehensive metrics on audit evidence collection, verification, and management
/// </summary>
public class SMSAuditEvidenceStatistics
{
    /// <summary>
    /// Date range for the statistics
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// End date for the statistics
    /// </summary>
    public DateTime EndDate { get; set; }

    /// <summary>
    /// Total number of evidence items collected
    /// </summary>
    public int TotalEvidenceItems { get; set; }

    /// <summary>
    /// Evidence by type breakdown
    /// </summary>
    public int DocumentEvidence { get; set; }
    public int PhotoEvidence { get; set; }
    public int VideoEvidence { get; set; }
    public int InterviewEvidence { get; set; }
    public int DigitalEvidence { get; set; }
    public int PhysicalEvidence { get; set; }

    /// <summary>
    /// Evidence verification status
    /// </summary>
    public int VerifiedEvidence { get; set; }
    public int UnverifiedEvidence { get; set; }
    public int PendingVerification { get; set; }

    /// <summary>
    /// Evidence archival status
    /// </summary>
    public int ArchivedEvidence { get; set; }
    public int ActiveEvidence { get; set; }
    public int PendingArchival { get; set; }

    /// <summary>
    /// Storage and file metrics
    /// </summary>
    public long TotalStorageSizeBytes { get; set; }
    public double AverageFileSizeMB { get; set; }
    public int LargestFileSizeMB { get; set; }
    public int SmallestFileSizeKB { get; set; }

    /// <summary>
    /// Evidence by confidentiality level
    /// </summary>
    public Dictionary<string, int> EvidenceByConfidentialityLevel { get; set; } = new();

    /// <summary>
    /// Evidence by audit type
    /// </summary>
    public Dictionary<string, int> EvidenceByAuditType { get; set; } = new();

    /// <summary>
    /// Evidence by department
    /// </summary>
    public Dictionary<string, int> EvidenceByDepartment { get; set; } = new();

    /// <summary>
    /// Evidence by collector (top 10)
    /// </summary>
    public Dictionary<string, int> EvidenceByCollector { get; set; } = new();

    /// <summary>
    /// Evidence collection timeline
    /// </summary>
    public List<EvidenceCollectionTrendData> CollectionTrends { get; set; } = new();

    /// <summary>
    /// Verification timeline
    /// </summary>
    public List<EvidenceVerificationTrendData> VerificationTrends { get; set; } = new();

    /// <summary>
    /// Storage usage over time
    /// </summary>
    public List<StorageUsageTrendData> StorageTrends { get; set; } = new();

    /// <summary>
    /// Evidence quality metrics
    /// </summary>
    public EvidenceQualityMetrics QualityMetrics { get; set; } = new();

    /// <summary>
    /// Retention and compliance metrics
    /// </summary>
    public RetentionComplianceMetrics RetentionMetrics { get; set; } = new();

    /// <summary>
    /// Evidence source analysis
    /// </summary>
    public Dictionary<string, int> EvidenceBySource { get; set; } = new();

    /// <summary>
    /// Evidence linked to findings
    /// </summary>
    public int EvidenceLinkedToFindings { get; set; }
    public int UnlinkedEvidence { get; set; }
    public double EvidenceLinkageRate { get; set; }
}

/// <summary>
/// Evidence collection trend data
/// </summary>
public class EvidenceCollectionTrendData
{
    public string Period { get; set; } = string.Empty;
    public int TotalCollected { get; set; }
    public int DocumentsCollected { get; set; }
    public int PhotosCollected { get; set; }
    public int VideosCollected { get; set; }
    public int InterviewsCollected { get; set; }
    public long StorageSizeBytes { get; set; }
}

/// <summary>
/// Evidence verification trend data
/// </summary>
public class EvidenceVerificationTrendData
{
    public string Period { get; set; } = string.Empty;
    public int EvidenceVerified { get; set; }
    public int EvidencePendingVerification { get; set; }
    public double VerificationRate { get; set; }
    public double AverageVerificationTimeDays { get; set; }
}

/// <summary>
/// Storage usage trend data
/// </summary>
public class StorageUsageTrendData
{
    public string Period { get; set; } = string.Empty;
    public long TotalStorageBytes { get; set; }
    public double StorageGrowthPercent { get; set; }
    public int NewFilesAdded { get; set; }
    public int FilesArchived { get; set; }
}

/// <summary>
/// Evidence quality metrics
/// </summary>
public class EvidenceQualityMetrics
{
    public double AverageVerificationTimeDays { get; set; }
    public double MedianVerificationTimeDays { get; set; }
    public int EvidenceWithCompleteMetadata { get; set; }
    public int EvidenceWithIncompleteMetadata { get; set; }
    public double MetadataCompletenessRate { get; set; }
    public int HighQualityEvidence { get; set; }
    public int MediumQualityEvidence { get; set; }
    public int LowQualityEvidence { get; set; }
}

/// <summary>
/// Retention and compliance metrics
/// </summary>
public class RetentionComplianceMetrics
{
    public int EvidenceNearingRetentionExpiry { get; set; }
    public int EvidenceExpired { get; set; }
    public int EvidenceWithDefinedRetention { get; set; }
    public int EvidenceWithoutRetentionPlan { get; set; }
    public double RetentionComplianceRate { get; set; }
    public Dictionary<string, int> EvidenceByRetentionPeriod { get; set; } = new();
    public Dictionary<string, int> EvidenceByRetentionReason { get; set; } = new();
    public List<ExpiringEvidenceDetail> ExpiringEvidenceDetails { get; set; } = new();
}

/// <summary>
/// Details of evidence nearing expiry
/// </summary>
public class ExpiringEvidenceDetail
{
    public string EvidenceCode { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string EvidenceType { get; set; } = string.Empty;
    public DateTime ExpiryDate { get; set; }
    public int DaysToExpiry { get; set; }
    public string AuditCode { get; set; } = string.Empty;
    public string ResponsiblePerson { get; set; } = string.Empty;
}
