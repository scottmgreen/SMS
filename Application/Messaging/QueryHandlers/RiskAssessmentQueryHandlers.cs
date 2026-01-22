using Microsoft.Extensions.Logging;

using SMS_Application.Messaging.Queries;

namespace SMS_Application.Messaging.QueryHandlers;

// =============================================
// RISK ASSESSMENT QUERY HANDLERS
// =============================================

public class GetRiskAssessmentByIdQueryHandler : BaseQueryBundle, IRequestHandler<GetRiskAssessmentByIdQuery, Result<RiskAssessment>>
{
    private readonly RiskAssessmentService _appService;
    private readonly ILogger<GetRiskAssessmentByIdQueryHandler> _logger;

    public GetRiskAssessmentByIdQueryHandler(RiskAssessmentService appService, ILogger<GetRiskAssessmentByIdQueryHandler> logger)
    {
        _appService = appService ?? throw new ArgumentNullException(nameof(appService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<RiskAssessment>> HandleAsync(GetRiskAssessmentByIdQuery request, CancellationToken ct = default)
    {
        try
        {
            if (request?.RiskAssessmentId is null)
            {
                _logger.LogApplicationError("GetRiskAssessmentByIdQuery received with null RiskAssessmentId", ApplicationEventIds.Error, null);
                return Result<RiskAssessment>.Failure<RiskAssessment>(DomainErrors.RiskAssessmentError.NotFound);
            }

            _logger.LogInformation("Processing GetRiskAssessmentByIdQuery for ID: {Id}", request.RiskAssessmentId.Value);

            var result = await _appService.GetRiskAssessmentByIdAsync(request.RiskAssessmentId, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully retrieved RiskAssessment with ID: {Id}", request.RiskAssessmentId.Value);
            }
            else
            {
                _logger.LogApplicationError("Failed to retrieve RiskAssessment with ID: {Id}. Error: {Error}",
                    ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("GetRiskAssessmentByIdQuery operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while retrieving RiskAssessment", ApplicationEventIds.Error, ex);
            return Result<RiskAssessment>.Failure<RiskAssessment>(DomainErrors.RiskAssessmentError.NotFound);
        }
    }
}

public class GetRiskAssessmentByHazardIdQueryHandler : BaseQueryBundle, IRequestHandler<GetRiskAssessmentsByHazardIdQuery, Result<List<RiskAssessment>>>
{
    private readonly RiskAssessmentDataService _appService;
    private readonly ILogger<GetRiskAssessmentByHazardIdQueryHandler> _logger;

    public GetRiskAssessmentByHazardIdQueryHandler(RiskAssessmentDataService dataService, ILogger<GetRiskAssessmentByHazardIdQueryHandler> logger)
    {
        _appService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<List<RiskAssessment>>> HandleAsync(GetRiskAssessmentsByHazardIdQuery request, CancellationToken ct = default)
    {
        try
        {
            if (request?.HazardId is null)
            {
                _logger.LogApplicationError("GetRiskAssessmentByIdQuery received with null RiskAssessmentId", ApplicationEventIds.Error, null);
                return Result<List<RiskAssessment>>.Failure<List<RiskAssessment>>(DomainErrors.RiskAssessmentError.NotFound);
            }

            _logger.LogInformation("Processing GetRiskAssessmentByIdQuery for ID: {Id}", request.HazardId.Value);

            var result = await _appService.GetRiskAssessmentsByHazardIdAsync(request.HazardId, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully retrieved RiskAssessment with Hazard ID: {Id}", request.HazardId.Value);
            }
            else
            {
                _logger.LogApplicationError("Failed to retrieve RiskAssessment with ID: {Id}. Error: {Error}",
                    ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("GetRiskAssessmentByIdQuery operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while retrieving RiskAssessment", ApplicationEventIds.Error, ex);
            return Result<List<RiskAssessment>>.Failure<List<RiskAssessment>>(DomainErrors.RiskAssessmentError.NotFound);
        }
    }
}



public class GetAllRiskAssessmentsQueryHandler : BaseQueryBundle, IRequestHandler<GetAllRiskAssessmentsQuery, Result<List<RiskAssessment>>>
{
    private readonly RiskAssessmentDataService _dataService;
    private readonly ILogger<GetAllRiskAssessmentsQueryHandler> _logger;

    public GetAllRiskAssessmentsQueryHandler(RiskAssessmentDataService dataService, ILogger<GetAllRiskAssessmentsQueryHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<List<RiskAssessment>>> HandleAsync(GetAllRiskAssessmentsQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetAllRiskAssessmentsQuery");

            var result = await _dataService.GetAllRiskAssessmentsAsync(ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully retrieved {Count} RiskAssessments", result.Value?.Count ?? 0);
            }
            else
            {
                _logger.LogApplicationError("Failed to retrieve RiskAssessments. Error: {Error}", ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("GetAllRiskAssessmentsQuery operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while retrieving all RiskAssessments", ApplicationEventIds.Error, ex);
            return Result<List<RiskAssessment>>.Failure<List<RiskAssessment>>(DomainErrors.RiskAssessmentError.NotFound);
        }
    }
}