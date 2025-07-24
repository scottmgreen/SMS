namespace CBT3_Domain.Interfaces;

public interface ICourse
{
    CourseID Id { get; set; }
    string CourseName { get; set; }
    string CourseType { get; set; }
    int ExpiryMonths { get; set; }
    List<Lesson> Lessons { get; set; }
}