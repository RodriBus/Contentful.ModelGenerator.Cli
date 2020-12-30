using CliFx;
using Contentful.ModelGenerator.Cli.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Contentful.ModelGenerator.Cli
{
    internal static class Program
    {
        public static async Task<int> Main()
        {
            Console.Title = Utils.ToolHelper.GetToolName();

            var services = new ServiceCollection();

            services.AddHttpClient();

            // Register services
            services.AddSingleton<Utils.IModelGenerator, Utils.ModelGenerator>();

            services.AddSingleton(_ => Konsole.Window.HostConsole);

            // Register commands
            services.AddTransient<Commands.GenerateModelsCommand>();
            services.AddTransient<IModelGeneratorReporter, ModelGeneratorReporter>();

            var serviceProvider = services.BuildServiceProvider();

            return await new CliApplicationBuilder()
                .UseTypeActivator(serviceProvider.GetService)
                .AddCommandsFromThisAssembly()
                .UseExecutableName(Utils.ToolHelper.GetToolExecutableName())
                .Build()
                .RunAsync();
        }
    }
}