using System.Reflection;
using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using sorovi.DependencyInjection.AutoRegister.TestAssembly;

namespace sorovi.DependencyInjection.AutoRegister.Benchmark;

[MemoryDiagnoser]
public class AddingToServiceCollectionBenchmark
{
    private static readonly Predicate<Type> _defaultTypeFilter = type => type.Name != nameof(WithTransientServiceAttributeAndInterface);
    private static readonly Func<AutoRegisterConfigure, AutoRegisterConfigure> _configure = c => c.WithTypeFilter(_defaultTypeFilter).WithAssemblies(typeof(JustAClass).Assembly);

    [Benchmark(Baseline = true)]
    public IServiceCollection ManuallyAdd()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddHostedService<BackgroundService>();

        serviceCollection.AddTransient<WithTransientServiceAttribute>();
        serviceCollection.AddScoped<WithScopedServiceAttribute>();
        serviceCollection.AddSingleton<WithSingletonServiceAttribute>();

        serviceCollection.AddTransient<IService, WithTransientServiceAttributeAndInheritInterface>();
        serviceCollection.AddTransient<IService, WithTransientServiceAttributeAndInheritInterfaceSecond>();

        serviceCollection.AddTransient<IService2, ShouldAddClassMultipleTimes>();
        serviceCollection.AddTransient<IService2, ShouldAddClassMultipleTimesSecond>();

        return serviceCollection;
    }

    [Benchmark]
    public IServiceCollection AutoRegister()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.RegisterServices(_configure);
        return serviceCollection;
    }
}