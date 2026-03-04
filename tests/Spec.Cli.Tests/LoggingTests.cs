using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Spec.Cli.Tests;

public class LoggingTests
{
    [Fact]
    public void ConfigureServices_RegistersLogging()
    {
        var services = new ServiceCollection();
        Program.ConfigureServices(services);
        var provider = services.BuildServiceProvider();

        var logger = provider.GetService<ILogger<Program>>();

        Assert.NotNull(logger);
    }

    [Fact]
    public void ConfigureServices_RegistersLoggerFactory()
    {
        var services = new ServiceCollection();
        Program.ConfigureServices(services);
        var provider = services.BuildServiceProvider();

        var loggerFactory = provider.GetService<ILoggerFactory>();

        Assert.NotNull(loggerFactory);
    }
}
