using Microsoft.Extensions.Logging;
using SMS_Domain.Enums;

namespace SMS_Application.CommandHandlers;

public class CreateSMSJobTitleCommandHandler : BaseCommandBundle, IBaseRequestHandler<CreateSMSJobTitleCommand, Result<SMSJobTitle>>
{
    private readonly SMSJobTitleService _jobTitleService;
    private readonly ILogger<CreateSMSJobTitleCommandHandler> _logger;

    public CreateSMSJobTitleCommandHandler(
        SMSJobTitleService jobTitleService,
        ILogger<CreateSMSJobTitleCommandHandler> logger)
    {
        _jobTitleService = jobTitleService ?? throw new ArgumentNullException(nameof(jobTitleService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<SMSJobTitle>> HandleAsync(CreateSMSJobTitleCommand request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Processing CreateSMSJobTitleCommand for title: {Code}", ApplicationEventIds.Information, request.JobTitle?.Value);

            if (request?.JobTitle is null)
            {
                _logger.LogApplicationWarning("CreateSMSJobTitleCommand received null title", ApplicationEventIds.Warning);
                return Result<SMSJobTitle>.Failure<SMSJobTitle>(DomainErrors.SMSDepartmentError.NullOrEmpty);
            }

            return await _jobTitleService.CreateSMSJobTitleAsync(request.JobTitle, request.CreatedBy, ct).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            _logger.LogApplicationWarning("CreateSMSJobTitleCommand operation was cancelled", ApplicationEventIds.Warning);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Error processing CreateSMSJobTitleCommand", ApplicationEventIds.Error);
            return Result<SMSJobTitle>.Failure<SMSJobTitle>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }
}

public class UpdateSMSJobTitleCommandHandler : BaseCommandBundle, IBaseRequestHandler<UpdateSMSJobTitleCommand, Result<SMSJobTitle>>
{
    private readonly SMSJobTitleService _jobTitleService;
    private readonly ILogger<UpdateSMSJobTitleCommandHandler> _logger;

    public UpdateSMSJobTitleCommandHandler(
        SMSJobTitleService jobTitleService,
        ILogger<UpdateSMSJobTitleCommandHandler> logger)
    {
        _jobTitleService = jobTitleService ?? throw new ArgumentNullException(nameof(jobTitleService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<SMSJobTitle>> HandleAsync(UpdateSMSJobTitleCommand request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Processing UpdateSMSJobTitleCommand for title: {Code}", ApplicationEventIds.Information, request.OriginalCode);

            if (request?.JobTitle is null)
            {
                _logger.LogApplicationWarning("UpdateSMSJobTitleCommand received null title", ApplicationEventIds.Warning);
                return Result<SMSJobTitle>.Failure<SMSJobTitle>(DomainErrors.SMSDepartmentError.NullOrEmpty);
            }

            return await _jobTitleService.UpdateSMSJobTitleAsync(request.OriginalCode, request.JobTitle, request.UpdatedBy, ct).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            _logger.LogApplicationWarning("UpdateSMSJobTitleCommand operation was cancelled", ApplicationEventIds.Warning);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Error processing UpdateSMSJobTitleCommand", ApplicationEventIds.Error);
            return Result<SMSJobTitle>.Failure<SMSJobTitle>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }
}

public class DeleteSMSJobTitleCommandHandler : BaseCommandBundle, IBaseRequestHandler<DeleteSMSJobTitleCommand, Result<bool>>
{
    private readonly SMSJobTitleService _jobTitleService;
    private readonly ILogger<DeleteSMSJobTitleCommandHandler> _logger;

    public DeleteSMSJobTitleCommandHandler(
        SMSJobTitleService jobTitleService,
        ILogger<DeleteSMSJobTitleCommandHandler> logger)
    {
        _jobTitleService = jobTitleService ?? throw new ArgumentNullException(nameof(jobTitleService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> HandleAsync(DeleteSMSJobTitleCommand request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Processing DeleteSMSJobTitleCommand for title: {Code}", ApplicationEventIds.Information, request.JobTitleCode);

            return await _jobTitleService.DeleteSMSJobTitleAsync(request.JobTitleCode, request.DeletedBy, ct).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            _logger.LogApplicationWarning("DeleteSMSJobTitleCommand operation was cancelled", ApplicationEventIds.Warning);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Error processing DeleteSMSJobTitleCommand", ApplicationEventIds.Error);
            return Result<bool>.Failure<bool>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }
}
