namespace sorovi.DependencyInjection.AutoRegister.TestAssembly
{
    [TransientService(typeof(IService))]
    public class WithTransientServiceAttributeAndInheritInterfaceSecond : IService
    {
    }
}