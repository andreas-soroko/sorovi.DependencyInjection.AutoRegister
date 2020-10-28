using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace sorovi.DependencyInjection.AutoRegister
{
    public static class ServiceCollectionExtension
    {
        private delegate IServiceCollection AddTypeDelegate(Type serviceType);

        private delegate IServiceCollection AddTypeWithInterfaceDelegate(Type interfaceType, Type serviceType);

        public static void RegisterServices(this IServiceCollection services, params Assembly[] assemblies)
        {
            if (assemblies.Length == 0)
                assemblies = new[] {Assembly.GetEntryAssembly()};

            var types = assemblies.SelectMany(
                assembly =>
                    assembly.GetExportedTypes()
                        .Where(type => type.IsClass && !type.IsAbstract && !type.IsNested && !type.IsGenericType)
                        .Select(type => (
                            Type: type,
                            Attribute: (Attribute)
                                       type.GetCustomAttribute<ServiceAttribute>() ??
                                       type.GetCustomAttribute<BackgroundServiceAttribute>()
                        ))
                        .Where(typeInfo => typeInfo.Attribute != null)
            );

            foreach (var typeInfo in types)
            {
                switch (typeInfo.Attribute)
                {
                    case ServiceAttribute serviceAttribute:
                        var (addType, addTypeWithInterface) = GetAddServiceMethods(services, serviceAttribute);

                        if (serviceAttribute.InterfaceType != null)
                        {
                            addTypeWithInterface(serviceAttribute.InterfaceType, typeInfo.Type);
                            continue;
                        }

                        addType(typeInfo.Type);
                        break;
                    case BackgroundServiceAttribute backgroundServiceAttribute:
                        services.AddSingleton(typeof(IHostedService), typeInfo);
                        break;
                }
            }
        }

        private static (AddTypeDelegate addType, AddTypeWithInterfaceDelegate addTypeWithInterface) GetAddServiceMethods(IServiceCollection services, ServiceAttribute initiaAttribute)
        {
            return initiaAttribute.LifeTime switch
            {
                ServiceAttribute.LifeTimeType.Scoped => (services.AddScoped, services.AddScoped),
                ServiceAttribute.LifeTimeType.Singleton => (services.AddSingleton, services.AddSingleton),
                ServiceAttribute.LifeTimeType.Transient => (services.AddTransient, services.AddTransient),
                _ => throw new Exception("unknown lifetime method")
            };
        }
    }
}