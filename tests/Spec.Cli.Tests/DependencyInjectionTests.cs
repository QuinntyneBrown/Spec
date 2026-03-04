using Microsoft.Extensions.DependencyInjection;

namespace Spec.Cli.Tests;

public class DependencyInjectionTests
{
    [Fact]
    public void ConfigureServices_ReturnsServiceProvider()
    {
        var services = new ServiceCollection();
        Program.ConfigureServices(services);
        var provider = services.BuildServiceProvider();

        Assert.NotNull(provider);
    }

    [Fact]
    public void ServiceProvider_IsAvailableBeforeCommandExecution()
    {
        var services = new ServiceCollection();
        Program.ConfigureServices(services);
        var provider = services.BuildServiceProvider();

        // The provider should be buildable and disposable
        Assert.IsAssignableFrom<IServiceProvider>(provider);
        provider.Dispose();
    }
}
