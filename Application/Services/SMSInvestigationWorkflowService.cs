using SMS_Application.Interfaces;
using SMS_Application.Messaging.Commands;
using SMS_Application.Messaging.Queries;
using SMS_Domain.Entities;
using SMS_Shared.Common;
using Microsoft.Extensions.Logging;

namespace SMS_Application.Services;

/// <summary>
/// SMS Investigation Workflow Service - Mission Critical
/// Orchestrates the complete investigation workflow following SMS standards
/// Uses pure Domain Entities and CQRS pattern via MediatorService
/// </summary>
public class SMSInvestigationWorkflowService : ISMSInvestigationWorkflowService
{
    private readonly IMediator _mediator;
    private readonly ILogger<SMSInvestigationWorkflowService> _logger;

    public SMSInvestigationWorkflowService(
        IMediator mediator,
        ILogger<SMSInvestigationWorkflowService> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region Investigation Lifecycle Management

    /// <summary>
    /// Create a new investigation for a hazard
    /// </summary>
    public async Task<Result<Investigation>> CreateInvestigationAsync(
        string hazardCode, 
        string assignedInvestigatorId, 
        string investigationNotes,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Creating investigation for hazard {HazardCode}", hazardCode);

            // Validate inputs
            if (string.IsNullOrWhiteSpace(hazardCode))
            {
                return Result<Investigation>.Failure<Investigation>(DomainErrors.InvestigationError.InvalidCode);
            }

            if (string.IsNullOrWhiteSpace(assignedInvestigatorId))
            {
                return Result<Investigation>.Failure<Investigation>(DomainErrors.InvestigationError.InvalidInvestigator);
            }

            // Generate investigation code
            var investigationCode = $"IN-0000";
            var investigationId = new InvestigationID(investigationCode);

            // Create Investigation domain entity with proper HazardCode
            var investigation = new Investigation(investigationId)
            {
                Code = investigationCode,
                HazardCode = hazardCode, // This is the critical field that cannot be null
                AssignedInvestigatorId = assignedInvestigatorId,
                InvestigationNotes = investigationNotes ?? string.Empty,
                Status = "Assigned"
            };

            _logger.LogInformation("Creating investigation with HazardCode: {HazardCode}, AssignedTo: {Investigator}", 
                hazardCode, assignedInvestigatorId);

            // Use CQRS to create investigation
            var command = new CreateInvestigationCommand(investigation);
            var result = await _mediator.SendAsync(command, cancellationToken);

            if (result.IsFailure)
            {
                _logger.LogError("Failed to create investigation for hazard {HazardCode}: {Error}", 
                    hazardCode, result.Error.Message);
                return result;
            }

            _logger.LogInformation("Successfully created investigation {InvestigationCode} for hazard {HazardCode}", 
                investigationCode, hazardCode);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating investigation for hazard {HazardCode}", hazardCode);
            return Result<Investigation>.Failure<Investigation>(DomainErrors.InvestigationError.CreateFailed);
        }
    }

    /// <summary>
    /// Get investigation by ID
    /// </summary>
    public async Task<Result<Investigation>> GetInvestigationAsync(string investigationId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Retrieving investigation {InvestigationId}", investigationId);

            var id = new InvestigationID(investigationId);
            var query = new GetInvestigationByIdQuery(id);
            var result = await _mediator.SendAsync(query, cancellationToken);

            if (result.IsFailure)
            {
                _logger.LogWarning("Investigation {InvestigationId} not found", investigationId);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving investigation {InvestigationId}", investigationId);
            return Result<Investigation>.Failure<Investigation>(DomainErrors.InvestigationError.NotFound);
        }
    }

    /// <summary>
    /// Get investigation by hazard code
    /// </summary>
    public async Task<Result<Investigation>> GetInvestigationByHazardCodeAsync(string hazardCode, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Retrieving investigation for hazard {HazardCode}", hazardCode);

            // Get all investigations and filter by hazard code
            var query = new GetAllInvestigationsQuery();
            var result = await _mediator.SendAsync(query, cancellationToken);

            if (result.IsFailure)
            {
                return Result<Investigation>.Failure<Investigation>(result.Error);
            }

            var investigation = result.Value.FirstOrDefault(i => i.ReportCode == hazardCode);
            if (investigation == null)
            {
                return Result<Investigation>.Failure<Investigation>(DomainErrors.InvestigationError.NotFound);
            }

            return Result<Investigation>.Success(investigation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving investigation for hazard {HazardCode}", hazardCode);
            return Result<Investigation>.Failure<Investigation>(DomainErrors.InvestigationError.NotFound);
        }
    }

    /// <summary>
    /// Update investigation notes
    /// </summary>
    public async Task<Result<Investigation>> UpdateInvestigationNotesAsync(
        string investigationId, 
        string investigationNotes,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Updating investigation notes for {InvestigationId}", investigationId);

            // Get existing investigation
            var investigationResult = await GetInvestigationAsync(investigationId, cancellationToken);
            if (investigationResult.IsFailure)
            {
                return investigationResult;
            }

            var investigation = investigationResult.Value;
            investigation.InvestigationNotes = investigationNotes ?? string.Empty;

            // Update via CQRS
            var command = new UpdateInvestigationCommand(investigation);
            var result = await _mediator.SendAsync(command, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully updated investigation notes for {InvestigationId}", investigationId);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating investigation notes for {InvestigationId}", investigationId);
            return Result<Investigation>.Failure<Investigation>(DomainErrors.InvestigationError.UpdateFailed);
        }
    }

    /// <summary>
    /// Complete investigation and return decision
    /// </summary>
    public async Task<Result<Investigation>> CompleteInvestigationAsync(
        string investigationId,
        InvestigationDecisionType decisionType,
        string decisionRationale,
        string decisionMaker,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Completing investigation {InvestigationId} with decision {DecisionType}", 
                investigationId, decisionType);

            // Validate decision inputs
            if (string.IsNullOrWhiteSpace(decisionRationale))
            {
                return Result<Investigation>.Failure<Investigation>(DomainErrors.InvestigationError.MissingRecommendations);
            }

            if (string.IsNullOrWhiteSpace(decisionMaker))
            {
                return Result<Investigation>.Failure<Investigation>(DomainErrors.InvestigationError.InvalidInvestigator);
            }

            // Get existing investigation
            var investigationResult = await GetInvestigationAsync(investigationId, cancellationToken);
            if (investigationResult.IsFailure)
            {
                return investigationResult;
            }

            var investigation = investigationResult.Value;
            
            // Update investigation with completion data
            var completionNotes = $"INVESTIGATION COMPLETED - Decision: {decisionType} | " +
                                 $"Rationale: {decisionRationale} | " +
                                 $"Decision Maker: {decisionMaker} | " +
                                 $"Completion Date: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss UTC}";

            investigation.InvestigationNotes = string.IsNullOrEmpty(investigation.InvestigationNotes) 
                ? completionNotes 
                : $"{investigation.InvestigationNotes}\n\n{completionNotes}";

            // Update via CQRS
            var command = new UpdateInvestigationCommand(investigation);
            var result = await _mediator.SendAsync(command, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully completed investigation {InvestigationId} with decision {DecisionType}", 
                    investigationId, decisionType);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing investigation {InvestigationId}", investigationId);
            return Result<Investigation>.Failure<Investigation>(DomainErrors.InvestigationError.UpdateFailed);
        }
    }

    /// <summary>
    /// Get all investigations
    /// </summary>
    public async Task<Result<List<Investigation>>> GetAllInvestigationsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Retrieving all investigations");

            var query = new GetAllInvestigationsQuery();
            var result = await _mediator.SendAsync(query, cancellationToken);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all investigations");
            return Result<List<Investigation>>.Failure<List<Investigation>>(DomainErrors.InvestigationError.NotFound);
        }
    }

    /// <summary>
    /// Delete investigation
    /// </summary>
    public async Task<Result<bool>> DeleteInvestigationAsync(string investigationId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Deleting investigation {InvestigationId}", investigationId);

            var id = new InvestigationID(investigationId);
            var command = new DeleteInvestigationCommand(id);
            var result = await _mediator.SendAsync(command, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully deleted investigation {InvestigationId}", investigationId);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting investigation {InvestigationId}", investigationId);
            return Result<bool>.Failure<bool>(DomainErrors.InvestigationError.DeleteFailed);
        }
    }

    /// <summary>
    /// Get available investigators (SMS Application Users with investigation permissions)
    /// </summary>
    //public async Task<Result<List<SMSApplicationUser>>> GetAvailableInvestigatorsAsync(CancellationToken cancellationToken = default)
    //{
    //    try
    //    {
    //        _logger.LogInformation("Retrieving available investigators");

    //        // Get all SMS Application Users who can perform investigations
    //        var query = new GetAllSMSApplicationUsersQuery();
    //        var result = await _mediator.SendAsync(query, cancellationToken);

    //        if (result.IsFailure)
    //        {
    //            return Result<List<SMSApplicationUser>>.Failure<List<SMSApplicationUser>>(result.Error);
    //        }

    //        // Filter for investigators (could be based on permission level or specific role)
    //        var investigators = result.Value
    //            .Where(u => u.PermissionLevel == "Investigator" || 
    //                       u.PermissionLevel == "Manager" || 
    //                       u.PermissionLevel == "Administrator")
    //            .ToList();

    //        _logger.LogInformation("Found {Count} available investigators", investigators.Count);

    //        return Result<List<SMSApplicationUser>>.Success(investigators);
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Error retrieving available investigators");
    //        return Result<List<SMSApplicationUser>>.Failure<List<SMSApplicationUser>>(DomainErrors.SMSApplicationUserError.NotFound);
    //    }
    //}

    #endregion
}

/// <summary>
/// Investigation decision types that determine workflow routing
/// </summary>
public enum InvestigationDecisionType
{
    SMSRisk,                      // Return to validation for risk assessment
    NoSMSRisk,                    // Close as non-SMS risk
    RequiresMoreInvestigation,    // Continue investigation
    ReferExternal                 // Refer to external organization
}