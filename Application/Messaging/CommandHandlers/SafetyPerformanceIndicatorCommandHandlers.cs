using Microsoft.Extensions.Logging;
using SMS_Application.Common;
using SMS_Application.Interfaces;
using SMS_Application.Messaging.Commands;
using SMS_Domain.Entities;
using SMS_Domain.Errors;
using SMS_Infrastructure.Services;
using SMS_Shared.Common;

namespace SMS_Application.Messaging.CommandHandlers;

public class CreateSafetyPerformanceIndicatorCommandHandler : BaseCommandBundle, IRequestHandler<CreateSafetyPerformanceIndicatorCommand, Result<SafetyPerformanceIndicator>>
{
    private readonly SafetyPerformanceIndicatorDataService _dataService;
    private readonly ILogger<CreateSafetyPerformanceIndicatorCommandHandler> _logger;

    public CreateSafetyPerformanceIndicatorCommandHandler(
        SafetyPerformanceIndicatorDataService dataService,
        ILogger<CreateSafetyPerformanceIndicatorCommandHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<SafetyPerformanceIndicator>> HandleAsync(
        CreateSafetyPerformanceIndicatorCommand request, 
        CancellationToken cancellationToken)
    {
        try
        {
            if (request is null)
            {
                _logger.LogError("CreateSafetyPerformanceIndicatorCommand received with null request");
                return Result<SafetyPerformanceIndicator>.Failure<SafetyPerformanceIndicator>(
                    DomainErrors.SPIError.NullOrEmpty);
            }

            _logger.LogInformation("Processing CreateSafetyPerformanceIndicatorCommand for Name: {Name}", request.Name);

            // Create the SPI entity
            var spi = new SafetyPerformanceIndicator(
                new SafetyPerformanceIndicatorID(string.Empty), // ID will be generated
                request.Name,
                request.Description,
                request.IndicatorType,
                request.CreatedBy
            );

            // Set additional properties
            spi.MeasurementUnit = request.MeasurementUnit;
            spi.MeasurementFrequency = request.MeasurementFrequency;
            spi.CalculationMethod = request.CalculationMethod;
            spi.DataSource = request.DataSource;
            spi.TargetValue = request.TargetValue;
            spi.AcceptableRange = request.AcceptableRange;
            spi.WarningThreshold = request.WarningThreshold;
            spi.CriticalThreshold = request.CriticalThreshold;
            spi.ResponsibleDepartment = request.ResponsibleDepartment;
            spi.DataOwner = request.DataOwner;
            spi.ReviewAuthority = request.ReviewAuthority;
            spi.NextReviewDate = request.NextReviewDate;
            spi.AlertsEnabled = request.AlertsEnabled;
            spi.AlertRecipients = request.AlertRecipients;

            var result = await _dataService.CreateSafetyPerformanceIndicatorAsync(spi, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation(
                    "Successfully created Safety Performance Indicator with ID: {Id}, Name: {Name}",
                    result.Value?.Id?.Value, request.Name);
            }
            else
            {
                _logger.LogError("Failed to create Safety Performance Indicator with Name: {Name}. Error: {Error}",
                    request.Name, result.Error?.Message);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("CreateSafetyPerformanceIndicatorCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while creating Safety Performance Indicator with Name: {Name}", request?.Name);
            return Result<SafetyPerformanceIndicator>.Failure<SafetyPerformanceIndicator>(DomainErrors.SPIError.CreateFailed);
        }
    }
}

public class UpdateSafetyPerformanceIndicatorCommandHandler : BaseCommandBundle, IRequestHandler<UpdateSafetyPerformanceIndicatorCommand, Result<SafetyPerformanceIndicator>>
{
    private readonly SafetyPerformanceIndicatorDataService _dataService;
    private readonly ILogger<UpdateSafetyPerformanceIndicatorCommandHandler> _logger;

    public UpdateSafetyPerformanceIndicatorCommandHandler(
        SafetyPerformanceIndicatorDataService dataService,
        ILogger<UpdateSafetyPerformanceIndicatorCommandHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<SafetyPerformanceIndicator>> HandleAsync(
        UpdateSafetyPerformanceIndicatorCommand request, 
        CancellationToken cancellationToken)
    {
        try
        {
            if (request is null)
            {
                _logger.LogError("UpdateSafetyPerformanceIndicatorCommand received with null request");
                return Result<SafetyPerformanceIndicator>.Failure<SafetyPerformanceIndicator>(
                    DomainErrors.SPIError.NullOrEmpty);
            }

            _logger.LogInformation(
                "Processing UpdateSafetyPerformanceIndicatorCommand for ID: {Id}, Name: {Name}",
                request.Id?.Value, request.Name);

            // Create the updated SPI entity
            var spi = new SafetyPerformanceIndicator(
                request.Id,
                request.Name,
                request.Description,
                request.IndicatorType,
                request.UpdatedBy
            );

            // Set the code
            spi.Code = request.Code;

            // Update status
            spi.UpdateStatus(request.Status, request.UpdatedBy);

            // Set additional properties
            spi.MeasurementUnit = request.MeasurementUnit;
            spi.MeasurementFrequency = request.MeasurementFrequency;
            spi.CalculationMethod = request.CalculationMethod;
            spi.DataSource = request.DataSource;
            spi.TargetValue = request.TargetValue;
            spi.AcceptableRange = request.AcceptableRange;
            spi.WarningThreshold = request.WarningThreshold;
            spi.CriticalThreshold = request.CriticalThreshold;
            spi.ResponsibleDepartment = request.ResponsibleDepartment;
            spi.DataOwner = request.DataOwner;
            spi.ReviewAuthority = request.ReviewAuthority;
            spi.NextReviewDate = request.NextReviewDate;
            spi.LastReviewDate = request.LastReviewDate;
            spi.LastReviewNotes = request.LastReviewNotes;
            spi.AlertsEnabled = request.AlertsEnabled;
            spi.AlertRecipients = request.AlertRecipients;

            var result = await _dataService.UpdateSafetyPerformanceIndicatorAsync(spi, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation(
                    "Successfully updated Safety Performance Indicator with ID: {Id}",
                    request.Id?.Value);
            }
            else
            {
                _logger.LogError(
                    "Failed to update Safety Performance Indicator with ID: {Id}. Error: {Error}",
                    request.Id?.Value, result.Error?.Message);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("UpdateSafetyPerformanceIndicatorCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Unexpected error occurred while updating Safety Performance Indicator with ID: {Id}", 
                request?.Id?.Value);
            return Result<SafetyPerformanceIndicator>.Failure<SafetyPerformanceIndicator>(DomainErrors.SPIError.UpdateFailed);
        }
    }
}

public class DeleteSafetyPerformanceIndicatorCommandHandler : BaseCommandBundle, IRequestHandler<DeleteSafetyPerformanceIndicatorCommand, Result<bool>>
{
    private readonly SafetyPerformanceIndicatorDataService _dataService;
    private readonly ILogger<DeleteSafetyPerformanceIndicatorCommandHandler> _logger;

    public DeleteSafetyPerformanceIndicatorCommandHandler(
        SafetyPerformanceIndicatorDataService dataService,
        ILogger<DeleteSafetyPerformanceIndicatorCommandHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> HandleAsync(
        DeleteSafetyPerformanceIndicatorCommand request, 
        CancellationToken cancellationToken)
    {
        try
        {
            if (request?.SafetyPerformanceIndicatorId is null)
            {
                _logger.LogError("DeleteSafetyPerformanceIndicatorCommand received with null request or ID");
                return Result<bool>.Failure<bool>(DomainErrors.SPIError.NullOrEmpty);
            }

            _logger.LogInformation(
                "Processing DeleteSafetyPerformanceIndicatorCommand for ID: {Id}",
                request.SafetyPerformanceIndicatorId?.Value);

            var result = await _dataService.DeleteSafetyPerformanceIndicatorAsync(
                request.SafetyPerformanceIndicatorId, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation(
                    "Successfully deleted Safety Performance Indicator with ID: {Id}",
                    request.SafetyPerformanceIndicatorId?.Value);
            }
            else
            {
                _logger.LogError(
                    "Failed to delete Safety Performance Indicator with ID: {Id}. Error: {Error}",
                    request.SafetyPerformanceIndicatorId?.Value, result.Error?.Message);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("DeleteSafetyPerformanceIndicatorCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Unexpected error occurred while deleting Safety Performance Indicator with ID: {Id}", 
                request?.SafetyPerformanceIndicatorId?.Value);
            return Result<bool>.Failure<bool>(DomainErrors.SPIError.DeleteFailed);
        }
    }
}