namespace SMS_Application.Common;

public abstract class BaseState : IBaseState
{
    public BaseState(IMediator mediator, IMessenger messenger)
    {
        Messenger = messenger;
        Mediator = mediator;
        IsPaused = false;
    }
    public IMessenger Messenger { get; set; }
    public IMediator Mediator { get; set; }
    public bool IsPaused { get; set; }

    public abstract void EnterState(IBaseMachine machine);


    public abstract void ExitState(bool completed);


    public abstract void HandleState(IBaseEntity cbt_entity);


}
