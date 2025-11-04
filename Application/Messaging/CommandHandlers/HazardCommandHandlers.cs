using Microsoft.Extensions.Logging;

namespace SMS_Application.Messaging.CommandHandlers;

// =============================================
// HAZARD COMMAND HANDLERS
// =============================================

public class CreateHazardCommandHandler : BaseCommandBundle, IRequestHandler<CreateHazardCommand, Result<Hazard>>
{
    private readonly HazardDataService _dataService;
    private readonly ILogger<CreateHazardCommandHandler> _logger;

    public CreateHazardCommandHandler(HazardDataService dataService, ILogger<CreateHazardCommandHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<Hazard>> HandleAsync(CreateHazardCommand request, CancellationToken ct = default)
    {
        try
        {
            if (request?.Hazard is null)
            {
                _logger.LogError("CreateHazardCommand received with null Hazard");
                return Result<Hazard>.Failure<Hazard>(DomainErrors.HazardError.NullOrEmpty);
            }

            _logger.LogInformation("Processing CreateHazardCommand for Code: {Code}", request.Hazard.Code);

            var result = await _dataService.CreateHazardAsync(request.Hazard, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully created Hazard with ID: {Id}, Code: {Code}",
                    result.Value?.Id, result.Value?.Code);
            }
            else
            {
                _logger.LogError("Failed to create Hazard with Code: {Code}. Error: {Error}",
                    request.Hazard.Code, result.Error?.Message);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("CreateHazardCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while creating Hazard");
            return Result<Hazard>.Failure<Hazard>(DomainErrors.HazardError.CreateFailed);
        }
    }
}

public class UpdateHazardCommandHandler : BaseCommandBundle, IRequestHandler<UpdateHazardCommand, Result<Hazard>>
{
    private readonly HazardDataService _dataService;
    private readonly ILogger<UpdateHazardCommandHandler> _logger;

    public UpdateHazardCommandHandler(HazardDataService dataService, ILogger<UpdateHazardCommandHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<Hazard>> HandleAsync(UpdateHazardCommand request, CancellationToken ct = default)
    {
        try
        {
            if (request?.Hazard is null)
            {
                _logger.LogError("UpdateHazardCommand received with null Hazard");
                return Result<Hazard>.Failure<Hazard>(DomainErrors.HazardError.NullOrEmpty);
            }

            _logger.LogInformation("Processing UpdateHazardCommand for ID: {Id}, Code: {Code}",
                request.Hazard.Id, request.Hazard.Code);

            var result = await _dataService.UpdateHazardAsync(request.Hazard, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully updated Hazard with ID: {Id}", request.Hazard.Id);
            }
            else
            {
                _logger.LogError("Failed to update Hazard with ID: {Id}. Error: {Error}",
                    request.Hazard.Id, result.Error?.Message);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("UpdateHazardCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while updating Hazard with ID: {Id}", request.Hazard?.Id);
            return Result<Hazard>.Failure<Hazard>(DomainErrors.HazardError.UpdateFailed);
        }
    }
}

public class DeleteHazardCommandHandler : BaseCommandBundle, IRequestHandler<DeleteHazardCommand, Result<bool>>
{
    private readonly HazardDataService _dataService;
    private readonly ILogger<DeleteHazardCommandHandler> _logger;

    public DeleteHazardCommandHandler(HazardDataService dataService, ILogger<DeleteHazardCommandHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> HandleAsync(DeleteHazardCommand request, CancellationToken ct = default)
    {
        try
        {
            if (request?.HazardId is null)
            {
                _logger.LogError("DeleteHazardCommand received with null HazardId");
                return Result<bool>.Failure<bool>(DomainErrors.HazardError.NullOrEmpty);
            }

            _logger.LogInformation("Processing DeleteHazardCommand for ID: {Id}", request.HazardId);

            var result = await _dataService.DeleteHazardAsync(request.HazardId, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully deleted Hazard with ID: {Id}", request.HazardId);
            }
            else
            {
                _logger.LogError("Failed to delete Hazard with ID: {Id}. Error: {Error}",
                    request.HazardId, result.Error?.Message);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("DeleteHazardCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while deleting Hazard with ID: {Id}", request.HazardId);
            return Result<bool>.Failure<bool>(DomainErrors.HazardError.DeleteFailed);
        }
    }
}