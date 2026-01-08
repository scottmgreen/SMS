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
                new SafetyPerformanceIndicatorID("PI-0000"), // ID will be generated
                request.Name,
                request.Description,
                request.IndicatorType,
                request.CreatedBy
            );
            spi.Code = spi.Id.Value;
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

public class AddSPIDataPointCommandHandler : BaseCommandBundle, IRequestHandler<AddSPIDataPointCommand, Result<SafetyPerformanceIndicator>>
{
    private readonly SafetyPerformanceIndicatorDataService _dataService;
    private readonly ILogger<AddSPIDataPointCommandHandler> _logger;

    public AddSPIDataPointCommandHandler(SafetyPerformanceIndicatorDataService dataService,ILogger<AddSPIDataPointCommandHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<SafetyPerformanceIndicator>> HandleAsync(AddSPIDataPointCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request is null)
            {
                _logger.LogError("AddSPIDataPointCommand received with null request");
                return Result<SafetyPerformanceIndicator>.Failure<SafetyPerformanceIndicator>(
                    DomainErrors.SPIError.NullOrEmpty);
            }

            _logger.LogInformation("Processing AddSPIDataPointCommand for SPI ID: {SPIId}, Value: {Value}, Date: {Date}",
                request.DataPoint.SPIId, request.DataPoint.Value, request.DataPoint.MeasurementDate);

            // Get the SPI by code
            var getSpiResult = await _dataService.GetSafetyPerformanceIndicatorByCodeAsync(request.DataPoint.SPIId, cancellationToken);
            if (!getSpiResult.IsSuccess || getSpiResult.Value == null)
            {
                _logger.LogError("Could not find SPI with code: {SPIId}", request.DataPoint.SPIId);
                return Result<SafetyPerformanceIndicator>.Failure<SafetyPerformanceIndicator>(
                    DomainErrors.SPIError.NotFound);
            }

            var spi = getSpiResult.Value;

            // Create the data point using domain method
            //var addResult = spi.AddDataPoint(request.DataPoint.Value, request.DataPoint.MeasurementDate, request.DataPoint.DataSource, request.DataPoint.CreatedBy);

            var addResult = await _dataService.AddSPIDataPointAsync(spi.Code, request.DataPoint, cancellationToken);


            if (addResult.IsFailure)
            {
                _logger.LogError("Failed to add data point to SPI: {Error}", addResult.Error?.Message);
                return Result<SafetyPerformanceIndicator>.Failure<SafetyPerformanceIndicator>(addResult.Error);
            }

            // The AddDataPoint method creates a new SPIDataPoint and adds it to the collection
            // We need to set additional properties on the last added data point
            //var newDataPoint = spi.DataPoints?.LastOrDefault();
            //if (newDataPoint != null && !string.IsNullOrEmpty(request.Notes))
            //{
            //    newDataPoint.Notes = request.Notes;
            //}

            // Refresh the SPI to get the updated data points
            var refreshedSpiResult = await _dataService.GetSafetyPerformanceIndicatorByCodeAsync(request.DataPoint.SPIId, cancellationToken);
            if (!refreshedSpiResult.IsSuccess)
            {
                _logger.LogError("Failed to refresh SPI after adding data point");
                return Result<SafetyPerformanceIndicator>.Failure<SafetyPerformanceIndicator>(refreshedSpiResult.Error);
            }

            _logger.LogInformation("Successfully added data point to SPI with ID: {SPIId}, Value: {Value}",
                request.DataPoint.SPIId, request.DataPoint.Value);

            return refreshedSpiResult;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("AddSPIDataPointCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while adding data point to SPI with ID: {SPIId}", 
                request?.DataPoint.SPIId);
            return Result<SafetyPerformanceIndicator>.Failure<SafetyPerformanceIndicator>(DomainErrors.SPIError.UpdateFailed);
        }
    }

    private string GetPeriodFromDate(DateTime date, SPIMeasurementFrequency frequency)
    {
        return frequency.Value switch
        {
            "DAILY" => date.ToString("yyyy-MM-dd"),
            "WEEKLY" => $"{date.Year}-W{GetWeekNumber(date):D2}",
            "MONTHLY" => date.ToString("yyyy-MM"),
            "QUARTERLY" => $"{date.Year}-Q{GetQuarter(date)}",
            "ANNUALLY" => date.ToString("yyyy"),
            _ => date.ToString("yyyy-MM")
        };
    }

    private int GetWeekNumber(DateTime date)
    {
        var culture = System.Globalization.CultureInfo.CurrentCulture;
        return culture.Calendar.GetWeekOfYear(date, 
            System.Globalization.CalendarWeekRule.FirstDay, DayOfWeek.Monday);
    }

    private int GetQuarter(DateTime date)
    {
        return (date.Month - 1) / 3 + 1;
    }
}

public class UpdateSPIDataPointCommandHandler : BaseCommandBundle, IRequestHandler<UpdateSPIDataPointCommand, Result<SafetyPerformanceIndicator>>
{
    private readonly SafetyPerformanceIndicatorDataService _dataService;
    private readonly ILogger<UpdateSPIDataPointCommandHandler> _logger;

    public UpdateSPIDataPointCommandHandler(
        SafetyPerformanceIndicatorDataService dataService,
        ILogger<UpdateSPIDataPointCommandHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<SafetyPerformanceIndicator>> HandleAsync(
        UpdateSPIDataPointCommand request, 
        CancellationToken cancellationToken)
    {
        try
        {
            if (request is null)
            {
                _logger.LogError("UpdateSPIDataPointCommand received with null request");
                return Result<SafetyPerformanceIndicator>.Failure<SafetyPerformanceIndicator>(
                    DomainErrors.SPIError.NullOrEmpty);
            }

            _logger.LogInformation("Processing UpdateSPIDataPointCommand for SPI ID: {SPIId}, DataPoint ID: {Code}, Value: {Value}",
                request.DataPoint.SPIId, request.DataPoint.Code, request.DataPoint.Value);

            // Get the SPI by code first to validate it exists
            var getSpiResult = await _dataService.GetSafetyPerformanceIndicatorByCodeAsync(request.DataPoint.SPIId, cancellationToken);
            if (!getSpiResult.IsSuccess || getSpiResult.Value == null)
            {
                _logger.LogError("Could not find SPI with code: {SPIId}", request.DataPoint.SPIId);
                return Result<SafetyPerformanceIndicator>.Failure<SafetyPerformanceIndicator>(
                    DomainErrors.SPIError.NotFound);
            }

            var spi = getSpiResult.Value;

            // Create the updated data point
            //var dataPoint = new SPIDataPoint(new SPIDataPointID("DP-0000"))
            //{
            //    Code = request.Code,
            //    Value = request.Value,
            //    MeasurementDate = request.MeasurementDate,
            //    DataSource = request.DataSource,
            //    Notes = request.Notes,
            //    IsVerified = request.IsVerified,
            //    VerifiedBy = request.VerifiedBy,
            //    VerifiedDate = request.IsVerified ? DateTime.UtcNow : null,
            //    Period = GetPeriodFromDate(request.MeasurementDate, spi.MeasurementFrequency)
            //};

            // Update the data point in the database
            var updateResult = await _dataService.UpdateSPIDataPointAsync(request.DataPoint, cancellationToken);

            if (!updateResult.IsSuccess)
            {
                _logger.LogError("Failed to update data point in database: {Error}", updateResult.Error?.Message);
                return Result<SafetyPerformanceIndicator>.Failure<SafetyPerformanceIndicator>(updateResult.Error);
            }

            // Refresh the SPI to get the updated data points
            var refreshedSpiResult = await _dataService.GetSafetyPerformanceIndicatorByCodeAsync(request.DataPoint.SPIId, cancellationToken);
            if (!refreshedSpiResult.IsSuccess)
            {
                _logger.LogError("Failed to refresh SPI after updating data point");
                return Result<SafetyPerformanceIndicator>.Failure<SafetyPerformanceIndicator>(refreshedSpiResult.Error);
            }

            _logger.LogInformation("Successfully updated data point for SPI with ID: {SPIId}, DataPoint ID: {Code}",
                request.DataPoint.SPIId, request.DataPoint.Code);

            return refreshedSpiResult;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("UpdateSPIDataPointCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while updating data point for SPI with ID: {SPIId}", 
                request?.DataPoint.SPIId);
            return Result<SafetyPerformanceIndicator>.Failure<SafetyPerformanceIndicator>(DomainErrors.SPIError.UpdateFailed);
        }
    }

    private string GetPeriodFromDate(DateTime date, SPIMeasurementFrequency frequency)
    {
        return frequency.Value switch
        {
            "DAILY" => date.ToString("yyyy-MM-dd"),
            "WEEKLY" => $"{date.Year}-W{GetWeekNumber(date):D2}",
            "MONTHLY" => date.ToString("yyyy-MM"),
            "QUARTERLY" => $"{date.Year}-Q{GetQuarter(date)}",
            "ANNUALLY" => date.ToString("yyyy"),
            _ => date.ToString("yyyy-MM")
        };
    }

    private int GetWeekNumber(DateTime date)
    {
        var culture = System.Globalization.CultureInfo.CurrentCulture;
        return culture.Calendar.GetWeekOfYear(date, 
            System.Globalization.CalendarWeekRule.FirstDay, DayOfWeek.Monday);
    }

    private int GetQuarter(DateTime date)
    {
        return (date.Month - 1) / 3 + 1;
    }
}

public class DeleteSPIDataPointCommandHandler : BaseCommandBundle, IRequestHandler<DeleteSPIDataPointCommand, Result<SafetyPerformanceIndicator>>
{
    private readonly SafetyPerformanceIndicatorDataService _dataService;
    private readonly ILogger<DeleteSPIDataPointCommandHandler> _logger;

    public DeleteSPIDataPointCommandHandler(
        SafetyPerformanceIndicatorDataService dataService,
        ILogger<DeleteSPIDataPointCommandHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<SafetyPerformanceIndicator>> HandleAsync(
        DeleteSPIDataPointCommand request, 
        CancellationToken cancellationToken)
    {
        try
        {
            if (request is null)
            {
                _logger.LogError("DeleteSPIDataPointCommand received with null request");
                return Result<SafetyPerformanceIndicator>.Failure<SafetyPerformanceIndicator>(
                    DomainErrors.SPIError.NullOrEmpty);
            }

            _logger.LogInformation("Processing DeleteSPIDataPointCommand for SPI ID: {SPIId}, DataPoint ID: {Code}",
                request.DataPoint.SPIId, request.DataPoint.Code);

            // Delete the data point from the database
            var deleteResult = await _dataService.DeleteSPIDataPointAsync(request.DataPoint.Code, cancellationToken);

            if (!deleteResult.IsSuccess)
            {
                _logger.LogError("Failed to delete data point from database: {Error}", deleteResult.Error?.Message);
                return Result<SafetyPerformanceIndicator>.Failure<SafetyPerformanceIndicator>(deleteResult.Error);
            }

            // Refresh the SPI to get the updated data points
            var refreshedSpiResult = await _dataService.GetSafetyPerformanceIndicatorByCodeAsync(request.DataPoint.SPIId, cancellationToken);
            if (!refreshedSpiResult.IsSuccess)
            {
                _logger.LogError("Failed to refresh SPI after deleting data point");
                return Result<SafetyPerformanceIndicator>.Failure<SafetyPerformanceIndicator>(refreshedSpiResult.Error);
            }

            _logger.LogInformation("Successfully deleted data point for SPI with ID: {SPIId}, DataPoint ID: {Code}",
                request.DataPoint.SPIId, request.DataPoint.Code);

            return refreshedSpiResult;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("DeleteSPIDataPointCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while deleting data point for SPI with ID: {SPIId}", 
                request?.DataPoint.SPIId);
            return Result<SafetyPerformanceIndicator>.Failure<SafetyPerformanceIndicator>(DomainErrors.SPIError.DeleteFailed);
        }
    }
}