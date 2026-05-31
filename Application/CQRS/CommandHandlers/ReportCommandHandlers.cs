//-----------------------------------------------------------------------
// <copyright file="ReportCommandHandlers.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Command handlers implementing SMS report management business logic and operations.
//                  Implements command handlers for processing write operations.
//                  Handles business logic execution and domain entity coordination.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;

using Microsoft.Extensions.Logging;

namespace SMS_Application.CommandHandlers;

// =============================================
// REPORT COMMAND HANDLERS - Clean Architecture Pattern
// =============================================

public class CreateReportCommandHandler : BaseCommandBundle, IBaseRequestHandler<CreateReportCommand, Result<Report>>
{
    private readonly ReportService _reportService;
    private readonly ILogger<CreateReportCommandHandler> _logger;

    public CreateReportCommandHandler(ReportService reportService, ILogger<CreateReportCommandHandler> logger)
    {
        _reportService = reportService ?? throw new ArgumentNullException(nameof(reportService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<Report>> HandleAsync(CreateReportCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request?.Report is null)
            {
                _logger.LogApplicationError("CreateReportCommand received with null request or report", ApplicationEventIds.Error);
                return Result<Report>.Failure<Report>(DomainErrors.ReportError.NullOrEmpty);
            }

            _logger.LogApplicationInformation(" Processing CreateReportCommand for Code: {Code}", request.Report.Code);

            var result = await _reportService.CreateReportAsync(request.Report, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation(" Successfully created Report with ID: {Id}, Code: {Code}",
                    result.Value?.Id, result.Value?.Code);
            }
            else
            {
                _logger.LogApplicationError("Failed to create Report with Code: {Code}. Error: {Error}",
                    ApplicationEventIds.Error, request.Report.Code, result.Error?.Message ?? "Unknown");
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogApplicationWarning("CreateReportCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error occurred while creating Report", ApplicationEventIds.Error);
            return Result<Report>.Failure<Report>(DomainErrors.ReportError.CreateFailed);
        }
    }
}

public class UpdateReportCommandHandler : BaseCommandBundle, IBaseRequestHandler<UpdateReportCommand, Result<Report>>
{
    private readonly ReportService _reportService;
    private readonly ILogger<UpdateReportCommandHandler> _logger;

    public UpdateReportCommandHandler(ReportService reportService, ILogger<UpdateReportCommandHandler> logger)
    {
        _reportService = reportService ?? throw new ArgumentNullException(nameof(reportService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<Report>> HandleAsync(UpdateReportCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request?.Report is null)
            {
                _logger.LogApplicationError("UpdateReportCommand received with null request or report", ApplicationEventIds.Error);
                return Result<Report>.Failure<Report>(DomainErrors.ReportError.NullOrEmpty);
            }

            _logger.LogApplicationInformation(" Processing UpdateReportCommand for ID: {Id}", request.Report.Id);

            var result = await _reportService.UpdateReportAsync(request.Report, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation(" Successfully updated Report with ID: {Id}", request.Report.Id);
            }
            else
            {
                _logger.LogApplicationError("Failed to update Report with ID: {Id}. Error: {Error}",
                    ApplicationEventIds.Error, request.Report.Id, result.Error?.Message ?? "Unknown");
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogApplicationWarning("UpdateReportCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error occurred while updating Report with ID: {Id}", ApplicationEventIds.Error, request?.Report?.Id);
            return Result<Report>.Failure<Report>(DomainErrors.ReportError.UpdateFailed);
        }
    }
}

public class DeleteReportCommandHandler : BaseCommandBundle, IBaseRequestHandler<DeleteReportCommand, Result<bool>>
{
    private readonly ReportService _reportService;
    private readonly ILogger<DeleteReportCommandHandler> _logger;

    public DeleteReportCommandHandler(ReportService reportService, ILogger<DeleteReportCommandHandler> logger)
    {
        _reportService = reportService ?? throw new ArgumentNullException(nameof(reportService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> HandleAsync(DeleteReportCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request is null)
            {
                _logger.LogApplicationError("DeleteReportCommand received with null request", ApplicationEventIds.Error);
                return Result<bool>.Failure<bool>(DomainErrors.ReportError.NullOrEmpty);
            }

            _logger.LogApplicationInformation(" Processing DeleteReportCommand for ID: {Id}", request.ReportId);

            var result = await _reportService.DeleteReportAsync(request.ReportId, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation(" Successfully deleted Report with ID: {Id}", request.ReportId);
            }
            else
            {
                _logger.LogApplicationError("Failed to delete Report with ID: {Id}. Error: {Error}",
                    ApplicationEventIds.Error, request.ReportId, result.Error?.Message ?? "Unknown");
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogApplicationWarning("DeleteReportCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error occurred while deleting Report with ID: {Id}", ApplicationEventIds.Error, request?.ReportId);
            return Result<bool>. Failure<bool>(DomainErrors.ReportError.DeleteFailed);
        }
    }
}

public class UpdateReportStatusCommandHandler : BaseCommandBundle, IBaseRequestHandler<UpdateReportStatusCommand, Result<bool>>
{
    private readonly ReportService _reportService;
    private readonly ILogger<UpdateReportStatusCommandHandler> _logger;

    public UpdateReportStatusCommandHandler(ReportService reportService, ILogger<UpdateReportStatusCommandHandler> logger)
    {
        _reportService = reportService ?? throw new ArgumentNullException(nameof(reportService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> HandleAsync(UpdateReportStatusCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request is null)
            {
                _logger.LogApplicationError("UpdateReportStatusCommand received with null request", ApplicationEventIds.Error);
                return Result<bool>.Failure<bool>(DomainErrors.ReportError.NullOrEmpty);
            }

            _logger.LogApplicationInformation(" Processing UpdateReportStatusCommand for ReportCode: {ReportCode}, Status: {Status}",
                request.ReportCode, request.ReportStatus);

            var result = await _reportService.UpdateReportStatusAsync(request.ReportCode, request.ReportStatus, request.UpdatedBy, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation(" Successfully updated Report status for Code: {ReportCode} to {Status}",
                    request.ReportCode, request.ReportStatus);
            }
            else
            {
                _logger.LogApplicationError("Failed to update Report status for Code: {ReportCode}. Error: {Error}",
                    ApplicationEventIds.Error, request.ReportCode, result.Error?.Message ?? "Unknown");
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogApplicationWarning("UpdateReportStatusCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error occurred while updating Report status for Code: {ReportCode}", ApplicationEventIds.Error, request?.ReportCode);
            return Result<bool>.Failure<bool>(DomainErrors.ReportError.UpdateFailed);
        }
    }
}

