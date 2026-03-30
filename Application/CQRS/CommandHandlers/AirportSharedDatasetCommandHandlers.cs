//-----------------------------------------------------------------------
// <copyright file="AirportSharedDatasetCommandHandlers.cs" company="SMS Safety Management System">
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
// AIRPORT SHARED DATASET COMMAND HANDLERS - Clean Architecture Pattern
// =============================================

public class CreateAirportSharedDatasetCommandHandler : BaseCommandBundle, IRequestHandler<CreateAirportSharedDatasetCommand, Result<AirportSharedDataset>>
{
    private readonly AirportSharedDatasetService _airportSharedDatasetService;
    private readonly ILogger<CreateAirportSharedDatasetCommandHandler> _logger;

    public CreateAirportSharedDatasetCommandHandler(AirportSharedDatasetService airportSharedDatasetService, ILogger<CreateAirportSharedDatasetCommandHandler> logger)
    {
        _airportSharedDatasetService = airportSharedDatasetService ?? throw new ArgumentNullException(nameof(airportSharedDatasetService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<AirportSharedDataset>> HandleAsync(CreateAirportSharedDatasetCommand request, CancellationToken ct = default)
    {
        try
        {
            if (request?.AirportSharedDataset is null)
            {
                _logger.LogApplicationError("CreateAirportSharedDatasetCommand received with null AirportSharedDataset", ApplicationEventIds.Error, null);
                return Result<AirportSharedDataset>.Failure<AirportSharedDataset>(DomainErrors.AirportSharedDatasetError.NullOrEmpty);
            }

            // Validate Airport Shared Dataset Code
            if (string.IsNullOrWhiteSpace(request.AirportSharedDataset.Code))
            {
                _logger.LogApplicationError("CreateAirportSharedDatasetCommand received with null or empty Code", ApplicationEventIds.Error, null);
                return Result<AirportSharedDataset>.Failure<AirportSharedDataset>(DomainErrors.AirportSharedDatasetError.CodeRequired);
            }

            // Validate REQUIRED ReportID 
            if (string.IsNullOrWhiteSpace(request.AirportSharedDataset.ReportCode))
            {
                _logger.LogApplicationError("CreateAirportSharedDatasetCommand received with null or empty ReportCode", ApplicationEventIds.Error, null);
                return Result<AirportSharedDataset>.Failure<AirportSharedDataset>(DomainErrors.AirportSharedDatasetError.ReportIDRequired);
            }

            _logger.LogInformation("✅ Clean Architecture: Processing CreateAirportSharedDatasetCommand for Code: {Code}, ReportID: {ReportCode}",
                request.AirportSharedDataset.Code, request.AirportSharedDataset.ReportCode);

            var result = await _airportSharedDatasetService.CreateAirportSharedDatasetAsync(request.AirportSharedDataset, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("✅ Clean Architecture: Successfully created AirportSharedDataset with ID: {Id}, Code: {Code}, ReportID: {ReportCode}",
                    result.Value?.Id, result.Value?.Code, result.Value?.ReportCode);
            }
            else
            {
                _logger.LogApplicationError("Failed to create AirportSharedDataset with Code: {Code}, ReportID: {ReportCode}. Error: {Error}",
                    ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("CreateAirportSharedDatasetCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while creating AirportSharedDataset with Code: {Code}, ReportID: {ReportCode}",
                ApplicationEventIds.Error, ex);
            return Result<AirportSharedDataset>.Failure<AirportSharedDataset>(DomainErrors.AirportSharedDatasetError.CreateFailed);
        }
    }
}

public class UpdateAirportSharedDatasetCommandHandler : BaseCommandBundle, IRequestHandler<UpdateAirportSharedDatasetCommand, Result<AirportSharedDataset>>
{
    private readonly AirportSharedDatasetService _airportSharedDatasetService;
    private readonly ILogger<UpdateAirportSharedDatasetCommandHandler> _logger;

    public UpdateAirportSharedDatasetCommandHandler(AirportSharedDatasetService airportSharedDatasetService, ILogger<UpdateAirportSharedDatasetCommandHandler> logger)
    {
        _airportSharedDatasetService = airportSharedDatasetService ?? throw new ArgumentNullException(nameof(airportSharedDatasetService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<AirportSharedDataset>> HandleAsync(UpdateAirportSharedDatasetCommand request, CancellationToken ct = default)
    {
        try
        {
            if (request?.AirportSharedDataset is null)
            {
                _logger.LogApplicationError("UpdateAirportSharedDatasetCommand received with null AirportSharedDataset", ApplicationEventIds.Error, null);
                return Result<AirportSharedDataset>.Failure<AirportSharedDataset>(DomainErrors.AirportSharedDatasetError.NullOrEmpty);
            }

            // Validate Airport Shared Dataset Code
            if (string.IsNullOrWhiteSpace(request.AirportSharedDataset.Code))
            {
                _logger.LogApplicationError("UpdateAirportSharedDatasetCommand received with null or empty Code", ApplicationEventIds.Error, null);
                return Result<AirportSharedDataset>.Failure<AirportSharedDataset>(DomainErrors.AirportSharedDatasetError.CodeRequired);
            }

            // Validate REQUIRED ReportID 
            if (string.IsNullOrWhiteSpace(request.AirportSharedDataset.ReportCode))
            {
                _logger.LogApplicationError("UpdateAirportSharedDatasetCommand received with null or empty ReportCode", ApplicationEventIds.Error, null);
                return Result<AirportSharedDataset>.Failure<AirportSharedDataset>(DomainErrors.AirportSharedDatasetError.ReportIDRequired);
            }

            _logger.LogInformation("✅ Clean Architecture: Processing UpdateAirportSharedDatasetCommand for ID: {Id}, Code: {Code}, ReportID: {ReportCode}",
                request.AirportSharedDataset.Id, request.AirportSharedDataset.Code, request.AirportSharedDataset.ReportCode);

            var result = await _airportSharedDatasetService.UpdateAirportSharedDatasetAsync(request.AirportSharedDataset, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("✅ Clean Architecture: Successfully updated AirportSharedDataset with ID: {Id}", request.AirportSharedDataset.Id);
            }
            else
            {
                _logger.LogApplicationError("Failed to update AirportSharedDataset with ID: {Id}, Code: {Code}, ReportID: {ReportCode}. Error: {Error}",
                    ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("UpdateAirportSharedDatasetCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while updating AirportSharedDataset with ID: {Id}, Code: {Code}, ReportID: {ReportCode}",
                ApplicationEventIds.Error, ex);
            return Result<AirportSharedDataset>.Failure<AirportSharedDataset>(DomainErrors.AirportSharedDatasetError.UpdateFailed);
        }
    }
}

public class DeleteAirportSharedDatasetCommandHandler : BaseCommandBundle, IRequestHandler<DeleteAirportSharedDatasetCommand, Result<bool>>
{
    private readonly AirportSharedDatasetService _airportSharedDatasetService;
    private readonly ILogger<DeleteAirportSharedDatasetCommandHandler> _logger;

    public DeleteAirportSharedDatasetCommandHandler(AirportSharedDatasetService airportSharedDatasetService, ILogger<DeleteAirportSharedDatasetCommandHandler> logger)
    {
        _airportSharedDatasetService = airportSharedDatasetService ?? throw new ArgumentNullException(nameof(airportSharedDatasetService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> HandleAsync(DeleteAirportSharedDatasetCommand request, CancellationToken ct = default)
    {
        try
        {
            if (request?.AirportSharedDatasetId is null)
            {
                _logger.LogApplicationError("DeleteAirportSharedDatasetCommand received with null AirportSharedDatasetId", ApplicationEventIds.Error, null);
                return Result<bool>.Failure<bool>(DomainErrors.AirportSharedDatasetError.NullOrEmpty);
            }

            _logger.LogInformation("✅ Clean Architecture: Processing DeleteAirportSharedDatasetCommand for ID: {Id}", request.AirportSharedDatasetId);

            var result = await _airportSharedDatasetService.DeleteAirportSharedDatasetAsync(request.AirportSharedDatasetId, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("✅ Clean Architecture: Successfully deleted AirportSharedDataset with ID: {Id}", request.AirportSharedDatasetId);
            }
            else
            {
                _logger.LogApplicationError("Failed to delete AirportSharedDataset with ID: {Id}. Error: {Error}",
                    ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("DeleteAirportSharedDatasetCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while deleting AirportSharedDataset with ID: {Id}", ApplicationEventIds.Error, ex);
            return Result<bool>.Failure<bool>(DomainErrors.AirportSharedDatasetError.DeleteFailed);
        }
    }
}
