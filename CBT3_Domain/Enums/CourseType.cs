
namespace CBT3_Domain.Enums;

public abstract class CourseType : BaseEnum<CourseType>
{
    protected CourseType(string value, string name) : base(value, name) { }

    public static readonly CourseType SIDA = new SIDA_CourseType();
    public static readonly CourseType HANDS_ON = new HANDS_ON_CourseType();
    public static readonly CourseType OTHER = new OTHER_CourseType();
    



    private sealed class SIDA_CourseType : CourseType
    {
        public SIDA_CourseType() : base("SIDA", "SIDA")
        {
        }
    }
    private sealed class HANDS_ON_CourseType : CourseType
    {
        public HANDS_ON_CourseType() : base("HANDS_ON", "HANDS_ON")
        {
        }
    }
    private sealed class OTHER_CourseType : CourseType
    {
        public OTHER_CourseType() : base("OTHER", "OTHER")
        {
        }
    }
    

}
