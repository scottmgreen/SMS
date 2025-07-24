using CBT3_Domain.Entities;
using CBT3_Domain.Interfaces;

namespace CBT3_Infrastructure.Common;



public static class Mappers
{
    public static List<T> LoadCollection<T>(this SqlDataReader reader) where T : new()
    {
        List<T> collection = new List<T>();

        while (reader.Read())
        {
            T entity = reader.CreateEntityFromReader<T>();
            collection.Add(entity);
        }

        return collection;
    }

    private static T CreateEntityFromReader<T>(this SqlDataReader reader) where T : new()
    {
        T entity = new T();

        // Assuming you have a method or logic to populate entity properties from SqlDataReader
        reader.PopulateEntityFromReader(entity);

        return entity;
    }

    private static void PopulateEntityFromReader<T>(this SqlDataReader reader, T entity)
    {
        // Implement logic to map SqlDataReader columns to entity properties
        // This can be done manually or using an Object-Relational Mapper (ORM)
        // For simplicity, assuming properties have the same name as SqlDataReader columns

        for (int i = 0; i < reader.FieldCount; i++)
        {
            string columnName = reader.GetName(i);
            object columnValue = reader.GetValue(i);

            // Assuming entity properties have the same name as SqlDataReader columns
            PropertyInfo property = typeof(T).GetProperty(columnName);
            if (property != null && columnValue != DBNull.Value)
            {
                property.SetValue(entity, columnValue, null);
            }
        }
    }

    public static T GetValue<T>(this IDataReader reader, string columnName)
    {
        ////Id = (int)reader["Id"],
        ////Value1 = (int)reader["Value1"],
        ////Value2 = reader["Value2"].ToString()


        object value = reader[columnName]; // read column value

        return value == DBNull.Value ? default : (T)value;


    }
    public static TrainingStation MapToTrainingStation(SqlDataReader reader)
    {
        TrainingStation trainingMachine = new();
        // Required non-nullable properties
        trainingMachine.HostName = reader[FieldNames.fHostName]?.ToString() ?? throw new InvalidOperationException("HostName cannot be null.");
        trainingMachine.Status = TrainingStationStatus.FromValue(reader.GetValue<string>(FieldNames.fTrainingStationStatus) ?? throw new InvalidOperationException("Status cannot be null."));

        // Nullable value objects
        trainingMachine.SessionID = reader.GetValue<string>(FieldNames.fTrainingSessionID) is { Length: > 0 } sessionId
            ? new TrainingSessionID(sessionId)
            : null;

        trainingMachine.TraineeID = reader.GetValue<string>(FieldNames.fTraineeId) is { Length: > 0 } traineeId
            ? new TraineeID(traineeId)
            : null;

        trainingMachine.CourseID = reader.GetValue<string>(FieldNames.fCourseId) is { Length: > 0 } courseId
            ? new CourseID(courseId)
            : null;

        // Nullable DateTime properties
        trainingMachine.CourseStartedAt = reader.GetValue<DateTime?>(FieldNames.fCourseStartedAt);
        trainingMachine.CourseEndedAt = reader.GetValue<DateTime?>(FieldNames.fCourseEndedAt);

        // Nullable string property
        trainingMachine.CircuitId = reader.GetValue<string>(FieldNames.fCircuitID) ?? string.Empty;
           

        return trainingMachine;
    }

    public static TrainingRecord MapToTrainingRecord(SqlDataReader reader)
    {
        TrainingRecord trainingrecord = new();
        try
        {
            trainingrecord.FirstName = reader.GetValue<string>(FieldNames.fFirstName);
            trainingrecord.LastName = reader.GetValue<string>(FieldNames.fLastName);
            trainingrecord.UPID = Convert.ToInt32(reader.GetValue<string>(FieldNames.fUPID));
            trainingrecord.DYOB = Convert.ToInt32(reader.GetValue<string>(FieldNames.fYearOfBirth));
            trainingrecord.CourseCode = reader.GetValue<string>(FieldNames.fCourseName);
            trainingrecord.WorkstationID = reader.GetValue<string>(FieldNames.fHostName);

            //if (reader.GetValue<string>(FieldNames.fCourseId) != null)
            //{
            //    courseID = new(reader.GetValue<string>(FieldNames.fCourseId));
            //}

            trainingrecord.CoursePassed = reader.GetValue<bool>("fldb_CoursePass");

            trainingrecord.CourseStartDate = reader.GetValue<DateTime>(FieldNames.fCourseStartedAt);
            trainingrecord.CourseEndDate = reader.GetValue<DateTime>(FieldNames.fCourseEndedAt); 


            
        }
        catch
        {

            throw;
        }
        return trainingrecord;

    }








    public static TrainingSession MapToTrainingSession(SqlDataReader reader)
    {
        TrainingSessionID trainingSessionID = new(reader.GetValue<string>(FieldNames.fTrainingSessionID));
        TrainingSession trainingSession = new(trainingSessionID);

        try
        {
            trainingSession.HostName = reader[FieldNames.fHostName].ToString();
            TraineeID traineeID = null;
            if (reader.GetValue<string>(FieldNames.fTraineeId) != null)
            {
                traineeID = new TraineeID(reader.GetValue<string>(FieldNames.fTraineeId));
            }

            trainingSession.TraineeID = traineeID;

            CourseID courseID = null;
            if (reader.GetValue<string>(FieldNames.fCourseId) != null)
            {
                courseID = new(reader.GetValue<string>(FieldNames.fCourseId));
            }

            trainingSession.CourseID = courseID;

            trainingSession.CourseEndedAt = reader[FieldNames.fCourseStartedAt] == DBNull.Value
                    ? (DateTime?)null
                    : (DateTime)reader[FieldNames.fCourseStartedAt];

            trainingSession.CourseEndedAt = reader[FieldNames.fCourseEndedAt] == DBNull.Value
                    ? (DateTime?)null
                    : (DateTime)reader[FieldNames.fCourseEndedAt];


            trainingSession.CreatedBy = "SYSTEM";
            trainingSession.CreatedDate = DateTime.UtcNow;
        }
        catch 
        {

            throw;
        }
        return trainingSession;

    }
    public static Course MapToCourse(SqlDataReader reader)
    {
        CourseID courseID = new(reader.GetValue<string>(FieldNames.fCourseId));
        Course course = new Course(courseID);

        course.CourseName = reader[FieldNames.fCourseName].ToString();
        course.ExpiryMonths = reader.GetValue<int>(FieldNames.fExpiryMonths);
        course.CourseType = CourseType.FromValue(reader[FieldNames.fCourseType].ToString());
        course.CustomerServiceRequirement = reader.GetValue<bool>(FieldNames.fCustomerServiceRequirement);
        course.CreatedBy = "SYSTEM";
        course.CreatedDate = DateTime.UtcNow;
        return course;
    }

    public static Lesson MapToLesson(SqlDataReader reader)
    {
        LessonID lessonID = new(reader.GetValue<string>(FieldNames.fLessonId));
        Lesson lesson = new Lesson(lessonID);

        lesson.CourseID = new(reader.GetValue<string>(FieldNames.fCourseId));
        lesson.LessonName = reader.GetValue<string>(FieldNames.fLessonName);
        lesson.LessonOrder = reader.GetValue<int>(FieldNames.fLessonOrder);
        lesson.LessonPages = new List<LessonPage>();

        lesson.CreatedBy = "SYSTEM";
        lesson.CreatedDate = DateTime.UtcNow;
        return lesson;
    }
    public static LessonPage MapToLessonPage(SqlDataReader reader)
    {
        LessonPageID lessonPageID = new(reader.GetValue<string>(FieldNames.fLessonPageId));
        LessonPage lessonpage = new LessonPage(lessonPageID);
        lessonpage.LessonID = new(reader.GetValue<string>(FieldNames.fLessonId));
        lessonpage.LessonPageType = PageType.FromValue(reader.GetValue<string>(FieldNames.fPageTypeId));
        lessonpage.LessonPageSubType = reader.GetValue<string>(FieldNames.fPageSubTypeId);
        lessonpage.PageOrder = reader.GetValue<int>(FieldNames.fPageOrder);
        lessonpage.PageText = reader.GetValue<string>(FieldNames.fPageText);
        lessonpage.VideoURL = reader.GetValue<string>(FieldNames.fVideoURL);
        lessonpage.AudioURL = reader.GetValue<string>(FieldNames.fAudioURL);
        lessonpage.ImageURL = reader.GetValue<string>(FieldNames.fImageURL);

        return lessonpage;
    }

    public static LessonQuiz MapToLessonQuiz(SqlDataReader reader)
    {
        LessonQuizID lessonquizID = new(reader.GetValue<string>(FieldNames.fLessonQuizId));
        LessonQuiz lessonquiz = new LessonQuiz(lessonquizID);
        lessonquiz.LessonID = new(reader.GetValue<string>(FieldNames.fLessonId));
        lessonquiz.AttemptsAllowed = reader.GetValue<int>(FieldNames.fAttemptsAllowed);
        lessonquiz.CreatedBy = "SYSTEM";
        lessonquiz.CreatedDate = DateTime.UtcNow;
        return lessonquiz;
    }
    public static QuestionPool MapToQuestionPool(SqlDataReader reader)
    {
        QuestionPoolID questionpoolID = new(reader.GetValue<string>(FieldNames.fQuestionPoolId));
        QuestionPool questionpool = new QuestionPool(questionpoolID);
        questionpool.JumpBackStart = reader.GetValue<string>(FieldNames.fJumpBackStart);
        questionpool.JumpBackEnd = reader.GetValue<string>(FieldNames.fJumpBackEnd);
        questionpool.JumpBackFilename = reader.GetValue<string>(FieldNames.fJumpBackFileName);

        return questionpool;
    }
    public static Question MapToQuestion(SqlDataReader reader)
    {
        QuestionID questionID = new(reader.GetValue<string>(FieldNames.fQuestionId));
        Question question = new Question(questionID);
        question.QuestionText = reader.GetValue<string>(FieldNames.fQuestionText).Trim();
        question.QuestionImage = reader.GetValue<string>(FieldNames.fQuestionImage);
        question.QuestionType = QuestionType.FromValue(reader.GetValue<string>(FieldNames.fQuestionType));
        question.QuestionPoolID = new(reader.GetValue<string>(FieldNames.fQuestionPoolId));
        return question;
    }

    public static Answer MapToAnswer(SqlDataReader reader)
    {
        AnswerID answerID = new(reader.GetValue<string>(FieldNames.fAnswerId));
        Answer answer = new Answer(answerID);
        answer.QuestionID = new(reader.GetValue<string>(FieldNames.fQuestionId)); ;
        answer.AnswerText = reader.GetValue<string>(FieldNames.fAnswerText).Trim();
        answer.AnswerImage = reader.GetValue<string>(FieldNames.fAnswerImage);
        answer.IsCorrect = reader.GetValue<bool>(FieldNames.fIsCorrectAnswer);
        return answer;
    }
    public static Trainee MapToTrainee(SqlDataReader reader)
    {
        TraineeID traineeID = new TraineeID(reader.GetValue<string>(FieldNames.fTraineeId));
        Trainee trainee = new Trainee(traineeID);

        trainee.FirstName = FirstName.Create(reader.GetValue<string>(FieldNames.fFirstName)).Value;
        trainee.LastName = LastName.Create(reader.GetValue<string>(FieldNames.fLastName)).Value;
        trainee.UPID = UPID.Create(reader.GetValue<string>(FieldNames.fUPID)).Value;
        trainee.YearOfBirth = YearOfBirth.Create(reader.GetValue<string>(FieldNames.fYearOfBirth)).Value;
        trainee.SubscribeToEmailNewsletter = SubscribeToEmailNewsletter.Create(reader.GetValue<bool?>(FieldNames.fSubscribeToEmailNewsletter)).Value;
        trainee.SubscribeToTextNewsletter = SubscribeToTextNewsletter.Create(reader.GetValue<bool?>(FieldNames.fSubscribeToTextNewsletter)).Value;
        trainee.SubscribeToOperationalTexts = SubscribeToOperationalTexts.Create(reader.GetValue<bool?>(FieldNames.fSubscribeToOperationalTexts)).Value;
        trainee.CreatedBy = reader.GetValue<string>(FieldNames.fCreatedBy);
        trainee.CreatedDate = reader.GetValue<DateTime>(FieldNames.fCreatedDate);

        return trainee;
    }



}
