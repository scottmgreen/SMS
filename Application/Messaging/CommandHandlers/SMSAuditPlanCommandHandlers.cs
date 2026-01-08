using SMS_Application.Common;
using SMS_Application.Interfaces;
using SMS_Application.Messaging.Commands;
using SMS_Application.Services;
using SMS_Domain.Entities;
using SMS_Shared.Common;
using Microsoft.Extensions.Logging;

namespace SMS_Application.Messaging.CommandHandlers;

/// <summary>
/// SMS Audit Plan Command Handlers following established patterns
/// </summary>

public class CreateSMSAuditPlanCommandHandler : BaseCommandBundle, IRequestHandler<CreateSMSAuditPlanCommand, Result<SMSAuditPlan>>
{
    private readonly SMSAuditPlanService _auditPlanService;
    private readonly ILogger<CreateSMSAuditPlanCommandHandler> _logger;

    public CreateSMSAuditPlanCommandHandler(
        SMSAuditPlanService auditPlanService,
        ILogger<CreateSMSAuditPlanCommandHandler> logger)
    {
        _auditPlanService = auditPlanService ?? throw new ArgumentNullException(nameof(auditPlanService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<SMSAuditPlan>> HandleAsync(CreateSMSAuditPlanCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request is null)
            {
                _logger.LogError("CreateSMSAuditPlanCommand received with null request");
                return Result<SMSAuditPlan>.Failure<SMSAuditPlan>(new Error("NULL_REQUEST", "Request cannot be null"));
            }

            _logger.LogInformation("Processing CreateSMSAuditPlanCommand for audit plan: {Name}", request.Name);

            // Generate audit plan code
            var auditPlanCode = _auditPlanService.GenerateAuditPlanCode(request.AuditType, request.PlannedStartDate);
            
            // Create audit plan entity
            var auditPlan = new SMSAuditPlan(new SMSAuditPlanID(auditPlanCode), request.CreatedBy)
            {
                Name = request.Name,
                Description = request.Description,
                AuditType = request.AuditType,
                PlannedStartDate = request.PlannedStartDate,
                PlannedEndDate = request.PlannedEndDate,
                AuditScope = request.AuditScope,
                AuditObjectives = request.AuditObjectives,
                LeadAuditor = request.LeadAuditor,
                ResponsibleDepartment = request.ResponsibleDepartment,
                Status = "Draft",
                EstimatedDurationHours = _auditPlanService.CalculateRecommendedDuration(request.AuditType, request.AuditScope)
            };

            // Validate audit plan
            var validationResult = _auditPlanService.ValidateAuditPlan(auditPlan);
            if (validationResult.IsFailure)
            {
                _logger.LogError("Audit plan validation failed: {Error}", validationResult.Error?.Message);
                return Result<SMSAuditPlan>.Failure<SMSAuditPlan>(validationResult.Error);
            }

            // Save audit plan using the service
            var result = await _auditPlanService.CreateAuditPlanAsync(auditPlan, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully created SMS audit plan: {AuditPlanCode}", auditPlanCode);
            }
            else
            {
                _logger.LogError("Failed to create SMS audit plan: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("CreateSMSAuditPlanCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while creating SMS audit plan");
            return Result<SMSAuditPlan>.Failure<SMSAuditPlan>(new Error("CREATE_FAILED", "Failed to create audit plan"));
        }
    }
}

public class UpdateSMSAuditPlanCommandHandler : BaseCommandBundle, IRequestHandler<UpdateSMSAuditPlanCommand, Result<SMSAuditPlan>>
{
    private readonly SMSAuditPlanService _auditPlanService;
    private readonly ILogger<UpdateSMSAuditPlanCommandHandler> _logger;

    public UpdateSMSAuditPlanCommandHandler(
        SMSAuditPlanService auditPlanService,
        ILogger<UpdateSMSAuditPlanCommandHandler> logger)
    {
        _auditPlanService = auditPlanService ?? throw new ArgumentNullException(nameof(auditPlanService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<SMSAuditPlan>> HandleAsync(UpdateSMSAuditPlanCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request is null)
            {
                _logger.LogError("UpdateSMSAuditPlanCommand received with null request");
                return Result<SMSAuditPlan>.Failure<SMSAuditPlan>(new Error("NULL_REQUEST", "Request cannot be null"));
            }

            _logger.LogInformation("Processing UpdateSMSAuditPlanCommand for audit plan: {AuditPlanCode}", request.AuditPlanCode);

            // Get existing audit plan
            var existingPlanResult = await _auditPlanService.GetAuditPlanByCodeAsync(request.AuditPlanCode, cancellationToken);
            if (existingPlanResult.IsFailure)
            {
                _logger.LogError("Audit plan not found: {AuditPlanCode}", request.AuditPlanCode);
                return Result<SMSAuditPlan>.Failure<SMSAuditPlan>(existingPlanResult.Error);
            }

            var auditPlan = existingPlanResult.Value!;

            // Update audit plan properties
            auditPlan.Name = request.Name;
            auditPlan.Description = request.Description;
            auditPlan.AuditType = request.AuditType;
            auditPlan.PlannedStartDate = request.PlannedStartDate;
            auditPlan.PlannedEndDate = request.PlannedEndDate;
            auditPlan.AuditScope = request.AuditScope;
            auditPlan.AuditObjectives = request.AuditObjectives;
            auditPlan.LeadAuditor = request.LeadAuditor;
            auditPlan.ResponsibleDepartment = request.ResponsibleDepartment;
            auditPlan.UpdatedBy = request.UpdatedBy;
            auditPlan.UpdatedDate = DateTime.UtcNow;

            // Validate updated audit plan
            var validationResult = _auditPlanService.ValidateAuditPlan(auditPlan);
            if (validationResult.IsFailure)
            {
                _logger.LogError("Audit plan validation failed: {Error}", validationResult.Error?.Message);
                return Result<SMSAuditPlan>.Failure<SMSAuditPlan>(validationResult.Error);
            }

            // Save updated audit plan
            var result = await _auditPlanService.UpdateAuditPlanAsync(auditPlan, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully updated SMS audit plan: {AuditPlanCode}", request.AuditPlanCode);
            }
            else
            {
                _logger.LogError("Failed to update SMS audit plan: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("UpdateSMSAuditPlanCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while updating SMS audit plan: {AuditPlanCode}", request?.AuditPlanCode);
            return Result<SMSAuditPlan>.Failure<SMSAuditPlan>(new Error("UPDATE_FAILED", "Failed to update audit plan"));
        }
    }
}

public class ApproveSMSAuditPlanCommandHandler : BaseCommandBundle, IRequestHandler<ApproveSMSAuditPlanCommand, Result<SMSAuditPlan>>
{
    private readonly SMSAuditPlanService _auditPlanService;
    private readonly ILogger<ApproveSMSAuditPlanCommandHandler> _logger;

    public ApproveSMSAuditPlanCommandHandler(
        SMSAuditPlanService auditPlanService,
        ILogger<ApproveSMSAuditPlanCommandHandler> logger)
    {
        _auditPlanService = auditPlanService ?? throw new ArgumentNullException(nameof(auditPlanService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<SMSAuditPlan>> HandleAsync(ApproveSMSAuditPlanCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request is null)
            {
                _logger.LogError("ApproveSMSAuditPlanCommand received with null request");
                return Result<SMSAuditPlan>.Failure<SMSAuditPlan>(new Error("NULL_REQUEST", "Request cannot be null"));
            }

            _logger.LogInformation("Processing ApproveSMSAuditPlanCommand for audit plan: {AuditPlanCode}", request.AuditPlanCode);

            // Get existing audit plan
            var existingPlanResult = await _auditPlanService.GetAuditPlanByCodeAsync(request.AuditPlanCode, cancellationToken);
            if (existingPlanResult.IsFailure)
            {
                _logger.LogError("Audit plan not found: {AuditPlanCode}", request.AuditPlanCode);
                return Result<SMSAuditPlan>.Failure<SMSAuditPlan>(existingPlanResult.Error);
            }

            var auditPlan = existingPlanResult.Value!;

            // Validate approval authority (simplified - would check against actual user roles)
            var authorityValidation = _auditPlanService.ValidateApprovalAuthority(auditPlan, "AuditManager");
            if (authorityValidation.IsFailure)
            {
                _logger.LogError("Approval authority validation failed: {Error}", authorityValidation.Error?.Message);
                return Result<SMSAuditPlan>.Failure<SMSAuditPlan>(authorityValidation.Error);
            }

            // Approve audit plan using domain method
            var approvalResult = auditPlan.ApproveAuditPlan(request.ApprovedBy, request.ApprovalNotes);
            if (approvalResult.IsFailure)
            {
                _logger.LogError("Failed to approve audit plan: {Error}", approvalResult.Error?.Message);
                return Result<SMSAuditPlan>.Failure<SMSAuditPlan>(approvalResult.Error);
            }

            // Save approved audit plan
            var result = await _auditPlanService.UpdateAuditPlanAsync(auditPlan, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully approved SMS audit plan: {AuditPlanCode} by {ApprovedBy}", 
                    request.AuditPlanCode, request.ApprovedBy);
            }
            else
            {
                _logger.LogError("Failed to save approved SMS audit plan: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("ApproveSMSAuditPlanCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while approving SMS audit plan: {AuditPlanCode}", request?.AuditPlanCode);
            return Result<SMSAuditPlan>.Failure<SMSAuditPlan>(new Error("APPROVAL_FAILED", "Failed to approve audit plan"));
        }
    }
}

public class DeleteSMSAuditPlanCommandHandler : BaseCommandBundle, IRequestHandler<DeleteSMSAuditPlanCommand, Result<bool>>
{
    private readonly SMSAuditPlanService _auditPlanService;
    private readonly ILogger<DeleteSMSAuditPlanCommandHandler> _logger;

    public DeleteSMSAuditPlanCommandHandler(
        SMSAuditPlanService auditPlanService,
        ILogger<DeleteSMSAuditPlanCommandHandler> logger)
    {
        _auditPlanService = auditPlanService ?? throw new ArgumentNullException(nameof(auditPlanService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> HandleAsync(DeleteSMSAuditPlanCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request is null)
            {
                _logger.LogError("DeleteSMSAuditPlanCommand received with null request");
                return Result<bool>.Failure<bool>(new Error("NULL_REQUEST", "Request cannot be null"));
            }

            _logger.LogInformation("Processing DeleteSMSAuditPlanCommand for audit plan: {AuditPlanCode}", request.AuditPlanCode);

            var result = await _auditPlanService.DeleteAuditPlanAsync(request.AuditPlanCode, request.DeletedBy, request.DeletionReason, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully deleted SMS audit plan: {AuditPlanCode} by {DeletedBy}", 
                    request.AuditPlanCode, request.DeletedBy);
            }
            else
            {
                _logger.LogError("Failed to delete SMS audit plan: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("DeleteSMSAuditPlanCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while deleting SMS audit plan: {AuditPlanCode}", request?.AuditPlanCode);
            return Result<bool>.Failure<bool>(new Error("DELETE_FAILED", "Failed to delete audit plan"));
        }
    }
}