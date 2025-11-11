using SMS_Domain.Entities;
using SMS_Domain.Models;
using SMS_Shared.Common;

namespace SMS_Infrastructure.Interfaces;

/// <summary>
/// Hazard File Repository Interface
/// Provides data access operations for hazard file management
/// </summary>
public interface IHazardFileRepository
{
    // Core CRUD operations
    Task<Result<HazardFile>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<HazardFile>> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<Result<HazardFile>> AddAsync(HazardFile hazardFile, CancellationToken cancellationToken = default);
    Task<Result<HazardFile>> UpdateAsync(HazardFile hazardFile, CancellationToken cancellationToken = default);
    Task<Result<bool>> DeactivateAsync(int id, string reason, string deactivatedBy, CancellationToken cancellationToken = default);
    Task<Result<bool>> ReactivateAsync(int id, string reactivatedBy, CancellationToken cancellationToken = default);

    // Query operations
    Task<Result<IEnumerable<HazardFile>>> GetByHazardCodeAsync(string hazardCode, bool includeFileData = false, string? category = null, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<HazardFile>>> GetByReportCodeAsync(string reportCode, bool includeFileData = false, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<HazardFile>>> GetActiveFilesAsync(CancellationToken cancellationToken = default);
    
    // File data operations
    Task<Result<HazardFile>> GetFileDataAsync(string code, CancellationToken cancellationToken = default);
    
    // Search operations
    Task<Result<IEnumerable<HazardFile>>> SearchAsync(
        string? hazardCode = null,
        string? reportCode = null,
        string? fileType = null,
        string? category = null,
        string? searchText = null,
        string? uploadedBy = null,
        DateTime? dateFrom = null,
        DateTime? dateTo = null,
        bool includeConfidential = false,
        int maxResults = 100,
        CancellationToken cancellationToken = default);

    // Statistics
    Task<Result<HazardFileStatistics>> GetStatisticsAsync(string hazardCode, CancellationToken cancellationToken = default);

    // File type queries
    Task<Result<IEnumerable<HazardFile>>> GetImageFilesAsync(string hazardCode, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<HazardFile>>> GetDocumentFilesAsync(string hazardCode, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<HazardFile>>> GetVideoFilesAsync(string hazardCode, CancellationToken cancellationToken = default);
    
    // Confidential file operations
    Task<Result<IEnumerable<HazardFile>>> GetConfidentialFilesAsync(string hazardCode, CancellationToken cancellationToken = default);
    Task<Result<bool>> SetFileConfidentialityAsync(string code, bool isConfidential, string updatedBy, CancellationToken cancellationToken = default);
}