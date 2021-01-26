using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using sorovi.DependencyInjection.AutoRegister.Exceptions;

namespace sorovi.DependencyInjection.AutoRegister
{
    public static class ServiceCollectionExtension
    {
        private delegate IServiceCollection AddTypeDelegate(Type serviceType);

        private delegate IServiceCollection AddTypeWithInterfaceDelegate(Type interfaceType, Type serviceType);


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
            if (assemblies.Length == 0)
                assemblies = new[] {Assembly.GetEntryAssembly()};

            var types = assemblies.SelectMany(
                assembly =>
                    assembly.GetExportedTypes()
                        .Where(type =>
                        {
                            var defaultCondition = type.IsClass && !type.IsAbstract && !type.IsNested && !type.IsGenericType;
                            if (predicate is null) { return defaultCondition; }

                            return defaultCondition && predicate(type);
                        })
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
                            if (!serviceAttribute.InterfaceType.IsAssignableFrom(typeInfo.Type)) { throw new MissingInterfaceImplException(typeInfo.Type, serviceAttribute.InterfaceType); }

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
        }

        private static (AddTypeDelegate addType, AddTypeWithInterfaceDelegate addTypeWithInterface) GetAddServiceMethods(
            IServiceCollection services,
            ServiceAttribute initiaAttribute
        ) =>
            initiaAttribute switch
            {
                SingletonServiceAttribute _ => (services.AddSingleton, services.AddSingleton),
                ScopedServiceAttribute _ => (services.AddScoped, services.AddScoped),
                TransientServiceAttribute _ => (services.AddTransient, services.AddTransient),
                _ => throw new Exception("unknown lifetime")
            };
    }
}