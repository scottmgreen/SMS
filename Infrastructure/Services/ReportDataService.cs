// =============================================
// SMS DATA SERVICES - COMPLETE SET
// All 10 SMS Data Services for easy copy/paste
// =============================================

// 2. ReportDataService.cs
using SMS_Infrastructure.Interfaces;
using SMS_Infrastructure.Persistence;

namespace SMS_Infrastructure.Services;

public class ReportDataService : BaseDataService<ReportDataService>, IReportDataService
{
    private readonly ILogger<ReportDataService> _logger;
    private readonly string _logheader;
    private readonly ReportRepository _repo;

    public ReportDataService(ILogger<ReportDataService> logger, IServiceScopeFactory serviceScopeFactory, IConfiguration configuration, ReportRepository repo)
        : base(logger, serviceScopeFactory, configuration)
    {
        _logger = base.Logger;
        _logheader = base.LogHeader;
        _repo = repo;

        _logger.LogInfrastructureInformation(InfrastructureEventIds.InfrastructureEvent, $"{_logheader} {repo.GetType().Name}");
    }

    public Task<Result<Report>> CreateReportAsync(Report report, CancellationToken ct = default)
    {
        return _repo.CreateReportAsync(report, ct);
    }

    public Task<Result<Report>> GetReportByIdAsync(ReportID id, CancellationToken ct = default)
    {
        return _repo.GetReportByIdAsync(id, ct);
    }

    public Task<Result<List<Report>>> GetAllReportsAsync(CancellationToken ct = default)
    {
        return _repo.GetAllReportsAsync(ct);
    }

    public Task<Result<Report>> UpdateReportAsync(Report report, CancellationToken ct = default)
    {
        return _repo.UpdateReportAsync(report, ct);
    }

    public Task<Result<bool>> DeleteReportAsync(ReportID id, CancellationToken ct = default)
    {
        return _repo.DeleteReportAsync(id, ct);
    }
}
