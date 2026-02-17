namespace SMS_Domain.Enums;

/// <summary>
/// Interview Type Smart Enumeration
/// Represents the different types of interviews conducted during SMS investigations
/// </summary>
public sealed class InterviewType : BaseEnum<InterviewType>
{
    public static readonly InterviewType Witness = new("WITNESS", nameof(Witness));
    public static readonly InterviewType Expert = new("EXPERT", nameof(Expert));
    public static readonly InterviewType Stakeholder = new("STAKEHOLDER", nameof(Stakeholder));
    public static readonly InterviewType FollowUp = new("FOLLOW_UP", nameof(FollowUp));

    public InterviewType(string value, string name) : base(value, name)
    {
    }

    

   

    

    /// <summary>
    /// Get the recommended minimum duration for this interview type in minutes
    /// </summary>
    public int GetRecommendedMinimumDurationMinutes()
    {
        return Value switch
        {
            "WITNESS" => 30,
            "EXPERT" => 60,
            "STAKEHOLDER" => 45,
            "FOLLOW_UP" => 30,
            _ => 30
        };
    }

    

    

    /// <summary>
    /// Get preparation guidelines for this interview type
    /// </summary>
    public string GetPreparationGuidelines()
    {
        return Value switch
        {
            "WITNESS" => "Review incident timeline, prepare basic questions about what was observed",
            "EXPERT" => "Research technical aspects, prepare specialized questions, consider bringing technical documents",
            "STAKEHOLDER" => "Review organizational policies, prepare questions about impact and systemic issues",
            "FOLLOW_UP" => "Review previous interview notes, prepare clarifying questions",
            _ => "Standard interview preparation recommended"
        };
    }

    

    /// <summary>
    /// Get icon class for this interview type
    /// </summary>
    public string GetIconClass()
    {
        return Value switch
        {
            "WITNESS" => "fas fa-eye",
            "EXPERT" => "fas fa-user-graduate",
            "STAKEHOLDER" => "fas fa-users",
            "FOLLOW_UP" => "fas fa-redo",
            _ => "fas fa-microphone"
        };
    }
}