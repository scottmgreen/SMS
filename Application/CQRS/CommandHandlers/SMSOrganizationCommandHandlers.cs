using Microsoft.Extensions.Logging;
using SMS_Domain.Enums;

namespace SMS_Application.CommandHandlers;

public class CreateSMSOrganizationCommandHandler : BaseCommandBundle, IBaseRequestHandler<CreateSMSOrganizationCommand, Result<SMSOrganization>>
{
    private readonly SMSOrganizationService _organizationService;
    private readonly ILogger<CreateSMSOrganizationCommandHandler> _logger;

    public CreateSMSOrganizationCommandHandler(
        SMSOrganizationService organizationService,
        ILogger<CreateSMSOrganizationCommandHandler> logger)
    {
        _organizationService = organizationService ?? throw new ArgumentNullException(nameof(organizationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<SMSOrganization>> HandleAsync(CreateSMSOrganizationCommand request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Processing CreateSMSOrganizationCommand for organization: {Code}", ApplicationEventIds.Information, request.Organization?.Value);

            if (request?.Organization is null)
            {
                _logger.LogApplicationWarning("CreateSMSOrganizationCommand received null organization", ApplicationEventIds.Warning);
                return Result<SMSOrganization>.Failure<SMSOrganization>(DomainErrors.SMSDepartmentError.NullOrEmpty);
            }

            return await _organizationService.CreateSMSOrganizationAsync(request.Organization, request.CreatedBy, ct).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            _logger.LogApplicationWarning("CreateSMSOrganizationCommand operation was cancelled", ApplicationEventIds.Warning);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Error processing CreateSMSOrganizationCommand", ApplicationEventIds.Error);
            return Result<SMSOrganization>.Failure<SMSOrganization>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }
}

public class UpdateSMSOrganizationCommandHandler : BaseCommandBundle, IBaseRequestHandler<UpdateSMSOrganizationCommand, Result<SMSOrganization>>
{
    private readonly SMSOrganizationService _organizationService;
    private readonly ILogger<UpdateSMSOrganizationCommandHandler> _logger;

    public UpdateSMSOrganizationCommandHandler(
        SMSOrganizationService organizationService,
        ILogger<UpdateSMSOrganizationCommandHandler> logger)
    {
        _organizationService = organizationService ?? throw new ArgumentNullException(nameof(organizationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<SMSOrganization>> HandleAsync(UpdateSMSOrganizationCommand request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Processing UpdateSMSOrganizationCommand for organization: {Code}", ApplicationEventIds.Information, request.OriginalCode);

            if (request?.Organization is null)
            {
                _logger.LogApplicationWarning("UpdateSMSOrganizationCommand received null organization", ApplicationEventIds.Warning);
                return Result<SMSOrganization>.Failure<SMSOrganization>(DomainErrors.SMSDepartmentError.NullOrEmpty);
            }

            return await _organizationService.UpdateSMSOrganizationAsync(request.OriginalCode, request.Organization, request.UpdatedBy, ct).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            _logger.LogApplicationWarning("UpdateSMSOrganizationCommand operation was cancelled", ApplicationEventIds.Warning);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Error processing UpdateSMSOrganizationCommand", ApplicationEventIds.Error);
            return Result<SMSOrganization>.Failure<SMSOrganization>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }
}

public class DeleteSMSOrganizationCommandHandler : BaseCommandBundle, IBaseRequestHandler<DeleteSMSOrganizationCommand, Result<bool>>
{
    private readonly SMSOrganizationService _organizationService;
    private readonly ILogger<DeleteSMSOrganizationCommandHandler> _logger;

    public DeleteSMSOrganizationCommandHandler(
        SMSOrganizationService organizationService,
        ILogger<DeleteSMSOrganizationCommandHandler> logger)
    {
        _organizationService = organizationService ?? throw new ArgumentNullException(nameof(organizationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> HandleAsync(DeleteSMSOrganizationCommand request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Processing DeleteSMSOrganizationCommand for organization: {Code}", ApplicationEventIds.Information, request.OrganizationCode);

            return await _organizationService.DeleteSMSOrganizationAsync(request.OrganizationCode, request.DeletedBy, ct).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            _logger.LogApplicationWarning("DeleteSMSOrganizationCommand operation was cancelled", ApplicationEventIds.Warning);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Error processing DeleteSMSOrganizationCommand", ApplicationEventIds.Error);
            return Result<bool>.Failure<bool>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }
}
