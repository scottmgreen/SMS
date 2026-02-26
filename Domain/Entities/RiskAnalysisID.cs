//-----------------------------------------------------------------------
// <copyright file="RiskAnalysisID.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Strongly-typed identifier for riskanalysis entities ensuring type safety.
//                  Immutable value object encapsulating domain concepts with
//                  business logic and validation rules.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Domain.Entities;

public class RiskAnalysisID : BaseID<string>
{
    public RiskAnalysisID(string id) : base(id) { }
}

