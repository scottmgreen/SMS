//-----------------------------------------------------------------------
// <copyright file="EntityTests.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Unit tests for SMS domain entities validating business rules and behavior.
//                  Tests entity creation, validation, state management, and invariants using actual entity properties.
// </copyright>
//-----------------------------------------------------------------------

using FluentAssertions;
using SMS_Domain.Entities;
using SMS_Domain.Enums;
using SMS_Domain.ValueObjects;

namespace PDXSMS_UnitTests.Domain;

public class EntityTests
{
    #region AirportSharedDataset Tests

    [Fact]
    public void AirportSharedDataset_Constructor_ShouldCreateValidInstance()
    {
        // Arrange
        var id = new AirportSharedDatasetID("ASD-001");

        // Act
        var dataset = new AirportSharedDataset(id);

        // Assert
        dataset.Should().NotBeNull();
        dataset.Code.Should().NotBeNull(); // Use Code instead of ID
        dataset.CreatedBy.Should().Be("SYSTEM");
        dataset.CreatedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void AirportSharedDataset_ReportCode_ShouldBeSettableAndGettable()
    {
        // Arrange
        var id = new AirportSharedDatasetID("ASD-002");
        var dataset = new AirportSharedDataset(id);

        // Act
        dataset.ReportCode = "RPT-001"; // Correct property name (not ReportID)

        // Assert
        dataset.ReportCode.Should().Be("RPT-001");
    }

    [Fact]
    public void AirportSharedDataset_Properties_ShouldBeSettableAndGettable()
    {
        // Arrange
        var id = new AirportSharedDatasetID("ASD-003");
        var dataset = new AirportSharedDataset(id);

        // Act
        dataset.Code = "ASD-003";
        dataset.ReportCode = "RPT-002";
        dataset.HazardCode = "HZ-001";
        dataset.PrivateNarrative = "Test private narrative";
        dataset.SharedNarrative = "Test shared narrative";
        dataset.LocationArea = "Airside";
        dataset.LocationSubArea = "Runway";
        dataset.Weather = "Clear";
        dataset.TriggeringEvent = "Equipment Malfunction";
        dataset.AircraftInvolved = true;
        dataset.PropertyDamage = false;
        dataset.PersonalInjury = false;
        dataset.Fatality = false;

        // Assert
        dataset.Code.Should().Be("ASD-003");
        dataset.ReportCode.Should().Be("RPT-002");
        dataset.HazardCode.Should().Be("HZ-001");
        dataset.PrivateNarrative.Should().Be("Test private narrative");
        dataset.SharedNarrative.Should().Be("Test shared narrative");
        dataset.LocationArea.Should().Be("Airside");
        dataset.LocationSubArea.Should().Be("Runway");
        dataset.Weather.Should().Be("Clear");
        dataset.TriggeringEvent.Should().Be("Equipment Malfunction");
        dataset.AircraftInvolved.Should().BeTrue();
        dataset.PropertyDamage.Should().BeFalse();
        dataset.PersonalInjury.Should().BeFalse();
        dataset.Fatality.Should().BeFalse();
    }

    [Fact]
    public void AirportSharedDataset_BooleanFlags_ShouldDefaultToFalse()
    {
        // Arrange
        var id = new AirportSharedDatasetID("ASD-004");

        // Act
        var dataset = new AirportSharedDataset(id);

        // Assert
        dataset.AircraftInvolved.Should().BeFalse();
        dataset.PoweredEquipmentInvolved.Should().BeFalse();
        dataset.NonPoweredEquipmentInvolved.Should().BeFalse();
        dataset.PedestrianInvolved.Should().BeFalse();
        dataset.OtherInvolved.Should().BeFalse();
        dataset.PropertyDamage.Should().BeFalse();
        dataset.PersonalInjury.Should().BeFalse();
        dataset.Fatality.Should().BeFalse();
        dataset.OtherIssues.Should().BeFalse();
    }

    #endregion

    #region RiskAnalysis Tests

    [Fact]
    public void RiskAnalysis_Constructor_ShouldCreateValidInstance()
    {
        // Arrange
        var id = new RiskAnalysisID("RA-001");

        // Act
        var riskAnalysis = new RiskAnalysis(id);

        // Assert
        riskAnalysis.Should().NotBeNull();
        riskAnalysis.Code.Should().NotBeNull(); // Use Code instead of ID
        riskAnalysis.CreatedBy.Should().Be("SYSTEM");
        riskAnalysis.CreatedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        riskAnalysis.AssessmentType.Should().Be(RiskAnalysisType.Initial);
    }

    [Fact]
    public void RiskAnalysis_ActualProperties_ShouldBeSettableAndGettable()
    {
        // Arrange
        var id = new RiskAnalysisID("RA-002");
        var riskAnalysis = new RiskAnalysis(id);

        // Act - Using ACTUAL properties from the RiskAnalysis entity
        riskAnalysis.Code = "RA-002";
        riskAnalysis.AssessmentType = RiskAnalysisType.Residual;
        riskAnalysis.HazardCode = "HZ-001";
        riskAnalysis.RiskAssessmentCode = "RASS-001";
        riskAnalysis.InitialWorstCredibleOutcome = "Aircraft damage";
        riskAnalysis.InitialRootCause = "Human error";
        riskAnalysis.InitialAdditionalComments = "Additional comments for initial analysis";
        riskAnalysis.ResidualWorstCredibleOutcome = "Minor equipment damage";
        riskAnalysis.ResidualRootCause = "Process improvement needed";
        riskAnalysis.ResidualAdditionalComments = "Post-mitigation assessment notes";

        // Assert - Using ACTUAL properties
        riskAnalysis.Code.Should().Be("RA-002");
        riskAnalysis.AssessmentType.Should().Be(RiskAnalysisType.Residual);
        riskAnalysis.HazardCode.Should().Be("HZ-001");
        riskAnalysis.RiskAssessmentCode.Should().Be("RASS-001");
        riskAnalysis.InitialWorstCredibleOutcome.Should().Be("Aircraft damage");
        riskAnalysis.InitialRootCause.Should().Be("Human error");
        riskAnalysis.InitialAdditionalComments.Should().Be("Additional comments for initial analysis");
        riskAnalysis.ResidualWorstCredibleOutcome.Should().Be("Minor equipment damage");
        riskAnalysis.ResidualRootCause.Should().Be("Process improvement needed");
        riskAnalysis.ResidualAdditionalComments.Should().Be("Post-mitigation assessment notes");
    }

    #endregion

    #region RiskAssessment Tests

    [Fact]
    public void RiskAssessment_EnumProperties_ShouldBeSettableWithCorrectEnums()
    {
        // Arrange
        var id = new RiskAssessmentID("RASS-001");
        var riskAssessment = new RiskAssessment(id);

        // Act - Using actual enum values
        riskAssessment.AssessmentType = RiskAssessmentType.Technical;
        riskAssessment.Status = RiskAssessmentStatus.AssessmentComplete;
        riskAssessment.Stage = RiskAssessmentStage.DescribingSystem; // Use actual enum value

        // Assert
        riskAssessment.AssessmentType.Should().Be(RiskAssessmentType.Technical);
        riskAssessment.Status.Should().Be(RiskAssessmentStatus.AssessmentComplete);
        riskAssessment.Stage.Should().Be(RiskAssessmentStage.DescribingSystem); // Use actual enum value
    }

    #endregion

    #region ScoringPanel Tests

    [Fact]
    public void ScoringPanel_NumericProperties_ShouldUseCorrectTypes()
    {
        // Arrange
        var id = new ScoringPanelID("SP-001");
        var scoringPanel = new ScoringPanel(id);

        // Act - Using correct numeric types (int? and decimal?)
        scoringPanel.InitialLikelihood = 3;
        scoringPanel.InitialSeverity = 4;
        scoringPanel.InitialScore = 75.5m;
        scoringPanel.InitialRationale = "Initial scoring rationale";

        // Assert - Using correct types
        scoringPanel.InitialLikelihood.Should().Be(3);
        scoringPanel.InitialSeverity.Should().Be(4);
        scoringPanel.InitialScore.Should().Be(75.5m);
        scoringPanel.InitialRationale.Should().Be("Initial scoring rationale");
    }

    [Fact]
    public void ScoringPanel_BackwardCompatibilityProperties_ShouldWork()
    {
        // Arrange
        var id = new ScoringPanelID("SP-002");
        var scoringPanel = new ScoringPanel(id);

        // Act - Using backward compatibility properties (NotMapped)
        scoringPanel.Likelihood = 5;
        scoringPanel.Severity = 5;
        scoringPanel.Score = 100.0m;
        scoringPanel.Rationale = "Maximum risk score";

        // Assert
        scoringPanel.Likelihood.Should().Be(5);
        scoringPanel.Severity.Should().Be(5);
        scoringPanel.Score.Should().Be(100.0m);
        scoringPanel.Rationale.Should().Be("Maximum risk score");
    }

    #endregion

    #region ReportValidation Tests

    [Fact]
    public void ReportValidation_Status_ShouldUseCorrectEnumType()
    {
        // Arrange
        var id = new ReportValidationID("RV-001");
        var reportValidation = new ReportValidation(id);

        // Act - Using correct enum type
        reportValidation.Status = ReportValidationStatus.ValidationNeeded; // Use actual enum value

        // Assert
        reportValidation.Status.Should().Be(ReportValidationStatus.ValidationNeeded); // Use actual enum value
    }

    #endregion

    #region BaseAuditableEntity Tests

    [Fact]
    public void BaseAuditableEntity_AuditFields_ShouldBeSetByConstructor()
    {
        // Arrange & Act
        var hazard = new Hazard(new HazardID("HZ-AUDIT-001"));

        // Assert
        hazard.CreatedBy.Should().Be("SYSTEM");
        hazard.CreatedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        hazard.UpdatedBy.Should().Be("SYSTEM");
        hazard.UpdatedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void BaseAuditableEntity_UpdatedFields_ShouldBeModifiable()
    {
        // Arrange
        var report = new Report(new ReportID("RP-AUDIT-001"));
        var newUpdateTime = DateTime.UtcNow.AddMinutes(5);

        // Act
        report.UpdatedBy = "TEST_USER";
        report.UpdatedDate = newUpdateTime;

        // Assert
        report.UpdatedBy.Should().Be("TEST_USER");
        report.UpdatedDate.Should().Be(newUpdateTime);
        // Original created values should remain unchanged
        report.CreatedBy.Should().Be("SYSTEM");
        report.CreatedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    #endregion

    #region Entity Equality Tests

    [Fact]
    public void Entities_WithSameId_ShouldBeEqual()
    {
        // Arrange
        var id1 = new HazardID("HZ-EQUAL-001");
        var id2 = new HazardID("HZ-EQUAL-001");
        var hazard1 = new Hazard(id1);
        var hazard2 = new Hazard(id2);

        // Act & Assert
        id1.Should().Be(id2); // Value object equality
        hazard1.Code.Should().Be(hazard2.Code); // Use Code property
    }

    [Fact]
    public void Entities_WithDifferentIds_ShouldNotBeEqual()
    {
        // Arrange
        var hazard1 = new Hazard(new HazardID("HZ-DIFF-001"));
        var hazard2 = new Hazard(new HazardID("HZ-DIFF-002"));

        // Act & Assert
        hazard1.Code.Should().NotBe(hazard2.Code); // Use Code property
    }

    #endregion
}