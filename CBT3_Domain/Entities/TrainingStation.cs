
namespace CBT3_Domain.Entities;

public class TrainingStation : ITrainingStation
{
    public string HostName { get; set; }
    public TrainingStationStatus Status { get; set; }
    public TrainingSessionID? SessionID { get; set; }
    public TraineeID? TraineeID { get; set; }
    public CourseID? CourseID { get; set; }
    public DateTime? CourseStartedAt { get; set; }
    public DateTime? CourseEndedAt { get; set; }
    public string? CircuitId { get; set; }
}
