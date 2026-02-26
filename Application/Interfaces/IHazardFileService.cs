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
public interface IHazardFileService
{
    Task<Result<HazardFile>> CreateHazardFileAsync(HazardFile hazardFile, CancellationToken ct = default);
    Task<Result<bool>> DeactivateHazardFileAsync(int id, string reason, string deactivatedBy, CancellationToken ct = default);
    Task<Result<HazardFile>> GetHazardFileByCodeAsync(string code, CancellationToken ct = default);
    Task<Result<HazardFile>> GetHazardFileDataAsync(string code, CancellationToken ct = default);
    Task<Result<IEnumerable<HazardFile>>> GetHazardFilesByHazardCodeAsync(string hazardCode, bool includeFileData = false, string? category = null, CancellationToken ct = default);
    Task<Result<IEnumerable<HazardFile>>> SearchHazardFilesAsync(string? hazardCode = null, string? reportCode = null, string? fileType = null, string? category = null, string? searchText = null, string? uploadedBy = null, DateTime? dateFrom = null, DateTime? dateTo = null, bool includeConfidential = false, int maxResults = 100, CancellationToken ct = default);
    Task<Result<HazardFile>> UpdateHazardFileAsync(HazardFile hazardFile, CancellationToken ct = default);
}
