using System;
using System.Linq;

namespace CBT3_Application.Messaging;

public class FinishTrainingSessionCommand : BaseCommandBundle, IRequest<TrainingSession>
{
    public FinishTrainingSessionCommand(TrainingSession session)
    {
        TrainingSession = session;
    }
    public TrainingSession TrainingSession { get; init; }

}