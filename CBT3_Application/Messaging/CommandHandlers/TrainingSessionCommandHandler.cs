using CBT3_Application.Services;

using CBT3_Infrastructure.Interfaces;

namespace CBT3_Application.Messaging;

//public class TrainingSessionCommandHandler : BaseCommandBundle, IRequestHandler<TrainingSessionCommand, ITrainingSession>
//{
//    private readonly SystemService _systemService;

//    public TrainingSessionCommandHandler(SystemService systemService)
//    {
//        _systemService = systemService;

//    }
//    public Task<ITrainingSession> HandleAsync(TrainingSessionCommand request, CancellationToken ct = default)
//    {
//        ITrainingSession session = request.TrainingSession;


//        _systemService.UpdateTrainingSession(session);
//        return Task.FromResult(request.TrainingSession);
//    }
//}

