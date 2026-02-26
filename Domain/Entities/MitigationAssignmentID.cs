//-----------------------------------------------------------------------
// <copyright file="MitigationAssignmentID.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Strongly-typed identifier for mitigationassignment entities ensuring type safety.
//                  Immutable value object encapsulating domain concepts with
//                  business logic and validation rules.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Domain.Entities;

public class MitigationAssignmentID : BaseID<string>
{
    public MitigationAssignmentID(string id) : base(id) { }
}

