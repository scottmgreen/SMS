namespace CBT3_Application.Common;

public abstract class BaseMachine : BaseAggregateRoot, IBaseMachine
{
    public BaseMachine(IMediator mediator, IMessenger messenger)
    {
        Messenger = messenger;
        Mediator = mediator;

    }

    public IMessenger Messenger { get; set; }
    public IMediator Mediator { get; set; }
    public bool IsPaused { get; set; }
    public IBaseState CurrentState { get; set; }
    public IBaseState NextState { get; set; }

    public abstract Result Start();

    public abstract void StateChange();

    public void StateComplete()
    {
        CurrentState.ExitState(true);
    }

    public virtual void MachinePause(string message)
    {
        Console.WriteLine("Machine Is Paused...");
        IsPaused = true;
    }

    public virtual void MachineResume()
    {
        Console.WriteLine("Machine Is Resumining...");
        IsPaused = false;
    }

    public virtual void InitializeMachine()
    {
        //
    }

}
