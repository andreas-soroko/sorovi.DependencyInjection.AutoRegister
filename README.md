# sorovi.DependencyInjection.AutoRegister

[![NuGet](https://img.shields.io/nuget/v/sorovi.DependencyInjection.AutoRegister.svg?style=flat-square)](https://www.nuget.org/packages/sorovi.DependencyInjection.AutoRegister/)
[![NuGet](https://img.shields.io/nuget/dt/sorovi.DependencyInjection.AutoRegister.svg?style=flat-square)](https://www.nuget.org/packages/sorovi.DependencyInjection.AutoRegister/)

You are tired from adding every service to the (MS.)DI Container? Great, that library is exactly what you are looking for. It searchs for a couple of attributes on your classes and will add them automatically!


---

### Examples

##### Quickstart

just add the `RegisterServices`  and it will search for all classes in the entry assembly for the following attributes

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

you can easily create more fine granular filter if the default one is not strict enough

```csharp   
serviceCollection.RegisterServices(configure => configure
    .WithTypeFilter( type => type.Name.EndsWith("Service"))
);
```