using SMS_Domain.Entities;
using SMS_Domain.Errors;

namespace PDXSMS_UnitTests.Domain;

/// <summary>
/// Tests for all SMS Domain Entities
/// Tests entity creation, property setting, business rules, and validation
/// </summary>
public class EntityTests
{
    #region Hazard Entity Tests

    [Fact]
    public void Hazard_Creation_SetsPropertiesCorrectly()
    {
        // Arrange
        var hazardId = new HazardID("HAZ-001");

        // Act
        var hazard = new Hazard(hazardId);

        // Assert
        hazard.Id.Should().Be(hazardId);
        hazard.Code.Should().Be(string.Empty);
        hazard.Name.Should().BeNull();
        hazard.Description.Should().BeNull();
    }

    [Fact]
    public void Hazard_PropertyAssignment_WorksCorrectly()
    {
        // Arrange
        var hazard = new Hazard(new HazardID("HAZ-001"));

        // Act
        hazard.Code = "HAZ-CODE-001";
        hazard.Name = "Test Hazard";
        hazard.Description = "Test hazard description";
        hazard.ReportCode = "RPT-001";

        // Assert
        hazard.Code.Should().Be("HAZ-CODE-001");
        hazard.Name.Should().Be("Test Hazard");
        hazard.Description.Should().Be("Test hazard description");
        hazard.ReportCode.Should().Be("RPT-001");
    }

    #endregion

    #region Report Entity Tests

    [Fact]
    public void Report_Creation_SetsPropertiesCorrectly()
    {
        // Arrange
        var reportId = new ReportID("RPT-001");

        // Act
        var report = new Report(reportId);

        // Assert
        report.Id.Should().Be(reportId);
        report.Code.Should().Be(string.Empty);
    }

    [Fact]
    public void Report_PropertyAssignment_WorksCorrectly()
    {
        // Arrange
        var report = new Report(new ReportID("RPT-001"));

        // Act
        report.Code = "RPT-CODE-001";
        report.Name = "Test Report";
        report.Description = "Test report description";
        report.Status = "Active";
        report.Stage = "Investigation";

        // Assert
        report.Code.Should().Be("RPT-CODE-001");
        report.Name.Should().Be("Test Report");
        report.Description.Should().Be("Test report description");
        report.Status.Should().Be("Active");
        report.Stage.Should().Be("Investigation");
    }

    #endregion

    #region AirportSharedDataset Entity Tests

    [Fact]
    public void AirportSharedDataset_Creation_SetsPropertiesCorrectly()
    {
        // Arrange
        var datasetId = new AirportSharedDatasetID("ASD-001");

        // Act
        var dataset = new AirportSharedDataset(datasetId);

        // Assert
        dataset.Id.Should().Be(datasetId);
        dataset.Code.Should().Be(string.Empty);
        dataset.ReportID.Should().Be(string.Empty);
    }

    [Fact]
    public void AirportSharedDataset_CompletePropertyAssignment_WorksCorrectly()
    {
        // Arrange
        var dataset = new AirportSharedDataset(new AirportSharedDatasetID("ASD-001"));

        // Act - Basic Information
        dataset.Code = "ASD-CODE-001";
        dataset.ReportID = "RPT-001";
        dataset.HazardCode = "HAZ-001";
        dataset.PrivateNarrative = "Private narrative text";
        dataset.SharedNarrative = "Shared narrative text";

        // Location Classification
        dataset.LocationArea = "Movement Area";
        dataset.LocationSubArea = "Runway 10L-28R";
        dataset.LocationOther = "Other location details";

        // Environmental Context
        dataset.Weather = "Clear";

        // Event Details
        dataset.TriggeringEvent = "Aircraft Incident";

        // Involved Components
        dataset.AircraftInvolved = true;
        dataset.PoweredEquipmentInvolved = false;
        dataset.NonPoweredEquipmentInvolved = false;
        dataset.PedestrianInvolved = true;
        dataset.OtherInvolved = false;
        dataset.OtherDescription = "Other component description";

        // Resulting Issues
        dataset.PropertyDamage = true;
        dataset.PropertyDamageComments = "Minor wing damage";
        dataset.PersonalInjury = false;
        dataset.PersonalInjuryComments = null;
        dataset.Fatality = false;
        dataset.FatalityComments = null;
        dataset.OtherIssues = false;
        dataset.OtherIssuesDescription = null;

        // Operational Impact
        dataset.AirlineCompanyOperator = "Test Airlines";
        dataset.OperatorsAuthorized = "Yes";
        dataset.FlightDelay = "Yes";
        dataset.FlightDelayDetails = "15 minute delay for inspection";
        dataset.EquipmentRemovedFromService = "No";
        dataset.EquipmentRemovalDetails = null;

        // Investigation Details
        dataset.PoliceReport = "No";
        dataset.PoliceReportDetails = null;

        // Contributing Factors
        dataset.ContributingFactors = "Weather,Human Factors";
        dataset.FactorsOtherDescription = "Additional factor details";

        // Assert - Basic Information
        dataset.Code.Should().Be("ASD-CODE-001");
        dataset.ReportID.Should().Be("RPT-001");
        dataset.HazardCode.Should().Be("HAZ-001");
        dataset.PrivateNarrative.Should().Be("Private narrative text");
        dataset.SharedNarrative.Should().Be("Shared narrative text");

        // Assert - Location
        dataset.LocationArea.Should().Be("Movement Area");
        dataset.LocationSubArea.Should().Be("Runway 10L-28R");
        dataset.LocationOther.Should().Be("Other location details");

        // Assert - Environment
        dataset.Weather.Should().Be("Clear");

        // Assert - Event
        dataset.TriggeringEvent.Should().Be("Aircraft Incident");

        // Assert - Components
        dataset.AircraftInvolved.Should().BeTrue();
        dataset.PoweredEquipmentInvolved.Should().BeFalse();
        dataset.PedestrianInvolved.Should().BeTrue();
        dataset.OtherDescription.Should().Be("Other component description");

        // Assert - Issues
        dataset.PropertyDamage.Should().BeTrue();
        dataset.PropertyDamageComments.Should().Be("Minor wing damage");
        dataset.PersonalInjury.Should().BeFalse();

        // Assert - Operational Impact
        dataset.AirlineCompanyOperator.Should().Be("Test Airlines");
        dataset.OperatorsAuthorized.Should().Be("Yes");
        dataset.FlightDelay.Should().Be("Yes");
        dataset.FlightDelayDetails.Should().Be("15 minute delay for inspection");

        // Assert - Investigation
        dataset.PoliceReport.Should().Be("No");

        // Assert - Factors
        dataset.ContributingFactors.Should().Be("Weather,Human Factors");
        dataset.FactorsOtherDescription.Should().Be("Additional factor details");
    }

    #endregion

    #region Investigation Entity Tests

    [Fact]
    public void Investigation_Creation_SetsPropertiesCorrectly()
    {
        // Arrange
        var investigationId = new InvestigationID("INV-001");

        // Act
        var investigation = new Investigation(investigationId);

        // Assert
        investigation.Id.Should().Be(investigationId);
    }

    [Fact]
    public void Investigation_PropertyAssignment_WorksCorrectly()
    {
        // Arrange
        var investigation = new Investigation(new InvestigationID("INV-001"));

        // Act
        investigation.Code = "INV-CODE-001";
        investigation.ReportCode = "RPT-001";
        investigation.InvestigationNotes = "Investigation notes";

        // Assert
        investigation.Code.Should().Be("INV-CODE-001");
        investigation.ReportCode.Should().Be("RPT-001");
        investigation.InvestigationNotes.Should().Be("Investigation notes");
    }

    #endregion

    #region Interview Entity Tests

    [Fact]
    public void Interview_Creation_SetsPropertiesCorrectly()
    {
        // Arrange
        var interviewId = new InterviewID("INT-001");

        // Act
        var interview = new Interview(interviewId);

        // Assert
        interview.Id.Should().Be(interviewId);
    }

    [Fact]
    public void Interview_PropertyAssignment_WorksCorrectly()
    {
        // Arrange
        var interview = new Interview(new InterviewID("INT-001"));

        // Act
        interview.Code = "INT-CODE-001";
        interview.InvestigationCode = "INV-001";
        interview.SMSInvestigatorCode = "INV-USER-001";
        interview.PersonInterviewed = "John Doe";
        interview.PersonInterviewedNotes = "Witness statement";
        interview.InvestigatorNotes = "Investigation findings";

        // Assert
        interview.Code.Should().Be("INT-CODE-001");
        interview.InvestigationCode.Should().Be("INV-001");
        interview.SMSInvestigatorCode.Should().Be("INV-USER-001");
        interview.PersonInterviewed.Should().Be("John Doe");
        interview.PersonInterviewedNotes.Should().Be("Witness statement");
        interview.InvestigatorNotes.Should().Be("Investigation findings");
    }

    #endregion

    #region RiskAnalysis Entity Tests

    [Fact]
    public void RiskAnalysis_Creation_SetsPropertiesCorrectly()
    {
        // Arrange
        var riskAnalysisId = new RiskAnalysisID("RA-001");

        // Act
        var riskAnalysis = new RiskAnalysis(riskAnalysisId);

        // Assert
        riskAnalysis.Id.Should().Be(riskAnalysisId);
    }

    [Fact]
    public void RiskAnalysis_PropertyAssignment_WorksCorrectly()
    {
        // Arrange
        var riskAnalysis = new RiskAnalysis(new RiskAnalysisID("RA-001"));

        // Act
        riskAnalysis.Code = "RA-CODE-001";
        riskAnalysis.Name = "Test Risk Analysis";
        riskAnalysis.Description = "Risk analysis description";
        riskAnalysis.HazardCode = "HAZ-001";
        riskAnalysis.Status = "Active";
        riskAnalysis.Stage = "Analysis";
        riskAnalysis.WorstCredibleOutcome = "Aircraft damage";
        riskAnalysis.RootCause = "Human error";

        // Assert
        riskAnalysis.Code.Should().Be("RA-CODE-001");
        riskAnalysis.Name.Should().Be("Test Risk Analysis");
        riskAnalysis.Description.Should().Be("Risk analysis description");
        riskAnalysis.HazardCode.Should().Be("HAZ-001");
        riskAnalysis.Status.Should().Be("Active");
        riskAnalysis.Stage.Should().Be("Analysis");
        riskAnalysis.WorstCredibleOutcome.Should().Be("Aircraft damage");
        riskAnalysis.RootCause.Should().Be("Human error");
    }

    #endregion

    #region RiskAssessment Entity Tests

    [Fact]
    public void RiskAssessment_Creation_SetsPropertiesCorrectly()
    {
        // Arrange
        var riskAssessmentId = new RiskAssessmentID("RAS-001");

        // Act
        var riskAssessment = new RiskAssessment(riskAssessmentId);

        // Assert
        riskAssessment.Id.Should().Be(riskAssessmentId);
    }

    [Fact]
    public void RiskAssessment_PropertyAssignment_WorksCorrectly()
    {
        // Arrange
        var riskAssessment = new RiskAssessment(new RiskAssessmentID("RAS-001"));

        // Act
        riskAssessment.Code = "RAS-CODE-001";
        riskAssessment.Name = "Test Risk Assessment";
        riskAssessment.Description = "Risk assessment description";
        riskAssessment.HazardCode = "HAZ-001";
        riskAssessment.AssessmentType = "Qualitative";
        riskAssessment.Status = "Complete";
        riskAssessment.Stage = "Assessment";

        // Assert
        riskAssessment.Code.Should().Be("RAS-CODE-001");
        riskAssessment.Name.Should().Be("Test Risk Assessment");
        riskAssessment.Description.Should().Be("Risk assessment description");
        riskAssessment.HazardCode.Should().Be("HAZ-001");
        riskAssessment.AssessmentType.Should().Be("Qualitative");
        riskAssessment.Status.Should().Be("Complete");
        riskAssessment.Stage.Should().Be("Assessment");
    }

    #endregion

    #region ScoringPanel Entity Tests

    [Fact]
    public void ScoringPanel_Creation_SetsPropertiesCorrectly()
    {
        // Arrange
        var scoringPanelId = new ScoringPanelID("SP-001");

        // Act
        var scoringPanel = new ScoringPanel(scoringPanelId);

        // Assert
        scoringPanel.Id.Should().Be(scoringPanelId);
    }

    [Fact]
    public void ScoringPanel_PropertyAssignment_WorksCorrectly()
    {
        // Arrange
        var scoringPanel = new ScoringPanel(new ScoringPanelID("SP-001"));

        // Act
        scoringPanel.Code = "SP-CODE-001";
        scoringPanel.HazardCode = "HAZ-001";
        scoringPanel.SMSUserCode = "USER-001";
        scoringPanel.Likelihood = "Probable";
        scoringPanel.Severity = "Major";
        scoringPanel.Score = "High";

        // Assert
        scoringPanel.Code.Should().Be("SP-CODE-001");
        scoringPanel.HazardCode.Should().Be("HAZ-001");
        scoringPanel.SMSUserCode.Should().Be("USER-001");
        scoringPanel.Likelihood.Should().Be("Probable");
        scoringPanel.Severity.Should().Be("Major");
        scoringPanel.Score.Should().Be("High");
    }

    #endregion

    #region ReportValidation Entity Tests

    [Fact]
    public void ReportValidation_Creation_SetsPropertiesCorrectly()
    {
        // Arrange
        var reportValidationId = new ReportValidationID("RV-001");

        // Act
        var reportValidation = new ReportValidation(reportValidationId);

        // Assert
        reportValidation.Id.Should().Be(reportValidationId);
    }

    [Fact]
    public void ReportValidation_PropertyAssignment_WorksCorrectly()
    {
        // Arrange
        var reportValidation = new ReportValidation(new ReportValidationID("RV-001"));

        // Act
        reportValidation.Code = "RV-CODE-001";
        reportValidation.ReportCode = "RPT-001";
        reportValidation.ValidationDecision = "Approved";
        reportValidation.Status = "Complete";
        reportValidation.Stage = "Validation";

        // Assert
        reportValidation.Code.Should().Be("RV-CODE-001");
        reportValidation.ReportCode.Should().Be("RPT-001");
        reportValidation.ValidationDecision.Should().Be("Approved");
        reportValidation.Status.Should().Be("Complete");
        reportValidation.Stage.Should().Be("Validation");
    }

    #endregion
}