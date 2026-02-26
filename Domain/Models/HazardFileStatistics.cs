//-----------------------------------------------------------------------
// <copyright file="HazardFileStatistics.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Domain model representing statistical data and metrics for SMS hazardfile reporting.
//                  Domain model representing complex data structures
//                  for business reporting and analytics.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Domain.Models;

/// <summary>
/// Hazard File Statistics Model
/// Provides statistical information about files associated with a hazard
/// </summary>
public class HazardFileStatistics
{
    public string HazardCode { get; set; } = string.Empty;
    public int TotalFiles { get; set; }
    public long TotalSizeBytes { get; set; }
    public int PhotoCount { get; set; }
    public int DocumentCount { get; set; }
    public int VideoCount { get; set; }
    public int ConfidentialCount { get; set; }
    public DateTime? FirstUploadDate { get; set; }
    public DateTime? LastUploadDate { get; set; }

    /// <summary>
    /// Get formatted total file size
    /// </summary>
    public string GetFormattedTotalSize()
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = TotalSizeBytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }

    /// <summary>
    /// Check if hazard has files
    /// </summary>
    public bool HasFiles => TotalFiles > 0;

    /// <summary>
    /// Check if hazard has photos
    /// </summary>
    public bool HasPhotos => PhotoCount > 0;

    /// <summary>
    /// Check if hazard has documents
    /// </summary>
    public bool HasDocuments => DocumentCount > 0;

    /// <summary>
    /// Check if hazard has videos
    /// </summary>
    public bool HasVideos => VideoCount > 0;

    /// <summary>
    /// Check if hazard has confidential files
    /// </summary>
    public bool HasConfidentialFiles => ConfidentialCount > 0;
}
