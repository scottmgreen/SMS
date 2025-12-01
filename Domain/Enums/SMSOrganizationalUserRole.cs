using System.Reflection;
using SMS_Domain.Common;

namespace SMS_Domain.Enums;

//public abstract class SMSOrganizationalUserRole : BaseEnum<SMSOrganizationalUserRole>
//{
//    protected SMSOrganizationalUserRole(string value, string name, string category, string description, int authorityLevel) : base(value, name) 
//    { 
//        Category = category;
//        Description = description;
//        AuthorityLevel = authorityLevel;
//    }

//    public string Category { get; }
//    public string Description { get; }
//    public int AuthorityLevel { get; }

//    #region Executive Roles
//    public static readonly SMSOrganizationalUserRole AccountableExecutive = new AccountableExecutiveRole();
//    public static readonly SMSOrganizationalUserRole ResponsibleExecutive = new ResponsibleExecutiveRole();
//    public static readonly SMSOrganizationalUserRole ResponsibleManager = new ResponsibleManagerRole();
//    #endregion

//    #region Management Roles
//    public static readonly SMSOrganizationalUserRole SMSManager = new SMSManagerRole();
//    public static readonly SMSOrganizationalUserRole SMSCoordinator = new SMSCoordinatorRole();
//    #endregion

//    #region Operational Roles
//    public static readonly SMSOrganizationalUserRole SMSTeamMember = new SMSTeamMemberRole();
//    public static readonly SMSOrganizationalUserRole SubjectMatterExpert = new SubjectMatterExpertRole();
//    public static readonly SMSOrganizationalUserRole SafetyInvestigator = new SafetyInvestigatorRole();
//    public static readonly SMSOrganizationalUserRole SafetyAnalyst = new SafetyAnalystRole();
//    #endregion

//    #region Committee Roles
//    public static readonly SMSOrganizationalUserRole CommitteeChair = new CommitteeChairRole();
//    public static readonly SMSOrganizationalUserRole CommitteeMember = new CommitteeMemberRole();
//    public static readonly SMSOrganizationalUserRole CommitteeFacilitator = new CommitteeFacilitatorRole();
//    //public static readonly SMSRole CoreVotingMember = new CoreVotingMemberRole();
//    #endregion

//    #region External Stakeholder Roles
//    //public static readonly SMSRole AirlineRepresentative = new AirlineRepresentativeRole();
//    //public static readonly SMSRole GroundHandlerRepresentative = new GroundHandlerRepresentativeRole();
//    //public static readonly SMSRole TenantRepresentative = new TenantRepresentativeRole();
//    //public static readonly SMSRole ContractorRepresentative = new ContractorRepresentativeRole();
//    //public static readonly SMSRole RegulatoryRepresentative = new RegulatoryRepresentativeRole();
//    //public static readonly SMSRole ExternalConsultant = new ExternalConsultantRole();
//    #endregion

//    // Executive Role Implementations
//    private sealed class AccountableExecutiveRole : SMSOrganizationalUserRole
//    {
//        public AccountableExecutiveRole() : base("ACCOUNTABLE_EXECUTIVE", "Accountable Executive", "Executive", 
//            "Ultimately responsible for SMS performance, resource allocation, and ensuring compliance with 14 CFR Part 139 Subpart E. Reviews critical risks and compliance issues.", 10)
//        {
//        }
//        SMSUserRolePermission Permissions { get; set; }
//    }

//    private sealed class ResponsibleExecutiveRole : SMSOrganizationalUserRole
//    {
//        public ResponsibleExecutiveRole() : base("RESPONSIBLE_EXECUTIVE", "Responsible Executive", "Executive", 
//            "Provides oversight of safety initiatives, monitors system performance, and works with AE to implement strategic improvements. Reviews high and escalated risks.", 9)
//        {
//        }
//    }

//    private sealed class ResponsibleManagerRole : SMSOrganizationalUserRole
//    {
//        public ResponsibleManagerRole() : base("RESPONSIBLE_MANAGER", "Responsible Manager", "Executive", 
//            "Oversees daily SMS operations at operational level, ensuring hazard reports, risk assessments, and corrective actions are executed effectively. Reviews medium and low risks.", 8)
//        {
//        }
//    }

//    // Management Role Implementations
//    private sealed class SMSManagerRole : SMSOrganizationalUserRole
//    {
//        public SMSManagerRole() : base("SMS_MANAGER", "SMS Manager", "Management", 
//            "Manages day-to-day SMS processes, administers hazard reporting system, coordinates risk assessments, and leads safety investigations.", 7)
//        {
//        }
//    }

//    private sealed class SMSCoordinatorRole : SMSOrganizationalUserRole
//    {
//        public SMSCoordinatorRole() : base("SMS_COORDINATOR", "SMS Coordinator", "Management", 
//            "Supports SMS Manager in data collection, trend analysis, report preparation, safety investigations, and training coordination.", 6)
//        {
//        }
//    }

//    // Operational Role Implementations
//    private sealed class SMSTeamMemberRole : SMSOrganizationalUserRole
//    {
//        public SMSTeamMemberRole() : base("SMS_TEAM_MEMBER", "SMS Team Member", "Operational", 
//            "Represents diverse subject matter expertise and cross-section of airport work groups, assists in hazard review and risk evaluation.", 5)
//        {
//        }
//    }

//    private sealed class SubjectMatterExpertRole : SMSOrganizationalUserRole
//    {
//        public SubjectMatterExpertRole() : base("SUBJECT_MATTER_EXPERT", "Subject Matter Expert", "Operational", 
//            "Provides specialized knowledge and expertise for specific operational areas and safety domains.", 5)
//        {
//        }
//    }

//    private sealed class SafetyInvestigatorRole : SMSOrganizationalUserRole
//    {
//        public SafetyInvestigatorRole() : base("SAFETY_INVESTIGATOR", "Safety Investigator", "Operational", 
//            "Conducts safety investigations, identifies contributing factors, and documents findings for systemic improvements.", 6)
//        {
//        }
//    }

//    private sealed class SafetyAnalystRole : SMSOrganizationalUserRole
//    {
//        public SafetyAnalystRole() : base("SAFETY_ANALYST", "Safety Analyst", "Operational", 
//            "Analyzes safety data, identifies trends, prepares reports, and supports risk assessment activities.", 5)
//        {
//        }
//    }

//    // Committee Role Implementations
//    private sealed class CommitteeChairRole : SMSOrganizationalUserRole
//    {
//        public CommitteeChairRole() : base("COMMITTEE_CHAIR", "Committee Chair", "Committee", 
//            "Leads committee meetings, sets agenda, guides decision-making process, and ensures meeting objectives are met.", 7)
//        {
//        }
//    }

//    private sealed class CommitteeMemberRole : SMSOrganizationalUserRole
//    {
//        public CommitteeMemberRole() : base("COMMITTEE_MEMBER", "Committee Member", "Committee", 
//            "Participates in committee discussions, provides input on safety matters, and contributes to collaborative decision-making.", 4)
//        {
//        }
//    }

//    private sealed class CommitteeFacilitatorRole : SMSOrganizationalUserRole
//    {
//        public CommitteeFacilitatorRole() : base("COMMITTEE_FACILITATOR", "Committee Facilitator", "Committee", 
//            "Supports committee operations, manages logistics, documents meetings, and ensures proper follow-up of action items.", 4)
//        {
//        }
//    }

//    //private sealed class CoreVotingMemberRole : SMSRole
//    //{
//    //    public CoreVotingMemberRole() : base("CORE_VOTING_MEMBER", "Core Voting Member", "Committee", 
//    //        "Essential committee member with voting authority, represents key Port of Portland departments in decision-making.", 5)
//    //    {
//    //    }
//    //}

//    //// External Stakeholder Role Implementations
//    //private sealed class AirlineRepresentativeRole : SMSRole
//    //{
//    //    public AirlineRepresentativeRole() : base("AIRLINE_REPRESENTATIVE", "Airline Representative", "External", 
//    //        "Represents airline interests in SMS committees, provides operational perspective on airside safety matters.", 3)
//    //    {
//    //    }
//    //}

//    //private sealed class GroundHandlerRepresentativeRole : SMSRole
//    //{
//    //    public GroundHandlerRepresentativeRole() : base("GROUND_HANDLER_REPRESENTATIVE", "Ground Handler Representative", "External", 
//    //        "Represents ground handling operations, provides insights on equipment and operational safety in AOA.", 3)
//    //    {
//    //    }
//    //}

//    //private sealed class TenantRepresentativeRole : SMSRole
//    //{
//    //    public TenantRepresentativeRole() : base("TENANT_REPRESENTATIVE", "Tenant Representative", "External", 
//    //        "Represents various airport tenants and their operational interests in safety management processes.", 2)
//    //    {
//    //    }
//    //}

//    //private sealed class ContractorRepresentativeRole : SMSRole
//    //{
//    //    public ContractorRepresentativeRole() : base("CONTRACTOR_REPRESENTATIVE", "Contractor Representative", "External", 
//    //        "Represents construction and service contractors operating within airport boundaries.", 2)
//    //    {
//    //    }
//    //}

//    //private sealed class RegulatoryRepresentativeRole : SMSRole
//    //{
//    //    public RegulatoryRepresentativeRole() : base("REGULATORY_REPRESENTATIVE", "Regulatory Representative", "External", 
//    //        "Represents regulatory agencies and oversight bodies in SMS governance and compliance activities.", 8)
//    //    {
//    //    }
//    //}

//    //private sealed class ExternalConsultantRole : SMSRole
//    //{
//    //    public ExternalConsultantRole() : base("EXTERNAL_CONSULTANT", "External Consultant", "External", 
//    //        "Provides specialized external expertise and consultation services for specific SMS initiatives.", 4)
//    //    {
//    //    }
//    //}

//    /// <summary>
//    /// Determines if this role can approve risks at the specified authority level
//    /// </summary>
//    /// <param name="requiredAuthorityLevel">Required authority level for approval</param>
//    /// <returns>True if role has sufficient authority to approve</returns>
//    public bool CanApprove(int requiredAuthorityLevel)
//    {
//        return AuthorityLevel >= requiredAuthorityLevel;
//    }

//    /// <summary>
//    /// Gets all roles within a specific category
//    /// </summary>
//    /// <param name="category">Role category (Executive, Management, Operational, Committee, External)</param>
//    /// <returns>Collection of roles in the specified category</returns>
//    public static IEnumerable<SMSOrganizationalUserRole> GetRolesByCategory(string category)
//    {
//        return GetAllValues().Where(role => string.Equals(role.Category, category, StringComparison.OrdinalIgnoreCase));
//    }

//    /// <summary>
//    /// Gets all available role values
//    /// </summary>
//    /// <returns>Collection of all SMS roles</returns>
//    public static IEnumerable<SMSOrganizationalUserRole> GetAllValues()
//    {
//        return typeof(SMSOrganizationalUserRole)
//            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
//            .Where(f => f.FieldType == typeof(SMSOrganizationalUserRole))
//            .Select(f => (SMSOrganizationalUserRole)f.GetValue(null)!)
//            .Where(role => role != null);
//    }

//    /// <summary>
//    /// Checks if this is an executive-level role
//    /// </summary>
//    public bool IsExecutiveRole => Category == "Executive";

//    /// <summary>
//    /// Checks if this is a management-level role
//    /// </summary>
//    public bool IsManagementRole => Category == "Management";

//    /// <summary>
//    /// Checks if this is an operational-level role
//    /// </summary>
//    public bool IsOperationalRole => Category == "Operational";

//    /// <summary>
//    /// Checks if this is a committee-specific role
//    /// </summary>
//    public bool IsCommitteeRole => Category == "Committee";

//    /// <summary>
//    /// Checks if this is an external stakeholder role
//    /// </summary>
//    public bool IsExternalRole => Category == "External";
//}