//-----------------------------------------------------------------------
// <copyright file="InterviewQueries.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Query definitions for read operations in the SMS CQRS architecture.
//                  Defines query objects for read operations in the CQRS pattern.
//                  Queries retrieve data without causing side effects.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Application.Messaging.Queries;

// =============================================
// INTERVIEW QUERIES
// =============================================

public class GetInterviewByCodeQuery : BaseQueryBundle, IRequest<Result<Interview>>
{
    public InterviewID InterviewId { get; set; }

    public GetInterviewByCodeQuery(InterviewID interviewId)
    {
        InterviewId = interviewId ?? throw new ArgumentNullException(nameof(interviewId));
    }
}

public class GetAllInterviewsQuery : BaseQueryBundle, IRequest<Result<List<Interview>>>
{
    public GetAllInterviewsQuery()
    {
    }
}
