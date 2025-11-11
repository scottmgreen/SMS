using Microsoft.Extensions.Logging;
using SMS_Infrastructure.Services;
using SMS_Application.Messaging.Commands;
using SMS_Domain.Entities;
using SMS_Domain.Errors;
using SMS_Application.Interfaces;
using SMS_Shared.Common;

// =============================================
// INTERVIEW COMMAND HANDLERS
// =============================================

public class CreateHazardLocationCommandHandler : BaseCommandBundle, IRequestHandler<CreateHazardLocationCommand, Result<HazardLocation>>
{
    private readonly HazardLocationDataService _dataService;
    private readonly ILogger<CreateHazardLocationCommandHandler> _logger;

    public CreateHazardLocationCommandHandler(HazardLocationDataService dataService, ILogger<CreateHazardLocationCommandHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<HazardLocation>> HandleAsync(CreateHazardLocationCommand request, CancellationToken ct = default)
    {
        try
        {
            if (request?.HazardLocation is null)
            {
                _logger.LogError("CreateHazardLocationCommand received with null Interview");
                return Result<HazardLocation>.Failure<HazardLocation>(DomainErrors.HazardLocationError.NullOrEmpty);
            }

            _logger.LogInformation("Processing CreateHazardLocationCommand for Code: {Code}", request.HazardLocation.Code);

            var result = await _dataService.CreateHazardLocationAsync(request.HazardLocation, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully created HazardLocation with ID: {Id}, Code: {Code}",
                    result.Value?.Id, result.Value?.Code);
            }
            else
            {
                _logger.LogError("Failed to create HazardLocation with Code: {Code}. Error: {Error}",
                    request.HazardLocation.Code, result.Error?.Message);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("CreateHazardLocationCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while creating HazardLocation");
            return Result<HazardLocation>.Failure<HazardLocation>(DomainErrors.HazardLocationError.CreateFailed);
        }
    }
}

public class UpdateHazardLocationCommandHandler : BaseCommandBundle, IRequestHandler<UpdateHazardLocationCommand, Result<HazardLocation>>
{
    private readonly HazardLocationDataService _dataService;
    private readonly ILogger<UpdateHazardLocationCommandHandler> _logger;

    public UpdateHazardLocationCommandHandler(HazardLocationDataService dataService, ILogger<UpdateHazardLocationCommandHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<HazardLocation>> HandleAsync(UpdateHazardLocationCommand request, CancellationToken ct = default)
    {
        try
        {
            if (request?.HazardLocation is null)
            {
                _logger.LogError("UpdateInterviewCommand received with null Interview");
                return Result<HazardLocation>.Failure<HazardLocation>(DomainErrors.HazardLocationError.NullOrEmpty);
            }

            _logger.LogInformation("Processing UpdateInterviewCommand for ID: {Id}, Code: {Code}",
                request.HazardLocation.Id, request.HazardLocation.Code);

            var result = await _dataService.UpdateHazardLocationAsync(request.HazardLocation, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully updated HazardLocation with ID: {Id}", request.HazardLocation.Id);
            }
            else
            {
                _logger.LogError("Failed to update HazardLocation with ID: {Id}. Error: {Error}",
                    request.HazardLocation.Id, result.Error?.Message);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("UpdateHazardLocationCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while updating Interview with ID: {Id}", request.HazardLocation?.Id);
            return Result<HazardLocation>.Failure<HazardLocation>(DomainErrors.HazardLocationError.UpdateFailed);
        }
    }
}

public class DeleteHazardLocationCommandHandler : BaseCommandBundle, IRequestHandler<DeleteHazardLocationCommand, Result<bool>>
{
    private readonly HazardLocationDataService _dataService;
    private readonly ILogger<DeleteHazardLocationCommandHandler> _logger;

    public DeleteHazardLocationCommandHandler(HazardLocationDataService dataService, ILogger<DeleteHazardLocationCommandHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> HandleAsync(DeleteHazardLocationCommand request, CancellationToken ct = default)
    {
        try
        {
            if (request?.HazardLocationId is null)
            {
                _logger.LogError("DeleteInterviewCommand received with null HazardLocationId");
                return Result<bool>.Failure<bool>(DomainErrors.HazardLocationError.NullOrEmpty);
            }

            _logger.LogInformation("Processing DeleteHazardLocationCommand for ID: {Id}", request.HazardLocationId);

            var result = await _dataService.DeleteHazardLocationAsync(request.HazardLocationId, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully deleted HazardLocation with ID: {Id}", request.HazardLocationId);
            }
            else
            {
                _logger.LogError("Failed to delete Interview with ID: {Id}. Error: {Error}",
                    request.HazardLocationId, result.Error?.Message);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("DeleteHazardLocationCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while deleting HazardLocation with ID: {Id}", request.HazardLocationId);
            return Result<bool>.Failure<bool>(DomainErrors.HazardLocationError.DeleteFailed);
        }
    }
}