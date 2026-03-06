//-----------------------------------------------------------------------
// <copyright file="IHazardFileService.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: SMS Hazard management service handling hazard identification and lifecycle.
//                  Provides business logic operations and coordinates domain entities
//                  through the CQRS pattern via Mediator services.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Application.Interfaces;

/// <summary>
/// Application service interface for Hazard File management and business operations
/// </summary>
public interface IHazardFileService
{
    // Core CRUD Operations
    Task<Result<HazardFile>> CreateHazardFileAsync(HazardFile hazardFile, CancellationToken ct = default);
    Task<Result<HazardFile>> UpdateHazardFileAsync(HazardFile hazardFile, CancellationToken ct = default);
    Task<Result<bool>> DeactivateHazardFileAsync(int id, string reason, string deactivatedBy, CancellationToken ct = default);
    Task<Result<bool>> ReactivateHazardFileAsync(int fileId, string reactivatedBy, CancellationToken ct = default);
    Task<Result<bool>> SetFileConfidentialityAsync(string fileCode, bool isConfidential, string updatedBy, CancellationToken ct = default);

    // Query Operations
    Task<Result<HazardFile>> GetHazardFileByCodeAsync(string code, CancellationToken ct = default);
    Task<Result<HazardFile>> GetHazardFileDataAsync(string code, CancellationToken ct = default);
    Task<Result<IEnumerable<HazardFile>>> GetHazardFilesByHazardCodeAsync(string hazardCode, bool includeFileData = false, string? category = null, CancellationToken ct = default);
    Task<Result<IEnumerable<HazardFile>>> GetHazardFilesByReportCodeAsync(string reportCode, bool includeFileData = false, CancellationToken ct = default);
    Task<Result<IEnumerable<HazardFile>>> GetActiveHazardFilesAsync(CancellationToken ct = default);
    Task<Result<IEnumerable<HazardFile>>> SearchHazardFilesAsync(string? hazardCode = null, string? reportCode = null, string? fileType = null, string? category = null, string? searchText = null, string? uploadedBy = null, DateTime? dateFrom = null, DateTime? dateTo = null, bool includeConfidential = false, int maxResults = 100, CancellationToken ct = default);

    // File Type Specific Queries
    Task<Result<IEnumerable<HazardFile>>> GetHazardPhotosAsync(string hazardCode, CancellationToken ct = default);
    Task<Result<IEnumerable<HazardFile>>> GetHazardDocumentsAsync(string hazardCode, CancellationToken ct = default);
    Task<Result<IEnumerable<HazardFile>>> GetHazardVideosAsync(string hazardCode, CancellationToken ct = default);
    Task<Result<IEnumerable<HazardFile>>> GetConfidentialHazardFilesAsync(string hazardCode, CancellationToken ct = default);
}
