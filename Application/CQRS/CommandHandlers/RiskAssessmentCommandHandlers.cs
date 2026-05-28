//-----------------------------------------------------------------------
// <copyright file="RiskAssessmentCommandHandlers.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Command handlers implementing SMS risk assessment and analysis logic.
//                  Implements command handlers for processing write operations.
//                  Handles business logic execution and domain entity coordination.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;

using Microsoft.Extensions.Logging;

namespace SMS_Application.CommandHandlers;

// =============================================
// RISK ASSESSMENT COMMAND HANDLERS - Clean Architecture Pattern
// =============================================

public class CreateRiskAssessmentCommandHandler : BaseCommandBundle, IBaseRequestHandler<CreateRiskAssessmentCommand, Result<RiskAssessment>>
{
    private readonly IRiskAssessmentService _riskAssessmentService;
    private readonly ILogger<CreateRiskAssessmentCommandHandler> _logger;

    public CreateRiskAssessmentCommandHandler(IRiskAssessmentService riskAssessmentService, ILogger<CreateRiskAssessmentCommandHandler> logger)
    {
        _riskAssessmentService = riskAssessmentService ?? throw new ArgumentNullException(nameof(riskAssessmentService));
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

            _logger.LogApplicationInformation(" Processing CreateRiskAssessmentCommand for Code: {Code}", request.RiskAssessment.Code);

            var result = await _riskAssessmentService.CreateRiskAssessmentAsync(request.RiskAssessment, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation(" Successfully created RiskAssessment with ID: {Id}, Code: {Code}",
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
            _logger.LogApplicationWarning("CreateRiskAssessmentCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while creating RiskAssessment", ApplicationEventIds.Error, ex);
            return Result<RiskAssessment>.Failure<RiskAssessment>(DomainErrors.RiskAssessmentError.CreateFailed);
        }
    }
}

public class UpdateRiskAssessmentCommandHandler : BaseCommandBundle, IBaseRequestHandler<UpdateRiskAssessmentCommand, Result<RiskAssessment>>
{
    private readonly IRiskAssessmentService _riskAssessmentService;
    private readonly ILogger<UpdateRiskAssessmentCommandHandler> _logger;

    public UpdateRiskAssessmentCommandHandler(IRiskAssessmentService riskAssessmentService, ILogger<UpdateRiskAssessmentCommandHandler> logger)
    {
        _riskAssessmentService = riskAssessmentService ?? throw new ArgumentNullException(nameof(riskAssessmentService));
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

            _logger.LogApplicationInformation(" Processing UpdateRiskAssessmentCommand for ID: {Id}, Code: {Code}",
                request.RiskAssessment.Id, request.RiskAssessment.Code);

            var result = await _riskAssessmentService.UpdateRiskAssessmentAsync(request.RiskAssessment, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation(" Successfully updated RiskAssessment with ID: {Id}", request.RiskAssessment.Id);
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
            _logger.LogApplicationWarning("UpdateRiskAssessmentCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while updating RiskAssessment with ID: {Id}", ApplicationEventIds.Error, ex);
            return Result<RiskAssessment>.Failure<RiskAssessment>(DomainErrors.RiskAssessmentError.UpdateFailed);
        }
    }
}

public class DeleteRiskAssessmentCommandHandler : BaseCommandBundle, IBaseRequestHandler<DeleteRiskAssessmentCommand, Result<bool>>
{
    private readonly IRiskAssessmentService _riskAssessmentService;
    private readonly ILogger<DeleteRiskAssessmentCommandHandler> _logger;

    public DeleteRiskAssessmentCommandHandler(IRiskAssessmentService riskAssessmentService, ILogger<DeleteRiskAssessmentCommandHandler> logger)
    {
        _riskAssessmentService = riskAssessmentService ?? throw new ArgumentNullException(nameof(riskAssessmentService));
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

            _logger.LogApplicationInformation(" Processing DeleteRiskAssessmentCommand for ID: {Id}", request.RiskAssessmentId);

            var result = await _riskAssessmentService.DeleteRiskAssessmentAsync(request.RiskAssessmentId, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation(" Successfully deleted RiskAssessment with ID: {Id}", request.RiskAssessmentId);
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
            _logger.LogApplicationWarning("DeleteRiskAssessmentCommand operation was cancelled");
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
// STEP-SPECIFIC COMMAND HANDLERS FOR STEPS 1-5 - Clean Architecture Pattern
// =============================================

public class SaveStep1CommandHandler : BaseCommandBundle, IBaseRequestHandler<SaveStep1Command, Result<RiskAssessment>>
{
    private readonly IRiskAssessmentService _riskAssessmentService;
    private readonly ILogger<SaveStep1CommandHandler> _logger;

    public SaveStep1CommandHandler(IRiskAssessmentService riskAssessmentService, ILogger<SaveStep1CommandHandler> logger)
    {
        _riskAssessmentService = riskAssessmentService ?? throw new ArgumentNullException(nameof(riskAssessmentService));
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

            _logger.LogApplicationInformation(" Processing SaveStep1Command for RiskAssessment: {Id}", request.RiskAssessmentId.Value);

            // Get the existing assessment first
            var assessmentResult = await _riskAssessmentService.GetRiskAssessmentByIdAsync(request.RiskAssessmentId, ct);
            if (assessmentResult.IsFailure)
            {
                return Result<RiskAssessment>.Failure<RiskAssessment>(assessmentResult.Error);
            }

            var assessment = assessmentResult.Value;

            // Update the assessment with Step 1 data
            assessment.LeadAssessorId = request.LeadAssessorId;
            assessment.SystemDescription = request.SystemDescription;
            assessment.SystemBoundaries = request.SystemBoundaries;
            assessment.SystemPurpose = request.SystemPurpose;
            assessment.FiveMPersonnel = request.FiveMPersonnel;
            assessment.FiveMEquipment = request.FiveMEquipment;
            assessment.FiveMProcedures = request.FiveMProcedures;
            assessment.FiveMResources = request.FiveMResources;
            assessment.FiveMPhysicalEnvironment = request.FiveMPhysicalEnvironment;
            assessment.FiveMOperationalEnvironment = request.FiveMOperationalEnvironment;
            assessment.UpdatedBy = request.UpdatedBy;
            assessment.UpdatedDate = DateTime.UtcNow;

            var result = await _riskAssessmentService.UpdateRiskAssessmentAsync(assessment, ct);

            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation(" Successfully saved Step 1 for RiskAssessment: {Id}", request.RiskAssessmentId);
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
            _logger.LogApplicationWarning("SaveStep1Command operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while saving Step 1 for RiskAssessment: {Id}", ApplicationEventIds.Error, ex);
            return Result<RiskAssessment>.Failure<RiskAssessment>(DomainErrors.RiskAssessmentError.UpdateFailed);
        }
    }
}

public class SaveStep4CommandHandler : BaseCommandBundle, IBaseRequestHandler<SaveStep4Command, Result<RiskAssessment>>
{
    private readonly IRiskAssessmentService _riskAssessmentService;
    private readonly ILogger<SaveStep4CommandHandler> _logger;

    public SaveStep4CommandHandler(IRiskAssessmentService riskAssessmentService, ILogger<SaveStep4CommandHandler> logger)
    {
        _riskAssessmentService = riskAssessmentService ?? throw new ArgumentNullException(nameof(riskAssessmentService));
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

            _logger.LogApplicationInformation(" Processing SaveStep4Command for RiskAssessment: {Id}", request.RiskAssessmentId.Value);

            // Get the existing assessment first
            var assessmentResult = await _riskAssessmentService.GetRiskAssessmentByIdAsync(request.RiskAssessmentId, ct);
            if (assessmentResult.IsFailure)
            {
                return Result<RiskAssessment>.Failure<RiskAssessment>(assessmentResult.Error);
            }

            var assessment = assessmentResult.Value;

            // Update the assessment with Step 4 data
            assessment.FinalSeverityScore = request.FinalSeverityScore;
            assessment.FinalLikelihoodScore = request.FinalLikelihoodScore;
            assessment.FinalRiskLevel = request.FinalRiskLevel;
            assessment.UpdatedBy = request.UpdatedBy;
            assessment.UpdatedDate = DateTime.UtcNow;

            var result = await _riskAssessmentService.UpdateRiskAssessmentAsync(assessment, ct);

            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation(" Successfully saved Step 4 for RiskAssessment: {Id}", request.RiskAssessmentId);
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
            _logger.LogApplicationWarning("SaveStep4Command operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while saving Step 4 for RiskAssessment: {Id}", ApplicationEventIds.Error, ex);
            return Result<RiskAssessment>.Failure<RiskAssessment>(DomainErrors.RiskAssessmentError.UpdateFailed);
        }
    }
}

public class SaveStep5CommandHandler : BaseCommandBundle, IBaseRequestHandler<SaveStep5Command, Result<RiskAssessment>>
{
    private readonly IRiskAssessmentService _riskAssessmentService;
    private readonly ILogger<SaveStep5CommandHandler> _logger;

    public SaveStep5CommandHandler(IRiskAssessmentService riskAssessmentService, ILogger<SaveStep5CommandHandler> logger)
    {
        _riskAssessmentService = riskAssessmentService ?? throw new ArgumentNullException(nameof(riskAssessmentService));
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

            _logger.LogApplicationInformation(" Processing SaveStep5Command for RiskAssessment: {Id}", request.RiskAssessmentId);

            // Get the existing assessment first
            var assessmentResult = await _riskAssessmentService.GetRiskAssessmentByIdAsync(request.RiskAssessmentId, ct);
            if (assessmentResult.IsFailure)
            {
                return Result<RiskAssessment>.Failure<RiskAssessment>(assessmentResult.Error);
            }

            var assessment = assessmentResult.Value;

            // Update the assessment with Step 5 data
            assessment.UpdatedBy = request.UpdatedBy;
            assessment.UpdatedDate = DateTime.UtcNow;

            var result = await _riskAssessmentService.UpdateRiskAssessmentAsync(assessment, ct);

            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation(" Successfully saved Step 5 for RiskAssessment: {Id}", request.RiskAssessmentId);
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
            _logger.LogApplicationWarning("SaveStep5Command operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while saving Step 5 for RiskAssessment: {Id}", ApplicationEventIds.Error, ex);
            return Result<RiskAssessment>.Failure<RiskAssessment>(DomainErrors.RiskAssessmentError.UpdateFailed);
        }
    }
}

public class UpdateProgressCommandHandler : BaseCommandBundle, IBaseRequestHandler<UpdateProgressCommand, Result<RiskAssessment>>
{
    private readonly IRiskAssessmentService _riskAssessmentService;
    private readonly ILogger<UpdateProgressCommandHandler> _logger;

    public UpdateProgressCommandHandler(IRiskAssessmentService riskAssessmentService, ILogger<UpdateProgressCommandHandler> logger)
    {
        _riskAssessmentService = riskAssessmentService ?? throw new ArgumentNullException(nameof(riskAssessmentService));
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

            _logger.LogApplicationInformation(" Processing UpdateProgressCommand for RiskAssessment: {Id}, Step: {Step}",
                request.RiskAssessmentId, request.CurrentStep);

            // Get the existing assessment first
            var assessmentResult = await _riskAssessmentService.GetRiskAssessmentByIdAsync(request.RiskAssessmentId, ct);
            if (assessmentResult.IsFailure)
            {
                return Result<RiskAssessment>.Failure<RiskAssessment>(assessmentResult.Error);
            }

            var assessment = assessmentResult.Value;

            // Update the assessment with progress data
            assessment.CurrentStep = request.CurrentStep;

            // Handle Smart Enumeration status conversion using the proper method
            var statusFromString = RiskAssessmentStatus.GetAllValues()
                .FirstOrDefault(s => s.Value == request.Status || s.Name == request.Status);
            if (statusFromString != null)
            {
                assessment.Status = statusFromString;
            }

            // Handle Smart Enumeration stage conversion using the proper method  
            var stageFromString = RiskAssessmentStage.GetAllValues()
                .FirstOrDefault(s => s.Value == request.Stage || s.Name == request.Stage);
            if (stageFromString != null)
            {
                assessment.Stage = stageFromString;
            }

            // Use the CompleteStep method which will handle status and stage updates based on business rules
            assessment.CompleteStep(request.CurrentStep);

            assessment.UpdatedBy = request.UpdatedBy;
            assessment.UpdatedDate = DateTime.UtcNow;

            var result = await _riskAssessmentService.UpdateRiskAssessmentAsync(assessment, ct);

            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation(" Successfully updated progress for RiskAssessment: {Id}", request.RiskAssessmentId);
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
            _logger.LogApplicationWarning("UpdateProgressCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while updating progress for RiskAssessment: {Id}", ApplicationEventIds.Error, ex);
            return Result<RiskAssessment>.Failure<RiskAssessment>(DomainErrors.RiskAssessmentError.UpdateFailed);
        }
    }
}

