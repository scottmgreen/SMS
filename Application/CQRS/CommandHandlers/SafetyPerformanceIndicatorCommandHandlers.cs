//-----------------------------------------------------------------------
// <copyright file="SafetyPerformanceIndicatorCommandHandlers.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Command handlers implementing business logic for SMS write operations.
//                  Implements command handlers for processing write operations.
//                  Handles business logic execution and domain entity coordination.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;
using SMS_Domain.Events;
using SMS_Application.Queries;

using Microsoft.Extensions.Logging;

namespace SMS_Application.CommandHandlers;

// =============================================
// SAFETY PERFORMANCE INDICATOR COMMAND HANDLERS - Clean Architecture Pattern
// =============================================

public class CreateSafetyPerformanceIndicatorCommandHandler : BaseCommandBundle, IBaseRequestHandler<CreateSafetyPerformanceIndicatorCommand, Result<SafetyPerformanceIndicator>>
{
    private readonly ISafetyPerformanceIndicatorService _spiService;
    private readonly ILogger<CreateSafetyPerformanceIndicatorCommandHandler> _logger;

    public CreateSafetyPerformanceIndicatorCommandHandler(
        ISafetyPerformanceIndicatorService spiService,
        ILogger<CreateSafetyPerformanceIndicatorCommandHandler> logger)
    {
        _spiService = spiService ?? throw new ArgumentNullException(nameof(spiService));
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
                _logger.LogApplicationError("CreateSafetyPerformanceIndicatorCommand received with null request", ApplicationEventIds.Error, null);
                return Result<SafetyPerformanceIndicator>.Failure<SafetyPerformanceIndicator>(
                    DomainErrors.SPIError.NullOrEmpty);
            }

            _logger.LogApplicationInformation(" Processing CreateSafetyPerformanceIndicatorCommand for Name: {Name}", request.Name);

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

            var result = await _spiService.CreateSafetyPerformanceIndicatorAsync(spi, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation(" Successfully created Safety Performance Indicator with ID: {Id}, Name: {Name}",
                    result.Value?.Id, result.Value?.Name);
            }
            else
            {
                _logger.LogApplicationError("Failed to create Safety Performance Indicator with Name: {Name}. Error: {Error}",
                    ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogApplicationWarning("CreateSafetyPerformanceIndicatorCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while creating Safety Performance Indicator", ApplicationEventIds.Error, ex);
            return Result<SafetyPerformanceIndicator>.Failure<SafetyPerformanceIndicator>(DomainErrors.SPIError.CreateFailed);
        }
    }

public class UpdateSPIConfigurationCommandHandler : BaseCommandBundle, IBaseRequestHandler<UpdateSPIConfigurationCommand, Result<SafetyPerformanceIndicator>>
{
    private readonly SafetyPerformanceIndicatorService _spiService;
    private readonly ILogger<UpdateSPIConfigurationCommandHandler> _logger;

    public UpdateSPIConfigurationCommandHandler(
        SafetyPerformanceIndicatorService spiService,
        ILogger<UpdateSPIConfigurationCommandHandler> logger)
    {
        _spiService = spiService ?? throw new ArgumentNullException(nameof(spiService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<SafetyPerformanceIndicator>> HandleAsync(UpdateSPIConfigurationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request is null)
            {
                _logger.LogApplicationError("UpdateSPIConfigurationCommand received with null request", ApplicationEventIds.Error, null);
                return Result<SafetyPerformanceIndicator>.Failure<SafetyPerformanceIndicator>(DomainErrors.SPIError.NullOrEmpty);
            }

            return await _spiService.UpdateConfigurationAsync(
                request.SPIId,
                request.Name,
                request.Description,
                request.IndicatorType,
                request.MeasurementUnit,
                request.MeasurementFrequency,
                request.CalculationMethod,
                request.DataSource,
                request.UpdatedBy,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error while updating SPI configuration", ApplicationEventIds.Error, ex);
            return Result<SafetyPerformanceIndicator>.Failure<SafetyPerformanceIndicator>(DomainErrors.SPIError.UpdateFailed);
        }
    }
}

public class SetSPITargetsCommandHandler : BaseCommandBundle, IBaseRequestHandler<SetSPITargetsCommand, Result<SafetyPerformanceIndicator>>
{
    private readonly SafetyPerformanceIndicatorService _spiService;
    private readonly ILogger<SetSPITargetsCommandHandler> _logger;

    public SetSPITargetsCommandHandler(
        SafetyPerformanceIndicatorService spiService,
        ILogger<SetSPITargetsCommandHandler> logger)
    {
        _spiService = spiService ?? throw new ArgumentNullException(nameof(spiService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<SafetyPerformanceIndicator>> HandleAsync(SetSPITargetsCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request is null)
            {
                _logger.LogApplicationError("SetSPITargetsCommand received with null request", ApplicationEventIds.Error, null);
                return Result<SafetyPerformanceIndicator>.Failure<SafetyPerformanceIndicator>(DomainErrors.SPIError.NullOrEmpty);
            }

            return await _spiService.SetTargetsAsync(
                request.SPIId,
                request.TargetValue,
                request.AcceptableRange,
                request.WarningThreshold,
                request.CriticalThreshold,
                request.UpdatedBy,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error while setting SPI targets", ApplicationEventIds.Error, ex);
            return Result<SafetyPerformanceIndicator>.Failure<SafetyPerformanceIndicator>(DomainErrors.SPIError.TargetUpdateFailed);
        }
    }
}

public class UpdateSPIStatusCommandHandler : BaseCommandBundle, IBaseRequestHandler<UpdateSPIStatusCommand, Result<SafetyPerformanceIndicator>>
{
    private readonly SafetyPerformanceIndicatorService _spiService;
    private readonly ILogger<UpdateSPIStatusCommandHandler> _logger;

    public UpdateSPIStatusCommandHandler(
        SafetyPerformanceIndicatorService spiService,
        ILogger<UpdateSPIStatusCommandHandler> logger)
    {
        _spiService = spiService ?? throw new ArgumentNullException(nameof(spiService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<SafetyPerformanceIndicator>> HandleAsync(UpdateSPIStatusCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request is null)
            {
                _logger.LogApplicationError("UpdateSPIStatusCommand received with null request", ApplicationEventIds.Error, null);
                return Result<SafetyPerformanceIndicator>.Failure<SafetyPerformanceIndicator>(DomainErrors.SPIError.NullOrEmpty);
            }

            return await _spiService.UpdateStatusAsync(request.SPIId, request.Status, request.UpdatedBy, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error while updating SPI status", ApplicationEventIds.Error, ex);
            return Result<SafetyPerformanceIndicator>.Failure<SafetyPerformanceIndicator>(DomainErrors.SPIError.StatusUpdateFailed);
        }
    }
}

public class ScheduleSPIReviewCommandHandler : BaseCommandBundle, IBaseRequestHandler<ScheduleSPIReviewCommand, Result<SafetyPerformanceIndicator>>
{
    private readonly SafetyPerformanceIndicatorService _spiService;
    private readonly ILogger<ScheduleSPIReviewCommandHandler> _logger;

    public ScheduleSPIReviewCommandHandler(
        SafetyPerformanceIndicatorService spiService,
        ILogger<ScheduleSPIReviewCommandHandler> logger)
    {
        _spiService = spiService ?? throw new ArgumentNullException(nameof(spiService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<SafetyPerformanceIndicator>> HandleAsync(ScheduleSPIReviewCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request is null)
            {
                _logger.LogApplicationError("ScheduleSPIReviewCommand received with null request", ApplicationEventIds.Error, null);
                return Result<SafetyPerformanceIndicator>.Failure<SafetyPerformanceIndicator>(DomainErrors.SPIError.NullOrEmpty);
            }

            return await _spiService.ScheduleReviewAsync(request.SPIId, request.ReviewDate, request.ScheduledBy, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error while scheduling SPI review", ApplicationEventIds.Error, ex);
            return Result<SafetyPerformanceIndicator>.Failure<SafetyPerformanceIndicator>(DomainErrors.SPIError.ReviewScheduleFailed);
        }
    }
}

public class CompleteSPIReviewCommandHandler : BaseCommandBundle, IBaseRequestHandler<CompleteSPIReviewCommand, Result<SafetyPerformanceIndicator>>
{
    private readonly SafetyPerformanceIndicatorService _spiService;
    private readonly ILogger<CompleteSPIReviewCommandHandler> _logger;

    public CompleteSPIReviewCommandHandler(
        SafetyPerformanceIndicatorService spiService,
        ILogger<CompleteSPIReviewCommandHandler> logger)
    {
        _spiService = spiService ?? throw new ArgumentNullException(nameof(spiService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<SafetyPerformanceIndicator>> HandleAsync(CompleteSPIReviewCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request is null)
            {
                _logger.LogApplicationError("CompleteSPIReviewCommand received with null request", ApplicationEventIds.Error, null);
                return Result<SafetyPerformanceIndicator>.Failure<SafetyPerformanceIndicator>(DomainErrors.SPIError.NullOrEmpty);
            }

            return await _spiService.CompleteReviewAsync(request.SPIId, request.ReviewNotes, request.NextReviewDate, request.ReviewedBy, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error while completing SPI review", ApplicationEventIds.Error, ex);
            return Result<SafetyPerformanceIndicator>.Failure<SafetyPerformanceIndicator>(DomainErrors.SPIError.ReviewCompletionFailed);
        }
    }
}

public class RecalculateSPIDashboardCommandHandler : BaseCommandBundle, IBaseRequestHandler<RecalculateSPIDashboardCommand, Result<bool>>
{
    private readonly IBaseMediator _mediator;
    private readonly IBaseEventBus _eventBus;
    private readonly ILogger<RecalculateSPIDashboardCommandHandler> _logger;

    public RecalculateSPIDashboardCommandHandler(
        IBaseMediator mediator,
        IBaseEventBus eventBus,
        ILogger<RecalculateSPIDashboardCommandHandler> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> HandleAsync(RecalculateSPIDashboardCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request is null)
            {
                _logger.LogApplicationError("RecalculateSPIDashboardCommand received with null request", ApplicationEventIds.Error, null);
                return Result<bool>.Failure<bool>(DomainErrors.SPIError.NullOrEmpty);
            }

            var calculationDate = request.CalculationDate ?? DateTime.UtcNow;
            var query = new GetSPIDashboardDataQuery(
                startDate: calculationDate.AddMonths(-12),
                endDate: calculationDate,
                spiIds: request.SPIIds,
                includeTrends: true,
                includeAlerts: true);

            var dashboardResult = await _mediator.SendAsync(query, cancellationToken);
            if (dashboardResult.IsFailure)
            {
                return Result<bool>.Failure<bool>(dashboardResult.Error);
            }

            var refreshEvent = new SPIDashboardRefreshEvent(
                affectedSPICodes: request.SPIIds ?? new List<string>(),
                refreshReason: "SPI dashboard recalculated",
                dashboardSection: SPIDashboardSection.All,
                refreshEntireDashboard: true,
                userId: request.RequestedBy,
                priority: UIEventPriority.Normal);

            var refreshResult = await _eventBus.PublishUIEventAsync(refreshEvent, EventExecutionMode.Manual, cancellationToken);
            if (refreshResult.IsFailure)
            {
                _logger.LogApplicationWarning("SPI dashboard recalculation completed but refresh publish failed: {Error}", refreshResult.Error?.Message);
            }

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error while recalculating SPI dashboard", ApplicationEventIds.Error, ex);
            return Result<bool>.Failure<bool>(DomainErrors.SPIError.UpdateFailed);
        }
    }
}

public class GenerateSPIAlertsCommandHandler : BaseCommandBundle, IBaseRequestHandler<GenerateSPIAlertsCommand, Result<List<SPIAlertResult>>>
{
    private readonly SafetyPerformanceIndicatorService _spiService;
    private readonly ILogger<GenerateSPIAlertsCommandHandler> _logger;

    public GenerateSPIAlertsCommandHandler(
        SafetyPerformanceIndicatorService spiService,
        ILogger<GenerateSPIAlertsCommandHandler> logger)
    {
        _spiService = spiService ?? throw new ArgumentNullException(nameof(spiService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<List<SPIAlertResult>>> HandleAsync(GenerateSPIAlertsCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request is null)
            {
                _logger.LogApplicationError("GenerateSPIAlertsCommand received with null request", ApplicationEventIds.Error, null);
                return Result<List<SPIAlertResult>>.Failure<List<SPIAlertResult>>(DomainErrors.SPIError.NullOrEmpty);
            }

            var alertsResult = await _spiService.GetAlertsAsync(
                spiIds: request.SPIIds,
                activeAlertsOnly: true,
                startDate: request.CheckDate?.Date,
                endDate: request.CheckDate?.Date.AddDays(1).AddTicks(-1),
                ct: cancellationToken);

            if (alertsResult.IsFailure)
            {
                return Result<List<SPIAlertResult>>.Failure<List<SPIAlertResult>>(alertsResult.Error);
            }

            var mappedAlerts = alertsResult.Value.Select(alert => new SPIAlertResult
            {
                SPIId = alert.SPIId,
                SPIName = alert.SPIName,
                AlertType = alert.AlertType,
                CurrentValue = alert.CurrentValue,
                ThresholdValue = alert.ThresholdValue,
                AlertMessage = alert.AlertMessage,
                AlertDate = alert.AlertDate,
                TrendDirection = alert.TrendDirection
            }).ToList();

            return Result<List<SPIAlertResult>>.Success(mappedAlerts);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error while generating SPI alerts", ApplicationEventIds.Error, ex);
            return Result<List<SPIAlertResult>>.Failure<List<SPIAlertResult>>(DomainErrors.SPIError.AlertConfigurationFailed);
        }
    }
}
}

public class UpdateSafetyPerformanceIndicatorCommandHandler : BaseCommandBundle, IBaseRequestHandler<UpdateSafetyPerformanceIndicatorCommand, Result<SafetyPerformanceIndicator>>
{
    private readonly ISafetyPerformanceIndicatorService _spiService;
    private readonly ILogger<UpdateSafetyPerformanceIndicatorCommandHandler> _logger;

    public UpdateSafetyPerformanceIndicatorCommandHandler(
        ISafetyPerformanceIndicatorService spiService,
        ILogger<UpdateSafetyPerformanceIndicatorCommandHandler> logger)
    {
        _spiService = spiService ?? throw new ArgumentNullException(nameof(spiService));
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
                _logger.LogApplicationError("UpdateSafetyPerformanceIndicatorCommand received with null request", ApplicationEventIds.Error, null);
                return Result<SafetyPerformanceIndicator>.Failure<SafetyPerformanceIndicator>(
                    DomainErrors.SPIError.NullOrEmpty);
            }

            _logger.LogApplicationInformation(" Processing UpdateSafetyPerformanceIndicatorCommand for ID: {Id}", request.Id?.Value);

            // First, get the existing SPI to update
            var existingSpiResult = await _spiService.GetSafetyPerformanceIndicatorByIdAsync(request.Id, cancellationToken);
            if (existingSpiResult.IsFailure)
            {
                _logger.LogApplicationError("Failed to retrieve existing SPI with ID: {Id}", ApplicationEventIds.Error, null);
                return Result<SafetyPerformanceIndicator>.Failure<SafetyPerformanceIndicator>(existingSpiResult.Error);
            }

            var existingSpi = existingSpiResult.Value;

            // Update the existing SPI properties directly
            existingSpi.Name = request.Name;
            existingSpi.Description = request.Description;
            existingSpi.IndicatorType = request.IndicatorType;
            existingSpi.Status = request.Status;
            existingSpi.MeasurementUnit = request.MeasurementUnit;
            existingSpi.MeasurementFrequency = request.MeasurementFrequency;
            existingSpi.CalculationMethod = request.CalculationMethod;
            existingSpi.DataSource = request.DataSource;
            existingSpi.TargetValue = request.TargetValue;
            existingSpi.AcceptableRange = request.AcceptableRange;
            existingSpi.WarningThreshold = request.WarningThreshold;
            existingSpi.CriticalThreshold = request.CriticalThreshold;
            existingSpi.ResponsibleDepartment = request.ResponsibleDepartment;
            existingSpi.DataOwner = request.DataOwner;
            existingSpi.ReviewAuthority = request.ReviewAuthority;
            existingSpi.NextReviewDate = request.NextReviewDate;
            existingSpi.LastReviewDate = request.LastReviewDate;
            existingSpi.LastReviewNotes = request.LastReviewNotes;
            existingSpi.AlertsEnabled = request.AlertsEnabled;
            existingSpi.AlertRecipients = request.AlertRecipients;
            existingSpi.UpdatedBy = request.UpdatedBy;
            existingSpi.UpdatedDate = DateTime.UtcNow;

            // Update the SPI through the service
            var result = await _spiService.UpdateSafetyPerformanceIndicatorAsync(existingSpi, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation(" Successfully updated Safety Performance Indicator with ID: {Id}", 
                    request.Id?.Value);
            }
            else
            {
                _logger.LogApplicationError("Failed to update Safety Performance Indicator with ID: {Id}. Error: {Error}",
                    ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogApplicationWarning("UpdateSafetyPerformanceIndicatorCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while updating Safety Performance Indicator with ID: {Id}", ApplicationEventIds.Error, ex);
            return Result<SafetyPerformanceIndicator>.Failure<SafetyPerformanceIndicator>(DomainErrors.SPIError.UpdateFailed);
        }
    }
}

public class UpdateSPIDataPointCommandHandler : BaseCommandBundle, IBaseRequestHandler<UpdateSPIDataPointCommand, Result<SafetyPerformanceIndicator>>
{
    private readonly ISafetyPerformanceIndicatorService _spiService;
    private readonly IBaseEventBus _eventBus;
    private readonly ILogger<UpdateSPIDataPointCommandHandler> _logger;

    public UpdateSPIDataPointCommandHandler(
        ISafetyPerformanceIndicatorService spiService,
        IBaseEventBus eventBus,
        ILogger<UpdateSPIDataPointCommandHandler> logger)
    {
        _spiService = spiService ?? throw new ArgumentNullException(nameof(spiService));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<SafetyPerformanceIndicator>> HandleAsync(
        UpdateSPIDataPointCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            if (request?.DataPoint is null)
            {
                _logger.LogApplicationError("UpdateSPIDataPointCommand received with null request or DataPoint", ApplicationEventIds.Error, null);
                return Result<SafetyPerformanceIndicator>.Failure<SafetyPerformanceIndicator>(DomainErrors.SPIError.NullOrEmpty);
            }

            _logger.LogApplicationInformation("Processing UpdateSPIDataPointCommand for SPI: {SPIId}, DataPoint: {DataPointId}",
                request.DataPoint.SPIId, request.DataPoint.Id?.Value);

            // Update the data point through the SPI service
            // Note: We'll need to get the SPI first, update the datapoint, then save the SPI
            var spiResult = await _spiService.GetSafetyPerformanceIndicatorByCodeAsync(request.DataPoint.SPIId, cancellationToken);

            if (spiResult.IsFailure)
            {
                _logger.LogApplicationError("SPI not found with code: {SPICode}", ApplicationEventIds.Error, null);
                return Result<SafetyPerformanceIndicator>.Failure<SafetyPerformanceIndicator>(spiResult.Error);
            }

            var spi = spiResult.Value;

            // Find and update the specific data point
            var dataPointToUpdate = spi.DataPoints?.FirstOrDefault(dp => dp.Id?.Value == request.DataPoint.Id?.Value);
            if (dataPointToUpdate == null)
            {
                _logger.LogApplicationError("Data point not found with ID: {DataPointId}", ApplicationEventIds.Error, null);
                return Result<SafetyPerformanceIndicator>.Failure<SafetyPerformanceIndicator>(
                    new Error("DATAPOINT_NOT_FOUND", "Data point not found"));
            }

            var previousValue = dataPointToUpdate.Value;

            // Update the data point properties
            dataPointToUpdate.Value = request.DataPoint.Value;
            dataPointToUpdate.MeasurementDate = request.DataPoint.MeasurementDate;
            dataPointToUpdate.DataSource = request.DataPoint.DataSource;
            dataPointToUpdate.Period = request.DataPoint.Period;
            dataPointToUpdate.Notes = request.DataPoint.Notes;
            dataPointToUpdate.IsVerified = request.DataPoint.IsVerified;
            dataPointToUpdate.VerifiedBy = request.DataPoint.VerifiedBy;
            dataPointToUpdate.VerifiedDate = request.DataPoint.VerifiedDate;
            dataPointToUpdate.UpdatedBy = request.DataPoint.UpdatedBy ?? "SYSTEM";
            dataPointToUpdate.UpdatedDate = DateTime.UtcNow;

            // Save the updated SPI
            var updateResult = await _spiService.UpdateSafetyPerformanceIndicatorAsync(spi, cancellationToken);

            if (updateResult.IsSuccess)
            {
                _logger.LogApplicationInformation("Successfully updated SPI data point for SPI: {SPIId}, DataPoint: {DataPointId}",
                    request.DataPoint.SPIId, request.DataPoint.Id?.Value);

                await SPIDataPointEventPublisher.PublishAsync(_eventBus, _logger, updateResult.Value, request.DataPoint, previousValue, nameof(UpdateSPIDataPointCommandHandler), cancellationToken);
            }
            else
            {
                _logger.LogApplicationError("Failed to update SPI data point for SPI: {SPIId}. Error: {Error}",
                    ApplicationEventIds.Error, null);
            }

            return updateResult;
        }
        catch (OperationCanceledException)
        {
            _logger.LogApplicationWarning("UpdateSPIDataPointCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while updating SPI data point", ApplicationEventIds.Error, ex);
            return Result<SafetyPerformanceIndicator>.Failure<SafetyPerformanceIndicator>(DomainErrors.SPIError.UpdateFailed);
        }
    }
}

public class AddSPIDataPointCommandHandler : BaseCommandBundle, IBaseRequestHandler<AddSPIDataPointCommand, Result<SafetyPerformanceIndicator>>
{
    private readonly ISafetyPerformanceIndicatorService _spiService;
    private readonly IBaseEventBus _eventBus;
    private readonly ILogger<AddSPIDataPointCommandHandler> _logger;

    public AddSPIDataPointCommandHandler(
        ISafetyPerformanceIndicatorService spiService,
        IBaseEventBus eventBus,
        ILogger<AddSPIDataPointCommandHandler> logger)
    {
        _spiService = spiService ?? throw new ArgumentNullException(nameof(spiService));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<SafetyPerformanceIndicator>> HandleAsync(AddSPIDataPointCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request?.DataPoint is null || string.IsNullOrWhiteSpace(request.DataPoint.SPIId))
            {
                _logger.LogApplicationError("AddSPIDataPointCommand received with invalid request", ApplicationEventIds.Error, null);
                return Result<SafetyPerformanceIndicator>.Failure<SafetyPerformanceIndicator>(DomainErrors.SPIError.NullOrEmpty);
            }

            var result = await _spiService.AddSPIDataPointAsync(request.DataPoint.SPIId, request.DataPoint, cancellationToken);
            if (result.IsFailure || result.Value is null)
            {
                return result;
            }

            await SPIDataPointEventPublisher.PublishAsync(_eventBus, _logger, result.Value, request.DataPoint, null, nameof(AddSPIDataPointCommandHandler), cancellationToken);
            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogApplicationWarning("AddSPIDataPointCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error while adding SPI data point", ApplicationEventIds.Error, ex);
            return Result<SafetyPerformanceIndicator>.Failure<SafetyPerformanceIndicator>(DomainErrors.SPIError.DataPointAddFailed);
        }
    }
}

public class DeleteSPIDataPointCommandHandler : BaseCommandBundle, IBaseRequestHandler<DeleteSPIDataPointCommand, Result<SafetyPerformanceIndicator>>
{
    private readonly ISafetyPerformanceIndicatorService _spiService;
    private readonly IBaseEventBus _eventBus;
    private readonly ILogger<DeleteSPIDataPointCommandHandler> _logger;

    public DeleteSPIDataPointCommandHandler(
        ISafetyPerformanceIndicatorService spiService,
        IBaseEventBus eventBus,
        ILogger<DeleteSPIDataPointCommandHandler> logger)
    {
        _spiService = spiService ?? throw new ArgumentNullException(nameof(spiService));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<SafetyPerformanceIndicator>> HandleAsync(DeleteSPIDataPointCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request?.DataPoint is null || string.IsNullOrWhiteSpace(request.DataPoint.Code))
            {
                _logger.LogApplicationError("DeleteSPIDataPointCommand received with invalid request", ApplicationEventIds.Error, null);
                return Result<SafetyPerformanceIndicator>.Failure<SafetyPerformanceIndicator>(DomainErrors.SPIError.NullOrEmpty);
            }

            var deleteResult = await _spiService.DeleteSPIDataPointAsync(request.DataPoint.Code, cancellationToken);
            if (deleteResult.IsFailure || deleteResult.Value is null)
            {
                return deleteResult;
            }

            var refreshEvent = new SPIDashboardRefreshEvent(
                affectedSPICodes: new List<string> { deleteResult.Value.Code },
                refreshReason: "SPI data point deleted",
                dashboardSection: SPIDashboardSection.All,
                refreshEntireDashboard: true,
                priority: UIEventPriority.Normal,
                refreshMetadata: new Dictionary<string, object>
                {
                    { "SPICode", deleteResult.Value.Code },
                    { "Source", nameof(DeleteSPIDataPointCommandHandler) }
                });

            var refreshResult = await _eventBus.PublishUIEventAsync(refreshEvent, EventExecutionMode.Manual, cancellationToken);
            if (refreshResult.IsFailure)
            {
                _logger.LogApplicationWarning("Failed to publish SPIDashboardRefreshEvent after delete for SPI {SPICode}: {Error}",
                    deleteResult.Value.Code,
                    refreshResult.Error?.Message);
            }

            return deleteResult;
        }
        catch (OperationCanceledException)
        {
            _logger.LogApplicationWarning("DeleteSPIDataPointCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error while deleting SPI data point", ApplicationEventIds.Error, ex);
            return Result<SafetyPerformanceIndicator>.Failure<SafetyPerformanceIndicator>(DomainErrors.SPIError.DeleteFailed);
        }
    }
}

internal static class SPIDataPointEventPublisher
{
    public static async Task PublishAsync(
        IBaseEventBus eventBus,
        ILogger logger,
        SafetyPerformanceIndicator spi,
        SPIDataPoint currentDataPoint,
        decimal? previousValue,
        string source,
        CancellationToken cancellationToken)
    {
        var currentValue = currentDataPoint.Value;
        var previousStatus = ResolveComplianceStatus(previousValue, spi);
        var newStatus = ResolveComplianceStatus(currentValue, spi);

        if (previousStatus != newStatus)
        {
            var complianceThreshold = spi.TargetValue ?? spi.WarningThreshold ?? spi.CriticalThreshold ?? currentValue;
            var complianceEvent = new SPIComplianceChangedEvent(
                id: new SMSEventID("EV-0000"),
                spiCode: spi.Code,
                spiName: spi.Name,
                previousStatus: previousStatus,
                newStatus: newStatus,
                currentValue: currentValue,
                complianceThreshold: complianceThreshold,
                complianceStandard: "TARGET_ALIGNMENT",
                reportingPeriod: currentDataPoint.Period,
                complianceCheckDate: DateTime.UtcNow,
                requiresRegulatoryReporting: newStatus is SMS_Domain.Events.SPIComplianceStatus.NonCompliant or SMS_Domain.Events.SPIComplianceStatus.AtRisk,
                regulatoryBody: "FAA");

            var compliancePublishResult = await eventBus.PublishDomainEventAsync(complianceEvent, cancellationToken);
            if (compliancePublishResult.IsFailure)
            {
                logger.LogApplicationWarning("Failed to publish SPIComplianceChangedEvent for SPI {SPICode}: {Error}",
                    spi.Code,
                    compliancePublishResult.Error?.Message);
            }
        }

        if ((spi.CriticalThreshold.HasValue && currentValue >= spi.CriticalThreshold.Value) ||
            (spi.WarningThreshold.HasValue && currentValue >= spi.WarningThreshold.Value))
        {
            var threshold = spi.CriticalThreshold.HasValue && currentValue >= spi.CriticalThreshold.Value
                ? spi.CriticalThreshold.Value
                : spi.WarningThreshold!.Value;

            var severity = spi.CriticalThreshold.HasValue && currentValue >= spi.CriticalThreshold.Value
                ? SPISeverityLevel.Critical
                : SPISeverityLevel.High;

            var thresholdEvent = new SPIThresholdExceededEvent(
                id: new SMSEventID("EV-0000"),
                spiCode: spi.Code,
                spiName: spi.Name,
                currentValue: currentValue,
                thresholdValue: threshold,
                severity: severity,
                stakeholderGroups: new List<string>(),
                reportingPeriod: currentDataPoint.Period,
                aggregateId: spi.Id.Value);

            var thresholdPublishResult = await eventBus.PublishDomainEventAsync(thresholdEvent, cancellationToken);
            if (thresholdPublishResult.IsFailure)
            {
                logger.LogApplicationWarning("Failed to publish SPIThresholdExceededEvent for SPI {SPICode}: {Error}",
                    spi.Code,
                    thresholdPublishResult.Error?.Message);
            }
        }

        var refreshEvent = new SPIDashboardRefreshEvent(
            affectedSPICodes: new List<string> { spi.Code },
            refreshReason: "SPI data point updated",
            dashboardSection: SPIDashboardSection.All,
            refreshEntireDashboard: true,
            priority: UIEventPriority.Normal,
            refreshMetadata: new Dictionary<string, object>
            {
                { "SPICode", spi.Code },
                { "Source", source }
            });

        var refreshResult = await eventBus.PublishUIEventAsync(refreshEvent, EventExecutionMode.Manual, cancellationToken);
        if (refreshResult.IsFailure)
        {
            logger.LogApplicationWarning("Failed to publish SPIDashboardRefreshEvent for SPI {SPICode}: {Error}",
                spi.Code,
                refreshResult.Error?.Message);
        }
    }

    private static SMS_Domain.Events.SPIComplianceStatus ResolveComplianceStatus(decimal? value, SafetyPerformanceIndicator spi)
    {
        if (!value.HasValue)
        {
            return SMS_Domain.Events.SPIComplianceStatus.Unknown;
        }

        if (spi.TargetValue.HasValue && value.Value >= spi.TargetValue.Value)
        {
            return SMS_Domain.Events.SPIComplianceStatus.Compliant;
        }

        if (spi.WarningThreshold.HasValue && value.Value >= spi.WarningThreshold.Value)
        {
            return SMS_Domain.Events.SPIComplianceStatus.AtRisk;
        }

        return SMS_Domain.Events.SPIComplianceStatus.NonCompliant;
    }
}

public class DeleteSafetyPerformanceIndicatorCommandHandler : BaseCommandBundle, IBaseRequestHandler<DeleteSafetyPerformanceIndicatorCommand, Result<bool>>
{
    private readonly ISafetyPerformanceIndicatorService _spiService;
    private readonly ILogger<DeleteSafetyPerformanceIndicatorCommandHandler> _logger;

    public DeleteSafetyPerformanceIndicatorCommandHandler(
        ISafetyPerformanceIndicatorService spiService,
        ILogger<DeleteSafetyPerformanceIndicatorCommandHandler> logger)
    {
        _spiService = spiService ?? throw new ArgumentNullException(nameof(spiService));
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
                _logger.LogApplicationError("DeleteSafetyPerformanceIndicatorCommand received with null request or ID", ApplicationEventIds.Error, null);
                return Result<bool>.Failure<bool>(DomainErrors.SPIError.NullOrEmpty);
            }

            _logger.LogApplicationInformation(" Processing DeleteSafetyPerformanceIndicatorCommand for ID: {Id}",
                request.SafetyPerformanceIndicatorId?.Value);

            var result = await _spiService.DeleteSafetyPerformanceIndicatorAsync(request.SafetyPerformanceIndicatorId, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation(" Successfully deleted Safety Performance Indicator with ID: {Id}",
                    request.SafetyPerformanceIndicatorId?.Value);
            }
            else
            {
                _logger.LogApplicationError("Failed to delete Safety Performance Indicator with ID: {Id}. Error: {Error}",
                    ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogApplicationWarning("DeleteSafetyPerformanceIndicatorCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while deleting Safety Performance Indicator with ID: {Id}", ApplicationEventIds.Error, ex);
            return Result<bool>.Failure<bool>(DomainErrors.SPIError.DeleteFailed);
        }
    }
}


