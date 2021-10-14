using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using sorovi.DependencyInjection.AutoRegister.Common;
using sorovi.DependencyInjection.AutoRegister.Exceptions;

namespace sorovi.DependencyInjection.AutoRegister
{
    public static class ServiceCollectionExtension
    {
        private delegate void AddTypeDelegate(Type serviceType);

        private delegate void AddTypeWithInterfaceDelegate(Type interfaceType, Type serviceType);


        /// <summary>
        /// Finds all classes with <see cref="TransientServiceAttribute"/>, <see cref="ScopedServiceAttribute"/>, <see cref="SingletonServiceAttribute"/>
        /// or <see cref="BackgroundServiceAttribute"/> and will register it
        /// </summary>
        /// <param name="services"></param>
        /// <param name="assemblies">Assemblies to look for the attributes, if null EntryAssembly will be used</param>
        public static void RegisterServices(this IServiceCollection services, params Assembly[] assemblies)
        {
            RegisterServices(services, assemblies, null);
        }

        /// <summary>
        /// Finds all classes with <see cref="TransientServiceAttribute"/>, <see cref="ScopedServiceAttribute"/>, <see cref="SingletonServiceAttribute"/>
        /// or <see cref="BackgroundServiceAttribute"/> and will register it
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configure"></param>
        public static void RegisterServices(this IServiceCollection services, Func<AutoRegisterConfigure, AutoRegisterConfigure> configure)
        {
            var autoRegisterConfigure = configure(new AutoRegisterConfigure());

            RegisterServices(services, autoRegisterConfigure.Assemblies, autoRegisterConfigure.TypeFilter);
        }

        /// <summary>
        /// Finds all classes with <see cref="TransientServiceAttribute"/>, <see cref="ScopedServiceAttribute"/>, <see cref="SingletonServiceAttribute"/>
        /// or <see cref="BackgroundServiceAttribute"/> and will register it
        /// </summary>
        /// <param name="services"></param>
        /// <param name="assemblies">Assemblies to look for the attributes, if null EntryAssembly will be used</param>
        /// <param name="predicate">Additional filter options</param>
        /// <exception cref="MissingInterfaceImplException"></exception>
        public static void RegisterServices(this IServiceCollection services, Assembly[] assemblies, Predicate<Type> predicate = null)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            var alreadyKnownAssembliesDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(AlreadyKnownAssemblies));
            var alreadyKnownAssemblies = alreadyKnownAssembliesDescriptor?.ImplementationInstance as AlreadyKnownAssemblies ?? new AlreadyKnownAssemblies();

            if (assemblies is null || assemblies.Length == 0)
                assemblies = new[] { Assembly.GetEntryAssembly() };

            assemblies = assemblies
                .Where(assembly => !alreadyKnownAssemblies.KnownAssemblies.Contains(assembly))
                .ToArray();

            var types = assemblies.SelectMany(
                    assembly =>
                        assembly.GetExportedTypes()
                            .Where(type =>
                            {
                                var defaultCondition = type.IsClass && !type.IsAbstract && !type.IsNested && !type.IsGenericType;
                                if (predicate is null)
                                {
                                    return defaultCondition;
                                }

                                return defaultCondition && predicate(type);
                            })
                            .Select(type => (
                                Type: type,
                                Attribute: (Attribute)
                                           type.GetCustomAttribute<ServiceAttribute>() ??
                                           type.GetCustomAttribute<BackgroundServiceAttribute>()
                            ))
                            .Where(typeInfo => typeInfo.Attribute != null)
                )
                .ToArray();


            var serviceCollectionMethods = GetAddServiceMethods(services);
            foreach (var typeInfo in types)
            {
                switch (typeInfo.Attribute)
                {
                    case ServiceAttribute serviceAttribute:

                        if (!serviceCollectionMethods.ContainsKey(serviceAttribute.Mode))
                        {
                            throw new Exception($"unknown 'Mode': {serviceAttribute.Mode}");
                        }

                        if (!serviceCollectionMethods[serviceAttribute.Mode].ContainsKey(serviceAttribute.GetType()))
                        {
                            throw new Exception($"unknown lifetime attribute: {serviceAttribute.GetType().FullName}");
                        }

                        var (addType, addTypeWithInterface) = serviceCollectionMethods[serviceAttribute.Mode][serviceAttribute.GetType()];

                        if (serviceAttribute.InterfaceType != null)
                        {
                            if (!serviceAttribute.InterfaceType.IsAssignableFrom(typeInfo.Type))
                            {
                                throw new MissingInterfaceImplException(typeInfo.Type, serviceAttribute.InterfaceType);
                            }

                            addTypeWithInterface(serviceAttribute.InterfaceType, typeInfo.Type);
                            continue;
                        }

                        addType(typeInfo.Type);
                        break;
                    case BackgroundServiceAttribute _:
                        services.AddSingleton(typeof(IHostedService), typeInfo);
                        break;
                }
            }

            if (alreadyKnownAssembliesDescriptor != null)
            {
                services.Remove(alreadyKnownAssembliesDescriptor);
            }

            services.AddSingleton(new AlreadyKnownAssemblies(alreadyKnownAssemblies.KnownAssemblies.Concat(assemblies).ToArray()));
        }

        private static Dictionary<Mode, Dictionary<Type, (AddTypeDelegate addType, AddTypeWithInterfaceDelegate addTypeWithInterface)>> GetAddServiceMethods(IServiceCollection services) =>
            new Dictionary<Mode, Dictionary<Type, (AddTypeDelegate addType, AddTypeWithInterfaceDelegate addTypeWithInterface)>>()
            {
                [Mode.Add] = new Dictionary<Type, (AddTypeDelegate addType, AddTypeWithInterfaceDelegate addTypeWithInterface)>()
                {
                    [typeof(SingletonServiceAttribute)] = (type => services.AddSingleton(type), (type, interfaceType) => services.AddSingleton(type, interfaceType)),
                    [typeof(ScopedServiceAttribute)] = (type => services.AddScoped(type), (type, interfaceType) => services.AddScoped(type, interfaceType)),
                    [typeof(TransientServiceAttribute)] = (type => services.AddTransient(type), (type, interfaceType) => services.AddTransient(type, interfaceType)),
                },
                [Mode.TryAdd] = new Dictionary<Type, (AddTypeDelegate addType, AddTypeWithInterfaceDelegate addTypeWithInterface)>()
                {
                    [typeof(SingletonServiceAttribute)] = (services.TryAddSingleton, services.TryAddSingleton),
                    [typeof(ScopedServiceAttribute)] = (services.TryAddScoped, services.TryAddScoped),
                    [typeof(TransientServiceAttribute)] = (services.TryAddTransient, services.TryAddTransient),
                }
            };

        private class AlreadyKnownAssemblies
        {
            public IReadOnlyCollection<Assembly> KnownAssemblies { get; }

            public AlreadyKnownAssemblies()
            {
                KnownAssemblies = Array.Empty<Assembly>();
            }

            public AlreadyKnownAssemblies(IReadOnlyCollection<Assembly> knownAssemblies)
            {
                KnownAssemblies = knownAssemblies;
            }
        }
    }
}