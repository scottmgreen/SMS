

using SMS_Domain.Enums;

namespace SMS3.Components.Shared
{
    /// <summary>
    /// Centralized aviation risk matrix calculations - ONE SOURCE OF TRUTH
    /// CLEANED UP: Removed all duplicate methods and inconsistencies
    /// </summary>
    public static class AviationRiskMatrixCalculator
    {
        #region Core Matrix Methods (Keep These)

        /// <summary>
        /// Generate aviation matrix code from severity and likelihood values (e.g., "3C", "5A")
        /// </summary>
        /// <param name="severity">Severity value (1-5)</param>
        /// <param name="likelihood">Likelihood value (1-5)</param>
        /// <returns>Matrix code like "3C", "5A", etc.</returns>
        public static string GetMatrixCode(int severity, int likelihood)
        {
            var likelihoodLetter = likelihood switch
            {
                1 => "A", // Rare
                2 => "B", // Unlikely
                3 => "C", // Possible
                4 => "D", // Likely
                5 => "E", // Frequent
                _ => "?"
            };
            return $"{severity}{likelihoodLetter}";
        }

        /// <summary>
        /// Calculate matrix code from average severity and likelihood
        /// THIS IS THE AUTHORITATIVE METHOD for hazard averages
        /// </summary>
        /// <param name="averageSeverity">Average severity from panels</param>
        /// <param name="averageLikelihood">Average likelihood from panels</param>
        /// <returns>Matrix code based on rounded averages</returns>
        public static string GetAverageMatrixCode(double averageSeverity, double averageLikelihood)
        {
            var roundedSeverity = (int)Math.Round(averageSeverity);
            var roundedLikelihood = (int)Math.Round(averageLikelihood);
            return GetMatrixCode(roundedSeverity, roundedLikelihood);
        }

        /// <summary>
        /// Parse matrix code back to severity and likelihood
        /// </summary>
        public static (int? severity, int? likelihood) ParseMatrixCode(string matrixCode)
        {
            if (string.IsNullOrEmpty(matrixCode) || matrixCode.Length < 2)
                return (null, null);

            var severityPart = matrixCode.Substring(0, matrixCode.Length - 1);
            var likelihoodLetter = matrixCode.Substring(matrixCode.Length - 1);

            if (!int.TryParse(severityPart, out int severity))
                return (null, null);

            var likelihood = likelihoodLetter.ToUpper() switch
            {
                "A" => 1,
                "B" => 2,
                "C" => 3,
                "D" => 4,
                "E" => 5,
                _ => (int?)null
            };
            return (severity, likelihood);
        }

        /// <summary>
        /// Get aviation risk level from severity and likelihood
        /// FIXED: Now properly maps to all 4 risk levels including Critical
        /// </summary>
        public static RiskLevel GetAviationRiskLevel(int severity, int likelihood)
        {
            return (severity, likelihood) switch
            {
                // Critical Risk (Dark Red) - #dc3545 - Highest combinations
                (5, 5) or (5, 4) or (5, 3) or (4, 5) or (4, 4) => RiskLevel.Critical,
                
                // High Risk (Orange) - #fd7e14 - High severity/likelihood combinations
                (3, 5) or (4, 3) or (3, 4) or (2, 5) => RiskLevel.High,
                
                // Medium Risk (Yellow) - #ffc107 - Medium severity/likelihood combinations
                (5, 2) or (5, 1) or (4, 2) or (3, 2) or (3, 3) or (2, 3) or (2, 4) or (1, 5) => RiskLevel.Medium,
                
                // Low Risk (Green) - #28a745 - Low severity/likelihood combinations
                (4, 1) or (3, 1) or (2, 1) or (2, 2) or (1, 1) or (1, 2) or (1, 3) or (1, 4) => RiskLevel.Low,
                
                // Default fallback
                _ => RiskLevel.Low
            };
        }

        /// <summary>
        /// Get the exact hex color for a matrix cell using original aviation matrix colors
        /// </summary>
        public static string GetAviationMatrixColor(int severity, int likelihood)
        {
            return (severity, likelihood) switch
            {
                (5, 1) => "#ffc107",
                (5, 2) => "#fd7e14",
                (5, 3) => "#dc3545",
                (5, 4) => "#dc3545",
                (5, 5) => "#dc3545",
                (4, 1) => "#28a745",
                (4, 2) => "#ffc107",
                (4, 3) => "#fd7e14",
                (4, 4) => "#dc3545",
                (4, 5) => "#dc3545",
                (3, 1) => "#28a745",
                (3, 2) => "#ffc107",
                (3, 3) => "#ffc107",
                (3, 4) => "#fd7e14",
                (3, 5) => "#dc3545",
                (2, 1) => "#28a745",
                (2, 2) => "#28a745",
                (2, 3) => "#ffc107",
                (2, 4) => "#ffc107",
                (2, 5) => "#fd7e14",
                (1, 1) => "#28a745",
                (1, 2) => "#28a745",
                (1, 3) => "#28a745",
                (1, 4) => "#28a745",
                (1, 5) => "#ffc107",
                _ => "#f8f9fa"
            };
        }

        /// <summary>
        /// Determine if a color is light (needs black text) or dark (needs white text)
        /// </summary>
        public static bool IsLightColor(string hexColor)
        {
            if (string.IsNullOrEmpty(hexColor) || !hexColor.StartsWith("#") || hexColor.Length != 7)
                return false;

            try
            {
                var hex = hexColor[1..];
                var r = Convert.ToInt32(hex[0..2], 16);
                var g = Convert.ToInt32(hex[2..4], 16);
                var b = Convert.ToInt32(hex[4..6], 16);
                var luminance = (0.299 * r + 0.587 * g + 0.114 * b) / 255;
                return luminance > 0.5;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region UI Helper Methods

        /// <summary>
        /// Generate CSS style for matrix cell
        /// </summary>
        public static string GetMatrixCellStyle(int severity, int likelihood)
        {
            var backgroundColor = GetAviationMatrixColor(severity, likelihood);
            var textColor = IsLightColor(backgroundColor) ? "#000" : "#fff";
            return $"background: {backgroundColor}; color: {textColor}; display: flex; align-items: center; justify-content: center; padding: 4px;";
        }

        /// <summary>
        /// Generate styled span CSS for score displays
        /// </summary>
        public static string GetScoreDisplayStyle(int severity, int likelihood, bool isPreview = false)
        {
            var backgroundColor = GetAviationMatrixColor(severity, likelihood);
            var textColor = IsLightColor(backgroundColor) ? "#000" : "#fff";
            var border = isPreview ?
                "border: 2px dashed rgba(0,0,0,0.3); opacity: 0.8;" :
                "border: 1px solid rgba(0,0,0,0.2);";

            return $"background: {backgroundColor}; color: {textColor}; padding: 4px 8px; border-radius: 4px; font-weight: bold; font-size: 0.9rem; display: inline-block; text-align: center; min-width: 30px; {border}";
        }

        #endregion

        #region Panel-Based Calculations (The ONE TRUE METHOD)

        /// <summary>
        /// ?? THE ONE TRUE METHOD - Calculate all hazard risk data from scoring panels
        /// This replaces all the scattered calculation methods throughout the codebase
        /// </summary>
        /// <param name="scoringPanels">Collection of scoring panels</param>
        /// <param name="useResidual">True for Step 5 (residual), False for Step 4 (initial)</param>
        /// <returns>Complete risk calculation results</returns>
        public static HazardRiskCalculation CalculateHazardRisk(IEnumerable<ScoringPanel> scoringPanels, bool useResidual = false)
        {
            var completedPanels = scoringPanels.Where(p => HasValidScore(p, useResidual)).ToList();
            
            if (!completedPanels.Any())
            {
                return HazardRiskCalculation.Empty;
            }

            // Get severity and likelihood values based on step context
            var severityValues = completedPanels.Select(p => 
                useResidual ? p.ResidualSeverity!.Value : p.InitialSeverity!.Value);
            var likelihoodValues = completedPanels.Select(p => 
                useResidual ? p.ResidualLikelihood!.Value : p.InitialLikelihood!.Value);
            var scoreValues = completedPanels.Select(p => 
                useResidual ? p.ResidualScore!.Value : p.InitialScore!.Value);

            // Aviation standard: Average severity and likelihood separately
            var averageSeverity = severityValues.Average();
            var averageLikelihood = likelihoodValues.Average();
            var averageScore = scoreValues.Average();

            // Round to get final values for matrix calculation
            var roundedSeverity = (int)Math.Round(averageSeverity);
            var roundedLikelihood = (int)Math.Round(averageLikelihood);

            // Generate matrix code and determine risk level
            var matrixCode = GetMatrixCode(roundedSeverity, roundedLikelihood);
            var riskLevel = GetAviationRiskLevel(roundedSeverity, roundedLikelihood);
            var backgroundColor = GetAviationMatrixColor(roundedSeverity, roundedLikelihood);
            var textColor = IsLightColor(backgroundColor) ? "#000000" : "#ffffff";

            return new HazardRiskCalculation
            {
                AverageSeverity = averageSeverity,
                AverageLikelihood = averageLikelihood,
                AverageScore = (decimal)averageScore, // FIX: Cast double to decimal
                RoundedSeverity = roundedSeverity,
                RoundedLikelihood = roundedLikelihood,
                MatrixCode = matrixCode,
                RiskLevel = riskLevel,
                BackgroundColor = backgroundColor,
                TextColor = textColor,
                CompletedPanelCount = completedPanels.Count,
                IsValid = true
            };
        }

        /// <summary>
        /// Calculate matrix code for a single scoring panel
        /// </summary>
        public static string GetPanelMatrixCode(ScoringPanel panel)
        {
            if (!panel.Severity.HasValue || !panel.Likelihood.HasValue)
                return "-";
            return GetMatrixCode(panel.Severity.Value, panel.Likelihood.Value);
        }

        /// <summary>
        /// Check if a scoring panel has valid score data
        /// </summary>
        private static bool HasValidScore(ScoringPanel panel, bool useResidual)
        {
            if (useResidual)
            {
                return panel.ResidualSeverity.HasValue &&
                       panel.ResidualLikelihood.HasValue &&
                       panel.ResidualScore.HasValue;
            }
            else
            {
                return panel.InitialSeverity.HasValue &&
                       panel.InitialLikelihood.HasValue &&
                       panel.InitialScore.HasValue;
            }
        }

        #endregion

        #region Debug Methods (Optional)

        /// <summary>
        /// Debug method to show calculation breakdown
        /// </summary>
        public static string CalculateHazardMatrixCodeWithDebug(string hazardCode, IEnumerable<ScoringPanel> panels, Action<string>? logAction = null)
        {
            var calculation = CalculateHazardRisk(panels, false);

            logAction?.Invoke($"=== MATRIX DEBUG: {hazardCode} ===");
            logAction?.Invoke($"Panel Count: {calculation.CompletedPanelCount}");
            logAction?.Invoke($"Averages: Sev={calculation.AverageSeverity:F2}?{calculation.RoundedSeverity}, Like={calculation.AverageLikelihood:F2}?{calculation.RoundedLikelihood}");
            logAction?.Invoke($"FINAL CODE: {calculation.MatrixCode}");
            logAction?.Invoke("=== END DEBUG ===");

            return calculation.MatrixCode;
        }

        #endregion
    }

    /// <summary>
    /// Complete hazard risk calculation result - replaces all the scattered return types
    /// </summary>
    public class HazardRiskCalculation
    {
        public double AverageSeverity { get; set; }
        public double AverageLikelihood { get; set; }
        public decimal AverageScore { get; set; } // FIX: Changed to decimal to match expected type
        public int RoundedSeverity { get; set; }
        public int RoundedLikelihood { get; set; }
        public string MatrixCode { get; set; } = string.Empty;
        public RiskLevel RiskLevel { get; set; } = RiskLevel.Unkonwn;
        public string BackgroundColor { get; set; } = "#f8f9fa";
        public string TextColor { get; set; } = "#000000";
        public int CompletedPanelCount { get; set; }
        public bool IsValid { get; set; }

        public static HazardRiskCalculation Empty => new()
        {
            MatrixCode = "-",
            RiskLevel = RiskLevel.Unkonwn,
            AverageScore = 0m, // FIX: Use decimal literal
            IsValid = false
        };

        public string GetCssStyle() => $"background: {BackgroundColor}; color: {TextColor};";
    }
}
