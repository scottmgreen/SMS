using Microsoft.Extensions.Logging;

namespace SMS_Application.Messaging.CommandHandlers;

// =============================================
// HAZARD REPORT TRACKING COMMAND HANDLERS
// =============================================

public class CreateHazardReportTrackingCommandHandler : BaseCommandBundle, IRequestHandler<CreateHazardReportTrackingCommand, Result<HazardReportTracking>>
{
    private readonly HazardReportTrackingService _service;
    private readonly ILogger<CreateHazardReportTrackingCommandHandler> _logger;

    public CreateHazardReportTrackingCommandHandler(
        HazardReportTrackingService service,
        ILogger<CreateHazardReportTrackingCommandHandler> logger)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<HazardReportTracking>> HandleAsync(CreateHazardReportTrackingCommand request, CancellationToken ct = default)
    {
        try
        {
            if (request?.HazardReportTracking is null)
            {
                _logger.LogApplicationError("CreateHazardReportTrackingCommand received with null HazardReportTracking", ApplicationEventIds.Error, null);
                return Result<HazardReportTracking>.Failure<HazardReportTracking>(DomainErrors.HazardReportTrackingError.NullOrEmpty);
            }

            _logger.LogInformation("Processing CreateHazardReportTrackingCommand for HazardCode: {HazardCode}, ReportCode: {ReportCode}",
                request.HazardReportTracking.HazardCode, request.HazardReportTracking.ReportCode);

            var result = await _service.CreateHazardReportTrackingAsync(request.HazardReportTracking, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully created HazardReportTracking with TrackingCode: {TrackingCode}",
                    result.Value?.TrackingCode);
            }
            else
            {
                _logger.LogApplicationError("Failed to create HazardReportTracking. Error: {Error}",
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

public class UpdateHazardReportTrackingCommandHandler : BaseCommandBundle, IRequestHandler<UpdateHazardReportTrackingCommand, Result<HazardReportTracking>>
{
    private readonly HazardReportTrackingService _service;
    private readonly ILogger<UpdateHazardReportTrackingCommandHandler> _logger;

    public UpdateHazardReportTrackingCommandHandler(
        HazardReportTrackingService service,
        ILogger<UpdateHazardReportTrackingCommandHandler> logger)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<HazardReportTracking>> HandleAsync(UpdateHazardReportTrackingCommand request, CancellationToken ct = default)
    {
        try
        {
            if (request?.HazardReportTracking is null)
            {
                _logger.LogApplicationError("UpdateHazardReportTrackingCommand received with null HazardReportTracking", ApplicationEventIds.Error, null);
                return Result<HazardReportTracking>.Failure<HazardReportTracking>(DomainErrors.HazardReportTrackingError.NullOrEmpty);
            }

            _logger.LogInformation("Processing UpdateHazardReportTrackingCommand for TrackingCode: {TrackingCode}",
                request.HazardReportTracking.TrackingCode);

            var result = await _service.UpdateHazardReportTrackingAsync(request.HazardReportTracking, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully updated HazardReportTracking with TrackingCode: {TrackingCode}",
                    request.HazardReportTracking.TrackingCode);
            }
            else
            {
                _logger.LogApplicationError("Failed to update HazardReportTracking. Error: {Error}",
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
            _logger.LogApplicationError("Unexpected error occurred while updating HazardReportTracking with TrackingCode: {TrackingCode}",
                ApplicationEventIds.Error, ex);
            return Result<HazardReportTracking>.Failure<HazardReportTracking>(DomainErrors.HazardReportTrackingError.UpdateFailed);
        }
    }
}

public class DeleteHazardReportTrackingCommandHandler : BaseCommandBundle, IRequestHandler<DeleteHazardReportTrackingCommand, Result<bool>>
{
    private readonly HazardReportTrackingService _service;
    private readonly ILogger<DeleteHazardReportTrackingCommandHandler> _logger;

    public DeleteHazardReportTrackingCommandHandler(
        HazardReportTrackingService service,
        ILogger<DeleteHazardReportTrackingCommandHandler> logger)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> HandleAsync(DeleteHazardReportTrackingCommand request, CancellationToken ct = default)
    {
        try
        {
            if (request?.HazardReportTrackingId is null)
            {
                _logger.LogApplicationError("DeleteHazardReportTrackingCommand received with null HazardReportTrackingId", ApplicationEventIds.Error, null);
                return Result<bool>.Failure<bool>(DomainErrors.HazardReportTrackingError.NullOrEmpty);
            }

            _logger.LogInformation("Processing DeleteHazardReportTrackingCommand for ID: {Id}", request.HazardReportTrackingId);

            var result = await _service.DeleteHazardReportTrackingAsync(request.HazardReportTrackingId, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully deleted HazardReportTracking with ID: {Id}", request.HazardReportTrackingId);
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
            _logger.LogApplicationError("Unexpected error occurred while deleting HazardReportTracking with ID: {Id}",
                ApplicationEventIds.Error, ex);
            return Result<bool>.Failure<bool>(DomainErrors.HazardReportTrackingError.DeleteFailed);
        }
    }
}

public class CreateHazardReportWithTrackingCommandHandler : BaseCommandBundle, IRequestHandler<CreateHazardReportWithTrackingCommand, Result<HazardReportTrackingResult>>
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