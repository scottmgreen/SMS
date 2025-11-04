using Microsoft.Extensions.Logging;

namespace SMS_Application.Messaging.CommandHandlers;

// =============================================
// RISK ANALYSIS COMMAND HANDLERS
// =============================================

public class CreateRiskAnalysisCommandHandler : BaseCommandBundle, IRequestHandler<CreateRiskAnalysisCommand, Result<RiskAnalysis>>
{
    private readonly RiskAnalysisDataService _dataService;
    private readonly ILogger<CreateRiskAnalysisCommandHandler> _logger;

    public CreateRiskAnalysisCommandHandler(RiskAnalysisDataService dataService, ILogger<CreateRiskAnalysisCommandHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<RiskAnalysis>> HandleAsync(CreateRiskAnalysisCommand request, CancellationToken ct = default)
    {
        try
        {
            if (request?.RiskAnalysis is null)
            {
                _logger.LogError("CreateRiskAnalysisCommand received with null RiskAnalysis");
                return Result<RiskAnalysis>.Failure<RiskAnalysis>(DomainErrors.RiskAnalysisError.NullOrEmpty);
            }

            _logger.LogInformation("Processing CreateRiskAnalysisCommand for Code: {Code}", request.RiskAnalysis.Code);

            var result = await _dataService.CreateRiskAnalysisAsync(request.RiskAnalysis, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully created RiskAnalysis with ID: {Id}, Code: {Code}",
                    result.Value?.Id, result.Value?.Code);
            }
            else
            {
                _logger.LogError("Failed to create RiskAnalysis with Code: {Code}. Error: {Error}",
                    request.RiskAnalysis.Code, result.Error?.Message);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("CreateRiskAnalysisCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while creating RiskAnalysis");
            return Result<RiskAnalysis>.Failure<RiskAnalysis>(DomainErrors.RiskAnalysisError.CreateFailed);
        }
    }
}

public class UpdateRiskAnalysisCommandHandler : BaseCommandBundle, IRequestHandler<UpdateRiskAnalysisCommand, Result<RiskAnalysis>>
{
    private readonly RiskAnalysisDataService _dataService;
    private readonly ILogger<UpdateRiskAnalysisCommandHandler> _logger;

    public UpdateRiskAnalysisCommandHandler(RiskAnalysisDataService dataService, ILogger<UpdateRiskAnalysisCommandHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<RiskAnalysis>> HandleAsync(UpdateRiskAnalysisCommand request, CancellationToken ct = default)
    {
        try
        {
            if (request?.RiskAnalysis is null)
            {
                _logger.LogError("UpdateRiskAnalysisCommand received with null RiskAnalysis");
                return Result<RiskAnalysis>.Failure<RiskAnalysis>(DomainErrors.RiskAnalysisError.NullOrEmpty);
            }

            _logger.LogInformation("Processing UpdateRiskAnalysisCommand for ID: {Id}, Code: {Code}",
                request.RiskAnalysis.Id, request.RiskAnalysis.Code);

            var result = await _dataService.UpdateRiskAnalysisAsync(request.RiskAnalysis, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully updated RiskAnalysis with ID: {Id}", request.RiskAnalysis.Id);
            }
            else
            {
                _logger.LogError("Failed to update RiskAnalysis with ID: {Id}. Error: {Error}",
                    request.RiskAnalysis.Id, result.Error?.Message);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("UpdateRiskAnalysisCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while updating RiskAnalysis with ID: {Id}", request.RiskAnalysis?.Id);
            return Result<RiskAnalysis>.Failure<RiskAnalysis>(DomainErrors.RiskAnalysisError.UpdateFailed);
        }
    }
}

public class DeleteRiskAnalysisCommandHandler : BaseCommandBundle, IRequestHandler<DeleteRiskAnalysisCommand, Result<bool>>
{
    private readonly RiskAnalysisDataService _dataService;
    private readonly ILogger<DeleteRiskAnalysisCommandHandler> _logger;

    public DeleteRiskAnalysisCommandHandler(RiskAnalysisDataService dataService, ILogger<DeleteRiskAnalysisCommandHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> HandleAsync(DeleteRiskAnalysisCommand request, CancellationToken ct = default)
    {
        try
        {
            if (request?.RiskAnalysisId is null)
            {
                _logger.LogError("DeleteRiskAnalysisCommand received with null RiskAnalysisId");
                return Result<bool>.Failure<bool>(DomainErrors.RiskAnalysisError.NullOrEmpty);
            }

            _logger.LogInformation("Processing DeleteRiskAnalysisCommand for ID: {Id}", request.RiskAnalysisId);

            var result = await _dataService.DeleteRiskAnalysisAsync(request.RiskAnalysisId, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully deleted RiskAnalysis with ID: {Id}", request.RiskAnalysisId);
            }
            else
            {
                _logger.LogError("Failed to delete RiskAnalysis with ID: {Id}. Error: {Error}",
                    request.RiskAnalysisId, result.Error?.Message);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("DeleteRiskAnalysisCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while deleting RiskAnalysis with ID: {Id}", request.RiskAnalysisId);
            return Result<bool>.Failure<bool>(DomainErrors.RiskAnalysisError.DeleteFailed);
        }
    }
}