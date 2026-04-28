//-----------------------------------------------------------------------
// <copyright file="IHazardFileRepository.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Repository implementing data access operations for SMS ihazardfile entities with safety management integration.
//                  Infrastructure service contract defining data access operations
//                  and external system integration interfaces.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;

namespace SMS_Infrastructure.Interfaces;

/// <summary>
/// Hazard File Repository Interface
/// Provides data access operations for hazard file management
/// </summary>
public interface IHazardFileRepository
{
    // Core CRUD operations
    Task<Result<HazardFile>> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<Result<HazardFile>> AddAsync(HazardFile hazardFile, CancellationToken cancellationToken = default);
    Task<Result<HazardFile>> UpdateAsync(HazardFile hazardFile, CancellationToken cancellationToken = default);
    Task<Result<bool>> DeactivateAsync(string code, string reason, string deactivatedBy, CancellationToken cancellationToken = default);
    Task<Result<bool>> ReactivateAsync(string code, string reactivatedBy, CancellationToken cancellationToken = default);

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



    // File type queries
    Task<Result<IEnumerable<HazardFile>>> GetImageFilesAsync(string hazardCode, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<HazardFile>>> GetDocumentFilesAsync(string hazardCode, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<HazardFile>>> GetVideoFilesAsync(string hazardCode, CancellationToken cancellationToken = default);

    // Confidential file operations
    Task<Result<IEnumerable<HazardFile>>> GetConfidentialFilesAsync(string hazardCode, CancellationToken cancellationToken = default);
    Task<Result<bool>> SetFileConfidentialityAsync(string code, bool isConfidential, string updatedBy, CancellationToken cancellationToken = default);
}
