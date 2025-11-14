// =============================================
// SMS DATA SERVICES - COMPLETE SET
// All 10 SMS Data Services for easy copy/paste
// =============================================

// 1. HazardDataService.cs
using SMS_Infrastructure.Persistence;

namespace SMS_Infrastructure.Services;

// 9. ReportValidationDataService.cs
public class ReportValidationDataService : BaseDataService<ReportValidationDataService>
{
    private readonly ILogger<ReportValidationDataService> _logger;
    private readonly string _logheader;
    private readonly ReportValidationRepository _repo;

    public ReportValidationDataService(ILogger<ReportValidationDataService> logger, IServiceScopeFactory serviceScopeFactory, IConfiguration configuration, ReportValidationRepository repo)
        : base(logger, serviceScopeFactory, configuration)
    {
        _logger = base.Logger;
        _logheader = base.LogHeader;
        _repo = repo;

        _logger.LogInfrastructureInformation(InfrastructureEventIds.InfrastructureEvent, $"{_logheader} {repo.GetType().Name}");
    }

    public Task<Result<ReportValidation>> CreateReportValidationAsync(ReportValidation reportValidation, CancellationToken ct = default)
    {
        return _repo.CreateReportValidationAsync(reportValidation, ct);
    }

    public Task<Result<ReportValidation>> GetReportValidationByIdAsync(ReportValidationID id, CancellationToken ct = default)
    {
        return _repo.GetReportValidationByIdAsync(id, ct);
    }

    public Task<Result<ReportValidation>> GetReportValidationByReportIdAsync(ReportID id, CancellationToken ct = default)
    {
        return _repo.GetReportValidationByReportIdAsync(id, ct);
    }

    public Task<Result<List<ReportValidation>>> GetAllReportValidationsAsync(CancellationToken ct = default)
    {
        return _repo.GetAllReportValidationsAsync(ct);
    }

    public Task<Result<ReportValidation>> UpdateReportValidationAsync(ReportValidation reportValidation, CancellationToken ct = default)
    {
        return _repo.UpdateReportValidationAsync(reportValidation, ct);
    }

    public Task<Result<bool>> DeleteReportValidationAsync(ReportValidationID id, CancellationToken ct = default)
    {
        return _repo.DeleteReportValidationAsync(id, ct);
    }
}
