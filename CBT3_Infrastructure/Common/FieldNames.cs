namespace CBT3_Infrastructure.Common;


public static class FieldNames
{
    /// <summary>
    /// Audit
    /// </summary>
    /// 
    private static readonly Lazy<string> _fHostName = new Lazy<string>(() => "fldv_HostName");
    public static string fHostName => _fHostName.Value;

    private static readonly Lazy<string> _fTrainingSessionID = new Lazy<string>(() => "fldv_TrainingSessionID");
    public static string fTrainingSessionID => _fTrainingSessionID.Value;

    private static readonly Lazy<string> _fCourseStartedAt = new Lazy<string>(() => "fldd_CourseStartedAt");
    public static string fCourseStartedAt => _fCourseStartedAt.Value;
   
    private static readonly Lazy<string> _fCourseEndedAt = new Lazy<string>(() => "fldd_CourseEndedAt");
    public static string fCourseEndedAt => _fCourseEndedAt.Value;

    
    private static readonly Lazy<string> _fCircuitId = new Lazy<string>(() => "fldv_TrainingStationCircuitId");
    public static string fCircuitID => _fCircuitId.Value;

    private static readonly Lazy<string> _fTrainingStationStatus = new Lazy<string>(() => "fldv_TrainingStationStatus");
    public static string fTrainingStationStatus => _fTrainingStationStatus.Value;

    private static readonly Lazy<string> _fTrainingStatus = new Lazy<string>(() => "fldv_TrainingStatus");
    public static string fTrainingStatus => _fTrainingStatus.Value;


    private static readonly Lazy<string> _fRecordedAt = new Lazy<string>(() => "fldd_RecordedAt");
    public static string fRecordedAt => _fRecordedAt.Value;

    private static readonly Lazy<string> _fUserID = new Lazy<string>(() => "fldv_UserID");
    public static string fUserID => _fUserID.Value;

    private static readonly Lazy<string> _fWorkstation = new Lazy<string>(() => "fldv_Workstation");
    public static string fWorkstation => _fWorkstation.Value;

    private static readonly Lazy<string> _fEventDateTime = new Lazy<string>(() => "fldd_EventDateTime");
    public static string fEventDateTime => _fEventDateTime.Value;

    private static readonly Lazy<string> _fMessageType = new Lazy<string>(() => "fldv_MessageType");
    public static string fMessageType => _fMessageType.Value;

    private static readonly Lazy<string> _fSeverity = new Lazy<string>(() => "fldv_Severity");
    public static string fSeverity => _fSeverity.Value;

    private static readonly Lazy<string> _fModule = new Lazy<string>(() => "fldv_Module");
    public static string fModule => _fModule.Value;

    private static readonly Lazy<string> _fFunction = new Lazy<string>(() => "fldv_Function");
    public static string fFunction => _fFunction.Value;

    private static readonly Lazy<string> _fDescription = new Lazy<string>(() => "fldv_Description");
    public static string fDescription => _fDescription.Value;
    /// <summary>
    /// Trainee
    /// </summary>
    private static readonly Lazy<string> _fTraineeId = new Lazy<string>(() => "fldv_TraineeID");
    public static string fTraineeId => _fTraineeId.Value;

    private static readonly Lazy<string> _fFirstName = new Lazy<string>(() => "fldv_FirstName");
    public static string fFirstName => _fFirstName.Value;

    private static readonly Lazy<string> _fLastName = new Lazy<string>(() => "fldv_LastName");
    public static string fLastName => _fLastName.Value;

    private static readonly Lazy<string> _fUPID = new Lazy<string>(() => "fldv_UPID");
    public static string fUPID => _fUPID.Value;

    private static readonly Lazy<string> _fYearOfBirth = new Lazy<string>(() => "fldv_YearOfBirth");
    public static string fYearOfBirth => _fYearOfBirth.Value;

    private static readonly Lazy<string> _fSubscribeToEmailNewsletter = new Lazy<string>(() => "fldb_SubscribeToEmailNewsletter");
    public static string fSubscribeToEmailNewsletter => _fSubscribeToEmailNewsletter.Value;

    private static readonly Lazy<string> _fSubscribeToTextNewsletter = new Lazy<string>(() => "fldb_SubscribeToTextNewsletter");
    public static string fSubscribeToTextNewsletter => _fSubscribeToTextNewsletter.Value;

    private static readonly Lazy<string> _fSubscribeToOperationalTexts = new Lazy<string>(() => "fldb_SubscribeToOperationalTexts");
    public static string fSubscribeToOperationalTexts => _fSubscribeToOperationalTexts.Value;

    private static readonly Lazy<string> _fCreatedBy = new Lazy<string>(() => "fldv_CreatedBy");
    public static string fCreatedBy => _fCreatedBy.Value;

    private static readonly Lazy<string> _fCreatedDate = new Lazy<string>(() => "fldv_CreatedDate");
    public static string fCreatedDate => _fCreatedDate.Value;

    private static readonly Lazy<string> _fUpdatedBy = new Lazy<string>(() => "fldv_UpdatedBy");
    public static string fUpdatedBy => _fUpdatedBy.Value;

    private static readonly Lazy<string> _fUpdatedDate = new Lazy<string>(() => "fldv_UpdatedDate");
    public static string fUpdatedDate => _fUpdatedDate.Value;
    ///
    ///Course
    ///
    private static readonly Lazy<string> _fCourseId = new Lazy<string>(() => "fldv_CourseID");
    public static string fCourseId => _fCourseId.Value;

    private static readonly Lazy<string> _fCourseName = new Lazy<string>(() => "fldv_CourseName");
    public static string fCourseName => _fCourseName.Value;

    private static readonly Lazy<string> _fExpiryMonths = new Lazy<string>(() => "fldi_ExpiryMonths");
    public static string fExpiryMonths => _fExpiryMonths.Value;

    private static readonly Lazy<string> _fCourseType = new Lazy<string>(() => "fldv_CourseType");
    public static string fCourseType => _fCourseType.Value;

    private static readonly Lazy<string> _fCustomerServiceRequirement = new Lazy<string>(() => "fldb_CustomerServiceRequirement");
    public static string fCustomerServiceRequirement => _fCustomerServiceRequirement.Value;
    ///
    ///Lesson
    ///
    private static readonly Lazy<string> _fLessonId = new Lazy<string>(() => "fldv_LessonID");
    public static string fLessonId => _fLessonId.Value;

    private static readonly Lazy<string> _fLessonName = new Lazy<string>(() => "fldv_LessonName");
    public static string fLessonName => _fLessonName.Value;

    private static readonly Lazy<string> _fLessonOrder = new Lazy<string>(() => "fldi_LessonOrder");
    public static string fLessonOrder => _fLessonOrder.Value;

    private static readonly Lazy<string> _fLessonPageId = new Lazy<string>(() => "fldv_LessonPageID");
    public static string fLessonPageId => _fLessonPageId.Value;

    private static readonly Lazy<string> _fPageTypeId = new Lazy<string>(() => "fldv_PageTypeID");
    public static string fPageTypeId => _fPageTypeId.Value;

    private static readonly Lazy<string> _fPageSubTypeId = new Lazy<string>(() => "fldv_PageSubType");
    public static string fPageSubTypeId => _fPageSubTypeId.Value;

    private static readonly Lazy<string> _fPageOrder = new Lazy<string>(() => "fldi_PageOrder");
    public static string fPageOrder => _fPageOrder.Value;

    private static readonly Lazy<string> _fPageText = new Lazy<string>(() => "fldv_PageText");
    public static string fPageText => _fPageText.Value;

    private static readonly Lazy<string> _fVideoURL = new Lazy<string>(() => "fldv_VideoURL");
    public static string fVideoURL => _fVideoURL.Value;

    private static readonly Lazy<string> _fAudioURL = new Lazy<string>(() => "fldv_AudioURL");
    public static string fAudioURL => _fAudioURL.Value;

    private static readonly Lazy<string> _fImageURL = new Lazy<string>(() => "fldv_ImageURL");
    public static string fImageURL => _fImageURL.Value;
    ///
    ///
    ///
    private static readonly Lazy<string> _fUserId = new Lazy<string>(() => "fldi_CBTUserID");
    public static string fUserId => _fUserId.Value;

    private static readonly Lazy<string> _fAttemptNumber = new Lazy<string>(() => "fldi_AttemptNumber");
    public static string fAttemptNumber => _fAttemptNumber.Value;

    private static readonly Lazy<string> _fIsCorrect = new Lazy<string>(() => "fldb_IsCorrect");
    public static string fIsCorrect => _fIsCorrect.Value;
    ///
    ///
    ///
    private static readonly Lazy<string> _fLessonQuizId = new Lazy<string>(() => "fldv_LessonQuizID");
    public static string fLessonQuizId => _fLessonQuizId.Value;

    private static readonly Lazy<string> _fAttemptsAllowed = new Lazy<string>(() => "fldi_AttemptsAllowed");
    public static string fAttemptsAllowed => _fAttemptsAllowed.Value;
    ///
    ///
    ///
    private static readonly Lazy<string> _fQuestionPoolId = new Lazy<string>(() => "fldv_QuestionPoolID");
    public static string fQuestionPoolId => _fQuestionPoolId.Value;

    private static readonly Lazy<string> _fJumpBackStart = new Lazy<string>(() => "fldv_JumpBackStart");
    public static string fJumpBackStart => _fJumpBackStart.Value;

    private static readonly Lazy<string> _fJumpBackEnd = new Lazy<string>(() => "fldv_JumpBackEnd");
    public static string fJumpBackEnd => _fJumpBackEnd.Value;

    private static readonly Lazy<string> _fJumpBackFileName = new Lazy<string>(() => "fldv_JumpBackFileName");
    public static string fJumpBackFileName => _fJumpBackFileName.Value;
    ///
    ///
    ///
    private static readonly Lazy<string> _fQuestionId = new Lazy<string>(() => "fldv_QuestionID");
    public static string fQuestionId => _fQuestionId.Value;

    private static readonly Lazy<string> _fQuestionText = new Lazy<string>(() => "fldv_QuestionText");
    public static string fQuestionText => _fQuestionText.Value;

    private static readonly Lazy<string> _fQuestionImage = new Lazy<string>(() => "fldv_QuestionImage");
    public static string fQuestionImage => _fQuestionImage.Value;

    private static readonly Lazy<string> _fQuestionType = new Lazy<string>(() => "fldv_QuestionType");
    public static string fQuestionType => _fQuestionType.Value;
    ///
    ///
    ///
    private static readonly Lazy<string> _fAnswerId = new Lazy<string>(() => "fldv_AnswerID");
    public static string fAnswerId => _fAnswerId.Value;

    private static readonly Lazy<string> _fAnswerText = new Lazy<string>(() => "fldv_AnswerText");
    public static string fAnswerText => _fAnswerText.Value;

    private static readonly Lazy<string> _fAnswerImage = new Lazy<string>(() => "fldv_AnswerImage");
    public static string fAnswerImage => _fAnswerImage.Value;

    private static readonly Lazy<string> _fIsCorrectAnswer = new Lazy<string>(() => "fldb_IsCorrect");
    public static string fIsCorrectAnswer => _fIsCorrectAnswer.Value;
    ///
    ///
    ///
    private static readonly Lazy<string> _fLessonPageTypeId = new Lazy<string>(() => "fldv_PageTypeID");
    public static string fLessonPageTypeId => _fLessonPageTypeId.Value;

    private static readonly Lazy<string> _fLessonPageTypeName = new Lazy<string>(() => "fldv_PageType");
    public static string fLessonPageTypeName => _fLessonPageTypeName.Value;

    private static readonly Lazy<string> _fLessonPageSubType = new Lazy<string>(() => "fldv_PageSubType");
    public static string fLessonPageSubType => _fLessonPageSubType.Value;
}



