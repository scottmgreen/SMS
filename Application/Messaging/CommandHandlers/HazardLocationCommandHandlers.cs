//-----------------------------------------------------------------------
// <copyright file="HazardLocationCommandHandlers.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Command handlers implementing SMS hazard management and lifecycle logic.
//                  Implements command handlers for processing write operations.
//                  Handles business logic execution and domain entity coordination.
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using SMS_Application.Messaging.CommandHandlers;

// =============================================
// HAZARD LOCATION COMMAND HANDLERS - Clean Architecture Pattern
// =============================================

public class CreateHazardLocationCommandHandler : BaseCommandBundle, IRequestHandler<CreateHazardLocationCommand, Result<HazardLocation>>
{
    private readonly HazardLocationService _hazardLocationService;
    private readonly ILogger<CreateHazardLocationCommandHandler> _logger;

    public CreateHazardLocationCommandHandler(HazardLocationService hazardLocationService, ILogger<CreateHazardLocationCommandHandler> logger)
    {
        _hazardLocationService = hazardLocationService ?? throw new ArgumentNullException(nameof(hazardLocationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<HazardLocation>> HandleAsync(CreateHazardLocationCommand request, CancellationToken ct = default)
    {
        try
        {
            if (request?.HazardLocation is null)
            {
                _logger.LogApplicationError("CreateHazardLocationCommand received with null HazardLocation", ApplicationEventIds.Error, null);
                return Result<HazardLocation>.Failure<HazardLocation>(DomainErrors.HazardLocationError.NullOrEmpty);
            }

            _logger.LogInformation("✅ Clean Architecture: Processing CreateHazardLocationCommand for Code: {Code}", request.HazardLocation.Code);

            var result = await _hazardLocationService.CreateHazardLocationAsync(request.HazardLocation, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("✅ Clean Architecture: Successfully created HazardLocation with ID: {Id}, Code: {Code}",
                    result.Value?.Id, result.Value?.Code);
            }
            else
            {
                _logger.LogApplicationError("Failed to create HazardLocation with Code: {Code}. Error: {Error}",
                    ApplicationEventIds.Error, null);
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
            _logger.LogApplicationError("Unexpected error occurred while creating HazardLocation", ApplicationEventIds.Error, ex);
            return Result<HazardLocation>.Failure<HazardLocation>(DomainErrors.HazardLocationError.CreateFailed);
        }
    }
}

public class UpdateHazardLocationCommandHandler : BaseCommandBundle, IRequestHandler<UpdateHazardLocationCommand, Result<HazardLocation>>
{
    private readonly HazardLocationService _hazardLocationService;
    private readonly ILogger<UpdateHazardLocationCommandHandler> _logger;

    public UpdateHazardLocationCommandHandler(HazardLocationService hazardLocationService, ILogger<UpdateHazardLocationCommandHandler> logger)
    {
        _hazardLocationService = hazardLocationService ?? throw new ArgumentNullException(nameof(hazardLocationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<HazardLocation>> HandleAsync(UpdateHazardLocationCommand request, CancellationToken ct = default)
    {
        try
        {
            if (request?.HazardLocation is null)
            {
                _logger.LogApplicationError("UpdateHazardLocationCommand received with null HazardLocation", ApplicationEventIds.Error, null);
                return Result<HazardLocation>.Failure<HazardLocation>(DomainErrors.HazardLocationError.NullOrEmpty);
            }

            _logger.LogInformation("✅ Clean Architecture: Processing UpdateHazardLocationCommand for ID: {Id}, Code: {Code}",
                request.HazardLocation.Id, request.HazardLocation.Code);

            var result = await _hazardLocationService.UpdateHazardLocationAsync(request.HazardLocation, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("✅ Clean Architecture: Successfully updated HazardLocation with ID: {Id}", request.HazardLocation.Id);
            }
            else
            {
                _logger.LogApplicationError("Failed to update HazardLocation with ID: {Id}. Error: {Error}",
                    ApplicationEventIds.Error, null);
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
            _logger.LogApplicationError("Unexpected error occurred while updating HazardLocation with ID: {Id}", ApplicationEventIds.Error, ex);
            return Result<HazardLocation>.Failure<HazardLocation>(DomainErrors.HazardLocationError.UpdateFailed);
        }
    }
}

public class DeleteHazardLocationCommandHandler : BaseCommandBundle, IRequestHandler<DeleteHazardLocationCommand, Result<bool>>
{
    private readonly HazardLocationService _hazardLocationService;
    private readonly ILogger<DeleteHazardLocationCommandHandler> _logger;

    public DeleteHazardLocationCommandHandler(HazardLocationService hazardLocationService, ILogger<DeleteHazardLocationCommandHandler> logger)
    {
        _hazardLocationService = hazardLocationService ?? throw new ArgumentNullException(nameof(hazardLocationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> HandleAsync(DeleteHazardLocationCommand request, CancellationToken ct = default)
    {
        try
        {
            if (request?.HazardLocationId is null)
            {
                _logger.LogApplicationError("DeleteHazardLocationCommand received with null HazardLocationId", ApplicationEventIds.Error, null);
                return Result<bool>.Failure<bool>(DomainErrors.HazardLocationError.NullOrEmpty);
            }

            _logger.LogInformation("✅ Clean Architecture: Processing DeleteHazardLocationCommand for ID: {Id}", request.HazardLocationId);

            var result = await _hazardLocationService.DeleteHazardLocationAsync(request.HazardLocationId, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("✅ Clean Architecture: Successfully deleted HazardLocation with ID: {Id}", request.HazardLocationId);
            }
            else
            {
                _logger.LogApplicationError("Failed to delete HazardLocation with ID: {Id}. Error: {Error}",
                    ApplicationEventIds.Error, null);
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
            _logger.LogApplicationError("Unexpected error occurred while deleting HazardLocation with ID: {Id}", ApplicationEventIds.Error, ex);
            return Result<bool>.Failure<bool>(DomainErrors.HazardLocationError.DeleteFailed);
        }
    }
}
