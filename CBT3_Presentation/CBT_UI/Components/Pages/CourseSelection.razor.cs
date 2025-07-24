using System.Net.NetworkInformation;

using CBT_UI.Components.Pages.Shared;

using CBT3_Application.Interfaces;
using CBT3_Application.Messaging;
using CBT3_Application.Messaging.Queries;
using CBT3_Application.Services;

using CBT3_Domain.Common;
using CBT3_Domain.Entities;
using CBT3_Domain.Errors;

using CBT3_UI;

using Microsoft.AspNetCore.Components;

namespace CBT_UI.Components.Pages;

public partial class CourseSelection : ComponentBase
{
    [CascadingParameter]
    public CascadingAppState AppState { get; set; }
    [Inject]
    protected CBT3_App _cbtApp { get; set; }

    public CourseMachine? CourseMachineSvc { get; set; } = null;

    
    public async Task StartCourseAsync()
    {
        if (_cbtApp.Trainee is not null && _cbtApp.Course is not null)
        {

            /// <summary>
            /// Get the CourseMachine and Initialize CQRS / MEDIATOR
            /// </summary>
            ///
            await InitializeCourseMachine();
        }
        else
        {
            Console.WriteLine("Better go back..");
        }
    }

    
    private async Task InitializeCourseMachine()
    {

        CourseMachineSvc = new CourseMachine(_mediator, _messenger);
        CourseMachineSvc.InitializeMachine(_cbtApp.Trainee, _cbtApp.Course);
        MachineStartCommand request = new MachineStartCommand(CourseMachineSvc);
        var result = await _mediator.SendAsync(request, default);
    }

    private void UpdateToolBarMessage(string message)
    {
        AppState.SetProperty(this, "Course", message);
    }
}
