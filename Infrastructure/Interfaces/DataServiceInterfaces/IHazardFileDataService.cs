//-----------------------------------------------------------------------
// <copyright file="IHazardFileDataService.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Data service contracts defining business-focused data operations for SMS domain entities.
//                  Infrastructure service contract defining data access operations
//                  and external system integration interfaces.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;

namespace SMS_Infrastructure.Interfaces;

/// <summary>
/// Hazard File Data Service Interface
/// </summary>
public interface IHazardFileDataService
{
    Task<Result<HazardFile>> CreateHazardFileAsync(HazardFile hazardFile, CancellationToken ct = default);
    Task<Result<HazardFile>> GetHazardFileByCodeAsync(string code, CancellationToken ct = default);
    Task<Result<IEnumerable<HazardFile>>> GetHazardFilesByHazardCodeAsync(string hazardCode, bool includeFileData = false, string? category = null, CancellationToken ct = default);
    Task<Result<IEnumerable<HazardFile>>> GetHazardFilesByReportCodeAsync(string reportCode, bool includeFileData = false, CancellationToken ct = default);
    Task<Result<HazardFile>> GetHazardFileDataAsync(string code, CancellationToken ct = default);
    Task<Result<IEnumerable<HazardFile>>> GetActiveHazardFilesAsync(CancellationToken ct = default);
    Task<Result<HazardFile>> UpdateHazardFileAsync(HazardFile hazardFile, CancellationToken ct = default);
    Task<Result<bool>> DeactivateHazardFileAsync(string code, string reason, string deactivatedBy, CancellationToken ct = default);
    Task<Result<bool>> ReactivateHazardFileAsync(string code, string reactivatedBy, CancellationToken ct = default);
    Task<Result<IEnumerable<HazardFile>>> SearchHazardFilesAsync(
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
        CancellationToken ct = default);

    Task<Result<IEnumerable<HazardFile>>> GetHazardPhotosAsync(string hazardCode, CancellationToken ct = default);
    Task<Result<IEnumerable<HazardFile>>> GetHazardDocumentsAsync(string hazardCode, CancellationToken ct = default);
    Task<Result<IEnumerable<HazardFile>>> GetHazardVideosAsync(string hazardCode, CancellationToken ct = default);
    Task<Result<IEnumerable<HazardFile>>> GetConfidentialHazardFilesAsync(string hazardCode, CancellationToken ct = default);
    Task<Result<bool>> SetFileConfidentialityAsync(string code, bool isConfidential, string updatedBy, CancellationToken ct = default);
}
