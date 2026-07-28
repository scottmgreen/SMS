//-----------------------------------------------------------------------
// <copyright file="InvestigationCommandHandlers.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Command handlers implementing SMS investigation workflow and process logic.
//                  Implements command handlers for processing write operations.
//                  Handles business logic execution and domain entity coordination.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;
using SMS_Domain.Enums;
using SMS_Domain.Events;

using Microsoft.Extensions.Logging;

namespace SMS_Application.CommandHandlers;

// =============================================
// INVESTIGATION COMMAND HANDLERS - Clean Architecture Pattern
// =============================================

public class CreateInvestigationCommandHandler : BaseCommandBundle, IBaseRequestHandler<CreateInvestigationCommand, Result<Investigation>>
{
    private readonly InvestigationService _investigationService;
    private readonly ILogger<CreateInvestigationCommandHandler> _logger;

    public CreateInvestigationCommandHandler(InvestigationService investigationService, ILogger<CreateInvestigationCommandHandler> logger)
    {
        _investigationService = investigationService ?? throw new ArgumentNullException(nameof(investigationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<Investigation>> HandleAsync(CreateInvestigationCommand request, CancellationToken ct = default)
    {
        try
        {
            if (request?.Investigation is null)
            {
                _logger.LogApplicationError("CreateInvestigationCommand received with null Investigation", ApplicationEventIds.Error, null);
                return Result<Investigation>.Failure<Investigation>(DomainErrors.InvestigationError.NullOrEmpty);
            }

            _logger.LogApplicationInformation(" Processing CreateInvestigationCommand for Code: {Code}", request.Investigation.Code);

            var result = await _investigationService.CreateInvestigationAsync(request.Investigation, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation(" Successfully created Investigation with ID: {Id}, Code: {Code}",
                    result.Value?.Id, result.Value?.Code);
            }
            else
            {
                _logger.LogApplicationError("Failed to create Investigation with Code: {Code}. Error: {Error}",
                    ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogApplicationWarning("CreateInvestigationCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while creating Investigation", ApplicationEventIds.Error, ex);
            return Result<Investigation>.Failure<Investigation>(DomainErrors.InvestigationError.CreateFailed);
        }
    }
}

public class UpdateInvestigationCommandHandler : BaseCommandBundle, IBaseRequestHandler<UpdateInvestigationCommand, Result<Investigation>>
{
    private readonly InvestigationService _investigationService;
    private readonly IBaseEventBus _eventBus;
    private readonly ILogger<UpdateInvestigationCommandHandler> _logger;

    public UpdateInvestigationCommandHandler(
        InvestigationService investigationService,
        IBaseEventBus eventBus,
        ILogger<UpdateInvestigationCommandHandler> logger)
    {
        _investigationService = investigationService ?? throw new ArgumentNullException(nameof(investigationService));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<Investigation>> HandleAsync(UpdateInvestigationCommand request, CancellationToken ct = default)
    {
        try
        {
            if (request?.Investigation is null)
            {
                _logger.LogApplicationError("UpdateInvestigationCommand received with null Investigation", ApplicationEventIds.Error, null);
                return Result<Investigation>.Failure<Investigation>(DomainErrors.InvestigationError.NullOrEmpty);
            }

            _logger.LogApplicationInformation(" Processing UpdateInvestigationCommand for ID: {Id}, Code: {Code}",
                request.Investigation.Id, request.Investigation.Code);

            InvestigationStatus? previousStatus = null;
            if (!string.IsNullOrWhiteSpace(request.Investigation.Code))
            {
                var existingInvestigationResult = await _investigationService
                    .GetInvestigationByCodeAsync(new InvestigationID(request.Investigation.Code), ct)
                    .ConfigureAwait(false);

                if (existingInvestigationResult.IsSuccess && existingInvestigationResult.Value is not null)
                {
                    previousStatus = existingInvestigationResult.Value.Status;
                }
            }

            var result = await _investigationService.UpdateInvestigationAsync(request.Investigation, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation(" Successfully updated Investigation with ID: {Id}", request.Investigation.Id);

                var updatedInvestigation = result.Value;
                if (updatedInvestigation is not null)
                {
                    await TransitionEventPublisher.PublishIfChangedAsync(
                        _eventBus,
                        previousStatus,
                        updatedInvestigation.Status,
                        () => new InvestigationStatusChangedEvent(
                            id: new SMSEventID("EV-0000"),
                            investigationId: updatedInvestigation.Id.Value,
                            investigationCode: updatedInvestigation.Code,
                            previousStatus: previousStatus!,
                            newStatus: updatedInvestigation.Status,
                            changedBy: updatedInvestigation.UpdatedBy ?? updatedInvestigation.CreatedBy ?? string.Empty,
                            changedDate: updatedInvestigation.UpdatedDate ?? DateTime.UtcNow),
                        ct).ConfigureAwait(false);
                }
            }
            else
            {
                _logger.LogApplicationError("Failed to update Investigation with ID: {Id}. Error: {Error}",
                    ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogApplicationWarning("UpdateInvestigationCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while updating Investigation with ID: {Id}", ApplicationEventIds.Error, ex);
            return Result<Investigation>.Failure<Investigation>(DomainErrors.InvestigationError.UpdateFailed);
        }
    }
}

public class DeleteInvestigationCommandHandler : BaseCommandBundle, IBaseRequestHandler<DeleteInvestigationCommand, Result<bool>>
{
    private readonly InvestigationService _investigationService;
    private readonly ILogger<DeleteInvestigationCommandHandler> _logger;

    public DeleteInvestigationCommandHandler(InvestigationService investigationService, ILogger<DeleteInvestigationCommandHandler> logger)
    {
        _investigationService = investigationService ?? throw new ArgumentNullException(nameof(investigationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> HandleAsync(DeleteInvestigationCommand request, CancellationToken ct = default)
    {
        try
        {
            if (request?.InvestigationId is null)
            {
                _logger.LogApplicationError("DeleteInvestigationCommand received with null InvestigationId", ApplicationEventIds.Error, null);
                return Result<bool>.Failure<bool>(DomainErrors.InvestigationError.NullOrEmpty);
            }

            _logger.LogApplicationInformation(" Processing DeleteInvestigationCommand for ID: {Id}", request.InvestigationId);

            var result = await _investigationService.DeleteInvestigationAsync(request.InvestigationId, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation(" Successfully deleted Investigation with ID: {Id}", request.InvestigationId);
            }
            else
            {
                _logger.LogApplicationError("Failed to delete Investigation with ID: {Id}. Error: {Error}",
                    ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogApplicationWarning("DeleteInvestigationCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while deleting Investigation with ID: {Id}", ApplicationEventIds.Error, ex);
            return Result<bool>.Failure<bool>(DomainErrors.InvestigationError.DeleteFailed);
        }
    }
}

