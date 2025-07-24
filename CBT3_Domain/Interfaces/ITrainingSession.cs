namespace CBT3_Domain.Interfaces;


public interface ITrainingSession
{
    /// <summary>
    /// [fldv_TrainingSessionID]
      //,[fldv_HostName]
      //,[fldv_TraineeID]
      //,[fldv_CourseID]
      //,[fldd_CourseStartedAt]
      //,[fldd_CourseEndedAt]
    /// </summary>
    public string HostName { get; set; }
    public TraineeID TraineeID { get; set; }
    public CourseID CourseID { get; set; }
    public DateTime CourseStartedAt { get; set; }
    public DateTime CourseEndedAt { get; set; }
    public TrainingStatus TrainingStatus { get; set; }
}
