//-----------------------------------------------------------------------
// <copyright file="InterviewQueryHandlers.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Query handlers implementing data retrieval logic for SMS read operations.
//                  Defines contract for application services ensuring clean architecture
//                  boundaries and dependency inversion compliance.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;

using Microsoft.Extensions.Logging;
using SMS_Application.Interfaces;
using SMS_Application.Messaging.Queries;

namespace SMS_Application.Messaging.QueryHandlers;

// =============================================
// INTERVIEW QUERY HANDLERS - Clean Architecture Pattern
// =============================================

public class GetInterviewByCodeQueryHandler : BaseQueryBundle, IBaseRequestHandler<GetInterviewByCodeQuery, Result<Interview>>
{
    private readonly IInterviewService _interviewService;
    private readonly ILogger<GetInterviewByCodeQueryHandler> _logger;

    public GetInterviewByCodeQueryHandler(IInterviewService interviewService, ILogger<GetInterviewByCodeQueryHandler> logger)
    {
        _interviewService = interviewService ?? throw new ArgumentNullException(nameof(interviewService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<Interview>> HandleAsync(GetInterviewByCodeQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation(" Processing GetInterviewByCodeQuery for Code: {Code}", request.InterviewId);
            var result = await _interviewService.GetInterviewByCodeAsync(new InterviewID(request.InterviewId.Value), ct).ConfigureAwait(false);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing GetInterviewByCodeQuery for Code: {Code}", ApplicationEventIds.Error, ex);
            return Result<Interview>.Failure<Interview>(DomainErrors.InterviewError.NotFound);
        }
    }
}

public class GetAllInterviewsQueryHandler : BaseQueryBundle, IBaseRequestHandler<GetAllInterviewsQuery, Result<List<Interview>>>
{
    private readonly IInterviewService _interviewService;
    private readonly ILogger<GetAllInterviewsQueryHandler> _logger;

    public GetAllInterviewsQueryHandler(IInterviewService interviewService, ILogger<GetAllInterviewsQueryHandler> logger)
    {
        _interviewService = interviewService ?? throw new ArgumentNullException(nameof(interviewService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<List<Interview>>> HandleAsync(GetAllInterviewsQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation(" Processing GetAllInterviewsQuery");
            var result = await _interviewService.GetAllInterviewsAsync(ct).ConfigureAwait(false);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing GetAllInterviewsQuery", ApplicationEventIds.Error, ex);
            return Result<List<Interview>>.Failure<List<Interview>>(DomainErrors.InterviewError.NullOrEmpty);
        }
    }
}
