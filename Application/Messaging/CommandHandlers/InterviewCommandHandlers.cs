using Microsoft.Extensions.Logging;
using SMS_Application.Common;
using SMS_Application.Interfaces;
using SMS_Application.Messaging.Commands;
using SMS_Domain.Entities;
using SMS_Domain.ValueObjects;
using SMS_Domain.Errors;
using SMS_Infrastructure.Services;
using SMS_Shared.Common;

namespace SMS_Application.Messaging.CommandHandlers;

// =============================================
// INTERVIEW COMMAND HANDLERS
// =============================================

public class CreateInterviewCommandHandler : BaseCommandBundle, IRequestHandler<CreateInterviewCommand, Result<Interview>>
{
    private readonly InterviewDataService _dataService;
    private readonly ILogger<CreateInterviewCommandHandler> _logger;

    public CreateInterviewCommandHandler(InterviewDataService dataService, ILogger<CreateInterviewCommandHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<Interview>> HandleAsync(CreateInterviewCommand request, CancellationToken ct = default)
    {
        try
        {
            if (request?.Interview is null)
            {
                _logger.LogApplicationError("CreateInterviewCommand received with null Interview", ApplicationEventIds.Error, null);
                return Result<Interview>.Failure<Interview>(DomainErrors.InterviewError.NullOrEmpty);
            }

            _logger.LogInformation("Processing CreateInterviewCommand for Code: {Code}", request.Interview.Code);

            var result = await _dataService.CreateInterviewAsync(request.Interview, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully created Interview with ID: {Id}, Code: {Code}",
                    result.Value?.Id, result.Value?.Code);
            }
            else
            {
                _logger.LogApplicationError("Failed to create Interview with Code: {Code}. Error: {Error}",
                    ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("CreateInterviewCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while creating Interview", ApplicationEventIds.Error, ex);
            return Result<Interview>.Failure<Interview>(DomainErrors.InterviewError.CreateFailed);
        }
    }
}

public class UpdateInterviewCommandHandler : BaseCommandBundle, IRequestHandler<UpdateInterviewCommand, Result<Interview>>
{
    private readonly InterviewDataService _dataService;
    private readonly ILogger<UpdateInterviewCommandHandler> _logger;

    public UpdateInterviewCommandHandler(InterviewDataService dataService, ILogger<UpdateInterviewCommandHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<Interview>> HandleAsync(UpdateInterviewCommand request, CancellationToken ct = default)
    {
        try
        {
            if (request?.Interview is null)
            {
                _logger.LogApplicationError("UpdateInterviewCommand received with null Interview", ApplicationEventIds.Error, null);
                return Result<Interview>.Failure<Interview>(DomainErrors.InterviewError.NullOrEmpty);
            }

            _logger.LogInformation("Processing UpdateInterviewCommand for ID: {Id}, Code: {Code}",
                request.Interview.Id, request.Interview.Code);

            var result = await _dataService.UpdateInterviewAsync(request.Interview, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully updated Interview with ID: {Id}", request.Interview.Id);
            }
            else
            {
                _logger.LogApplicationError("Failed to update Interview with ID: {Id}. Error: {Error}",
                    ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("UpdateInterviewCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while updating Interview with ID: {Id}", ApplicationEventIds.Error, ex);
            return Result<Interview>.Failure<Interview>(DomainErrors.InterviewError.UpdateFailed);
        }
    }
}

public class DeleteInterviewCommandHandler : BaseCommandBundle, IRequestHandler<DeleteInterviewCommand, Result<bool>>
{
    private readonly InterviewDataService _dataService;
    private readonly ILogger<DeleteInterviewCommandHandler> _logger;

    public DeleteInterviewCommandHandler(InterviewDataService dataService, ILogger<DeleteInterviewCommandHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> HandleAsync(DeleteInterviewCommand request, CancellationToken ct = default)
    {
        try
        {
            if (request?.InterviewId is null)
            {
                _logger.LogApplicationError("DeleteInterviewCommand received with null InterviewId", ApplicationEventIds.Error, null);
                return Result<bool>.Failure<bool>(DomainErrors.InterviewError.NullOrEmpty);
            }

            _logger.LogInformation("Processing DeleteInterviewCommand for ID: {Id}", request.InterviewId);

            var result = await _dataService.DeleteInterviewAsync(request.InterviewId, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully deleted Interview with ID: {Id}", request.InterviewId);
            }
            else
            {
                _logger.LogApplicationError("Failed to delete Interview with ID: {Id}. Error: {Error}",
                    ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("DeleteInterviewCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while deleting Interview with ID: {Id}", ApplicationEventIds.Error, ex);
            return Result<bool>.Failure<bool>(DomainErrors.InterviewError.DeleteFailed);
        }
    }
}