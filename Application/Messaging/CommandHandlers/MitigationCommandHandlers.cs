using Microsoft.Extensions.Logging;

namespace SMS_Application.Messaging.CommandHandlers;

// =============================================
// MITIGATION COMMAND HANDLERS
// =============================================

public class CreateMitigationCommandHandler : BaseCommandBundle, IRequestHandler<CreateMitigationCommand, Result<Mitigation>>
{
    private readonly MitigationDataService _dataService;
    private readonly ILogger<CreateMitigationCommandHandler> _logger;

    public CreateMitigationCommandHandler(MitigationDataService dataService, ILogger<CreateMitigationCommandHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<Mitigation>> HandleAsync(CreateMitigationCommand request, CancellationToken ct = default)
    {
        try
        {
            if (request?.Mitigation is null)
            {
                _logger.LogApplicationError("CreateMitigationCommand received with null Mitigation", ApplicationEventIds.Error, null);
                return Result<Mitigation>.Failure<Mitigation>(DomainErrors.MitigationError.NullOrEmpty);
            }

            _logger.LogInformation("Processing CreateMitigationCommand for Code: {Code}", request.Mitigation.Code);

            var result = await _dataService.CreateMitigationAsync(request.Mitigation, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully created Mitigation with ID: {Id}, Code: {Code}",
                    result.Value?.Id, result.Value?.Code);
            }
            else
            {
                _logger.LogApplicationError("Failed to create Mitigation with Code: {Code}. Error: {Error}",
                    ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("CreateMitigationCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while creating Mitigation", ApplicationEventIds.Error, ex);
            return Result<Mitigation>.Failure<Mitigation>(DomainErrors.MitigationError.CreateFailed);
        }
    }
}

public class UpdateMitigationCommandHandler : BaseCommandBundle, IRequestHandler<UpdateMitigationCommand, Result<Mitigation>>
{
    private readonly MitigationDataService _dataService;
    private readonly ILogger<UpdateMitigationCommandHandler> _logger;

    public UpdateMitigationCommandHandler(MitigationDataService dataService, ILogger<UpdateMitigationCommandHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<Mitigation>> HandleAsync(UpdateMitigationCommand request, CancellationToken ct = default)
    {
        try
        {
            if (request?.Mitigation is null)
            {
                _logger.LogApplicationError("UpdateMitigationCommand received with null Mitigation", ApplicationEventIds.Error, null);
                return Result<Mitigation>.Failure<Mitigation>(DomainErrors.MitigationError.NullOrEmpty);
            }

            _logger.LogInformation("Processing UpdateMitigationCommand for ID: {Id}, Code: {Code}",
                request.Mitigation.Id, request.Mitigation.Code);

            var result = await _dataService.UpdateMitigationAsync(request.Mitigation, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully updated Mitigation with ID: {Id}", request.Mitigation.Id);
            }
            else
            {
                _logger.LogApplicationError("Failed to update Mitigation with ID: {Id}. Error: {Error}",
                    ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("UpdateMitigationCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while updating Mitigation with ID: {Id}", ApplicationEventIds.Error, ex);
            return Result<Mitigation>.Failure<Mitigation>(DomainErrors.MitigationError.UpdateFailed);
        }
    }
}

public class DeleteMitigationCommandHandler : BaseCommandBundle, IRequestHandler<DeleteMitigationCommand, Result<bool>>
{
    private readonly MitigationDataService _dataService;
    private readonly ILogger<DeleteMitigationCommandHandler> _logger;

    public DeleteMitigationCommandHandler(MitigationDataService dataService, ILogger<DeleteMitigationCommandHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> HandleAsync(DeleteMitigationCommand request, CancellationToken ct = default)
    {
        try
        {
            if (request?.MitigationId is null)
            {
                _logger.LogApplicationError("DeleteMitigationCommand received with null MitigationId", ApplicationEventIds.Error, null);
                return Result<bool>.Failure<bool>(DomainErrors.MitigationError.NullOrEmpty);
            }

            _logger.LogInformation("Processing DeleteMitigationCommand for ID: {Id}", request.MitigationId);

            var result = await _dataService.DeleteMitigationAsync(request.MitigationId, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully deleted Mitigation with ID: {Id}", request.MitigationId);
            }
            else
            {
                _logger.LogApplicationError("Failed to delete Mitigation with ID: {Id}. Error: {Error}",
                    ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("DeleteMitigationCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while deleting Mitigation with ID: {Id}", ApplicationEventIds.Error, ex);
            return Result<bool>.Failure<bool>(DomainErrors.MitigationError.DeleteFailed);
        }
    }
}