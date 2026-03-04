//-----------------------------------------------------------------------
// <copyright file="MitigationCommandHandlers.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Command handlers implementing business logic for SMS write operations.
//                  Implements command handlers for processing write operations.
//                  Handles business logic execution and domain entity coordination.
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.Extensions.Logging;

namespace SMS_Application.Messaging.CommandHandlers;

// =============================================
// MITIGATION COMMAND HANDLERS - Clean Architecture Pattern
// =============================================

public class CreateMitigationCommandHandler : BaseCommandBundle, IRequestHandler<CreateMitigationCommand, Result<Mitigation>>
{
    private readonly IMitigationService _mitigationService;
    private readonly ILogger<CreateMitigationCommandHandler> _logger;

    public CreateMitigationCommandHandler(IMitigationService mitigationService, ILogger<CreateMitigationCommandHandler> logger)
    {
        _mitigationService = mitigationService ?? throw new ArgumentNullException(nameof(mitigationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<Mitigation>> HandleAsync(CreateMitigationCommand request, CancellationToken ct = default)
    {
        try
        {
            if (request?.Mitigation is null)
            {
                _logger.LogApplicationError("CreateMitigationCommand received with null Mitigation", ApplicationEventIds.Error, null);
                return Result<Mitigation>.Failure<Mitigation>(DomainErrors.MitigationError.NullOrEmpty);
            }

            _logger.LogInformation("✅ Clean Architecture: Processing CreateMitigationCommand for Code: {Code}", request.Mitigation.Code);

            var result = await _mitigationService.CreateMitigationAsync(request.Mitigation, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("✅ Clean Architecture: Successfully created Mitigation with Code: {Code}",
                    result.Value?.Code);
            }
            else
            {
                _logger.LogApplicationError("Failed to create Mitigation with Code: {Code}. Error: {Error}",
                    ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("CreateMitigationCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while creating Mitigation", ApplicationEventIds.Error, ex);
            return Result<Mitigation>.Failure<Mitigation>(DomainErrors.MitigationError.CreateFailed);
        }
    }
}

public class UpdateMitigationCommandHandler : BaseCommandBundle, IRequestHandler<UpdateMitigationCommand, Result<Mitigation>>
{
    private readonly IMitigationService _mitigationService;
    private readonly ILogger<UpdateMitigationCommandHandler> _logger;

    public UpdateMitigationCommandHandler(IMitigationService mitigationService, ILogger<UpdateMitigationCommandHandler> logger)
    {
        _mitigationService = mitigationService ?? throw new ArgumentNullException(nameof(mitigationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<Mitigation>> HandleAsync(UpdateMitigationCommand request, CancellationToken ct = default)
    {
        try
        {
            if (request?.Mitigation is null)
            {
                _logger.LogApplicationError("UpdateMitigationCommand received with null Mitigation", ApplicationEventIds.Error, null);
                return Result<Mitigation>.Failure<Mitigation>(DomainErrors.MitigationError.NullOrEmpty);
            }

            _logger.LogInformation("✅ Clean Architecture: Processing UpdateMitigationCommand for Code: {Code}",
                request.Mitigation.Code);

            var result = await _mitigationService.UpdateMitigationAsync(request.Mitigation, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("✅ Clean Architecture: Successfully updated Mitigation with Code: {Code}", request.Mitigation.Code);
            }
            else
            {
                _logger.LogApplicationError("Failed to update Mitigation with Code: {Code}. Error: {Error}",
                    ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("UpdateMitigationCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while updating Mitigation with Code: {Code}", ApplicationEventIds.Error, ex);
            return Result<Mitigation>.Failure<Mitigation>(DomainErrors.MitigationError.UpdateFailed);
        }
    }
}

public class DeleteMitigationCommandHandler : BaseCommandBundle, IRequestHandler<DeleteMitigationCommand, Result<bool>>
{
    private readonly IMitigationService _mitigationService;
    private readonly ILogger<DeleteMitigationCommandHandler> _logger;

    public DeleteMitigationCommandHandler(IMitigationService mitigationService, ILogger<DeleteMitigationCommandHandler> logger)
    {
        _mitigationService = mitigationService ?? throw new ArgumentNullException(nameof(mitigationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> HandleAsync(DeleteMitigationCommand request, CancellationToken ct = default)
    {
        try
        {
            if (request?.MitigationId is null)
            {
                _logger.LogApplicationError("DeleteMitigationCommand received with null MitigationId", ApplicationEventIds.Error, null);
                return Result<bool>.Failure<bool>(DomainErrors.MitigationError.NullOrEmpty);
            }

            _logger.LogInformation("✅ Clean Architecture: Processing DeleteMitigationCommand for Code: {Code}", request.MitigationId);

            var result = await _mitigationService.DeleteMitigationAsync(request.MitigationId, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("✅ Clean Architecture: Successfully deleted Mitigation with Code: {Code}", request.MitigationId);
            }
            else
            {
                _logger.LogApplicationError("Failed to delete Mitigation with Code: {Code}. Error: {Error}",
                    ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("DeleteMitigationCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while deleting Mitigation with Code: {Code}", ApplicationEventIds.Error, ex);
            return Result<bool>.Failure<bool>(DomainErrors.MitigationError.DeleteFailed);
        }
    }
}
