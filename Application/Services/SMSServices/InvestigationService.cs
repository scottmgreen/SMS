//-----------------------------------------------------------------------
// <copyright file="InvestigationService.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: SMS Investigation workflow service managing safety investigation processes.
//                  Provides business logic operations and coordinates domain entities
//                  through the CQRS pattern via Mediator services.
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using SMS_Domain.Entities;
using SMS_Application.Interfaces;
using SMS_Domain.Events;
using SMS_Domain.Enums;

namespace SMS_Application.Services;

public sealed class InvestigationService
{
    private readonly InvestigationDataService _dataService;
    private readonly IBaseEventBus _eventBus;
    private readonly ILogger<InvestigationService> _logger;

    public InvestigationService(
        InvestigationDataService dataService,
        IBaseEventBus eventBus,
        ILogger<InvestigationService> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<Investigation>> CreateInvestigationAsync(Investigation investigation, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Creating investigation with code: {Code}", investigation?.Code);
            var result = await _dataService.CreateInvestigationAsync(investigation, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation("Successfully created investigation with ID: {Id}", result.Value?.Id);
            }
            else
            {
                _logger.LogApplicationError("Failed to create investigation. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error creating investigation");
            return Result<Investigation>.Failure<Investigation>(DomainErrors.InvestigationError.CreateFailed);
        }
    }

    public async Task<Result<Investigation>> GetInvestigationByCodeAsync(InvestigationID code, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Retrieving investigation with Code: {Code}", code.Value);
            return await _dataService.GetInvestigationByCodeAsync(code.Value, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error retrieving investigation with Code: {Code}", code.Value);
            return Result<Investigation>.Failure<Investigation>(DomainErrors.InvestigationError.NotFound);
        }
    }

    public async Task<Result<List<Investigation>>> GetAllInvestigationsAsync(CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Retrieving all investigations");
            return await _dataService.GetAllInvestigationsAsync(ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error retrieving all investigations");
            return Result<List<Investigation>>.Failure<List<Investigation>>(DomainErrors.InvestigationError.NullOrEmpty);
        }
    }

    public async Task<Result<Investigation>> UpdateInvestigationAsync(Investigation investigation, CancellationToken ct = default)
    {
        try
        {
            InvestigationStatus? previousStatus = null;
            if (investigation is not null && !string.IsNullOrWhiteSpace(investigation.Code))
            {
                var existingResult = await _dataService.GetInvestigationByCodeAsync(investigation.Code, ct).ConfigureAwait(false);
                if (existingResult.IsSuccess && existingResult.Value is not null)
                {
                    previousStatus = existingResult.Value.Status;
                }
            }

            _logger.LogApplicationInformation("Updating investigation with ID: {Id}", investigation?.Id);
            var result = await _dataService.UpdateInvestigationAsync(investigation, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation("Successfully updated investigation with ID: {Id}", investigation?.Id);

                if (investigation is not null)
                {
                    await TransitionEventPublisher.PublishIfChangedAsync(
                        _eventBus,
                        previousStatus,
                        investigation.Status,
                        () => new InvestigationStatusChangedEvent(
                            id: new SMSEventID(Guid.NewGuid().ToString()),
                            investigationId: investigation.Id.Value,
                            investigationCode: investigation.Code,
                            previousStatus: previousStatus!,
                            newStatus: investigation.Status,
                            changedBy: investigation.UpdatedBy ?? "SYSTEM",
                            changedDate: investigation.UpdatedDate ?? DateTime.UtcNow),
                        ct).ConfigureAwait(false);
                }
            }
            else
            {
                _logger.LogApplicationError("Failed to update investigation. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error updating investigation with ID: {Id}", investigation?.Id);
            return Result<Investigation>.Failure<Investigation>(DomainErrors.InvestigationError.UpdateFailed);
        }
    }

    public async Task<Result<bool>> DeleteInvestigationAsync(InvestigationID id, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Deleting investigation with ID: {Id}", id);
            var result = await _dataService.DeleteInvestigationAsync(id, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation("Successfully deleted investigation with ID: {Id}", id);
            }
            else
            {
                _logger.LogApplicationError("Failed to delete investigation. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error deleting investigation with ID: {Id}", id);
            return Result<bool>.Failure<bool>(DomainErrors.InvestigationError.DeleteFailed);
        }
    }
}

