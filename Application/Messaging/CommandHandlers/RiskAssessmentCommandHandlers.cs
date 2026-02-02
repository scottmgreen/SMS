using Microsoft.Extensions.Logging;

namespace SMS_Application.Messaging.CommandHandlers;

// =============================================
// RISK ASSESSMENT COMMAND HANDLERS
// =============================================

public class CreateRiskAssessmentCommandHandler : BaseCommandBundle, IRequestHandler<CreateRiskAssessmentCommand, Result<RiskAssessment>>
{
    private readonly RiskAssessmentDataService _dataService;
    private readonly ILogger<CreateRiskAssessmentCommandHandler> _logger;

    public CreateRiskAssessmentCommandHandler(RiskAssessmentDataService dataService, ILogger<CreateRiskAssessmentCommandHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<RiskAssessment>> HandleAsync(CreateRiskAssessmentCommand request, CancellationToken ct = default)
    {
        try
        {
            if (request?.RiskAssessment is null)
            {
                _logger.LogApplicationError("CreateRiskAssessmentCommand received with null RiskAssessment", ApplicationEventIds.Error, null);
                return Result<RiskAssessment>.Failure<RiskAssessment>(DomainErrors.RiskAssessmentError.NullOrEmpty);
            }

            _logger.LogInformation("Processing CreateRiskAssessmentCommand for Code: {Code}", request.RiskAssessment.Code);

            var result = await _dataService.CreateRiskAssessmentAsync(request.RiskAssessment, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully created RiskAssessment with ID: {Id}, Code: {Code}",
                    result.Value?.Id, result.Value?.Code);
            }
            else
            {
                _logger.LogApplicationError("Failed to create RiskAssessment with Code: {Code}. Error: {Error}",
                    ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("CreateRiskAssessmentCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while creating RiskAssessment", ApplicationEventIds.Error, ex);
            return Result<RiskAssessment>.Failure<RiskAssessment>(DomainErrors.RiskAssessmentError.CreateFailed);
        }
    }
}

public class UpdateRiskAssessmentCommandHandler : BaseCommandBundle, IRequestHandler<UpdateRiskAssessmentCommand, Result<RiskAssessment>>
{
    private readonly RiskAssessmentDataService _dataService;
    private readonly ILogger<UpdateRiskAssessmentCommandHandler> _logger;

    public UpdateRiskAssessmentCommandHandler(RiskAssessmentDataService dataService, ILogger<UpdateRiskAssessmentCommandHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<RiskAssessment>> HandleAsync(UpdateRiskAssessmentCommand request, CancellationToken ct = default)
    {
        try
        {
            if (request?.RiskAssessment is null)
            {
                _logger.LogApplicationError("UpdateRiskAssessmentCommand received with null RiskAssessment", ApplicationEventIds.Error, null);
                return Result<RiskAssessment>.Failure<RiskAssessment>(DomainErrors.RiskAssessmentError.NullOrEmpty);
            }

            _logger.LogInformation("Processing UpdateRiskAssessmentCommand for ID: {Id}, Code: {Code}",
                request.RiskAssessment.Id, request.RiskAssessment.Code);

            var result = await _dataService.UpdateRiskAssessmentAsync(request.RiskAssessment, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully updated RiskAssessment with ID: {Id}", request.RiskAssessment.Id);
            }
            else
            {
                _logger.LogApplicationError("Failed to update RiskAssessment with ID: {Id}. Error: {Error}",
                    ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("UpdateRiskAssessmentCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while updating RiskAssessment with ID: {Id}", ApplicationEventIds.Error, ex);
            return Result<RiskAssessment>.Failure<RiskAssessment>(DomainErrors.RiskAssessmentError.UpdateFailed);
        }
    }
}

public class DeleteRiskAssessmentCommandHandler : BaseCommandBundle, IRequestHandler<DeleteRiskAssessmentCommand, Result<bool>>
{
    private readonly RiskAssessmentDataService _dataService;
    private readonly ILogger<DeleteRiskAssessmentCommandHandler> _logger;

    public DeleteRiskAssessmentCommandHandler(RiskAssessmentDataService dataService, ILogger<DeleteRiskAssessmentCommandHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> HandleAsync(DeleteRiskAssessmentCommand request, CancellationToken ct = default)
    {
        try
        {
            if (request?.RiskAssessmentId is null)
            {
                _logger.LogApplicationError("DeleteRiskAssessmentCommand received with null RiskAssessmentId", ApplicationEventIds.Error, null);
                return Result<bool>.Failure<bool>(DomainErrors.RiskAssessmentError.NullOrEmpty);
            }

            _logger.LogInformation("Processing DeleteRiskAssessmentCommand for ID: {Id}", request.RiskAssessmentId);

            var result = await _dataService.DeleteRiskAssessmentAsync(request.RiskAssessmentId, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully deleted RiskAssessment with ID: {Id}", request.RiskAssessmentId);
            }
            else
            {
                _logger.LogApplicationError("Failed to delete RiskAssessment with ID: {Id}. Error: {Error}",
                    ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("DeleteRiskAssessmentCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while deleting RiskAssessment with ID: {Id}", ApplicationEventIds.Error, ex);
            return Result<bool>.Failure<bool>(DomainErrors.RiskAssessmentError.DeleteFailed);
        }
    }
}

// =============================================
// STEP-SPECIFIC COMMAND HANDLERS FOR STEPS 1-5
// =============================================

public class SaveStep1CommandHandler : BaseCommandBundle, IRequestHandler<SaveStep1Command, Result<RiskAssessment>>
{
    private readonly RiskAssessmentDataService _dataService;
    private readonly ILogger<SaveStep1CommandHandler> _logger;

    public SaveStep1CommandHandler(RiskAssessmentDataService dataService, ILogger<SaveStep1CommandHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<RiskAssessment>> HandleAsync(SaveStep1Command request, CancellationToken ct = default)
    {
        try
        {
            if (request is null)
            {
                _logger.LogApplicationError("SaveStep1Command received with null request", ApplicationEventIds.Error, null);
                return Result<RiskAssessment>.Failure<RiskAssessment>(DomainErrors.RiskAssessmentError.NullOrEmpty);
            }

            _logger.LogInformation("Processing SaveStep1Command for RiskAssessment: {Id}", request.RiskAssessmentId.Value);

            var result = await _dataService.SaveStep1Async(
                request.RiskAssessmentId,
                request.LeadAssessorId,
                request.SystemDescription,
                request.SystemBoundaries,
                request.SystemPurpose,
                request.FiveMPersonnel,
                request.FiveMEquipment,
                request.FiveMProcedures,
                request.FiveMResources,
                request.FiveMPhysicalEnvironment,
                request.FiveMOperationalEnvironment,
                request.UpdatedBy,
                ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully saved Step 1 for RiskAssessment: {Id}", request.RiskAssessmentId);
            }
            else
            {
                _logger.LogApplicationError("Failed to save Step 1 for RiskAssessment: {Id}. Error: {Error}",
                    ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("SaveStep1Command operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while saving Step 1 for RiskAssessment: {Id}", ApplicationEventIds.Error, ex);
            return Result<RiskAssessment>.Failure<RiskAssessment>(DomainErrors.RiskAssessmentError.UpdateFailed);
        }
    }
}

//public class SaveStep3CommandHandler : BaseCommandBundle, IRequestHandler<SaveStep3Command, Result<RiskAssessment>>
//{
//    private readonly RiskAssessmentDataService _dataService;
//    private readonly ILogger<SaveStep3CommandHandler> _logger;

//    public SaveStep3CommandHandler(RiskAssessmentDataService dataService, ILogger<SaveStep3CommandHandler> logger)
//    {
//        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
//        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
//    }

//    public async Task<Result<RiskAssessment>> HandleAsync(SaveStep3Command request, CancellationToken ct = default)
//    {
//        try
//        {
//            if (request is null)
//            {
//                _logger.LogApplicationError("SaveStep3Command received with null request", ApplicationEventIds.Error, null);
//                return Result<RiskAssessment>.Failure<RiskAssessment>(DomainErrors.RiskAssessmentError.NullOrEmpty);
//            }

//            _logger.LogInformation("Processing SaveStep3Command for RiskAssessment: {Id}", request.RiskAssessmentId.Value);

//            var result = await _dataService.SaveStep3Async(
//                request.RiskAssessmentId,
//                request.RiskAnalysisMethod,
//                request.RiskCriteria,
//                request.UpdatedBy,
//                ct).ConfigureAwait(false);

//            if (result.IsSuccess)
//            {
//                _logger.LogInformation("Successfully saved Step 3 for RiskAssessment: {Id}", request.RiskAssessmentId);
//            }
//            else
//            {
//                _logger.LogApplicationError("Failed to save Step 3 for RiskAssessment: {Id}. Error: {Error}",
//                    ApplicationEventIds.Error, null);
//            }

//            return result;
//        }
//        catch (OperationCanceledException)
//        {
//            _logger.LogWarning("SaveStep3Command operation was cancelled");
//            throw;
//        }
//        catch (Exception ex)
//        {
//            _logger.LogApplicationError("Unexpected error occurred while saving Step 3 for RiskAssessment: {Id}", ApplicationEventIds.Error, ex);
//            return Result<RiskAssessment>.Failure<RiskAssessment>(DomainErrors.RiskAssessmentError.UpdateFailed);
//        }
//    }
//}

public class SaveStep4CommandHandler : BaseCommandBundle, IRequestHandler<SaveStep4Command, Result<RiskAssessment>>
{
    private readonly RiskAssessmentDataService _dataService;
    private readonly ILogger<SaveStep4CommandHandler> _logger;

    public SaveStep4CommandHandler(RiskAssessmentDataService dataService, ILogger<SaveStep4CommandHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<RiskAssessment>> HandleAsync(SaveStep4Command request, CancellationToken ct = default)
    {
        try
        {
            if (request is null)
            {
                _logger.LogApplicationError("SaveStep4Command received with null request", ApplicationEventIds.Error, null);
                return Result<RiskAssessment>.Failure<RiskAssessment>(DomainErrors.RiskAssessmentError.NullOrEmpty);
            }

            _logger.LogInformation("Processing SaveStep4Command for RiskAssessment: {Id}", request.RiskAssessmentId.Value);

            var result = await _dataService.SaveStep4Async(
                request.RiskAssessmentId,
                request.FinalSeverityScore,
                request.FinalLikelihoodScore,
                request.FinalRiskLevel,
                request.AssessmentRationale,
                request.UpdatedBy,
                ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully saved Step 4 for RiskAssessment: {Id}", request.RiskAssessmentId);
            }
            else
            {
                _logger.LogApplicationError("Failed to save Step 4 for RiskAssessment: {Id}. Error: {Error}",
                    ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("SaveStep4Command operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while saving Step 4 for RiskAssessment: {Id}", ApplicationEventIds.Error, ex);
            return Result<RiskAssessment>.Failure<RiskAssessment>(DomainErrors.RiskAssessmentError.UpdateFailed);
        }
    }
}

public class SaveStep5CommandHandler : BaseCommandBundle, IRequestHandler<SaveStep5Command, Result<RiskAssessment>>
{
    private readonly RiskAssessmentDataService _dataService;
    private readonly ILogger<SaveStep5CommandHandler> _logger;

    public SaveStep5CommandHandler(RiskAssessmentDataService dataService, ILogger<SaveStep5CommandHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<RiskAssessment>> HandleAsync(SaveStep5Command request, CancellationToken ct = default)
    {
        try
        {
            if (request is null)
            {
                _logger.LogApplicationError("SaveStep5Command received with null request", ApplicationEventIds.Error, null);
                return Result<RiskAssessment>.Failure<RiskAssessment>(DomainErrors.RiskAssessmentError.NullOrEmpty);
            }

            _logger.LogInformation("Processing SaveStep5Command for RiskAssessment: {Id}", request.RiskAssessmentId);

            var result = await _dataService.SaveStep5Async(request.RiskAssessmentId,  request.UpdatedBy,  ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully saved Step 5 for RiskAssessment: {Id}", request.RiskAssessmentId);
            }
            else
            {
                _logger.LogApplicationError("Failed to save Step 5 for RiskAssessment: {Id}. Error: {Error}",
                    ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("SaveStep5Command operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while saving Step 5 for RiskAssessment: {Id}", ApplicationEventIds.Error, ex);
            return Result<RiskAssessment>.Failure<RiskAssessment>(DomainErrors.RiskAssessmentError.UpdateFailed);
        }
    }
}

public class UpdateProgressCommandHandler : BaseCommandBundle, IRequestHandler<UpdateProgressCommand, Result<RiskAssessment>>
{
    private readonly RiskAssessmentDataService _dataService;
    private readonly ILogger<UpdateProgressCommandHandler> _logger;

    public UpdateProgressCommandHandler(RiskAssessmentDataService dataService, ILogger<UpdateProgressCommandHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<RiskAssessment>> HandleAsync(UpdateProgressCommand request, CancellationToken ct = default)
    {
        try
        {
            if (request is null)
            {
                _logger.LogApplicationError("UpdateProgressCommand received with null request", ApplicationEventIds.Error, null);
                return Result<RiskAssessment>.Failure<RiskAssessment>(DomainErrors.RiskAssessmentError.NullOrEmpty);
            }

            _logger.LogInformation("Processing UpdateProgressCommand for RiskAssessment: {Id}, Step: {Step}",
                request.RiskAssessmentId, request.CurrentStep);

            var result = await _dataService.UpdateProgressAsync(
                request.RiskAssessmentId,
                request.CurrentStep,
                request.CompletedSteps,
                request.CompletionPercentage,
                request.Status,
                request.Stage,
                request.UpdatedBy,
                ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully updated progress for RiskAssessment: {Id}", request.RiskAssessmentId);
            }
            else
            {
                _logger.LogApplicationError("Failed to update progress for RiskAssessment: {Id}. Error: {Error}",
                    ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("UpdateProgressCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while updating progress for RiskAssessment: {Id}", ApplicationEventIds.Error, ex);
            return Result<RiskAssessment>.Failure<RiskAssessment>(DomainErrors.RiskAssessmentError.UpdateFailed);
        }
    }
}