//-----------------------------------------------------------------------
// <copyright file="IRiskAnalysisService.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Application service interface for SMS risk analysis management.
//                  Provides business logic operations for risk analysis entities.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Application.Interfaces;

/// <summary>
/// Application service interface for Risk Analysis management and business operations
/// </summary>
public interface IRiskAnalysisService
{
    /// <summary>
    /// Creates a new risk analysis
    /// </summary>
    Task<Result<RiskAnalysis>> CreateRiskAnalysisAsync(RiskAnalysis analysis, CancellationToken ct = default);

    /// <summary>
    /// Gets risk analysis by ID
    /// </summary>
    Task<Result<RiskAnalysis>> GetRiskAnalysisByIdAsync(RiskAnalysisID id, CancellationToken ct = default);

    /// <summary>
    /// Gets all risk analyses for a specific hazard
    /// </summary>
    Task<Result<RiskAnalysis>> GetRiskAnalysisByHazardCodeAsync(string hazardCode, CancellationToken ct = default);

    /// <summary>
    /// Updates an existing risk analysis
    /// </summary>
    Task<Result<RiskAnalysis>> UpdateRiskAnalysisAsync(RiskAnalysis analysis, CancellationToken ct = default);

    /// <summary>
    /// Deletes a risk analysis
    /// </summary>
    Task<Result<bool>> DeleteRiskAnalysisAsync(RiskAnalysisID id, CancellationToken ct = default);

    /// <summary>
    /// Performs comprehensive risk analysis calculation
    /// </summary>
    Task<Result<RiskAnalysisResult>> PerformRiskAnalysisAsync(RiskAnalysisParameters parameters, CancellationToken ct = default);
}

/// <summary>
/// Parameters for risk analysis calculation
/// </summary>
public class RiskAnalysisParameters
{
    public string HazardCode { get; set; } = string.Empty;
    public int Severity { get; set; }
    public int Likelihood { get; set; }
    public string AnalysisType { get; set; } = string.Empty;
}

/// <summary>
/// Result of risk analysis calculation
/// </summary>
public class RiskAnalysisResult
{
    public string RiskLevel { get; set; } = string.Empty;
    public decimal RiskScore { get; set; }
    public string Recommendations { get; set; } = string.Empty;
    public DateTime AnalyzedDate { get; set; } = DateTime.UtcNow;
}