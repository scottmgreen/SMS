using Microsoft.Extensions.Logging;
using SMS_Domain.Enums;
using SMS_Domain.Errors;
using SMS_Infrastructure.Interfaces;

namespace SMS_Application.Services;

public sealed class SMSJobTitleService
{
    private readonly ISMSJobTitleRepository _jobTitleRepository;
    private readonly ILogger<SMSJobTitleService> _logger;

    public SMSJobTitleService(
        ISMSJobTitleRepository jobTitleRepository,
        ILogger<SMSJobTitleService> logger)
    {
        _jobTitleRepository = jobTitleRepository ?? throw new ArgumentNullException(nameof(jobTitleRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<IEnumerable<SMSJobTitle>>> GetAllSMSJobTitlesAsync(CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Retrieving all SMS job titles", ApplicationEventIds.Information);
            return await _jobTitleRepository.GetAllAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error retrieving SMS job titles", ApplicationEventIds.Error);
            return Result<IEnumerable<SMSJobTitle>>.Failure<IEnumerable<SMSJobTitle>>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }

    public async Task<Result<SMSJobTitle>> GetSMSJobTitleByCodeAsync(string code, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Retrieving SMS job title with code: {Code}", ApplicationEventIds.Information, code);
            return await _jobTitleRepository.GetByCodeAsync(code, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error retrieving SMS job title with code: {Code}", ApplicationEventIds.Error, code);
            return Result<SMSJobTitle>.Failure<SMSJobTitle>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }

    public async Task<Result<SMSJobTitle>> CreateSMSJobTitleAsync(SMSJobTitle title, string createdBy, CancellationToken ct = default)
    {
        try
        {
            if (title is null)
            {
                _logger.LogApplicationWarning("CreateSMSJobTitleAsync received null title", ApplicationEventIds.Warning);
                return Result<SMSJobTitle>.Failure<SMSJobTitle>(DomainErrors.SMSDepartmentError.NullOrEmpty);
            }

            _logger.LogApplicationInformation("Creating SMS job title with code: {Code}", ApplicationEventIds.Information, title.Value);
            return await _jobTitleRepository.CreateAsync(title, createdBy, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error creating SMS job title", ApplicationEventIds.Error);
            return Result<SMSJobTitle>.Failure<SMSJobTitle>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }

    public async Task<Result<SMSJobTitle>> UpdateSMSJobTitleAsync(string originalCode, SMSJobTitle title, string updatedBy, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(originalCode) || title is null)
            {
                _logger.LogApplicationWarning("UpdateSMSJobTitleAsync received invalid input", ApplicationEventIds.Warning);
                return Result<SMSJobTitle>.Failure<SMSJobTitle>(DomainErrors.SMSDepartmentError.InvalidDepartment);
            }

            _logger.LogApplicationInformation("Updating SMS job title with code: {Code}", ApplicationEventIds.Information, originalCode);
            return await _jobTitleRepository.UpdateAsync(originalCode, title, updatedBy, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error updating SMS job title with code: {Code}", ApplicationEventIds.Error, originalCode);
            return Result<SMSJobTitle>.Failure<SMSJobTitle>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }

    public async Task<Result<bool>> DeleteSMSJobTitleAsync(string code, string deletedBy, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                _logger.LogApplicationWarning("DeleteSMSJobTitleAsync received empty code", ApplicationEventIds.Warning);
                return Result<bool>.Failure<bool>(DomainErrors.SMSDepartmentError.InvalidDepartment);
            }

            _logger.LogApplicationInformation("Deleting SMS job title with code: {Code}", ApplicationEventIds.Information, code);
            return await _jobTitleRepository.DeleteAsync(code, deletedBy, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error deleting SMS job title with code: {Code}", ApplicationEventIds.Error, code);
            return Result<bool>.Failure<bool>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }
}
