using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Workflows.Application.Messaging;

public static class MessagingServiceCollectionExtensions
{
    public static IServiceCollection AddSlimMediator(this IServiceCollection services, params Assembly[] assemblies)
    {
        services.AddScoped<ISender, DefaultSender>();

        foreach (Assembly asm in assemblies)
        {
            foreach (TypeInfo t in asm.DefinedTypes)
            {
                foreach (Type i in t.ImplementedInterfaces)
                {
                    if (i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>))
                    {
                        services.AddTransient(i, t);
                    }
                }
            }
        }

        return services;
    }
}