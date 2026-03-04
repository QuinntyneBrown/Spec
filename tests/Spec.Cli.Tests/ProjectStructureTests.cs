using System.CommandLine;

namespace Spec.Cli.Tests;

public class ProjectStructureTests
{
    [Fact]
    public void RootCommand_IsConfigured()
    {
        var command = Program.CreateRootCommand();

        Assert.NotNull(command);
        Assert.IsType<RootCommand>(command);
    }

    [Fact]
    public async Task RootCommand_Help_ReturnsZeroExitCode()
    {
        var command = Program.CreateRootCommand();
        var parseResult = command.Parse("--help");

        var exitCode = await parseResult.InvokeAsync();

        Assert.Equal(0, exitCode);
    }

    [Fact]
    public async Task RootCommand_Help_OutputContainsDescription()
    {
        var command = Program.CreateRootCommand();
        var parseResult = command.Parse("--help");

        var writer = new StringWriter();
        Console.SetOut(writer);

        await parseResult.InvokeAsync();

        var output = writer.ToString();
        Assert.Contains("spec", output, StringComparison.OrdinalIgnoreCase);
    }
}
