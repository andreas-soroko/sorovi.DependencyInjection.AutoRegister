namespace sorovi.DependencyInjection.AutoRegister.TestAssembly
{
    [TransientService(typeof(IService2), Mode = Mode.Add)]
    public class ShouldAddClassMultipleTimes : IService2
    {
    }

    [TransientService(typeof(IService2), Mode = Mode.Add)]
    public class ShouldAddClassMultipleTimesSecond : IService2
    {
    }
}