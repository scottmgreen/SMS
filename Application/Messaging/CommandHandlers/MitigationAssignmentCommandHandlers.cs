using Microsoft.Extensions.Logging;

namespace SMS_Application.Messaging.CommandHandlers;

// =============================================
// MITIGATION ASSIGNMENT COMMAND HANDLERS
// =============================================

public class CreateMitigationAssignmentCommandHandler : BaseCommandBundle, IRequestHandler<CreateMitigationAssignmentCommand, Result<MitigationAssignment>>
{
    private readonly MitigationAssignmentDataService _dataService;
    private readonly ILogger<CreateMitigationAssignmentCommandHandler> _logger;

    public CreateMitigationAssignmentCommandHandler(MitigationAssignmentDataService dataService, ILogger<CreateMitigationAssignmentCommandHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<MitigationAssignment>> HandleAsync(CreateMitigationAssignmentCommand request, CancellationToken ct = default)
    {
        try
        {
            if (request?.MitigationAssignment is null)
            {
                _logger.LogError("CreateMitigationAssignmentCommand received with null MitigationAssignment");
                return Result<MitigationAssignment>.Failure<MitigationAssignment>(DomainErrors.MitigationAssignmentError.NullOrEmpty);
            }

            _logger.LogInformation("Processing CreateMitigationAssignmentCommand for Code: {Code}", request.MitigationAssignment.Code);

            var result = await _dataService.CreateMitigationAssignmentAsync(request.MitigationAssignment, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully created MitigationAssignment with ID: {Id}, Code: {Code}",
                    result.Value?.Id, result.Value?.Code);
            }
            else
            {
                _logger.LogError("Failed to create MitigationAssignment with Code: {Code}. Error: {Error}",
                    request.MitigationAssignment.Code, result.Error?.Message);
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
            _logger.LogError(ex, "Unexpected error occurred while creating MitigationAssignment");
            return Result<MitigationAssignment>.Failure<MitigationAssignment>(DomainErrors.MitigationAssignmentError.CreateFailed);
        }
    }
}

public class UpdateMitigationAssignmentCommandHandler : BaseCommandBundle, IRequestHandler<UpdateMitigationAssignmentCommand, Result<MitigationAssignment>>
{
    private readonly MitigationAssignmentDataService _dataService;
    private readonly ILogger<UpdateMitigationAssignmentCommandHandler> _logger;

    public UpdateMitigationAssignmentCommandHandler(MitigationAssignmentDataService dataService, ILogger<UpdateMitigationAssignmentCommandHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<MitigationAssignment>> HandleAsync(UpdateMitigationAssignmentCommand request, CancellationToken ct = default)
    {
        try
        {
            if (request?.MitigationAssignment is null)
            {
                _logger.LogError("UpdateMitigationAssignmentCommand received with null MitigationAssignment");
                return Result<MitigationAssignment>.Failure<MitigationAssignment>(DomainErrors.MitigationAssignmentError.NullOrEmpty);
            }

            _logger.LogInformation("Processing UpdateMitigationAssignmentCommand for ID: {Id}, Code: {Code}",
                request.MitigationAssignment.Id, request.MitigationAssignment.Code);

            var result = await _dataService.UpdateMitigationAssignmentAsync(request.MitigationAssignment, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully updated MitigationAssignment with ID: {Id}", request.MitigationAssignment.Id);
            }
            else
            {
                _logger.LogError("Failed to update MitigationAssignment with ID: {Id}. Error: {Error}",
                    request.MitigationAssignment.Id, result.Error?.Message);
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
            _logger.LogError(ex, "Unexpected error occurred while updating MitigationAssignment with ID: {Id}", request.MitigationAssignment?.Id);
            return Result<MitigationAssignment>.Failure<MitigationAssignment>(DomainErrors.MitigationAssignmentError.UpdateFailed);
        }
    }
}

public class DeleteMitigationAssignmentCommandHandler : BaseCommandBundle, IRequestHandler<DeleteMitigationAssignmentCommand, Result<bool>>
{
    private readonly MitigationAssignmentDataService _dataService;
    private readonly ILogger<DeleteMitigationAssignmentCommandHandler> _logger;

    public DeleteMitigationAssignmentCommandHandler(MitigationAssignmentDataService dataService, ILogger<DeleteMitigationAssignmentCommandHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> HandleAsync(DeleteMitigationAssignmentCommand request, CancellationToken ct = default)
    {
        try
        {
            if (request?.MitigationAssignmentId is null)
            {
                _logger.LogError("DeleteMitigationAssignmentCommand received with null MitigationAssignmentId");
                return Result<bool>.Failure<bool>(DomainErrors.MitigationAssignmentError.NullOrEmpty);
            }

            _logger.LogInformation("Processing DeleteMitigationAssignmentCommand for ID: {Id}", request.MitigationAssignmentId);

            var result = await _dataService.DeleteMitigationAssignmentAsync(request.MitigationAssignmentId, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully deleted MitigationAssignment with ID: {Id}", request.MitigationAssignmentId);
            }
            else
            {
                _logger.LogError("Failed to delete MitigationAssignment with ID: {Id}. Error: {Error}",
                    request.MitigationAssignmentId, result.Error?.Message);
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
            _logger.LogError(ex, "Unexpected error occurred while deleting MitigationAssignment with ID: {Id}", request.MitigationAssignmentId);
            return Result<bool>.Failure<bool>(DomainErrors.MitigationAssignmentError.DeleteFailed);
        }
    }
}