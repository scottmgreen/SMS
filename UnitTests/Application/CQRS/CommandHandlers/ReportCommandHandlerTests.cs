//-----------------------------------------------------------------------
// <copyright file="ApplicationUtilityTests.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Unit tests for Application layer utilities and helper methods.
//                  Tests performance measurement, data generation, and testing utilities.
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.Extensions.DependencyInjection;
using PDXSMS_UnitTests.Application.Common;
using SMS_Application.Interfaces;
using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using SMS_Domain.Common;

namespace PDXSMS_UnitTests.Application.Messaging.CommandHandlers;

/// <summary>
/// Unit tests for Application layer utilities and helper methods
/// Tests performance measurement, data generation, and testing utilities
/// </summary>
public class ApplicationUtilityTests : ApplicationTestBase
{
    protected override void RegisterServices(IServiceCollection services)
    {
        // Basic service registration for utility tests
    }

    protected override void RegisterMockedServices(IServiceCollection services)
    {
        // Mock services as needed
    }

    #region Performance Measurement Tests

    [Fact]
    public async Task MeasureAsync_WithSimpleOperation_ShouldMeasureAccurately()
    {
        // Arrange
        var expectedResult = "test result";

        // Act
        var (result, duration) = await MeasureAsync(async () =>
        {
            await Task.Delay(25); // Small delay for measurement
            return expectedResult;
        });

        // Assert
        result.Should().Be(expectedResult);
        duration.Should().BeGreaterThan(TimeSpan.FromMilliseconds(20));
        duration.Should().BeLessThan(TimeSpan.FromMilliseconds(100));
    }

    [Fact]
    public async Task MeasureAsync_WithSynchronousOperation_ShouldMeasureQuickly()
    {
        // Arrange & Act
        var (result, duration) = await MeasureAsync(() =>
        {
            var calculation = 0;
            for (int i = 0; i < 1000; i++)
            {
                calculation += i;
            }
            return Task.FromResult(calculation);
        });

        // Assert
        result.Should().Be(499500); // Sum of 0 to 999
        duration.Should().BeLessThan(TimeSpan.FromMilliseconds(50));
    }

    [Fact]
    public async Task MeasurePerformanceAsync_WithMultipleIterations_ShouldProvideMetrics()
    {
        // Arrange & Act
        var metrics = await MeasurePerformanceAsync(async () =>
        {
            await Task.Delay(5);
            return "iteration result";
        }, iterations: 5);

        // Assert
        metrics.Should().NotBeNull();
        metrics.TotalIterations.Should().Be(5);
        metrics.SuccessfulIterations.Should().BeGreaterThan(0);
        metrics.SuccessRate.Should().BeGreaterThan(0);
        metrics.AverageDuration.Should().BeGreaterThan(TimeSpan.Zero);
    }

    [Fact]
    public void Performance_GeneralUsagePatterns_ShouldPerformWell()
    {
        // Arrange
        var metrics = "Test performance metrics";

        // Act - Simulate some performance testing
        var stopwatch = Stopwatch.StartNew();
        
        // Simulate work
        Thread.Sleep(10);
        
        stopwatch.Stop();

        // Assert
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(5000);
    }

    #endregion

    #region Cancellation Testing

    [Fact]
    public async Task TestCancellationScenario_WithQuickOperation_ShouldCompleteBeforeCancellation()
    {
        // Arrange & Act
        var wasCancelled = await TestCancellationScenario(async token =>
        {
            await Task.Delay(1, token); // Very short delay
            return "completed";
        });

        // Assert
        wasCancelled.Should().BeFalse(); // Should complete before cancellation
    }

    [Fact]
    public void CreateTimeoutToken_ShouldCreateValidToken()
    {
        // Arrange
        var timeout = TimeSpan.FromMilliseconds(100);

        // Act
        var token = CreateTimeoutToken(timeout);

        // Assert
        token.Should().NotBe(CancellationToken.None);
        token.IsCancellationRequested.Should().BeFalse(); // Should not be cancelled immediately
    }

    #endregion

    #region Data Validation Tests

    [Fact]
    public void ValidateResult_WithSuccessfulResult_ShouldPassValidation()
    {
        // Arrange
        var successResult = Result<string>.Success("test value");

        // Act & Assert - Should not throw
        ValidateResult(successResult, shouldSucceed: true);
    }

    [Fact]
    public void ValidateResult_WithFailedResult_ShouldPassValidation()
    {
        // Arrange
        var failedResult = Result<string>.Failure<string>(new Error("TEST_ERROR", "test error"));

        // Act & Assert - Should not throw
        ValidateResult(failedResult, shouldSucceed: false, expectedError: "test error");
    }

    [Fact]
    public void ValidateResult_WithWrongExpectation_ShouldFailValidation()
    {
        // Arrange
        var successResult = Result<string>.Success("test value");

        // Act & Assert
        Assert.Throws<Exception>(() => ValidateResult(successResult, shouldSucceed: false));
    }

    #endregion

    #region Code Generation Tests

    [Fact]
    public void GenerateUniqueCode_WithTimestamp_ShouldIncludeTimestamp()
    {
        // Arrange & Act
        var code = GenerateUniqueCode("TST");
        var now = DateTime.Now.ToString("HHmmss");

        // Assert
        code.Should().StartWith("TST-T");
        code.Should().Contain(now.Substring(0, 4)); // At least first 4 digits of timestamp
    }

    [Fact]
    public void GenerateUniqueCode_CalledSimultaneously_ShouldGenerateUniqueCodes()
    {
        // Arrange & Act
        var codes = Enumerable.Range(0, 10)
            .AsParallel()
            .Select(_ => GenerateUniqueCode("CONC"))
            .ToList();

        // Assert
        codes.Should().HaveCount(10);
        codes.Should().OnlyHaveUniqueItems();
        codes.Should().OnlyContain(c => c.StartsWith("CONC-T"));
    }

    #endregion

    #region Configuration Tests

    [Fact]
    public void TestConfiguration_ShouldHaveRequiredSettings()
    {
        // Arrange
        var configuration = ServiceProvider.GetRequiredService<Microsoft.Extensions.Configuration.IConfiguration>();

        // Act & Assert
        configuration["Testing:Environment"].Should().Be("UnitTest");
        configuration.GetConnectionString("DefaultConnection").Should().NotBeEmpty();
        configuration["Logging:LogLevel:Default"].Should().Be("Information");
    }

    #endregion

    #region Entity Validation Tests

    [Theory]
    [InlineData("HZ")]
    [InlineData("RP")] 
    [InlineData("INV")]
    [InlineData("IV")]
    [InlineData("MIT")]
    public void EntityCreation_WithDifferentPrefixes_ShouldGenerateCorrectEntities(string prefix)
    {
        // Arrange & Act
        var entity = prefix switch
        {
            "HZ" => (dynamic)CreateTestHazard(),
            "RP" => (dynamic)CreateTestReport(),
            "INV" => (dynamic)CreateTestInvestigation(),
            "IV" => (dynamic)CreateTestInterview(),
            "MIT" => (dynamic)CreateTestMitigation(),
            _ => throw new ArgumentException("Unknown prefix")
        };

        // Assert
        entity.Should().NotBeNull();
        ((string)entity.Code).Should().StartWith(prefix);
        ((string)entity.CreatedBy).Should().Be("TEST_USER");
        ((DateTime)entity.CreatedDate).Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    #endregion

    #region Stress Tests

    [Fact]
    public void CreateMultipleTestEntities_ShouldHandleLargeQuantities()
    {
        // Arrange & Act
        var stopwatch = Stopwatch.StartNew();
        
        var hazards = CreateTestEntities(50, CreateTestHazard, "HZ-STRESS");
        var reports = CreateTestEntities(50, CreateTestReport, "RP-STRESS");
        var investigations = CreateTestEntities(50, CreateTestInvestigation, "INV-STRESS");
        
        stopwatch.Stop();

        // Assert
        hazards.Should().HaveCount(50);
        reports.Should().HaveCount(50);
        investigations.Should().HaveCount(50);
        
        var allCodes = hazards.Select(h => h.Code)
            .Concat(reports.Select(r => r.Code))
            .Concat(investigations.Select(i => i.Code))
            .ToList();
            
        allCodes.Should().OnlyHaveUniqueItems();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(2000); // Should complete within 2 seconds
    }

    #endregion

    #region Result Tests

    [Fact]
    public void Result_FailureHandling_ShouldWorkCorrectly()
    {
        // Arrange & Act
        var error = new Error("TEST_ERROR", "test error"); // Use proper Error object
        var failedResult = Result<string>.Failure<string>(error);

        // Assert
        failedResult.IsFailure.Should().BeTrue();
        failedResult.Error.Code.Should().Be("TEST_ERROR");
    }

    #endregion
}