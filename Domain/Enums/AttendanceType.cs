using System.Reflection;
using SMS_Domain.Common;

namespace SMS_Domain.Enums;

/// <summary>
/// Attendance types for meeting participation tracking
/// </summary>
public abstract class AttendanceType : BaseEnum<AttendanceType>
{
    protected AttendanceType(string value, string name, string description, bool countsAsPresent) : base(value, name)
    {
        Description = description;
        CountsAsPresent = countsAsPresent;
    }

    public string Description { get; }
    public bool CountsAsPresent { get; }

    #region Attendance Types

    /// <summary>Member was present for the meeting</summary>
    public static readonly AttendanceType Present = new PresentType();

    /// <summary>Member was absent from the meeting</summary>
    public static readonly AttendanceType Absent = new AbsentType();

    /// <summary>Member was excused from the meeting</summary>
    public static readonly AttendanceType Excused = new ExcusedType();

    /// <summary>Member attended virtually/remotely</summary>
    public static readonly AttendanceType Virtual = new VirtualType();

    /// <summary>Member attended partially (arrived late or left early)</summary>
    public static readonly AttendanceType Partial = new PartialType();

    #endregion

    #region Implementations

    private sealed class PresentType : AttendanceType
    {
        public PresentType() : base("PRESENT", "Present",
            "Member was physically present for the entire meeting", true)
        {
        }
    }

    private sealed class AbsentType : AttendanceType
    {
        public AbsentType() : base("ABSENT", "Absent",
            "Member did not attend the meeting", false)
        {
        }
    }

    private sealed class ExcusedType : AttendanceType
    {
        public ExcusedType() : base("EXCUSED", "Excused",
            "Member was excused from attending the meeting", false)
        {
        }
    }

    private sealed class VirtualType : AttendanceType
    {
        public VirtualType() : base("VIRTUAL", "Virtual",
            "Member attended the meeting remotely via virtual connection", true)
        {
        }
    }

    private sealed class PartialType : AttendanceType
    {
        public PartialType() : base("PARTIAL", "Partial",
            "Member attended part of the meeting (late arrival or early departure)", true)
        {
        }
    }

    #endregion

    /// <summary>
    /// Gets all available attendance types
    /// </summary>
    public static IEnumerable<AttendanceType> GetAllValues()
    {
        return typeof(AttendanceType)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(f => f.FieldType == typeof(AttendanceType))
            .Select(f => (AttendanceType)f.GetValue(null)!)
            .Where(at => at != null);
    }

    /// <summary>
    /// Gets attendance types that count as present
    /// </summary>
    public static IEnumerable<AttendanceType> GetPresentTypes()
    {
        return GetAllValues().Where(at => at.CountsAsPresent);
    }

    /// <summary>
    /// Checks if this attendance type counts toward quorum
    /// </summary>
    public bool CountsTowardQuorum => CountsAsPresent;

    /// <summary>
    /// Checks if this is an absence
    /// </summary>
    public bool IsAbsence => !CountsAsPresent;
}