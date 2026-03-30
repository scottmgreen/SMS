//-----------------------------------------------------------------------
// <copyright file="InterviewCommands.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Command definitions for write operations in the SMS CQRS architecture.
//                  Defines command objects for write operations in the CQRS pattern.
//                  Commands represent business intentions and trigger state changes.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;

namespace SMS_Application.Messaging.Commands;

public class CreateInterviewCommand : BaseCommandBundle, IRequest<Result<Interview>>, ICreateCommand
{
    public Interview Interview { get; set; }

    public CreateInterviewCommand(Interview interview)
    {
        Interview = interview ?? throw new ArgumentNullException(nameof(interview));
    }

    public void SetCreatedBy(string userId, DateTime timestamp)
    {
        Interview.CreatedBy = userId;
        Interview.CreatedDate = timestamp;
    }

    public void SetUpdatedBy(string userId, DateTime timestamp)
    {
        // For create commands, we typically don't set UpdatedBy
    }
}

public class UpdateInterviewCommand : BaseCommandBundle, IRequest<Result<Interview>>, IUpdateCommand
{
    public Interview Interview { get; set; }

    public UpdateInterviewCommand(Interview interview)
    {
        Interview = interview ?? throw new ArgumentNullException(nameof(interview));
    }

    public void SetCreatedBy(string userId, DateTime timestamp)
    {
        // For update commands, we typically don't modify CreatedBy
    }

    public void SetUpdatedBy(string userId, DateTime timestamp)
    {
        Interview.UpdatedBy = userId;
        Interview.UpdatedDate = timestamp;
    }
}

public class DeleteInterviewCommand : BaseCommandBundle, IRequest<Result<bool>>, IDeleteCommand
{
    public InterviewID InterviewId { get; set; }
    public string DeletedBy { get; set; } = string.Empty;

    public DeleteInterviewCommand(InterviewID interviewId)
    {
        InterviewId = interviewId ?? throw new ArgumentNullException(nameof(interviewId));
    }

    public void SetDeletedBy(string userId, DateTime timestamp)
    {
        DeletedBy = userId;
    }
}
