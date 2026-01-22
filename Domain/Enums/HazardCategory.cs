namespace SMS_Domain.Enums;

/// <summary>
/// Hazard HazardCategory Smart Enum - Primary categorization for hazard types
/// Provides structured categorization with guidance text for each category
/// </summary>
public abstract class HazardCategory : BaseEnum<HazardCategory>
{
    protected HazardCategory(string value, string name, string description, int sortOrder) : base(value, name)
    {
        Description = description;
        SortOrder = sortOrder;
    }

    public string Description { get; }
    public int SortOrder { get; }

    #region Hazard Categories

    /// <summary>Aircraft and operational incidents requiring investigation and reporting</summary>
    public static readonly HazardCategory Incident = new IncidentCategory();

    /// <summary>Biological hazards including pathogens and contamination</summary>
    public static readonly HazardCategory Biological = new BiologicalCategory();

    /// <summary>Chemical and fuel hazards including dangerous goods</summary>
    public static readonly HazardCategory HazardousSubstances = new HazardousSubstancesCategory();

    /// <summary>Physical environmental hazards affecting safety</summary>
    public static readonly HazardCategory Physical = new PhysicalCategory();

    /// <summary>Human physiological factors affecting performance</summary>
    public static readonly HazardCategory Physiological = new PhysiologicalCategory();

    /// <summary>Foreign Object Debris (FOD) hazards</summary>
    public static readonly HazardCategory FOD = new FODCategory();

    /// <summary>Wildlife hazards and animal strikes</summary>
    public static readonly HazardCategory Wildlife = new WildlifeCategory();

    /// <summary>General safety hazards not classified elsewhere</summary>
    public static readonly HazardCategory GeneralHazard = new GeneralHazardCategory();

    /// <summary>Safety reviews, exercises, and procedural assessments</summary>
    public static readonly HazardCategory SafetyReview = new SafetyReviewCategory();

    /// <summary>Non-standard operations requiring special procedures</summary>
    public static readonly HazardCategory NonStandardOperation = new NonStandardOperationCategory();

    /// <summary>Changes in operations, equipment, or procedures</summary>
    public static readonly HazardCategory OperationalChange = new OperationalChangeCategory();

    #endregion

    #region Implementations

    private sealed class IncidentCategory : HazardCategory
    {
        public IncidentCategory() : base("INCIDENT", "Incident",
            "Aircraft and operational incidents including accidents, surface incidents, incursions, and near collisions requiring investigation and reporting", 1)
        {
        }
    }

    private sealed class BiologicalCategory : HazardCategory
    {
        public BiologicalCategory() : base("BIOLOGICAL", "Biological",
            "Biological hazards including mold contamination, bloodborne pathogens, and other biological contaminants affecting health and safety", 2)
        {
        }
    }

    private sealed class HazardousSubstancesCategory : HazardCategory
    {
        public HazardousSubstancesCategory() : base("HAZARDOUS_SUBSTANCES", "Hazardous Substances",
            "Chemical hazards including fuel spills, chemical exposures, dangerous goods incidents, and hazardous material handling", 3)
        {
        }
    }

    private sealed class PhysicalCategory : HazardCategory
    {
        public PhysicalCategory() : base("PHYSICAL", "Physical",
            "Physical environmental hazards including noise exposure, vibration, radiation, extreme temperatures, and poor lighting conditions", 4)
        {
        }
    }

    private sealed class PhysiologicalCategory : HazardCategory
    {
        public PhysiologicalCategory() : base("PHYSIOLOGICAL", "Physiological",
            "Human physiological factors affecting performance including fatigue, stress, overexertion, dehydration, and inadequate sleep", 5)
        {
        }
    }

    private sealed class FODCategory : HazardCategory
    {
        public FODCategory() : base("FOD", "FOD",
            "Foreign Object Debris including metal objects, PPE, concrete/pavement debris, aircraft parts, tools, mail/freight, and trash on operational surfaces", 6)
        {
        }
    }

    private sealed class WildlifeCategory : HazardCategory
    {
        public WildlifeCategory() : base("WILDLIFE", "Wildlife",
            "Wildlife hazards including bird strikes, mammal encounters, reptile incidents, and other animal-related safety concerns", 7)
        {
        }
    }

    private sealed class GeneralHazardCategory : HazardCategory
    {
        public GeneralHazardCategory() : base("GENERAL_HAZARD", "General Hazard",
            "General safety hazards and concerns that do not fit into other specific categories", 8)
        {
        }
    }

    private sealed class SafetyReviewCategory : HazardCategory
    {
        public SafetyReviewCategory() : base("SAFETY_REVIEW", "Safety Review",
            "Safety reviews including exercises/drills, procedural review requests, plan or policy review requests, and other safety assessments", 9)
        {
        }
    }

    private sealed class NonStandardOperationCategory : HazardCategory
    {
        public NonStandardOperationCategory() : base("NON_STANDARD_OPERATION", "Non-Standard Operation",
            "Non-standard operations including UAS missions, temporary airside exceptions to security programs, special events, and other non-routine activities", 10)
        {
        }
    }

    private sealed class OperationalChangeCategory : HazardCategory
    {
        public OperationalChangeCategory() : base("OPERATIONAL_CHANGE", "Operational Change",
            "Changes in operations including new equipment, infrastructure changes, technology updates, aircraft changes, procedural modifications, staffing changes, and traffic volume variations", 11)
        {
        }
    }

    #endregion

    /// <summary>
    /// Gets all available hazard categories ordered by sort order
    /// </summary>
    public static IEnumerable<HazardCategory> GetAllValues()
    {
        return typeof(HazardCategory)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(f => f.FieldType == typeof(HazardCategory))
            .Select(f => (HazardCategory)f.GetValue(null)!)
            .Where(hc => hc != null)
            .OrderBy(hc => hc.SortOrder);
    }

    /// <summary>
    /// Gets hazard categories that typically require immediate attention
    /// </summary>
    //public static IEnumerable<HazardCategory> GetHighPriorityCategories()
    //{
    //    return GetAllValues().Where(hc =>
    //        hc == Incident ||
    //        hc == HazardousSubstances ||
    //        hc == Wildlife);
    //}

    /// <summary>
    /// Gets hazard categories related to operational changes
    /// </summary>
    //public static IEnumerable<HazardCategory> GetOperationalCategories()
    //{
    //    return GetAllValues().Where(hc =>
    //        hc == NonStandardOperation ||
    //        hc == OperationalChange ||
    //        hc == SafetyReview);
    //}
}