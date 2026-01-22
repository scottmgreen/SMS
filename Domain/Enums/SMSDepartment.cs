namespace SMS_Domain.Enums;

public abstract class SMSDepartment : BaseEnum<SMSDepartment>
{
    protected SMSDepartment(string value, string name, string description, string[] responsibilities) : base(value, name)
    {
        Description = description;
        Responsibilities = responsibilities;
    }

    public string Description { get; }
    public string[] Responsibilities { get; }

    // Core Port of Portland Departments
    public static readonly SMSDepartment AirportOperations = new AirportOperationsRole();
    public static readonly SMSDepartment OccupationalSafety = new OccupationalSafetyRole();
    public static readonly SMSDepartment PublicSafety = new PublicSafetyRole();
    public static readonly SMSDepartment Construction = new ConstructionRole();
    public static readonly SMSDepartment AirportBusiness = new AirportBusinessRole();
    public static readonly SMSDepartment Maintenance = new MaintenanceRole();
    public static readonly SMSDepartment Environmental = new EnvironmentalRole();
    public static readonly SMSDepartment AviationSecurity = new AviationSecurityRole();
    public static readonly SMSDepartment FireEMS = new FireEMSRole();
    public static readonly SMSDepartment EmergencyManagement = new EmergencyManagementRole();
    public static readonly SMSDepartment InformationTechnology = new InformationTechnologyRole();
    public static readonly SMSDepartment Legal = new LegalRole();
    public static readonly SMSDepartment PublicAffairs = new PublicAffairsRole();
    public static readonly SMSDepartment Engineering = new EngineeringRole();
    public static readonly SMSDepartment AirsidePlanning = new AirsidePlanningRole();
    public static readonly SMSDepartment Wildlife = new WildlifeRole();
    public static readonly SMSDepartment UASProgram = new UASProgramRole();
    public static readonly SMSDepartment AirportCommunications = new AirportCommunicationsRole();
    public static readonly SMSDepartment CentralProjectsOffice = new CentralProjectsOfficeRole();
    public static readonly SMSDepartment AviationLongRangePlanning = new AviationLongRangePlanningRole();

    private sealed class AirportOperationsRole : SMSDepartment
    {
        public AirportOperationsRole() : base("AIRPORT_OPERATIONS", "Airport Operations",
            "Primary responsibility for airside operations, movement area management, and operational safety oversight",
            new[] { "Airside Operations", "Movement Area Control", "Operational Safety", "Daily Operations Management" })
        {
        }
    }

    private sealed class OccupationalSafetyRole : SMSDepartment
    {
        public OccupationalSafetyRole() : base("OCCUPATIONAL_SAFETY", "Occupational Safety",
            "Workplace safety management, employee health and safety programs, and safety training coordination",
            new[] { "Workplace Safety", "Employee Safety Programs", "Safety Training", "Injury Prevention" })
        {
        }
    }

    private sealed class PublicSafetyRole : SMSDepartment
    {
        public PublicSafetyRole() : base("PUBLIC_SAFETY", "Public Safety & Security",
            "Airport security operations, law enforcement coordination, and public safety management",
            new[] { "Airport Security", "Law Enforcement", "Public Safety", "Security Operations" })
        {
        }
    }

    private sealed class ConstructionRole : SMSDepartment
    {
        public ConstructionRole() : base("CONSTRUCTION", "Construction",
            "Construction project management, contractor oversight, and construction safety coordination",
            new[] { "Construction Management", "Contractor Oversight", "Construction Safety", "Project Coordination" })
        {
        }
    }

    private sealed class AirportBusinessRole : SMSDepartment
    {
        public AirportBusinessRole() : base("AIRPORT_BUSINESS", "Airport Business & Properties",
            "Business operations, property management, tenant relations, and commercial activities",
            new[] { "Business Operations", "Property Management", "Tenant Relations", "Commercial Operations" })
        {
        }
    }

    private sealed class MaintenanceRole : SMSDepartment
    {
        public MaintenanceRole() : base("MAINTENANCE", "Maintenance",
            "Airport infrastructure maintenance, equipment maintenance, and facility management",
            new[] { "Infrastructure Maintenance", "Equipment Maintenance", "Facility Management", "Preventive Maintenance" })
        {
        }
    }

    private sealed class EnvironmentalRole : SMSDepartment
    {
        public EnvironmentalRole() : base("ENVIRONMENTAL", "Environmental",
            "Environmental compliance, sustainability programs, and environmental impact management",
            new[] { "Environmental Compliance", "Sustainability", "Environmental Monitoring", "Regulatory Compliance" })
        {
        }
    }

    private sealed class AviationSecurityRole : SMSDepartment
    {
        public AviationSecurityRole() : base("AVIATION_SECURITY", "Aviation Security",
            "TSA coordination, security screening operations, and aviation security compliance",
            new[] { "Security Screening", "TSA Coordination", "Security Compliance", "Threat Assessment" })
        {
        }
    }

    private sealed class FireEMSRole : SMSDepartment
    {
        public FireEMSRole() : base("FIRE_EMS", "Fire/EMS",
            "Aircraft rescue and firefighting, emergency medical services, and emergency response",
            new[] { "Aircraft Firefighting", "Emergency Medical Services", "Emergency Response", "Rescue Operations" })
        {
        }
    }

    private sealed class EmergencyManagementRole : SMSDepartment
    {
        public EmergencyManagementRole() : base("EMERGENCY_MANAGEMENT", "Emergency Management",
            "Emergency planning, crisis management, and emergency response coordination",
            new[] { "Emergency Planning", "Crisis Management", "Emergency Coordination", "Disaster Response" })
        {
        }
    }

    private sealed class InformationTechnologyRole : SMSDepartment
    {
        public InformationTechnologyRole() : base("INFORMATION_TECHNOLOGY", "Information Technology",
            "IT systems management, cybersecurity, and technology infrastructure support",
            new[] { "IT Systems", "Cybersecurity", "Technology Infrastructure", "System Administration" })
        {
        }
    }

    private sealed class LegalRole : SMSDepartment
    {
        public LegalRole() : base("LEGAL", "Legal",
            "Legal compliance, contract management, and regulatory affairs coordination",
            new[] { "Legal Compliance", "Contract Management", "Regulatory Affairs", "Legal Advisory" })
        {
        }
    }

    private sealed class PublicAffairsRole : SMSDepartment
    {
        public PublicAffairsRole() : base("PUBLIC_AFFAIRS", "Public Affairs",
            "Public communications, media relations, and stakeholder engagement",
            new[] { "Public Communications", "Media Relations", "Stakeholder Engagement", "Community Relations" })
        {
        }
    }

    private sealed class EngineeringRole : SMSDepartment
    {
        public EngineeringRole() : base("ENGINEERING", "Engineering",
            "Infrastructure design, engineering analysis, and technical project support",
            new[] { "Infrastructure Design", "Engineering Analysis", "Technical Support", "Project Engineering" })
        {
        }
    }

    private sealed class AirsidePlanningRole : SMSDepartment
    {
        public AirsidePlanningRole() : base("AIRSIDE_PLANNING", "Airside Planning",
            "Airside development planning, capacity management, and operational planning",
            new[] { "Airside Development", "Capacity Planning", "Operational Planning", "Strategic Planning" })
        {
        }
    }

    private sealed class WildlifeRole : SMSDepartment
    {
        public WildlifeRole() : base("WILDLIFE", "Wildlife",
            "Wildlife hazard management, wildlife control programs, and habitat management",
            new[] { "Wildlife Control", "Hazard Management", "Habitat Management", "Wildlife Monitoring" })
        {
        }
    }

    private sealed class UASProgramRole : SMSDepartment
    {
        public UASProgramRole() : base("UAS_PROGRAM", "UAS Program",
            "Unmanned aircraft systems management, drone operations coordination, and airspace management",
            new[] { "UAS Operations", "Drone Management", "Airspace Coordination", "UAS Safety" })
        {
        }
    }

    private sealed class AirportCommunicationsRole : SMSDepartment
    {
        public AirportCommunicationsRole() : base("AIRPORT_COMMUNICATIONS", "Airport Communications Center",
            "Communications center operations, coordination, and emergency communications",
            new[] { "Communications Operations", "Emergency Communications", "Coordination Center", "Radio Operations" })
        {
        }
    }

    private sealed class CentralProjectsOfficeRole : SMSDepartment
    {
        public CentralProjectsOfficeRole() : base("CENTRAL_PROJECTS", "Central Projects Office",
            "Central project management, capital improvement coordination, and project oversight",
            new[] { "Project Management", "Capital Improvements", "Project Coordination", "Contract Administration" })
        {
        }
    }

    private sealed class AviationLongRangePlanningRole : SMSDepartment
    {
        public AviationLongRangePlanningRole() : base("AVIATION_LONG_RANGE_PLANNING", "Aviation Long-Range Planning",
            "Long-term aviation planning, strategic development, and future capacity planning",
            new[] { "Strategic Planning", "Long-term Development", "Capacity Forecasting", "Master Planning" })
        {
        }
    }

    /// <summary>
    /// Checks if this department has responsibility for a specific area
    /// </summary>
    public bool HasResponsibility(string responsibility)
    {
        return Responsibilities.Contains(responsibility, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gets departments by responsibility area
    /// </summary>
    public static IEnumerable<SMSDepartment> GetDepartmentsByResponsibility(string responsibility)
    {
        return GetAllDepartments().Where(dept => dept.HasResponsibility(responsibility));
    }

    /// <summary>
    /// Gets all available departments
    /// </summary>
    public static IEnumerable<SMSDepartment> GetAllDepartments()
    {
        return typeof(SMSDepartment)
            .GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.DeclaredOnly)
            .Where(f => f.FieldType == typeof(SMSDepartment))
            .Select(f => (SMSDepartment)f.GetValue(null)!)
            .Where(dept => dept != null);
    }
}