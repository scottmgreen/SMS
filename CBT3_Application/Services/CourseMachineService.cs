using System.Collections.Frozen;

using CBT3_Application.Interfaces;

namespace CBT3_Application.Services;


public class CourseMachine : BaseMachine
{

    public LessonMachine ChildMachine;
    public Course Course { get; set; }
    public bool CoursePass { get; set; } = false;
    public Trainee Trainee { get; set; }
    private IBaseState CurrentState { get; set; }
    private IBaseState NextState { get; set; }

    public CourseMachine(IMediator mediator, IMessenger eventaggregator) : base(mediator, eventaggregator) { }

    private void ChildMachineStartCommand()
    {
        CancellationToken ct = new();
        var message = new MachineStartCommand(ChildMachine);
        Mediator.SendAsync(message, ct);
    }

    public Result InitializeMachine(Trainee trainee, Course course)
    {


        if (trainee is not null)
        {
            Trainee = trainee;
        }
        else
        {
            return Result.Failure(DomainErrors.TraineeError.NullOrEmpty);
        }
        if (course is not null)
        {
            Course = course;

        }
        else
        {
            return Result.Failure(DomainErrors.CourseError.NullOrEmpty);
        }
        if (Course.Lessons.Count > 0)
        {
            // Set initial state
            this.CurrentState = new CourseStartState(Mediator, Messenger, Course);
            // Create LessonMachine
            ChildMachine = new LessonMachine(Mediator, Messenger, Course.Lessons, this);
            return Result.Success();
        }
        else
        {
            return Result.Failure(DomainErrors.CourseError.NoLessonsFound);
        }

    }

    public override void MachinePause(string message)
    {
        Messenger.Publish(new MachinePauseEvent(DateTime.Now, Course, MachineState.CourseMachinePaused, $"CourseMachine PAUSE"));
        this.IsPaused = true;
    }
    public override void MachineResume()
    {
        this.IsPaused = false;
        if (!ChildMachine.IsPaused)
        {
            ChildMachineStartCommand();
        }
    }
    public override Result Start()
    {
        try
        {
            if (CurrentState is not null)
            {
                CurrentState.EnterState(this);              // Initialize the state
                CurrentState.HandleState(Course);          // Execute the state// Start the LessonMachine
                return Result.Success();
            }
            else
            {
                return Result.Failure(DomainErrors.MachineError.CurrentStateNullOrEmpty);
            }
        }
        catch (Exception ex)
        {
            return Result.Failure(new Error(ex.Source, ex.Message));
        }
    }
    public override void StateChange()
    {
        if (this.CurrentState is CourseStartState)
        {
            this.NextState = new CourseFinishState(Mediator, Messenger, Course);  // Set the next state to LastCourseState
        }

        if (NextState is not null)
        {
            CurrentState = NextState;
            CurrentState.EnterState(this);    // Initialize the next state
            CurrentState.HandleState(Course); // Execute the next state 
        }
        else
        {
            return;
        }

    }

    public override string ToString()
    {
        return $"{this.GetType().Name}: {this.Course?.Id?.Value.ToString() ?? "No Course ID"}";
    }


    #region Lesson Machine
    public sealed class LessonMachine : BaseMachine
    {
        public LessonPageMachine ChildMachine { get; set; }
        private CourseMachine ParentMachine;

        public Course Course { get; set; }
        public bool LessonFail { get; set; }
        public Trainee Trainee { get; set; }

        private List<Lesson> _lessons = new();
        private List<LessonState> _lessonStates = new();

        private IBaseEntity CurrentEntity { get; set; }
        private IBaseState CurrentState { get; set; }
        private IBaseEntity NextEntity { get; set; }
        private IBaseState NextState { get; set; }

        public LessonMachine(IMediator mediator, IMessenger eventaggregator, List<Lesson> lessons, CourseMachine parentMachine) : base(mediator, eventaggregator)
        {
            InitializeMachine(lessons, parentMachine);

        }

        public void InitializeMachine(List<Lesson> lessons, CourseMachine parentMachine)
        {
            ParentMachine = parentMachine;
            Trainee = ParentMachine.Trainee;
            Course = ParentMachine.Course;
            _lessons = lessons;

            foreach (Lesson lesson in _lessons)
            {
                LessonState baselessonstate = new LessonState(Mediator, Messenger, lesson, this);
                _lessonStates.Add(baselessonstate);
            }

            CurrentState = _lessonStates.FirstOrDefault();
            CurrentEntity = _lessons.FirstOrDefault();
            ChildMachine = _lessonStates.FirstOrDefault().ChildMachine;
        }

        public override void MachinePause(string message)
        {
            Messenger.Publish(new MachinePauseEvent(DateTime.Now, CurrentEntity, MachineState.LessonMachinePaused, $"LessonMachine PAUSE"));
            this.IsPaused = true;

        }
        public override void MachineResume()
        {
            this.IsPaused = false;
            MachineStateChangeCommand();
        }
        public override Result Start()
        {
            try
            {
                CurrentState.EnterState(this);            // Initialize the state
                CurrentState.HandleState(CurrentEntity);  // Execute the state
                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure(new Error(ex.Source, ex.Message));
            }
        }
        public override void StateChange()
        {
            bool containsFailPageType = _lessons.Any(lesson => lesson.LessonPages.Any(page => page.LessonPageType == PageType.PT_BASE_08));//failindex
            int failIndex = 0;
            if (containsFailPageType)
            {
                failIndex = _lessons.FindIndex(lesson => lesson.LessonPages.Any(page => page.LessonPageType == PageType.PT_BASE_08));
            }

            if (!IsPaused)
            {
                if (LessonFail && containsFailPageType)
                {
                    NextState = _lessonStates[failIndex];
                    NextEntity = _lessons[failIndex];
                }
                // Find the index of the current state in the list
                var currentIndex = _lessonStates.IndexOf((LessonState)CurrentState);

                // Check if there is a next state
                if (currentIndex < _lessonStates.Count - 1)
                {

                    // Set the next state to the state at the next index
                    NextState = _lessonStates[currentIndex + 1];
                    NextEntity = _lessons[currentIndex + 1];
                }
                else
                {
                    // Handle end of lessons or transition logic
                    NextState = null; // or set it to an appropriate state
                    NextEntity = null;
                    ParentMachineStateChangeCommand();
                }

                if (NextState != null)
                {

                    CurrentState = NextState;
                    CurrentState.EnterState(this);           // Initialize the next state
                    CurrentState.HandleState(CurrentEntity); // Execute the next state 

                }
            }
            else
            {
                //unpause and set nextstate ==null // this seems to only happen on the end of a quiz at the end of a lesson? 
                IsPaused = false;
                // Handle end of lessons or transition logic
                NextState = null; // or set it to an appropriate state
                NextEntity = null;
                ParentMachineStateChangeCommand();

            }

        }
        private void MachineStateChangeCommand()
        {
            CancellationToken ct = new();
            var message = new MachineStateChangeCommand(this);
            Mediator.SendAsync(message, ct);
        }
        private void ParentMachineStateChangeCommand()
        {
            CancellationToken ct = new();
            var message = new MachineStateChangeCommand(ParentMachine);
            Mediator.SendAsync(message, ct);
        }

        public override string ToString()
        {
            return $"{this.GetType().Name}: {this._lessons.Count().ToString() ?? "No Lessons"}";
        }



        #region Lesson Page Machine
        public sealed class LessonPageMachine : BaseMachine
        {
            public bool LessonFail { get; set; } = false;
            private Course _course;
            private Lesson _lesson;
            private List<LessonPage> _pages = new();
            private List<IBaseState> _pageStates = new();
            private Trainee _trainee;

            private LessonMachine ParentMachine;
            private int _failIndex;

            private IBaseEntity CurrentEntity { get; set; }
            private IBaseState CurrentState { get; set; }
            private IBaseEntity NextEntity { get; set; }
            private IBaseState NextState { get; set; }

            public LessonPageMachine(IMediator mediator, IMessenger eventaggregator) : base(mediator, eventaggregator) { }

            private void HandleStateCompletion()
            {

                // Signal the parent machine
                ParentMachine.CurrentState.ExitState(true);
            }

            public void InitializeMachine(Trainee trainee, Course course, List<LessonPage> pages, Lesson lesson, LessonMachine parentMachine)
            {
                _trainee = trainee;
                _course = course;

                _lesson = lesson;
                _pages = pages;
                ParentMachine = parentMachine;

                if (parentMachine.LessonFail)
                {
                    var les = _course.Lessons.Where(x => x.LessonPages.Any(p => p.LessonPageType == PageType.PT_BASE_08)).FirstOrDefault();
                    _lesson = les;
                    _pages = _lesson.LessonPages;
                }



                bool containsFailPageType = _pages.Any(page => page.LessonPageType == PageType.PT_BASE_08);//failindex
                if (containsFailPageType)
                {
                    _failIndex = _pages.FindIndex(page => page.LessonPageType == PageType.PT_BASE_08);
                }

                _pages = _pages.OrderBy(x => x.PageOrder).ToList();

                foreach (LessonPage lessonpage in _pages)
                {
                    if (lessonpage.LessonPageType.Equals(PageType.PT_BASE_01))
                    {
                        LessonTextState pagestate = new LessonTextState(Mediator, Messenger, lessonpage, _lesson, this);
                        _pageStates.Add(pagestate);
                    }
                    else if (lessonpage.LessonPageType.Equals(PageType.PT_BASE_02))
                    {
                        LessonTextState pagestate = new LessonTextState(Mediator, Messenger, lessonpage, _lesson, this);
                        _pageStates.Add(pagestate);
                    }
                    else if (lessonpage.LessonPageType.Equals(PageType.PT_BASE_03))
                    {
                        LessonTextState pagestate = new LessonTextState(Mediator, Messenger, lessonpage, _lesson, this);
                        _pageStates.Add(pagestate);
                    }
                    else if (lessonpage.LessonPageType.Equals(PageType.PT_BASE_04))
                    {
                        LessonTextState pagestate = new LessonTextState(Mediator, Messenger, lessonpage, _lesson, this);
                        _pageStates.Add(pagestate);
                    }
                    else if (lessonpage.LessonPageType.Equals(PageType.PT_BASE_05))
                    {
                        LessonVideoState pagestate = new LessonVideoState(Mediator, Messenger, lessonpage, _lesson, this);
                        _pageStates.Add(pagestate);
                    }
                    else if (lessonpage.LessonPageType.Equals(PageType.PT_BASE_06)) //quiz
                    {
                        LessonQuizState pagestate = new LessonQuizState(Mediator, Messenger, new LessonQuizService(Mediator, Messenger), _trainee, _course, lessonpage, _lesson.LessonQuiz, this);
                        _pageStates.Add(pagestate);
                    }
                           
                    else if (lessonpage.LessonPageType.Equals(PageType.PT_BASE_07))
                    {
                        CoursePassState pagestate = new CoursePassState(Mediator, Messenger, lessonpage, _lesson, this);
                        _pageStates.Add(pagestate);
                    }
                    else if (lessonpage.LessonPageType.Equals(PageType.PT_BASE_08))
                    {
                        CourseFailState pagestate = new CourseFailState(Mediator, Messenger, lessonpage, _lesson, this);
                        _pageStates.Add(pagestate);
                    }

                    //CUSTOMS
                    else if (lessonpage.LessonPageType.Equals(PageType.PT_CUSTOM_01))//C1200_L3-INCURSION
                    {
                        LessonTextState pagestate = new LessonTextState(Mediator, Messenger, lessonpage, _lesson, this);
                        _pageStates.Add(pagestate);
                    }
                    else if (lessonpage.LessonPageType.Equals(PageType.PT_CUSTOM_02))//C1200_L4-PHONETIC_ALPHABET
                    {
                        LessonTextState pagestate = new LessonTextState(Mediator, Messenger, lessonpage, _lesson, this);
                        _pageStates.Add(pagestate);
                    }
                    else if (lessonpage.LessonPageType.Equals(PageType.PT_CUSTOM_03))//C1200_L5-MOST_DANGEROUS_LOCATIONS
                    {
                        LessonTextState pagestate = new LessonTextState(Mediator, Messenger, lessonpage, _lesson, this);
                        _pageStates.Add(pagestate);
                    }
                    else if (lessonpage.LessonPageType.Equals(PageType.PT_CUSTOM_04))//C1400_L1 --SIGNAGE REMINDER
                    {
                        LessonTextState pagestate = new LessonTextState(Mediator, Messenger, lessonpage, _lesson, this);
                        _pageStates.Add(pagestate);
                    }

                    else if (lessonpage.LessonPageType.Equals(PageType.PT_CUSTOM_05))//C1100_L1-FOREIGN_OBJECT_DEBRIS
                    {
                        LessonTextState pagestate = new LessonTextState(Mediator, Messenger, lessonpage, _lesson, this);
                        _pageStates.Add(pagestate);
                    }
                    else if (lessonpage.LessonPageType.Equals(PageType.PT_CUSTOM_06))//C1100_L2-VEHICLE_ENTERING_RESTRICTED_AREA
                    {
                        LessonTextState pagestate = new LessonTextState(Mediator, Messenger, lessonpage, _lesson, this);
                        _pageStates.Add(pagestate);
                    }
                    else if (lessonpage.LessonPageType.Equals(PageType.PT_CUSTOM_07)) //C1100_L2-AIRCRAFT_READY_TO_MOVE
                    {
                        LessonTextState pagestate = new LessonTextState(Mediator, Messenger, lessonpage, _lesson, this);
                        _pageStates.Add(pagestate);
                    }

                    //else
                    //{
                    //    LessonTextState pagestate = new LessonTextState(Mediator, EventAggregator, lessonpage, _lesson, this);
                    //    _pageStates.Add(pagestate);

                    //}
                }

                if (ParentMachine.LessonFail)
                {
                    CurrentState = _pageStates[_failIndex];
                    CurrentEntity = _pages[_failIndex];
                }
                else
                {
                    CurrentState = _pageStates.FirstOrDefault(); ;
                    CurrentEntity = _pages.FirstOrDefault();
                }

            }
            public override void MachinePause(string message)
            {
                Messenger.Publish(new MachinePauseEvent(DateTime.Now, CurrentEntity, MachineState.LessonPageMachinePaused, $"LessonPageMachine PAUSE"));
                this.IsPaused = true;

            }
            public override void MachineResume()
            {
                this.IsPaused = false;
                StateChange();
            }
            public override Result Start()
            {
                try
                {
                    CurrentState.EnterState(this);            // Initialize the state
                    CurrentState.HandleState(CurrentEntity);  // Execute the state
                    return Result.Success();
                }
                catch (Exception ex)
                {
                    return Result.Failure(new Error(ex.Source, ex.Message));
                }

            }
            public override void StateChange()
            {
                bool containsFailPageType = _pages.Any(page => page.LessonPageType == PageType.PT_BASE_08);//failindex
                if (containsFailPageType)
                {
                    _failIndex = _pages.FindIndex(page => page.LessonPageType == PageType.PT_BASE_08);
                }

                if (!this.IsPaused)
                {
                    var currentIndex = _pageStates.IndexOf(CurrentState);
                    // It's a Fail in a Lesson that does not have a Fail Page//
                    // Need to notify LessonMachine of Fail and movet to lesson that does have failed page 
                    //
                    if (LessonFail && containsFailPageType == false)
                    {
                        ParentMachine.LessonFail = true;
                        ParentMachine.ParentMachine.CoursePass = false;
                        HandleStateCompletion();
                        return;
                    }
                    // It's a Fail in a Lesson that DOES have a Fail Page//
                    if (LessonFail && containsFailPageType == true && CurrentState is not CourseFailState)
                    {
                        ParentMachine.LessonFail = true;
                        ParentMachine.ParentMachine.CoursePass = false;
                        NextState = _pageStates.Where(l => l.GetType() == typeof(CourseFailState)).FirstOrDefault(); //set it to fail state
                        NextEntity = _pages.Where(page => page.LessonPageType == PageType.PT_BASE_08).FirstOrDefault(); //set it to fail page
                        CurrentState = NextState;
                        CurrentEntity = NextEntity;
                        CurrentState.EnterState(this); // Initialize the next state
                        CurrentState.HandleState(CurrentEntity); // Execute the next state 
                        return;
                    }
                    // It's a Fail in a Lesson that DOES have a Fail Page AND we have already displayed the Failed Page => Course END //
                    if (LessonFail && containsFailPageType == true && CurrentState is CourseFailState)
                    {
                        ParentMachine.LessonFail = true;
                        ParentMachine.ParentMachine.CoursePass = false;
                        NextState = null; //set it to fail state
                        NextEntity = null; //set it to fail page
                        HandleStateCompletion();
                        return;
                    }

                    if (CurrentState is CoursePassState)
                    {
                        NextState = null;
                        NextEntity = null;
                        HandleStateCompletion();
                        return;
                    }

                    if (currentIndex == _pages.Count )
                    {
                        NextState = null; // or set it to an appropriate state
                        NextEntity = null;
                        HandleStateCompletion();
                    }
                        // Check if there is a next state
                        
                    if (currentIndex < _pages.Count - 1)
                    {
                        // Set the next state to the state at the next index

                        NextState = _pageStates[currentIndex + 1];
                        NextEntity = _pages[currentIndex + 1];

                        if (NextState is CourseFailState)
                        {
                            ParentMachine.ParentMachine.CoursePass = false;
                        }
                        else if (NextState is CoursePassState)
                        {
                            ParentMachine.ParentMachine.CoursePass = true;
                        }


                    }
                    else
                    {
                        // Handle end of lessons or transition logic
                        NextState = null; // or set it to an appropriate state
                        NextEntity = null;
                        HandleStateCompletion();
                    }



                    if (NextState != null)
                    {
                        CurrentState = NextState;
                        CurrentEntity = NextEntity;
                        CurrentState.EnterState(this); // Initialize the next state
                        CurrentState.HandleState(CurrentEntity); // Execute the next state 
                    }
                }

            }

            public override string ToString()
            {
                return $"{this.GetType().Name}: {this._pages.Count().ToString() ?? "No Pages "}";
            }

        }
        #endregion

    }
    #endregion



}

