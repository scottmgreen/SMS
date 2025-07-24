namespace CBT3_Application.Messaging;

//public partial class QueryBundle
//{
    public class GetCourseQueryHandler : BaseQueryBundle, IRequestHandler<GetCourseQuery, Result<Course>>
    {
        private TrainingService _trainingService;
        public GetCourseQueryHandler(TrainingService trainingService)
        {
        _trainingService = trainingService;
        }
        public Task<Result<Course>> HandleAsync(GetCourseQuery request, CancellationToken ct = default)
        {
            CourseID courseid = request.CourseId;

            Result<Course> course = Task.Run(() => _trainingService.GetCourseByIdAsync(courseid,ct)).Result;

            return Task.FromResult(course);
        }
    }
//}
