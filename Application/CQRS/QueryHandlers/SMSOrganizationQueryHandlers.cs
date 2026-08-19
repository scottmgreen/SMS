using Microsoft.Extensions.Logging;
using SMS_Application.Queries;
using SMS_Domain.Enums;

namespace SMS_Application.QueryHandlers;

public class GetAllSMSOrganizationsQueryHandler : BaseQueryBundle, IBaseRequestHandler<GetAllSMSOrganizationsQuery, Result<IEnumerable<SMSOrganization>>>
{
    private readonly SMSOrganizationService _organizationService;
    private readonly ILogger<GetAllSMSOrganizationsQueryHandler> _logger;

    public GetAllSMSOrganizationsQueryHandler(
        SMSOrganizationService organizationService,
        ILogger<GetAllSMSOrganizationsQueryHandler> logger)
    {
        _organizationService = organizationService ?? throw new ArgumentNullException(nameof(organizationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<IEnumerable<SMSOrganization>>> HandleAsync(GetAllSMSOrganizationsQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Processing GetAllSMSOrganizationsQuery", ApplicationEventIds.Information);
            return await _organizationService.GetAllSMSOrganizationsAsync(ct).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            _logger.LogApplicationWarning("GetAllSMSOrganizationsQuery operation was cancelled", ApplicationEventIds.Warning);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Error processing GetAllSMSOrganizationsQuery", ApplicationEventIds.Error);
            return Result<IEnumerable<SMSOrganization>>.Failure<IEnumerable<SMSOrganization>>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }
}

public class GetSMSOrganizationByCodeQueryHandler : BaseQueryBundle, IBaseRequestHandler<GetSMSOrganizationByCodeQuery, Result<SMSOrganization>>
{
    private readonly SMSOrganizationService _organizationService;
    private readonly ILogger<GetSMSOrganizationByCodeQueryHandler> _logger;

    public GetSMSOrganizationByCodeQueryHandler(
        SMSOrganizationService organizationService,
        ILogger<GetSMSOrganizationByCodeQueryHandler> logger)
    {
        _organizationService = organizationService ?? throw new ArgumentNullException(nameof(organizationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<SMSOrganization>> HandleAsync(GetSMSOrganizationByCodeQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Processing GetSMSOrganizationByCodeQuery for organization: {Code}", ApplicationEventIds.Information, request.OrganizationCode);
            return await _organizationService.GetSMSOrganizationByCodeAsync(request.OrganizationCode, ct).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            _logger.LogApplicationWarning("GetSMSOrganizationByCodeQuery operation was cancelled", ApplicationEventIds.Warning);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Error processing GetSMSOrganizationByCodeQuery", ApplicationEventIds.Error);
            return Result<SMSOrganization>.Failure<SMSOrganization>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }
}
