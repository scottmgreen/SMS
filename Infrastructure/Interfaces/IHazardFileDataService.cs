using SMS_Domain.Entities;
using SMS_Domain.Models;
using SMS_Shared.Common;

namespace SMS_Infrastructure.Interfaces;

/// <summary>
/// HazardFile Data Service Interface - follows the exact same pattern as IHazardDataService
/// Defines the contract for HazardFile business logic operations
/// </summary>
public interface IHazardFileDataService
{
    Task<Result<HazardFile>> CreateHazardFileAsync(HazardFile hazardFile, CancellationToken ct = default);
    Task<Result<HazardFile>> GetHazardFileByIdAsync(int id, CancellationToken ct = default);
    Task<Result<HazardFile>> GetHazardFileByCodeAsync(string code, CancellationToken ct = default);
    Task<Result<IEnumerable<HazardFile>>> GetHazardFilesByHazardCodeAsync(string hazardCode, bool includeFileData = false, string? category = null, CancellationToken ct = default);
    Task<Result<IEnumerable<HazardFile>>> GetHazardFilesByReportCodeAsync(string reportCode, bool includeFileData = false, CancellationToken ct = default);
    Task<Result<HazardFile>> GetHazardFileDataAsync(string code, CancellationToken ct = default);
    Task<Result<IEnumerable<HazardFile>>> GetActiveHazardFilesAsync(CancellationToken ct = default);
    Task<Result<HazardFile>> UpdateHazardFileAsync(HazardFile hazardFile, CancellationToken ct = default);
    Task<Result<bool>> DeactivateHazardFileAsync(int id, string reason, string deactivatedBy, CancellationToken ct = default);
    Task<Result<bool>> ReactivateHazardFileAsync(int id, string reactivatedBy, CancellationToken ct = default);
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
    Task<Result<HazardFileStatistics>> GetHazardFileStatisticsAsync(string hazardCode, CancellationToken ct = default);
    Task<Result<IEnumerable<HazardFile>>> GetHazardPhotosAsync(string hazardCode, CancellationToken ct = default);
    Task<Result<IEnumerable<HazardFile>>> GetHazardDocumentsAsync(string hazardCode, CancellationToken ct = default);
    Task<Result<IEnumerable<HazardFile>>> GetHazardVideosAsync(string hazardCode, CancellationToken ct = default);
    Task<Result<IEnumerable<HazardFile>>> GetConfidentialHazardFilesAsync(string hazardCode, CancellationToken ct = default);
    Task<Result<bool>> SetFileConfidentialityAsync(string code, bool isConfidential, string updatedBy, CancellationToken ct = default);
}