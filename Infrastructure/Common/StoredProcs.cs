namespace SMS_Infrastructure.Common
{
    public static class StoredProcs
    {
        /// <summary>
        /// System Operations
        /// </summary>
        private static readonly Lazy<string> _cn_spAddAuditLogEntry = new Lazy<string>(() => "sp_AddAuditLogEntry");
        public static string cn_spAddAuditLogEntry => _cn_spAddAuditLogEntry.Value;

        /// <summary>
        /// SMS Hazard CRUD Operations
        /// </summary>
        private static readonly Lazy<string> _pr_Hazard_Insert = new Lazy<string>(() => "pr_Hazard_Insert");
        public static string pr_Hazard_Insert => _pr_Hazard_Insert.Value;

        private static readonly Lazy<string> _pr_Hazard_GetById = new Lazy<string>(() => "pr_Hazard_GetById");
        public static string pr_Hazard_GetById => _pr_Hazard_GetById.Value;

        private static readonly Lazy<string> _pr_Hazard_GetAll = new Lazy<string>(() => "pr_Hazard_GetAll");
        public static string pr_Hazard_GetAll => _pr_Hazard_GetAll.Value;

        private static readonly Lazy<string> _pr_Hazard_Update = new Lazy<string>(() => "pr_Hazard_Update");
        public static string pr_Hazard_Update => _pr_Hazard_Update.Value;

        private static readonly Lazy<string> _pr_Hazard_Delete = new Lazy<string>(() => "pr_Hazard_Delete");
        public static string pr_Hazard_Delete => _pr_Hazard_Delete.Value;

        /// <summary>
        /// SMS Report CRUD Operations
        /// </summary>
        private static readonly Lazy<string> _pr_Report_Insert = new Lazy<string>(() => "pr_Report_Insert");
        public static string pr_Report_Insert => _pr_Report_Insert.Value;

        private static readonly Lazy<string> _pr_Report_GetById = new Lazy<string>(() => "pr_Report_GetById");
        public static string pr_Report_GetById => _pr_Report_GetById.Value;

        private static readonly Lazy<string> _pr_Report_GetAll = new Lazy<string>(() => "pr_Report_GetAll");
        public static string pr_Report_GetAll => _pr_Report_GetAll.Value;

        private static readonly Lazy<string> _pr_Report_Update = new Lazy<string>(() => "pr_Report_Update");
        public static string pr_Report_Update => _pr_Report_Update.Value;

        private static readonly Lazy<string> _pr_Report_Delete = new Lazy<string>(() => "pr_Report_Delete");
        public static string pr_Report_Delete => _pr_Report_Delete.Value;

        /// <summary>
        /// SMS Investigation CRUD Operations
        /// </summary>
        private static readonly Lazy<string> _pr_Investigation_Insert = new Lazy<string>(() => "pr_Investigation_Insert");
        public static string pr_Investigation_Insert => _pr_Investigation_Insert.Value;

        private static readonly Lazy<string> _pr_Investigation_GetById = new Lazy<string>(() => "pr_Investigation_GetById");
        public static string pr_Investigation_GetById => _pr_Investigation_GetById.Value;

        private static readonly Lazy<string> _pr_Investigation_GetAll = new Lazy<string>(() => "pr_Investigation_GetAll");
        public static string pr_Investigation_GetAll => _pr_Investigation_GetAll.Value;

        private static readonly Lazy<string> _pr_Investigation_Update = new Lazy<string>(() => "pr_Investigation_Update");
        public static string pr_Investigation_Update => _pr_Investigation_Update.Value;

        private static readonly Lazy<string> _pr_Investigation_Delete = new Lazy<string>(() => "pr_Investigation_Delete");
        public static string pr_Investigation_Delete => _pr_Investigation_Delete.Value;

        /// <summary>
        /// SMS Interview CRUD Operations
        /// </summary>
        private static readonly Lazy<string> _pr_Interview_Insert = new Lazy<string>(() => "pr_Interview_Insert");
        public static string pr_Interview_Insert => _pr_Interview_Insert.Value;

        private static readonly Lazy<string> _pr_Interview_GetById = new Lazy<string>(() => "pr_Interview_GetById");
        public static string pr_Interview_GetById => _pr_Interview_GetById.Value;

        private static readonly Lazy<string> _pr_Interview_GetAll = new Lazy<string>(() => "pr_Interview_GetAll");
        public static string pr_Interview_GetAll => _pr_Interview_GetAll.Value;

        private static readonly Lazy<string> _pr_Interview_Update = new Lazy<string>(() => "pr_Interview_Update");
        public static string pr_Interview_Update => _pr_Interview_Update.Value;

        private static readonly Lazy<string> _pr_Interview_Delete = new Lazy<string>(() => "pr_Interview_Delete");
        public static string pr_Interview_Delete => _pr_Interview_Delete.Value;

        /// <summary>
        /// SMS Risk Analysis CRUD Operations
        /// </summary>
        private static readonly Lazy<string> _pr_RiskAnalysis_Insert = new Lazy<string>(() => "pr_RiskAnalysis_Insert");
        public static string pr_RiskAnalysis_Insert => _pr_RiskAnalysis_Insert.Value;

        private static readonly Lazy<string> _pr_RiskAnalysis_GetById = new Lazy<string>(() => "pr_RiskAnalysis_GetById");
        public static string pr_RiskAnalysis_GetById => _pr_RiskAnalysis_GetById.Value;

        private static readonly Lazy<string> _pr_RiskAnalysis_GetAll = new Lazy<string>(() => "pr_RiskAnalysis_GetAll");
        public static string pr_RiskAnalysis_GetAll => _pr_RiskAnalysis_GetAll.Value;

        private static readonly Lazy<string> _pr_RiskAnalysis_Update = new Lazy<string>(() => "pr_RiskAnalysis_Update");
        public static string pr_RiskAnalysis_Update => _pr_RiskAnalysis_Update.Value;

        private static readonly Lazy<string> _pr_RiskAnalysis_Delete = new Lazy<string>(() => "pr_RiskAnalysis_Delete");
        public static string pr_RiskAnalysis_Delete => _pr_RiskAnalysis_Delete.Value;

        /// <summary>
        /// SMS Risk Assessment CRUD Operations
        /// </summary>
        private static readonly Lazy<string> _pr_RiskAssessment_Insert = new Lazy<string>(() => "pr_RiskAssessment_Insert");
        public static string pr_RiskAssessment_Insert => _pr_RiskAssessment_Insert.Value;

        private static readonly Lazy<string> _pr_RiskAssessment_GetById = new Lazy<string>(() => "pr_RiskAssessment_GetById");
        public static string pr_RiskAssessment_GetById => _pr_RiskAssessment_GetById.Value;

        private static readonly Lazy<string> _pr_RiskAssessment_GetAll = new Lazy<string>(() => "pr_RiskAssessment_GetAll");
        public static string pr_RiskAssessment_GetAll => _pr_RiskAssessment_GetAll.Value;

        private static readonly Lazy<string> _pr_RiskAssessment_Update = new Lazy<string>(() => "pr_RiskAssessment_Update");
        public static string pr_RiskAssessment_Update => _pr_RiskAssessment_Update.Value;

        private static readonly Lazy<string> _pr_RiskAssessment_Delete = new Lazy<string>(() => "pr_RiskAssessment_Delete");
        public static string pr_RiskAssessment_Delete => _pr_RiskAssessment_Delete.Value;

        /// <summary>
        /// SMS Mitigation CRUD Operations
        /// </summary>
        private static readonly Lazy<string> _pr_Mitigation_Insert = new Lazy<string>(() => "pr_Mitigation_Insert");
        public static string pr_Mitigation_Insert => _pr_Mitigation_Insert.Value;

        private static readonly Lazy<string> _pr_Mitigation_GetById = new Lazy<string>(() => "pr_Mitigation_GetById");
        public static string pr_Mitigation_GetById => _pr_Mitigation_GetById.Value;

        private static readonly Lazy<string> _pr_Mitigation_GetAll = new Lazy<string>(() => "pr_Mitigation_GetAll");
        public static string pr_Mitigation_GetAll => _pr_Mitigation_GetAll.Value;

        private static readonly Lazy<string> _pr_Mitigation_Update = new Lazy<string>(() => "pr_Mitigation_Update");
        public static string pr_Mitigation_Update => _pr_Mitigation_Update.Value;

        private static readonly Lazy<string> _pr_Mitigation_Delete = new Lazy<string>(() => "pr_Mitigation_Delete");
        public static string pr_Mitigation_Delete => _pr_Mitigation_Delete.Value;

        /// <summary>
        /// SMS Mitigation Assignment CRUD Operations
        /// </summary>
        private static readonly Lazy<string> _pr_MitigationAssignment_Insert = new Lazy<string>(() => "pr_MitigationAssignment_Insert");
        public static string pr_MitigationAssignment_Insert => _pr_MitigationAssignment_Insert.Value;

        private static readonly Lazy<string> _pr_MitigationAssignment_GetById = new Lazy<string>(() => "pr_MitigationAssignment_GetById");
        public static string pr_MitigationAssignment_GetById => _pr_MitigationAssignment_GetById.Value;

        private static readonly Lazy<string> _pr_MitigationAssignment_GetAll = new Lazy<string>(() => "pr_MitigationAssignment_GetAll");
        public static string pr_MitigationAssignment_GetAll => _pr_MitigationAssignment_GetAll.Value;

        private static readonly Lazy<string> _pr_MitigationAssignment_Update = new Lazy<string>(() => "pr_MitigationAssignment_Update");
        public static string pr_MitigationAssignment_Update => _pr_MitigationAssignment_Update.Value;

        private static readonly Lazy<string> _pr_MitigationAssignment_Delete = new Lazy<string>(() => "pr_MitigationAssignment_Delete");
        public static string pr_MitigationAssignment_Delete => _pr_MitigationAssignment_Delete.Value;

        /// <summary>
        /// SMS Report Validation CRUD Operations
        /// </summary>
        private static readonly Lazy<string> _pr_ReportValidation_Insert = new Lazy<string>(() => "pr_ReportValidation_Insert");
        public static string pr_ReportValidation_Insert => _pr_ReportValidation_Insert.Value;

        private static readonly Lazy<string> _pr_ReportValidation_GetById = new Lazy<string>(() => "pr_ReportValidation_GetById");
        public static string pr_ReportValidation_GetById => _pr_ReportValidation_GetById.Value;

        private static readonly Lazy<string> _pr_ReportValidation_GetAll = new Lazy<string>(() => "pr_ReportValidation_GetAll");
        public static string pr_ReportValidation_GetAll => _pr_ReportValidation_GetAll.Value;

        private static readonly Lazy<string> _pr_ReportValidation_Update = new Lazy<string>(() => "pr_ReportValidation_Update");
        public static string pr_ReportValidation_Update => _pr_ReportValidation_Update.Value;

        private static readonly Lazy<string> _pr_ReportValidation_Delete = new Lazy<string>(() => "pr_ReportValidation_Delete");
        public static string pr_ReportValidation_Delete => _pr_ReportValidation_Delete.Value;

        /// <summary>
        /// SMS Scoring Panel CRUD Operations
        /// </summary>
        private static readonly Lazy<string> _pr_ScoringPanel_Insert = new Lazy<string>(() => "pr_ScoringPanel_Insert");
        public static string pr_ScoringPanel_Insert => _pr_ScoringPanel_Insert.Value;

        private static readonly Lazy<string> _pr_ScoringPanel_GetById = new Lazy<string>(() => "pr_ScoringPanel_GetById");
        public static string pr_ScoringPanel_GetById => _pr_ScoringPanel_GetById.Value;

        private static readonly Lazy<string> _pr_ScoringPanel_GetAll = new Lazy<string>(() => "pr_ScoringPanel_GetAll");
        public static string pr_ScoringPanel_GetAll => _pr_ScoringPanel_GetAll.Value;

        private static readonly Lazy<string> _pr_ScoringPanel_Update = new Lazy<string>(() => "pr_ScoringPanel_Update");
        public static string pr_ScoringPanel_Update => _pr_ScoringPanel_Update.Value;

        private static readonly Lazy<string> _pr_ScoringPanel_Delete = new Lazy<string>(() => "pr_ScoringPanel_Delete");
        public static string pr_ScoringPanel_Delete => _pr_ScoringPanel_Delete.Value;

        /// <summary>
        /// SMS Airport Shared Dataset CRUD Operations
        /// </summary>
        private static readonly Lazy<string> _pr_AirportSharedDataset_Insert = new Lazy<string>(() => "pr_AirportSharedDataset_Insert");
        public static string pr_AirportSharedDataset_Insert => _pr_AirportSharedDataset_Insert.Value;

        private static readonly Lazy<string> _pr_AirportSharedDataset_GetById = new Lazy<string>(() => "pr_AirportSharedDataset_GetById");
        public static string pr_AirportSharedDataset_GetById => _pr_AirportSharedDataset_GetById.Value;

        private static readonly Lazy<string> _pr_AirportSharedDataset_GetAll = new Lazy<string>(() => "pr_AirportSharedDataset_GetAll");
        public static string pr_AirportSharedDataset_GetAll => _pr_AirportSharedDataset_GetAll.Value;

        private static readonly Lazy<string> _pr_AirportSharedDataset_Update = new Lazy<string>(() => "pr_AirportSharedDataset_Update");
        public static string pr_AirportSharedDataset_Update => _pr_AirportSharedDataset_Update.Value;

        private static readonly Lazy<string> _pr_AirportSharedDataset_Delete = new Lazy<string>(() => "pr_AirportSharedDataset_Delete");
        public static string pr_AirportSharedDataset_Delete => _pr_AirportSharedDataset_Delete.Value;
    }
}