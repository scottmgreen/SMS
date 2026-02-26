//-----------------------------------------------------------------------
// <copyright file="InvestigationID.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Strongly-typed identifier for investigation entities ensuring type safety.
//                  Domain service contract defining business operations
//                  and ensuring clean architecture boundaries.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Domain.Entities;

public sealed class InvestigationID : BaseID<string>
{
    public InvestigationID(string id) : base(id) { }
}

