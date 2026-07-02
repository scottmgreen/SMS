using System.Reflection;
using SMS_Domain.Interfaces;
using SMS_Domain.Enums;

namespace SMS_Application.Services;

/// <summary>
/// Reflection-based registry that discovers queued event CLR types by EventType value.
/// Eliminates manual replay maps in EventQueueService.
/// </summary>
public sealed class QueuedEventTypeRegistry
{
    private readonly ILogger<QueuedEventTypeRegistry> _logger;
    private readonly Dictionary<string, Type> _domainEventTypes = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, Type> _integrationEventTypes = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, Type> _uiEventTypes = new(StringComparer.OrdinalIgnoreCase);

    public QueuedEventTypeRegistry(ILogger<QueuedEventTypeRegistry> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        DiscoverAll();
    }

    public bool TryResolveDomainEventType(string eventType, out Type type)
        => _domainEventTypes.TryGetValue(eventType, out type!);

    public bool TryResolveIntegrationEventType(string eventType, out Type type)
        => _integrationEventTypes.TryGetValue(eventType, out type!);

    public bool TryResolveUIEventType(string eventType, out Type type)
        => _uiEventTypes.TryGetValue(eventType, out type!);

    public IReadOnlyCollection<string> GetKnownTypes(EventCategory category)
    {
        if (category == EventCategory.DomainEvent)
        {
            return _domainEventTypes.Keys.ToList();
        }

        if (category == EventCategory.IntegrationEvent)
        {
            return _integrationEventTypes.Keys.ToList();
        }

        if (category == EventCategory.UIEvent)
        {
            return _uiEventTypes.Keys.ToList();
        }

        return Array.Empty<string>();
    }

    private void DiscoverAll()
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => !a.IsDynamic)
            .Where(a => !IsSystemAssembly(a.FullName))
            .ToList();

        foreach (var assembly in assemblies)
        {
            foreach (var type in GetLoadableTypes(assembly))
            {
                if (type.IsAbstract || type.IsInterface)
                {
                    continue;
                }

                if (typeof(IBaseDomainEvent).IsAssignableFrom(type))
                {
                    RegisterEventType(_domainEventTypes, type, "Domain");
                }

                if (typeof(IBaseIntegrationEvent).IsAssignableFrom(type))
                {
                    RegisterEventType(_integrationEventTypes, type, "Integration");
                }

                if (typeof(IBaseUIEvent).IsAssignableFrom(type))
                {
                    RegisterEventType(_uiEventTypes, type, "UI");
                }
            }
        }

        _logger.LogInformation(
            "QueuedEventTypeRegistry discovered {DomainCount} domain, {IntegrationCount} integration, and {UICount} UI event types.",
            _domainEventTypes.Count,
            _integrationEventTypes.Count,
            _uiEventTypes.Count);
    }

    private void RegisterEventType(IDictionary<string, Type> targetMap, Type eventClrType, string categoryName)
    {
        var eventTypeValue = ResolveEventTypeValue(eventClrType);
        if (string.IsNullOrWhiteSpace(eventTypeValue))
        {
            _logger.LogDebug("QueuedEventTypeRegistry could not resolve EventType for {Category} CLR type {TypeName}", categoryName, eventClrType.FullName);
            return;
        }

        if (targetMap.TryGetValue(eventTypeValue, out var existing))
        {
            if (existing != eventClrType)
            {
                var winner = ChooseDeterministicType(existing, eventClrType);
                targetMap[eventTypeValue] = winner;
                var loser = winner == existing ? eventClrType : existing;

                _logger.LogWarning(
                    "QueuedEventTypeRegistry duplicate EventType '{EventType}' for {Category}. Deterministically selected {WinnerType}, skipped {LoserType}.",
                    eventTypeValue,
                    categoryName,
                    winner.FullName,
                    loser.FullName);
            }

            return;
        }

        targetMap[eventTypeValue] = eventClrType;
    }

    private static Type ChooseDeterministicType(Type left, Type right)
    {
        var leftName = left.FullName ?? left.Name;
        var rightName = right.FullName ?? right.Name;

        return string.CompareOrdinal(leftName, rightName) <= 0 ? left : right;
    }

    private string? ResolveEventTypeValue(Type eventClrType)
    {
        // Preferred convention used by some UI/Test events.
        var staticTypeValue = eventClrType.GetField("TypeValue", BindingFlags.Public | BindingFlags.Static)?.GetValue(null) as string;
        if (!string.IsNullOrWhiteSpace(staticTypeValue))
        {
            return staticTypeValue;
        }

        var staticTypeValueProp = eventClrType.GetProperty("TypeValue", BindingFlags.Public | BindingFlags.Static)?.GetValue(null) as string;
        if (!string.IsNullOrWhiteSpace(staticTypeValueProp))
        {
            return staticTypeValueProp;
        }

        // Fallback: instantiate and read EventType from interface implementation.
        var instance = TryCreateInstance(eventClrType);
        if (instance is IBaseDomainEvent domainEvent)
        {
            return domainEvent.EventType;
        }

        if (instance is IBaseIntegrationEvent integrationEvent)
        {
            return integrationEvent.EventType;
        }

        if (instance is IBaseUIEvent uiEvent)
        {
            return uiEvent.EventType;
        }

        return null;
    }

    private object? TryCreateInstance(Type eventClrType)
    {
        try
        {
            var parameterlessCtor = eventClrType.GetConstructor(Type.EmptyTypes);
            if (parameterlessCtor != null)
            {
                return Activator.CreateInstance(eventClrType);
            }

            var constructors = eventClrType.GetConstructors(BindingFlags.Public | BindingFlags.Instance)
                .OrderBy(c => c.GetParameters().Length)
                .ToList();

            foreach (var ctor in constructors)
            {
                var parameters = ctor.GetParameters();
                var args = new object?[parameters.Length];

                var canConstruct = true;
                for (var i = 0; i < parameters.Length; i++)
                {
                    if (!TryCreateDefaultValue(parameters[i].ParameterType, out var arg))
                    {
                        canConstruct = false;
                        break;
                    }

                    args[i] = arg;
                }

                if (!canConstruct)
                {
                    continue;
                }

                try
                {
                    return ctor.Invoke(args);
                }
                catch
                {
                    // try the next constructor
                }
            }
        }
        catch
        {
            // ignored intentionally; caller handles null
        }

        return null;
    }

    private static bool TryCreateDefaultValue(Type type, out object? value)
    {
        if (type == typeof(string))
        {
            value = "QueuedEventRegistry";
            return true;
        }

        if (type == typeof(Guid))
        {
            value = Guid.NewGuid();
            return true;
        }

        if (type == typeof(DateTime))
        {
            value = DateTime.UtcNow;
            return true;
        }

        if (type == typeof(DateTimeOffset))
        {
            value = DateTimeOffset.UtcNow;
            return true;
        }

        if (type == typeof(bool))
        {
            value = false;
            return true;
        }

        if (type == typeof(int))
        {
            value = 0;
            return true;
        }

        if (type == typeof(decimal))
        {
            value = 0m;
            return true;
        }

        if (type == typeof(double))
        {
            value = 0d;
            return true;
        }

        if (type == typeof(float))
        {
            value = 0f;
            return true;
        }

        if (type == typeof(TimeSpan))
        {
            value = TimeSpan.Zero;
            return true;
        }

        if (type.IsEnum)
        {
            value = Enum.GetValues(type).GetValue(0);
            return true;
        }

        var nullableUnderlying = Nullable.GetUnderlyingType(type);
        if (nullableUnderlying != null)
        {
            value = null;
            return true;
        }

        var stringCtor = type.GetConstructor(new[] { typeof(string) });
        if (stringCtor != null)
        {
            value = stringCtor.Invoke(new object[] { "QueuedEventRegistry" });
            return true;
        }

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
        {
            value = Activator.CreateInstance(type);

            // Some event constructors require at least one recipient, etc.
            // Seed List<string> with a default value to satisfy guard clauses.
            if (type.GetGenericArguments()[0] == typeof(string) && value is System.Collections.IList list)
            {
                list.Add("system@localhost");
            }

            return true;
        }

        if (type.IsArray)
        {
            value = Array.CreateInstance(type.GetElementType()!, 0);
            return true;
        }

        if (!type.IsValueType)
        {
            var parameterlessCtor = type.GetConstructor(Type.EmptyTypes);
            if (parameterlessCtor != null)
            {
                value = parameterlessCtor.Invoke(Array.Empty<object>());
                return true;
            }

            value = null;
            return true;
        }

        value = Activator.CreateInstance(type);
        return value != null;
    }

    private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            return ex.Types.Where(t => t != null)!;
        }
    }

    private static bool IsSystemAssembly(string? assemblyName)
    {
        if (string.IsNullOrWhiteSpace(assemblyName))
        {
            return true;
        }

        return assemblyName.StartsWith("System.", StringComparison.Ordinal) ||
               assemblyName.StartsWith("Microsoft.", StringComparison.Ordinal) ||
               assemblyName.StartsWith("mscorlib", StringComparison.OrdinalIgnoreCase) ||
               assemblyName.StartsWith("netstandard", StringComparison.OrdinalIgnoreCase) ||
               assemblyName.StartsWith("Windows", StringComparison.OrdinalIgnoreCase);
    }
}
