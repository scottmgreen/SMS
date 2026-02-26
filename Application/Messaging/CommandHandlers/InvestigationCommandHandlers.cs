//-----------------------------------------------------------------------
// <copyright file="InvestigationCommandHandlers.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Command handlers implementing SMS investigation workflow and process logic.
//                  Implements command handlers for processing write operations.
//                  Handles business logic execution and domain entity coordination.
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.Extensions.Logging;

namespace SMS_Application.Messaging.CommandHandlers;

// =============================================
// INVESTIGATION COMMAND HANDLERS
// =============================================

public class CreateInvestigationCommandHandler : BaseCommandBundle, IRequestHandler<CreateInvestigationCommand, Result<Investigation>>
{
    private readonly InvestigationDataService _dataService;
    private readonly ILogger<CreateInvestigationCommandHandler> _logger;

    public CreateInvestigationCommandHandler(InvestigationDataService dataService, ILogger<CreateInvestigationCommandHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<Investigation>> HandleAsync(CreateInvestigationCommand request, CancellationToken ct = default)
    {
        try
        {
            if (request?.Investigation is null)
            {
                _logger.LogApplicationError("CreateInvestigationCommand received with null Investigation", ApplicationEventIds.Error, null);
                return Result<Investigation>.Failure<Investigation>(DomainErrors.InvestigationError.NullOrEmpty);
            }

            _logger.LogInformation("Processing CreateInvestigationCommand for Code: {Code}", request.Investigation.Code);

            var result = await _dataService.CreateInvestigationAsync(request.Investigation, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully created Investigation with ID: {Id}, Code: {Code}",
                    result.Value?.Id, result.Value?.Code);
            }
            else
            {
                _logger.LogApplicationError("Failed to create Investigation with Code: {Code}. Error: {Error}",
                    ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("CreateInvestigationCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while creating Investigation", ApplicationEventIds.Error, ex);
            return Result<Investigation>.Failure<Investigation>(DomainErrors.InvestigationError.CreateFailed);
        }
    }
}

public class UpdateInvestigationCommandHandler : BaseCommandBundle, IRequestHandler<UpdateInvestigationCommand, Result<Investigation>>
{
    private readonly InvestigationDataService _dataService;
    private readonly ILogger<UpdateInvestigationCommandHandler> _logger;

    public UpdateInvestigationCommandHandler(InvestigationDataService dataService, ILogger<UpdateInvestigationCommandHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<Investigation>> HandleAsync(UpdateInvestigationCommand request, CancellationToken ct = default)
    {
        try
        {
            if (request?.Investigation is null)
            {
                _logger.LogApplicationError("UpdateInvestigationCommand received with null Investigation", ApplicationEventIds.Error, null);
                return Result<Investigation>.Failure<Investigation>(DomainErrors.InvestigationError.NullOrEmpty);
            }

            _logger.LogInformation("Processing UpdateInvestigationCommand for ID: {Id}, Code: {Code}",
                request.Investigation.Id, request.Investigation.Code);

            var result = await _dataService.UpdateInvestigationAsync(request.Investigation, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully updated Investigation with ID: {Id}", request.Investigation.Id);
            }
            else
            {
                _logger.LogApplicationError("Failed to update Investigation with ID: {Id}. Error: {Error}",
                    ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("UpdateInvestigationCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while updating Investigation with ID: {Id}", ApplicationEventIds.Error, ex);
            return Result<Investigation>.Failure<Investigation>(DomainErrors.InvestigationError.UpdateFailed);
        }
    }
}

public class DeleteInvestigationCommandHandler : BaseCommandBundle, IRequestHandler<DeleteInvestigationCommand, Result<bool>>
{
    private readonly InvestigationDataService _dataService;
    private readonly ILogger<DeleteInvestigationCommandHandler> _logger;

    public DeleteInvestigationCommandHandler(InvestigationDataService dataService, ILogger<DeleteInvestigationCommandHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> HandleAsync(DeleteInvestigationCommand request, CancellationToken ct = default)
    {
        try
        {
            if (request?.InvestigationId is null)
            {
                _logger.LogApplicationError("DeleteInvestigationCommand received with null InvestigationId", ApplicationEventIds.Error, null);
                return Result<bool>.Failure<bool>(DomainErrors.InvestigationError.NullOrEmpty);
            }

            _logger.LogInformation("Processing DeleteInvestigationCommand for ID: {Id}", request.InvestigationId);

            var result = await _dataService.DeleteInvestigationAsync(request.InvestigationId, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully deleted Investigation with ID: {Id}", request.InvestigationId);
            }
            else
            {
                _logger.LogApplicationError("Failed to delete Investigation with ID: {Id}. Error: {Error}",
                    ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("DeleteInvestigationCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while deleting Investigation with ID: {Id}", ApplicationEventIds.Error, ex);
            return Result<bool>.Failure<bool>(DomainErrors.InvestigationError.DeleteFailed);
        }
    }
}
