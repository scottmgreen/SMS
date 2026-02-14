namespace SMS3.Components.Shared
{
    /// <summary>
    /// Centralized aviation risk matrix calculations to ensure consistency across all components
    /// </summary>
    public static class AviationRiskMatrixCalculator
    {
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
        /// Calculate matrix code for a single scoring panel
        /// </summary>
        /// <param name="panel">Scoring panel with severity and likelihood values</param>
        /// <returns>Matrix code or "-" if incomplete</returns>
        public static string GetPanelMatrixCode(ScoringPanel panel)
        {
            if (!panel.Severity.HasValue || !panel.Likelihood.HasValue)
                return "-";

            return GetMatrixCode(panel.Severity.Value, panel.Likelihood.Value);
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
        /// Get aviation risk level from severity and likelihood
        /// </summary>
        /// <param name="severity">Severity value (1-5)</param>
        /// <param name="likelihood">Likelihood value (1-5)</param>
        /// <returns>Risk level (High, Medium, Low, Acceptable)</returns>
        //public static string GetAviationRiskLevel(int severity, int likelihood)
        //{
        //    return (severity, likelihood) switch
        //    {
        //        // High Risk (Red)
        //        (5, 3) or (5, 4) or (5, 5) or (4, 4) or (4, 5) or (3, 5) => "High",

        //        // Medium Risk (Orange)  
        //        (5, 2) or (4, 3) or (3, 4) or (2, 5) => "Medium",

        //        // Low Risk (Yellow)
        //        (5, 1) or (4, 2) or (3, 2) or (3, 3) or (2, 3) or (2, 4) or (1, 5) => "Low",

        //        // Acceptable Risk (Green)
        //        (4, 1) or (3, 1) or (2, 1) or (2, 2) or (1, 1) or (1, 2) or (1, 3) or (1, 4) => "Acceptable",

        //        _ => "Unknown"
        //    };
        //}
        /// <summary>
        /// Get aviation risk level from severity and likelihood
        /// </summary>
        /// <param name="severity">Severity value (1-5)</param>
        /// <param name="likelihood">Likelihood value (1-5)</param>
        /// <returns>Risk level enum</returns>
        public static RiskLevel GetAviationRiskLevel(int severity, int likelihood)
        {
            return (severity, likelihood) switch
            {
                // High Risk (Red)
                (5, 3) or (5, 4) or (5, 5) or (4, 4) or (4, 5) or (3, 5) => RiskLevel.High,

                // Medium Risk (Orange)  
                (5, 2) or (4, 3) or (3, 4) or (2, 5) => RiskLevel.Medium,

                // Low Risk (Yellow) - includes what was previously "Acceptable"
                (5, 1) or (4, 2) or (3, 2) or (3, 3) or (2, 3) or (2, 4) or (1, 5) or
                (4, 1) or (3, 1) or (2, 1) or (2, 2) or (1, 1) or (1, 2) or (1, 3) or (1, 4) => RiskLevel.Low,

                _ => RiskLevel.Low // Default fallback instead of "Unknown"
            };
        }
        /// <summary>
        /// Get the exact hex color for a matrix cell
        /// </summary>
        /// <param name="severity">Severity value (1-5)</param>
        /// <param name="likelihood">Likelihood value (1-5)</param>
        /// <returns>Hex color code</returns>
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
        /// <param name="hexColor">Hex color code</param>
        /// <returns>True if light color</returns>
        public static bool IsLightColor(string hexColor)
        {
            if (hexColor.StartsWith("#"))
                hexColor = hexColor[1..];

            var r = Convert.ToInt32(hexColor[0..2], 16);
            var g = Convert.ToInt32(hexColor[2..4], 16);
            var b = Convert.ToInt32(hexColor[4..6], 16);

            var luminance = (0.299 * r + 0.587 * g + 0.114 * b) / 255;

            return luminance > 0.5;
        }

        /// <summary>
        /// Generate CSS style for matrix cell
        /// </summary>
        /// <param name="severity">Severity value (1-5)</param>
        /// <param name="likelihood">Likelihood value (1-5)</param>
        /// <returns>CSS style string</returns>
        public static string GetMatrixCellStyle(int severity, int likelihood)
        {
            var backgroundColor = GetAviationMatrixColor(severity, likelihood);
            return $"background: {backgroundColor}; display: flex; align-items: center; justify-content: center; padding: 4px;";
        }

        /// <summary>
        /// Generate styled span CSS for score displays
        /// </summary>
        /// <param name="severity">Severity value (1-5)</param>
        /// <param name="likelihood">Likelihood value (1-5)</param>
        /// <param name="isPreview">Whether this is a preview (dashed border)</param>
        /// <returns>CSS style string for span elements</returns>
        public static string GetScoreDisplayStyle(int severity, int likelihood, bool isPreview = false)
        {
            var backgroundColor = GetAviationMatrixColor(severity, likelihood);
            var textColor = IsLightColor(backgroundColor) ? "#000" : "#fff";
            var border = isPreview ?
                "border: 2px dashed rgba(0,0,0,0.3); opacity: 0.8;" :
                "border: 1px solid rgba(0,0,0,0.2);";

            return $"background: {backgroundColor}; color: {textColor}; padding: 4px 8px; border-radius: 4px; font-weight: bold; font-size: 0.9rem; display: inline-block; text-align: center; min-width: 30px; {border}";
        }

        /// <summary>
        /// Debug method to show calculation breakdown
        /// </summary>
        /// <param name="hazardCode">Hazard code for logging</param>
        /// <param name="panels">List of completed scoring panels</param>
        /// <returns>Calculated matrix code with detailed logging</returns>
        public static string CalculateHazardMatrixCodeWithDebug(string hazardCode, IEnumerable<ScoringPanel> panels, Action<string> logAction = null)
        {
            var completedPanels = panels.Where(p => p.Severity.HasValue && p.Likelihood.HasValue).ToList();

            if (!completedPanels.Any())
                return "-";

            var avgSeverity = completedPanels.Average(p => p.Severity!.Value);
            var avgLikelihood = completedPanels.Average(p => p.Likelihood!.Value);

            var roundedSeverity = (int)Math.Round(avgSeverity);
            var roundedLikelihood = (int)Math.Round(avgLikelihood);

            var matrixCode = GetMatrixCode(roundedSeverity, roundedLikelihood);

            // Detailed logging
            logAction?.Invoke($"=== MATRIX DEBUG: {hazardCode} ===");
            logAction?.Invoke($"Panel Count: {completedPanels.Count}");

            foreach (var panel in completedPanels)
            {
                var panelCode = GetPanelMatrixCode(panel);
                logAction?.Invoke($"  {panel.SMSUserCode}: Sev={panel.Severity}, Like={panel.Likelihood}, Code={panelCode}");
            }

            logAction?.Invoke($"Averages: Sev={avgSeverity:F2}?{roundedSeverity}, Like={avgLikelihood:F2}?{roundedLikelihood}");
            logAction?.Invoke($"FINAL CODE: {matrixCode}");
            logAction?.Invoke("=== END DEBUG ===");

            return matrixCode;
        }
    }
}