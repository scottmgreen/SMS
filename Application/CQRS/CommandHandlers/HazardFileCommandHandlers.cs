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

namespace SMS_Application.CommandHandlers;

// =============================================
// HAZARD FILE COMMAND HANDLERS - Clean Architecture Pattern
// =============================================

public class CreateHazardFileCommandHandler : BaseCommandBundle, IBaseRequestHandler<CreateHazardFileCommand, Result<HazardFile>>
{
    private readonly IHazardFileService _hazardFileService;
    private readonly HazardFileExternalStorageService _externalStorageService;
    private readonly ILogger<CreateHazardFileCommandHandler> _logger;

    public CreateHazardFileCommandHandler(
        IHazardFileService hazardFileService,
        HazardFileExternalStorageService externalStorageService,
        ILogger<CreateHazardFileCommandHandler> logger)
    {
        _hazardFileService = hazardFileService ?? throw new ArgumentNullException(nameof(hazardFileService));
        _externalStorageService = externalStorageService ?? throw new ArgumentNullException(nameof(externalStorageService));
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

            _logger.LogApplicationInformation(" Processing CreateHazardFileCommand for Code: {Code}", request.HazardFile.Code);

            var shouldPrepareExternalStorage =
                !string.IsNullOrWhiteSpace(request.HazardFile.HazardCode)
                && request.HazardFile.FileData is { Length: > 0 };

            if (shouldPrepareExternalStorage)
            {
                var preparationResult = await _externalStorageService.PrepareHazardFileForStorageAsync(
                    request.HazardFile,
                    request.HazardFile.FileData!,
                    cancellationToken);

                if (preparationResult.IsFailure || preparationResult.Value is null)
                {
                    _logger.LogApplicationError("Failed to prepare hazard file storage for Code: {Code}. Error: {Error}",
                        request.HazardFile.Code,
                        preparationResult.Error?.Message);

                    return Result<HazardFile>.Failure<HazardFile>(
                        preparationResult.Error ?? DomainErrors.HazardFileError.CreateFailed);
                }

                request.HazardFile = preparationResult.Value;
            }

            var result = await _hazardFileService.CreateHazardFileAsync(request.HazardFile, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation(" Successfully created HazardFile with ID: {Id}, Code: {Code}",
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
            _logger.LogApplicationWarning("CreateHazardFileCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while creating HazardFile", ApplicationEventIds.Error, ex);
            return Result<HazardFile>.Failure<HazardFile>(DomainErrors.HazardFileError.CreateFailed);
        }
    }
}
public class DeactivateHazardFileCommandHandler : BaseCommandBundle, IBaseRequestHandler<DeactivateHazardFileCommand, Result<bool>>
{
    private readonly IHazardFileService _hazardFileService;
    private readonly ILogger<DeactivateHazardFileCommandHandler> _logger;

    public DeactivateHazardFileCommandHandler(IHazardFileService hazardFileService, ILogger<DeactivateHazardFileCommandHandler> logger)
    {
        _hazardFileService = hazardFileService ?? throw new ArgumentNullException(nameof(hazardFileService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> HandleAsync(DeactivateHazardFileCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request is null || string.IsNullOrWhiteSpace(request.FileCode))
            {
                _logger.LogApplicationError("DeactivateHazardFileCommand received with null request or file code", ApplicationEventIds.Error, null);
                return Result<bool>.Failure<bool>(DomainErrors.HazardFileError.NullOrEmpty);
            }

            _logger.LogApplicationInformation("Processing DeactivateHazardFileCommand for Code: {Code}", request.FileCode);

            var result = await _hazardFileService.DeactivateHazardFileAsync(
                request.FileCode,
                request.Reason,
                request.DeactivatedBy,
                cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation("Successfully deactivated HazardFile with Code: {Code}", request.FileCode);
            }
            else
            {
                _logger.LogApplicationError("Failed to deactivate HazardFile with Code: {Code}. Error: {Error}", ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogApplicationWarning("DeactivateHazardFileCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while deactivating HazardFile with Code: {Code}", ApplicationEventIds.Error, ex);
            return Result<bool>.Failure<bool>(DomainErrors.HazardFileError.UpdateFailed);
        }
    }
}

public class ReactivateHazardFileCommandHandler : BaseCommandBundle, IBaseRequestHandler<ReactivateHazardFileCommand, Result<bool>>
{
    private readonly IHazardFileService _hazardFileService;
    private readonly ILogger<ReactivateHazardFileCommandHandler> _logger;

    public ReactivateHazardFileCommandHandler(IHazardFileService hazardFileService, ILogger<ReactivateHazardFileCommandHandler> logger)
    {
        _hazardFileService = hazardFileService ?? throw new ArgumentNullException(nameof(hazardFileService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> HandleAsync(ReactivateHazardFileCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request is null || string.IsNullOrWhiteSpace(request.FileCode))
            {
                _logger.LogApplicationError("ReactivateHazardFileCommand received with null request or file code", ApplicationEventIds.Error, null);
                return Result<bool>.Failure<bool>(DomainErrors.HazardFileError.NullOrEmpty);
            }

            _logger.LogApplicationInformation("Processing ReactivateHazardFileCommand for Code: {Code}", request.FileCode);

            var result = await _hazardFileService.ReactivateHazardFileAsync(
                request.FileCode,
                request.ReactivatedBy,
                cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation("Successfully reactivated HazardFile with Code: {Code}", request.FileCode);
            }
            else
            {
                _logger.LogApplicationError("Failed to reactivate HazardFile with Code: {Code}. Error: {Error}", ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogApplicationWarning("ReactivateHazardFileCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while reactivating HazardFile with Code: {Code}", ApplicationEventIds.Error, ex);
            return Result<bool>.Failure<bool>(DomainErrors.HazardFileError.UpdateFailed);
        }
    }
}
public class UpdateHazardFileCommandHandler : BaseCommandBundle, IBaseRequestHandler<UpdateHazardFileCommand, Result<HazardFile>>
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

            _logger.LogApplicationInformation(" Processing UpdateHazardFileCommand for ID: {Id}", request.HazardFile.Id);

            var result = await _hazardFileService.UpdateHazardFileAsync(request.HazardFile, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation(" Successfully updated HazardFile with ID: {Id}", request.HazardFile.Id);
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
            _logger.LogApplicationWarning("UpdateHazardFileCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while updating HazardFile with ID: {Id}", ApplicationEventIds.Error, ex);
            return Result<HazardFile>.Failure<HazardFile>(DomainErrors.HazardFileError.UpdateFailed);
        }
    }
}

