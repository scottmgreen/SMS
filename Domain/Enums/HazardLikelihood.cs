namespace SMS_Domain.Enums;

public abstract class HazardLikelihood : BaseEnum<HazardLikelihood>
{
    protected HazardLikelihood(string value, string name, int score, string matrixLetter) : base(value, name)
    {
        Score = score;
        MatrixLetter = matrixLetter;
    }

    public int Score { get; }
    public string MatrixLetter { get; }

    public static readonly HazardLikelihood Rare = new RareLikelihood();
    public static readonly HazardLikelihood Unlikely = new UnlikelyLikelihood();
    public static readonly HazardLikelihood Possible = new PossibleLikelihood();
    public static readonly HazardLikelihood Likely = new LikelyLikelihood();
    public static readonly HazardLikelihood Frequent = new FrequentLikelihood();
    public static readonly HazardLikelihood Unknown = new UnknownLikelihood();

    public static HazardLikelihood FromScore(int score)
    {
        return score switch
        {
            1 => Rare,
            2 => Unlikely,
            3 => Possible,
            4 => Likely,
            5 => Frequent,
            _ => Unknown
        };
    }

    public static string GetDisplayName(int score) => FromScore(score).Name;

    private sealed class RareLikelihood : HazardLikelihood
    {
        public RareLikelihood() : base("1", "Rare", 1, "A") { }
    }

    private sealed class UnlikelyLikelihood : HazardLikelihood
    {
        public UnlikelyLikelihood() : base("2", "Unlikely", 2, "B") { }
    }

    private sealed class PossibleLikelihood : HazardLikelihood
    {
        public PossibleLikelihood() : base("3", "Possible", 3, "C") { }
    }

    private sealed class LikelyLikelihood : HazardLikelihood
    {
        public LikelyLikelihood() : base("4", "Likely", 4, "D") { }
    }

    private sealed class FrequentLikelihood : HazardLikelihood
    {
        public FrequentLikelihood() : base("5", "Frequent", 5, "E") { }
    }

    private sealed class UnknownLikelihood : HazardLikelihood
    {
        public UnknownLikelihood() : base("0", "Unknown", 0, "?") { }
    }
}
