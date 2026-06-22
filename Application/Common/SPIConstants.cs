//-----------------------------------------------------------------------
// <copyright file="SPIConstants.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Shared utility providing common functionality for Application layer components.
//                  Provides shared utilities, constants, and base classes
//                  for Application layer components.
// </copyright>
//-----------------------------------------------------------------------

//----------------------------------------------------------------------
// <copyright file="SPIConstants.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Defines constants and shared data for Safety Performance Indicators (SPI).
//                  Provides standardized data sources, validation messages, and helper methods
//                  for SPI management across the application.
// </copyright>
//----------------------------------------------------------------------

using System.Reflection;
using SMS_Domain.Interfaces;

namespace SMS_Application.Common;

/// <summary>
/// Safety Performance Indicator Constants and Utilities
/// Provides constants, validation messages, and shared data for Safety Performance Indicators.
/// Ensures consistency in data source definitions and validation across all SPI operations.
/// 
/// Classes:
/// - SPIDataSources: Standard data source options for SPIs with helper methods
/// - ValidationMessages: Common validation error messages for SPI operations
/// </summary>
public static class SPIConstants
{
    /// <summary>
    /// Dynamic data source discovery for SPIs and data points
    /// REFACTORED: Uses reflection to discover domain events that implement IEventSource
    /// </summary>
    public static class SPIDataSources
    {
        // ===================================================================
        // DYNAMIC DATA SOURCES: Discovered from Domain Events
        // ===================================================================

        /// <summary>
        /// Gets all domain events that can serve as SPI data sources
        /// Uses reflection to discover events implementing IEventSource
        /// </summary>
        public static List<EventDataSourceInfo> GetEventDrivenSources()
        {
            var eventDataSources = new List<EventDataSourceInfo>();

            try
            {
                // Get all assemblies in the current domain
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();

                foreach (var assembly in assemblies)
                {
                    // Skip system assemblies
                    if (IsSystemAssembly(assembly))
                        continue;

                    try
                    {
                        // Find all types that implement IEventSource
                        var eventSourceTypes = assembly.GetTypes()
                            .Where(type => typeof(SMS_Domain.Interfaces.IEventSource).IsAssignableFrom(type) && 
                                          !type.IsInterface && 
                                          !type.IsAbstract)
                            .ToList();

                        foreach (var eventType in eventSourceTypes)
                        {
                            // Create an instance to get the metadata
                            var eventInstance = CreateEventInstance(eventType);
                            if (eventInstance != null)
                            {
                                eventDataSources.Add(new EventDataSourceInfo
                                {
                                    EventType = eventType,
                                    EventTypeName = eventType.Name,
                                    DisplayName = eventInstance.EventSourceDisplayName,
                                    Category = eventInstance.EventSourceCategory,
                                    Description = eventInstance.EventSourceDescription,
                                    IsAutomatic = eventInstance.IsAutomaticDataSource,
                                    Priority = eventInstance.DisplayPriority
                                });
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // Skip assemblies that can't be reflected (e.g., dynamic assemblies)
                        System.Diagnostics.Debug.WriteLine($"Skipping assembly {assembly.FullName}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                // Fallback to empty list if reflection fails
                System.Diagnostics.Debug.WriteLine($"Error discovering event data sources: {ex.Message}");
            }

            // Sort by priority, then by display name
            return eventDataSources
                .OrderBy(eds => eds.Priority)
                .ThenBy(eds => eds.DisplayName)
                .ToList();
        }

        /// <summary>
        /// Gets event-driven data sources grouped by category
        /// </summary>
        public static Dictionary<string, List<EventDataSourceInfo>> GetEventDrivenSourcesByCategory()
        {
            return GetEventDrivenSources()
                .GroupBy(eds => eds.Category)
                .ToDictionary(g => g.Key, g => g.ToList());
        }

        /// <summary>
        /// Gets display names for dropdown binding
        /// </summary>
        public static List<string> GetEventDrivenSourceNames()
        {
            return GetEventDrivenSources()
                .Where(eds => eds.IsAutomatic)
                .Select(eds => eds.DisplayName)
                .ToList();
        }

        /// <summary>
        /// Finds event data source info by display name
        /// </summary>
        public static EventDataSourceInfo? FindByDisplayName(string displayName)
        {
            return GetEventDrivenSources()
                .FirstOrDefault(eds => eds.DisplayName == displayName);
        }

        /// <summary>
        /// Gets recommended data sources for specific SPI categories
        /// </summary>
        public static List<EventDataSourceInfo> GetRecommendedFor(string spiCategory)
        {
            var categoryFilter = spiCategory?.ToUpper();
            return GetEventDrivenSources()
                .Where(eds => eds.IsAutomatic && 
                            (string.IsNullOrEmpty(categoryFilter) || 
                             eds.Category.ToUpper().Contains(categoryFilter)))
                .ToList();
        }

        // ===================================================================
        // LEGACY MANUAL DATA SOURCES: For backward compatibility
        // ===================================================================

        public const string ManualEntry = "Manual Entry";
        public const string ExcelImport = "Excel Import";
        public const string APIIntegration = "API Integration";
        public const string ExternalSystem = "External System";
        public const string CalculatedValue = "Calculated Value";

        /// <summary>
        /// Gets manual data sources that require user input
        /// </summary>
        public static List<string> GetManualSources()
        {
            return new List<string>
            {
                ManualEntry,
                ExcelImport,
                APIIntegration,
                ExternalSystem,
                CalculatedValue
            };
        }

        /// <summary>
        /// Gets all data sources (event-driven + manual)
        /// </summary>
        public static List<string> GetAll()
        {
            var allSources = new List<string>();
            allSources.AddRange(GetEventDrivenSourceNames());
            //allSources.AddRange(GetManualSources());
            return allSources;
        }

        #region Helper Methods

        /// <summary>
        /// Checks if an assembly is a system assembly to skip during reflection
        /// </summary>
        private static bool IsSystemAssembly(Assembly assembly)
        {
            var assemblyName = assembly.FullName;
            return assemblyName.StartsWith("System.") ||
                   assemblyName.StartsWith("Microsoft.") ||
                   assemblyName.StartsWith("mscorlib") ||
                   assemblyName.StartsWith("netstandard") ||
                   assemblyName.StartsWith("Radzen");
        }

        /// <summary>
        /// Creates an instance of an event type for metadata extraction
        /// Uses reflection with fallback for parameterless constructors
        /// </summary>
        private static SMS_Domain.Interfaces.IEventSource? CreateEventInstance(Type eventType)
        {
            try
            {
                // Try to create with default constructor first
                if (eventType.GetConstructor(Type.EmptyTypes) != null)
                {
                    return Activator.CreateInstance(eventType) as SMS_Domain.Interfaces.IEventSource;
                }

                // For events with required parameters, try constructors in order of least parameters
                var constructors = eventType.GetConstructors();
                foreach (var constructor in constructors.OrderBy(c => c.GetParameters().Length))
                {
                    var parameters = constructor.GetParameters();
                    var args = new object[parameters.Length];

                    for (int i = 0; i < parameters.Length; i++)
                    {
                        args[i] = GetDefaultValue(parameters[i].ParameterType);
                    }

                    try
                    {
                        return Activator.CreateInstance(eventType, args) as SMS_Domain.Interfaces.IEventSource;
                    }
                    catch
                    {
                        // Try next constructor if this signature cannot be created with default values
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Could not create instance of {eventType.Name}: {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// Gets default value for a parameter type
        /// </summary>
        private static object GetDefaultValue(Type parameterType)
        {
            if (parameterType == typeof(string))
                return "Test";
            if (parameterType == typeof(DateTime))
                return DateTime.UtcNow;
            if (parameterType == typeof(bool))
                return false;
            if (parameterType.IsEnum)
                return Enum.GetValues(parameterType).GetValue(0);

            // Handle strongly typed identifiers and other types that can be built from a string
            var stringConstructor = parameterType.GetConstructor(new[] { typeof(string) });
            if (stringConstructor != null)
                return Activator.CreateInstance(parameterType, "Test")!;

            // Handle classes that support parameterless construction
            if (!parameterType.IsValueType)
            {
                var parameterlessConstructor = parameterType.GetConstructor(Type.EmptyTypes);
                if (parameterlessConstructor != null)
                    return Activator.CreateInstance(parameterType)!;
            }

            if (parameterType.IsValueType)
                return Activator.CreateInstance(parameterType);

            return null!;
        }

        #endregion
    }

    /// <summary>
    /// Information about a domain event that can serve as an SPI data source
    /// </summary>
    public class EventDataSourceInfo
    {
        public Type EventType { get; set; } = null!;
        public string EventTypeName { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsAutomatic { get; set; }
        public int Priority { get; set; }
    }
    }

    /// <summary>
    /// Common validation messages for SPIs
    /// </summary>
    public static class ValidationMessages
    {
        public const string NameRequired = "SPI name is required";
        public const string DescriptionRequired = "Description is required";
        public const string TypeRequired = "Indicator type is required";
        public const string DataSourceRequired = "Data source is required";
        public const string ValueRequired = "Value is required";
        public const string MeasurementDateRequired = "Measurement date is required";
        public const string VerifiedByRequired = "Verified by is required when marking as verified";
    }

