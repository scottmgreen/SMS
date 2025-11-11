using SMS_Application.Interfaces;
using SMS_Application.Messaging.Commands;
using SMS_Domain.Entities;
using SMS_Domain.ValueObjects;
using SMS_Domain.Enums;
using SMS_Infrastructure.Interfaces;
using SMS_Infrastructure.Services;
using SMS_Shared.Common;
using Microsoft.Extensions.Logging;
using SMS_Domain.Errors;

namespace SMS_Application.Messaging.CommandHandlers;

// =============================================
// HAZARD COMMAND HANDLERS - SMS Backend Integration
// =============================================

/// <summary>
/// Command handler for creating hazards using individual properties
/// Used by HazardReporting page for direct SMS Backend integration
/// </summary>
public class CreateHazardCommandHandler : BaseCommandBundle, IRequestHandler<CreateHazardCommand, Result<Hazard>>
{
    private readonly HazardDataService _dataService;
    private readonly ILogger<CreateHazardCommandHandler> _logger;

    public CreateHazardCommandHandler(
        IHazardRepository hazardRepository, 
        HazardDataService dataService, 
        ILogger<CreateHazardCommandHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<Hazard>> HandleAsync(CreateHazardCommand request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("??? SMS Backend: Creating new hazard - {Name}", request.Hazard.Name);

            
            var hazardResult = Hazard.CreateFromHazardReport(
                request.Hazard.Description,
                request.Hazard.HazardType,
                request.Hazard.ReportedBy,
                request.Hazard.ReportingDepartment,
                request.Hazard.HazardType,
                request.Hazard.HazardLocation,
                request.Hazard.IsConfidential,
                request.Hazard.IsAnonymous
            );

            if (hazardResult.IsFailure)
            {
                _logger.LogWarning("?? SMS Backend: Failed to create hazard entity - {Error}", hazardResult.Error.Message);
                return Result<Hazard>.Failure<Hazard>(hazardResult.Error);
            }

            var hazard = hazardResult.Value;
            hazard.ReportCode = request.Hazard.ReportCode;

            var dataResult = await _dataService.CreateHazardAsync(hazard, ct);
            if (dataResult.IsFailure)
            {
                _logger.LogError("? SMS Backend: Failed to save hazard via both data service");
                return Result<Hazard>.Failure<Hazard>(dataResult.Error);
            }
                
            _logger.LogInformation("? SMS Backend: Hazard saved via data service - {Code}", hazard.Code);
            return Result<Hazard>.Success(dataResult.Value);
        
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? SMS Backend: Exception creating hazard - {Name}", request.Hazard.Name);
            return Result<Hazard>.Failure<Hazard>(DomainErrors.HazardError.CreateFailed); // Use the correct error constant
        }
    }
}


public class UpdateHazardCommandHandler : BaseCommandBundle, IRequestHandler<UpdateHazardCommand, Result<Hazard>>
{
    private readonly HazardDataService _dataService;
    private readonly ILogger<UpdateHazardCommandHandler> _logger;

    public UpdateHazardCommandHandler(HazardDataService dataService, ILogger<UpdateHazardCommandHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<Hazard>> HandleAsync(UpdateHazardCommand request, CancellationToken ct = default)
    {
        try
        {
            if (request?.Hazard is null)
            {
                _logger.LogError("UpdateHazardCommand received with null Hazard");
                return Result<Hazard>.Failure<Hazard>(DomainErrors.HazardError.NullOrEmpty);
            }

            _logger.LogInformation("Processing UpdateHazardCommand for ID: {Id}, Code: {Code}",
                request.Hazard.Id, request.Hazard.Code);

            var result = await _dataService.UpdateHazardAsync(request.Hazard, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully updated Hazard with ID: {Id}", request.Hazard.Id);
            }
            else
            {
                _logger.LogError("Failed to update Hazard with ID: {Id}. Error: {Error}",
                    request.Hazard.Id, result.Error?.Message);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("UpdateHazardCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while updating Hazard with ID: {Id}", request.Hazard?.Id);
            return Result<Hazard>.Failure<Hazard>(DomainErrors.HazardError.UpdateFailed);
        }
    }
}

public class DeleteHazardCommandHandler : BaseCommandBundle, IRequestHandler<DeleteHazardCommand, Result<bool>>
{
    private readonly HazardDataService _dataService;
    private readonly ILogger<DeleteHazardCommandHandler> _logger;

    public DeleteHazardCommandHandler(HazardDataService dataService, ILogger<DeleteHazardCommandHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> HandleAsync(DeleteHazardCommand request, CancellationToken ct = default)
    {
        try
        {
            if (request?.HazardId is null)
            {
                _logger.LogError("DeleteHazardCommand received with null HazardId");
                return Result<bool>.Failure<bool>(DomainErrors.HazardError.NullOrEmpty);
            }

            _logger.LogInformation("Processing DeleteHazardCommand for ID: {Id}", request.HazardId);

            var result = await _dataService.DeleteHazardAsync(request.HazardId, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully deleted Hazard with ID: {Id}", request.HazardId);
            }
            else
            {
                _logger.LogError("Failed to delete Hazard with ID: {Id}. Error: {Error}",
                    request.HazardId, result.Error?.Message);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("DeleteHazardCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while deleting Hazard with ID: {Id}", request.HazardId);
            return Result<bool>.Failure<bool>(DomainErrors.HazardError.DeleteFailed);
        }
    }
}