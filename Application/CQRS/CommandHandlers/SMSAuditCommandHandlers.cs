//-----------------------------------------------------------------------
// <copyright file="SMSAuditCommandHandlers.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Command handlers implementing SMS audit management business logic and workflow.
//                  Implements command handlers for processing write operations.
//                  Handles business logic execution and domain entity coordination.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;

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
                _logger.LogApplicationError("CreateSMSAuditCommand received with null request", ApplicationEventIds.Error, null);
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
                _logger.LogApplicationError("Audit validation failed: {Error}", ApplicationEventIds.Error, null);
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
                _logger.LogApplicationError("Failed to create SMS audit: {Error}", ApplicationEventIds.Error, null);
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
            _logger.LogApplicationError("Unexpected error occurred while creating SMS audit", ApplicationEventIds.Error, ex);
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
                _logger.LogApplicationError("StartSMSAuditCommand received with null request", ApplicationEventIds.Error, null);
                return Result<SMSAudit>.Failure<SMSAudit>(new Error("NULL_REQUEST", "Request cannot be null"));
            }

            _logger.LogInformation("Processing StartSMSAuditCommand for audit: {AuditCode}", request.AuditCode);

            //Get existing audit
            var existingAuditResult = await _auditService.GetAuditByCodeAsync(request.AuditCode, cancellationToken);
            if (existingAuditResult.IsFailure)
            {
                _logger.LogApplicationError("Audit not found: {AuditCode}", ApplicationEventIds.Error, null);
                return Result<SMSAudit>.Failure<SMSAudit>(existingAuditResult.Error);
            }

            var audit = existingAuditResult.Value!;

            // Start audit using domain method
            var startResult = audit.StartAudit(request.StartedBy);
            if (startResult.IsFailure)
            {
                _logger.LogApplicationError("Failed to start audit: {Error}", ApplicationEventIds.Error, null);
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
                _logger.LogApplicationError("Failed to save started SMS audit: {Error}", ApplicationEventIds.Error, null);
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
            _logger.LogApplicationError("Unexpected error occurred while starting SMS audit: {AuditCode}", ApplicationEventIds.Error, ex);
            return Result<SMSAudit>.Failure<SMSAudit>(new Error("START_FAILED", "Failed to start audit"));
        }
    }
}

public class CompleteSMSAuditCommandHandler : BaseCommandBundle, IRequestHandler<CompleteSMSAuditCommand, Result<SMSAudit>>
{
    private readonly SMSAuditService _auditService;
    private readonly SMSAuditPlanService _auditPlanService; // Add this
    private readonly ILogger<CompleteSMSAuditCommandHandler> _logger;

    public CompleteSMSAuditCommandHandler(
        SMSAuditService auditService,
        SMSAuditPlanService auditPlanService, // Add this
        ILogger<CompleteSMSAuditCommandHandler> logger)
    {
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        _auditPlanService = auditPlanService ?? throw new ArgumentNullException(nameof(auditPlanService)); // Add this
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<SMSAudit>> HandleAsync(CompleteSMSAuditCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request is null)
            {
                _logger.LogApplicationError("CompleteSMSAuditCommand received with null request", ApplicationEventIds.Error, null);
                return Result<SMSAudit>.Failure<SMSAudit>(new Error("NULL_REQUEST", "Request cannot be null"));
            }

            _logger.LogInformation("Processing CompleteSMSAuditCommand for audit: {AuditCode}", request.AuditCode);

            // Get existing audit with findings
            var existingAuditResult = await _auditService.GetAuditByCodeAsync(request.AuditCode, cancellationToken, includeFindings: true);
            if (existingAuditResult.IsFailure)
            {
                _logger.LogApplicationError("Audit not found: {AuditCode}", ApplicationEventIds.Error, null);
                return Result<SMSAudit>.Failure<SMSAudit>(existingAuditResult.Error);
            }

            var audit = existingAuditResult.Value!;

            // Complete audit using domain method
            var completeResult = audit.CompleteAudit(request.CompletedBy, request.AuditSummary, request.KeyFindings);
            if (completeResult.IsFailure)
            {
                _logger.LogApplicationError("Failed to complete audit: {Error}", ApplicationEventIds.Error, null);
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

                // NEW: Check if associated audit plan should be completed
                await CheckAndCompleteAuditPlanIfNeeded(audit.AuditPlanCode, cancellationToken);
            }
            else
            {
                _logger.LogApplicationError("Failed to save completed SMS audit: {Error}", ApplicationEventIds.Error, null);
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
            _logger.LogApplicationError("Unexpected error occurred while completing SMS audit: {AuditCode}", ApplicationEventIds.Error, ex);
            return Result<SMSAudit>.Failure<SMSAudit>(new Error("COMPLETE_FAILED", "Failed to complete audit"));
        }
    }

    /// <summary>
    /// Checks if all audits for an audit plan are completed, and if so, marks the plan as completed
    /// </summary>
    private async Task CheckAndCompleteAuditPlanIfNeeded(string? auditPlanCode, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrEmpty(auditPlanCode))
            {
                _logger.LogInformation("No audit plan code provided, skipping plan completion check");
                return;
            }

            _logger.LogInformation("Checking if audit plan {AuditPlanCode} should be completed", auditPlanCode);

            // Get the audit plan
            var auditPlanResult = await _auditPlanService.GetAuditPlanByCodeAsync(auditPlanCode, cancellationToken);
            if (auditPlanResult.IsFailure)
            {
                _logger.LogWarning("Could not find audit plan {AuditPlanCode} for completion check", auditPlanCode);
                return;
            }

            var auditPlan = auditPlanResult.Value!;

            // Skip if already completed
            if (auditPlan.Status == "Completed")
            {
                _logger.LogInformation("Audit plan {AuditPlanCode} is already completed", auditPlanCode);
                return;
            }

            // Get all audits associated with this plan
            var allAuditsResult = await _auditService.GetAuditsByPlanAsync(auditPlanCode, null, false, cancellationToken);
            if (allAuditsResult.IsFailure || !allAuditsResult.Value.Any())
            {
                _logger.LogInformation("No audits found for plan {AuditPlanCode}, cannot complete plan", auditPlanCode);
                return;
            }

            var allAudits = allAuditsResult.Value;
            var completedAudits = allAudits.Count(a => a.Status == "Completed");
            var totalAudits = allAudits.Count;

            _logger.LogInformation("Audit plan {AuditPlanCode}: {CompletedAudits}/{TotalAudits} audits completed",
                auditPlanCode, completedAudits, totalAudits);

            // If all audits are completed, complete the plan
            if (completedAudits == totalAudits && allAudits.All(a => a.Status == "Completed"))
            {
                _logger.LogInformation("All audits completed for plan {AuditPlanCode}, marking plan as completed", auditPlanCode);

                // Complete the audit plan using domain method
                var completePlanResult = auditPlan.CompleteAuditPlan("SYSTEM", "All associated audits completed");
                if (completePlanResult.IsSuccess)
                {
                    // Save the completed audit plan
                    var updatePlanResult = await _auditPlanService.UpdateAuditPlanAsync(auditPlan, cancellationToken);
                    if (updatePlanResult.IsSuccess)
                    {
                        _logger.LogInformation("Successfully completed audit plan {AuditPlanCode}", auditPlanCode);
                    }
                    else
                    {
                        _logger.LogApplicationError("Failed to save completed audit plan {AuditPlanCode}: {Error}",
                            ApplicationEventIds.Error, null);
                    }
                }
                else
                {
                    _logger.LogApplicationError("Failed to complete audit plan {AuditPlanCode}: {Error}",
                        ApplicationEventIds.Error, null);
                }
            }
            else
            {
                _logger.LogInformation("Not all audits completed yet for plan {AuditPlanCode}, keeping plan status as {Status}",
                    auditPlanCode, auditPlan.Status);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking audit plan completion for {AuditPlanCode}", auditPlanCode);
            // Don't throw - this is a secondary operation that shouldn't fail the primary audit completion
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
                _logger.LogApplicationError("AddSMSAuditFindingCommand received with null request", ApplicationEventIds.Error, null);
                return Result<SMSAuditFinding>.Failure<SMSAuditFinding>(new Error("NULL_REQUEST", "Request cannot be null"));
            }

            _logger.LogInformation("Processing AddSMSAuditFindingCommand for audit: {AuditCode}", request.AuditCode);

            // Get existing audit
            var existingAuditResult = await _auditService.GetAuditByCodeAsync(request.AuditCode, cancellationToken, includeFindings: true);
            if (existingAuditResult.IsFailure)
            {
                _logger.LogApplicationError("Audit not found: {AuditCode}", ApplicationEventIds.Error, null);
                return Result<SMSAuditFinding>.Failure<SMSAuditFinding>(existingAuditResult.Error);
            }

            var audit = existingAuditResult.Value!;

            // Add finding using domain method
            var addFindingResult = audit.AddFinding(request.FindingDescription, request.Severity, request.FoundBy);
            if (addFindingResult.IsFailure)
            {
                _logger.LogApplicationError("Failed to add finding to audit: {Error}", ApplicationEventIds.Error, null);
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
                _logger.LogApplicationError("Failed to save audit with new finding: {Error}", ApplicationEventIds.Error, null);
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
            _logger.LogApplicationError("Unexpected error occurred while adding SMS audit finding: {AuditCode}", ApplicationEventIds.Error, ex);
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
                _logger.LogApplicationError("UpdateSMSAuditCommand received with null request", ApplicationEventIds.Error, null);
                return Result<SMSAudit>.Failure<SMSAudit>(new Error("NULL_REQUEST", "Request cannot be null"));
            }

            _logger.LogInformation("Processing UpdateSMSAuditCommand for audit: {Name}", request.Audit.Name);

            // Validate audit
            var validationResult = _auditService.ValidateAudit(request.Audit);
            if (validationResult.IsFailure)
            {
                _logger.LogApplicationError("Audit validation failed: {Error}", ApplicationEventIds.Error, null);
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
                _logger.LogApplicationError("Failed to create SMS audit: {Error}", ApplicationEventIds.Error, null);
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
            _logger.LogApplicationError("Unexpected error occurred while creating SMS audit", ApplicationEventIds.Error, ex);
            return Result<SMSAudit>.Failure<SMSAudit>(new Error("CREATE_FAILED", "Failed to create audit"));
        }
    }
}
