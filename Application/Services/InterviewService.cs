//-----------------------------------------------------------------------
// <copyright file="InterviewService.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Application service for SMS interview management.
//                  Provides business logic operations and coordinates domain entities.
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.Extensions.Logging;

namespace SMS_Application.Services;

/// <summary>
/// Application service for Interview management and business operations
/// </summary>
public sealed class InterviewService : IInterviewService
{
    private readonly InterviewDataService _dataService;
    private readonly ILogger<InterviewService> _logger;

    public InterviewService(
        InterviewDataService dataService,
        ILogger<InterviewService> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region IInterviewService Implementation

    public async Task<Result<Interview>> CreateInterviewAsync(Interview interview, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Creating interview with code: {Code}", interview?.Code);
            var result = await _dataService.CreateInterviewAsync(interview, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully created interview with Code: {Code}", result.Value?.Code);
            }
            else
            {
                _logger.LogError("Failed to create interview. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error creating interview");
            return Result<Interview>.Failure<Interview>(DomainErrors.InterviewError.CreateFailed);
        }
    }

    public async Task<Result<Interview>> GetInterviewByCodeAsync(InterviewID code, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Retrieving interview with Code: {Code}", code);
            return await _dataService.GetInterviewByCodeAsync(code, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving interview with Code: {Code}", code);
            return Result<Interview>.Failure<Interview>(DomainErrors.InterviewError.NotFound);
        }
    }

    public async Task<Result<List<Interview>>> GetAllInterviewsAsync(CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Retrieving all interviews");
            return await _dataService.GetAllInterviewsAsync(ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving all interviews");
            return Result<List<Interview>>.Failure<List<Interview>>(DomainErrors.InterviewError.NullOrEmpty);
        }
    }

    public async Task<Result<List<Interview>>> GetInterviewsByInvestigationCodeAsync(InvestigationID investigationCode, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Retrieving interviews for investigation Code: {InvestigationCode}", investigationCode);
            var result = await _dataService.GetByInvestigationAsync(investigationCode.Value, ct).ConfigureAwait(false);
            
            if (result.IsSuccess && result.Value != null)
            {
                var interviewList = result.Value.ToList();
                return Result<List<Interview>>.Success(interviewList);
            }
            
            return Result<List<Interview>>.Failure<List<Interview>>(result.Error);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving interviews for investigation Code: {InvestigationCode}", investigationCode);
            return Result<List<Interview>>.Failure<List<Interview>>(DomainErrors.InterviewError.NotFound);
        }
    }

    public async Task<Result<Interview>> UpdateInterviewAsync(Interview interview, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Updating interview with Code: {Code}", interview?.Code);
            var result = await _dataService.UpdateInterviewAsync(interview, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully updated interview with Code: {Code}", interview?.Code);
            }
            else
            {
                _logger.LogError("Failed to update interview. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error updating interview with Code: {Code}", interview?.Code);
            return Result<Interview>.Failure<Interview>(DomainErrors.InterviewError.UpdateFailed);
        }
    }

    public async Task<Result<bool>> DeleteInterviewAsync(InterviewID code, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Deleting interview with Code: {Code}", code);
            var result = await _dataService.DeleteInterviewAsync(code, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully deleted interview with Code: {Code}", code);
            }
            else
            {
                _logger.LogError("Failed to delete interview. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error deleting interview with Code: {Code}", code);
            return Result<bool>.Failure<bool>(DomainErrors.InterviewError.DeleteFailed);
        }
    }

    public async Task<Result<Interview>> ScheduleInterviewAsync(InterviewID code, DateTime scheduledDateTime, string scheduledBy, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Scheduling interview {Code} for {DateTime} by {ScheduledBy}", code, scheduledDateTime, scheduledBy);
            
            var interviewResult = await _dataService.GetInterviewByCodeAsync(code, ct);
            if (interviewResult.IsFailure || interviewResult.Value == null)
            {
                return Result<Interview>.Failure<Interview>(DomainErrors.InterviewError.NotFound);
            }

            var interview = interviewResult.Value;
            interview.InterviewDate = scheduledDateTime;
            interview.UpdatedBy = scheduledBy;
            interview.UpdatedDate = DateTime.UtcNow;

            var result = await _dataService.UpdateInterviewAsync(interview, ct);
            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully scheduled interview {Code}", code);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error scheduling interview with Code: {Code}", code);
            return Result<Interview>.Failure<Interview>(DomainErrors.InterviewError.UpdateFailed);
        }
    }

    public async Task<Result<Interview>> CompleteInterviewAsync(InterviewID code, string notes, string completedBy, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Completing interview {Code} by {CompletedBy}", code, completedBy);
            
            var interviewResult = await _dataService.GetInterviewByCodeAsync(code, ct);
            if (interviewResult.IsFailure || interviewResult.Value == null)
            {
                return Result<Interview>.Failure<Interview>(DomainErrors.InterviewError.NotFound);
            }

            var interview = interviewResult.Value;
            interview.InvestigatorNotes = notes;
            interview.UpdatedBy = completedBy;
            interview.UpdatedDate = DateTime.UtcNow;
            interview.CompletedDate = DateTime.UtcNow;

            var result = await _dataService.UpdateInterviewAsync(interview, ct);
            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully completed interview {Code}", code);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error completing interview with Code: {Code}", code);
            return Result<Interview>.Failure<Interview>(DomainErrors.InterviewError.UpdateFailed);
        }
    }

    #endregion
}
