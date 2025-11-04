using SMS_Domain.Entities;

namespace PDXSMS_UnitTests.Domain;

/// <summary>
/// Tests for all SMS Entity ID Value Objects
/// Tests validation, equality, and edge cases
/// </summary>
public class ValueObjectTests
{
    #region HazardID Tests

    [Fact]
    public void HazardID_ValidValue_CreatesSuccessfully()
    {
        // Arrange & Act
        var id = new HazardID("123");

        // Assert
        id.Value.Should().Be("123");
        id.ToString().Should().Be("123");
    }

    [Fact]
    public void HazardID_NullValue_ThrowsArgumentException()
    {
        // Act & Assert
        Action act = () => new HazardID(null!);
        act.Should().Throw<ArgumentException>()
           .WithMessage("*cannot be null*");
    }

    [Fact]
    public void HazardID_EmptyValue_ThrowsArgumentException()
    {
        // Act & Assert
        Action act = () => new HazardID("");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void HazardID_ValueProperty_WorksCorrectly()
    {
        // Arrange
        var id = new HazardID("456");

        // Act & Assert
        id.Value.Should().Be("456");
    }

    [Fact]
    public void HazardID_Equality_WorksCorrectly()
    {
        // Arrange
        var id1 = new HazardID("123");
        var id2 = new HazardID("123");
        var id3 = new HazardID("456");

        // Assert
        id1.Should().Be(id2);
        id1.Should().NotBe(id3);
        (id1 == id2).Should().BeTrue();
        (id1 != id3).Should().BeTrue();
    }

    #endregion

    #region ReportID Tests

    [Fact]
    public void ReportID_ValidValue_CreatesSuccessfully()
    {
        // Arrange & Act
        var id = new ReportID("RPT-001");

        // Assert
        id.Value.Should().Be("RPT-001");
    }

    [Fact]
    public void ReportID_NullValue_ThrowsArgumentException()
    {
        // Act & Assert
        Action act = () => new ReportID(null!);
        act.Should().Throw<ArgumentException>();
    }

    #endregion

    #region AirportSharedDatasetID Tests

    [Fact]
    public void AirportSharedDatasetID_ValidValue_CreatesSuccessfully()
    {
        // Arrange & Act
        var id = new AirportSharedDatasetID("ASD-001");

        // Assert
        id.Value.Should().Be("ASD-001");
    }

    [Fact]
    public void AirportSharedDatasetID_NullValue_ThrowsArgumentException()
    {
        // Act & Assert
        Action act = () => new AirportSharedDatasetID(null!);
        act.Should().Throw<ArgumentException>()
           .WithMessage("*cannot be null*");
    }

    #endregion

    #region InvestigationID Tests

    [Fact]
    public void InvestigationID_ValidValue_CreatesSuccessfully()
    {
        // Arrange & Act
        var id = new InvestigationID("INV-001");

        // Assert
        id.Value.Should().Be("INV-001");
    }

    #endregion

    #region InterviewID Tests

    [Fact]
    public void InterviewID_ValidValue_CreatesSuccessfully()
    {
        // Arrange & Act
        var id = new InterviewID("INT-001");

        // Assert
        id.Value.Should().Be("INT-001");
    }

    #endregion

    #region RiskAnalysisID Tests

    [Fact]
    public void RiskAnalysisID_ValidValue_CreatesSuccessfully()
    {
        // Arrange & Act
        var id = new RiskAnalysisID("RA-001");

        // Assert
        id.Value.Should().Be("RA-001");
    }

    #endregion

    #region RiskAssessmentID Tests

    [Fact]
    public void RiskAssessmentID_ValidValue_CreatesSuccessfully()
    {
        // Arrange & Act
        var id = new RiskAssessmentID("RAS-001");

        // Assert
        id.Value.Should().Be("RAS-001");
    }

    #endregion

    #region MitigationID Tests

    [Fact]
    public void MitigationID_ValidValue_CreatesSuccessfully()
    {
        // Arrange & Act
        var id = new MitigationID("MIT-001");

        // Assert
        id.Value.Should().Be("MIT-001");
    }

    #endregion

    #region ScoringPanelID Tests

    [Fact]
    public void ScoringPanelID_ValidValue_CreatesSuccessfully()
    {
        // Arrange & Act
        var id = new ScoringPanelID("SP-001");

        // Assert
        id.Value.Should().Be("SP-001");
    }

    #endregion

    #region ReportValidationID Tests

    [Fact]
    public void ReportValidationID_ValidValue_CreatesSuccessfully()
    {
        // Arrange & Act
        var id = new ReportValidationID("RV-001");

        // Assert
        id.Value.Should().Be("RV-001");
    }

    #endregion
}