using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBT3_Application.Messaging;

public class UpdateTrainingSessionCommand : BaseCommandBundle, IRequest<TrainingSession>
{
    public UpdateTrainingSessionCommand(TrainingSession session)
    {
        TrainingSession = session;
    }
    public TrainingSession TrainingSession { get; init; }

}
