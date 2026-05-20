//-----------------------------------------------------------------------
// <copyright file="HazardReportTrackingCommandHandlers.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Command handlers implementing SMS hazard report tracking business logic and operations.
//                  Implements command handlers for processing write operations.
//                  Handles business logic execution and domain entity coordination.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;

using Microsoft.Extensions.Logging;

namespace SMS_Application.Messaging.CommandHandlers;

// =============================================
// HAZARD REPORT TRACKING COMMAND HANDLERS - Clean Architecture Pattern
// =============================================

public class CreateHazardReportTrackingCommandHandler : BaseCommandBundle, IBaseRequestHandler<CreateHazardReportTrackingCommand, Result<HazardReportTracking>>
{
    private readonly HazardReportTrackingService _hazardReportTrackingService;
    private readonly ILogger<CreateHazardReportTrackingCommandHandler> _logger;

    public CreateHazardReportTrackingCommandHandler(HazardReportTrackingService hazardReportTrackingService, ILogger<CreateHazardReportTrackingCommandHandler> logger)
    {
        _hazardReportTrackingService = hazardReportTrackingService ?? throw new ArgumentNullException(nameof(hazardReportTrackingService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<HazardReportTracking>> HandleAsync(CreateHazardReportTrackingCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request?.HazardReportTracking is null)
            {
                _logger.LogApplicationError("CreateHazardReportTrackingCommand received with null request or tracking", ApplicationEventIds.Error, null);
                return Result<HazardReportTracking>.Failure<HazardReportTracking>(DomainErrors.HazardReportTrackingError.NullOrEmpty);
            }

            _logger.LogInformation(" Processing CreateHazardReportTrackingCommand for ID: {Id}", request.HazardReportTracking.Id);

            var result = await _hazardReportTrackingService.CreateHazardReportTrackingAsync(request.HazardReportTracking, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation(" Successfully created HazardReportTracking with ID: {Id}",
                    result.Value?.Id);
            }
            else
            {
                _logger.LogApplicationError("Failed to create HazardReportTracking with TrackingId: {TrackingId}. Error: {Error}",
                    ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("CreateHazardReportTrackingCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while creating HazardReportTracking", ApplicationEventIds.Error, ex);
            return Result<HazardReportTracking>.Failure<HazardReportTracking>(DomainErrors.HazardReportTrackingError.CreateFailed);
        }
    }
}

public class UpdateHazardReportTrackingCommandHandler : BaseCommandBundle, IBaseRequestHandler<UpdateHazardReportTrackingCommand, Result<HazardReportTracking>>
{
    private readonly HazardReportTrackingService _hazardReportTrackingService;
    private readonly ILogger<UpdateHazardReportTrackingCommandHandler> _logger;

    public UpdateHazardReportTrackingCommandHandler(HazardReportTrackingService hazardReportTrackingService, ILogger<UpdateHazardReportTrackingCommandHandler> logger)
    {
        _hazardReportTrackingService = hazardReportTrackingService ?? throw new ArgumentNullException(nameof(hazardReportTrackingService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<HazardReportTracking>> HandleAsync(UpdateHazardReportTrackingCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request?.HazardReportTracking is null)
            {
                _logger.LogApplicationError("UpdateHazardReportTrackingCommand received with null request or tracking", ApplicationEventIds.Error, null);
                return Result<HazardReportTracking>.Failure<HazardReportTracking>(DomainErrors.HazardReportTrackingError.NullOrEmpty);
            }

            _logger.LogInformation(" Processing UpdateHazardReportTrackingCommand for ID: {Id}", request.HazardReportTracking.Id);

            var result = await _hazardReportTrackingService.UpdateHazardReportTrackingAsync(request.HazardReportTracking, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation(" Successfully updated HazardReportTracking with ID: {Id}", request.HazardReportTracking.Id);
            }
            else
            {
                _logger.LogApplicationError("Failed to update HazardReportTracking with ID: {Id}. Error: {Error}",
                    ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("UpdateHazardReportTrackingCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while updating HazardReportTracking with ID: {Id}", ApplicationEventIds.Error, ex);
            return Result<HazardReportTracking>.Failure<HazardReportTracking>(DomainErrors.HazardReportTrackingError.UpdateFailed);
        }
    }
}

public class DeleteHazardReportTrackingCommandHandler : BaseCommandBundle, IBaseRequestHandler<DeleteHazardReportTrackingCommand, Result<bool>>
{
    private readonly HazardReportTrackingService _hazardReportTrackingService;
    private readonly ILogger<DeleteHazardReportTrackingCommandHandler> _logger;

    public DeleteHazardReportTrackingCommandHandler(HazardReportTrackingService hazardReportTrackingService, ILogger<DeleteHazardReportTrackingCommandHandler> logger)
    {
        _hazardReportTrackingService = hazardReportTrackingService ?? throw new ArgumentNullException(nameof(hazardReportTrackingService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> HandleAsync(DeleteHazardReportTrackingCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request is null)
            {
                _logger.LogApplicationError("DeleteHazardReportTrackingCommand received with null request", ApplicationEventIds.Error, null);
                return Result<bool>.Failure<bool>(DomainErrors.HazardReportTrackingError.NullOrEmpty);
            }

            _logger.LogInformation(" Processing DeleteHazardReportTrackingCommand for ID: {Id}", request.HazardReportTrackingId);

            var result = await _hazardReportTrackingService.DeleteHazardReportTrackingAsync(request.HazardReportTrackingId, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation(" Successfully deleted HazardReportTracking with ID: {Id}", request.HazardReportTrackingId);
            }
            else
            {
                _logger.LogApplicationError("Failed to delete HazardReportTracking with ID: {Id}. Error: {Error}",
                    ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("DeleteHazardReportTrackingCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while deleting HazardReportTracking with ID: {Id}", ApplicationEventIds.Error, ex);
            return Result<bool>.Failure<bool>(DomainErrors.HazardReportTrackingError.DeleteFailed);
        }
    }
}

public class CreateHazardReportWithTrackingCommandHandler : BaseCommandBundle, IBaseRequestHandler<CreateHazardReportWithTrackingCommand, Result<HazardReportTrackingResult>>
{
    private readonly HazardReportTrackingService _service;
    private readonly ILogger<CreateHazardReportWithTrackingCommandHandler> _logger;

    public CreateHazardReportWithTrackingCommandHandler(
        HazardReportTrackingService service,
        ILogger<CreateHazardReportWithTrackingCommandHandler> logger)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<HazardReportTrackingResult>> HandleAsync(CreateHazardReportWithTrackingCommand request, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request?.HazardCode) || string.IsNullOrWhiteSpace(request?.ReportCode))
            {
                _logger.LogApplicationError("CreateHazardReportWithTrackingCommand received with invalid parameters", ApplicationEventIds.Error, null);
                return Result<HazardReportTrackingResult>.Failure<HazardReportTrackingResult>(DomainErrors.HazardReportTrackingError.NullOrEmpty);
            }

            _logger.LogInformation("Processing CreateHazardReportWithTrackingCommand for HazardCode: {HazardCode}, ReportCode: {ReportCode}",
                request.HazardCode, request.ReportCode);

            var result = await _service.CreateHazardReportWithTrackingAsync(request.HazardCode, request.ReportCode, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully created HazardReport with TrackingCode: {TrackingCode}",
                    result.Value?.TrackingCode);
            }
            else
            {
                _logger.LogApplicationError("Failed to create HazardReport with tracking. Error: {Error}",
                    ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("CreateHazardReportWithTrackingCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while creating HazardReport with tracking", ApplicationEventIds.Error, ex);
            return Result<HazardReportTrackingResult>.Failure<HazardReportTrackingResult>(DomainErrors.HazardReportTrackingError.CreateFailed);
        }
    }
}
