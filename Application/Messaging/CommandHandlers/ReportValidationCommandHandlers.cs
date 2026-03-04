//-----------------------------------------------------------------------
// <copyright file="ReportValidationCommandHandlers.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Command handlers implementing SMS report management and processing logic.
//                  Implements command handlers for processing write operations.
//                  Handles business logic execution and domain entity coordination.
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using SMS_Application.Messaging.Queries;
using SMS_Domain.Common;
using Application.Interfaces;

namespace SMS_Application.Messaging.CommandHandlers;

// =============================================
// REPORT VALIDATION COMMAND HANDLERS - Clean Architecture Pattern
// =============================================

public class CreateReportValidationCommandHandler : BaseCommandBundle, IRequestHandler<CreateReportValidationCommand, Result<ReportValidation>>
{
    private readonly IReportValidationService _reportValidationService;
    private readonly ILogger<CreateReportValidationCommandHandler> _logger;

    public CreateReportValidationCommandHandler(IReportValidationService reportValidationService, ILogger<CreateReportValidationCommandHandler> logger)
    {
        _reportValidationService = reportValidationService ?? throw new ArgumentNullException(nameof(reportValidationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<ReportValidation>> HandleAsync(CreateReportValidationCommand request, CancellationToken ct = default)
    {
        try
        {
            if (request?.ReportValidation is null)
            {
                _logger.LogApplicationError("CreateReportValidationCommand received with null ReportValidation", ApplicationEventIds.Error, null);
                return Result<ReportValidation>.Failure<ReportValidation>(DomainErrors.ReportError.NullOrEmpty);
            }

            _logger.LogInformation("✅ Clean Architecture: Processing CreateReportValidationCommand for Code: {Code}", request.ReportValidation.Code);

            // 🎯 CLEAN ARCHITECTURE: All complex business logic is now encapsulated in the Application Service
            var result = await _reportValidationService.CreateReportValidationAsync(request.ReportValidation, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("✅ Clean Architecture: Successfully created ReportValidation with Code: {Code}", result.Value?.Code);
            }
            else
            {
                _logger.LogApplicationError("Failed to create ReportValidation with Code: {Code}. Error: {Error}",
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
            _logger.LogApplicationError("Unexpected error occurred while creating ReportValidation", ApplicationEventIds.Error, ex);
            return Result<ReportValidation>.Failure<ReportValidation>(DomainErrors.ReportError.CreateFailed);
        }
    }
}

public class UpdateReportValidationCommandHandler : BaseCommandBundle, IRequestHandler<UpdateReportValidationCommand, Result<ReportValidation>>
{
    private readonly IReportValidationService _reportValidationService;
    private readonly ILogger<UpdateReportValidationCommandHandler> _logger;

    public UpdateReportValidationCommandHandler(IReportValidationService reportValidationService, ILogger<UpdateReportValidationCommandHandler> logger)
    {
        _reportValidationService = reportValidationService ?? throw new ArgumentNullException(nameof(reportValidationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<ReportValidation>> HandleAsync(UpdateReportValidationCommand request, CancellationToken ct = default)
    {
        try
        {
            if (request?.ReportValidation is null)
            {
                _logger.LogApplicationError("UpdateReportValidationCommand received with null ReportValidation", ApplicationEventIds.Error, null);
                return Result<ReportValidation>.Failure<ReportValidation>(DomainErrors.ReportError.NullOrEmpty);
            }

            _logger.LogInformation("✅ Clean Architecture: Processing UpdateReportValidationCommand for Code: {Code}", request.ReportValidation.Code);

            var result = await _reportValidationService.UpdateReportValidationAsync(request.ReportValidation, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("✅ Clean Architecture: Successfully updated ReportValidation with Code: {Code}", request.ReportValidation.Code);
            }
            else
            {
                _logger.LogApplicationError("Failed to update ReportValidation with Code: {Code}. Error: {Error}",
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
            _logger.LogApplicationError("Unexpected error occurred while updating ReportValidation", ApplicationEventIds.Error, ex);
            return Result<ReportValidation>.Failure<ReportValidation>(DomainErrors.ReportError.UpdateFailed);
        }
    }
}

public class DeleteReportValidationCommandHandler : BaseCommandBundle, IRequestHandler<DeleteReportValidationCommand, Result<bool>>
{
    private readonly IReportValidationService _reportValidationService;
    private readonly ILogger<DeleteReportValidationCommandHandler> _logger;

    public DeleteReportValidationCommandHandler(IReportValidationService reportValidationService, ILogger<DeleteReportValidationCommandHandler> logger)
    {
        _reportValidationService = reportValidationService ?? throw new ArgumentNullException(nameof(reportValidationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> HandleAsync(DeleteReportValidationCommand request, CancellationToken ct = default)
    {
        try
        {
            if (request?.ReportValidationId is null)
            {
                _logger.LogApplicationError("DeleteReportValidationCommand received with null ReportValidationId", ApplicationEventIds.Error, null);
                return Result<bool>.Failure<bool>(DomainErrors.ReportError.NullOrEmpty);
            }

            _logger.LogInformation("✅ Clean Architecture: Processing DeleteReportValidationCommand for Code: {Code}", request.ReportValidationId);

            var result = await _reportValidationService.DeleteReportValidationAsync(request.ReportValidationId, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("✅ Clean Architecture: Successfully deleted ReportValidation with Code: {Code}", request.ReportValidationId);
            }
            else
            {
                _logger.LogApplicationError("Failed to delete ReportValidation with Code: {Code}. Error: {Error}",
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
            _logger.LogApplicationError("Unexpected error occurred while deleting ReportValidation", ApplicationEventIds.Error, ex);
            return Result<bool>.Failure<bool>(DomainErrors.ReportError.DeleteFailed);
        }
    }
}

public class ResetReportValidationCommandHandler : BaseCommandBundle, IRequestHandler<ResetReportValidationCommand, Result<bool>>
{
    private readonly IReportValidationService _reportValidationService;
    private readonly ILogger<ResetReportValidationCommandHandler> _logger;

    public ResetReportValidationCommandHandler(IReportValidationService reportValidationService, ILogger<ResetReportValidationCommandHandler> logger)
    {
        _reportValidationService = reportValidationService ?? throw new ArgumentNullException(nameof(reportValidationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> HandleAsync(ResetReportValidationCommand request, CancellationToken ct = default)
    {
        try
        {
            if (request?.ReportValidationId is null)
            {
                _logger.LogApplicationError("ResetReportValidationCommand received with null ReportValidationId", ApplicationEventIds.Error, null);
                return Result<bool>.Failure<bool>(DomainErrors.ReportError.NullOrEmpty);
            }

            _logger.LogInformation("✅ Clean Architecture: Processing ResetReportValidationCommand for Code: {Code}", request.ReportValidationId);

            // 🎯 CLEAN ARCHITECTURE: Complex business logic is now in the Application Service
            var result = await _reportValidationService.ResetReportValidationAsync(request.ReportValidationId, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("✅ Clean Architecture: Successfully reset ReportValidation with Code: {Code}", request.ReportValidationId);
            }
            else
            {
                _logger.LogApplicationError("Failed to reset ReportValidation with Code: {Code}. Error: {Error}",
                    ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("ResetReportValidationCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while resetting ReportValidation", ApplicationEventIds.Error, ex);
            return Result<bool>.Failure<bool>(DomainErrors.ReportError.DeleteFailed);
        }
    }
}
