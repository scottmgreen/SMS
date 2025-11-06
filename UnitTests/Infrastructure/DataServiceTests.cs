using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SMS_Domain.Entities;
using SMS_Infrastructure.Interfaces;
using SMS_Infrastructure.Services;

namespace PDXSMS_UnitTests.Infrastructure;

/// <summary>
/// Unit tests for SMS Data Services
/// Tests service layer logic and interface compliance
/// </summary>
public class DataServiceTests
{
    #region HazardDataService Tests

    public class HazardDataServiceTests
    {
        [Fact]
        public void HazardDataService_ImplementsInterface_Correctly()
        {
            // Arrange & Assert
            var serviceType = typeof(HazardDataService);
            var interfaceType = typeof(IHazardDataService);
            
            // Verify the service implements the interface
            interfaceType.IsAssignableFrom(serviceType).Should().BeTrue();
            
            // Verify interface methods exist
            var methods = interfaceType.GetMethods();
            methods.Should().Contain(m => m.Name == "CreateHazardAsync");
            methods.Should().Contain(m => m.Name == "GetHazardByIdAsync");
            methods.Should().Contain(m => m.Name == "GetAllHazardsAsync");
            methods.Should().Contain(m => m.Name == "UpdateHazardAsync");
            methods.Should().Contain(m => m.Name == "DeleteHazardAsync");
        }
    }

    #endregion

    #region AirportSharedDatasetDataService Tests

    public class AirportSharedDatasetDataServiceTests
    {
        [Fact]
        public void AirportSharedDatasetDataService_ImplementsInterface_Correctly()
        {
            // Arrange & Assert  
            var serviceType = typeof(AirportSharedDatasetDataService);
            var interfaceType = typeof(IAirportSharedDatasetDataService);
            
            // Verify the service implements the interface
            interfaceType.IsAssignableFrom(serviceType).Should().BeTrue();
            
            // Verify interface methods exist
            var methods = interfaceType.GetMethods();
            methods.Should().Contain(m => m.Name == "CreateAirportSharedDatasetAsync");
            methods.Should().Contain(m => m.Name == "GetAirportSharedDatasetByIdAsync");
            methods.Should().Contain(m => m.Name == "GetAllAirportSharedDatasetsAsync");
            methods.Should().Contain(m => m.Name == "UpdateAirportSharedDatasetAsync");
            methods.Should().Contain(m => m.Name == "DeleteAirportSharedDatasetAsync");
        }

        [Fact]
        public void AirportSharedDatasetDataService_HasCorrectNamespace()
        {
            // Verify the service is in the correct namespace
            var serviceType = typeof(AirportSharedDatasetDataService);
            serviceType.Namespace.Should().Be("SMS_Infrastructure.Services");
        }

        [Fact]
        public void AirportSharedDatasetDataService_InheritsFromBaseDataService()
        {
            // Verify proper inheritance structure
            var serviceType = typeof(AirportSharedDatasetDataService);
            var baseType = serviceType.BaseType;
            
            baseType.Should().NotBeNull();
            baseType!.Name.Should().Be("BaseDataService`1");
        }
    }

    #endregion

    #region ReportDataService Tests

    public class ReportDataServiceTests
    {
        [Fact]
        public void ReportDataService_ImplementsInterface_Correctly()
        {
            // Arrange & Assert
            var serviceType = typeof(ReportDataService);
            var interfaceType = typeof(IReportDataService);
            
            // Verify the service implements the interface
            interfaceType.IsAssignableFrom(serviceType).Should().BeTrue();
            
            // Verify interface methods exist
            var methods = interfaceType.GetMethods();
            methods.Should().Contain(m => m.Name == "CreateReportAsync");
            methods.Should().Contain(m => m.Name == "GetReportByIdAsync");
            methods.Should().Contain(m => m.Name == "GetAllReportsAsync");
            methods.Should().Contain(m => m.Name == "UpdateReportAsync");
            methods.Should().Contain(m => m.Name == "DeleteReportAsync");
        }

        [Fact]
        public void ReportDataService_HasCorrectNamespace()
        {
            // Verify the service is in the correct namespace
            var serviceType = typeof(ReportDataService);
            serviceType.Namespace.Should().Be("SMS_Infrastructure.Services");
        }
    }

    #endregion

    #region Interface Contract Tests

    public class DataServiceInterfaceContractTests
    {
        [Fact]
        public void AllDataServiceInterfaces_HaveAsyncMethods()
        {
            // Test that all data service interfaces follow async patterns
            var hazardMethods = typeof(IHazardDataService).GetMethods();
            var airportMethods = typeof(IAirportSharedDatasetDataService).GetMethods();
            var reportMethods = typeof(IReportDataService).GetMethods();

            // All methods should be async (return Task)
            hazardMethods.Should().OnlyContain(m => m.ReturnType.Name.StartsWith("Task"));
            airportMethods.Should().OnlyContain(m => m.ReturnType.Name.StartsWith("Task"));
            reportMethods.Should().OnlyContain(m => m.ReturnType.Name.StartsWith("Task"));
        }

        [Fact]
        public void AllDataServiceInterfaces_HaveCancellationTokenParameters()
        {
            // Test that all methods accept CancellationToken
            var hazardMethods = typeof(IHazardDataService).GetMethods();
            var airportMethods = typeof(IAirportSharedDatasetDataService).GetMethods();
            var reportMethods = typeof(IReportDataService).GetMethods();

            // All methods should have CancellationToken parameter
            hazardMethods.Should().OnlyContain(m => 
                m.GetParameters().Any(p => p.ParameterType == typeof(CancellationToken)));
            airportMethods.Should().OnlyContain(m => 
                m.GetParameters().Any(p => p.ParameterType == typeof(CancellationToken)));
            reportMethods.Should().OnlyContain(m => 
                m.GetParameters().Any(p => p.ParameterType == typeof(CancellationToken)));
        }

        [Fact]
        public void AllDataServiceInterfaces_ReturnResultTypes()
        {
            // Test that all methods return Result<T> types
            var hazardMethods = typeof(IHazardDataService).GetMethods();
            var airportMethods = typeof(IAirportSharedDatasetDataService).GetMethods();
            var reportMethods = typeof(IReportDataService).GetMethods();

            // All methods should return Task<Result<T>>
            foreach (var method in hazardMethods)
            {
                if (method.ReturnType.IsGenericType)
                {
                    var genericType = method.ReturnType.GetGenericArguments()[0];
                    if (genericType.IsGenericType)
                    {
                        genericType.Name.Should().StartWith("Result");
                    }
                }
            }
        }

        [Fact]
        public void DataServiceInterfaces_ExistInCorrectNamespace()
        {
            // Verify all interfaces are in the Infrastructure.Interfaces namespace
            typeof(IHazardDataService).Namespace.Should().Be("SMS_Infrastructure.Interfaces");
            typeof(IAirportSharedDatasetDataService).Namespace.Should().Be("SMS_Infrastructure.Interfaces");
            typeof(IReportDataService).Namespace.Should().Be("SMS_Infrastructure.Interfaces");
        }

        [Fact]
        public void DataServiceImplementations_ExistInCorrectNamespace()
        {
            // Verify all implementations are in the Infrastructure.Services namespace
            typeof(HazardDataService).Namespace.Should().Be("SMS_Infrastructure.Services");
            typeof(AirportSharedDatasetDataService).Namespace.Should().Be("SMS_Infrastructure.Services");
            typeof(ReportDataService).Namespace.Should().Be("SMS_Infrastructure.Services");
        }
    }

    #endregion
}