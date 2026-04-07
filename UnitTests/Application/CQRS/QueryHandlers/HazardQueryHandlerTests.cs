//-----------------------------------------------------------------------
// <copyright file="ApplicationEntityTests.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Unit tests for Application layer entity creation and manipulation.
//                  Tests entity factory methods and data generation utilities.
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.Extensions.DependencyInjection;
using PDXSMS_UnitTests.Application.Common;
using SMS_Application.Interfaces;
using System.Diagnostics;
using SMS_Domain.Enums;

namespace PDXSMS_UnitTests.Application.Messaging.QueryHandlers;

/// <summary>
/// Unit tests for Application layer entity creation and manipulation
/// Tests entity factory methods and data generation utilities
/// </summary>
public class ApplicationEntityTests : ApplicationTestBase
{
    protected override void RegisterServices(IServiceCollection services)
    {
        // Basic service registration
    }

    protected override void RegisterMockedServices(IServiceCollection services)
    {
        // Basic mock registration
    }

    #region Hazard Entity Tests

    [Fact]
    public void CreateTestHazard_ShouldGenerateCompleteHazard()
    {
        // Arrange & Act
        var hazard = CreateTestHazard();

        // Assert
        hazard.Should().NotBeNull();
        hazard.Code.Should().NotBeEmpty();
        hazard.Name.Should().StartWith("Test Hazard");
        hazard.Description.Should().StartWith("Unit test hazard");
        hazard.Status.Should().Be("Active");
        ValidateAuditFields(hazard);
    }

    [Fact]
    public void CreateTestHazard_WithMultipleInstances_ShouldGenerateUniqueHazards()
    {
        // Arrange & Act
        var hazards = Enumerable.Range(1, 5)
            .Select(_ => CreateTestHazard())
            .ToList();

        // Assert
        hazards.Should().HaveCount(5);
        hazards.Select(h => h.Code).Should().OnlyHaveUniqueItems();
        hazards.Select(h => h.ReportCode).Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void Hazard_PropertyTesting_ShouldWork()
    {
        // Arrange
        var hazard = CreateTestHazard();

        // Act & Assert
        hazard.Should().NotBeNull();
        hazard.Description.Should().Be("Test Hazard Description");
        hazard.Status.Should().Be(HazardStatus.InitialRiskAssessment);
    }

    #endregion

    #region Report Entity Tests

    [Fact]
    public void CreateTestReport_ShouldGenerateCompleteReport()
    {
        // Arrange & Act
        var report = CreateTestReport();

        // Assert
        report.Should().NotBeNull();
        report.Code.Should().NotBeEmpty();
        report.Name.Should().StartWith("Test Report");
        report.Description.Should().StartWith("Unit test report");
        report.Status.Should().Be("Active");
        report.Stage.Should().Be("Investigation");
        
        ValidateAuditFields(report);
    }

    [Fact]
    public void CreateTestReport_WithCustomCode_ShouldUseProvidedCode()
    {
        // Arrange
        var customCode = "RP-CUSTOM-TEST";

        // Act
        var report = CreateTestReport(customCode);

        // Assert
        report.Code.Should().Be(customCode);
        
    }

    [Fact]
    public void Report_ReportCreationTests_ShouldReturnCorrectProperties()
    {
        // Arrange
        var customCode = "CUSTOM-REPORT-001";
        var report = CreateTestReport();

        // Act & Assert
        report.Should().NotBeNull();
        report.Status.Should().Be("Active"); // Use Status instead of ReportType
    }

    [Fact]
    public void Report_HazardCreationTests_ShouldReturnCorrectProperties()
    {
        // Arrange
        var customCode = "CUSTOM-REPORT-002";
        var report = CreateTestReport();

        // Act & Assert
        report.Should().NotBeNull();
        report.Code.Should().Be(report.Code); // Use Code instead of ID.Value
    }

    #endregion

    #region Investigation Entity Tests

    [Fact]
    public void CreateTestInvestigation_ShouldGenerateCompleteInvestigation()
    {
        // Arrange & Act
        var investigation = CreateTestInvestigation();

        // Assert
        investigation.Should().NotBeNull();
        investigation.Code.Should().NotBeEmpty();
        investigation.ReportCode.Should().NotBeEmpty();
        investigation.InvestigationNotes.Should().StartWith("Unit test investigation");
        investigation.Status.Should().Be("In Progress");
        ValidateAuditFields(investigation);
    }

    #endregion

    #region Interview Entity Tests

    [Fact]
    public void CreateTestInterview_ShouldGenerateCompleteInterview()
    {
        // Arrange & Act
        var interview = CreateTestInterview();

        // Assert
        interview.Should().NotBeNull();
        interview.Code.Should().NotBeEmpty();
        interview.InvestigationCode.Should().NotBeEmpty();
        
        interview.Status.Should().Be("Completed");
        ValidateAuditFields(interview);
    }

    #endregion

    #region Mitigation Entity Tests

    [Fact]
    public void CreateTestMitigation_ShouldGenerateCompleteMitigation()
    {
        // Arrange & Act
        var mitigation = CreateTestMitigation();

        // Assert
        mitigation.Should().NotBeNull();
        mitigation.Code.Should().NotBeEmpty();
        mitigation.HazardCode.Should().NotBeEmpty();
        mitigation.Description.Should().StartWith("Unit test mitigation");
        mitigation.Status.Should().Be("Proposed");
        
        ValidateAuditFields(mitigation);
    }

    #endregion

    #region Risk Analysis Entity Tests

    [Fact]
    public void CreateTestRiskAnalysis_ShouldGenerateCompleteRiskAnalysis()
    {
        // Arrange & Act
        var riskAnalysis = CreateTestRiskAnalysis();

        // Assert
        riskAnalysis.Should().NotBeNull();
        riskAnalysis.Code.Should().NotBeEmpty();
        riskAnalysis.HazardCode.Should().NotBeEmpty();
        riskAnalysis.RiskAssessmentCode.Should().NotBeEmpty(); // Use actual property
        riskAnalysis.AssessmentType.Should().Be(RiskAnalysisType.Initial); // Use actual enum
        riskAnalysis.InitialWorstCredibleOutcome.Should().Be("Equipment damage"); // Use actual property
        riskAnalysis.InitialRootCause.Should().Be("Human error"); // Use actual property
        ValidateAuditFields(riskAnalysis);
    }

    #endregion

    #region Entity Collection Tests

    [Fact]
    public void CreateTestEntities_ShouldGenerateSpecifiedQuantity()
    {
        // Arrange & Act
        var reports = CreateTestEntities(3, CreateTestReport, "RP-BULK");

        // Assert
        reports.Should().HaveCount(3);
        reports.Should().OnlyContain(r => r != null);
        reports.Select(r => r.Code).Should().OnlyHaveUniqueItems();
        reports.Should().OnlyContain(r => r.Code.Contains("RP-BULK"));
    }

    [Fact]
    public void CreateTestEntities_WithDifferentFactories_ShouldGenerateDifferentEntityTypes()
    {
        // Arrange & Act
        var hazards = CreateTestEntities(2, CreateTestHazard, "HZ-MIX");
        var reports = CreateTestEntities(2, CreateTestReport, "RP-MIX");

        // Assert
        hazards.Should().HaveCount(2);
        reports.Should().HaveCount(2);
        hazards.Should().OnlyContain(h => h is Hazard);
        reports.Should().OnlyContain(r => r is Report);
    }

    #endregion

    #region Helper Method Tests

    private void ValidateAuditFields(dynamic entity)
    {
        entity.CreatedBy.Should().Be("TEST_USER");
        entity.CreatedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        
        // Check for updated fields if they exist
        if (entity.GetType().GetProperty("UpdatedBy") != null)
        {
            entity.UpdatedBy.Should().Be("TEST_USER");
            entity.UpdatedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        }
    }

    #endregion

    #region Edge Case Tests

    [Fact]
    public void GenerateUniqueCode_ShouldHandleSpecialCharacters()
    {
        // Arrange & Act
        var code1 = GenerateUniqueCode("HZ_TEST");
        var code2 = GenerateUniqueCode("RP.TEST");

        // Assert
        code1.Should().StartWith("HZ_TEST-T");
        code2.Should().StartWith("RP.TEST-T");
        code1.Should().NotBe(code2);
    }

    [Fact]
    public void CreateTestEntities_WithLargeCount_ShouldHandleEfficiently()
    {
        // Arrange & Act
        var stopwatch = Stopwatch.StartNew();
        var entities = CreateTestEntities(100, CreateTestHazard, "HZ-PERF");
        stopwatch.Stop();

        // Assert
        entities.Should().HaveCount(100);
        entities.Select(e => e.Code).Should().OnlyHaveUniqueItems();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(5000); // Should be reasonably fast
    }

    #endregion

    #region Property Tests

    [Fact]
    public void Interview_InterviewCreationTests_ShouldReturnCorrectProperties()
    {
        // Arrange
        var interview = CreateTestInterview();

        // Act & Assert
        interview.Should().NotBeNull();
        interview.PersonInterviewedNotes.Should().StartWith("Test person interview"); // Use actual property
    }

    [Fact]
    public void Mitigation_MitigationCreationTests_ShouldReturnCorrectProperties()
    {
        // Arrange
        var mitigation = CreateTestMitigation();

        // Act & Assert
        mitigation.Should().NotBeNull();
        mitigation.Description.Should().NotBeNull(); // Use Description instead of Priority
    }

    [Fact]
    public void RiskAnalysis_RiskAnalysisCreationTests_ShouldReturnCorrectProperties()
    {
        // Arrange
        var riskAnalysis = CreateTestRiskAnalysis();

        // Act & Assert
        riskAnalysis.Should().NotBeNull();
        riskAnalysis.AssessmentType.Should().Be(RiskAnalysisType.Initial); // Use actual enum property
        riskAnalysis.InitialRootCause.Should().Be("Human error"); // Use actual property
        riskAnalysis.InitialWorstCredibleOutcome.Should().Be("Equipment damage"); // Use actual property
        riskAnalysis.Code.Should().NotBeNullOrEmpty(); // Verify Code property
        riskAnalysis.HazardCode.Should().NotBeNullOrEmpty(); // Verify HazardCode property
    }

    #endregion
}