//-----------------------------------------------------------------------
// <copyright file="HazardType.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Enumeration defining classification types for SMS hazard entities.
//                  Domain enumeration defining valid states and classifications
//                  for business entities and processes.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Domain.Enums;

/// <summary>
/// Hazard Type Smart Enum - Specific hazard types within categories
/// Provides detailed classification with regulatory guidance where applicable
/// </summary>
public abstract class HazardType : BaseEnum<HazardType>
{
    protected HazardType(string value, string name, string description, string category, string? guidanceText = null, bool requiresRegulatory = false) : base(value, name)
    {
        Description = description;
        Category = category;
        GuidanceText = guidanceText ?? description;
        RequiresRegulatoryReporting = requiresRegulatory;
    }

    public string Description { get; }
    public string Category { get; }
    public string GuidanceText { get; }
    public bool RequiresRegulatoryReporting { get; }

    #region Incident Types
    public static readonly HazardType Default = new DefaultType();

    public static readonly HazardType Other = new OtherType();

    /// <summary>Aircraft incident per 49 CFR 830.2</summary>
    public static readonly HazardType AircraftIncident = new AircraftIncidentType();

    /// <summary>Aircraft accident per 49 CFR 830.2</summary>
    public static readonly HazardType AircraftAccident = new AircraftAccidentType();

    /// <summary>Surface incident in movement area</summary>
    public static readonly HazardType SurfaceIncidentMovement = new SurfaceIncidentMovementType();

    /// <summary>Surface incident in non-movement area</summary>
    public static readonly HazardType SurfaceIncidentNonMovement = new SurfaceIncidentNonMovementType();

    /// <summary>Runway or taxiway incursion</summary>
    public static readonly HazardType Incursion = new IncursionType();

    /// <summary>Near midair collision</summary>
    public static readonly HazardType NearMidairCollision = new NearMidairCollisionType();

    /// <summary>Other incident type</summary>
    public static readonly HazardType IncidentOther = new IncidentOtherType();

    #endregion

    #region Biological Types

    /// <summary>Mold contamination</summary>
    public static readonly HazardType Mold = new MoldType();

    /// <summary>Bloodborne pathogen exposure</summary>
    public static readonly HazardType BloodbornePathogens = new BloodbornePathogensType();

    /// <summary>Other biological hazard</summary>
    public static readonly HazardType BiologicalOther = new BiologicalOtherType();

    #endregion

    #region Hazardous Substances Types

    /// <summary>Chemical exposure or spill</summary>
    public static readonly HazardType Chemicals = new ChemicalsType();

    /// <summary>Fuel spill or exposure</summary>
    public static readonly HazardType Fuel = new FuelType();

    /// <summary>Dangerous goods incident</summary>
    public static readonly HazardType DangerousGoods = new DangerousGoodsType();

    /// <summary>Other hazardous substance</summary>
    public static readonly HazardType HazardousSubstancesOther = new HazardousSubstancesOtherType();

    #endregion

    #region Physical Types

    /// <summary>Excessive noise exposure</summary>
    public static readonly HazardType Noise = new NoiseType();

    /// <summary>Harmful vibration</summary>
    public static readonly HazardType Vibration = new VibrationType();

    /// <summary>Radiation exposure</summary>
    public static readonly HazardType Radiation = new RadiationType();

    /// <summary>Extreme temperature conditions</summary>
    public static readonly HazardType ExtremeTemperatures = new ExtremeTemperaturesType();

    /// <summary>Poor lighting conditions</summary>
    public static readonly HazardType PoorLighting = new PoorLightingType();

    /// <summary>Other physical hazard</summary>
    public static readonly HazardType PhysicalOther = new PhysicalOtherType();

    #endregion

    #region Physiological Types

    /// <summary>Fatigue-related impairment</summary>
    public static readonly HazardType Fatigue = new FatigueType();

    /// <summary>Stress-related impairment</summary>
    public static readonly HazardType Stress = new StressType();

    /// <summary>Physical overexertion</summary>
    public static readonly HazardType Overexertion = new OverexertionType();

    /// <summary>Dehydration</summary>
    public static readonly HazardType Dehydration = new DehydrationType();

    /// <summary>Inadequate sleep</summary>
    public static readonly HazardType InadequateSleep = new InadequateSleepType();

    /// <summary>Other physiological factor</summary>
    public static readonly HazardType PhysiologicalOther = new PhysiologicalOtherType();

    #endregion

    #region FOD Types

    /// <summary>Metal FOD</summary>
    public static readonly HazardType MetalFOD = new MetalFODType();

    /// <summary>Personal protective equipment FOD</summary>
    public static readonly HazardType PPEFOD = new PPEFODType();

    /// <summary>Concrete, pavement, or surface debris</summary>
    public static readonly HazardType ConcretePavementFOD = new ConcretePavementFODType();

    /// <summary>General debris FOD</summary>
    public static readonly HazardType DebrisFOD = new DebrisFODType();

    /// <summary>Aircraft parts FOD</summary>
    public static readonly HazardType AircraftPartsFOD = new AircraftPartsFODType();

    /// <summary>Tools left on operational surfaces</summary>
    public static readonly HazardType ToolsFOD = new ToolsFODType();

    /// <summary>Mail or freight FOD</summary>
    public static readonly HazardType MailFreightFOD = new MailFreightFODType();

    /// <summary>Trash or litter FOD</summary>
    public static readonly HazardType TrashFOD = new TrashFODType();

    /// <summary>Other FOD</summary>
    public static readonly HazardType FODOther = new FODOtherType();

    #endregion

    #region Wildlife Types

    /// <summary>Bird strike or bird hazard</summary>
    public static readonly HazardType Birds = new BirdsType();

    /// <summary>Mammal encounter</summary>
    public static readonly HazardType Mammals = new MammalsType();

    /// <summary>Reptile encounter</summary>
    public static readonly HazardType Reptiles = new ReptilesType();

    /// <summary>Other wildlife encounter</summary>
    public static readonly HazardType WildlifeOther = new WildlifeOtherType();

    #endregion

    #region General Hazard Type

    /// <summary>General safety hazard</summary>
    public static readonly HazardType GeneralHazard = new GeneralHazardType();

    #endregion

    #region Safety Review Types

    /// <summary>Safety exercises or drills</summary>
    public static readonly HazardType ExercisesDrills = new ExercisesDrillsType();

    /// <summary>Procedural review request</summary>
    public static readonly HazardType ProceduralReview = new ProceduralReviewType();

    /// <summary>Plan or policy review request</summary>
    public static readonly HazardType PlanPolicyReview = new PlanPolicyReviewType();

    /// <summary>Other safety review</summary>
    public static readonly HazardType SafetyReviewOther = new SafetyReviewOtherType();

    #endregion

    #region Non-Standard Operation Types

    /// <summary>Unmanned aircraft system missions</summary>
    public static readonly HazardType UASMissions = new UASMissionsType();

    /// <summary>Temporary airside exceptions to airport security program</summary>
    public static readonly HazardType TemporaryAirsideExceptions = new TemporaryAirsideExceptionsType();

    /// <summary>Special event operations</summary>
    public static readonly HazardType SpecialEvent = new SpecialEventType();

    /// <summary>Other non-standard operation</summary>
    public static readonly HazardType NonStandardOperationOther = new NonStandardOperationOtherType();

    #endregion

    #region Operational Change Types

    /// <summary>New or changing equipment</summary>
    public static readonly HazardType NewChangingEquipment = new NewChangingEquipmentType();

    /// <summary>New or changing infrastructure, layouts, or projects</summary>
    public static readonly HazardType NewChangingInfrastructure = new NewChangingInfrastructureType();

    /// <summary>New or changing technology</summary>
    public static readonly HazardType NewChangingTechnology = new NewChangingTechnologyType();

    /// <summary>New or changing aircraft</summary>
    public static readonly HazardType NewChangingAircraft = new NewChangingAircraftType();

    /// <summary>New or modified procedures, policies, agreements, plans, or regulations</summary>
    public static readonly HazardType NewModifiedProcedures = new NewModifiedProceduresType();

    /// <summary>New or changing staffing or tenants</summary>
    public static readonly HazardType NewChangingStaffing = new NewChangingStaffingType();

    /// <summary>New or changing traffic volume or aircraft types</summary>
    public static readonly HazardType NewChangingTraffic = new NewChangingTrafficType();

    /// <summary>Other operational change</summary>
    public static readonly HazardType OperationalChangeOther = new OperationalChangeOtherType();

    #endregion

    #region Incident Implementations
    private sealed class DefaultType : HazardType
    {
        public DefaultType() : base("DEFAULT_TYPE", "Default Type","Default", "DEFAULT_CATEGORY",
            "To Be Determined by SMS Staff",
            false)
        {
        }
    }
    private sealed class OtherType : HazardType
    {
        public OtherType() : base("OTHER_TYPE", "Other", "Other", "OTHER_CATEGORY", "Other hazards not classified elsewhere",
            false)
        {
        }
    }
    private sealed class AircraftIncidentType : HazardType
    {
        public AircraftIncidentType() : base("AIRCRAFT_INCIDENT", "Aircraft Incident",
            "Aircraft incident per 49 CFR 830.2", "INCIDENT",
            "Aircraft Incident (49 CFR 830.2) means an occurrence other than an aircraft accident, associated with the operation of an aircraft, which affects or could affect the safety of operations.",
            true)
        {
        }
    }

    private sealed class AircraftAccidentType : HazardType
    {
        public AircraftAccidentType() : base("AIRCRAFT_ACCIDENT", "Aircraft Accident",
            "Aircraft accident per 49 CFR 830.2", "INCIDENT",
            "Aircraft Accident (49 CFR 830.2) means an occurrence associated with the operation of an aircraft which takes place between the time any person boards the aircraft with the intention of flight and all such persons have disembarked, and in which any person suffers death or serious injury, or in which the aircraft receives substantial damage, includes \"unmanned aircraft accident.\"",
            true)
        {
        }
    }

    private sealed class SurfaceIncidentMovementType : HazardType
    {
        public SurfaceIncidentMovementType() : base("SURFACE_INCIDENT_MOVEMENT", "Surface Incident (movement)",
            "Surface incident in movement area", "INCIDENT",
            "Surface Incident (movement): Any incident occurring in the movement area involving people or equipment and impacting or having the potential to impact safety or operations.")
        {
        }
    }

    private sealed class SurfaceIncidentNonMovementType : HazardType
    {
        public SurfaceIncidentNonMovementType() : base("SURFACE_INCIDENT_NON_MOVEMENT", "Surface Incident (non-movement)",
            "Surface incident in non-movement area", "INCIDENT",
            "Surface Incident (non-movement): Any incident occurring in the non-movement area involving people or equipment and impacting or having the potential to impact safety or operations.")
        {
        }
    }

    private sealed class IncursionType : HazardType
    {
        public IncursionType() : base("INCURSION", "Incursion",
            "Runway or taxiway incursion", "INCIDENT",
            "Incursion: Any occurrence at an airport involving the incorrect presence of an aircraft, vehicle, or person on the protected area of a surface designated for the landing and take-off of aircraft. (FAA Order 7050.1B)",
            true)
        {
        }
    }

    private sealed class NearMidairCollisionType : HazardType
    {
        public NearMidairCollisionType() : base("NEAR_MIDAIR_COLLISION", "Near Midair Collision",
            "Near midair collision", "INCIDENT",
            "NMAC (FAA AIM 7-7-3) is defined as an incident associated with the operation of an aircraft in which a possibility of collision occurs as a result of proximity of less than 500 feet to another aircraft, or a report is received from a pilot or a flight crewmember stating that a collision hazard existed between two or more aircraft.",
            true)
        {
        }
    }

    private sealed class IncidentOtherType : HazardType
    {
        public IncidentOtherType() : base("INCIDENT_OTHER", "Other",
            "Other incident type", "INCIDENT")
        {
        }
    }

    #endregion

    #region Biological Implementations

    private sealed class MoldType : HazardType
    {
        public MoldType() : base("MOLD", "Mold",
            "Mold contamination", "BIOLOGICAL")
        {
        }
    }

    private sealed class BloodbornePathogensType : HazardType
    {
        public BloodbornePathogensType() : base("BLOODBORNE_PATHOGENS", "Bloodborne pathogens",
            "Bloodborne pathogen exposure", "BIOLOGICAL")
        {
        }
    }

    private sealed class BiologicalOtherType : HazardType
    {
        public BiologicalOtherType() : base("BIOLOGICAL_OTHER", "Other",
            "Other biological hazard", "BIOLOGICAL")
        {
        }
    }

    #endregion

    #region Hazardous Substances Implementations

    private sealed class ChemicalsType : HazardType
    {
        public ChemicalsType() : base("CHEMICALS", "Chemicals",
            "Chemical exposure or spill", "HAZARDOUS_SUBSTANCES")
        {
        }
    }

    private sealed class FuelType : HazardType
    {
        public FuelType() : base("FUEL", "Fuel",
            "Fuel spill or exposure", "HAZARDOUS_SUBSTANCES")
        {
        }
    }

    private sealed class DangerousGoodsType : HazardType
    {
        public DangerousGoodsType() : base("DANGEROUS_GOODS", "Dangerous goods",
            "Dangerous goods incident", "HAZARDOUS_SUBSTANCES")
        {
        }
    }

    private sealed class HazardousSubstancesOtherType : HazardType
    {
        public HazardousSubstancesOtherType() : base("HAZARDOUS_SUBSTANCES_OTHER", "Other",
            "Other hazardous substance", "HAZARDOUS_SUBSTANCES")
        {
        }
    }

    #endregion

    #region Physical Implementations

    private sealed class NoiseType : HazardType
    {
        public NoiseType() : base("NOISE", "Noise",
            "Excessive noise exposure", "PHYSICAL")
        {
        }
    }

    private sealed class VibrationType : HazardType
    {
        public VibrationType() : base("VIBRATION", "Vibration",
            "Harmful vibration", "PHYSICAL")
        {
        }
    }

    private sealed class RadiationType : HazardType
    {
        public RadiationType() : base("RADIATION", "Radiation",
            "Radiation exposure", "PHYSICAL")
        {
        }
    }

    private sealed class ExtremeTemperaturesType : HazardType
    {
        public ExtremeTemperaturesType() : base("EXTREME_TEMPERATURES", "Extreme temperatures",
            "Extreme temperature conditions", "PHYSICAL")
        {
        }
    }

    private sealed class PoorLightingType : HazardType
    {
        public PoorLightingType() : base("POOR_LIGHTING", "Poor lighting",
            "Poor lighting conditions", "PHYSICAL")
        {
        }
    }

    private sealed class PhysicalOtherType : HazardType
    {
        public PhysicalOtherType() : base("PHYSICAL_OTHER", "Other",
            "Other physical hazard", "PHYSICAL")
        {
        }
    }

    #endregion

    #region Physiological Implementations

    private sealed class FatigueType : HazardType
    {
        public FatigueType() : base("FATIGUE", "Fatigue",
            "Fatigue-related impairment", "PHYSIOLOGICAL")
        {
        }
    }

    private sealed class StressType : HazardType
    {
        public StressType() : base("STRESS", "Stress",
            "Stress-related impairment", "PHYSIOLOGICAL")
        {
        }
    }

    private sealed class OverexertionType : HazardType
    {
        public OverexertionType() : base("OVEREXERTION", "Overexertion",
            "Physical overexertion", "PHYSIOLOGICAL")
        {
        }
    }

    private sealed class DehydrationType : HazardType
    {
        public DehydrationType() : base("DEHYDRATION", "Dehydration",
            "Dehydration", "PHYSIOLOGICAL")
        {
        }
    }

    private sealed class InadequateSleepType : HazardType
    {
        public InadequateSleepType() : base("INADEQUATE_SLEEP", "Inadequate sleep",
            "Inadequate sleep", "PHYSIOLOGICAL")
        {
        }
    }

    private sealed class PhysiologicalOtherType : HazardType
    {
        public PhysiologicalOtherType() : base("PHYSIOLOGICAL_OTHER", "Other",
            "Other physiological factor", "PHYSIOLOGICAL")
        {
        }
    }

    #endregion

    #region FOD Implementations

    private sealed class MetalFODType : HazardType
    {
        public MetalFODType() : base("METAL_FOD", "Metal",
            "Metal FOD", "FOD")
        {
        }
    }

    private sealed class PPEFODType : HazardType
    {
        public PPEFODType() : base("PPE_FOD", "PPE",
            "Personal protective equipment FOD", "FOD")
        {
        }
    }

    private sealed class ConcretePavementFODType : HazardType
    {
        public ConcretePavementFODType() : base("CONCRETE_PAVEMENT_FOD", "Concrete/Pavement/Surface",
            "Concrete, pavement, or surface debris", "FOD")
        {
        }
    }

    private sealed class DebrisFODType : HazardType
    {
        public DebrisFODType() : base("DEBRIS_FOD", "Debris",
            "General debris FOD", "FOD")
        {
        }
    }

    private sealed class AircraftPartsFODType : HazardType
    {
        public AircraftPartsFODType() : base("AIRCRAFT_PARTS_FOD", "Aircraft Parts",
            "Aircraft parts FOD", "FOD")
        {
        }
    }

    private sealed class ToolsFODType : HazardType
    {
        public ToolsFODType() : base("TOOLS_FOD", "Tools",
            "Tools left on operational surfaces", "FOD")
        {
        }
    }

    private sealed class MailFreightFODType : HazardType
    {
        public MailFreightFODType() : base("MAIL_FREIGHT_FOD", "Mail/Freight",
            "Mail or freight FOD", "FOD")
        {
        }
    }

    private sealed class TrashFODType : HazardType
    {
        public TrashFODType() : base("TRASH_FOD", "Trash",
            "Trash or litter FOD", "FOD")
        {
        }
    }

    private sealed class FODOtherType : HazardType
    {
        public FODOtherType() : base("FOD_OTHER", "Other",
            "Other FOD", "FOD")
        {
        }
    }

    #endregion

    #region Wildlife Implementations

    private sealed class BirdsType : HazardType
    {
        public BirdsType() : base("BIRDS", "Birds",
            "Bird strike or bird hazard", "WILDLIFE")
        {
        }
    }

    private sealed class MammalsType : HazardType
    {
        public MammalsType() : base("MAMMALS", "Mammals",
            "Mammal encounter", "WILDLIFE")
        {
        }
    }

    private sealed class ReptilesType : HazardType
    {
        public ReptilesType() : base("REPTILES", "Reptiles",
            "Reptile encounter", "WILDLIFE")
        {
        }
    }

    private sealed class WildlifeOtherType : HazardType
    {
        public WildlifeOtherType() : base("WILDLIFE_OTHER", "Other",
            "Other wildlife encounter", "WILDLIFE")
        {
        }
    }

    #endregion

    #region General Hazard Implementation

    private sealed class GeneralHazardType : HazardType
    {
        public GeneralHazardType() : base("GENERAL_HAZARD", "General Hazard",
            "General safety hazard", "GENERAL_HAZARD")
        {
        }
    }

    #endregion

    #region Safety Review Implementations

    private sealed class ExercisesDrillsType : HazardType
    {
        public ExercisesDrillsType() : base("EXERCISES_DRILLS", "Exercises/drills",
            "Safety exercises or drills", "SAFETY_REVIEW")
        {
        }
    }

    private sealed class ProceduralReviewType : HazardType
    {
        public ProceduralReviewType() : base("PROCEDURAL_REVIEW", "Procedural review request",
            "Procedural review request", "SAFETY_REVIEW")
        {
        }
    }

    private sealed class PlanPolicyReviewType : HazardType
    {
        public PlanPolicyReviewType() : base("PLAN_POLICY_REVIEW", "Plan or policy review request",
            "Plan or policy review request", "SAFETY_REVIEW")
        {
        }
    }

    private sealed class SafetyReviewOtherType : HazardType
    {
        public SafetyReviewOtherType() : base("SAFETY_REVIEW_OTHER", "Other",
            "Other safety review", "SAFETY_REVIEW")
        {
        }
    }

    #endregion

    #region Non-Standard Operation Implementations

    private sealed class UASMissionsType : HazardType
    {
        public UASMissionsType() : base("UAS_MISSIONS", "UAS Missions",
            "Unmanned aircraft system missions", "NON_STANDARD_OPERATION")
        {
        }
    }

    private sealed class TemporaryAirsideExceptionsType : HazardType
    {
        public TemporaryAirsideExceptionsType() : base("TEMPORARY_AIRSIDE_EXCEPTIONS", "Temporary Airside Exceptions to Airport Security Program",
            "Temporary airside exceptions to airport security program", "NON_STANDARD_OPERATION")
        {
        }
    }

    private sealed class SpecialEventType : HazardType
    {
        public SpecialEventType() : base("SPECIAL_EVENT", "Special Event",
            "Special event operations", "NON_STANDARD_OPERATION")
        {
        }
    }

    private sealed class NonStandardOperationOtherType : HazardType
    {
        public NonStandardOperationOtherType() : base("NON_STANDARD_OPERATION_OTHER", "Other",
            "Other non-standard operation", "NON_STANDARD_OPERATION")
        {
        }
    }

    #endregion

    #region Operational Change Implementations

    private sealed class NewChangingEquipmentType : HazardType
    {
        public NewChangingEquipmentType() : base("NEW_CHANGING_EQUIPMENT", "New or changing equipment",
            "New or changing equipment", "OPERATIONAL_CHANGE")
        {
        }
    }

    private sealed class NewChangingInfrastructureType : HazardType
    {
        public NewChangingInfrastructureType() : base("NEW_CHANGING_INFRASTRUCTURE", "New or changing infrastructure, layouts, or projects",
            "New or changing infrastructure, layouts, or projects", "OPERATIONAL_CHANGE")
        {
        }
    }

    private sealed class NewChangingTechnologyType : HazardType
    {
        public NewChangingTechnologyType() : base("NEW_CHANGING_TECHNOLOGY", "New or changing technology",
            "New or changing technology", "OPERATIONAL_CHANGE")
        {
        }
    }

    private sealed class NewChangingAircraftType : HazardType
    {
        public NewChangingAircraftType() : base("NEW_CHANGING_AIRCRAFT", "New or changing aircraft",
            "New or changing aircraft", "OPERATIONAL_CHANGE")
        {
        }
    }

    private sealed class NewModifiedProceduresType : HazardType
    {
        public NewModifiedProceduresType() : base("NEW_MODIFIED_PROCEDURES", "New or modified procedures, policies, agreements, plans, or regulations",
            "New or modified procedures, policies, agreements, plans, or regulations", "OPERATIONAL_CHANGE")
        {
        }
    }

    private sealed class NewChangingStaffingType : HazardType
    {
        public NewChangingStaffingType() : base("NEW_CHANGING_STAFFING", "New or changing staffing or tenants",
            "New or changing staffing or tenants", "OPERATIONAL_CHANGE")
        {
        }
    }

    private sealed class NewChangingTrafficType : HazardType
    {
        public NewChangingTrafficType() : base("NEW_CHANGING_TRAFFIC", "New or changing traffic volume or aircraft types",
            "New or changing traffic volume or aircraft types", "OPERATIONAL_CHANGE")
        {
        }
    }

    private sealed class OperationalChangeOtherType : HazardType
    {
        public OperationalChangeOtherType() : base("OPERATIONAL_CHANGE_OTHER", "Other",
            "Other operational change", "OPERATIONAL_CHANGE")
        {
        }
    }

    #endregion

    /// <summary>
    /// Gets all available hazard types
    /// </summary>
    public static IEnumerable<HazardType> GetAllValues()
    {
        return typeof(HazardType)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(f => f.FieldType == typeof(HazardType))
            .Select(f => (HazardType)f.GetValue(null)!)
            .Where(ht => ht != null);
    }

    /// <summary>
    /// Gets hazard types for a specific category
    /// </summary>
    public static IEnumerable<HazardType> GetByCategory(string category)
    {
        return GetAllValues().Where(ht => ht.Category == category);
    }

    /// <summary>
    /// Gets hazard types for a specific category enum
    /// </summary>
    public static IEnumerable<HazardType> GetByCategory(HazardCategory category)
    {
        return GetByCategory(category.Value);
    }

    

    

    

   
}
