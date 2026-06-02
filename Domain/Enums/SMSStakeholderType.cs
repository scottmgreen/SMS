//-----------------------------------------------------------------------
// <copyright file="SMSStakeholderType.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Enumeration defining classification types for SMS smsstakeholder entities.
//                  Domain enumeration defining valid states and classifications
//                  for business entities and processes.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Domain.Enums;

/// <summary>
/// SMS User types for authentication and authorization management
/// </summary>
public abstract class SMSStakeholderType : BaseEnum<SMSStakeholderType>
{
    protected SMSStakeholderType(string value, string name, string description) : base(value, name)
    {
        Description = description;

    }

    public string Description { get; }


    #region SMS Stakeholder Types

    /// <summary>Airline </summary>
    public static readonly SMSStakeholderType Airline = new AirlineType();

    /// <summary>Inspector  (internal staff)</summary>
    public static readonly SMSStakeholderType Inspector = new InspectorType();

    /// <summary>Contractor</summary>
    public static readonly SMSStakeholderType Contractor = new ContractorType();

    /// <summary>Port of Portland</summary>
    public static readonly SMSStakeholderType PortOfPortland = new PortOfPortlandType();

    #endregion

    #region Implementations

    private sealed class AirlineType : SMSStakeholderType
    {
        public AirlineType() : base("SUT-0001", "Airline", "Airline Stakeholder with role-based access to SMS functions")
        {
        }
    }

    private sealed class InspectorType : SMSStakeholderType
    {
        public InspectorType() : base("SUT-0002", "Inspector",
            "Internal organizational user with role-based access to SMS functions")
        {
        }
    }

    private sealed class ContractorType : SMSStakeholderType
    {
        public ContractorType() : base("SUT-0003", "Contractor",
            "External stakeholder with role-based access to SMS functions")
        {
        }
    }
    private sealed class PortOfPortlandType : SMSStakeholderType
    {
        public PortOfPortlandType() : base("SUT-0004", "Port of Portland",
            "Internal stakeholder with role-based access to SMS functions")
        {
        }
    }
    #endregion


    /// <summary>
    /// Gets all stakeholder type values as string array for dropdowns
    /// </summary>
    public static string[] GetAllValuesAsStringArray()
    {
        return GetAllValues().Select(st => st.Value).ToArray();
    }


}
