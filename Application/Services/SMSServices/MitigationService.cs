//-----------------------------------------------------------------------
// <copyright file="MitigationService.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Application service for SMS mitigation management.
//                  Provides business logic operations and coordinates domain entities.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;
using Microsoft.Extensions.Logging;
using SMS_Application.Interfaces;
using SMS_Application.Queries;
using SMS_Application.Commands;
using SMS_Domain.ValueObjects;
using SMS_Domain.Enums;

namespace SMS_Application.Services;

/// <summary>
/// Application service for Mitigation management and business operations
/// </summary>
public sealed class MitigationService : IMitigationService
{
    private readonly MitigationDataService _dataService;
    private readonly IBaseMediator _mediator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<MitigationService> _logger;

    public MitigationService(
        MitigationDataService dataService,
        IBaseMediator mediator,
        ICurrentUserService currentUserService,
        ILogger<MitigationService> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region IMitigationService Implementation

    public async Task<Result<Mitigation>> CreateMitigationAsync(Mitigation mitigation, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Creating mitigation with code: {Code}", mitigation?.Code);
            
            // Set default status for new mitigations
            if (mitigation != null)
            {
                mitigation.Status = MitigationStatus.PendingApproval;
            }

            var result = await _dataService.CreateMitigationAsync(mitigation, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation("Successfully created mitigation with Code: {Code}", result.Value?.Code);
            }
            else
            {
                _logger.LogApplicationError("Failed to create mitigation. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error creating mitigation");
            return Result<Mitigation>.Failure<Mitigation>(DomainErrors.MitigationError.CreateFailed);
        }
    }

    public async Task<Result<Mitigation>> GetMitigationByCodeAsync(MitigationID code, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Retrieving mitigation with Code: {Code}", code);
            return await _dataService.GetMitigationByCodeAsync(code, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error retrieving mitigation with Code: {Code}", code);
            return Result<Mitigation>.Failure<Mitigation>(DomainErrors.MitigationError.NotFound);
        }
    }

    public async Task<Result<List<Mitigation>>> GetAllMitigationsAsync(CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Retrieving all mitigations");
            return await _dataService.GetAllMitigationsAsync(ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error retrieving all mitigations");
            return Result<List<Mitigation>>.Failure<List<Mitigation>>(DomainErrors.MitigationError.NullOrEmpty);
        }
    }

    public async Task<Result<List<Mitigation>>> GetMitigationsByHazardCodeAsync(string hazardCode, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Retrieving mitigations for hazard code: {HazardCode}", hazardCode);
            return await _dataService.GetMitigationsByHazardCodeAsync(hazardCode, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error retrieving mitigations for hazard code: {HazardCode}", hazardCode);
            return Result<List<Mitigation>>.Failure<List<Mitigation>>(DomainErrors.MitigationError.NotFound);
        }
    }

    public async Task<Result<List<Mitigation>>> GetMitigationsByStatusAsync(MitigationStatus status, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Retrieving mitigations with status: {Status}", status);
            
            // Since the DataService doesn't have this method, get all mitigations and filter by status
            var allMitigationsResult = await _dataService.GetAllMitigationsAsync(ct).ConfigureAwait(false);
            
            if (allMitigationsResult.IsFailure)
            {
                return Result<List<Mitigation>>.Failure<List<Mitigation>>(allMitigationsResult.Error);
            }

            var filteredMitigations = allMitigationsResult.Value?
                .Where(m => m.Status.Equals(status))
                .ToList() ?? new List<Mitigation>();

            return Result<List<Mitigation>>.Success(filteredMitigations);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error retrieving mitigations with status: {Status}", status);
            return Result<List<Mitigation>>.Failure<List<Mitigation>>(DomainErrors.MitigationError.NotFound);
        }
    }

    public async Task<Result<Mitigation>> UpdateMitigationAsync(Mitigation mitigation, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Updating mitigation with Code: {Code}", mitigation?.Code);

            if (mitigation is not null)
            {
                var isOverdue = mitigation.TargetDate.HasValue && mitigation.TargetDate.Value.Date <= DateTime.UtcNow.Date;

                if (mitigation.Progress >= 100)
                {
                    mitigation.Status = MitigationStatus.Complete;
                }
                else if (isOverdue && mitigation.Progress < 100)
                {
                    mitigation.Status = MitigationStatus.PastExpectedTargetDate;
                }
                else if (mitigation.Progress > 0)
                {
                    mitigation.Status = MitigationStatus.InProgress;
                }
                else if (!isOverdue && mitigation.Status == MitigationStatus.PastExpectedTargetDate)
                {
                    mitigation.Status = MitigationStatus.Approved;
                }
            }

            var result = await _dataService.UpdateMitigationAsync(mitigation, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation("Successfully updated mitigation with Code: {Code}", mitigation?.Code);

                if (mitigation is not null && mitigation.Progress >= 100 && !string.IsNullOrWhiteSpace(mitigation.HazardCode))
                {
                    await UpdateReportStatusForCompletedMitigationAsync(mitigation.HazardCode.Trim(), ct).ConfigureAwait(false);
                }
            }
            else
            {
                _logger.LogApplicationError("Failed to update mitigation. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error updating mitigation with Code: {Code}", mitigation?.Code);
            return Result<Mitigation>.Failure<Mitigation>(DomainErrors.MitigationError.UpdateFailed);
        }
    }

    private async Task UpdateReportStatusForCompletedMitigationAsync(string hazardCode, CancellationToken ct)
    {
        try
        {
            var hazardResult = await _mediator.SendAsync(new GetHazardByCodeQuery(new HazardID(hazardCode)), ct).ConfigureAwait(false);
            if (hazardResult.IsFailure || hazardResult.Value is null || string.IsNullOrWhiteSpace(hazardResult.Value.ReportCode))
            {
                _logger.LogApplicationWarning("Could not resolve report for completed mitigation hazard: {HazardCode}", hazardCode);
                return;
            }

            var reportCode = hazardResult.Value.ReportCode;
            var reportResult = await _mediator.SendAsync(new GetReportByCodeQuery(new ReportID(reportCode)), ct).ConfigureAwait(false);
            if (reportResult.IsFailure || reportResult.Value is null)
            {
                _logger.LogApplicationWarning("Could not load report {ReportCode} for mitigation completion update", reportCode);
                return;
            }

            if (reportResult.Value.Status == ReportStatus.MitigationComplete)
            {
                return;
            }

            var updatedBy = string.IsNullOrWhiteSpace(_currentUserService.UserCode)
                ? _currentUserService.UserDisplayName
                : _currentUserService.UserCode;

            var statusResult = await _mediator.SendAsync(new UpdateReportStatusCommand(reportCode, ReportStatus.MitigationComplete, updatedBy),ct).ConfigureAwait(false);

            if (statusResult.IsSuccess)
            {
                _logger.LogApplicationInformation("Updated report {ReportCode} status to {Status} after mitigation completion", reportCode, ReportStatus.MitigationComplete.Value);
            }
            else
            {
                _logger.LogApplicationWarning("Failed to update report {ReportCode} status to mitigation complete. Error: {Error}", reportCode, statusResult.Error?.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogApplicationWarning(ex, "Error updating report status from mitigation completion for hazard {HazardCode}", hazardCode);
        }
    }

    public async Task<Result<bool>> DeleteMitigationAsync(MitigationID code, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Deleting mitigation with Code: {Code}", code);
            var result = await _dataService.DeleteMitigationAsync(code, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation("Successfully deleted mitigation with Code: {Code}", code);
            }
            else
            {
                _logger.LogApplicationError("Failed to delete mitigation. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error deleting mitigation with Code: {Code}", code);
            return Result<bool>.Failure<bool>(DomainErrors.MitigationError.DeleteFailed);
        }
    }

    public async Task<Result<Mitigation>> ApproveMitigationAsync(MitigationID code, string approvedBy, string approvalNotes, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Approving mitigation {Code} by {ApprovedBy}", code, approvedBy);
            
            var mitigationResult = await _dataService.GetMitigationByCodeAsync(code, ct);
            if (mitigationResult.IsFailure || mitigationResult.Value == null)
            {
                return Result<Mitigation>.Failure<Mitigation>(DomainErrors.MitigationError.NotFound);
            }

            var mitigation = mitigationResult.Value;
            mitigation.Status = MitigationStatus.Approved;
            mitigation.ApprovedBy = approvedBy;  // ? FIXED: Set ApprovedBy property
            mitigation.UpdatedBy = approvedBy;
            mitigation.UpdatedDate = DateTime.UtcNow;
            // Note: Add approval notes to existing notes or use a specific approval notes field if available

            var result = await _dataService.UpdateMitigationAsync(mitigation, ct);
            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation("Successfully approved mitigation {Code}", code);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error approving mitigation with Code: {Code}", code);
            return Result<Mitigation>.Failure<Mitigation>(DomainErrors.MitigationError.UpdateFailed);
        }
    }

    public async Task<Result<Mitigation>> ImplementMitigationAsync(MitigationID code, string implementedBy, DateTime implementationDate, string implementationNotes, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Implementing mitigation {Code} by {ImplementedBy}", code, implementedBy);
            
            var mitigationResult = await _dataService.GetMitigationByCodeAsync(code, ct);
            if (mitigationResult.IsFailure || mitigationResult.Value == null)
            {
                return Result<Mitigation>.Failure<Mitigation>(DomainErrors.MitigationError.NotFound);
            }

            var mitigation = mitigationResult.Value;
            mitigation.Status = MitigationStatus.InProgress;
            mitigation.UpdatedBy = implementedBy;
            mitigation.UpdatedDate = DateTime.UtcNow;
            // Note: Set implementation date and notes if fields are available in the entity

            var result = await _dataService.UpdateMitigationAsync(mitigation, ct);
            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation("Successfully implemented mitigation {Code}", code);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error implementing mitigation with Code: {Code}", code);
            return Result<Mitigation>.Failure<Mitigation>(DomainErrors.MitigationError.UpdateFailed);
        }
    }

    public async Task<Result<Mitigation>> CloseMitigationAsync(MitigationID code, string closedBy, string closureNotes, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Closing mitigation {Code} by {ClosedBy}", code, closedBy);
            
            var mitigationResult = await _dataService.GetMitigationByCodeAsync(code, ct);
            if (mitigationResult.IsFailure || mitigationResult.Value == null)
            {
                return Result<Mitigation>.Failure<Mitigation>(DomainErrors.MitigationError.NotFound);
            }

            var mitigation = mitigationResult.Value;
            mitigation.Status = MitigationStatus.Complete; // Use correct enum value
            mitigation.UpdatedBy = closedBy;
            mitigation.UpdatedDate = DateTime.UtcNow;
            // Note: Set closure notes if field is available in the entity

            var result = await _dataService.UpdateMitigationAsync(mitigation, ct);
            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation("Successfully closed mitigation {Code}", code);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error closing mitigation with Code: {Code}", code);
            return Result<Mitigation>.Failure<Mitigation>(DomainErrors.MitigationError.UpdateFailed);
        }
    }

    #endregion
}

