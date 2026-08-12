//-----------------------------------------------------------------------
// <copyright file="EventQueueID.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Strongly-typed identifier for queued event entities.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Domain.Entities;

public sealed class EventQueueID : BaseID<string>
{
    public EventQueueID(string id) : base(id)
    {
    }
}