namespace SMS_Domain.Errors;

/// <summary>
/// Contains the domain errors for Safety Management System (SMS).
/// </summary>
public static class DomainErrors
{
    /// <summary>
    /// Contains system-level errors.
    /// </summary>
    public static class SystemError
    {
        public static Error AuditLogEntryError => new Error("SystemError.AuditLogEntryError", "Audit Log Entry Error.");
        public static Error FileExistsError => new Error("SystemError.FileExistsError", "File Does Not Exist Error.");
        // public static Error VideoPlaybackError => new Error("SystemError.VideoPlaybackError", "Video Playback Error.");
        // public static Error LessonQuizError => new Error("SystemError.LessonQuizError", "Lesson Quiz Error.");
        // public static Error CustomPageError => new Error("SystemError.CustomPageError", "Custom Page Error.");
    }

    /// <summary>
    /// Contains airport shared dataset-related errors.
    /// </summary>
    public static class AirportSharedDatasetError
    {
        public static Error NullOrEmpty => new Error("AirportSharedDataset.NullOrEmpty", "The Airport Shared Dataset is required.");
        public static Error CodeRequired => new Error("AirportSharedDataset.CodeRequired", "The Airport Shared Dataset Code is required.");
        public static Error ReportIDRequired => new Error("AirportSharedDataset.ReportIDRequired", "The Report ID is required - cannot create dataset without parent report.");
        public static Error HazardCodeRequired => new Error("AirportSharedDataset.HazardCodeRequired", "The Hazard Code is required.");
        public static Error InvalidCode => new Error("AirportSharedDataset.InvalidCode", "The Airport Shared Dataset Code is invalid.");
        public static Error InvalidReportID => new Error("AirportSharedDataset.InvalidReportID", "The Report ID is invalid.");
        public static Error InvalidNarrative => new Error("AirportSharedDataset.InvalidNarrative", "The narrative exceeds maximum length.");
        public static Error NotFound => new Error("AirportSharedDataset.NotFound", "The Airport Shared Dataset was not found.");
        public static Error CreateFailed => new Error("AirportSharedDataset.CreateFailed", "Failed to create the Airport Shared Dataset.");
        public static Error UpdateFailed => new Error("AirportSharedDataset.UpdateFailed", "Failed to update the Airport Shared Dataset.");
        public static Error DeleteFailed => new Error("AirportSharedDataset.DeleteFailed", "Failed to delete the Airport Shared Dataset.");
    }
    /// <summary>
    /// Contains hazard-related errors.
    /// </summary>
    public static class HazardError
    {
        public static Error NullOrEmpty => new Error("Hazard.NullOrEmpty", "The Hazard is required.");
        public static Error CodeRequired => new Error("Hazard.CodeRequired", "The Hazard Code is required.");
        public static Error ReportCodeRequired => new Error("Hazard.ReportCodeRequired", "The Report Code is required.");
        public static Error InvalidCode => new Error("Hazard.InvalidCode", "The Hazard Code is invalid.");
        public static Error NotFound => new Error("Hazard.NotFound", "The Hazard was not found.");
        public static Error CreateFailed => new Error("Hazard.CreateFailed", "Failed to create the Hazard.");
        public static Error UpdateFailed => new Error("Hazard.UpdateFailed", "Failed to update the Hazard.");
        public static Error DeleteFailed => new Error("Hazard.DeleteFailed", "Failed to delete the Hazard.");
    }

    /// <summary>
    /// Contains report-related errors.
    /// </summary>
    public static class ReportError
    {
        public static Error NullOrEmpty => new Error("Report.NullOrEmpty", "The Report is required.");
        public static Error CodeRequired => new Error("Report.CodeRequired", "The Report Code is required.");
        public static Error InvalidStatus => new Error("Report.InvalidStatus", "The Report Status is invalid.");
        public static Error InvalidStage => new Error("Report.InvalidStage", "The Report Stage is invalid.");
        public static Error NotFound => new Error("Report.NotFound", "The Report was not found.");
        public static Error CreateFailed => new Error("Report.CreateFailed", "Failed to create the Report.");
        public static Error UpdateFailed => new Error("Report.UpdateFailed", "Failed to update the Report.");
        public static Error DeleteFailed => new Error("Report.DeleteFailed", "Failed to delete the Report.");
    }

    /// <summary>
    /// Contains investigation-related errors.
    /// </summary>
    public static class InvestigationError
    {
        public static Error NullOrEmpty => new Error("Investigation.NullOrEmpty", "The Investigation is required.");
        public static Error ReportCodeRequired => new Error("Investigation.ReportCodeRequired", "The Report Code is required.");
        public static Error NotFound => new Error("Investigation.NotFound", "The Investigation was not found.");
        public static Error InvalidCode => new Error("Investigation.InvalidCode", "The Investigation Code is invalid.");
        public static Error CreateFailed => new Error("Investigation.CreateFailed", "Failed to create the Investigation.");
        public static Error UpdateFailed => new Error("Investigation.UpdateFailed", "Failed to update the Investigation.");
        public static Error DeleteFailed => new Error("Investigation.DeleteFailed", "Failed to delete the Investigation.");
    }

    /// <summary>
    /// Contains interview-related errors.
    /// </summary>
    public static class InterviewError
    {
        public static Error NullOrEmpty => new Error("Interview.NullOrEmpty", "The Interview is required.");
        public static Error InvestigationCodeRequired => new Error("Interview.InvestigationCodeRequired", "The Investigation Code is required.");
        public static Error PersonInterviewedRequired => new Error("Interview.PersonInterviewedRequired", "The Person Interviewed is required.");
        public static Error NotFound => new Error("Interview.NotFound", "The Interview was not found.");
        public static Error CreateFailed => new Error("Interview.CreateFailed", "Failed to create the Interview.");
        public static Error UpdateFailed => new Error("Interview.UpdateFailed", "Failed to update the Interview.");
        public static Error DeleteFailed => new Error("Interview.DeleteFailed", "Failed to delete the Interview.");
    }

    /// <summary>
    /// Contains risk analysis-related errors.
    /// </summary>
    public static class RiskAnalysisError
    {
        public static Error NullOrEmpty => new Error("RiskAnalysis.NullOrEmpty", "The Risk Analysis is required.");
        public static Error HazardCodeRequired => new Error("RiskAnalysis.HazardCodeRequired", "The Hazard Code is required.");
        public static Error InvalidStatus => new Error("RiskAnalysis.InvalidStatus", "The Risk Analysis Status is invalid.");
        public static Error InvalidStage => new Error("RiskAnalysis.InvalidStage", "The Risk Analysis Stage is invalid.");
        public static Error NotFound => new Error("RiskAnalysis.NotFound", "The Risk Analysis was not found.");
        public static Error CreateFailed => new Error("RiskAnalysis.CreateFailed", "Failed to create the Risk Analysis.");
        public static Error UpdateFailed => new Error("RiskAnalysis.UpdateFailed", "Failed to update the Risk Analysis.");
        public static Error DeleteFailed => new Error("RiskAnalysis.DeleteFailed", "Failed to delete the Risk Analysis.");
    }

    /// <summary>
    /// Contains risk assessment-related errors.
    /// </summary>
    public static class RiskAssessmentError
    {
        public static Error NullOrEmpty => new Error("RiskAssessment.NullOrEmpty", "The Risk Assessment is required.");
        public static Error HazardCodeRequired => new Error("RiskAssessment.HazardCodeRequired", "The Hazard Code is required.");
        public static Error InvalidAssessmentType => new Error("RiskAssessment.InvalidAssessmentType", "The Assessment Type is invalid.");
        public static Error InvalidStatus => new Error("RiskAssessment.InvalidStatus", "The Risk Assessment Status is invalid.");
        public static Error NotFound => new Error("RiskAssessment.NotFound", "The Risk Assessment was not found.");
        public static Error CreateFailed => new Error("RiskAssessment.CreateFailed", "Failed to create the Risk Assessment.");
        public static Error UpdateFailed => new Error("RiskAssessment.UpdateFailed", "Failed to update the Risk Assessment.");
        public static Error DeleteFailed => new Error("RiskAssessment.DeleteFailed", "Failed to delete the Risk Assessment.");
    }

    /// <summary>
    /// Contains mitigation-related errors.
    /// </summary>
    public static class MitigationError
    {
        public static Error NullOrEmpty => new Error("Mitigation.NullOrEmpty", "The Mitigation is required.");
        public static Error HazardCodeRequired => new Error("Mitigation.HazardCodeRequired", "The Hazard Code is required.");
        public static Error NotFound => new Error("Mitigation.NotFound", "The Mitigation was not found.");
        public static Error InvalidCode => new Error("Mitigation.InvalidCode", "The Mitigation Code is invalid.");
        public static Error CreateFailed => new Error("Mitigation.CreateFailed", "Failed to create the Mitigation.");
        public static Error UpdateFailed => new Error("Mitigation.UpdateFailed", "Failed to update the Mitigation.");
        public static Error DeleteFailed => new Error("Mitigation.DeleteFailed", "Failed to delete the Mitigation.");
    }

    /// <summary>
    /// Contains mitigation assignment-related errors.
    /// </summary>
    public static class MitigationAssignmentError
    {
        public static Error NullOrEmpty => new Error("MitigationAssignment.NullOrEmpty", "The Mitigation Assignment is required.");
        public static Error MitigationCodeRequired => new Error("MitigationAssignment.MitigationCodeRequired", "The Mitigation Code is required.");
        public static Error DepartmentCodeRequired => new Error("MitigationAssignment.DepartmentCodeRequired", "The Department Code is required.");
        public static Error NotFound => new Error("MitigationAssignment.NotFound", "The Mitigation Assignment was not found.");
        public static Error CreateFailed => new Error("MitigationAssignment.CreateFailed", "Failed to create the Mitigation Assignment.");
        public static Error UpdateFailed => new Error("MitigationAssignment.UpdateFailed", "Failed to update the Mitigation Assignment.");
        public static Error DeleteFailed => new Error("MitigationAssignment.DeleteFailed", "Failed to delete the Mitigation Assignment.");
    }

    /// <summary>
    /// Contains scoring panel-related errors.
    /// </summary>
    public static class ScoringPanelError
    {
        public static Error NullOrEmpty => new Error("ScoringPanel.NullOrEmpty", "The Scoring Panel is required.");
        public static Error HazardCodeRequired => new Error("ScoringPanel.HazardCodeRequired", "The Hazard Code is required.");
        public static Error SMSUserCodeRequired => new Error("ScoringPanel.SMSUserCodeRequired", "The SMS User Code is required.");
        public static Error InvalidLikelihood => new Error("ScoringPanel.InvalidLikelihood", "The Likelihood value is invalid.");
        public static Error InvalidSeverity => new Error("ScoringPanel.InvalidSeverity", "The Severity value is invalid.");
        public static Error InvalidScore => new Error("ScoringPanel.InvalidScore", "The Score value is invalid.");
        public static Error NotFound => new Error("ScoringPanel.NotFound", "The Scoring Panel was not found.");
        public static Error CreateFailed => new Error("ScoringPanel.CreateFailed", "Failed to create the Scoring Panel.");
        public static Error UpdateFailed => new Error("ScoringPanel.UpdateFailed", "Failed to update the Scoring Panel.");
        public static Error DeleteFailed => new Error("ScoringPanel.DeleteFailed", "Failed to delete the Scoring Panel.");
    }

    /// <summary>
    /// Contains report validation-related errors.
    /// </summary>
    public static class ReportValidationError
    {
        public static Error NullOrEmpty => new Error("ReportValidation.NullOrEmpty", "The Report Validation is required.");
        public static Error ReportCodeRequired => new Error("ReportValidation.ReportCodeRequired", "The Report Code is required.");
        public static Error ValidationDecisionRequired => new Error("ReportValidation.ValidationDecisionRequired", "The Validation Decision is required.");
        public static Error InvalidStatus => new Error("ReportValidation.InvalidStatus", "The Validation Status is invalid.");
        public static Error NotFound => new Error("ReportValidation.NotFound", "The Report Validation was not found.");
        public static Error CreateFailed => new Error("ReportValidation.CreateFailed", "Failed to create the Report Validation.");
        public static Error UpdateFailed => new Error("ReportValidation.UpdateFailed", "Failed to update the Report Validation.");
        public static Error DeleteFailed => new Error("ReportValidation.DeleteFailed", "Failed to delete the Report Validation.");
    }

    // Training/Course-related errors (commented out as they don't relate to SMS POCO classes)
    /*
    public static class MachineError
    {
        public static Error CurrentStateNullOrEmpty => new Error("Machine.CurrentStateNullOrEmpty", "The Current State is required.");
    }

    public static class TraineeError
    {
        public static Error NullOrEmpty => new Error("Trainee.NullOrEmpty", "The Trainee is required.");
    }

    public static class TrainingStationError
    {
        public static Error NullOrEmpty => new Error("TrainingStation.NullOrEmpty", "The Training Station can not be null.");
    }

    public static class TrainingSessionError
    {
        public static Error NullOrEmpty => new Error("TrainingSession.NullOrEmpty", "The Training Session can not be null.");
    }

    public static class CourseError
    {
        public static Error NullOrEmpty => new Error("Course.NullOrEmpty", "The Course is required.");
        public static Error NoLessonsFound => new Error("Course.NoLessonsFound", "The course must have at least one lesson.");
        public static Error InValidCourse => new Error("Course.InValidCourse", "Cannot start CourseMachine without a valid Course with lessons.");
        public static Error CourseCheck => new Error("Course.CourseCheck", "Course Check Error");
    }

    public static class LessonError
    {
        public static Error NullOrEmpty => new Error("Lesson.NullOrEmpty", "The Course is required.");
        public static Error NoLessonPagesFound => new Error("Lesson.NoLessonPagesFound", "The Lesson must have at least one LessonPage.");
        public static Error InValidLesson => new Error("Lesson.InValidLesson", "Cannot start LessonMachine without a valid Lesson with LessonPages.");
    }

    public static class LessonQuizError
    {
        public static Error NullOrEmpty => new Error("LessonQuiz.NullOrEmpty", "The Lesson Quiz is required.");
        public static Error NoQuestionPoolsFound => new Error("LessonQuiz.NoQuestionPoolsFound", "The Lesson Quiz must have at least one QuestionPool.");
        public static Error AnswerNotRecorded => new Error("LessonQuiz.AnswerNotRecorded", "The Answer was not recorded.");
    }

    public static class TrainingLogEntryError
    {
        public static Error NullOrEmptyParam => new Error("TrainingLogEntry.NullOrEmptyParam", "The Training Log Params are required.");
        public static Error CourseCompletion => new Error("TrainingLogEntry.CourseCompletion", "The Course Completion was not recorded.");
        public static Error AddTrainingLogEntry => new Error("TrainingLogEntry.AddTrainingLogEntry", "The Add Training Log failed.");
        public static Error DeleteTrainingLog => new Error("TrainingLogEntry.DeleteTrainingLog", "The Delete Training Log failed.");
    }
    */

    /// <summary>
    /// Contains the notification errors.
    /// </summary>
    public static class NotificationError
    {
        public static Error AlreadySent => new Error("Notification.AlreadySent", "The notification has already been sent.");
    }

    /// <summary>
    /// Contains the name errors.
    /// </summary>
    public static class NameError
    {
        public static Error NullOrEmpty => new Error("Name.NullOrEmpty", "The name is required.");
        public static Error LongerThanAllowed => new Error("Name.LongerThanAllowed", "The name is longer than allowed.");
    }

    // Personal information errors (commented out as they don't relate to SMS POCO classes)
    
    public static class FirstNameError
    {
        public static Error NullOrEmpty => new Error("FirstName.NullOrEmpty", "The first name is required.");
        public static Error LongerThanAllowed => new Error("FirstName.LongerThanAllowed", "The first name is longer than allowed.");
        public static Error ContainsSpecialCharactersOrNumbers => new Error("FirstName.ContainsSpecialCharactersOrNumbers", "The first name must not contain special characters or numeric values");
    }

    public static class LastNameError
    {
        public static Error NullOrEmpty => new Error("LastName.NullOrEmpty", "The last name is required.");
        public static Error LongerThanAllowed => new Error("LastName.LongerThanAllowed", "The last name is longer than allowed.");
        public static Error ContainsSpecialCharactersOrNumbers => new Error("LastName.ContainsSpecialCharactersOrNumbers", "The last name must not contain special characters or numeric values");
    }
    /*
    public static class UPIDError
    {
        public static Error NullOrEmpty => new Error("UPID.NullOrEmpty", "The UPID is required.");
        public static Error NonNumericCharacters => new Error("UPID.NonNumericCharacters", "Non numeric characters are not allowed");
        public static Error RequiredLength => new Error("UPID.RequiredLength", "The UPID is not of the required length.");
        public static Error OutOfAllowedRange => new Error("UPID.OutOfAllowedRange", "The UPID is not within the allowed range.");
        public static Error InvalidUPID => new Error("UPID.InvalidUPID", "The UPID is invalid.");
        public static Error MismatchUPID => new Error("UPID.MismatchUPID", "The UPIDs provided do not match.");
    }

    public static class YearOfBirthError
    {
        public static Error NullOrEmpty => new Error("YearOfBirth.NullOrEmpty", "The YearOfBirth is required.");
        public static Error NonNumericCharacters => new Error("YearOfBirth.NonNumericCharacters", "Non numeric characters are not allowed");
        public static Error RequiredLength => new Error("YearOfBirth.RequiredLength", "The YearOfBirth is not of the required length.");
        public static Error OutOfRange => new Error("YearOfBirth.OutOfRange", "The YearOfBirth is out of the allowable range.");
        public static Error MismatchYearOfBirth => new Error("YearOfBirth.MismatchYearOfBirth", "The Year of Birth provided does not match.");
    }

    public static class URLError
    {
        public static Error NullOrEmpty => new Error("URLError.NullOrEmpty", "The URL is required.");
        public static Error NumericCharacters => new Error("URLError.NumericCharacters", "Numeric characters are not allowed");
        public static Error InValid => new Error("URLError.InValid", "The URL is invalid.");
    }

    public static class SubscribeToEmailNewsletterError
    {
        public static Error NullOrEmpty => new Error("SubscribeToEmailNewsletterError.NullOrEmpty", "The Subscribe To EmailNewsletter option is required.");
        public static Error InValid => new Error("SubscribeToEmailNewsletterError.InValid", "The Subscribe To Email Newsletter option is invalid.");
    }

    public static class SubscribeToTextNewsletterError
    {
        public static Error NullOrEmpty => new Error("SubscribeToTextNewsletterError.NullOrEmpty", "The Subscribe To Text Newsletter option is required.");
        public static Error InValid => new Error("SubscribeToTextNewsletterError.InValid", "The Subscribe To Text Newsletter option is invalid.");
    }

    public static class SubscribeToOperationalTextsError
    {
        public static Error NullOrEmpty => new Error("SubscribeToOperationalTextsError.NullOrEmpty", "The Subscribe To Operational Texts option is required.");
        public static Error InValid => new Error("SubscribeToOperationalTextsError.InValid", "The Subscribe To Operational Texts option is invalid.");
    }
    */

    /// <summary>
    /// Contains general errors.
    /// </summary>
    public static class GeneralError
    {
        public static Error UnProcessableRequest => new Error(
            "General.UnProcessableRequest",
            "The server could not process the request.");

        public static Error ServerError => new Error("General.ServerError", "The server encountered an unrecoverable error.");
    }
}