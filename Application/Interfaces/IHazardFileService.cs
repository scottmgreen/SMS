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