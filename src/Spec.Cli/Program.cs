using System.CommandLine;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Spec.Cli;

public class Program
{
    public static async Task<int> Main(string[] args)
    {
        var configuration = BuildConfiguration();

        var services = new ServiceCollection();
        ConfigureServices(services, configuration);
        using var serviceProvider = services.BuildServiceProvider();

        var rootCommand = CreateRootCommand(serviceProvider);
        var parseResult = rootCommand.Parse(args);
        return await parseResult.InvokeAsync();
    }

    public static IConfiguration BuildConfiguration(string? basePath = null)
    {
        basePath ??= Directory.GetCurrentDirectory();

        var builder = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables();

        return builder.Build();
    }

    public static void ConfigureServices(IServiceCollection services, IConfiguration? configuration = null)
    {
        configuration ??= BuildConfiguration();

        services.AddSingleton(configuration);

        services.AddLogging(builder =>
        {
            builder.AddConsole();
        });

        services.AddSingleton<ISkillInstaller>(new SkillInstaller(Directory.GetCurrentDirectory()));
    }

    public static RootCommand CreateRootCommand(IServiceProvider? serviceProvider = null)
    {
        var rootCommand = new RootCommand("spec - A CLI tool for managing specifications");

        if (serviceProvider != null)
        {
            rootCommand.Add(InstallCommand.Create(serviceProvider));
        }

        return rootCommand;
    }
}
