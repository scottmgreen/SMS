//-----------------------------------------------------------------------
// <copyright file="SMSDepartment.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Enumeration defining valid values and classifications for SMS smsdepartment domain concepts.
//                  Domain enumeration defining valid states and classifications
//                  for business entities and processes.
// </copyright>
//-----------------------------------------------------------------------

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

    // Approved Final Department List
    public static readonly SMSDepartment AirlineServiceProvider = new AirlineServiceProviderRole();
    public static readonly SMSDepartment AirportCommunicationsCenter = new AirportCommunicationsCenterRole();
    public static readonly SMSDepartment AirportOperations = new AirportOperationsRole();
    public static readonly SMSDepartment AviationSecurity = new AviationSecurityRole();
    public static readonly SMSDepartment BusinessAndProperties = new BusinessAndPropertiesRole();
    public static readonly SMSDepartment CargoAirline = new CargoAirlineRole();
    public static readonly SMSDepartment Concessions = new ConcessionsRole();
    public static readonly SMSDepartment Construction = new ConstructionRole();
    public static readonly SMSDepartment Contractor = new ContractorRole();
    public static readonly SMSDepartment EmergencyManagement = new EmergencyManagementRole();
    public static readonly SMSDepartment Engineering = new EngineeringRole();
    public static readonly SMSDepartment Environmental = new EnvironmentalRole();
    public static readonly SMSDepartment FixedBaseOperator = new FixedBaseOperatorRole();
    public static readonly SMSDepartment IT = new ITRole();
    public static readonly SMSDepartment Maintenance = new MaintenanceRole();
    public static readonly SMSDepartment PassengerAirline = new PassengerAirlineRole();
    public static readonly SMSDepartment PlanningAndDevelopment = new PlanningAndDevelopmentRole();
    public static readonly SMSDepartment PortFire = new PortFireRole();
    public static readonly SMSDepartment PortPolice = new PortPoliceRole();
    public static readonly SMSDepartment PublicAffairs = new PublicAffairsRole();
    public static readonly SMSDepartment Risk = new RiskRole();
    public static readonly SMSDepartment SafetyAndLossControl = new SafetyAndLossControlRole();
    public static readonly SMSDepartment Wildlife = new WildlifeRole();

    private sealed class AirlineServiceProviderRole : SMSDepartment
    {
        public AirlineServiceProviderRole() : base("AIRLINE_SERVICE_PROVIDER", "Airline Service Provider",
            "Third-party service providers supporting airline operations and passenger services",
            new[] { "Airline Support Services", "Ground Handling", "Passenger Services", "Airline Operations Support" })
        {
        }
    }

    private sealed class AirportCommunicationsCenterRole : SMSDepartment
    {
        public AirportCommunicationsCenterRole() : base("AIRPORT_COMMUNICATIONS_CENTER", "Airport Communications Center",
            "Communications center operations, coordination, and emergency communications",
            new[] { "Communications Operations", "Emergency Communications", "Coordination Center", "Radio Operations" })
        {
        }
    }

    private sealed class AirportOperationsRole : SMSDepartment
    {
        public AirportOperationsRole() : base("AIRPORT_OPERATIONS", "Airport Operations",
            "Primary responsibility for airside operations, movement area management, and operational safety oversight",
            new[] { "Airside Operations", "Movement Area Control", "Operational Safety", "Daily Operations Management" })
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

    private sealed class BusinessAndPropertiesRole : SMSDepartment
    {
        public BusinessAndPropertiesRole() : base("BUSINESS_AND_PROPERTIES", "Business & Properties",
            "Business operations, property management, tenant relations, and commercial activities",
            new[] { "Business Operations", "Property Management", "Tenant Relations", "Commercial Operations" })
        {
        }
    }

    private sealed class CargoAirlineRole : SMSDepartment
    {
        public CargoAirlineRole() : base("CARGO_AIRLINE", "Cargo Airline",
            "Cargo airline operations, freight handling, and cargo security management",
            new[] { "Cargo Operations", "Freight Handling", "Cargo Security", "Logistics Operations" })
        {
        }
    }

    private sealed class ConcessionsRole : SMSDepartment
    {
        public ConcessionsRole() : base("CONCESSIONS", "Concessions",
            "Airport concessions management, retail operations, and commercial tenant oversight",
            new[] { "Retail Operations", "Concession Management", "Commercial Tenants", "Revenue Operations" })
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

    private sealed class ContractorRole : SMSDepartment
    {
        public ContractorRole() : base("CONTRACTOR", "Contractor",
            "External contractors providing specialized services and construction work",
            new[] { "Contract Services", "Specialized Work", "External Operations", "Contracted Activities" })
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

    private sealed class EngineeringRole : SMSDepartment
    {
        public EngineeringRole() : base("ENGINEERING", "Engineering",
            "Infrastructure design, engineering analysis, and technical project support",
            new[] { "Infrastructure Design", "Engineering Analysis", "Technical Support", "Project Engineering" })
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

    private sealed class FixedBaseOperatorRole : SMSDepartment
    {
        public FixedBaseOperatorRole() : base("FIXED_BASE_OPERATOR", "Fixed Base Operator",
            "General aviation services, aircraft maintenance, and private aviation operations",
            new[] { "General Aviation Services", "Aircraft Services", "Private Aviation", "FBO Operations" })
        {
        }
    }

    private sealed class ITRole : SMSDepartment
    {
        public ITRole() : base("IT", "IT",
            "IT systems management, cybersecurity, and technology infrastructure support",
            new[] { "IT Systems", "Cybersecurity", "Technology Infrastructure", "System Administration" })
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

    private sealed class PassengerAirlineRole : SMSDepartment
    {
        public PassengerAirlineRole() : base("PASSENGER_AIRLINE", "Passenger Airline",
            "Passenger airline operations, flight operations, and passenger service management",
            new[] { "Passenger Operations", "Flight Operations", "Passenger Services", "Airline Safety" })
        {
        }
    }

    private sealed class PlanningAndDevelopmentRole : SMSDepartment
    {
        public PlanningAndDevelopmentRole() : base("PLANNING_AND_DEVELOPMENT", "Planning & Development",
            "Strategic planning, development projects, and long-term capacity planning",
            new[] { "Strategic Planning", "Development Projects", "Capacity Planning", "Master Planning" })
        {
        }
    }

    private sealed class PortFireRole : SMSDepartment
    {
        public PortFireRole() : base("PORT_FIRE", "Port Fire",
            "Aircraft rescue and firefighting, emergency medical services, and emergency response",
            new[] { "Aircraft Firefighting", "Emergency Medical Services", "Emergency Response", "Rescue Operations" })
        {
        }
    }

    private sealed class PortPoliceRole : SMSDepartment
    {
        public PortPoliceRole() : base("PORT_POLICE", "Port Police",
            "Airport law enforcement, security operations, and public safety management",
            new[] { "Law Enforcement", "Security Operations", "Public Safety", "Criminal Investigation" })
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

    private sealed class RiskRole : SMSDepartment
    {
        public RiskRole() : base("RISK", "Risk",
            "Risk management, safety management system oversight, and risk assessment coordination",
            new[] { "Risk Management", "Safety Management", "Risk Assessment", "Safety Oversight" })
        {
        }
    }

    private sealed class SafetyAndLossControlRole : SMSDepartment
    {
        public SafetyAndLossControlRole() : base("SAFETY_AND_LOSS_CONTROL", "Safety & Loss Control",
            "Workplace safety management, employee health and safety programs, and loss prevention",
            new[] { "Workplace Safety", "Employee Safety Programs", "Safety Training", "Loss Prevention" })
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
