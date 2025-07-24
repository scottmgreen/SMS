//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="">
//     Author: Scott Green
//     Copyright (c) . All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using CBT3_Domain.Entities;

using CBT3_UI;

namespace CBT3_ConsoleApp;

public static class CBT3_EventHandlers
{
    public static IMessenger Messenger = CBT3_Application.Configuration.DependencyInjection.ServiceProvider.GetRequiredService<IMessenger>();
    public static IMediator Mediator = CBT3_Application.Configuration.DependencyInjection.ServiceProvider.GetRequiredService<IMediator>();

    public static void SubscribeToEvents()
    {
        Messenger.Subscribe<CourseStateEvent>(HandleCourseStateEvent);
        Messenger.Subscribe<LessonPageEvent>(HandleLessonPageEvent);
        Messenger.Subscribe<MachinePauseEvent>(HandleMachinePauseEvent);
        Messenger.Subscribe<LessonQuizStartedEvent>(HandleLessonQuizStartedEvent);
        Messenger.Subscribe<LessonQuizFinishedEvent>(HandleLessonQuizFinishedEvent);
    }
    public static void UnsubscribeToEvents()
    {
        Messenger.Unsubscribe<CourseStateEvent>(HandleCourseStateEvent);
        Messenger.Unsubscribe<LessonPageEvent>(HandleLessonPageEvent);
        Messenger.Unsubscribe<MachinePauseEvent>(HandleMachinePauseEvent);
        Messenger.Unsubscribe<LessonQuizStartedEvent>(HandleLessonQuizStartedEvent);
        Messenger.Unsubscribe<LessonQuizFinishedEvent>(HandleLessonQuizFinishedEvent);
    }
    static void HandleAskQuestionEvent(AskQuestionEvent @event)
    {
        //Lesson Quiz interaction for Question and Answers is Handled through the EventMessaging Bus//

        Question question = (Question)@event.Question;


        if (question.QuestionType == QuestionType.TrueFalse)
        {
            while (true)
            {
                string userInput = AnsiConsole.Ask<string>($"[lime]{question.QuestionText}[/] [yellow](T or F)[/] [lime] ? [/]");
                userInput = userInput.Trim().ToLower();

                //CBT3_ConsoleHelper.EraseLine();

                if (userInput == "t" || userInput == "f")
                {
                    string answerText = userInput == "t" ? "True" : "False";

                    Answer selectedAnswer = question.Answers.FirstOrDefault(a => a.AnswerText.Equals(answerText, StringComparison.OrdinalIgnoreCase));
                    selectedAnswer.IsSelected = true;

                    if (selectedAnswer != null)
                    {
                        //submit to service using messaging bus
                        Messenger.Publish(
                            new TFQuestionAnsweredEvent(
                                DateTime.Now,
                                selectedAnswer,
                                QuizState.QuestionAnswered,
                                $"{selectedAnswer.AnswerText}"));
                        break;
                    }
                    else
                    {
                        Console.WriteLine(string.Empty);
                        Console.ForegroundColor = ConsoleColor.Red;
                        int screenWidth = Console.WindowWidth;
                        Console.WriteLine(new string('-', screenWidth));
                        string message = $"Please enter valid options. either 'T' or 'F':\")";
                        Console.WriteLine(message);
                        Console.WriteLine(new string('-', screenWidth));
                        Console.ResetColor();
                        Console.WriteLine(string.Empty);
                    }
                }
                else
                {
                    Console.WriteLine(string.Empty);
                    Console.ForegroundColor = ConsoleColor.Red;
                    int screenWidth = Console.WindowWidth;
                    Console.WriteLine(new string('-', screenWidth));
                    string message = $"Please enter valid options. either 'T' or 'F':\")";
                    Console.WriteLine(message);
                    Console.WriteLine(new string('-', screenWidth));
                    Console.ResetColor();
                    Console.WriteLine(string.Empty);
                }
            }
        }
        else if (question.QuestionType == QuestionType.MultipleChoice)
        {
            while (true)
            {
                Console.WriteLine(question.QuestionText);
                Console.WriteLine("Choose the correct option(s) by entering the corresponding number(s):");

                // Print each option with numbering
                for (int i = 0; i < question.Answers.Count; i++)
                {
                    Console.WriteLine($"{i + 1}) {question.Answers[i].AnswerText}");
                }

                Console.WriteLine("Enter your answers (comma-separated, e.g., 1,3,4):");
                string userInput = Console.ReadLine();
               
                //CBT3_ConsoleHelper.EraseLine();

                string[] selectedOptions = userInput.Split(',');
                List<Answer> selectedAnswers = new List<Answer>();
                bool invalidOption = false;
                foreach (string option in selectedOptions)
                {
                    if (int.TryParse(option, out int selectedOption) &&
                        selectedOption > 0 &&
                        selectedOption <= question.Answers.Count)
                    {
                        selectedAnswers.Add(question.Answers[selectedOption - 1]);
                    }
                    else
                    {
                        Console.WriteLine($"Invalid option '{option}'. Ignored.");
                        invalidOption = true;
                    }
                }
                if (invalidOption)
                {
                    // If any invalid option was encountered, ask the user to re-enter their choices
                    Console.WriteLine(string.Empty);
                    Console.ForegroundColor = ConsoleColor.Red;
                    int screenWidth = Console.WindowWidth;
                    Console.WriteLine(new string('-', screenWidth));
                    string message = $"Please enter valid options. (comma-separated, e.g., 1,3,4):\")";
                    Console.WriteLine(message);
                    Console.WriteLine(new string('-', screenWidth));
                    Console.ResetColor();
                    Console.WriteLine(string.Empty);

                    continue; // Restart the loop to prompt the user again
                }
                selectedAnswers.ForEach(x => x.IsSelected = true); //need to set it as selected;
                                                                   //submit to service using messaging bus
                Messenger.Publish(
                    new MCQuestionAnsweredEvent(
                        DateTime.Now,
                        selectedAnswers,
                        QuizState.QuestionAnswered,
                        string.Empty));

                break;
            }
        }
    }

    static void HandleCourseStateEvent(CourseStateEvent @event)
    {
        //Console.WriteLine($"=>{@event.Text} ");
        //If the Course State Event is "CourseFinished", this requires 
        //Using the CQRS / MEDIATOR bus to send a CourseCompletionCommand
        //This command will fire off the _= await _dataService.CompleteCourseAsync(trainee, course, coursepass);
        //Stored proc to record the results and writeout the xml etc.

        //bool coursepass = @event.CoursePass;
        CBT3_App.CoursePass = @event.CoursePass;
        if (@event.State.Equals(CourseState.CourseFinished))
        {
            TraineeID traineeId = new(CBT3_App.Trainee.Id.Value);
            CourseID courseId = new(CBT3_App.Course.Id.Value);
            TrainingSession session = new(CBT3_App.)
            CourseCompletionCommand request = new CourseCompletionCommand(traineeId, courseId, CBT3_App.CoursePass);

            var result = Mediator.SendAsync<Result<bool>>(request,default).Result;

            if (result.IsSuccess)
            {
                Console.WriteLine($"=>{@event.Text} ");
            }

            
            


        }
    }

    static void HandleLessonPageEvent(LessonPageEvent @event)
    {

        //This determines what page strategy to use//Text/Video//Custom Page//Quiz//
        PageType pt = @event.Page.LessonPageType;
        LessonPage lessonPage = @event.Page;

        //if (pt.Equals(PageType.PT_BASE_01))
        //{
        //    Console.WriteLine($"=> {lessonPage.PageText} ");
        //}
        //else if (pt.Equals(PageType.PT_BASE_02))
        //{
        //    Console.WriteLine($"=> {lessonPage.PageText} ");
        //}
        //else if (pt.Equals(PageType.PT_BASE_03))
        //{
        //    Console.WriteLine($"=> {lessonPage.PageText} ");
        //}
        //if (pt.Equals(PageType.PT_BASE_06)) //quiz
        //{
        //    Console.WriteLine($"=>{@event.Text} {lessonPage.PageOrder}  {lessonPage.LessonPageSubType} ");
        //    return;
        //}

        if (pt.Equals(PageType.PT_BASE_05))
        {
            Console.WriteLine($"=>{@event.Text} {lessonPage.PageOrder} {lessonPage.LessonPageSubType} {lessonPage.VideoURL} ");
            return;
        }
        //else if (pt.Equals(PageType.PT_BASE_07))
        //{
        //    Console.WriteLine($"=> {lessonPage.PageText} ");
        //}
        //else if (pt.Equals(PageType.PT_BASE_08))
        //{
        //    Console.WriteLine($"=> {lessonPage.PageText} ");
        //}



        Console.WriteLine($"=>{@event.Text} {lessonPage.PageOrder} {lessonPage.LessonPageSubType} ");
        //Console.WriteLine($"=> {page.PageText} ");
    }

    static void HandleMachinePauseEvent(MachinePauseEvent @event)
    {
        //The nested Machines (Course=> Lesson=> LessonPage) requires evalutaing which machine raises the 
        //Machine Pause event//
        Console.ReadLine();
        MachineState state = @event.State;
        // Console.WriteLine(state.Value);
        if (state.Equals(MachineState.CourseMachinePaused))
        {
            MachineResumeCommand request = new MachineResumeCommand(CBT3_App.CourseMachineSvc);
            _ = Mediator.SendAsync(request, default);
        }
        if (state.Equals(MachineState.LessonMachinePaused))
        {
            MachineResumeCommand request = new MachineResumeCommand(CBT3_App.CourseMachineSvc.ChildMachine);
            _ = Mediator.SendAsync(request, default);
        }
        if (state.Equals(MachineState.LessonPageMachinePaused))
        {
            MachineResumeCommand request = new MachineResumeCommand(CBT3_App.CourseMachineSvc.ChildMachine.ChildMachine);
            _ = Mediator.SendAsync(request, default);
        }
    }

    static void HandleLessonQuizFinishedEvent(LessonQuizFinishedEvent @event)
    {
        bool isFail = @event.IsFail;

        if (isFail)
        {
            //_isQuizFail = true;
            Console.ForegroundColor = ConsoleColor.Red;
            int screenWidth = Console.WindowWidth;
            Console.WriteLine(new string('-', screenWidth));
            string message = $"Quiz failed! You exceeded the maximum attempts allowed.";
            Console.WriteLine(message);
            Console.WriteLine(new string('-', screenWidth));
            Console.ResetColor();
        }
        else
        {
            //_isQuizFail = false;
            Console.ForegroundColor = ConsoleColor.Blue;
            int screenWidth = Console.WindowWidth;
            Console.WriteLine(new string('-', screenWidth));
            string message = $"Congratulations! Quiz completed successfully.";
            Console.WriteLine(message);
            Console.WriteLine(new string('-', screenWidth));
            Console.ResetColor();
        }

        Messenger.Unsubscribe<AskQuestionEvent>(HandleAskQuestionEvent);
    }

    static void HandleLessonQuizStartedEvent(LessonQuizStartedEvent @event)
    {
        //Quiz Started State Raises the QuizState.QuizStarted event
        //The client needs to subscribe to the "HandleAskQuestionEvents"

        if (@event.State.Equals(QuizState.QuizStarted))
        {
            Messenger.Subscribe<AskQuestionEvent>(HandleAskQuestionEvent);
        }
        Console.WriteLine($"=>{@event.Text} ");
    }
}
