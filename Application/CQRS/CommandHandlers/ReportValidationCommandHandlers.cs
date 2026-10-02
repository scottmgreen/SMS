//-----------------------------------------------------------------------
// <copyright file="ReportValidationCommandHandlers.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Command handlers implementing SMS report validation business logic and operations.
//                  Implements command handlers for processing write operations.
//                  Handles business logic execution and domain entity coordination.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;
using SMS_Domain.Events;

using Microsoft.Extensions.Logging;
using SMS_Application.Interfaces;

namespace SMS_Application.CommandHandlers;

// =============================================
// REPORT VALIDATION COMMAND HANDLERS - Clean Architecture Pattern
// =============================================

public class CreateReportValidationCommandHandler : BaseCommandBundle, IBaseRequestHandler<CreateReportValidationCommand, Result<ReportValidation>>
{
    private readonly ReportValidationService _reportValidationService;
    private readonly ReportService _reportService;
    private readonly IBaseEventBus _eventBus;
    private readonly ILogger<CreateReportValidationCommandHandler> _logger;

    public CreateReportValidationCommandHandler(ReportValidationService reportValidationService, ReportService reportService, IBaseEventBus eventBus, ILogger<CreateReportValidationCommandHandler> logger)
    {
        _reportValidationService = reportValidationService ?? throw new ArgumentNullException(nameof(reportValidationService));
        _reportService = reportService ?? throw new ArgumentNullException(nameof(reportService));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

public class ResetReportValidationCommandHandler : BaseCommandBundle, IBaseRequestHandler<ResetReportValidationCommand, Result<bool>>
{
    private readonly ReportValidationService _reportValidationService;
    private readonly ILogger<ResetReportValidationCommandHandler> _logger;

    public ResetReportValidationCommandHandler(ReportValidationService reportValidationService, ILogger<ResetReportValidationCommandHandler> logger)
    {
        _reportValidationService = reportValidationService ?? throw new ArgumentNullException(nameof(reportValidationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> HandleAsync(ResetReportValidationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request is null)
            {
                _logger.LogApplicationError("ResetReportValidationCommand received with null request", ApplicationEventIds.Error, null);
                return Result<bool>.Failure<bool>(DomainErrors.ReportValidationError.NullOrEmpty);
            }

            _logger.LogApplicationInformation("Processing ResetReportValidationCommand for ID: {Id}", request.ReportValidationId);

            var result = await _reportValidationService.ResetReportValidationAsync(request.ReportValidationId, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation("Successfully reset Report Validation with ID: {Id}", request.ReportValidationId);
            }
            else
            {
                _logger.LogApplicationError("Failed to reset Report Validation with ID: {Id}. Error: {Error}",
                    ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogApplicationWarning("ResetReportValidationCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while resetting Report Validation with ID: {Id}", ApplicationEventIds.Error, ex);
            return Result<bool>.Failure<bool>(DomainErrors.ReportValidationError.UpdateFailed);
        }
    }
}

    public async Task<Result<ReportValidation>> HandleAsync(CreateReportValidationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request?.ReportValidation is null)
            {
                _logger.LogApplicationError("CreateReportValidationCommand received with null request or report validation", ApplicationEventIds.Error, null);
                return Result<ReportValidation>.Failure<ReportValidation>(DomainErrors.ReportValidationError.NullOrEmpty);
            }

            _logger.LogApplicationInformation(" Processing CreateReportValidationCommand for Code: {Code}", request.ReportValidation.Code);

            var result = await _reportValidationService.CreateReportValidationAsync(request.ReportValidation, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation(" Successfully created Report Validation with ID: {Id}, Code: {Code}",
                    result.Value?.Id, result.Value?.Code);

                var validationEvent = new ValidationDecisionMadeEvent(
                    new SMSEventID("EV-0000"),
                    result.Value?.ReportCode ?? request.ReportValidation.ReportCode,
                    result.Value?.ValidationDecision ?? request.ReportValidation.ValidationDecision,
                    result.Value?.ValidatedDate ?? request.ReportValidation.ValidatedDate ?? DateTime.Now)
                {
                    ValidatedBy = result.Value?.ValidatedBy ?? request.ReportValidation.ValidatedBy ?? string.Empty,
                    ValidationComments = result.Value?.ValidationComments ?? request.ReportValidation.ValidationComments ?? string.Empty
                };

                var publishResult = await _eventBus.PublishDomainEventAsync(validationEvent, cancellationToken);
                if (publishResult.IsFailure)
                {
                    _logger.LogApplicationWarning("Validation decision event publish failed for validation {ValidationCode}: {Error}",
                        result.Value?.Code ?? request.ReportValidation.Code,
                        publishResult.Error?.Message ?? "Unknown publish error");
                }

                await StampParentReportUpdatedDateAsync(
                    result.Value?.ReportCode ?? request.ReportValidation.ReportCode,
                    result.Value?.UpdatedBy ?? result.Value?.ValidatedBy ?? request.ReportValidation.ValidatedBy ?? request.ReportValidation.CreatedBy,
                    cancellationToken).ConfigureAwait(false);
            }
            else
            {
                _logger.LogApplicationError("Failed to create Report Validation with Code: {Code}. Error: {Error}",
                    ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogApplicationWarning("CreateReportValidationCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while creating Report Validation", ApplicationEventIds.Error, ex);
            return Result<ReportValidation>.Failure<ReportValidation>(DomainErrors.ReportValidationError.CreateFailed);
        }
    }

    private async Task StampParentReportUpdatedDateAsync(string? reportCode, string? updatedBy, CancellationToken cancellationToken)
    {
        var normalizedReportCode = reportCode?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(normalizedReportCode))
        {
            return;
        }

        var reportResult = await _reportService.GetReportByCodeAsync(new ReportID(normalizedReportCode), cancellationToken).ConfigureAwait(false);
        if (reportResult.IsFailure || reportResult.Value is null)
        {
            _logger.LogApplicationWarning("Failed loading parent report {ReportCode} for ReportValidation create stamp.", normalizedReportCode);
            return;
        }

        var report = reportResult.Value;
        report.UpdatedBy = !string.IsNullOrWhiteSpace(updatedBy)
            ? updatedBy.Trim()
            : (report.UpdatedBy ?? report.CreatedBy ?? string.Empty);
        report.UpdatedDate = DateTime.Now;

        var updateResult = await _reportService.UpdateReportAsync(report, cancellationToken).ConfigureAwait(false);
        if (updateResult.IsFailure)
        {
            _logger.LogApplicationWarning("Failed updating parent report timestamp for report {ReportCode} after ReportValidation create.", normalizedReportCode);
        }
    }
}

public class UpdateReportValidationCommandHandler : BaseCommandBundle, IBaseRequestHandler<UpdateReportValidationCommand, Result<ReportValidation>>
{
    private readonly ReportValidationService _reportValidationService;
    private readonly ReportService _reportService;
    private readonly IBaseEventBus _eventBus;
    private readonly ILogger<UpdateReportValidationCommandHandler> _logger;

    public UpdateReportValidationCommandHandler(ReportValidationService reportValidationService, ReportService reportService, IBaseEventBus eventBus, ILogger<UpdateReportValidationCommandHandler> logger)
    {
        _reportValidationService = reportValidationService ?? throw new ArgumentNullException(nameof(reportValidationService));
        _reportService = reportService ?? throw new ArgumentNullException(nameof(reportService));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<ReportValidation>> HandleAsync(UpdateReportValidationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request?.ReportValidation is null)
            {
                _logger.LogApplicationError("UpdateReportValidationCommand received with null request or report validation", ApplicationEventIds.Error, null);
                return Result<ReportValidation>.Failure<ReportValidation>(DomainErrors.ReportValidationError.NullOrEmpty);
            }

            _logger.LogApplicationInformation(" Processing UpdateReportValidationCommand for ID: {Id}", request.ReportValidation.Id);

            var result = await _reportValidationService.UpdateReportValidationAsync(request.ReportValidation, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation(" Successfully updated Report Validation with ID: {Id}", request.ReportValidation.Id);

                var validationEvent = new ValidationDecisionMadeEvent(
                    new SMSEventID("EV-0000"),
                    result.Value?.ReportCode ?? request.ReportValidation.ReportCode,
                    result.Value?.ValidationDecision ?? request.ReportValidation.ValidationDecision,
                    result.Value?.ValidatedDate ?? request.ReportValidation.ValidatedDate ?? DateTime.Now)
                {
                    ValidatedBy = result.Value?.ValidatedBy ?? request.ReportValidation.ValidatedBy ?? string.Empty,
                    ValidationComments = result.Value?.ValidationComments ?? request.ReportValidation.ValidationComments ?? string.Empty
                };

                var publishResult = await _eventBus.PublishDomainEventAsync(validationEvent, cancellationToken);
                if (publishResult.IsFailure)
                {
                    _logger.LogApplicationWarning("Validation decision event publish failed for validation {ValidationCode}: {Error}",
                        result.Value?.Code ?? request.ReportValidation.Code,
                        publishResult.Error?.Message ?? "Unknown publish error");
                }

                await StampParentReportUpdatedDateAsync(
                    result.Value?.ReportCode ?? request.ReportValidation.ReportCode,
                    result.Value?.UpdatedBy ?? result.Value?.ValidatedBy ?? request.ReportValidation.ValidatedBy ?? request.ReportValidation.UpdatedBy,
                    cancellationToken).ConfigureAwait(false);
            }
            else
            {
                _logger.LogApplicationError("Failed to update Report Validation with ID: {Id}. Error: {Error}",
                    ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogApplicationWarning("UpdateReportValidationCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while updating Report Validation with ID: {Id}", ApplicationEventIds.Error, ex);
            return Result<ReportValidation>.Failure<ReportValidation>(DomainErrors.ReportValidationError.UpdateFailed);
        }
    }

    private async Task StampParentReportUpdatedDateAsync(string? reportCode, string? updatedBy, CancellationToken cancellationToken)
    {
        var normalizedReportCode = reportCode?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(normalizedReportCode))
        {
            return;
        }

        var reportResult = await _reportService.GetReportByCodeAsync(new ReportID(normalizedReportCode), cancellationToken).ConfigureAwait(false);
        if (reportResult.IsFailure || reportResult.Value is null)
        {
            _logger.LogApplicationWarning("Failed loading parent report {ReportCode} for ReportValidation update stamp.", normalizedReportCode);
            return;
        }

        var report = reportResult.Value;
        report.UpdatedBy = !string.IsNullOrWhiteSpace(updatedBy)
            ? updatedBy.Trim()
            : (report.UpdatedBy ?? report.CreatedBy ?? string.Empty);
        report.UpdatedDate = DateTime.Now;

        var updateResult = await _reportService.UpdateReportAsync(report, cancellationToken).ConfigureAwait(false);
        if (updateResult.IsFailure)
        {
            _logger.LogApplicationWarning("Failed updating parent report timestamp for report {ReportCode} after ReportValidation update.", normalizedReportCode);
        }
    }
}

public class DeleteReportValidationCommandHandler : BaseCommandBundle, IBaseRequestHandler<DeleteReportValidationCommand, Result<bool>>
{
    private readonly ReportValidationService _reportValidationService;
    private readonly ReportService _reportService;
    private readonly ILogger<DeleteReportValidationCommandHandler> _logger;

    public DeleteReportValidationCommandHandler(ReportValidationService reportValidationService, ReportService reportService, ILogger<DeleteReportValidationCommandHandler> logger)
    {
        _reportValidationService = reportValidationService ?? throw new ArgumentNullException(nameof(reportValidationService));
        _reportService = reportService ?? throw new ArgumentNullException(nameof(reportService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> HandleAsync(DeleteReportValidationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request is null)
            {
                _logger.LogApplicationError("DeleteReportValidationCommand received with null request", ApplicationEventIds.Error, null);
                return Result<bool>.Failure<bool>(DomainErrors.ReportValidationError.NullOrEmpty);
            }

            _logger.LogApplicationInformation(" Processing DeleteReportValidationCommand for ID: {Id}", request.ReportValidationId);

            var existingValidationResult = await _reportValidationService
                .GetReportValidationByIdAsync(request.ReportValidationId, cancellationToken)
                .ConfigureAwait(false);

            var parentReportCode = existingValidationResult.IsSuccess && existingValidationResult.Value is not null
                ? existingValidationResult.Value.ReportCode
                : string.Empty;
            var actor = existingValidationResult.IsSuccess && existingValidationResult.Value is not null
                ? existingValidationResult.Value.UpdatedBy ?? existingValidationResult.Value.ValidatedBy ?? existingValidationResult.Value.CreatedBy
                : string.Empty;

            var result = await _reportValidationService.DeleteReportValidationAsync(request.ReportValidationId, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation(" Successfully deleted Report Validation with ID: {Id}", request.ReportValidationId);

                await StampParentReportUpdatedDateAsync(parentReportCode, actor, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                _logger.LogApplicationError("Failed to delete Report Validation with ID: {Id}. Error: {Error}",
                    ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogApplicationWarning("DeleteReportValidationCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while deleting Report Validation with ID: {Id}", ApplicationEventIds.Error, ex);
            return Result<bool>.Failure<bool>(DomainErrors.ReportValidationError.DeleteFailed);
        }
    }

    private async Task StampParentReportUpdatedDateAsync(string? reportCode, string? updatedBy, CancellationToken cancellationToken)
    {
        var normalizedReportCode = reportCode?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(normalizedReportCode))
        {
            return;
        }

        var reportResult = await _reportService.GetReportByCodeAsync(new ReportID(normalizedReportCode), cancellationToken).ConfigureAwait(false);
        if (reportResult.IsFailure || reportResult.Value is null)
        {
            _logger.LogApplicationWarning("Failed loading parent report {ReportCode} for ReportValidation delete stamp.", normalizedReportCode);
            return;
        }

        var report = reportResult.Value;
        report.UpdatedBy = !string.IsNullOrWhiteSpace(updatedBy)
            ? updatedBy.Trim()
            : (report.UpdatedBy ?? report.CreatedBy ?? string.Empty);
        report.UpdatedDate = DateTime.Now;

        var updateResult = await _reportService.UpdateReportAsync(report, cancellationToken).ConfigureAwait(false);
        if (updateResult.IsFailure)
        {
            _logger.LogApplicationWarning("Failed updating parent report timestamp for report {ReportCode} after ReportValidation delete.", normalizedReportCode);
        }
    }
}


