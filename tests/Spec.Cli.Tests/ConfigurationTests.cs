using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Spec.Cli.Tests;

public class ConfigurationTests
{
    [Fact]
    public void ConfigureServices_RegistersIConfiguration()
    {
        var services = new ServiceCollection();
        Program.ConfigureServices(services);
        var provider = services.BuildServiceProvider();

        var configuration = provider.GetService<IConfiguration>();

        Assert.NotNull(configuration);
    }

    [Fact]
    public void Configuration_LoadsFromJsonFile_WhenPresent()
    {
        // Create a temporary appsettings.json
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        var settingsPath = Path.Combine(tempDir, "appsettings.json");
        File.WriteAllText(settingsPath, """{"TestKey": "TestValue"}""");

        try
        {
            var configuration = Program.BuildConfiguration(tempDir);
            Assert.Equal("TestValue", configuration["TestKey"]);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void Configuration_LoadsFromEnvironmentVariables()
    {
        var envKey = $"SPEC_TEST_{Guid.NewGuid():N}";
        Environment.SetEnvironmentVariable(envKey, "EnvValue");

        try
        {
            var configuration = Program.BuildConfiguration();
            Assert.Equal("EnvValue", configuration[envKey]);
        }
        finally
        {
            Environment.SetEnvironmentVariable(envKey, null);
        }
    }

    [Fact]
    public void Configuration_DoesNotThrow_WhenAppSettingsJsonMissing()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            var configuration = Program.BuildConfiguration(tempDir);
            Assert.NotNull(configuration);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }
}
