using Microsoft.Extensions.Logging;
using SMS_Infrastructure.Services;
using SMS_Application.Messaging.Commands;
using SMS_Domain.Entities;
using SMS_Domain.Errors;
using SMS_Application.Interfaces;
using SMS_Shared.Common;

namespace SMS_Application.Messaging.CommandHandlers;

// =============================================
// HAZARD FILE COMMAND HANDLERS - Following Exact SMS Pattern
// =============================================

public class CreateHazardFileCommandHandler : BaseCommandBundle, IRequestHandler<CreateHazardFileCommand, Result<HazardFile>>
{
    private readonly HazardFileDataService _dataService;
    private readonly ILogger<CreateHazardFileCommandHandler> _logger;

    public CreateHazardFileCommandHandler(HazardFileDataService dataService, ILogger<CreateHazardFileCommandHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<HazardFile>> HandleAsync(CreateHazardFileCommand request, CancellationToken ct = default)
    {
        try
        {
            if (request?.HazardFile is null)
            {
                _logger.LogApplicationError("CreateHazardFileCommand received with null HazardFile", ApplicationEventIds.Error, null);
                return Result<HazardFile>.Failure<HazardFile>(DomainErrors.HazardFileError.NullOrEmpty);
            }

            _logger.LogInformation("Processing CreateHazardFileCommand for Code: {Code}, HazardCode: {HazardCode}", 
                request.HazardFile.Code, request.HazardFile.HazardCode);

            var result = await _dataService.CreateHazardFileAsync(request.HazardFile, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully created HazardFile with Code: {Code}, Size: {Size} bytes",
                    result.Value?.Code, result.Value?.FileSizeBytes);
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
    private readonly HazardFileDataService _dataService;
    private readonly ILogger<UpdateHazardFileCommandHandler> _logger;

    public UpdateHazardFileCommandHandler(HazardFileDataService dataService, ILogger<UpdateHazardFileCommandHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<HazardFile>> HandleAsync(UpdateHazardFileCommand request, CancellationToken ct = default)
    {
        try
        {
            if (request?.HazardFile is null)
            {
                _logger.LogApplicationError("UpdateHazardFileCommand received with null HazardFile", ApplicationEventIds.Error, null);
                return Result<HazardFile>.Failure<HazardFile>(DomainErrors.HazardFileError.NullOrEmpty);
            }

            _logger.LogInformation("Processing UpdateHazardFileCommand for Code: {Code}",
                request.HazardFile.Code);

            var result = await _dataService.UpdateHazardFileAsync(request.HazardFile, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully updated HazardFile with Code: {Code}", request.HazardFile.Code);
            }
            else
            {
                _logger.LogApplicationError("Failed to update HazardFile with Code: {Code}. Error: {Error}",
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
            _logger.LogApplicationError("Unexpected error occurred while updating HazardFile with Code: {Code}", ApplicationEventIds.Error, ex);
            return Result<HazardFile>.Failure<HazardFile>(DomainErrors.HazardFileError.UpdateFailed);
        }
    }
}

public class DeactivateHazardFileCommandHandler : BaseCommandBundle, IRequestHandler<DeactivateHazardFileCommand, Result<bool>>
{
    private readonly HazardFileDataService _dataService;
    private readonly ILogger<DeactivateHazardFileCommandHandler> _logger;

    public DeactivateHazardFileCommandHandler(HazardFileDataService dataService, ILogger<DeactivateHazardFileCommandHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> HandleAsync(DeactivateHazardFileCommand request, CancellationToken ct = default)
    {
        try
        {
            if (request?.FileId <= 0)
            {
                _logger.LogApplicationError("DeactivateHazardFileCommand received with invalid FileId", ApplicationEventIds.Error, null);
                return Result<bool>.Failure<bool>(DomainErrors.HazardFileError.NullOrEmpty);
            }

            _logger.LogInformation("Processing DeactivateHazardFileCommand for FileId: {FileId}, Reason: {Reason}", 
                request.FileId, request.Reason);

            var result = await _dataService.DeactivateHazardFileAsync(request.FileId, request.Reason, request.DeactivatedBy, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully deactivated HazardFile with FileId: {FileId}", request.FileId);
            }
            else
            {
                _logger.LogApplicationError("Failed to deactivate HazardFile with FileId: {FileId}. Error: {Error}",
                    ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("DeactivateHazardFileCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while deactivating HazardFile with FileId: {FileId}", ApplicationEventIds.Error, ex);
            return Result<bool>.Failure<bool>(DomainErrors.HazardFileError.DeleteFailed);
        }
    }
}

public class ReactivateHazardFileCommandHandler : BaseCommandBundle, IRequestHandler<ReactivateHazardFileCommand, Result<bool>>
{
    private readonly HazardFileDataService _dataService;
    private readonly ILogger<ReactivateHazardFileCommandHandler> _logger;

    public ReactivateHazardFileCommandHandler(HazardFileDataService dataService, ILogger<ReactivateHazardFileCommandHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> HandleAsync(ReactivateHazardFileCommand request, CancellationToken ct = default)
    {
        try
        {
            if (request?.FileId <= 0)
            {
                _logger.LogApplicationError("ReactivateHazardFileCommand received with invalid FileId", ApplicationEventIds.Error, null);
                return Result<bool>.Failure<bool>(DomainErrors.HazardFileError.NullOrEmpty);
            }

            _logger.LogInformation("Processing ReactivateHazardFileCommand for FileId: {FileId}", request.FileId);

            var result = await _dataService.ReactivateHazardFileAsync(request.FileId, request.ReactivatedBy, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully reactivated HazardFile with FileId: {FileId}", request.FileId);
            }
            else
            {
                _logger.LogApplicationError("Failed to reactivate HazardFile with FileId: {FileId}. Error: {Error}",
                    ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("ReactivateHazardFileCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while reactivating HazardFile with FileId: {FileId}", ApplicationEventIds.Error, ex);
            return Result<bool>.Failure<bool>(DomainErrors.HazardFileError.UpdateFailed);
        }
    }
}

public class SetHazardFileConfidentialityCommandHandler : BaseCommandBundle, IRequestHandler<SetHazardFileConfidentialityCommand, Result<bool>>
{
    private readonly HazardFileDataService _dataService;
    private readonly ILogger<SetHazardFileConfidentialityCommandHandler> _logger;

    public SetHazardFileConfidentialityCommandHandler(HazardFileDataService dataService, ILogger<SetHazardFileConfidentialityCommandHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> HandleAsync(SetHazardFileConfidentialityCommand request, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request?.FileCode))
            {
                _logger.LogApplicationError("SetHazardFileConfidentialityCommand received with null FileCode", ApplicationEventIds.Error, null);
                return Result<bool>.Failure<bool>(DomainErrors.HazardFileError.NullOrEmpty);
            }

            _logger.LogInformation("Processing SetHazardFileConfidentialityCommand for FileCode: {FileCode}, Confidential: {IsConfidential}", 
                request.FileCode, request.IsConfidential);

            var result = await _dataService.SetFileConfidentialityAsync(request.FileCode, request.IsConfidential, request.UpdatedBy, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully set confidentiality for HazardFile with FileCode: {FileCode}", request.FileCode);
            }
            else
            {
                _logger.LogApplicationError("Failed to set confidentiality for HazardFile with FileCode: {FileCode}. Error: {Error}",
                    ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("SetHazardFileConfidentialityCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while setting confidentiality for HazardFile with FileCode: {FileCode}", ApplicationEventIds.Error, ex);
            return Result<bool>.Failure<bool>(DomainErrors.HazardFileError.UpdateFailed);
        }
    }
}