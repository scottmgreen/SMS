using SMS_Infrastructure.Interfaces;

namespace SMS_Infrastructure.Services;

/// <summary>
/// HazardFile Data Service - follows the exact same pattern as HazardDataService
/// Provides business logic layer between CQRS handlers and repository
/// </summary>
public class HazardFileDataService : BaseDataService<HazardFileDataService>, IHazardFileDataService
{
    private readonly ILogger<HazardFileDataService> _logger;
    private readonly string _logheader;
    private readonly HazardFileRepository _repo;

    public HazardFileDataService(ILogger<HazardFileDataService> logger, IServiceScopeFactory serviceScopeFactory, IConfiguration configuration, HazardFileRepository repo)
        : base(logger, serviceScopeFactory, configuration)
    {
        _logger = base.Logger;
        _logheader = base.LogHeader;
        _repo = repo;

        _logger.LogInfrastructureInformation(InfrastructureEventIds.InfrastructureEvent, $"{_logheader} {repo.GetType().Name}");
    }

    public Task<Result<HazardFile>> CreateHazardFileAsync(HazardFile hazardFile, CancellationToken ct = default)
    {
        return _repo.AddAsync(hazardFile, ct);
    }

    public Task<Result<HazardFile>> GetHazardFileByIdAsync(int id, CancellationToken ct = default)
    {
        return _repo.GetByIdAsync(id, ct);
    }

    public Task<Result<HazardFile>> GetHazardFileByCodeAsync(string code, CancellationToken ct = default)
    {
        return _repo.GetByCodeAsync(code, ct);
    }

    public Task<Result<IEnumerable<HazardFile>>> GetHazardFilesByHazardCodeAsync(string hazardCode, bool includeFileData = false, string? category = null, CancellationToken ct = default)
    {
        return _repo.GetByHazardCodeAsync(hazardCode, includeFileData, category, ct);
    }

    public Task<Result<IEnumerable<HazardFile>>> GetHazardFilesByReportCodeAsync(string reportCode, bool includeFileData = false, CancellationToken ct = default)
    {
        return _repo.GetByReportCodeAsync(reportCode, includeFileData, ct);
    }

    public Task<Result<HazardFile>> GetHazardFileDataAsync(string code, CancellationToken ct = default)
    {
        return _repo.GetFileDataAsync(code, ct);
    }

    public Task<Result<IEnumerable<HazardFile>>> GetActiveHazardFilesAsync(CancellationToken ct = default)
    {
        return _repo.GetActiveFilesAsync(ct);
    }

    public Task<Result<HazardFile>> UpdateHazardFileAsync(HazardFile hazardFile, CancellationToken ct = default)
    {
        return _repo.UpdateAsync(hazardFile, ct);
    }

    public Task<Result<bool>> DeactivateHazardFileAsync(int id, string reason, string deactivatedBy, CancellationToken ct = default)
    {
        return _repo.DeactivateAsync(id, reason, deactivatedBy, ct);
    }

    public Task<Result<bool>> ReactivateHazardFileAsync(int id, string reactivatedBy, CancellationToken ct = default)
    {
        return _repo.ReactivateAsync(id, reactivatedBy, ct);
    }

    public Task<Result<IEnumerable<HazardFile>>> SearchHazardFilesAsync(
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
        CancellationToken ct = default)
    {
        return _repo.SearchAsync(hazardCode, reportCode, fileType, category, searchText, uploadedBy, dateFrom, dateTo, includeConfidential, maxResults, ct);
    }

    public Task<Result<HazardFileStatistics>> GetHazardFileStatisticsAsync(string hazardCode, CancellationToken ct = default)
    {
        return _repo.GetStatisticsAsync(hazardCode, ct);
    }

    public Task<Result<IEnumerable<HazardFile>>> GetHazardPhotosAsync(string hazardCode, CancellationToken ct = default)
    {
        return _repo.GetImageFilesAsync(hazardCode, ct);
    }

    public Task<Result<IEnumerable<HazardFile>>> GetHazardDocumentsAsync(string hazardCode, CancellationToken ct = default)
    {
        return _repo.GetDocumentFilesAsync(hazardCode, ct);
    }

    public Task<Result<IEnumerable<HazardFile>>> GetHazardVideosAsync(string hazardCode, CancellationToken ct = default)
    {
        return _repo.GetVideoFilesAsync(hazardCode, ct);
    }

    public Task<Result<IEnumerable<HazardFile>>> GetConfidentialHazardFilesAsync(string hazardCode, CancellationToken ct = default)
    {
        return _repo.GetConfidentialFilesAsync(hazardCode, ct);
    }

    public Task<Result<bool>> SetFileConfidentialityAsync(string code, bool isConfidential, string updatedBy, CancellationToken ct = default)
    {
        return _repo.SetFileConfidentialityAsync(code, isConfidential, updatedBy, ct);
    }
}