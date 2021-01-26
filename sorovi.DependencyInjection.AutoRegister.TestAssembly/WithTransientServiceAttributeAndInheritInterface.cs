namespace sorovi.DependencyInjection.AutoRegister.TestAssembly
{
    [TransientService(typeof(IService))]
    public class WithTransientServiceAttributeAndInheritInterface : IService
    {
    }
}