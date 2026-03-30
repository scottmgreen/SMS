//-----------------------------------------------------------------------
// <copyright file="InterviewCommandHandlers.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Command handlers implementing SMS interview management business logic and operations.
//                  Implements command handlers for processing write operations.
//                  Handles business logic execution and domain entity coordination.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;

using Microsoft.Extensions.Logging;

namespace SMS_Application.Messaging.CommandHandlers;

// =============================================
// INTERVIEW COMMAND HANDLERS - Clean Architecture Pattern
// =============================================

public class CreateInterviewCommandHandler : BaseCommandBundle, IRequestHandler<CreateInterviewCommand, Result<Interview>>
{
    private readonly IInterviewService _interviewService;
    private readonly ILogger<CreateInterviewCommandHandler> _logger;

    public CreateInterviewCommandHandler(IInterviewService interviewService, ILogger<CreateInterviewCommandHandler> logger)
    {
        _interviewService = interviewService ?? throw new ArgumentNullException(nameof(interviewService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<Interview>> HandleAsync(CreateInterviewCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request?.Interview is null)
            {
                _logger.LogApplicationError("CreateInterviewCommand received with null request or interview", ApplicationEventIds.Error, null);
                return Result<Interview>.Failure<Interview>(DomainErrors.InterviewError.NullOrEmpty);
            }

            _logger.LogInformation("✅ Clean Architecture: Processing CreateInterviewCommand for Code: {Code}", request.Interview.Code);

            var result = await _interviewService.CreateInterviewAsync(request.Interview, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("✅ Clean Architecture: Successfully created Interview with ID: {Id}, Code: {Code}",
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
    private readonly IInterviewService _interviewService;
    private readonly ILogger<UpdateInterviewCommandHandler> _logger;

    public UpdateInterviewCommandHandler(IInterviewService interviewService, ILogger<UpdateInterviewCommandHandler> logger)
    {
        _interviewService = interviewService ?? throw new ArgumentNullException(nameof(interviewService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<Interview>> HandleAsync(UpdateInterviewCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request?.Interview is null)
            {
                _logger.LogApplicationError("UpdateInterviewCommand received with null request or interview", ApplicationEventIds.Error, null);
                return Result<Interview>.Failure<Interview>(DomainErrors.InterviewError.NullOrEmpty);
            }

            _logger.LogInformation("✅ Clean Architecture: Processing UpdateInterviewCommand for ID: {Id}", request.Interview.Id);

            var result = await _interviewService.UpdateInterviewAsync(request.Interview, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("✅ Clean Architecture: Successfully updated Interview with ID: {Id}", request.Interview.Id);
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
    private readonly IInterviewService _interviewService;
    private readonly ILogger<DeleteInterviewCommandHandler> _logger;

    public DeleteInterviewCommandHandler(IInterviewService interviewService, ILogger<DeleteInterviewCommandHandler> logger)
    {
        _interviewService = interviewService ?? throw new ArgumentNullException(nameof(interviewService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> HandleAsync(DeleteInterviewCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request is null)
            {
                _logger.LogApplicationError("DeleteInterviewCommand received with null request", ApplicationEventIds.Error, null);
                return Result<bool>.Failure<bool>(DomainErrors.InterviewError.NullOrEmpty);
            }

            _logger.LogInformation("✅ Clean Architecture: Processing DeleteInterviewCommand for ID: {Id}", request.InterviewId);

            var result = await _interviewService.DeleteInterviewAsync(request.InterviewId, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("✅ Clean Architecture: Successfully deleted Interview with ID: {Id}", request.InterviewId);
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
