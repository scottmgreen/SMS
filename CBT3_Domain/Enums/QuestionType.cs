namespace CBT3_Domain.Enums;

//MC			--> Multiple Choice
//MC_IA		--> Multiple Choice With Image Answer
//MC_IQ       --> Multiple Choice With Image Question
//MC_IQ_IA    --> Multiple Choice With Image Question and Image Answer
//TF			--> True False
//TF_IQ		--> True False with Image Question


public abstract class QuestionType : BaseEnum<QuestionType>
{
    protected QuestionType(string value, string name) : base(value, name) { }

    public static readonly QuestionType MultipleChoice = new MultipleChoiceQuestionType();
    public static readonly QuestionType MultipleChoiceWithImageAnswer = new MultipleChoiceWithImageAnswerQuestionType();
    public static readonly QuestionType MultipleChoiceWithImageQuestion = new MultipleChoiceWithImageQuestionQuestionType();
    public static readonly QuestionType MultipleChoiceWithImageQuestionAndImageAnswer = new MultipleChoiceWithImageQuestionAndImageAnswerQuestionType();
    public static readonly QuestionType TrueFalse = new TrueFalseQuestionType();
    public static readonly QuestionType TrueFalseWithImageQuestion = new TrueFalseWithImageQuestionQuestionType();

    private sealed class MultipleChoiceQuestionType : QuestionType
    {
        public MultipleChoiceQuestionType() : base("MC", "Multiple Choice")
        {
        }
    }
    private sealed class MultipleChoiceWithImageAnswerQuestionType : QuestionType
    {
        public MultipleChoiceWithImageAnswerQuestionType() : base("MC_IA", "Multiple Choice With Image Answer")
        {
        }
    }
    private sealed class MultipleChoiceWithImageQuestionQuestionType : QuestionType
    {
        public MultipleChoiceWithImageQuestionQuestionType() : base("MC_IQ", "Multiple Choice With Image Question")
        {
        }
    }
    private sealed class MultipleChoiceWithImageQuestionAndImageAnswerQuestionType : QuestionType
    {
        public MultipleChoiceWithImageQuestionAndImageAnswerQuestionType() : base("MC_IQ_IA", "Multiple Choice With Image Question and Image Answer")
        {
        }
    }
    private sealed class TrueFalseQuestionType : QuestionType
    {
        public TrueFalseQuestionType() : base("TF", "True False")
        {
        }
    }
    private sealed class TrueFalseWithImageQuestionQuestionType : QuestionType
    {
        public TrueFalseWithImageQuestionQuestionType() : base("TF_IQ", "True False with Image Question")
        {
        }
    }

}
