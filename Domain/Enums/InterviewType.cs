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
    /// Get all interview types that typically require specialized preparation
    /// </summary>
    public static IEnumerable<InterviewType> GetSpecializedTypes()
    {
        return new[] { Expert, Stakeholder };
    }

    /// <summary>
    /// Get all interview types that are typically straightforward
    /// </summary>
    public static IEnumerable<InterviewType> GetStandardTypes()
    {
        return new[] { Witness, FollowUp };
    }

    /// <summary>
    /// Check if this interview type typically requires formal preparation
    /// </summary>
    public bool RequiresFormalPreparation()
    {
        return this == Expert || this == Stakeholder;
    }

    /// <summary>
    /// Check if this interview type is typically time-sensitive
    /// </summary>
    public bool IsTimeSensitive()
    {
        return this == Witness || this == FollowUp;
    }

    /// <summary>
    /// Check if this interview type typically involves external parties
    /// </summary>
    public bool InvolvesExternalParties()
    {
        return this == Stakeholder;
    }

    /// <summary>
    /// Check if this interview type requires special confidentiality considerations
    /// </summary>
    public bool RequiresConfidentialityConsiderations()
    {
        return this == Expert || this == Stakeholder;
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
    /// Get the recommended maximum duration for this interview type in minutes
    /// </summary>
    public int GetRecommendedMaximumDurationMinutes()
    {
        return Value switch
        {
            "WITNESS" => 90,
            "EXPERT" => 180,
            "STAKEHOLDER" => 120,
            "FOLLOW_UP" => 60,
            _ => 90
        };
    }

    /// <summary>
    /// Get display description for this interview type
    /// </summary>
    public string GetDisplayDescription()
    {
        return Value switch
        {
            "WITNESS" => "Interview with a witness to the event or incident",
            "EXPERT" => "Interview with a subject matter expert for technical insights",
            "STAKEHOLDER" => "Interview with an organizational stakeholder for context and impact",
            "FOLLOW_UP" => "Follow-up interview to clarify or expand on previous information",
            _ => Name
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
    /// Get CSS class for type display
    /// </summary>
    public string GetCssClass()
    {
        return Value switch
        {
            "WITNESS" => "interview-type-witness",
            "EXPERT" => "interview-type-expert",
            "STAKEHOLDER" => "interview-type-stakeholder",
            "FOLLOW_UP" => "interview-type-followup",
            _ => "interview-type-default"
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