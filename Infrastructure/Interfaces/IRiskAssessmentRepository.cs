//-----------------------------------------------------------------------
// <copyright file="IRiskAssessmentRepository.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Repository implementing data access operations for SMS iriskassessment entities supporting risk assessment workflows.
//                  Infrastructure service contract defining data access operations
//                  and external system integration interfaces.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Infrastructure.Interfaces;

/// <summary>
/// Risk Assessment Repository Interface - MISSION CRITICAL
/// Replaces file-based operations with proper SMS Backend repository pattern
/// </summary>
public interface IRiskAssessmentRepository
{
    Task<Result<RiskAssessment>> GetByCodeAsync(RiskAssessmentID id);
    Task<Result<RiskAssessment>> AddAsync(RiskAssessment riskAssessment);
    Task<Result<bool>> UpdateAsync(RiskAssessment riskAssessment);
    Task<Result<bool>> DeleteAsync(RiskAssessmentID id);
    Task<Result<IEnumerable<RiskAssessment>>> GetAllAsync();
    Task<Result<IEnumerable<RiskAssessment>>> GetByLeadAssessorAsync(string leadAssessorId);
    Task<Result<IEnumerable<RiskAssessment>>> GetByStatusAsync(RiskAssessmentStatus status);
    Task<Result<IEnumerable<RiskAssessment>>> GetByHazardIdAsync(string hazardId);
    Task<Result<IEnumerable<RiskAssessment>>> GetActiveAssessmentsAsync();
    Task<Result<IEnumerable<RiskAssessment>>> GetResidualAssessmentsAsync(string parentAssessmentId);
}

