using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace sorovi.DependencyInjection.AutoRegister.TestAssembly
{
    [BackgroundService]
    public class WithBackgroundServiceAttribute : BackgroundService
    {
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.CompletedTask;
        }
    }
}