namespace CBT3_Application.Interfaces;

public interface IBaseState
{

    void HandleState(IBaseEntity cbt_entity);
    void ExitState(bool completed);
    void EnterState(IBaseMachine machine);


}
