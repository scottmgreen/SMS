using SMS_Application.Common;
using SMS_Application.Interfaces;
using SMS_Application.Messaging.Commands;
using SMS_Application.Services;
using SMS_Domain.Entities;
using SMS_Shared.Common;
using Microsoft.Extensions.Logging;

namespace SMS_Application.Messaging.CommandHandlers;

/// <summary>
/// SMS Audit Command Handlers following established patterns
/// </summary>

public class CreateSMSAuditCommandHandler : BaseCommandBundle, IRequestHandler<CreateSMSAuditCommand, Result<SMSAudit>>
{
    private readonly SMSAuditService _auditService;
    private readonly ILogger<CreateSMSAuditCommandHandler> _logger;

    public CreateSMSAuditCommandHandler(
        SMSAuditService auditService,
        ILogger<CreateSMSAuditCommandHandler> logger)
    {
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<SMSAudit>> HandleAsync(CreateSMSAuditCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request is null)
            {
                _logger.LogError("CreateSMSAuditCommand received with null request");
                return Result<SMSAudit>.Failure<SMSAudit>(new Error("NULL_REQUEST", "Request cannot be null"));
            }

            _logger.LogInformation("Processing CreateSMSAuditCommand for audit: {Name}", request.Name);

            // Generate audit code
            var auditCode = _auditService.GenerateAuditCode(request.AuditType, request.ScheduledStartDate);
            
            // Create audit entity
            var audit = new SMSAudit(new SMSAuditID(auditCode), request.CreatedBy)
            {
                Name = request.Name,
                Description = request.Description,
                AuditType = request.AuditType,
                ScheduledStartDate = request.ScheduledStartDate,
                ScheduledEndDate = request.ScheduledEndDate,
                LeadAuditor = request.LeadAuditor,
                ResponsibleDepartment = request.ResponsibleDepartment,
                Status = "Scheduled",
                CurrentPhase = "Planning"
            };

            // Validate audit
            var validationResult = _auditService.ValidateAudit(audit);
            if (validationResult.IsFailure)
            {
                _logger.LogError("Audit validation failed: {Error}", validationResult.Error?.Message);
                return Result<SMSAudit>.Failure<SMSAudit>(validationResult.Error);
            }

            // Save audit using the service
            var result = await _auditService.CreateAuditAsync(audit, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully created SMS audit: {AuditCode}", auditCode);
            }
            else
            {
                _logger.LogError("Failed to create SMS audit: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("CreateSMSAuditCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while creating SMS audit");
            return Result<SMSAudit>.Failure<SMSAudit>(new Error("CREATE_FAILED", "Failed to create audit"));
        }
    }
}

public class StartSMSAuditCommandHandler : BaseCommandBundle, IRequestHandler<StartSMSAuditCommand, Result<SMSAudit>>
{
    private readonly SMSAuditService _auditService;
    private readonly ILogger<StartSMSAuditCommandHandler> _logger;

    public StartSMSAuditCommandHandler(
        SMSAuditService auditService,
        ILogger<StartSMSAuditCommandHandler> logger)
    {
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<SMSAudit>> HandleAsync(StartSMSAuditCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request is null)
            {
                _logger.LogError("StartSMSAuditCommand received with null request");
                return Result<SMSAudit>.Failure<SMSAudit>(new Error("NULL_REQUEST", "Request cannot be null"));
            }

            _logger.LogInformation("Processing StartSMSAuditCommand for audit: {AuditCode}", request.AuditCode);

            //Get existing audit
           var existingAuditResult = await _auditService.GetAuditByCodeAsync(request.AuditCode, cancellationToken);
            if (existingAuditResult.IsFailure)
            {
                _logger.LogError("Audit not found: {AuditCode}", request.AuditCode);
                return Result<SMSAudit>.Failure<SMSAudit>(existingAuditResult.Error);
            }

            var audit = existingAuditResult.Value!;

            // Start audit using domain method
            var startResult = audit.StartAudit(request.StartedBy);
            if (startResult.IsFailure)
            {
                _logger.LogError("Failed to start audit: {Error}", startResult.Error?.Message);
                return Result<SMSAudit>.Failure<SMSAudit>(startResult.Error);
            }

            // Save updated audit
            var result = await _auditService.UpdateAuditAsync(audit, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully started SMS audit: {AuditCode} by {StartedBy}", 
                    request.AuditCode, request.StartedBy);
            }
            else
            {
                _logger.LogError("Failed to save started SMS audit: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("StartSMSAuditCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while starting SMS audit: {AuditCode}", request?.AuditCode);
            return Result<SMSAudit>.Failure<SMSAudit>(new Error("START_FAILED", "Failed to start audit"));
        }
    }
}

public class CompleteSMSAuditCommandHandler : BaseCommandBundle, IRequestHandler<CompleteSMSAuditCommand, Result<SMSAudit>>
{
    private readonly SMSAuditService _auditService;
    private readonly ILogger<CompleteSMSAuditCommandHandler> _logger;

    public CompleteSMSAuditCommandHandler(
        SMSAuditService auditService,
        ILogger<CompleteSMSAuditCommandHandler> logger)
    {
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<SMSAudit>> HandleAsync(CompleteSMSAuditCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request is null)
            {
                _logger.LogError("CompleteSMSAuditCommand received with null request");
                return Result<SMSAudit>.Failure<SMSAudit>(new Error("NULL_REQUEST", "Request cannot be null"));
            }

            _logger.LogInformation("Processing CompleteSMSAuditCommand for audit: {AuditCode}", request.AuditCode);

            // Get existing audit with findings
            var existingAuditResult = await _auditService.GetAuditByCodeAsync(request.AuditCode, cancellationToken, includeFindings: true);
            if (existingAuditResult.IsFailure)
            {
                _logger.LogError("Audit not found: {AuditCode}", request.AuditCode);
                return Result<SMSAudit>.Failure<SMSAudit>(existingAuditResult.Error);
            }

            var audit = existingAuditResult.Value!;

            // Complete audit using domain method
            var completeResult = audit.CompleteAudit(request.CompletedBy, request.AuditSummary, request.KeyFindings);
            if (completeResult.IsFailure)
            {
                _logger.LogError("Failed to complete audit: {Error}", completeResult.Error?.Message);
                return Result<SMSAudit>.Failure<SMSAudit>(completeResult.Error);
            }

            // Update additional fields
            audit.Recommendations = request.Recommendations;
            audit.Conclusions = request.Conclusions;

            // Save completed audit
            var result = await _auditService.UpdateAuditAsync(audit, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully completed SMS audit: {AuditCode} by {CompletedBy}", 
                    request.AuditCode, request.CompletedBy);
            }
            else
            {
                _logger.LogError("Failed to save completed SMS audit: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("CompleteSMSAuditCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while completing SMS audit: {AuditCode}", request?.AuditCode);
            return Result<SMSAudit>.Failure<SMSAudit>(new Error("COMPLETE_FAILED", "Failed to complete audit"));
        }
    }
}

public class AddSMSAuditFindingCommandHandler : BaseCommandBundle, IRequestHandler<AddSMSAuditFindingCommand, Result<SMSAuditFinding>>
{
    private readonly SMSAuditService _auditService;
    private readonly ILogger<AddSMSAuditFindingCommandHandler> _logger;

    public AddSMSAuditFindingCommandHandler(
        SMSAuditService auditService,
        ILogger<AddSMSAuditFindingCommandHandler> logger)
    {
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<SMSAuditFinding>> HandleAsync(AddSMSAuditFindingCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request is null)
            {
                _logger.LogError("AddSMSAuditFindingCommand received with null request");
                return Result<SMSAuditFinding>.Failure<SMSAuditFinding>(new Error("NULL_REQUEST", "Request cannot be null"));
            }

            _logger.LogInformation("Processing AddSMSAuditFindingCommand for audit: {AuditCode}", request.AuditCode);

            // Get existing audit
            var existingAuditResult = await _auditService.GetAuditByCodeAsync(request.AuditCode, cancellationToken, includeFindings: true);
            if (existingAuditResult.IsFailure)
            {
                _logger.LogError("Audit not found: {AuditCode}", request.AuditCode);
                return Result<SMSAuditFinding>.Failure<SMSAuditFinding>(existingAuditResult.Error);
            }

            var audit = existingAuditResult.Value!;

            // Add finding using domain method
            var addFindingResult = audit.AddFinding(request.FindingDescription, request.Severity, request.FoundBy);
            if (addFindingResult.IsFailure)
            {
                _logger.LogError("Failed to add finding to audit: {Error}", addFindingResult.Error?.Message);
                return Result<SMSAuditFinding>.Failure<SMSAuditFinding>(addFindingResult.Error);
            }

            // Get the newly added finding and update additional properties
            var newFinding = audit.Findings.LastOrDefault();
            if (newFinding != null)
            {
                newFinding.FindingType = request.FindingType;
                newFinding.AffectedArea = request.AffectedArea;
                newFinding.RequirementReference = request.RequirementReference;
            }

            // Save updated audit with new finding
            var updateResult = await _auditService.UpdateAuditAsync(audit, cancellationToken);
            if (updateResult.IsFailure)
            {
                _logger.LogError("Failed to save audit with new finding: {Error}", updateResult.Error?.Message);
                return Result<SMSAuditFinding>.Failure<SMSAuditFinding>(updateResult.Error);
            }

            if (newFinding != null)
            {
                _logger.LogInformation("Successfully added finding to SMS audit: {AuditCode}, Finding: {FindingCode}", 
                    request.AuditCode, newFinding.Code);
                return Result<SMSAuditFinding>.Success(newFinding);
            }
            else
            {
                return Result<SMSAuditFinding>.Failure<SMSAuditFinding>(new Error("FINDING_NOT_CREATED", "Finding was not properly created"));
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("AddSMSAuditFindingCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while adding SMS audit finding: {AuditCode}", request?.AuditCode);
            return Result<SMSAuditFinding>.Failure<SMSAuditFinding>(new Error("ADD_FINDING_FAILED", "Failed to add audit finding"));
        }
    }
}

public class UpdateSMSAuditCommandHandler : BaseCommandBundle, IRequestHandler<UpdateSMSAuditCommand, Result<SMSAudit>>
{
    private readonly SMSAuditService _auditService;
    private readonly ILogger<UpdateSMSAuditCommandHandler> _logger;

    public UpdateSMSAuditCommandHandler(
        SMSAuditService auditService,
        ILogger<UpdateSMSAuditCommandHandler> logger)
    {
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<SMSAudit>> HandleAsync(UpdateSMSAuditCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request is null)
            {
                _logger.LogError("UpdateSMSAuditCommand received with null request");
                return Result<SMSAudit>.Failure<SMSAudit>(new Error("NULL_REQUEST", "Request cannot be null"));
            }

            _logger.LogInformation("Processing UpdateSMSAuditCommand for audit: {Name}", request.Audit.Name);

            // Validate audit
            var validationResult = _auditService.ValidateAudit(request.Audit);
            if (validationResult.IsFailure)
            {
                _logger.LogError("Audit validation failed: {Error}", validationResult.Error?.Message);
                return Result<SMSAudit>.Failure<SMSAudit>(validationResult.Error);
            }

            // Save audit using the service
            var result = await _auditService.UpdateAuditAsync(request.Audit, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully created SMS audit: {AuditCode}", request.Audit.Code);
            }
            else
            {
                _logger.LogError("Failed to create SMS audit: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("CreateSMSAuditCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while creating SMS audit");
            return Result<SMSAudit>.Failure<SMSAudit>(new Error("CREATE_FAILED", "Failed to create audit"));
        }
    }
}