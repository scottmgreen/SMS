namespace CBT3_Domain.Errors;


/// <summary>
/// Contains the domain errors.
/// Many are examples and not applied
/// </summary>
public static class DomainErrors
{
    /// <summary>
    /// Contains the name errors.
    /// </summary>
 public static class SystemError
        {
            public static Error AuditLogEntryError => new Error("SystemError.AuditLogEntryError", " Audit Log Entry Error.");
            public static Error FileExistsError => new Error("SystemError.FileExistsError", " File Does Not Exist Error.");
            public static Error VideoPlaybackError => new Error("SystemError.VideoPlaybackError", "Video Playback Error.");
            public static Error LessonQuizError => new Error("SystemError.LessonQuizError", "Lesson Quiz Error.");
            public static Error CustomPageError => new Error("SystemError.CustomPageError", "Custom Page Error.");

    }
public static class MachineError
        {
            public static Error CurrentStateNullOrEmpty => new Error("Machine.CurrentStateNullOrEmpty", "The CurrentS tate is required.");

        }
public static class TraineeError
        {
            public static Error NullOrEmpty => new Error("Trainee.NullOrEmpty", "The Trainee is required.");



        }

    public static class TraininingStationError
    {
        public static Error NullOrEmpty => new Error("TrainingStation.NullOrEmpty", "The Training Station can not be null.");



    }
    public static class TraininingSessionError
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

        /// <summary>
        /// Contains the first name errors.
        /// </summary>
        public static class FirstNameError
        {
            public static Error NullOrEmpty => new Error("FirstName.NullOrEmpty", "The first name is required.");

            public static Error LongerThanAllowed => new Error("FirstName.LongerThanAllowed", "The first name is longer than allowed.");

            public static Error ContainsSpecialCharactersOrNumbers => new Error("FirstName.ContainsSpecialCharactersOrNumbers", "The first name must not contain special characters or numeric values");
        }

        /// <summary>
        /// Contains the last name errors.
        /// </summary>
        public static class LastNameError
        {
            public static Error NullOrEmpty => new Error("LastName.NullOrEmpty", "The last name is required.");

            public static Error LongerThanAllowed => new Error("LastName.LongerThanAllowed", "The last name is longer than allowed.");

            public static Error ContainsSpecialCharactersOrNumbers => new Error("LastName.ContainsSpecialCharactersOrNumbers", "The last name must not contain special characters or numeric values");
        }
        /// <summary>
        /// Contains the UPID  errors.
        /// </summary>
        public static class UPIDError
        {
            public static Error NullOrEmpty => new Error("UPID.NullOrEmpty", "The UPID is required.");

            public static Error NonNumericCharacters => new Error("UPID.NonNumericCharacters", "Non numeric characters are not allowed");

            public static Error RequiredLength => new Error("UPID.RequiredLength", "The UPID is not of the required length.");

            public static Error OutOfAllowedRange=> new Error("UPID.OutOfAllowedRange", "The UPID is not within the allowed range.");

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
    //EmailNotificationError

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

