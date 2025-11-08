// =============================================
// SMS DATA SERVICES - COMPLETE SET
// All 10 SMS Data Services for easy copy/paste
// =============================================

// 1. HazardDataService.cs
using SMS_Domain.Errors;

namespace SMS_Infrastructure.Services;

// 6. RiskAssessmentDataService.cs
public class RiskAssessmentDataService : BaseDataService<RiskAssessmentDataService>
{
    private readonly ILogger<RiskAssessmentDataService> _logger;
    private readonly string _logheader;
    private readonly RiskAssessmentRepository _repo;

    public RiskAssessmentDataService(ILogger<RiskAssessmentDataService> logger, IServiceScopeFactory serviceScopeFactory, IConfiguration configuration, RiskAssessmentRepository repo)
        : base(logger, serviceScopeFactory, configuration)
    {
        _logger = base.Logger;
        _logheader = base.LogHeader;
        _repo = repo;

        _logger.LogInfrastructureInformation(InfrastructureEventIds.InfrastructureEvent, $"{_logheader} {repo.GetType().Name}");
    }

    public Task<Result<RiskAssessment>> CreateRiskAssessmentAsync(RiskAssessment riskAssessment, CancellationToken ct = default)
    {
        return _repo.CreateRiskAssessmentAsync(riskAssessment, ct);
    }

    public async Task<Result<RiskAssessment>> GetRiskAssessmentByIdAsync(RiskAssessmentID riskAssessmentId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Retrieving RiskAssessment by ID: {Id}", riskAssessmentId);

            var result = await _repo.GetRiskAssessmentByIdAsync(riskAssessmentId, cancellationToken).ConfigureAwait(false);

            if (result.IsFailure)
            {
                _logger.LogWarning("RiskAssessment not found with ID: {Id}", riskAssessmentId);
                return result;
            }

            _logger.LogInformation("Successfully retrieved RiskAssessment: {Id}", riskAssessmentId);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve RiskAssessment by ID: {Id}", riskAssessmentId);
            return Result<RiskAssessment>.Failure<RiskAssessment>(DomainErrors.RiskAssessmentError.NotFound);
        }
    }

    public Task<Result<List<RiskAssessment>>> GetAllRiskAssessmentsAsync(CancellationToken ct = default)
    {
        return _repo.GetAllRiskAssessmentsAsync(ct);
    }

    public Task<Result<RiskAssessment>> UpdateRiskAssessmentAsync(RiskAssessment riskAssessment, CancellationToken ct = default)
    {
        return _repo.UpdateRiskAssessmentAsync(riskAssessment, ct);
    }

    public async Task<Result<bool>> DeleteRiskAssessmentAsync(RiskAssessmentID riskAssessmentId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Deleting RiskAssessment with ID: {Id}", riskAssessmentId);

            var result = await _repo.DeleteRiskAssessmentAsync(riskAssessmentId, cancellationToken).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully deleted RiskAssessment with ID: {Id}", riskAssessmentId);
            }
            else
            {
                _logger.LogWarning("Failed to delete RiskAssessment with ID: {Id}", riskAssessmentId);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting RiskAssessment with ID: {Id}", riskAssessmentId);
            return Result<bool>.Failure<bool>(DomainErrors.RiskAssessmentError.DeletionFailed);
        }
    }
}
