using Microsoft.Extensions.Logging;

namespace SMS_Application.Messaging.CommandHandlers;

// =============================================
// REPORT VALIDATION COMMAND HANDLERS
// =============================================

public class CreateReportValidationCommandHandler : BaseCommandBundle, IRequestHandler<CreateReportValidationCommand, Result<ReportValidation>>
{
    private readonly ReportValidationDataService _dataService;
    private readonly ILogger<CreateReportValidationCommandHandler> _logger;

    public CreateReportValidationCommandHandler(ReportValidationDataService dataService, ILogger<CreateReportValidationCommandHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<ReportValidation>> HandleAsync(CreateReportValidationCommand request, CancellationToken ct = default)
    {
        try
        {
            if (request?.ReportValidation is null)
            {
                _logger.LogError("CreateReportValidationCommand received with null ReportValidation");
                return Result<ReportValidation>.Failure<ReportValidation>(DomainErrors.ReportValidationError.NullOrEmpty);
            }

            _logger.LogInformation("Processing CreateReportValidationCommand for Code: {Code}", request.ReportValidation.Code);

            var result = await _dataService.CreateReportValidationAsync(request.ReportValidation, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully created ReportValidation with ID: {Id}, Code: {Code}",
                    result.Value?.Id, result.Value?.Code);
            }
            else
            {
                _logger.LogError("Failed to create ReportValidation with Code: {Code}. Error: {Error}",
                    request.ReportValidation.Code, result.Error?.Message);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("CreateReportValidationCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while creating ReportValidation");
            return Result<ReportValidation>.Failure<ReportValidation>(DomainErrors.ReportValidationError.CreateFailed);
        }
    }
}

public class UpdateReportValidationCommandHandler : BaseCommandBundle, IRequestHandler<UpdateReportValidationCommand, Result<ReportValidation>>
{
    private readonly ReportValidationDataService _dataService;
    private readonly ILogger<UpdateReportValidationCommandHandler> _logger;

    public UpdateReportValidationCommandHandler(ReportValidationDataService dataService, ILogger<UpdateReportValidationCommandHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<ReportValidation>> HandleAsync(UpdateReportValidationCommand request, CancellationToken ct = default)
    {
        try
        {
            if (request?.ReportValidation is null)
            {
                _logger.LogError("UpdateReportValidationCommand received with null ReportValidation");
                return Result<ReportValidation>.Failure<ReportValidation>(DomainErrors.ReportValidationError.NullOrEmpty);
            }

            _logger.LogInformation("Processing UpdateReportValidationCommand for ID: {Id}, Code: {Code}",
                request.ReportValidation.Id, request.ReportValidation.Code);

            var result = await _dataService.UpdateReportValidationAsync(request.ReportValidation, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully updated ReportValidation with ID: {Id}", request.ReportValidation.Id);
            }
            else
            {
                _logger.LogError("Failed to update ReportValidation with ID: {Id}. Error: {Error}",
                    request.ReportValidation.Id, result.Error?.Message);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("UpdateReportValidationCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while updating ReportValidation with ID: {Id}", request.ReportValidation?.Id);
            return Result<ReportValidation>.Failure<ReportValidation>(DomainErrors.ReportValidationError.UpdateFailed);
        }
    }
}

public class DeleteReportValidationCommandHandler : BaseCommandBundle, IRequestHandler<DeleteReportValidationCommand, Result<bool>>
{
    private readonly ReportValidationDataService _dataService;
    private readonly ILogger<DeleteReportValidationCommandHandler> _logger;

    public DeleteReportValidationCommandHandler(ReportValidationDataService dataService, ILogger<DeleteReportValidationCommandHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> HandleAsync(DeleteReportValidationCommand request, CancellationToken ct = default)
    {
        try
        {
            if (request?.ReportValidationId is null)
            {
                _logger.LogError("DeleteReportValidationCommand received with null ReportValidationId");
                return Result<bool>.Failure<bool>(DomainErrors.ReportValidationError.NullOrEmpty);
            }

            _logger.LogInformation("Processing DeleteReportValidationCommand for ID: {Id}", request.ReportValidationId);

            var result = await _dataService.DeleteReportValidationAsync(request.ReportValidationId, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully deleted ReportValidation with ID: {Id}", request.ReportValidationId);
            }
            else
            {
                _logger.LogError("Failed to delete ReportValidation with ID: {Id}. Error: {Error}",
                    request.ReportValidationId, result.Error?.Message);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("DeleteReportValidationCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while deleting ReportValidation with ID: {Id}", request.ReportValidationId);
            return Result<bool>.Failure<bool>(DomainErrors.ReportValidationError.DeleteFailed);
        }
    }
}