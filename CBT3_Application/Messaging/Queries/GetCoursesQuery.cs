namespace CBT3_Application.Messaging;


    //public class GetCoursesQuery() : BaseQueryBundle, IRequest<Result<List<Course>>>
    //{

    //}

public class GetCoursesQuery : BaseQueryBundle, IRequest<Result<List<Course>>>
{
    public TraineeID TraineeId { get; set; }
    public GetCoursesQuery(TraineeID traineeId)
    {
        TraineeId = traineeId;
    }


}
