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

using Microsoft.Extensions.Logging;

namespace SMS_Application.Messaging.CommandHandlers;

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

            _logger.LogInformation("✅ Clean Architecture: Processing CreateSafetyPerformanceIndicatorCommand for Name: {Name}", request.Name);

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
                _logger.LogInformation("✅ Clean Architecture: Successfully created Safety Performance Indicator with ID: {Id}, Name: {Name}",
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
            _logger.LogWarning("CreateSafetyPerformanceIndicatorCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while creating Safety Performance Indicator", ApplicationEventIds.Error, ex);
            return Result<SafetyPerformanceIndicator>.Failure<SafetyPerformanceIndicator>(DomainErrors.SPIError.CreateFailed);
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

            _logger.LogInformation("✅ Clean Architecture: Processing UpdateSafetyPerformanceIndicatorCommand for ID: {Id}", request.Id?.Value);

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
                _logger.LogInformation("✅ Clean Architecture: Successfully updated Safety Performance Indicator with ID: {Id}", 
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
            _logger.LogWarning("UpdateSafetyPerformanceIndicatorCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while updating Safety Performance Indicator with ID: {Id}", ApplicationEventIds.Error, ex);
            return Result<SafetyPerformanceIndicator>.Failure<SafetyPerformanceIndicator>(DomainErrors.SPIError.UpdateFailed);
        }
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

            _logger.LogInformation("✅ Clean Architecture: Processing DeleteSafetyPerformanceIndicatorCommand for ID: {Id}",
                request.SafetyPerformanceIndicatorId?.Value);

            var result = await _spiService.DeleteSafetyPerformanceIndicatorAsync(request.SafetyPerformanceIndicatorId, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("✅ Clean Architecture: Successfully deleted Safety Performance Indicator with ID: {Id}",
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
            _logger.LogWarning("DeleteSafetyPerformanceIndicatorCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while deleting Safety Performance Indicator with ID: {Id}", ApplicationEventIds.Error, ex);
            return Result<bool>.Failure<bool>(DomainErrors.SPIError.DeleteFailed);
        }
    }
}
