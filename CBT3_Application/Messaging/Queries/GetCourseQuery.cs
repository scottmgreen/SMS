namespace CBT3_Application.Messaging;


//public partial class QueryBundle
//{
    public class GetCourseQuery : BaseQueryBundle,IRequest<Result<Course>>
    {
        public CourseID CourseId { get; set; }
        public GetCourseQuery(CourseID courseId)
        {
            CourseId = courseId;
        }

        
    }
//}
