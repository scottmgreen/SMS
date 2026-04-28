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

using Microsoft.Extensions.Logging;
using SMS_Application.Interfaces;

namespace SMS_Application.Messaging.CommandHandlers;

// =============================================
// REPORT VALIDATION COMMAND HANDLERS - Clean Architecture Pattern
// =============================================

public class CreateReportValidationCommandHandler : BaseCommandBundle, IBaseRequestHandler<CreateReportValidationCommand, Result<ReportValidation>>
{
    private readonly ReportValidationService _reportValidationService;
    private readonly ILogger<CreateReportValidationCommandHandler> _logger;

    public CreateReportValidationCommandHandler(ReportValidationService reportValidationService, ILogger<CreateReportValidationCommandHandler> logger)
    {
        _reportValidationService = reportValidationService ?? throw new ArgumentNullException(nameof(reportValidationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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

            _logger.LogInformation("✅ Clean Architecture: Processing CreateReportValidationCommand for Code: {Code}", request.ReportValidation.Code);

            var result = await _reportValidationService.CreateReportValidationAsync(request.ReportValidation, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("✅ Clean Architecture: Successfully created Report Validation with ID: {Id}, Code: {Code}",
                    result.Value?.Id, result.Value?.Code);
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
            _logger.LogWarning("CreateReportValidationCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while creating Report Validation", ApplicationEventIds.Error, ex);
            return Result<ReportValidation>.Failure<ReportValidation>(DomainErrors.ReportValidationError.CreateFailed);
        }
    }
}

public class UpdateReportValidationCommandHandler : BaseCommandBundle, IBaseRequestHandler<UpdateReportValidationCommand, Result<ReportValidation>>
{
    private readonly ReportValidationService _reportValidationService;
    private readonly ILogger<UpdateReportValidationCommandHandler> _logger;

    public UpdateReportValidationCommandHandler(ReportValidationService reportValidationService, ILogger<UpdateReportValidationCommandHandler> logger)
    {
        _reportValidationService = reportValidationService ?? throw new ArgumentNullException(nameof(reportValidationService));
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

            _logger.LogInformation("✅ Clean Architecture: Processing UpdateReportValidationCommand for ID: {Id}", request.ReportValidation.Id);

            var result = await _reportValidationService.UpdateReportValidationAsync(request.ReportValidation, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("✅ Clean Architecture: Successfully updated Report Validation with ID: {Id}", request.ReportValidation.Id);
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
            _logger.LogWarning("UpdateReportValidationCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while updating Report Validation with ID: {Id}", ApplicationEventIds.Error, ex);
            return Result<ReportValidation>.Failure<ReportValidation>(DomainErrors.ReportValidationError.UpdateFailed);
        }
    }
}

public class DeleteReportValidationCommandHandler : BaseCommandBundle, IBaseRequestHandler<DeleteReportValidationCommand, Result<bool>>
{
    private readonly ReportValidationService _reportValidationService;
    private readonly ILogger<DeleteReportValidationCommandHandler> _logger;

    public DeleteReportValidationCommandHandler(ReportValidationService reportValidationService, ILogger<DeleteReportValidationCommandHandler> logger)
    {
        _reportValidationService = reportValidationService ?? throw new ArgumentNullException(nameof(reportValidationService));
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

            _logger.LogInformation("✅ Clean Architecture: Processing DeleteReportValidationCommand for ID: {Id}", request.ReportValidationId);

            var result = await _reportValidationService.DeleteReportValidationAsync(request.ReportValidationId, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("✅ Clean Architecture: Successfully deleted Report Validation with ID: {Id}", request.ReportValidationId);
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
            _logger.LogWarning("DeleteReportValidationCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while deleting Report Validation with ID: {Id}", ApplicationEventIds.Error, ex);
            return Result<bool>.Failure<bool>(DomainErrors.ReportValidationError.DeleteFailed);
        }
    }
}
