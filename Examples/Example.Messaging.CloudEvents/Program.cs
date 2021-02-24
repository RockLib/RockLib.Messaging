using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RockLib.Messaging.DependencyInjection;
using System.Threading.Tasks;

namespace Example.Messaging.CloudEvents
{
    class Program
    {
        static Task Main(string[] args)
        {
            return CreateHostBuilder(args).RunConsoleAsync(options => options.SuppressStatusMessages = true);
        }

        static IHostBuilder CreateHostBuilder(string[] args)
        {
            IHostBuilder hostBuilder = Host.CreateDefaultBuilder(args);
            return hostBuilder.ConfigureServices((context, services) =>
            {
                services.AddNamedPipeSender("user-pipe");
                services.AddNamedPipeReceiver("user-pipe");

                services.AddNamedPipeSender("worker-pipe-1");
                services.AddNamedPipeReceiver("worker-pipe-1");

                services.AddNamedPipeSender("worker-pipe-2");
                services.AddNamedPipeReceiver("worker-pipe-2");

                services.Configure<ExampleOptions>(context.Configuration.GetSection("examplesettings"));
                services.AddHostedService<ExampleService>();
            });
        }
    }
}
