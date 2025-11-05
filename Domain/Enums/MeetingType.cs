using System.Reflection;
using SMS_Domain.Common;

namespace SMS_Domain.Enums;

/// <summary>
/// Meeting types for different operational needs within SMS committees
/// </summary>
public abstract class MeetingType : BaseEnum<MeetingType>
{
    protected MeetingType(string value, string name, string description, int priority) : base(value, name)
    {
        Description = description;
        Priority = priority;
    }

    public string Description { get; }
    public int Priority { get; }

    #region Meeting Types

    /// <summary>Regular scheduled committee meeting</summary>
    public static readonly MeetingType Regular = new RegularType();

    /// <summary>Special meeting called for urgent matters</summary>
    public static readonly MeetingType Special = new SpecialType();

    /// <summary>Emergency meeting for critical safety issues</summary>
    public static readonly MeetingType Emergency = new EmergencyType();

    /// <summary>Working session for detailed analysis</summary>
    public static readonly MeetingType WorkingSession = new WorkingSessionType();

    /// <summary>Review session for document approval</summary>
    public static readonly MeetingType Review = new ReviewType();

    /// <summary>Training or orientation meeting</summary>
    public static readonly MeetingType Training = new TrainingType();

    #endregion

    #region Implementations

    private sealed class RegularType : MeetingType
    {
        public RegularType() : base("REGULAR", "Regular Meeting",
            "Regularly scheduled committee meeting following standard agenda", 3)
        {
        }
    }

    private sealed class SpecialType : MeetingType
    {
        public SpecialType() : base("SPECIAL", "Special Meeting",
            "Special meeting called to address urgent or specific matters", 7)
        {
        }
    }

    private sealed class EmergencyType : MeetingType
    {
        public EmergencyType() : base("EMERGENCY", "Emergency Meeting",
            "Emergency meeting for critical safety issues requiring immediate attention", 10)
        {
        }
    }

    private sealed class WorkingSessionType : MeetingType
    {
        public WorkingSessionType() : base("WORKING_SESSION", "Working Session",
            "Detailed working session for in-depth analysis and collaboration", 5)
        {
        }
    }

    private sealed class ReviewType : MeetingType
    {
        public ReviewType() : base("REVIEW", "Review Session",
            "Review session for document approval and formal decisions", 6)
        {
        }
    }

    private sealed class TrainingType : MeetingType
    {
        public TrainingType() : base("TRAINING", "Training Meeting",
            "Training or orientation meeting for education and development", 4)
        {
        }
    }

    #endregion

    /// <summary>
    /// Gets all available meeting types
    /// </summary>
    public static IEnumerable<MeetingType> GetAllValues()
    {
        return typeof(MeetingType)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(f => f.FieldType == typeof(MeetingType))
            .Select(f => (MeetingType)f.GetValue(null)!)
            .Where(mt => mt != null);
    }

    /// <summary>
    /// Gets meeting types by priority level
    /// </summary>
    public static IEnumerable<MeetingType> GetMeetingTypesByPriority(int minPriority)
    {
        return GetAllValues().Where(mt => mt.Priority >= minPriority);
    }

    /// <summary>
    /// Checks if this is a high-priority meeting type
    /// </summary>
    public bool IsHighPriority => Priority >= 7;

    /// <summary>
    /// Checks if this is an emergency meeting type
    /// </summary>
    public bool IsEmergency => Priority >= 10;
}