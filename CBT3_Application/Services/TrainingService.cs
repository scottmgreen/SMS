

using CBT3_Application.Interfaces;

using CBT3_Domain.Entities;

namespace CBT3_Application.Services
{
    public class TrainingService
    {
        private readonly IMediator _mediator;
        private readonly TrainingDataService _trainingDataService;
        private readonly CourseDataService _courseDataService;
        public TrainingService(IMediator mediator,TrainingDataService trainingDataService, CourseDataService courseDataService)
        {
            _mediator = mediator;
            _trainingDataService = trainingDataService;
            _courseDataService = courseDataService;
        }
        public Task<Result<bool>> CheckForPreviousTrainingOnDateAsync(Trainee trainee, CourseID courseID, DateTime datetocheck, CancellationToken ct = default)
        {
            return _trainingDataService.CheckForPreviousTrainingOnDateAsync(trainee, courseID, datetocheck,ct);
        }
        
        public Task<Result<bool>> AddTrainingLogEntryAsync(TrainingLogEntry traininglogentry, CancellationToken ct = default)
        {
            return _trainingDataService.AddTrainingLogEntryAsync(traininglogentry, ct);
        }
        public Task<Result<bool>> DeleteTrainingLogEntriesAsync(TraineeID traineeId, CourseID courseId, LessonQuizID lessonquizId, CancellationToken ct = default)
        {
            return _trainingDataService.DeleteTrainingLogEntriesAsync(traineeId, courseId, lessonquizId, ct);
        }
        public Task<Result<bool>> CompleteCourseAsync(TraineeID traineeId, CourseID courseId, bool coursepass, CancellationToken ct = default)
        {
            return _trainingDataService.CompleteCourseAsync(traineeId, courseId, coursepass, ct);
        }

        public Task<Result<List<Course>>> GetCoursesAsync(TraineeID traineeId, CancellationToken ct = default)
        {
            return _courseDataService.GetCoursesAsync(traineeId, ct);
        }

        public Task<Result<Course>> GetCourseByIdAsync(CourseID id, CancellationToken ct = default)
        {
            return _courseDataService.GetCourseByIdAsync(id,ct);
        }
        public Task<Result<Trainee>> GetTraineeByIdAsync(TraineeID id, CancellationToken ct = default)
        {
            return _trainingDataService.GetTraineeByIdAsync(id, ct);
        }
        public Task<Result<List<string>>> GetCourseCodesAsync(Trainee trainee,bool sidaOnly, CancellationToken ct = default)
        {
            TraineeID traineeId = (TraineeID)trainee.Id;
            GetCoursesQuery coursesQuery = new GetCoursesQuery(traineeId);
            Result<List<Course>> courses_result = _mediator.SendAsync(coursesQuery, ct).Result;
            List<Course> courses1courses = new();
            if (sidaOnly)
            {
                courses1courses = (List<Course>)courses_result.Value.Where(c => c.CourseType == CourseType.SIDA).ToList();
            }
            else
            {
                courses1courses = courses_result.Value.ToList();
            }
            
            List<string>  coursecode_list = new List<string>();
            for (int i = 0; i < courses1courses.Count; i++)
            {
                var trainingFound = _trainingDataService.CheckForPreviousTrainingOnDateAsync(trainee, (CourseID)courses1courses[i].Id, DateTime.Now, default).Result;
                if (!trainingFound.Value)
                {
                    coursecode_list.Add(courses1courses[i].Id.Value);
                }
                
                
            }
            return Task.FromResult(Result<List<string>>.Success(coursecode_list));
        }
    }
}
