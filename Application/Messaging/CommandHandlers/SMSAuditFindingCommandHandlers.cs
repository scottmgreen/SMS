using SMS_Application.Common;
using SMS_Application.Interfaces;
using SMS_Application.Messaging.Commands;
using SMS_Application.Services;
using SMS_Infrastructure.Services;
using SMS_Domain.Entities;
using SMS_Shared.Common;
using Microsoft.Extensions.Logging;

namespace SMS_Application.Messaging.CommandHandlers;

/// <summary>
/// SMS Audit Finding Command Handlers following established patterns
/// </summary>

public class CreateSMSAuditFindingCommandHandler : BaseCommandBundle, IRequestHandler<CreateSMSAuditFindingCommand, Result<SMSAuditFinding>>
{
    private readonly SMSAuditFindingDataService _auditFindingDataService;
    private readonly SMSAuditService _auditService;
    private readonly ILogger<CreateSMSAuditFindingCommandHandler> _logger;

    public CreateSMSAuditFindingCommandHandler(
        SMSAuditFindingDataService auditFindingDataService,
        SMSAuditService auditService,
        ILogger<CreateSMSAuditFindingCommandHandler> logger)
    {
        _auditFindingDataService = auditFindingDataService ?? throw new ArgumentNullException(nameof(auditFindingDataService));
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<SMSAuditFinding>> HandleAsync(CreateSMSAuditFindingCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request is null)
            {
                _logger.LogError("CreateSMSAuditFindingCommand received with null request");
                return Result<SMSAuditFinding>.Failure<SMSAuditFinding>(new Error("NULL_REQUEST", "Request cannot be null"));
            }

            _logger.LogInformation("Processing CreateSMSAuditFindingCommand for audit: {AuditCode}", request.AuditCode);

            // Verify audit exists
            var auditResult = await _auditService.GetAuditByCodeAsync(request.AuditCode, cancellationToken);
            if (auditResult.IsFailure)
            {
                _logger.LogError("Audit not found: {AuditCode}", request.AuditCode);
                return Result<SMSAuditFinding>.Failure<SMSAuditFinding>(auditResult.Error);
            }

            // Generate finding code
            var findingCode = GenerateFindingCode(request.AuditCode, request.Severity);

            // Create finding entity
            var finding = new SMSAuditFinding(new SMSAuditFindingID(findingCode), request.CreatedBy)
            {
                AuditCode = request.AuditCode,
                FindingDescription = request.FindingDescription,
                // ? FIX: Map to both Description and Title for repository compatibility
                Description = request.FindingDescription, 
                Title = request.FindingDescription, // Use description as title for now
                Severity = request.Severity,
                FindingType = request.FindingType,
                Category = request.FindingType, // ? FIX: Map FindingType to Category for repository
                AffectedArea = request.AffectedArea,
                RequirementReference = request.RequirementReference,
                EvidenceDescription = request.EvidenceDescription,
                Status = "Open",
                DiscoveredDate = DateTime.UtcNow,
                FoundDate = DateTime.UtcNow,
                FoundBy = request.CreatedBy
            };

            // Save finding
            var result = await _auditFindingDataService.CreateAuditFindingAsync(finding, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully created SMS audit finding: {FindingCode}", findingCode);
            }
            else
            {
                _logger.LogError("Failed to create SMS audit finding: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("CreateSMSAuditFindingCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while creating SMS audit finding");
            return Result<SMSAuditFinding>.Failure<SMSAuditFinding>(new Error("CREATE_FINDING_FAILED", "Failed to create audit finding"));
        }
    }

    private string GenerateFindingCode(string auditCode, string severity)
    {
        var severityPrefix = severity switch
        {
            "Critical" => "C",
            "Major" => "M",
            "Minor" => "m",
            "Observation" => "O",
            _ => "F"
        };

        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        return $"FND-{severityPrefix}-{auditCode}-{timestamp}";
    }
}

public class UpdateSMSAuditFindingCommandHandler : BaseCommandBundle, IRequestHandler<UpdateSMSAuditFindingCommand, Result<SMSAuditFinding>>
{
    private readonly SMSAuditFindingDataService _auditFindingDataService;
    private readonly ILogger<UpdateSMSAuditFindingCommandHandler> _logger;

    public UpdateSMSAuditFindingCommandHandler(
        SMSAuditFindingDataService auditFindingDataService,
        ILogger<UpdateSMSAuditFindingCommandHandler> logger)
    {
        _auditFindingDataService = auditFindingDataService ?? throw new ArgumentNullException(nameof(auditFindingDataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<SMSAuditFinding>> HandleAsync(UpdateSMSAuditFindingCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request is null)
            {
                _logger.LogError("UpdateSMSAuditFindingCommand received with null request");
                return Result<SMSAuditFinding>.Failure<SMSAuditFinding>(new Error("NULL_REQUEST", "Request cannot be null"));
            }

            _logger.LogInformation("Processing UpdateSMSAuditFindingCommand for finding: {FindingCode}", request.FindingCode);

            // Get existing finding
            var existingResult = await _auditFindingDataService.GetAuditFindingByCodeAsync(request.FindingCode, cancellationToken);
            if (existingResult.IsFailure)
            {
                _logger.LogError("Finding not found: {FindingCode}", request.FindingCode);
                return Result<SMSAuditFinding>.Failure<SMSAuditFinding>(existingResult.Error);
            }

            var finding = existingResult.Value!;

            // Update finding properties
            finding.FindingDescription = request.FindingDescription;
            finding.Severity = request.Severity;
            finding.FindingType = request.FindingType;
            finding.AffectedArea = request.AffectedArea;
            finding.RequirementReference = request.RequirementReference;
            finding.EvidenceDescription = request.EvidenceDescription;
            finding.RootCause = request.RootCause;
            finding.UpdatedBy = request.UpdatedBy;
            finding.UpdatedDate = DateTime.UtcNow;

            // Save updated finding
            var result = await _auditFindingDataService.UpdateAuditFindingAsync(finding, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully updated SMS audit finding: {FindingCode}", request.FindingCode);
            }
            else
            {
                _logger.LogError("Failed to update SMS audit finding: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("UpdateSMSAuditFindingCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while updating SMS audit finding: {FindingCode}", request?.FindingCode);
            return Result<SMSAuditFinding>.Failure<SMSAuditFinding>(new Error("UPDATE_FINDING_FAILED", "Failed to update audit finding"));
        }
    }
}

public class AssignSMSAuditCorrectiveActionCommandHandler : BaseCommandBundle, IRequestHandler<AssignSMSAuditCorrectiveActionCommand, Result<SMSAuditFinding>>
{
    private readonly SMSAuditFindingDataService _auditFindingDataService;
    private readonly ILogger<AssignSMSAuditCorrectiveActionCommandHandler> _logger;

    public AssignSMSAuditCorrectiveActionCommandHandler(
        SMSAuditFindingDataService auditFindingDataService,
        ILogger<AssignSMSAuditCorrectiveActionCommandHandler> logger)
    {
        _auditFindingDataService = auditFindingDataService ?? throw new ArgumentNullException(nameof(auditFindingDataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<SMSAuditFinding>> HandleAsync(AssignSMSAuditCorrectiveActionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request is null)
            {
                _logger.LogError("AssignSMSAuditCorrectiveActionCommand received with null request");
                return Result<SMSAuditFinding>.Failure<SMSAuditFinding>(new Error("NULL_REQUEST", "Request cannot be null"));
            }

            _logger.LogInformation("Processing AssignSMSAuditCorrectiveActionCommand for finding: {FindingCode}", request.FindingCode);

            // Get existing finding
            var existingResult = await _auditFindingDataService.GetAuditFindingByCodeAsync(request.FindingCode, cancellationToken);
            if (existingResult.IsFailure)
            {
                _logger.LogError("Finding not found: {FindingCode}", request.FindingCode);
                return Result<SMSAuditFinding>.Failure<SMSAuditFinding>(existingResult.Error);
            }

            var finding = existingResult.Value!;

            // Use domain method to assign corrective action
            var assignResult = finding.AssignCorrectiveAction(
                request.CorrectiveAction, 
                request.ResponsiblePerson, 
                request.ResponsibleDepartment, 
                request.TargetCompletionDate, 
                request.AssignedBy);

            if (assignResult.IsFailure)
            {
                _logger.LogError("Failed to assign corrective action using domain method: {Error}", assignResult.Error?.Message);
                return Result<SMSAuditFinding>.Failure<SMSAuditFinding>(assignResult.Error);
            }

            // Save updated finding
            var result = await _auditFindingDataService.UpdateAuditFindingAsync(finding, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully assigned corrective action for finding: {FindingCode} to {ResponsiblePerson}", 
                    request.FindingCode, request.ResponsiblePerson);
            }
            else
            {
                _logger.LogError("Failed to save corrective action assignment: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("AssignSMSAuditCorrectiveActionCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while assigning corrective action: {FindingCode}", request?.FindingCode);
            return Result<SMSAuditFinding>.Failure<SMSAuditFinding>(new Error("ASSIGN_ACTION_FAILED", "Failed to assign corrective action"));
        }
    }
}

public class CompleteSMSAuditCorrectiveActionCommandHandler : BaseCommandBundle, IRequestHandler<CompleteSMSAuditCorrectiveActionCommand, Result<SMSAuditFinding>>
{
    private readonly SMSAuditFindingDataService _auditFindingDataService;
    private readonly ILogger<CompleteSMSAuditCorrectiveActionCommandHandler> _logger;

    public CompleteSMSAuditCorrectiveActionCommandHandler(
        SMSAuditFindingDataService auditFindingDataService,
        ILogger<CompleteSMSAuditCorrectiveActionCommandHandler> logger)
    {
        _auditFindingDataService = auditFindingDataService ?? throw new ArgumentNullException(nameof(auditFindingDataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<SMSAuditFinding>> HandleAsync(CompleteSMSAuditCorrectiveActionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request is null)
            {
                _logger.LogError("CompleteSMSAuditCorrectiveActionCommand received with null request");
                return Result<SMSAuditFinding>.Failure<SMSAuditFinding>(new Error("NULL_REQUEST", "Request cannot be null"));
            }

            _logger.LogInformation("Processing CompleteSMSAuditCorrectiveActionCommand for finding: {FindingCode}", request.FindingCode);

            // Get existing finding
            var existingResult = await _auditFindingDataService.GetAuditFindingByCodeAsync(request.FindingCode, cancellationToken);
            if (existingResult.IsFailure)
            {
                _logger.LogError("Finding not found: {FindingCode}", request.FindingCode);
                return Result<SMSAuditFinding>.Failure<SMSAuditFinding>(existingResult.Error);
            }

            var finding = existingResult.Value!;

            // Use domain method to complete corrective action
            var completeResult = finding.CompleteCorrectiveAction(
                request.CompletedBy, 
                request.CompletionDate, 
                request.CompletionEvidence);

            if (completeResult.IsFailure)
            {
                _logger.LogError("Failed to complete corrective action using domain method: {Error}", completeResult.Error?.Message);
                return Result<SMSAuditFinding>.Failure<SMSAuditFinding>(completeResult.Error);
            }

            // Save updated finding
            var result = await _auditFindingDataService.UpdateAuditFindingAsync(finding, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully completed corrective action for finding: {FindingCode}", request.FindingCode);
            }
            else
            {
                _logger.LogError("Failed to save corrective action completion: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("CompleteSMSAuditCorrectiveActionCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while completing corrective action: {FindingCode}", request?.FindingCode);
            return Result<SMSAuditFinding>.Failure<SMSAuditFinding>(new Error("COMPLETE_ACTION_FAILED", "Failed to complete corrective action"));
        }
    }
}

public class VerifySMSAuditFindingCommandHandler : BaseCommandBundle, IRequestHandler<VerifySMSAuditFindingCommand, Result<SMSAuditFinding>>
{
    private readonly SMSAuditFindingDataService _auditFindingDataService;
    private readonly ILogger<VerifySMSAuditFindingCommandHandler> _logger;

    public VerifySMSAuditFindingCommandHandler(
        SMSAuditFindingDataService auditFindingDataService,
        ILogger<VerifySMSAuditFindingCommandHandler> logger)
    {
        _auditFindingDataService = auditFindingDataService ?? throw new ArgumentNullException(nameof(auditFindingDataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<SMSAuditFinding>> HandleAsync(VerifySMSAuditFindingCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request is null)
            {
                _logger.LogError("VerifySMSAuditFindingCommand received with null request");
                return Result<SMSAuditFinding>.Failure<SMSAuditFinding>(new Error("NULL_REQUEST", "Request cannot be null"));
            }

            _logger.LogInformation("Processing VerifySMSAuditFindingCommand for finding: {FindingCode}", request.FindingCode);

            // Get existing finding
            var existingResult = await _auditFindingDataService.GetAuditFindingByCodeAsync(request.FindingCode, cancellationToken);
            if (existingResult.IsFailure)
            {
                _logger.LogError("Finding not found: {FindingCode}", request.FindingCode);
                return Result<SMSAuditFinding>.Failure<SMSAuditFinding>(existingResult.Error);
            }

            var finding = existingResult.Value!;

            // Use domain method to verify finding
            var verifyResult = finding.VerifyFinding(
                request.VerifiedBy, 
                request.VerificationMethod, 
                request.VerificationEvidence);

            if (verifyResult.IsFailure)
            {
                _logger.LogError("Failed to verify finding using domain method: {Error}", verifyResult.Error?.Message);
                return Result<SMSAuditFinding>.Failure<SMSAuditFinding>(verifyResult.Error);
            }

            // Save updated finding
            var result = await _auditFindingDataService.UpdateAuditFindingAsync(finding, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully verified finding: {FindingCode} by {VerifiedBy}", 
                    request.FindingCode, request.VerifiedBy);
            }
            else
            {
                _logger.LogError("Failed to save finding verification: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("VerifySMSAuditFindingCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while verifying finding: {FindingCode}", request?.FindingCode);
            return Result<SMSAuditFinding>.Failure<SMSAuditFinding>(new Error("VERIFY_FINDING_FAILED", "Failed to verify finding"));
        }
    }
}

public class DeleteSMSAuditFindingCommandHandler : BaseCommandBundle, IRequestHandler<DeleteSMSAuditFindingCommand, Result<bool>>
{
    private readonly SMSAuditFindingDataService _auditFindingDataService;
    private readonly ILogger<DeleteSMSAuditFindingCommandHandler> _logger;

    public DeleteSMSAuditFindingCommandHandler(
        SMSAuditFindingDataService auditFindingDataService,
        ILogger<DeleteSMSAuditFindingCommandHandler> logger)
    {
        _auditFindingDataService = auditFindingDataService ?? throw new ArgumentNullException(nameof(auditFindingDataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> HandleAsync(DeleteSMSAuditFindingCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request is null)
            {
                _logger.LogError("DeleteSMSAuditFindingCommand received with null request");
                return Result<bool>.Failure<bool>(new Error("NULL_REQUEST", "Request cannot be null"));
            }

            _logger.LogInformation("Processing DeleteSMSAuditFindingCommand for finding: {FindingCode}", request.FindingCode);

            // Delete finding
            var result = await _auditFindingDataService.DeleteAuditFindingAsync(
                request.FindingCode, 
                request.DeletedBy, 
                request.DeletionReason, 
                cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully deleted finding: {FindingCode} by {DeletedBy}", 
                    request.FindingCode, request.DeletedBy);
            }
            else
            {
                _logger.LogError("Failed to delete finding: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("DeleteSMSAuditFindingCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while deleting finding: {FindingCode}", request?.FindingCode);
            return Result<bool>.Failure<bool>(new Error("DELETE_FINDING_FAILED", "Failed to delete finding"));
        }
    }
}