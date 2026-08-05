using System.Reflection;
using SettleMate.Abstractions;
using SettleMate.Extensions;
using SettleMate.Pipelines;

public static class HandlerRegistrationExtensions
{
    public static IServiceCollection AddHandlersFromAssembly(this IServiceCollection services, Assembly assembly)
    {
        var types = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && !t.ContainsGenericParameters)
            .ToList();

        foreach (var implementation in types)
        {
            foreach (var iface in implementation.GetInterfaces())
            {
                if (iface.IsGenericType && iface.GetGenericTypeDefinition() == typeof(IHandler<,>))
                    services.AddScoped(iface, implementation);

                if (iface.IsGenericType && iface.GetGenericTypeDefinition() == typeof(IEventHandler<>))
                    services.AddScoped(iface, implementation);
            }
        }

        services.Decorate(typeof(IHandler<,>), typeof(ValidationDecorator<,>));
        services.Decorate(typeof(IHandler<,>), typeof(LoggingDecorator<,>));

        services.AddScoped<IEventDispatcher, EventDispatcher>();

        return services;
    }
}
