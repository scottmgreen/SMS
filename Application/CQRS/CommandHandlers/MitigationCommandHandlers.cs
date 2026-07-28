//-----------------------------------------------------------------------
// <copyright file="MitigationCommandHandlers.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Command handlers implementing SMS mitigation management business logic and operations.
//                  Implements command handlers for processing write operations.
//                  Handles business logic execution and domain entity coordination.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;
using SMS_Domain.Enums;
using SMS_Domain.Events;
using SMS_Application.Queries;

using Microsoft.Extensions.Logging;

namespace SMS_Application.CommandHandlers;

// =============================================
// MITIGATION COMMAND HANDLERS - Clean Architecture Pattern
// =============================================

public class CreateMitigationCommandHandler : BaseCommandBundle, IBaseRequestHandler<CreateMitigationCommand, Result<Mitigation>>
{
    private readonly IMitigationService _mitigationService;
    private readonly ILogger<CreateMitigationCommandHandler> _logger;

    public CreateMitigationCommandHandler(
        IMitigationService mitigationService,
        ILogger<CreateMitigationCommandHandler> logger)
    {
        _mitigationService = mitigationService ?? throw new ArgumentNullException(nameof(mitigationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<Mitigation>> HandleAsync(CreateMitigationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request?.Mitigation is null)
            {
                _logger.LogApplicationError("CreateMitigationCommand received with null request or mitigation", ApplicationEventIds.Error, null);
                return Result<Mitigation>.Failure<Mitigation>(DomainErrors.MitigationError.NullOrEmpty);
            }

            _logger.LogApplicationInformation(" Processing CreateMitigationCommand for Code: {Code}", request.Mitigation.Code);

            var result = await _mitigationService.CreateMitigationAsync(request.Mitigation, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation(" Successfully created Mitigation with ID: {Id}, Code: {Code}",
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
            _logger.LogApplicationWarning("CreateMitigationCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while creating Mitigation", ApplicationEventIds.Error, ex);
            return Result<Mitigation>.Failure<Mitigation>(DomainErrors.MitigationError.CreateFailed);
        }
    }

}

public class UpdateMitigationCommandHandler : BaseCommandBundle, IBaseRequestHandler<UpdateMitigationCommand, Result<Mitigation>>
{
    private readonly IMitigationService _mitigationService;
    private readonly IBaseMediator _mediator;
    private readonly IBaseEventBus _eventBus;
    private readonly ILogger<UpdateMitigationCommandHandler> _logger;

    public UpdateMitigationCommandHandler(
        IMitigationService mitigationService,
        IBaseMediator mediator,
        IBaseEventBus eventBus,
        ILogger<UpdateMitigationCommandHandler> logger)
    {
        _mitigationService = mitigationService ?? throw new ArgumentNullException(nameof(mitigationService));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<Mitigation>> HandleAsync(UpdateMitigationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request?.Mitigation is null)
            {
                _logger.LogApplicationError("UpdateMitigationCommand received with null request or mitigation", ApplicationEventIds.Error, null);
                return Result<Mitigation>.Failure<Mitigation>(DomainErrors.MitigationError.NullOrEmpty);
            }

            _logger.LogApplicationInformation(" Processing UpdateMitigationCommand for ID: {Id}", request.Mitigation.Id);

            MitigationStatus? previousStatus = null;
            var mitigationLookupCode = !string.IsNullOrWhiteSpace(request.Mitigation.Code)
                ? request.Mitigation.Code.Trim()
                : request.Mitigation.Id?.Value?.Trim();

            if (!string.IsNullOrWhiteSpace(mitigationLookupCode))
            {
                var existingMitigationResult = await _mitigationService
                    .GetMitigationByCodeAsync(new MitigationID(mitigationLookupCode), cancellationToken)
                    .ConfigureAwait(false);

                if (existingMitigationResult.IsSuccess && existingMitigationResult.Value is not null)
                {
                    previousStatus = existingMitigationResult.Value.Status;
                }
            }

            var result = await _mitigationService.UpdateMitigationAsync(request.Mitigation, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation(" Successfully updated Mitigation with ID: {Id}", request.Mitigation.Id);

                var updatedMitigation = result.Value;
                var reportId = string.Empty;

                if (updatedMitigation is not null && !string.IsNullOrWhiteSpace(updatedMitigation.HazardCode))
                {
                    var hazardResult = await _mediator
                        .SendAsync(new GetHazardByCodeQuery(new HazardID(updatedMitigation.HazardCode)), cancellationToken)
                        .ConfigureAwait(false);

                    if (hazardResult.IsSuccess && hazardResult.Value is not null)
                    {
                        reportId = hazardResult.Value.ReportCode ?? string.Empty;
                    }
                }
                
                if (updatedMitigation is not null)
                {
                    var previousStatusValue = previousStatus?.Value?.Trim() ?? string.Empty;
                    var currentStatusValue = updatedMitigation.Status.Value?.Trim() ?? string.Empty;
                    var statusChanged = !string.IsNullOrWhiteSpace(currentStatusValue)
                        && !string.Equals(previousStatusValue, currentStatusValue, StringComparison.OrdinalIgnoreCase);

                    if (statusChanged)
                    {
                        var statusChangedEvent = new MitigationStatusChangedEvent(
                            id: new SMSEventID("EV-0000"),
                            mitigationId: (updatedMitigation.Code ?? updatedMitigation.Id.Value ?? string.Empty).Trim(),
                            status: currentStatusValue,
                            changedBy: updatedMitigation.UpdatedBy ?? updatedMitigation.CreatedBy ?? string.Empty,
                            changedDate: updatedMitigation.UpdatedDate ?? DateTime.UtcNow)
                        {
                            ReportId = reportId
                        };

                        var statusPublishResult = await _eventBus.PublishDomainEventAsync(statusChangedEvent, cancellationToken).ConfigureAwait(false);
                        if (statusPublishResult.IsFailure)
                        {
                            _logger.LogApplicationWarning(
                                "Failed to publish MitigationStatusChanged event for {MitigationCode}: {Error}",
                                updatedMitigation.Code,
                                statusPublishResult.Error?.Message ?? "Unknown publish error");
                        }

                        if (string.Equals(currentStatusValue, MitigationStatus.Complete.Value, StringComparison.OrdinalIgnoreCase))
                        {
                            var mitigationCode = (updatedMitigation.Code ?? updatedMitigation.Id.Value ?? string.Empty).Trim();
                            var completedDate = updatedMitigation.UpdatedDate ?? DateTime.UtcNow;
                            var completedBy = updatedMitigation.ApprovedBy ?? updatedMitigation.UpdatedBy ?? updatedMitigation.CreatedBy ?? string.Empty;

                            var mitigationCompletedEvent = new MitigationCompletedEvent(
                                new SMSEventID("EV-0000"),
                                mitigationCode,
                                mitigationCode,
                                updatedMitigation.HazardCode ?? string.Empty,
                                updatedMitigation.TargetDate ?? completedDate.AddDays(30),
                                completedDate,
                                string.Empty)
                            {
                                ReportId = reportId,
                                CompletionNotes = $"Completed by {completedBy}",
                                EffectivenessRating = "Approved"
                            };

                            var completedPublishResult = await _eventBus.PublishDomainEventAsync(mitigationCompletedEvent, cancellationToken).ConfigureAwait(false);
                            if (completedPublishResult.IsFailure)
                            {
                                _logger.LogApplicationWarning(
                                    "Failed to publish MitigationCompleted event for {MitigationCode}: {Error}",
                                    updatedMitigation.Code,
                                    completedPublishResult.Error?.Message ?? "Unknown publish error");
                            }
                        }
                    }
                }
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
            _logger.LogApplicationWarning("UpdateMitigationCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while updating Mitigation with ID: {Id}", ApplicationEventIds.Error, ex);
            return Result<Mitigation>.Failure<Mitigation>(DomainErrors.MitigationError.UpdateFailed);
        }
    }
}

public class DeleteMitigationCommandHandler : BaseCommandBundle, IBaseRequestHandler<DeleteMitigationCommand, Result<bool>>
{
    private readonly IMitigationService _mitigationService;
    private readonly ILogger<DeleteMitigationCommandHandler> _logger;

    public DeleteMitigationCommandHandler(IMitigationService mitigationService, ILogger<DeleteMitigationCommandHandler> logger)
    {
        _mitigationService = mitigationService ?? throw new ArgumentNullException(nameof(mitigationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> HandleAsync(DeleteMitigationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request is null)
            {
                _logger.LogApplicationError("DeleteMitigationCommand received with null request", ApplicationEventIds.Error, null);
                return Result<bool>.Failure<bool>(DomainErrors.MitigationError.NullOrEmpty);
            }

            _logger.LogApplicationInformation(" Processing DeleteMitigationCommand for ID: {Id}", request.MitigationId);

            var result = await _mitigationService.DeleteMitigationAsync(request.MitigationId, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation(" Successfully deleted Mitigation with ID: {Id}", request.MitigationId);
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
            _logger.LogApplicationWarning("DeleteMitigationCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while deleting Mitigation with ID: {Id}", ApplicationEventIds.Error, ex);
            return Result<bool>.Failure<bool>(DomainErrors.MitigationError.DeleteFailed);
        }
    }
}

