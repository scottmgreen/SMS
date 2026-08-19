using Microsoft.Extensions.Logging;
using SMS_Domain.Enums;
using SMS_Domain.Errors;
using SMS_Infrastructure.Interfaces;

namespace SMS_Application.Services;

public sealed class SMSCompanyService
{
    private readonly ISMSCompanyRepository _companyRepository;
    private readonly ILogger<SMSCompanyService> _logger;

    public SMSCompanyService(
        ISMSCompanyRepository companyRepository,
        ILogger<SMSCompanyService> logger)
    {
        _companyRepository = companyRepository ?? throw new ArgumentNullException(nameof(companyRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<IEnumerable<SMSCompany>>> GetAllSMSCompaniesAsync(CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Retrieving all SMS companies", ApplicationEventIds.Information);
            return await _companyRepository.GetAllAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error retrieving SMS companies", ApplicationEventIds.Error);
            return Result<IEnumerable<SMSCompany>>.Failure<IEnumerable<SMSCompany>>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }

    public async Task<Result<SMSCompany>> GetSMSCompanyByCodeAsync(string code, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Retrieving SMS company with code: {Code}", ApplicationEventIds.Information, code);
            return await _companyRepository.GetByCodeAsync(code, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error retrieving SMS company with code: {Code}", ApplicationEventIds.Error, code);
            return Result<SMSCompany>.Failure<SMSCompany>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }

    public async Task<Result<SMSCompany>> CreateSMSCompanyAsync(SMSCompany company, string createdBy, CancellationToken ct = default)
    {
        try
        {
            if (company is null)
            {
                _logger.LogApplicationWarning("CreateSMSCompanyAsync received null company", ApplicationEventIds.Warning);
                return Result<SMSCompany>.Failure<SMSCompany>(DomainErrors.SMSDepartmentError.NullOrEmpty);
            }

            _logger.LogApplicationInformation("Creating SMS company with code: {Code}", ApplicationEventIds.Information, company.Value);
            return await _companyRepository.CreateAsync(company, createdBy, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error creating SMS company", ApplicationEventIds.Error);
            return Result<SMSCompany>.Failure<SMSCompany>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }

    public async Task<Result<SMSCompany>> UpdateSMSCompanyAsync(SMSCompany company, string updatedBy, CancellationToken ct = default)
    {
        try
        {
            if (company is null || string.IsNullOrWhiteSpace(company.Value))
            {
                _logger.LogApplicationWarning("UpdateSMSCompanyAsync received invalid company", ApplicationEventIds.Warning);
                return Result<SMSCompany>.Failure<SMSCompany>(DomainErrors.SMSDepartmentError.InvalidDepartment);
            }

            _logger.LogApplicationInformation("Updating SMS company with code: {Code}", ApplicationEventIds.Information, company.Value);
            return await _companyRepository.UpdateAsync(company, updatedBy, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error updating SMS company with code: {Code}", ApplicationEventIds.Error, company?.Value);
            return Result<SMSCompany>.Failure<SMSCompany>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }

    public async Task<Result<bool>> DeleteSMSCompanyAsync(string code, string deletedBy, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                _logger.LogApplicationWarning("DeleteSMSCompanyAsync received empty code", ApplicationEventIds.Warning);
                return Result<bool>.Failure<bool>(DomainErrors.SMSDepartmentError.InvalidDepartment);
            }

            _logger.LogApplicationInformation("Deleting SMS company with code: {Code}", ApplicationEventIds.Information, code);
            return await _companyRepository.DeleteAsync(code, deletedBy, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error deleting SMS company with code: {Code}", ApplicationEventIds.Error, code);
            return Result<bool>.Failure<bool>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }
}
