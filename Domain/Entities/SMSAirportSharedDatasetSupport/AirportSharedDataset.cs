//-----------------------------------------------------------------------
// <copyright file="AirportSharedDataset.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: SMS domain entity representing airportshareddataset with business rules and lifecycle management.
//                  Domain entity implementing business rules and invariants
//                  following Domain-Driven Design principles.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Domain.Entities;

/// <summary>
/// Airport Shared Dataset - Critical SMS Compliance Data
/// 
/// This contains all the regulatory-required data elements for SMS Risk validation
/// and hazard processing. Every field is important for compliance and must be preserved.
/// 
/// This entity maps to the comprehensive AirportSharedDatasetViewModel and provides
/// the foundational data structure for SMS compliance and regulatory reporting.
/// 
/// IMPORTANT: Cannot be created without a ReportID - this links the dataset to its parent report.
/// </summary>
public sealed class AirportSharedDataset : BaseAuditableEntity
{
    public AirportSharedDataset(AirportSharedDatasetID id) : base(id, "SYSTEM", DateTime.UtcNow) { }

    #region Required References

    /// <summary>
    /// REQUIRED: Associated Report ID - cannot create dataset without this
    /// Links this dataset to its parent report for SMS compliance tracking
    /// </summary>
    public string ReportCode { get; set; } = string.Empty;

    /// <summary>
    /// Code identifier for this dataset
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Optional: Associated Hazard Code for linking to parent hazard
    /// </summary>
    public string? HazardCode { get; set; }

    #endregion

    #region Basic Information

    /// <summary>
    /// Internal narrative - populated from original hazard description
    /// </summary>
    public string? PrivateNarrative { get; set; }

    /// <summary>
    /// External narrative suitable for stakeholder sharing
    /// </summary>
    public string? SharedNarrative { get; set; }

    #endregion

    #region Location Classification

    /// <summary>
    /// Primary location area classification
    /// </summary>
    public string? LocationArea { get; set; }

    /// <summary>
    /// Specific sub-area within the primary location
    /// </summary>
    public string? LocationSubArea { get; set; }

    /// <summary>
    /// Custom location description when "Other" is selected
    /// </summary>
    public string? LocationOther { get; set; }

    #endregion

    #region Environmental Context

    /// <summary>
    /// Weather conditions during hazard occurrence
    /// </summary>
    public string? Weather { get; set; }

    #endregion

    #region Event Details

    /// <summary>
    /// Primary triggering event classification
    /// </summary>
    public string? TriggeringEvent { get; set; }

    #endregion

    #region Involved Components

    public bool AircraftInvolved { get; set; }
    public bool PoweredEquipmentInvolved { get; set; }
    public bool NonPoweredEquipmentInvolved { get; set; }
    public bool PedestrianInvolved { get; set; }
    public bool OtherInvolved { get; set; }

    /// <summary>
    /// Description when "Other" component is involved
    /// </summary>
    public string? OtherDescription { get; set; }

    #endregion

    #region Resulting Issues

    public bool PropertyDamage { get; set; }
    public string? PropertyDamageComments { get; set; }

    public bool PersonalInjury { get; set; }
    public string? PersonalInjuryComments { get; set; }

    public bool Fatality { get; set; }
    public string? FatalityComments { get; set; }

    public bool OtherIssues { get; set; }
    public string? OtherIssuesDescription { get; set; }

    #endregion

    #region Operational Impact

    /// <summary>
    /// Airlines, companies, or operators involved
    /// </summary>
    public string? AirlineCompanyOperator { get; set; }

    /// <summary>
    /// Whether operators were authorized (Yes/No/Unknown/N/A)
    /// </summary>
    public string? OperatorsAuthorized { get; set; }

    /// <summary>
    /// Whether the incident caused flight delays (Yes/No/N/A)
    /// </summary>
    public string? FlightDelay { get; set; }

    /// <summary>
    /// Details about flight delays if they occurred
    /// </summary>
    public string? FlightDelayDetails { get; set; }

    /// <summary>
    /// Whether equipment was removed from service (Yes/No/N/A)
    /// </summary>
    public string? EquipmentRemovedFromService { get; set; }

    /// <summary>
    /// Details about equipment removal if it occurred
    /// </summary>
    public string? EquipmentRemovalDetails { get; set; }

    #endregion

    #region Investigation Details

    /// <summary>
    /// Whether a police report was taken (Yes/No)
    /// </summary>
    public string? PoliceReport { get; set; }

    /// <summary>
    /// Details about the police report
    /// </summary>
    public string? PoliceReportDetails { get; set; }

    #endregion

    #region Contributing Factors

    /// <summary>
    /// Comma-separated list of contributing factor names
    /// </summary>
    public string? ContributingFactors { get; set; }

    /// <summary>
    /// Description when "Other" contributing factor is selected
    /// </summary>
    public string? FactorsOtherDescription { get; set; }

    #endregion
}
