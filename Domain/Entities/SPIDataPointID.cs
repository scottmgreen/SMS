//-----------------------------------------------------------------------
// <copyright file="SPIDataPointID.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Strongly-typed identifier for spatapoint entities ensuring type safety.
//                  Immutable value object encapsulating domain concepts with
//                  business logic and validation rules.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Domain.Entities;

public class SPIDataPointID : BaseID<string>
{
    public SPIDataPointID(string id) : base(id) { }
}
