//-----------------------------------------------------------------------
// <copyright file="CourseFailState.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: State management component for workflow and business process coordination.
//                  Implements state management for workflow and business process
//                  coordination within the Application layer.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Application.States;

//public class CourseFailState : BaseState
//{
//    private LessonPage _lessonpage;
//    private Lesson _lesson;
//    private LessonPageMachine _parentMachine;

//    public CourseFailState(IMediator mediator, IMessenger messenger, LessonPage lessonpage, Lesson lesson, LessonPageMachine parentMachine) : base(mediator, messenger)
//    {
//        _parentMachine = parentMachine;
//        _lesson = lesson;
//        _lessonpage = lessonpage;
//    }
//    public override void HandleState(IBaseEntity cbt_entity)
//    {
//        Messenger.Publish(new LessonPageEvent(DateTime.Now, _lessonpage, $"{_lessonpage.Id} {_lessonpage.LessonPageType}"));
//        ExitState(true);
//    }

//    public override void ExitState(bool completed)
//    {
//        _parentMachine.LessonFail = true;
//        _parentMachine.MachinePause(string.Empty);
//    }

//    public override void EnterState(IBaseMachine machine)
//    {
//        _parentMachine = (LessonPageMachine)machine;
//    }


//}







