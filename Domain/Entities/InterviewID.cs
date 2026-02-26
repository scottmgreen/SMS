//-----------------------------------------------------------------------
// <copyright file="InterviewID.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Strongly-typed identifier for interview entities ensuring type safety.
//                  Domain service contract defining business operations
//                  and ensuring clean architecture boundaries.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Domain.Entities;

public class InterviewID : BaseID<string>
{
    public InterviewID(string id) : base(id) { }
}

