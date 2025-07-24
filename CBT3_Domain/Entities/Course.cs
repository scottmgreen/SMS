using System.Collections.Frozen;

namespace CBT3_Domain.Entities;

public sealed class Course : BaseAuditableEntity
{
    public Course(CourseID id) : base(id, "SYSTEM", DateTime.UtcNow) { }
    public string CourseName { get; set; }
    public int ExpiryMonths { get; set; }
    public CourseType CourseType { get; set; }
    public List<Lesson> Lessons { get; set; } = new();
    public bool CustomerServiceRequirement {get;set;}
}
