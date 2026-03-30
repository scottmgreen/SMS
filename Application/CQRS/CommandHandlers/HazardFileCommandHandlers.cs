//-----------------------------------------------------------------------
// <copyright file="HazardFileCommandHandlers.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Command handlers implementing SMS hazard file management business logic and operations.
//                  Implements command handlers for processing write operations.
//                  Handles business logic execution and domain entity coordination.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;

using Microsoft.Extensions.Logging;

namespace SMS_Application.Messaging.CommandHandlers;

// =============================================
// HAZARD FILE COMMAND HANDLERS - Clean Architecture Pattern
// =============================================

public class CreateHazardFileCommandHandler : BaseCommandBundle, IRequestHandler<CreateHazardFileCommand, Result<HazardFile>>
{
    private readonly IHazardFileService _hazardFileService;
    private readonly ILogger<CreateHazardFileCommandHandler> _logger;

    public CreateHazardFileCommandHandler(IHazardFileService hazardFileService, ILogger<CreateHazardFileCommandHandler> logger)
    {
        _hazardFileService = hazardFileService ?? throw new ArgumentNullException(nameof(hazardFileService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<HazardFile>> HandleAsync(CreateHazardFileCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request?.HazardFile is null)
            {
                _logger.LogApplicationError("CreateHazardFileCommand received with null request or hazard file", ApplicationEventIds.Error, null);
                return Result<HazardFile>.Failure<HazardFile>(DomainErrors.HazardFileError.NullOrEmpty);
            }

            _logger.LogInformation("✅ Clean Architecture: Processing CreateHazardFileCommand for Code: {Code}", request.HazardFile.Code);

            var result = await _hazardFileService.CreateHazardFileAsync(request.HazardFile, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("✅ Clean Architecture: Successfully created HazardFile with ID: {Id}, Code: {Code}",
                    result.Value?.Id, result.Value?.Code);
            }
            else
            {
                _logger.LogApplicationError("Failed to create HazardFile with Code: {Code}. Error: {Error}",
                    ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("CreateHazardFileCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while creating HazardFile", ApplicationEventIds.Error, ex);
            return Result<HazardFile>.Failure<HazardFile>(DomainErrors.HazardFileError.CreateFailed);
        }
    }
}

public class UpdateHazardFileCommandHandler : BaseCommandBundle, IRequestHandler<UpdateHazardFileCommand, Result<HazardFile>>
{
    private readonly IHazardFileService _hazardFileService;
    private readonly ILogger<UpdateHazardFileCommandHandler> _logger;

    public UpdateHazardFileCommandHandler(IHazardFileService hazardFileService, ILogger<UpdateHazardFileCommandHandler> logger)
    {
        _hazardFileService = hazardFileService ?? throw new ArgumentNullException(nameof(hazardFileService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<HazardFile>> HandleAsync(UpdateHazardFileCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request?.HazardFile is null)
            {
                _logger.LogApplicationError("UpdateHazardFileCommand received with null request or hazard file", ApplicationEventIds.Error, null);
                return Result<HazardFile>.Failure<HazardFile>(DomainErrors.HazardFileError.NullOrEmpty);
            }

            _logger.LogInformation("✅ Clean Architecture: Processing UpdateHazardFileCommand for ID: {Id}", request.HazardFile.Id);

            var result = await _hazardFileService.UpdateHazardFileAsync(request.HazardFile, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("✅ Clean Architecture: Successfully updated HazardFile with ID: {Id}", request.HazardFile.Id);
            }
            else
            {
                _logger.LogApplicationError("Failed to update HazardFile with ID: {Id}. Error: {Error}",
                    ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("UpdateHazardFileCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while updating HazardFile with ID: {Id}", ApplicationEventIds.Error, ex);
            return Result<HazardFile>.Failure<HazardFile>(DomainErrors.HazardFileError.UpdateFailed);
        }
    }
}
