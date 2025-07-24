using CBT3_Domain.Entities;
using CBT3_Domain.ValueObjects;

namespace CBT3_Infrastructure.Common;

public static class ParameterNames
{
    /// <summary>
    /// Audit
    /// </summary>
    /// 
    ///
    //@pFirstName NVARCHAR(50),
    //@pLastName NVARCHAR(50),
    //@pUPID NVARCHAR(50),
    //@pYearOfBirth NVARCHAR(50),
    //@pCourseCode INT,
    //@pDate DATE,
    //@pTrainingExists BIT OUTPUT

    //   @pTrainingSessionId varchar(50),
    //@pHostName varchar(50),
    //@pTraineeId varchar(50),
    //@pCourseId varchar(50),
    //@pCourseStartedAt datetime,
    //   @pCourseFinishedAt datetime,
    //   @pUpdatedHostName varchar(50) OUTPUT

    private static readonly Lazy<string> _pmTrainingSessionId = new Lazy<string>(() => "@pTrainingSessionID");
    public static string pmTrainingSessionId => _pmTrainingSessionId.Value;

    private static readonly Lazy<string> _pmHostName = new Lazy<string>(() => "@pHostName");
    public static string pmHostName => _pmHostName.Value;

    private static readonly Lazy<string> _pmCourseStartedAt = new Lazy<string>(() => "@pCourseStartedAt");
    public static string pmCourseStartedAt => _pmCourseStartedAt.Value;

    private static readonly Lazy<string> _pmCourseEndedAt = new Lazy<string>(() => "@pCourseEndedAt");
    public static string pmCourseEndedAt => _pmCourseEndedAt.Value;

    private static readonly Lazy<string> _pmUpdatedHostName = new Lazy<string>(() => "@pUpdatedHostName");
    public static string pmUpdatedHostName => _pmUpdatedHostName.Value;

    private static readonly Lazy<string> _pmTrainingStationStatus = new Lazy<string>(() => "@pTrainingStationStatus");
    public static string pmTrainingStationStatus => _pmTrainingStationStatus.Value;

    private static readonly Lazy<string> _pmTrainingStationCircuitId = new Lazy<string>(() => "@pTrainingStationCircuitId");
    public static string pmTrainingStationCircuitId => _pmTrainingStationCircuitId.Value;


    private static readonly Lazy<string> _pmTrainingStatus = new Lazy<string>(() => "@pTrainingStatus");
    public static string pmTrainingStatus => _pmTrainingStatus.Value;





    private static readonly Lazy<string> _pmCourseCode = new Lazy<string>(() => "@pCourseCode");
    public static string pmCourseCode => _pmCourseCode.Value;


    private static readonly Lazy<string> _pmDate = new Lazy<string>(() => "@pDate");
    public static string pmDate => _pmDate.Value;


    private static readonly Lazy<string> _pmTrainingExists = new Lazy<string>(() => "@pTrainingExists");
    public static string pmTrainingExists => _pmTrainingExists.Value;

    private static readonly Lazy<string> _pmCoursePass = new Lazy<string>(() => "@pCoursePass");
    public static string pmCoursePass => _pmCoursePass.Value;


    private static readonly Lazy<string> _pmRecordedAt = new Lazy<string>(() => "@pRecordedAt");
    public static string pmRecordedAt => _pmRecordedAt.Value;


    private static readonly Lazy<string> _pmUserID = new Lazy<string>(() => "@pUserID");
    public static string pmUserID => _pmUserID.Value;

    private static readonly Lazy<string> _pmMessageType = new Lazy<string>(() => "@pMessageType");
    public static string pmMessageType => _pmMessageType.Value;

    private static readonly Lazy<string> _pmSeverity = new Lazy<string>(() => "@pSeverity");
    public static string pmSeverity => _pmSeverity.Value;

    private static readonly Lazy<string> _pmModule = new Lazy<string>(() => "@pModule");
    public static string pmModule => _pmModule.Value;

    private static readonly Lazy<string> _pmFunction = new Lazy<string>(() => "@pFunction");
    public static string pmFunction => _pmFunction.Value;

    private static readonly Lazy<string> _pmDescription = new Lazy<string>(() => "@pDescription");
    public static string pmDescription => _pmDescription.Value;
    /// <summary>
    /// Trainee
    /// </summary>
    private static readonly Lazy<string> _pmTraineeId = new Lazy<string>(() => "@pTraineeID");
    public static string pmTraineeId => _pmTraineeId.Value;

    private static readonly Lazy<string> _pmFirstName = new Lazy<string>(() => "@pFirstName");
    public static string pmFirstName => _pmFirstName.Value;

    private static readonly Lazy<string> _pmLastName = new Lazy<string>(() => "@pLastName");
    public static string pmLastName => _pmLastName.Value;

    private static readonly Lazy<string> _pmUPID = new Lazy<string>(() => "@pUPID");
    public static string pmUPID => _pmUPID.Value;

    private static readonly Lazy<string> _pmYearOfBirth = new Lazy<string>(() => "@pYearOfBirth");
    public static string pmYearOfBirth => _pmYearOfBirth.Value;

    private static readonly Lazy<string> _pmSubscribeToEmailNewsletter = new Lazy<string>(() => "@pSubscribeToEmailNewsletter");
    public static string pmSubscribeToEmailNewsletter => _pmSubscribeToEmailNewsletter.Value;

    private static readonly Lazy<string> _pmSubscribeToTextNewsletter = new Lazy<string>(() => "@pSubscribeToTextNewsletter");
    public static string pmSubscribeToTextNewsletter => _pmSubscribeToTextNewsletter.Value;

    private static readonly Lazy<string> _pmSubscribeToOperationalTexts = new Lazy<string>(() => "@pSubscribeToOperationalTexts");
    public static string pmSubscribeToOperationalTexts => _pmSubscribeToOperationalTexts.Value;

    private static readonly Lazy<string> _pmCreatedBy = new Lazy<string>(() => "@pCreatedBy");
    public static string pmCreatedBy => _pmCreatedBy.Value;

    private static readonly Lazy<string> _pmCreatedDate = new Lazy<string>(() => "@pCreatedDate");
    public static string pmCreatedDate => _pmCreatedDate.Value;
    /// <summary>
    /// Course
    /// </summary>
    private static readonly Lazy<string> _pmCourseId = new Lazy<string>(() => "@pCourseID");
    public static string pmCourseId => _pmCourseId.Value;

    private static readonly Lazy<string> _pmCourseName = new Lazy<string>(() => "@pCourseName");
    public static string pmCourseName => _pmCourseName.Value;

    private static readonly Lazy<string> _pmExpiryMonths = new Lazy<string>(() => "@pExpiryMonths");
    public static string pmExpiryMonths => _pmExpiryMonths.Value;

    private static readonly Lazy<string> _pmCourseType = new Lazy<string>(() => "@pCourseType");
    public static string pmCourseType => _pmCourseType.Value;
    /// <summary>
    /// Lesson
    /// </summary>
    private static readonly Lazy<string> _pmLessonId = new Lazy<string>(() => "@pLessonID");
    public static string pmLessonId => _pmLessonId.Value;

    private static readonly Lazy<string> _pmLessonName = new Lazy<string>(() => "@pLessonName");
    public static string pmLessonName => _pmLessonName.Value;

    private static readonly Lazy<string> _pmLessonOrder = new Lazy<string>(() => "@pLessonOrder");
    public static string pmLessonOrder => _pmLessonOrder.Value;

    private static readonly Lazy<string> _pmPageTypeId = new Lazy<string>(() => "@pPageTypeID");
    public static string pmPageTypeId => _pmPageTypeId.Value;

    private static readonly Lazy<string> _pmPageOrder = new Lazy<string>(() => "@pPageOrder");
    public static string pmPageOrder => _pmPageOrder.Value;

    private static readonly Lazy<string> _pmPageText = new Lazy<string>(() => "@pPageText");
    public static string pmPageText => _pmPageText.Value;

    private static readonly Lazy<string> _pmVideoURL = new Lazy<string>(() => "@pVideoURL");
    public static string pmVideoURL => _pmVideoURL.Value;

    private static readonly Lazy<string> _pmAudioURL = new Lazy<string>(() => "@pAudioURL");
    public static string pmAudioURL => _pmAudioURL.Value;

    private static readonly Lazy<string> _pmImageURL = new Lazy<string>(() => "@pImageURL");
    public static string pmImageURL => _pmImageURL.Value;
    /// <summary>
    /// Answer
    /// </summary>
    private static readonly Lazy<string> _pmAnswerId = new Lazy<string>(() => "@pAnswerID");
    public static string pmAnswerId => _pmAnswerId.Value;

    private static readonly Lazy<string> _pmAnswerText = new Lazy<string>(() => "@pAnswerText");
    public static string pmAnswerText => _pmAnswerText.Value;

    private static readonly Lazy<string> _pmAnswerImage = new Lazy<string>(() => "@pAnswerImage");
    public static string pmAnswerImage => _pmAnswerImage.Value;

    private static readonly Lazy<string> _pmIsCorrectAnswer = new Lazy<string>(() => "@pIsCorrect");
    public static string pmIsCorrectAnswer => _pmIsCorrectAnswer.Value;
    /// <summary>
    /// Question
    /// </summary>

    private static readonly Lazy<string> _pmQuestionId = new Lazy<string>(() => "@pQuestionID");
    public static string pmQuestionId => _pmQuestionId.Value;

    private static readonly Lazy<string> _pmQuestionText = new Lazy<string>(() => "@pQuestionText");
    public static string pmQuestionText => _pmQuestionText.Value;

    private static readonly Lazy<string> _pmQuestionImage = new Lazy<string>(() => "@pQuestionImage");
    public static string pmQuestionImage => _pmQuestionImage.Value;

    private static readonly Lazy<string> _pmQuestionType = new Lazy<string>(() => "@pQuestionType");
    public static string pmQuestionType => _pmQuestionType.Value;

    private static readonly Lazy<string> _pmQuestionPoolId = new Lazy<string>(() => "@pQuestionPoolId");
    public static string pmQuestionPoolId => _pmQuestionPoolId.Value;
    /// <summary>
    /// Miscellaneous
    /// </summary>
    private static readonly Lazy<string> _pmUserId = new Lazy<string>(() => "@pCBTUserID");
    public static string pmUserId => _pmUserId.Value;

    private static readonly Lazy<string> _pmAttemptNumber = new Lazy<string>(() => "@pAttemptNumber");
    public static string pmAttemptNumber => _pmAttemptNumber.Value;

    private static readonly Lazy<string> _pmIsCorrect = new Lazy<string>(() => "@pIsCorrect");
    public static string pmIsCorrect => _pmIsCorrect.Value;

    private static readonly Lazy<string> _pmLessonQuizId = new Lazy<string>(() => "@pLessonQuizID");
    public static string pmLessonQuizId => _pmLessonQuizId.Value;

    private static readonly Lazy<string> _pmAttemptsAllowed = new Lazy<string>(() => "@pAttemptsAllowed");
    public static string pmAttemptsAllowed => _pmAttemptsAllowed.Value;


}



