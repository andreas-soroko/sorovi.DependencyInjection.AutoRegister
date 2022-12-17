# sorovi.DependencyInjection.AutoRegister

[![NuGet](https://img.shields.io/nuget/v/sorovi.DependencyInjection.AutoRegister.svg?style=flat-square)](https://www.nuget.org/packages/sorovi.DependencyInjection.AutoRegister/)
[![NuGet](https://img.shields.io/nuget/dt/sorovi.DependencyInjection.AutoRegister.svg?style=flat-square)](https://www.nuget.org/packages/sorovi.DependencyInjection.AutoRegister/)

You are tired from adding every service to the DI Container(ASP.Net)? Great, this library is exactly what you are looking for. It searches for a couple of attributes on your classes and will add them automatically!


---

### Examples

##### Quickstart

just call `RegisterServices` inside of the `ConfigureServices`-method` and it will search for all classes in the entry assembly for the following attributes:

- TransientService
- ScopedService
- SingletonService
- BackgroundService

```csharp
[TransientService(typeof(IMyService))]
public class MyService : IMyService { }
```

```csharp
public void ConfigureServices(IServiceCollection serviceCollection)
{
   
   serviceCollection.RegisterServices()
   // ...
}
```

##### search in multiple assemblies

```csharp   
serviceCollection.RegisterServices(
    Assembly.GetEntryAssembly(),
    typeof(ServiceInAnotherAssembly).Assembly
)
```

##### with additional filter

you can easily add a filter to only search in certain classes. You'll only need this if the default is not strict enough

```csharp   
serviceCollection.RegisterServices(configure => configure
    .WithTypeFilter( type => type.Name.EndsWith("Service"))
);
```

## Benchmark

> BenchmarkDotNet=v0.13.2, OS=macOS 13.0.1 (22A400) [Darwin 22.1.0]  
> Intel Core i7-9750H CPU 2.60GHz, 1 CPU, 12 logical and 6 physical cores  
> .NET SDK=6.0.202  
> [Host]     : .NET 6.0.4 (6.0.422.16404), X64 RyuJIT AVX2  
> DefaultJob : .NET 6.0.4 (6.0.422.16404), X64 RyuJIT AVX2

|       Method |       Mean |   Error   |  StdDev  | Ratio | RatioSD |   Gen0 | Allocated | Alloc Ratio |
|------------- |------------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
|  ManuallyAdd |    240.9 ns |  4.58 ns |  4.70 ns |  1.00 |    0.00 | 0.1032 |    648 B  |        1.00 |
| AutoRegister | 20,649.6 ns | 85.00 ns | 75.35 ns | 85.51 |    1.54 | 0.2441 |   1617 B  |        2.50 |