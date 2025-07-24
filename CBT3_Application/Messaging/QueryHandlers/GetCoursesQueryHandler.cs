namespace CBT3_Application.Messaging;


    public class GetCoursesQueryHandler : BaseQueryBundle, IRequestHandler<GetCoursesQuery, Result<List<Course>>>
    {
        private TrainingService _trainingService;
        public GetCoursesQueryHandler(TrainingService trainingService)
        {
            _trainingService = trainingService;
        }
        public Task<Result<List<Course>>> HandleAsync(GetCoursesQuery request, CancellationToken ct = default)
        {
            Result<List<Course>> list = Task.Run(() => _trainingService.GetCoursesAsync(request.TraineeId,ct)).Result;
            return Task.FromResult(list);
        }
    }


