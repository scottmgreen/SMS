//-----------------------------------------------------------------------
// <copyright file="MitigationAssignmentCommandHandlers.cs" company="SMS Safety Management System">
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
// MITIGATION ASSIGNMENT COMMAND HANDLERS - Clean Architecture Pattern
// =============================================

public class CreateMitigationAssignmentCommandHandler : BaseCommandBundle, IBaseRequestHandler<CreateMitigationAssignmentCommand, Result<MitigationAssignment>>
{
    private readonly MitigationAssignmentService _mitigationAssignmentService;
    private readonly ILogger<CreateMitigationAssignmentCommandHandler> _logger;

    public CreateMitigationAssignmentCommandHandler(MitigationAssignmentService mitigationAssignmentService, ILogger<CreateMitigationAssignmentCommandHandler> logger)
    {
        _mitigationAssignmentService = mitigationAssignmentService ?? throw new ArgumentNullException(nameof(mitigationAssignmentService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<MitigationAssignment>> HandleAsync(CreateMitigationAssignmentCommand request, CancellationToken ct = default)
    {
        try
        {
            if (request?.MitigationAssignment is null)
            {
                _logger.LogApplicationError("CreateMitigationAssignmentCommand received with null MitigationAssignment", ApplicationEventIds.Error, null);
                return Result<MitigationAssignment>.Failure<MitigationAssignment>(DomainErrors.MitigationAssignmentError.NullOrEmpty);
            }

            _logger.LogInformation(" Processing CreateMitigationAssignmentCommand for Code: {Code}", request.MitigationAssignment.Code);

            var result = await _mitigationAssignmentService.CreateMitigationAssignmentAsync(request.MitigationAssignment, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation(" Successfully created MitigationAssignment with ID: {Id}, Code: {Code}",
                    result.Value?.Id, result.Value?.Code);
            }
            else
            {
                _logger.LogApplicationError("Failed to create MitigationAssignment with Code: {Code}. Error: {Error}",
                    ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("CreateMitigationAssignmentCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while creating MitigationAssignment", ApplicationEventIds.Error, ex);
            return Result<MitigationAssignment>.Failure<MitigationAssignment>(DomainErrors.MitigationAssignmentError.CreateFailed);
        }
    }
}

public class UpdateMitigationAssignmentCommandHandler : BaseCommandBundle, IBaseRequestHandler<UpdateMitigationAssignmentCommand, Result<MitigationAssignment>>
{
    private readonly MitigationAssignmentService _mitigationAssignmentService;
    private readonly ILogger<UpdateMitigationAssignmentCommandHandler> _logger;

    public UpdateMitigationAssignmentCommandHandler(MitigationAssignmentService mitigationAssignmentService, ILogger<UpdateMitigationAssignmentCommandHandler> logger)
    {
        _mitigationAssignmentService = mitigationAssignmentService ?? throw new ArgumentNullException(nameof(mitigationAssignmentService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<MitigationAssignment>> HandleAsync(UpdateMitigationAssignmentCommand request, CancellationToken ct = default)
    {
        try
        {
            if (request?.MitigationAssignment is null)
            {
                _logger.LogApplicationError("UpdateMitigationAssignmentCommand received with null MitigationAssignment", ApplicationEventIds.Error, null);
                return Result<MitigationAssignment>.Failure<MitigationAssignment>(DomainErrors.MitigationAssignmentError.NullOrEmpty);
            }

            _logger.LogInformation(" Processing UpdateMitigationAssignmentCommand for ID: {Id}, Code: {Code}",
                request.MitigationAssignment.Id, request.MitigationAssignment.Code);

            var result = await _mitigationAssignmentService.UpdateMitigationAssignmentAsync(request.MitigationAssignment, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation(" Successfully updated MitigationAssignment with ID: {Id}", request.MitigationAssignment.Id);
            }
            else
            {
                _logger.LogApplicationError("Failed to update MitigationAssignment with ID: {Id}. Error: {Error}",
                    ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("UpdateMitigationAssignmentCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while updating MitigationAssignment with ID: {Id}", ApplicationEventIds.Error, ex);
            return Result<MitigationAssignment>.Failure<MitigationAssignment>(DomainErrors.MitigationAssignmentError.UpdateFailed);
        }
    }
}

public class DeleteMitigationAssignmentCommandHandler : BaseCommandBundle, IBaseRequestHandler<DeleteMitigationAssignmentCommand, Result<bool>>
{
    private readonly MitigationAssignmentService _mitigationAssignmentService;
    private readonly ILogger<DeleteMitigationAssignmentCommandHandler> _logger;

    public DeleteMitigationAssignmentCommandHandler(MitigationAssignmentService mitigationAssignmentService, ILogger<DeleteMitigationAssignmentCommandHandler> logger)
    {
        _mitigationAssignmentService = mitigationAssignmentService ?? throw new ArgumentNullException(nameof(mitigationAssignmentService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> HandleAsync(DeleteMitigationAssignmentCommand request, CancellationToken ct = default)
    {
        try
        {
            if (request?.MitigationAssignmentId is null)
            {
                _logger.LogApplicationError("DeleteMitigationAssignmentCommand received with null MitigationAssignmentId", ApplicationEventIds.Error, null);
                return Result<bool>.Failure<bool>(DomainErrors.MitigationAssignmentError.NullOrEmpty);
            }

            _logger.LogInformation(" Processing DeleteMitigationAssignmentCommand for ID: {Id}", request.MitigationAssignmentId);

            var result = await _mitigationAssignmentService.DeleteMitigationAssignmentAsync(request.MitigationAssignmentId, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation(" Successfully deleted MitigationAssignment with ID: {Id}", request.MitigationAssignmentId);
            }
            else
            {
                _logger.LogApplicationError("Failed to delete MitigationAssignment with ID: {Id}. Error: {Error}",
                    ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("DeleteMitigationAssignmentCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while deleting MitigationAssignment with ID: {Id}", ApplicationEventIds.Error, ex);
            return Result<bool>.Failure<bool>(DomainErrors.MitigationAssignmentError.DeleteFailed);
        }
    }
}
