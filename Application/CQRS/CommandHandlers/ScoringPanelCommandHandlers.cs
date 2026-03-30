//-----------------------------------------------------------------------
// <copyright file="ScoringPanelCommandHandlers.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Command handlers implementing SMS scoring panel business logic and operations.
//                  Implements command handlers for processing write operations.
//                  Handles business logic execution and domain entity coordination.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;

using Microsoft.Extensions.Logging;

namespace SMS_Application.Messaging.CommandHandlers;

// =============================================
// SCORING PANEL COMMAND HANDLERS - Clean Architecture Pattern
// =============================================

public class CreateScoringPanelCommandHandler : BaseCommandBundle, IRequestHandler<CreateScoringPanelCommand, Result<ScoringPanel>>
{
    private readonly IScoringPanelService _scoringPanelService;
    private readonly ILogger<CreateScoringPanelCommandHandler> _logger;

    public CreateScoringPanelCommandHandler(IScoringPanelService scoringPanelService, ILogger<CreateScoringPanelCommandHandler> logger)
    {
        _scoringPanelService = scoringPanelService ?? throw new ArgumentNullException(nameof(scoringPanelService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<ScoringPanel>> HandleAsync(CreateScoringPanelCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request?.ScoringPanel is null)
            {
                _logger.LogApplicationError("CreateScoringPanelCommand received with null request or scoring panel", ApplicationEventIds.Error, null);
                return Result<ScoringPanel>.Failure<ScoringPanel>(DomainErrors.ScoringPanelError.NullOrEmpty);
            }

            _logger.LogInformation("✅ Clean Architecture: Processing CreateScoringPanelCommand for Code: {Code}", request.ScoringPanel.Code);

            var result = await _scoringPanelService.CreateScoringPanelAsync(request.ScoringPanel, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("✅ Clean Architecture: Successfully created ScoringPanel with ID: {Id}, Code: {Code}",
                    result.Value?.Id, result.Value?.Code);
            }
            else
            {
                _logger.LogApplicationError("Failed to create ScoringPanel with Code: {Code}. Error: {Error}",
                    ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("CreateScoringPanelCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while creating ScoringPanel", ApplicationEventIds.Error, ex);
            return Result<ScoringPanel>.Failure<ScoringPanel>(DomainErrors.ScoringPanelError.CreateFailed);
        }
    }
}

public class UpdateScoringPanelCommandHandler : BaseCommandBundle, IRequestHandler<UpdateScoringPanelCommand, Result<ScoringPanel>>
{
    private readonly IScoringPanelService _scoringPanelService;
    private readonly ILogger<UpdateScoringPanelCommandHandler> _logger;

    public UpdateScoringPanelCommandHandler(IScoringPanelService scoringPanelService, ILogger<UpdateScoringPanelCommandHandler> logger)
    {
        _scoringPanelService = scoringPanelService ?? throw new ArgumentNullException(nameof(scoringPanelService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<ScoringPanel>> HandleAsync(UpdateScoringPanelCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request?.ScoringPanel is null)
            {
                _logger.LogApplicationError("UpdateScoringPanelCommand received with null request or scoring panel", ApplicationEventIds.Error, null);
                return Result<ScoringPanel>.Failure<ScoringPanel>(DomainErrors.ScoringPanelError.NullOrEmpty);
            }

            _logger.LogInformation("✅ Clean Architecture: Processing UpdateScoringPanelCommand for ID: {Id}", request.ScoringPanel.Id);

            var result = await _scoringPanelService.UpdateScoringPanelAsync(request.ScoringPanel, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("✅ Clean Architecture: Successfully updated ScoringPanel with ID: {Id}", request.ScoringPanel.Id);
            }
            else
            {
                _logger.LogApplicationError("Failed to update ScoringPanel with ID: {Id}. Error: {Error}",
                    ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("UpdateScoringPanelCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while updating ScoringPanel with ID: {Id}", ApplicationEventIds.Error, ex);
            return Result<ScoringPanel>.Failure<ScoringPanel>(DomainErrors.ScoringPanelError.UpdateFailed);
        }
    }
}

public class DeleteScoringPanelCommandHandler : BaseCommandBundle, IRequestHandler<DeleteScoringPanelCommand, Result<bool>>
{
    private readonly IScoringPanelService _scoringPanelService;
    private readonly ILogger<DeleteScoringPanelCommandHandler> _logger;

    public DeleteScoringPanelCommandHandler(IScoringPanelService scoringPanelService, ILogger<DeleteScoringPanelCommandHandler> logger)
    {
        _scoringPanelService = scoringPanelService ?? throw new ArgumentNullException(nameof(scoringPanelService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> HandleAsync(DeleteScoringPanelCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request is null)
            {
                _logger.LogApplicationError("DeleteScoringPanelCommand received with null request", ApplicationEventIds.Error, null);
                return Result<bool>.Failure<bool>(DomainErrors.ScoringPanelError.NullOrEmpty);
            }

            _logger.LogInformation("✅ Clean Architecture: Processing DeleteScoringPanelCommand for ID: {Id}", request.ScoringPanelId);

            var result = await _scoringPanelService.DeleteScoringPanelAsync(request.ScoringPanelId, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("✅ Clean Architecture: Successfully deleted ScoringPanel with ID: {Id}", request.ScoringPanelId);
            }
            else
            {
                _logger.LogApplicationError("Failed to delete ScoringPanel with ID: {Id}. Error: {Error}",
                    ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("DeleteScoringPanelCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while deleting ScoringPanel with ID: {Id}", ApplicationEventIds.Error, ex);
            return Result<bool>.Failure<bool>(DomainErrors.ScoringPanelError.DeleteFailed);
        }
    }
}
