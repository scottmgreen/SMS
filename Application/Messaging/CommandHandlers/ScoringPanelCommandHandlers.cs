using Microsoft.Extensions.Logging;

namespace SMS_Application.Messaging.CommandHandlers;

// =============================================
// SCORING PANEL COMMAND HANDLERS
// =============================================

public class CreateScoringPanelCommandHandler : BaseCommandBundle, IRequestHandler<CreateScoringPanelCommand, Result<ScoringPanel>>
{
    private readonly ScoringPanelDataService _dataService;
    private readonly ILogger<CreateScoringPanelCommandHandler> _logger;

    public CreateScoringPanelCommandHandler(ScoringPanelDataService dataService, ILogger<CreateScoringPanelCommandHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<ScoringPanel>> HandleAsync(CreateScoringPanelCommand request, CancellationToken ct = default)
    {
        try
        {
            if (request?.ScoringPanel is null)
            {
                _logger.LogError("CreateScoringPanelCommand received with null ScoringPanel");
                return Result<ScoringPanel>.Failure<ScoringPanel>(DomainErrors.ScoringPanelError.NullOrEmpty);
            }

            _logger.LogInformation("Processing CreateScoringPanelCommand for Code: {Code}", request.ScoringPanel.Code);

            var result = await _dataService.CreateScoringPanelAsync(request.ScoringPanel, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully created ScoringPanel with ID: {Id}, Code: {Code}",
                    result.Value?.Id, result.Value?.Code);
            }
            else
            {
                _logger.LogError("Failed to create ScoringPanel with Code: {Code}. Error: {Error}",
                    request.ScoringPanel.Code, result.Error?.Message);
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
            _logger.LogError(ex, "Unexpected error occurred while creating ScoringPanel");
            return Result<ScoringPanel>.Failure<ScoringPanel>(DomainErrors.ScoringPanelError.CreateFailed);
        }
    }
}

public class UpdateScoringPanelCommandHandler : BaseCommandBundle, IRequestHandler<UpdateScoringPanelCommand, Result<ScoringPanel>>
{
    private readonly ScoringPanelDataService _dataService;
    private readonly ILogger<UpdateScoringPanelCommandHandler> _logger;

    public UpdateScoringPanelCommandHandler(ScoringPanelDataService dataService, ILogger<UpdateScoringPanelCommandHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<ScoringPanel>> HandleAsync(UpdateScoringPanelCommand request, CancellationToken ct = default)
    {
        try
        {
            if (request?.ScoringPanel is null)
            {
                _logger.LogError("UpdateScoringPanelCommand received with null ScoringPanel");
                return Result<ScoringPanel>.Failure<ScoringPanel>(DomainErrors.ScoringPanelError.NullOrEmpty);
            }

            _logger.LogInformation("Processing UpdateScoringPanelCommand for ID: {Id}, Code: {Code}",
                request.ScoringPanel.Id, request.ScoringPanel.Code);

            var result = await _dataService.UpdateScoringPanelAsync(request.ScoringPanel, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully updated ScoringPanel with ID: {Id}", request.ScoringPanel.Id);
            }
            else
            {
                _logger.LogError("Failed to update ScoringPanel with ID: {Id}. Error: {Error}",
                    request.ScoringPanel.Id, result.Error?.Message);
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
            _logger.LogError(ex, "Unexpected error occurred while updating ScoringPanel with ID: {Id}", request.ScoringPanel?.Id);
            return Result<ScoringPanel>.Failure<ScoringPanel>(DomainErrors.ScoringPanelError.UpdateFailed);
        }
    }
}

public class DeleteScoringPanelCommandHandler : BaseCommandBundle, IRequestHandler<DeleteScoringPanelCommand, Result<bool>>
{
    private readonly ScoringPanelDataService _dataService;
    private readonly ILogger<DeleteScoringPanelCommandHandler> _logger;

    public DeleteScoringPanelCommandHandler(ScoringPanelDataService dataService, ILogger<DeleteScoringPanelCommandHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> HandleAsync(DeleteScoringPanelCommand request, CancellationToken ct = default)
    {
        try
        {
            if (request?.ScoringPanelId is null)
            {
                _logger.LogError("DeleteScoringPanelCommand received with null ScoringPanelId");
                return Result<bool>.Failure<bool>(DomainErrors.ScoringPanelError.NullOrEmpty);
            }

            _logger.LogInformation("Processing DeleteScoringPanelCommand for ID: {Id}", request.ScoringPanelId);

            var result = await _dataService.DeleteScoringPanelAsync(request.ScoringPanelId, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully deleted ScoringPanel with ID: {Id}", request.ScoringPanelId);
            }
            else
            {
                _logger.LogError("Failed to delete ScoringPanel with ID: {Id}. Error: {Error}",
                    request.ScoringPanelId, result.Error?.Message);
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
            _logger.LogError(ex, "Unexpected error occurred while deleting ScoringPanel with ID: {Id}", request.ScoringPanelId);
            return Result<bool>.Failure<bool>(DomainErrors.ScoringPanelError.DeleteFailed);
        }
    }
}