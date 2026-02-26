//-----------------------------------------------------------------------
// <copyright file="RiskAnalysis.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: SMS risk entity representing riskanalysis for risk assessment and management.
//                  Domain entity implementing business rules and invariants
//                  following Domain-Driven Design principles.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Domain.Entities;

public sealed class RiskAnalysis : BaseAuditableEntity
{
    public RiskAnalysis(RiskAnalysisID id) : base(id, "SYSTEM", DateTime.UtcNow) { }

    public string? Code { get; set; }
    public RiskAnalysisType AssessmentType { get; set; } = RiskAnalysisType.Initial;
    public string? HazardCode { get; set; }
    public string? RiskAssessmentCode { get; set; }
    public string? InitialWorstCredibleOutcome { get; set; }
    public string? InitialRootCause { get; set; }
    public string? InitialAdditionalComments { get; set; }

    public string? ResidualWorstCredibleOutcome { get; set; }
    public string? ResidualRootCause { get; set; }
    public string? ResidualAdditionalComments { get; set; }
}

