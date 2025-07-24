using CBT3_Domain.ValueObjects;

namespace CBT3_Domain.Entities;

public sealed class Trainee : BaseAuditableEntity, ITrainee
{
    public Trainee(TraineeID id) : base(id,"SYSTEM",DateTime.UtcNow) { }
    public FirstName FirstName { get; set; }
    public LastName LastName { get; set; }
    public UPID UPID { get; set; }
    public YearOfBirth YearOfBirth { get; set; }
    public SubscribeToEmailNewsletter SubscribeToEmailNewsletter { get; set; }
    public SubscribeToTextNewsletter SubscribeToTextNewsletter { get; set; }
    public SubscribeToOperationalTexts SubscribeToOperationalTexts { get; set; }
    public override string ToString()
    {
        return $"{FirstName.Value} {LastName.Value} {UPID.Value} {YearOfBirth.Value}";
    }
}
