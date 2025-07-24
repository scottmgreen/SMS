

namespace CBT3_Domain.Entities;

public sealed class TrainingSession : BaseAuditableEntity
{
    public TrainingSession(TrainingSessionID id) : base(id, "SYSTEM", DateTime.UtcNow) { }
    public string HostName { get; set; }
    public TraineeID? TraineeID { get; set; }
    public CourseID? CourseID { get; set; }
    public DateTime? CourseStartedAt { get; set; }
    public DateTime? CourseEndedAt { get; set; }
}

