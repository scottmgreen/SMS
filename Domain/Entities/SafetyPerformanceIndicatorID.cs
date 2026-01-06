using SMS_Domain.Common;

namespace SMS_Domain.Entities;

public sealed class SafetyPerformanceIndicatorID : BaseID<string>
{
    public SafetyPerformanceIndicatorID(string value) : base(value)
    {
    }

}