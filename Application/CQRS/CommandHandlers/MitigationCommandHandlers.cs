//-----------------------------------------------------------------------
// <copyright file="MitigationCommandHandlers.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Command handlers implementing SMS mitigation management business logic and operations.
//                  Implements command handlers for processing write operations.
//                  Handles business logic execution and domain entity coordination.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;

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

    public async Task<Result<Mitigation>> HandleAsync(CreateMitigationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request?.Mitigation is null)
            {
                _logger.LogApplicationError("CreateMitigationCommand received with null request or mitigation", ApplicationEventIds.Error, null);
                return Result<Mitigation>.Failure<Mitigation>(DomainErrors.MitigationError.NullOrEmpty);
            }

            _logger.LogInformation("✅ Clean Architecture: Processing CreateMitigationCommand for Code: {Code}", request.Mitigation.Code);

            var result = await _mitigationService.CreateMitigationAsync(request.Mitigation, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("✅ Clean Architecture: Successfully created Mitigation with ID: {Id}, Code: {Code}",
                    result.Value?.Id, result.Value?.Code);
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

    public async Task<Result<Mitigation>> HandleAsync(UpdateMitigationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request?.Mitigation is null)
            {
                _logger.LogApplicationError("UpdateMitigationCommand received with null request or mitigation", ApplicationEventIds.Error, null);
                return Result<Mitigation>.Failure<Mitigation>(DomainErrors.MitigationError.NullOrEmpty);
            }

            _logger.LogInformation("✅ Clean Architecture: Processing UpdateMitigationCommand for ID: {Id}", request.Mitigation.Id);

            var result = await _mitigationService.UpdateMitigationAsync(request.Mitigation, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("✅ Clean Architecture: Successfully updated Mitigation with ID: {Id}", request.Mitigation.Id);
            }
            else
            {
                _logger.LogApplicationError("Failed to update Mitigation with ID: {Id}. Error: {Error}",
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
            _logger.LogApplicationError("Unexpected error occurred while updating Mitigation with ID: {Id}", ApplicationEventIds.Error, ex);
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

    public async Task<Result<bool>> HandleAsync(DeleteMitigationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request is null)
            {
                _logger.LogApplicationError("DeleteMitigationCommand received with null request", ApplicationEventIds.Error, null);
                return Result<bool>.Failure<bool>(DomainErrors.MitigationError.NullOrEmpty);
            }

            _logger.LogInformation("✅ Clean Architecture: Processing DeleteMitigationCommand for ID: {Id}", request.MitigationId);

            var result = await _mitigationService.DeleteMitigationAsync(request.MitigationId, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("✅ Clean Architecture: Successfully deleted Mitigation with ID: {Id}", request.MitigationId);
            }
            else
            {
                _logger.LogApplicationError("Failed to delete Mitigation with ID: {Id}. Error: {Error}",
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
            _logger.LogApplicationError("Unexpected error occurred while deleting Mitigation with ID: {Id}", ApplicationEventIds.Error, ex);
            return Result<bool>.Failure<bool>(DomainErrors.MitigationError.DeleteFailed);
        }
    }
}
