namespace CBT3_Domain.Enums;

public abstract class PageType : BaseEnum<PageType>
{
    protected PageType(string value, string name) : base(value, name) { }

    public static readonly PageType PT_BASE_01 = new CourseIntroPageType();
    public static readonly PageType PT_BASE_02 = new LessonOverviewPageType();
    public static readonly PageType PT_BASE_03 = new KnowledgeCheckPageType();
    public static readonly PageType PT_BASE_04 = new CourseEndPageType();
    public static readonly PageType PT_BASE_05 = new VideoPageType();
    public static readonly PageType PT_BASE_06 = new QuizPageType();
    public static readonly PageType PT_BASE_07 = new CoursePassPageType();
    public static readonly PageType PT_BASE_08 = new CourseFailPageType();

    public static readonly PageType PT_CUSTOM_01 = new IncursionPageType();
    public static readonly PageType PT_CUSTOM_02 = new PhoneticAlphabetPageType();
    public static readonly PageType PT_CUSTOM_03 = new MostDangerousLocationsPageType();
    public static readonly PageType PT_CUSTOM_04 = new SignLocationsPageType();
    public static readonly PageType PT_CUSTOM_05 = new ForeignObjectDebrisPageType();
    public static readonly PageType PT_CUSTOM_06 = new VehicleEnteringRestrictedAreaPageType();
    public static readonly PageType PT_CUSTOM_07 = new AircraftReadyToMovePageType();

    public abstract string PageSubType { get; }

    private sealed class CourseIntroPageType : PageType
    {
        public CourseIntroPageType() : base("PT_BASE_01", "COURSE_INTRO")
        {

        }
        public override string PageSubType => "BASE";
    }

    private sealed class LessonOverviewPageType : PageType
    {
        public LessonOverviewPageType() : base("PT_BASE_02", "LESSON_OVERVIEW")
        {
        }

        public override string PageSubType => "BASE";
    }

    private sealed class KnowledgeCheckPageType : PageType
    {
        public KnowledgeCheckPageType() : base("PT_BASE_03", "KNOWLEDGE_CHECK")
        {
        }

        public override string PageSubType => "BASE";
    }

    private sealed class CourseEndPageType : PageType
    {
        public CourseEndPageType() : base("PT_BASE_04", "COURSE_END")
        {
        }

        public override string PageSubType => "BASE";
    }

    private sealed class VideoPageType : PageType
    {
        public VideoPageType() : base("PT_BASE_05", "VIDEO")
        {
        }

        public override string PageSubType => "BASE";
    }

    private sealed class QuizPageType : PageType
    {
        public QuizPageType() : base("PT_BASE_06", "QUIZ")
        {
        }

        public override string PageSubType => "BASE";
    }

    private sealed class CoursePassPageType : PageType
    {
        public CoursePassPageType() : base("PT_BASE_07", "COURSE_PASS")
        {
        }

        public override string PageSubType => "BASE";
    }
    private sealed class CourseFailPageType : PageType
    {
        public CourseFailPageType() : base("PT_BASE_08", "COURSE_FAIL")
        {
        }

        public override string PageSubType => "BASE";
    }


    private sealed class IncursionPageType : PageType
    {
        public IncursionPageType() : base("PT_CUSTOM_01", "INCURSION")
        {
        }

        public override string PageSubType => "CUSTOM";
    }
    private sealed class PhoneticAlphabetPageType : PageType
    {
        public PhoneticAlphabetPageType() : base("PT_CUSTOM_02", "PHONETIC_ALPHABET")
        {
        }

        public override string PageSubType => "CUSTOM";
    }
    private sealed class MostDangerousLocationsPageType : PageType
    {
        public MostDangerousLocationsPageType() : base("PT_CUSTOM_03", "MOST_DANGEROUS_LOCATIONS")
        {
        }

        public override string PageSubType => "CUSTOM";
    }

    private sealed class SignLocationsPageType : PageType
    {
        public SignLocationsPageType() : base("PT_CUSTOM_04", "SIGN_LOCATIONS")
        {
        }

        public override string PageSubType => "CUSTOM";
    }
    private sealed class ForeignObjectDebrisPageType : PageType
    {
        public ForeignObjectDebrisPageType() : base("PT_CUSTOM_05", "FOREIGN_OBJECT_DEBRIS")
        {
        }

        public override string PageSubType => "CUSTOM";
    }

    private sealed class VehicleEnteringRestrictedAreaPageType : PageType
    {
        public VehicleEnteringRestrictedAreaPageType() : base("PT_CUSTOM_06", "VEHICLE_ENTERING_RESTRICTED_AREA")
        {
        }

        public override string PageSubType => "CUSTOM";
    }
    private sealed class AircraftReadyToMovePageType : PageType
    {
        public AircraftReadyToMovePageType() : base("PT_CUSTOM_07", "AIRCRAFT_READY_TO_MOVE")
        {
        }

        public override string PageSubType => "CUSTOM";
    }
}
