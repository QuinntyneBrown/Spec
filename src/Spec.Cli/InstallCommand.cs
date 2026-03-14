using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Spec.Cli;

public static class InstallCommand
{
    public static Command Create(IServiceProvider serviceProvider)
    {
        var command = new Command("install", "Install the Spec skill for Claude Code");

        var globalOption = new Option<bool>("--global", "-g") { Description = "Install the skill globally to the user's home directory" };
        command.Add(globalOption);

        command.SetAction((parseResult, cancellationToken) =>
        {
            var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
            var isGlobal = parseResult.GetValue(globalOption);

            ISkillInstaller installer;
            string installPath;

            if (isGlobal)
            {
                var homePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                installer = new SkillInstaller(homePath);
                installPath = Path.Combine(homePath, ".claude", "skills", "spec", "SKILL.md");
            }
            else
            {
                installer = serviceProvider.GetRequiredService<ISkillInstaller>();
                installPath = ".claude/skills/spec/SKILL.md";
            }

            installer.Install();
            logger.LogInformation("Spec skill installed successfully to {Path}", installPath);

            return Task.FromResult(0);
        });

        return command;
    }
}
