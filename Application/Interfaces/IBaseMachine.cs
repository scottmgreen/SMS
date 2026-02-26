//-----------------------------------------------------------------------
// <copyright file="IBaseMachine.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Interface contract defining operations and ensuring clean architecture boundaries.
//                  Defines contract for application services ensuring clean architecture
//                  boundaries and dependency inversion compliance.
// </copyright>
//-----------------------------------------------------------------------

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

