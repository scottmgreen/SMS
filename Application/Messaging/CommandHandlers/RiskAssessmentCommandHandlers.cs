using Microsoft.Extensions.Logging;

namespace SMS_Application.Messaging.CommandHandlers;

// =============================================
// RISK ASSESSMENT COMMAND HANDLERS
// =============================================

public class CreateRiskAssessmentCommandHandler : BaseCommandBundle, IRequestHandler<CreateRiskAssessmentCommand, Result<RiskAssessment>>
{
    private readonly RiskAssessmentDataService _dataService;
    private readonly ILogger<CreateRiskAssessmentCommandHandler> _logger;

    public CreateRiskAssessmentCommandHandler(RiskAssessmentDataService dataService, ILogger<CreateRiskAssessmentCommandHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<RiskAssessment>> HandleAsync(CreateRiskAssessmentCommand request, CancellationToken ct = default)
    {
        try
        {
            if (request?.RiskAssessment is null)
            {
                _logger.LogError("CreateRiskAssessmentCommand received with null RiskAssessment");
                return Result<RiskAssessment>.Failure<RiskAssessment>(DomainErrors.RiskAssessmentError.NullOrEmpty);
            }

            _logger.LogInformation("Processing CreateRiskAssessmentCommand for Code: {Code}", request.RiskAssessment.Code);

            var result = await _dataService.CreateRiskAssessmentAsync(request.RiskAssessment, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully created RiskAssessment with ID: {Id}, Code: {Code}",
                    result.Value?.Id, result.Value?.Code);
            }
            else
            {
                _logger.LogError("Failed to create RiskAssessment with Code: {Code}. Error: {Error}",
                    request.RiskAssessment.Code, result.Error?.Message);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("CreateRiskAssessmentCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while creating RiskAssessment");
            return Result<RiskAssessment>.Failure<RiskAssessment>(DomainErrors.RiskAssessmentError.CreateFailed);
        }
    }
}

public class UpdateRiskAssessmentCommandHandler : BaseCommandBundle, IRequestHandler<UpdateRiskAssessmentCommand, Result<RiskAssessment>>
{
    private readonly RiskAssessmentDataService _dataService;
    private readonly ILogger<UpdateRiskAssessmentCommandHandler> _logger;

    public UpdateRiskAssessmentCommandHandler(RiskAssessmentDataService dataService, ILogger<UpdateRiskAssessmentCommandHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<RiskAssessment>> HandleAsync(UpdateRiskAssessmentCommand request, CancellationToken ct = default)
    {
        try
        {
            if (request?.RiskAssessment is null)
            {
                _logger.LogError("UpdateRiskAssessmentCommand received with null RiskAssessment");
                return Result<RiskAssessment>.Failure<RiskAssessment>(DomainErrors.RiskAssessmentError.NullOrEmpty);
            }

            _logger.LogInformation("Processing UpdateRiskAssessmentCommand for ID: {Id}, Code: {Code}",
                request.RiskAssessment.Id, request.RiskAssessment.Code);

            var result = await _dataService.UpdateRiskAssessmentAsync(request.RiskAssessment, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully updated RiskAssessment with ID: {Id}", request.RiskAssessment.Id);
            }
            else
            {
                _logger.LogError("Failed to update RiskAssessment with ID: {Id}. Error: {Error}",
                    request.RiskAssessment.Id, result.Error?.Message);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("UpdateRiskAssessmentCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while updating RiskAssessment with ID: {Id}", request.RiskAssessment?.Id);
            return Result<RiskAssessment>.Failure<RiskAssessment>(DomainErrors.RiskAssessmentError.UpdateFailed);
        }
    }
}

public class DeleteRiskAssessmentCommandHandler : BaseCommandBundle, IRequestHandler<DeleteRiskAssessmentCommand, Result<bool>>
{
    private readonly RiskAssessmentDataService _dataService;
    private readonly ILogger<DeleteRiskAssessmentCommandHandler> _logger;

    public DeleteRiskAssessmentCommandHandler(RiskAssessmentDataService dataService, ILogger<DeleteRiskAssessmentCommandHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> HandleAsync(DeleteRiskAssessmentCommand request, CancellationToken ct = default)
    {
        try
        {
            if (request?.RiskAssessmentId is null)
            {
                _logger.LogError("DeleteRiskAssessmentCommand received with null RiskAssessmentId");
                return Result<bool>.Failure<bool>(DomainErrors.RiskAssessmentError.NullOrEmpty);
            }

            _logger.LogInformation("Processing DeleteRiskAssessmentCommand for ID: {Id}", request.RiskAssessmentId);

            var result = await _dataService.DeleteRiskAssessmentAsync(request.RiskAssessmentId, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully deleted RiskAssessment with ID: {Id}", request.RiskAssessmentId);
            }
            else
            {
                _logger.LogError("Failed to delete RiskAssessment with ID: {Id}. Error: {Error}",
                    request.RiskAssessmentId, result.Error?.Message);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("DeleteRiskAssessmentCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while deleting RiskAssessment with ID: {Id}", request.RiskAssessmentId);
            return Result<bool>.Failure<bool>(DomainErrors.RiskAssessmentError.DeleteFailed);
        }
    }
}