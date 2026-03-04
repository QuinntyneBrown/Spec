using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Spec.Cli;

public static class InstallCommand
{
    public static Command Create(IServiceProvider serviceProvider)
    {
        var command = new Command("install", "Install the Spec skill for Claude Code");

        command.SetAction((parseResult, cancellationToken) =>
        {
            var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
            var installer = serviceProvider.GetRequiredService<ISkillInstaller>();

            installer.Install();
            logger.LogInformation("Spec skill installed successfully to .claude/skills/spec/SKILL.md");

            return Task.FromResult(0);
        });

        return command;
    }
}
