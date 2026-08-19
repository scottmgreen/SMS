using Microsoft.Extensions.Logging;
using SMS_Application.Queries;
using SMS_Domain.Enums;

namespace SMS_Application.QueryHandlers;

public class GetAllSMSCompaniesQueryHandler : BaseQueryBundle, IBaseRequestHandler<GetAllSMSCompaniesQuery, Result<IEnumerable<SMSCompany>>>
{
    private readonly SMSCompanyService _companyService;
    private readonly ILogger<GetAllSMSCompaniesQueryHandler> _logger;

    public GetAllSMSCompaniesQueryHandler(
        SMSCompanyService companyService,
        ILogger<GetAllSMSCompaniesQueryHandler> logger)
    {
        _companyService = companyService ?? throw new ArgumentNullException(nameof(companyService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<IEnumerable<SMSCompany>>> HandleAsync(GetAllSMSCompaniesQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Processing GetAllSMSCompaniesQuery", ApplicationEventIds.Information);
            return await _companyService.GetAllSMSCompaniesAsync(ct).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            _logger.LogApplicationWarning("GetAllSMSCompaniesQuery operation was cancelled", ApplicationEventIds.Warning);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Error processing GetAllSMSCompaniesQuery", ApplicationEventIds.Error);
            return Result<IEnumerable<SMSCompany>>.Failure<IEnumerable<SMSCompany>>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }
}

public class GetSMSCompanyByCodeQueryHandler : BaseQueryBundle, IBaseRequestHandler<GetSMSCompanyByCodeQuery, Result<SMSCompany>>
{
    private readonly SMSCompanyService _companyService;
    private readonly ILogger<GetSMSCompanyByCodeQueryHandler> _logger;

    public GetSMSCompanyByCodeQueryHandler(
        SMSCompanyService companyService,
        ILogger<GetSMSCompanyByCodeQueryHandler> logger)
    {
        _companyService = companyService ?? throw new ArgumentNullException(nameof(companyService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<SMSCompany>> HandleAsync(GetSMSCompanyByCodeQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Processing GetSMSCompanyByCodeQuery for company: {Code}", ApplicationEventIds.Information, request.CompanyCode);
            return await _companyService.GetSMSCompanyByCodeAsync(request.CompanyCode, ct).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            _logger.LogApplicationWarning("GetSMSCompanyByCodeQuery operation was cancelled", ApplicationEventIds.Warning);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Error processing GetSMSCompanyByCodeQuery", ApplicationEventIds.Error);
            return Result<SMSCompany>.Failure<SMSCompany>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }
}
