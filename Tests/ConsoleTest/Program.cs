using System;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ConsoleTest
{
    class Program
    {
        private static IHost __Hosting;
        public static IHost Hosting => __Hosting ??= CreateHostBuilder(Environment.GetCommandLineArgs()).Build();
        public static IHostBuilder CreateHostBuilder(string[] args) => Host
           .CreateDefaultBuilder(args)
           .ConfigureLogging(log => log.AddFile())
           .ConfigureServices(ConfigureServices)
        ;

        private static void ConfigureServices(HostBuilderContext host, IServiceCollection services) => services.AddScoped<TestsService>();

        static async Task Main(string[] args)
        {
            using var host = Hosting;
            await host.StartAsync();

            var service = host.Services.GetRequiredService<TestsService>();
            service.Invoke();

            Console.WriteLine("Completed.");
            Console.ReadLine();
            await host.StopAsync();
        }
    }
}
