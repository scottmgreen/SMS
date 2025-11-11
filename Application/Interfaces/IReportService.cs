namespace Application.Interfaces;

public interface IReportService
{
    Task<Result<Report>> CreateReportAsync(Report report, CancellationToken ct = default);
    Task<Result<bool>> DeleteReportAsync(ReportID id, CancellationToken ct = default);
    Task<Result<List<Report>>> GetAllReportsAsync(CancellationToken ct = default);
    Task<Result<Report>> GetReportByIdAsync(ReportID id, CancellationToken ct = default);
    Task<Result<Report>> UpdateReportAsync(Report report, CancellationToken ct = default);
}