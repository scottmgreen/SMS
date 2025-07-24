namespace CBT3_Domain.Interfaces
{
    public interface ITrainee
    {
        FirstName FirstName { get; set; }
        LastName LastName { get; set; }
        UPID UPID { get; set; }
        YearOfBirth YearOfBirth { get; set; }

        string ToString();
    }
}