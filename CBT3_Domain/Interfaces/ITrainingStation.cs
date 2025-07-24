namespace CBT3_Domain.Interfaces;
public interface ITrainingStation
{
    string HostName { get; set; }
    TrainingStationStatus Status { get; set; }
    TrainingSessionID? SessionID { get; set; }
    TraineeID? TraineeID { get; set; }
    CourseID? CourseID { get; set; }
    DateTime? CourseEndedAt { get; set; }
    DateTime? CourseStartedAt { get; set; }
    string? CircuitId { get; set; }

}
