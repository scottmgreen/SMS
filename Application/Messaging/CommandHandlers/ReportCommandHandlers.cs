using Microsoft.Extensions.Logging;

namespace SMS_Application.Messaging.CommandHandlers;

// =============================================
// REPORT COMMAND HANDLERS
// =============================================

public class CreateReportCommandHandler : BaseCommandBundle, IRequestHandler<CreateReportCommand, Result<Report>>
{
    private readonly ReportDataService _dataService;
    private readonly ILogger<CreateReportCommandHandler> _logger;

    public CreateReportCommandHandler(ReportDataService dataService, ILogger<CreateReportCommandHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<Report>> HandleAsync(CreateReportCommand request, CancellationToken ct = default)
    {
        try
        {
            if (request?.Report is null)
            {
                _logger.LogApplicationError("CreateReportCommand received with null Report", ApplicationEventIds.Error, null);
                return Result<Report>.Failure<Report>(DomainErrors.ReportError.NullOrEmpty);
            }

            _logger.LogInformation("Processing CreateReportCommand for Code: {Code}", request.Report.Code);

            var result = await _dataService.CreateReportAsync(request.Report, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully created Report with ID: {Id}, Code: {Code}",
                    result.Value?.Id, result.Value?.Code);
            }
            else
            {
                _logger.LogApplicationError("Failed to create Report with Code: {Code}. Error: {Error}",
                    ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("CreateReportCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while creating Report", ApplicationEventIds.Error, ex);
            return Result<Report>.Failure<Report>(DomainErrors.ReportError.CreateFailed);
        }
    }
}

public class UpdateReportCommandHandler : BaseCommandBundle, IRequestHandler<UpdateReportCommand, Result<Report>>
{
    private readonly ReportDataService _dataService;
    private readonly ILogger<UpdateReportCommandHandler> _logger;

    public UpdateReportCommandHandler(ReportDataService dataService, ILogger<UpdateReportCommandHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<Report>> HandleAsync(UpdateReportCommand request, CancellationToken ct = default)
    {
        try
        {
            if (request?.Report is null)
            {
                _logger.LogApplicationError("UpdateReportCommand received with null Report", ApplicationEventIds.Error, null);
                return Result<Report>.Failure<Report>(DomainErrors.ReportError.NullOrEmpty);
            }

            _logger.LogInformation("Processing UpdateReportCommand for ID: {Id}, Code: {Code}",
                request.Report.Id, request.Report.Code);

            var result = await _dataService.UpdateReportAsync(request.Report, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully updated Report with ID: {Id}", request.Report.Id);
            }
            else
            {
                _logger.LogApplicationError("Failed to update Report with ID: {Id}. Error: {Error}",
                    ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("UpdateReportCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while updating Report with ID: {Id}", ApplicationEventIds.Error, ex);
            return Result<Report>.Failure<Report>(DomainErrors.ReportError.UpdateFailed);
        }
    }
}

public class DeleteReportCommandHandler : BaseCommandBundle, IRequestHandler<DeleteReportCommand, Result<bool>>
{
    private readonly ReportDataService _dataService;
    private readonly ILogger<DeleteReportCommandHandler> _logger;

    public DeleteReportCommandHandler(ReportDataService dataService, ILogger<DeleteReportCommandHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> HandleAsync(DeleteReportCommand request, CancellationToken ct = default)
    {
        try
        {
            if (request?.ReportId is null)
            {
                _logger.LogApplicationError("DeleteReportCommand received with null ReportId", ApplicationEventIds.Error, null);
                return Result<bool>.Failure<bool>(DomainErrors.ReportError.NullOrEmpty);
            }

            _logger.LogInformation("Processing DeleteReportCommand for ID: {Id}", request.ReportId);

            var result = await _dataService.DeleteReportAsync(request.ReportId, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully deleted Report with ID: {Id}", request.ReportId);
            }
            else
            {
                _logger.LogApplicationError("Failed to delete Report with ID: {Id}. Error: {Error}",
                    ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("DeleteReportCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while deleting Report with ID: {Id}", ApplicationEventIds.Error, ex);
            return Result<bool>.Failure<bool>(DomainErrors.ReportError.DeleteFailed);
        }
    }
}