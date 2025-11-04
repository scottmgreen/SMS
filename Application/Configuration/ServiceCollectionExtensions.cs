using SMS_Application.Messaging.Pipelines;

namespace SMS_Application.Configuration;

internal static class ServiceCollectionExtensions
{

    public static IServiceCollection AddMediator(this IServiceCollection services, Assembly assembly)
    {

        services.AddTransient<IMediator, Mediator>();
        services.Register(typeof(IRequestHandler<,>), LifeTime.Scoped);
        services.AddTransient(typeof(IPipeline<,>), typeof(LoggingPipeline<,>));
        services.AddTransient(typeof(IPipeline<,>), typeof(AuditLogPipeline<,>));
        return services;

    }

    private static IServiceCollection Register(this IServiceCollection services, Type genericInterface, LifeTime lifetime)
    {
        var assembly = genericInterface.Assembly;
        var allTypes = assembly.GetTypes();
        var interfaceTypes = new List<Type>();

        foreach (var type in assembly.GetTypes())
        {
            interfaceTypes.AddRange(type.GetInterfaces().Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == genericInterface));

        }

        foreach (var @interface in interfaceTypes)
        {
            var @class = allTypes.First(x => x.IsAssignableTo(@interface));
            Register(@interface, @class, services, lifetime);
        }

        return services;
    }

    private static void Register(Type @interface, Type @class, IServiceCollection services, LifeTime lifeTime)
    {
        switch (lifeTime)
        {
            case LifeTime.Singleton:
                services.AddSingleton(@interface, @class);
                break;
            case LifeTime.Transient:
                services.AddTransient(@interface, @class);
                break;
            case LifeTime.Scoped:
                services.AddScoped(@interface, @class);
                break;
            default:
                throw new UnreachableException();
        }

    }

    public enum LifeTime
    {
        Singleton,
        Transient,
        Scoped
    }

}
