using Microsoft.Extensions.Logging;
using SMS_Domain.Enums;
using SMS_Domain.Errors;
using SMS_Infrastructure.Interfaces;

namespace SMS_Application.Services;

public sealed class SMSOrganizationService
{
    private readonly ISMSOrganizationRepository _organizationRepository;
    private readonly ILogger<SMSOrganizationService> _logger;

    public SMSOrganizationService(
        ISMSOrganizationRepository organizationRepository,
        ILogger<SMSOrganizationService> logger)
    {
        _organizationRepository = organizationRepository ?? throw new ArgumentNullException(nameof(organizationRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<IEnumerable<SMSOrganization>>> GetAllSMSOrganizationsAsync(CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Retrieving all SMS organizations", ApplicationEventIds.Information);
            return await _organizationRepository.GetAllAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error retrieving SMS organizations", ApplicationEventIds.Error);
            return Result<IEnumerable<SMSOrganization>>.Failure<IEnumerable<SMSOrganization>>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }

    public async Task<Result<SMSOrganization>> GetSMSOrganizationByCodeAsync(string code, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Retrieving SMS organization with code: {Code}", ApplicationEventIds.Information, code);
            return await _organizationRepository.GetByCodeAsync(code, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error retrieving SMS organization with code: {Code}", ApplicationEventIds.Error, code);
            return Result<SMSOrganization>.Failure<SMSOrganization>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }

    public async Task<Result<SMSOrganization>> CreateSMSOrganizationAsync(SMSOrganization organization, string createdBy, CancellationToken ct = default)
    {
        try
        {
            if (organization is null)
            {
                _logger.LogApplicationWarning("CreateSMSOrganizationAsync received null organization", ApplicationEventIds.Warning);
                return Result<SMSOrganization>.Failure<SMSOrganization>(DomainErrors.SMSDepartmentError.NullOrEmpty);
            }

            _logger.LogApplicationInformation("Creating SMS organization with code: {Code}", ApplicationEventIds.Information, organization.Value);
            return await _organizationRepository.CreateAsync(organization, createdBy, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error creating SMS organization", ApplicationEventIds.Error);
            return Result<SMSOrganization>.Failure<SMSOrganization>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }

    public async Task<Result<SMSOrganization>> UpdateSMSOrganizationAsync(string originalCode, SMSOrganization organization, string updatedBy, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(originalCode) || organization is null)
            {
                _logger.LogApplicationWarning("UpdateSMSOrganizationAsync received invalid input", ApplicationEventIds.Warning);
                return Result<SMSOrganization>.Failure<SMSOrganization>(DomainErrors.SMSDepartmentError.InvalidDepartment);
            }

            _logger.LogApplicationInformation("Updating SMS organization with code: {Code}", ApplicationEventIds.Information, originalCode);
            return await _organizationRepository.UpdateAsync(originalCode, organization, updatedBy, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error updating SMS organization with code: {Code}", ApplicationEventIds.Error, originalCode);
            return Result<SMSOrganization>.Failure<SMSOrganization>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }

    public async Task<Result<bool>> DeleteSMSOrganizationAsync(string code, string deletedBy, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                _logger.LogApplicationWarning("DeleteSMSOrganizationAsync received empty code", ApplicationEventIds.Warning);
                return Result<bool>.Failure<bool>(DomainErrors.SMSDepartmentError.InvalidDepartment);
            }

            _logger.LogApplicationInformation("Deleting SMS organization with code: {Code}", ApplicationEventIds.Information, code);
            return await _organizationRepository.DeleteAsync(code, deletedBy, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error deleting SMS organization with code: {Code}", ApplicationEventIds.Error, code);
            return Result<bool>.Failure<bool>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }
}
