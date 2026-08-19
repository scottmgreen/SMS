using Microsoft.Extensions.Logging;
using SMS_Domain.Enums;

namespace SMS_Application.CommandHandlers;

public class CreateSMSCompanyCommandHandler : BaseCommandBundle, IBaseRequestHandler<CreateSMSCompanyCommand, Result<SMSCompany>>
{
    private readonly SMSCompanyService _companyService;
    private readonly ILogger<CreateSMSCompanyCommandHandler> _logger;

    public CreateSMSCompanyCommandHandler(
        SMSCompanyService companyService,
        ILogger<CreateSMSCompanyCommandHandler> logger)
    {
        _companyService = companyService ?? throw new ArgumentNullException(nameof(companyService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<SMSCompany>> HandleAsync(CreateSMSCompanyCommand request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Processing CreateSMSCompanyCommand for company: {Code}", ApplicationEventIds.Information, request.Company?.Value);

            if (request?.Company is null)
            {
                _logger.LogApplicationWarning("CreateSMSCompanyCommand received null company", ApplicationEventIds.Warning);
                return Result<SMSCompany>.Failure<SMSCompany>(DomainErrors.SMSDepartmentError.NullOrEmpty);
            }

            return await _companyService.CreateSMSCompanyAsync(request.Company, request.CreatedBy, ct).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            _logger.LogApplicationWarning("CreateSMSCompanyCommand operation was cancelled", ApplicationEventIds.Warning);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Error processing CreateSMSCompanyCommand", ApplicationEventIds.Error);
            return Result<SMSCompany>.Failure<SMSCompany>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }
}

public class UpdateSMSCompanyCommandHandler : BaseCommandBundle, IBaseRequestHandler<UpdateSMSCompanyCommand, Result<SMSCompany>>
{
    private readonly SMSCompanyService _companyService;
    private readonly ILogger<UpdateSMSCompanyCommandHandler> _logger;

    public UpdateSMSCompanyCommandHandler(
        SMSCompanyService companyService,
        ILogger<UpdateSMSCompanyCommandHandler> logger)
    {
        _companyService = companyService ?? throw new ArgumentNullException(nameof(companyService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<SMSCompany>> HandleAsync(UpdateSMSCompanyCommand request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Processing UpdateSMSCompanyCommand for company: {Code}", ApplicationEventIds.Information, request.Company?.Value);

            if (request?.Company is null)
            {
                _logger.LogApplicationWarning("UpdateSMSCompanyCommand received null company", ApplicationEventIds.Warning);
                return Result<SMSCompany>.Failure<SMSCompany>(DomainErrors.SMSDepartmentError.NullOrEmpty);
            }

            return await _companyService.UpdateSMSCompanyAsync(request.Company, request.UpdatedBy, ct).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            _logger.LogApplicationWarning("UpdateSMSCompanyCommand operation was cancelled", ApplicationEventIds.Warning);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Error processing UpdateSMSCompanyCommand", ApplicationEventIds.Error);
            return Result<SMSCompany>.Failure<SMSCompany>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }
}

public class DeleteSMSCompanyCommandHandler : BaseCommandBundle, IBaseRequestHandler<DeleteSMSCompanyCommand, Result<bool>>
{
    private readonly SMSCompanyService _companyService;
    private readonly ILogger<DeleteSMSCompanyCommandHandler> _logger;

    public DeleteSMSCompanyCommandHandler(
        SMSCompanyService companyService,
        ILogger<DeleteSMSCompanyCommandHandler> logger)
    {
        _companyService = companyService ?? throw new ArgumentNullException(nameof(companyService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> HandleAsync(DeleteSMSCompanyCommand request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Processing DeleteSMSCompanyCommand for company: {Code}", ApplicationEventIds.Information, request.CompanyCode);

            return await _companyService.DeleteSMSCompanyAsync(request.CompanyCode, request.DeletedBy, ct).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            _logger.LogApplicationWarning("DeleteSMSCompanyCommand operation was cancelled", ApplicationEventIds.Warning);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Error processing DeleteSMSCompanyCommand", ApplicationEventIds.Error);
            return Result<bool>.Failure<bool>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }
}
