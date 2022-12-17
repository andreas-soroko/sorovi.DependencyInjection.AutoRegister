using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using sorovi.DependencyInjection.AutoRegister.Common;
using sorovi.DependencyInjection.AutoRegister.Exceptions;

namespace sorovi.DependencyInjection.AutoRegister
{
    public static class ServiceCollectionExtension
    {
        private static readonly MethodInfo _addHostedServiceMethodInfo = typeof(ServiceCollectionHostedServiceExtensions).GetMethod(nameof(ServiceCollectionHostedServiceExtensions.AddHostedService));


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
        public static void RegisterServices(this IServiceCollection services, Assembly[] assemblies, Predicate<Type> predicate)
        {
            if (services is null) { throw new ArgumentNullException(nameof(services)); }

            if (assemblies is null || assemblies.Length == 0) { assemblies = new[] { Assembly.GetEntryAssembly() }; }

            var alreadyKnownAssembliesDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(AlreadyKnownAssemblies));
            var alreadyKnownAssemblies = alreadyKnownAssembliesDescriptor?.ImplementationInstance as AlreadyKnownAssemblies ?? new AlreadyKnownAssemblies();

            assemblies = alreadyKnownAssemblies.KnownAssemblies.Length == 0
                ? assemblies
                : assemblies
                    .Where(assembly => !alreadyKnownAssemblies.KnownAssemblies.Contains(assembly))
                    .ToArray();

            var types = assemblies
                .SelectMany(assembly => assembly.GetExportedTypes())
                .Where(type => IsOfInterest(type) && (predicate is null || predicate(type)));

            foreach (var type in types)
            {
                var attribute = type.GetCustomAttribute(typeof(ServiceAttribute)) ?? type.GetCustomAttribute(typeof(BackgroundServiceAttribute));
                if (attribute is null) { continue; }

                switch (attribute)
                {
                    case ServiceAttribute serviceAttribute:
                        if (serviceAttribute.InterfaceType is not null && !serviceAttribute.InterfaceType.IsAssignableFrom(type)) { throw new MissingInterfaceImplException(type, serviceAttribute.InterfaceType); }

                        var serviceLifetime = attribute switch
                        {
                            TransientServiceAttribute => ServiceLifetime.Transient,
                            ScopedServiceAttribute => ServiceLifetime.Scoped,
                            SingletonServiceAttribute => ServiceLifetime.Singleton,
                            _ => throw new Exception($"unknown lifetime attribute: {attribute.GetType().FullName}")
                        };

                        var serviceDescriptor = ServiceDescriptor.Describe(serviceAttribute.InterfaceType ?? type, type, serviceLifetime);
                        AddServiceDescriptor(services, serviceAttribute.Mode, serviceDescriptor);
                        break;

                    case BackgroundServiceAttribute _:
                        var generic = _addHostedServiceMethodInfo.MakeGenericMethod(type);
                        generic.Invoke(null, new object[] { services });
                        break;
                }
            }

            if (alreadyKnownAssembliesDescriptor != null) { services.Remove(alreadyKnownAssembliesDescriptor); }

            services.AddSingleton(new AlreadyKnownAssemblies(alreadyKnownAssemblies.KnownAssemblies.Concat(assemblies).ToArray()));
        }
        
        private static bool IsOfInterest(in Type type) => type.IsClass && !type.IsAbstract && !type.IsGenericType && !type.IsNested;

        private static void AddServiceDescriptor(in IServiceCollection services, in Mode mode, in ServiceDescriptor descriptor)
        {
            switch (mode)
            {
                case Mode.Add:
                    services.Add(descriptor);
                    break;
                case Mode.TryAdd:
                    services.TryAdd(descriptor);
                    break;
                default:
                    throw new Exception($"unknown 'Mode': {mode}");
            }
        }


        private sealed class AlreadyKnownAssemblies
        {
            public Assembly[] KnownAssemblies { get; }

            public AlreadyKnownAssemblies()
            {
                KnownAssemblies = Array.Empty<Assembly>();
            }

            public AlreadyKnownAssemblies(in Assembly[] knownAssemblies)
            {
                KnownAssemblies = knownAssemblies;
            }
        }
    }
}