namespace sorovi.DependencyInjection.AutoRegister.TestAssembly
{
    public interface IService
    {
    }

    [Service(typeof(IService))]
    public class WithServiceAttributeButGiveInterface : IService
    {
    }
}