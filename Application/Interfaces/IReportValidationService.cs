namespace Application.Interfaces;

public interface IReportValidationService
{
    Task<Result<ReportValidation>> CreateReportValidationAsync(ReportValidation reportValidation, CancellationToken ct = default);
    Task<Result<bool>> DeleteReportValidationAsync(ReportValidationID id, CancellationToken ct = default);
    Task<Result<List<ReportValidation>>> GetAllReportValidationsAsync(CancellationToken ct = default);
    Task<Result<ReportValidation>> GetReportValidationByIdAsync(ReportValidationID id, CancellationToken ct = default);
    Task<Result<ReportValidation>> UpdateReportValidationAsync(ReportValidation reportValidation, CancellationToken ct = default);
}