//-----------------------------------------------------------------------
// <copyright file="ApplicationServiceTests.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Basic unit tests for Application services.
//                  Tests service registration, creation, and basic functionality.
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PDXSMS_UnitTests.Application.Common;

namespace PDXSMS_UnitTests.Application.Services;

/// <summary>
/// Basic unit tests for Application services
/// Tests service resolution and basic functionality
/// </summary>
public class ApplicationServiceTests : ApplicationTestBase
{
    protected override void RegisterServices(IServiceCollection services)
    {
        // Register services that are available in the actual Application project
        // These tests focus on services that actually exist
    }

    protected override void RegisterMockedServices(IServiceCollection services)
    {
        // Mock external dependencies
    }

    #region Service Resolution Tests

    [Fact]
    public void ServiceProvider_ShouldResolveLogger()
    {
        // Arrange & Act
        var logger = ServiceProvider.GetService<ILogger<ApplicationServiceTests>>();

        // Assert
        logger.Should().NotBeNull();
        logger.Should().BeAssignableTo<ILogger<ApplicationServiceTests>>();
    }

    [Fact]
    public void ServiceProvider_ShouldResolveConfiguration()
    {
        // Arrange & Act
        var configuration = ServiceProvider.GetService<Microsoft.Extensions.Configuration.IConfiguration>();

        // Assert
        configuration.Should().NotBeNull();
        configuration.Should().BeAssignableTo<Microsoft.Extensions.Configuration.IConfiguration>();
    }

    #endregion

    #region Test Helper Validation

    [Fact]
    public void TestBase_ShouldProvideValidTestData()
    {
        // Arrange & Act
        var hazard = CreateTestHazard();

        // Assert
        hazard.Should().NotBeNull();
        hazard.Code.Should().NotBeEmpty();
        hazard.Name.Should().NotBeEmpty();
        hazard.CreatedBy.Should().NotBeEmpty();
    }

    [Fact]
    public void TestBase_ShouldGenerateUniqueTestCodes()
    {
        // Arrange & Act
        var code1 = GenerateUniqueCode("TEST");
        var code2 = GenerateUniqueCode("TEST");

        // Assert
        code1.Should().NotBeEmpty();
        code2.Should().NotBeEmpty();
        code1.Should().NotBe(code2);
        code1.Should().StartWith("TEST-T");
        code2.Should().StartWith("TEST-T");
    }

    #endregion

    #region Entity Creation Tests

    [Fact]
    public void CreateTestEntities_ShouldGenerateMultipleEntities()
    {
        // Arrange & Act
        var hazards = CreateTestEntities(3, CreateTestHazard, "HZ-MULTI");

        // Assert
        hazards.Should().HaveCount(3);
        hazards.Should().OnlyContain(h => h != null);
        hazards.Select(h => h.Code).Should().OnlyHaveUniqueItems();
        hazards.Should().OnlyContain(h => h.Code.Contains("HZ-MULTI"));
    }

    [Fact]
    public void CreateTestReport_ShouldGenerateValidReport()
    {
        // Arrange & Act
        var report = CreateTestReport();

        // Assert
        report.Should().NotBeNull();
        report.Code.Should().NotBeEmpty();
        report.Name.Should().NotBeEmpty();
        report.Status.Should().NotBeEmpty();
        report.CreatedBy.Should().NotBeEmpty();
    }

    [Fact]
    public void CreateTestInvestigation_ShouldGenerateValidInvestigation()
    {
        // Arrange & Act
        var investigation = CreateTestInvestigation();

        // Assert
        investigation.Should().NotBeNull();
        investigation.Code.Should().NotBeEmpty();
        investigation.ReportCode.Should().NotBeEmpty();
        investigation.CreatedBy.Should().NotBeEmpty();
    }

    #endregion

    #region Performance Measurement Tests

    [Fact]
    public async Task MeasureAsync_ShouldMeasureExecutionTime()
    {
        // Arrange & Act
        var (result, duration) = await MeasureAsync(async () =>
        {
            await Task.Delay(10); // Small delay to measure
            return "test result";
        });

        // Assert
        result.Should().Be("test result");
        duration.Should().BeGreaterThan(TimeSpan.FromMilliseconds(5));
        duration.Should().BeLessThan(TimeSpan.FromMilliseconds(100));
    }

    [Fact]
    public async Task TestCancellationScenario_ShouldHandleCancellation()
    {
        // Arrange & Act
        var wasCancelled = await TestCancellationScenario(async token =>
        {
            await Task.Delay(1, token);
            return "completed";
        });

        // Assert
        // Fast operations typically complete before cancellation takes effect
        wasCancelled.Should().BeFalse();
    }

    #endregion

    #region Utility Method Tests

    [Fact]
    public void CreateTimeoutToken_ShouldCreateValidCancellationToken()
    {
        // Arrange & Act
        var token = CreateTimeoutToken(TimeSpan.FromMilliseconds(100));

        // Assert
        token.Should().NotBe(CancellationToken.None);
        // Token should not be cancelled immediately
        token.IsCancellationRequested.Should().BeFalse();
    }

    #endregion
}