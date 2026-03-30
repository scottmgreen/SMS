//-----------------------------------------------------------------------
// <copyright file="ScoringPanelCommands.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Command definitions for write operations in the SMS CQRS architecture.
//                  Defines command objects for write operations in the CQRS pattern.
//                  Commands represent business intentions and trigger state changes.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;

namespace SMS_Application.Messaging.Commands;

public class CreateScoringPanelCommand : BaseCommandBundle, IRequest<Result<ScoringPanel>>, ICreateCommand
{
    public ScoringPanel ScoringPanel { get; set; }

    public CreateScoringPanelCommand(ScoringPanel scoringPanel)
    {
        ScoringPanel = scoringPanel ?? throw new ArgumentNullException(nameof(scoringPanel));
    }

    public void SetCreatedBy(string userId, DateTime timestamp)
    {
        ScoringPanel.CreatedBy = userId;
        ScoringPanel.CreatedDate = timestamp;
    }

    public void SetUpdatedBy(string userId, DateTime timestamp)
    {
        // For create commands, we typically don't set UpdatedBy
    }
}

public class UpdateScoringPanelCommand : BaseCommandBundle, IRequest<Result<ScoringPanel>>, IUpdateCommand
{
    public ScoringPanel ScoringPanel { get; set; }

    public UpdateScoringPanelCommand(ScoringPanel scoringPanel)
    {
        ScoringPanel = scoringPanel ?? throw new ArgumentNullException(nameof(scoringPanel));
    }

    public void SetCreatedBy(string userId, DateTime timestamp)
    {
        // For update commands, we typically don't modify CreatedBy
    }

    public void SetUpdatedBy(string userId, DateTime timestamp)
    {
        ScoringPanel.UpdatedBy = userId;
        ScoringPanel.UpdatedDate = timestamp;
    }
}

public class DeleteScoringPanelCommand : BaseCommandBundle, IRequest<Result<bool>>
{
    public ScoringPanelID ScoringPanelId { get; set; }

    public DeleteScoringPanelCommand(ScoringPanelID scoringPanelId)
    {
        ScoringPanelId = scoringPanelId ?? throw new ArgumentNullException(nameof(scoringPanelId));
    }
}
