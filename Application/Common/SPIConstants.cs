namespace SMS_Application.Common;

/// <summary>
/// Constants and shared data for Safety Performance Indicators
/// </summary>
public static class SPIConstants
{
    /// <summary>
    /// Standard data source options for SPIs and data points
    /// </summary>
    public static class DataSources
    {
        public const string ManualEntry = "Manual Entry";
        public const string SystemGenerated = "System Generated";
        public const string ExternalImport = "External Import";
        public const string DatabaseQuery = "Database Query";
        public const string ExcelImport = "Excel Import";
        public const string APIIntegration = "API Integration";
        public const string AutomatedCollection = "Automated Collection";
        public const string SurveyData = "Survey Data";
        public const string ThirdPartySystem = "Third Party System";
        public const string LegacySystem = "Legacy System";
        public const string MobileApp = "Mobile App";
        public const string WebPortal = "Web Portal";
        public const string SensorData = "Sensor Data";
        public const string CalculatedValue = "Calculated Value";
        public const string QualityAssurance = "Quality Assurance";
        public const string SafetyReports = "Safety Reports";
        public const string AuditResults = "Audit Results";
        public const string InspectionData = "Inspection Data";

        /// <summary>
        /// Gets all available data source options
        /// </summary>
        public static List<string> GetAll()
        {
            return new List<string>
            {
                ManualEntry,
                SystemGenerated,
                ExternalImport,
                DatabaseQuery,
                ExcelImport,
                APIIntegration,
                AutomatedCollection,
                SurveyData,
                ThirdPartySystem,
                LegacySystem,
                MobileApp,
                WebPortal,
                SensorData,
                CalculatedValue,
                QualityAssurance,
                SafetyReports,
                AuditResults,
                InspectionData
            };
        }

        /// <summary>
        /// Gets default data sources for hazard report workflows
        /// </summary>
        public static List<string> GetHazardReportDefaults()
        {
            return new List<string>
            {
                SafetyReports,
                ManualEntry,
                SystemGenerated,
                ExcelImport
            };
        }

        /// <summary>
        /// Gets automated data sources
        /// </summary>
        public static List<string> GetAutomatedSources()
        {
            return new List<string>
            {
                SystemGenerated,
                DatabaseQuery,
                APIIntegration,
                AutomatedCollection,
                SensorData,
                CalculatedValue
            };
        }
    }

    /// <summary>
    /// Common validation messages for SPIs
    /// </summary>
    public static class ValidationMessages
    {
        public const string NameRequired = "SPI name is required";
        public const string DescriptionRequired = "Description is required";
        public const string TypeRequired = "Indicator type is required";
        public const string DataSourceRequired = "Data source is required";
        public const string ValueRequired = "Value is required";
        public const string MeasurementDateRequired = "Measurement date is required";
        public const string VerifiedByRequired = "Verified by is required when marking as verified";
    }
}