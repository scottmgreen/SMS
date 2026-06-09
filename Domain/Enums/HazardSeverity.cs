using SMS_Domain.Common;

namespace SMS_Domain.Enums;

public abstract class HazardSeverity : BaseEnum<HazardSeverity>
{
    protected HazardSeverity(string value, string name, int score) : base(value, name)
    {
        Score = score;
    }

    public int Score { get; }

    public static readonly HazardSeverity Catastrophic = new CatastrophicSeverity();
    public static readonly HazardSeverity Major = new MajorSeverity();
    public static readonly HazardSeverity Serious = new SeriousSeverity();
    public static readonly HazardSeverity Moderate = new ModerateSeverity();
    public static readonly HazardSeverity Minor = new MinorSeverity();
    public static readonly HazardSeverity Unknown = new UnknownSeverity();

    public static HazardSeverity FromScore(int score)
    {
        return score switch
        {
            5 => Catastrophic,
            4 => Major,
            3 => Serious,
            2 => Moderate,
            1 => Minor,
            _ => Unknown
        };
    }

    public static string GetDisplayName(int score) => FromScore(score).Name;

    private sealed class CatastrophicSeverity : HazardSeverity
    {
        public CatastrophicSeverity() : base("5", "Catastrophic", 5) { }
    }

    private sealed class MajorSeverity : HazardSeverity
    {
        public MajorSeverity() : base("4", "Major", 4) { }
    }

    private sealed class SeriousSeverity : HazardSeverity
    {
        public SeriousSeverity() : base("3", "Serious", 3) { }
    }

    private sealed class ModerateSeverity : HazardSeverity
    {
        public ModerateSeverity() : base("2", "Moderate", 2) { }
    }

    private sealed class MinorSeverity : HazardSeverity
    {
        public MinorSeverity() : base("1", "Minor", 1) { }
    }

    private sealed class UnknownSeverity : HazardSeverity
    {
        public UnknownSeverity() : base("0", "Unknown", 0) { }
    }
}
