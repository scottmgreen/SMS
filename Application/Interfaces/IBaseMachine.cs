namespace SMS_Application.Interfaces;

public interface IBaseMachine
{
    bool IsPaused { get; set; }
    Result Start();
    void MachinePause(string message);
    void MachineResume();
    void StateChange();
    void StateComplete();

    IBaseState CurrentState { get; set; }
    IBaseState NextState { get; set; }
}
