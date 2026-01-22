namespace SMS_Domain.Enums;

/// <summary>
/// Five M Component enumeration for hazard classification using the Five M methodology
/// Man, Machine, Method, Material, Milieu - standard aviation safety analysis framework
/// </summary>
public abstract class FiveMComponent : BaseEnum<FiveMComponent>
{
    protected FiveMComponent(string value, string name, string description, string definition, string[] exampleFactors, int analysisWeight) : base(value, name)
    {
        Description = description;
        Definition = definition;
        ExampleFactors = exampleFactors;
        AnalysisWeight = analysisWeight;
    }

    public string Description { get; }
    public string Definition { get; }
    public string[] ExampleFactors { get; }
    public int AnalysisWeight { get; } // 1-5 scale for analysis prioritization

    #region Five M Component Types

    /// <summary>Human factors - Personnel, training, experience, fatigue, communication</summary>
    public static readonly FiveMComponent Man = new ManComponent();

    /// <summary>Equipment factors - Aircraft, vehicles, tools, technology, maintenance</summary>
    public static readonly FiveMComponent Machine = new MachineComponent();

    /// <summary>Procedural factors - Policies, procedures, processes, workflows</summary>
    public static readonly FiveMComponent Method = new MethodComponent();

    /// <summary>Material factors - Parts, supplies, fuel, consumables, documentation</summary>
    public static readonly FiveMComponent Material = new MaterialComponent();

    /// <summary>Environmental factors - Weather, facilities, workspace, organizational culture</summary>
    public static readonly FiveMComponent Milieu = new MilieuComponent();

    #endregion

    #region Implementations

    private sealed class ManComponent : FiveMComponent
    {
        public ManComponent() : base("MAN", "Man (Human)",
            "Human factors including personnel capabilities, training, experience, and performance",
            "All aspects related to human performance, capabilities, limitations, and behavior in the operational environment",
            new[] {
                "Training deficiencies", "Experience level", "Fatigue", "Communication breakdown",
                "Situational awareness", "Decision making", "Workload", "Stress", "Health conditions",
                "Certification currency", "Language barriers", "Cultural factors"
            }, 5)
        {
        }
    }

    private sealed class MachineComponent : FiveMComponent
    {
        public MachineComponent() : base("MACHINE", "Machine (Equipment)",
            "Equipment factors including aircraft, vehicles, tools, and technological systems",
            "All mechanical, electrical, and technological systems and their operational status, maintenance, and design",
            new[] {
                "Equipment failure", "Maintenance issues", "Design defects", "Technological limitations",
                "System integration problems", "Age of equipment", "Calibration issues", "Software bugs",
                "Compatibility issues", "Performance degradation", "Safety system failures"
            }, 4)
        {
        }
    }

    private sealed class MethodComponent : FiveMComponent
    {
        public MethodComponent() : base("METHOD", "Method (Procedures)",
            "Procedural factors including policies, processes, and operational methods",
            "All formal and informal procedures, policies, processes, and methods used in operations",
            new[] {
                "Inadequate procedures", "Procedure not followed", "Unclear instructions", "Missing procedures",
                "Conflicting procedures", "Outdated documentation", "Process inefficiencies", "Workflow issues",
                "Quality control gaps", "Standard operating procedure violations", "Checklist omissions"
            }, 4)
        {
        }
    }

    private sealed class MaterialComponent : FiveMComponent
    {
        public MaterialComponent() : base("MATERIAL", "Material (Resources)",
            "Material factors including parts, supplies, fuel, and physical resources",
            "All physical materials, supplies, consumables, and resources required for safe operations",
            new[] {
                "Defective parts", "Substandard materials", "Contaminated fuel", "Inadequate supplies",
                "Wrong specifications", "Counterfeit parts", "Material degradation", "Storage issues",
                "Supply chain problems", "Documentation errors", "Quality control failures"
            }, 3)
        {
        }
    }

    private sealed class MilieuComponent : FiveMComponent
    {
        public MilieuComponent() : base("MILIEU", "Milieu (Environment)",
            "Environmental factors including weather, facilities, workspace, and organizational culture",
            "All environmental conditions, organizational culture, and contextual factors affecting operations",
            new[] {
                "Weather conditions", "Facility design", "Workspace layout", "Organizational culture",
                "Time pressures", "Regulatory environment", "Economic pressures", "Lighting conditions",
                "Noise levels", "Temperature extremes", "Airspace congestion", "Airport conditions"
            }, 4)
        {
        }
    }

    #endregion

    /// <summary>
    /// Gets all available Five M component values
    /// </summary>
    public static IEnumerable<FiveMComponent> GetAllValues()
    {
        return typeof(FiveMComponent)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(f => f.FieldType == typeof(FiveMComponent))
            .Select(f => (FiveMComponent)f.GetValue(null)!)
            .Where(fmc => fmc != null)
            .OrderByDescending(fmc => fmc.AnalysisWeight);
    }

    /// <summary>
    /// Gets high-priority components for analysis
    /// </summary>
    public static IEnumerable<FiveMComponent> GetHighPriorityComponents()
    {
        return GetAllValues().Where(fmc => fmc.AnalysisWeight >= 4);
    }

    /// <summary>
    /// Gets component by keyword analysis of hazard description
    /// </summary>
    public static FiveMComponent? AnalyzeFromDescription(string hazardDescription)
    {
        if (string.IsNullOrWhiteSpace(hazardDescription))
            return null;

        var description = hazardDescription.ToLowerInvariant();

        // Man (Human) keywords
        var manKeywords = new[] {
            "pilot", "mechanic", "controller", "crew", "staff", "personnel", "training", "fatigue",
            "communication", "experience", "skill", "error", "forgot", "missed", "confused"
        };

        // Machine (Equipment) keywords
        var machineKeywords = new[] {
            "engine", "aircraft", "equipment", "system", "failure", "malfunction", "maintenance",
            "hydraulic", "electrical", "avionics", "instrument", "computer", "software"
        };

        // Method (Procedures) keywords
        var methodKeywords = new[] {
            "procedure", "checklist", "policy", "process", "manual", "instruction", "guideline",
            "standard", "protocol", "workflow", "step", "sequence"
        };

        // Material keywords
        var materialKeywords = new[] {
            "fuel", "oil", "parts", "component", "supply", "material", "consumable", "defective",
            "contaminated", "worn", "expired", "substandard"
        };

        // Milieu (Environment) keywords
        var milieuKeywords = new[] {
            "weather", "wind", "rain", "fog", "turbulence", "facility", "runway", "airport",
            "environment", "culture", "pressure", "lighting", "noise", "temperature"
        };

        // Count matches for each component
        var manScore = manKeywords.Count(keyword => description.Contains(keyword));
        var machineScore = machineKeywords.Count(keyword => description.Contains(keyword));
        var methodScore = methodKeywords.Count(keyword => description.Contains(keyword));
        var materialScore = materialKeywords.Count(keyword => description.Contains(keyword));
        var milieuScore = milieuKeywords.Count(keyword => description.Contains(keyword));

        // Return the component with highest score
        var maxScore = new[] { manScore, machineScore, methodScore, materialScore, milieuScore }.Max();

        if (maxScore == 0) return null;

        if (manScore == maxScore) return Man;
        if (machineScore == maxScore) return Machine;
        if (methodScore == maxScore) return Method;
        if (materialScore == maxScore) return Material;
        if (milieuScore == maxScore) return Milieu;

        return null;
    }

    /// <summary>
    /// Gets recommended analysis questions for this component
    /// </summary>
    public List<string> GetAnalysisQuestions()
    {
        return this switch
        {
            var c when c == Man => new List<string>
            {
                "What was the training level and experience of personnel involved?",
                "Were there any fatigue or workload factors?",
                "Was communication clear and effective?",
                "Were personnel properly certified and current?",
                "What human performance factors contributed to this hazard?"
            },
            var c when c == Machine => new List<string>
            {
                "What is the maintenance status of the equipment?",
                "Are there any known defects or recurring issues?",
                "Was the equipment operating within specifications?",
                "Are there design limitations or compatibility issues?",
                "What system redundancies or backups are available?"
            },
            var c when c == Method => new List<string>
            {
                "Are current procedures adequate and up-to-date?",
                "Were established procedures followed correctly?",
                "Are procedures clearly written and understood?",
                "Are there conflicting or ambiguous procedures?",
                "What process improvements could prevent recurrence?"
            },
            var c when c == Material => new List<string>
            {
                "Are materials meeting required specifications?",
                "Is there adequate quality control of materials?",
                "Are materials stored and handled properly?",
                "Are supply chain processes reliable?",
                "What material testing or inspection is performed?"
            },
            var c when c == Milieu => new List<string>
            {
                "What environmental conditions existed?",
                "Are facility designs adequate for safe operations?",
                "What organizational or cultural factors are present?",
                "Are there external pressures affecting performance?",
                "How does the operational environment impact safety?"
            },
            _ => new List<string>()
        };
    }

    /// <summary>
    /// Determines if this component typically interacts with another component
    /// </summary>
    public bool InteractsWith(FiveMComponent other)
    {
        // Man interacts with all other components
        if (this == Man || other == Man) return true;

        // Common interaction patterns
        return (this, other) switch
        {
            var (a, b) when (a == Machine && b == Method) || (a == Method && b == Machine) => true,
            var (a, b) when (a == Machine && b == Material) || (a == Material && b == Machine) => true,
            var (a, b) when (a == Method && b == Milieu) || (a == Milieu && b == Method) => true,
            var (a, b) when (a == Material && b == Milieu) || (a == Milieu && b == Material) => true,
            _ => false
        };
    }

    /// <summary>
    /// Gets the typical mitigation approaches for this component
    /// </summary>
    public List<string> GetMitigationApproaches()
    {
        return this switch
        {
            var c when c == Man => new List<string>
            {
                "Enhanced training programs", "Improved procedures", "Fatigue management",
                "Communication protocols", "Crew resource management", "Simulation training"
            },
            var c when c == Machine => new List<string>
            {
                "Preventive maintenance", "System redundancy", "Design improvements",
                "Technology upgrades", "Regular inspections", "Performance monitoring"
            },
            var c when c == Method => new List<string>
            {
                "Procedure revision", "Process standardization", "Quality management",
                "Training updates", "Workflow optimization", "Checklist improvements"
            },
            var c when c == Material => new List<string>
            {
                "Quality control enhancement", "Supplier qualification", "Material testing",
                "Storage improvements", "Specification updates", "Supply chain management"
            },
            var c when c == Milieu => new List<string>
            {
                "Environmental controls", "Facility improvements", "Cultural change programs",
                "Policy modifications", "Organizational restructuring", "Risk management systems"
            },
            _ => new List<string>()
        };
    }

    /// <summary>
    /// Determines if this is a high-impact component requiring immediate attention
    /// </summary>
    public bool IsHighImpact => AnalysisWeight >= 4;

    /// <summary>
    /// Gets the icon class for UI display
    /// </summary>
    public string GetIconClass()
    {
        return this switch
        {
            var c when c == Man => "fas fa-user",
            var c when c == Machine => "fas fa-cog",
            var c when c == Method => "fas fa-list-check",
            var c when c == Material => "fas fa-box",
            var c when c == Milieu => "fas fa-globe",
            _ => "fas fa-question"
        };
    }

    /// <summary>
    /// Gets the color class for UI styling
    /// </summary>
    public string GetColorClass()
    {
        return this switch
        {
            var c when c == Man => "text-primary",
            var c when c == Machine => "text-warning",
            var c when c == Method => "text-info",
            var c when c == Material => "text-success",
            var c when c == Milieu => "text-secondary",
            _ => "text-muted"
        };
    }
}