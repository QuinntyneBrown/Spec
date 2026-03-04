using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Spec.Cli.Tests;

public class InstallCommandTests
{
    // --- L2-2.1: Install Command Registration ---

    [Fact]
    public void CreateRootCommand_ContainsInstallSubcommand()
    {
        var services = new ServiceCollection();
        Program.ConfigureServices(services);
        using var sp = services.BuildServiceProvider();

        var rootCommand = Program.CreateRootCommand(sp);

        var installCommand = rootCommand.Subcommands.FirstOrDefault(c => c.Name == "install");
        Assert.NotNull(installCommand);
    }

    [Fact]
    public void InstallCommand_HasDescription()
    {
        var services = new ServiceCollection();
        Program.ConfigureServices(services);
        using var sp = services.BuildServiceProvider();

        var rootCommand = Program.CreateRootCommand(sp);

        var installCommand = rootCommand.Subcommands.First(c => c.Name == "install");
        Assert.False(string.IsNullOrWhiteSpace(installCommand.Description));
    }

    [Fact]
    public async Task HelpOutput_ListsInstallCommand()
    {
        var services = new ServiceCollection();
        Program.ConfigureServices(services);
        using var sp = services.BuildServiceProvider();

        var rootCommand = Program.CreateRootCommand(sp);
        var writer = new StringWriter();
        Console.SetOut(writer);

        var parseResult = rootCommand.Parse("--help");
        await parseResult.InvokeAsync();

        var output = writer.ToString();
        Assert.Contains("install", output);
    }

    [Fact]
    public async Task InstallCommand_ExecutesSuccessfully()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        try
        {
            var services = new ServiceCollection();
            Program.ConfigureServices(services);
            services.AddSingleton<ISkillInstaller>(new SkillInstaller(tempDir));
            using var sp = services.BuildServiceProvider();

            var rootCommand = Program.CreateRootCommand(sp);
            var parseResult = rootCommand.Parse("install");
            var exitCode = await parseResult.InvokeAsync();

            Assert.Equal(0, exitCode);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    // --- L2-2.2: Claude Code Skill File Generation ---

    [Fact]
    public async Task InstallCommand_CreatesSkillFile()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        try
        {
            var services = new ServiceCollection();
            Program.ConfigureServices(services);
            services.AddSingleton<ISkillInstaller>(new SkillInstaller(tempDir));
            using var sp = services.BuildServiceProvider();

            var rootCommand = Program.CreateRootCommand(sp);
            var parseResult = rootCommand.Parse("install");
            await parseResult.InvokeAsync();

            var skillFilePath = Path.Combine(tempDir, ".claude", "skills", "spec", "SKILL.md");
            Assert.True(File.Exists(skillFilePath));
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task InstallCommand_CreatesDirectoryStructure()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        try
        {
            var services = new ServiceCollection();
            Program.ConfigureServices(services);
            services.AddSingleton<ISkillInstaller>(new SkillInstaller(tempDir));
            using var sp = services.BuildServiceProvider();

            var rootCommand = Program.CreateRootCommand(sp);
            var parseResult = rootCommand.Parse("install");
            await parseResult.InvokeAsync();

            var skillsDir = Path.Combine(tempDir, ".claude", "skills");
            Assert.True(Directory.Exists(skillsDir));
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task InstallCommand_OverwritesExistingFile()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        try
        {
            var skillDir = Path.Combine(tempDir, ".claude", "skills", "spec");
            Directory.CreateDirectory(skillDir);
            var skillFilePath = Path.Combine(skillDir, "SKILL.md");
            await File.WriteAllTextAsync(skillFilePath, "old content");

            var services = new ServiceCollection();
            Program.ConfigureServices(services);
            services.AddSingleton<ISkillInstaller>(new SkillInstaller(tempDir));
            using var sp = services.BuildServiceProvider();

            var rootCommand = Program.CreateRootCommand(sp);
            var parseResult = rootCommand.Parse("install");
            await parseResult.InvokeAsync();

            var content = await File.ReadAllTextAsync(skillFilePath);
            Assert.NotEqual("old content", content);
            Assert.Contains("# Spec Skill", content);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task InstallCommand_WritesCorrectSkillContent()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        try
        {
            var services = new ServiceCollection();
            Program.ConfigureServices(services);
            services.AddSingleton<ISkillInstaller>(new SkillInstaller(tempDir));
            using var sp = services.BuildServiceProvider();

            var rootCommand = Program.CreateRootCommand(sp);
            var parseResult = rootCommand.Parse("install");
            await parseResult.InvokeAsync();

            var skillFilePath = Path.Combine(tempDir, ".claude", "skills", "spec", "SKILL.md");
            var content = await File.ReadAllTextAsync(skillFilePath);

            Assert.Contains("# Spec Skill", content);
            Assert.Contains("## Step 1: Requirements Generation", content);
            Assert.Contains("## Step 2: Team-Based Implementation Process", content);
            Assert.Contains("### Implementor Agent", content);
            Assert.Contains("### Quality Assurance Agent", content);
            Assert.Contains("### Iteration", content);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    // --- L2-2.3: Idempotent Installation ---

    [Fact]
    public async Task InstallCommand_IsIdempotent()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        try
        {
            var services = new ServiceCollection();
            Program.ConfigureServices(services);
            services.AddSingleton<ISkillInstaller>(new SkillInstaller(tempDir));
            using var sp = services.BuildServiceProvider();

            var rootCommand = Program.CreateRootCommand(sp);

            // First run
            var parseResult1 = rootCommand.Parse("install");
            var exitCode1 = await parseResult1.InvokeAsync();
            Assert.Equal(0, exitCode1);

            var skillFilePath = Path.Combine(tempDir, ".claude", "skills", "spec", "SKILL.md");
            var contentAfterFirst = await File.ReadAllTextAsync(skillFilePath);

            // Second run
            var parseResult2 = rootCommand.Parse("install");
            var exitCode2 = await parseResult2.InvokeAsync();
            Assert.Equal(0, exitCode2);

            var contentAfterSecond = await File.ReadAllTextAsync(skillFilePath);

            Assert.Equal(contentAfterFirst, contentAfterSecond);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    // --- SkillInstaller unit tests ---

    [Fact]
    public void SkillInstaller_Install_CreatesFile()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        try
        {
            var installer = new SkillInstaller(tempDir);
            installer.Install();

            var skillFilePath = Path.Combine(tempDir, ".claude", "skills", "spec", "SKILL.md");
            Assert.True(File.Exists(skillFilePath));
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void SkillInstaller_Install_ContentMatchesExpected()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        try
        {
            var installer = new SkillInstaller(tempDir);
            installer.Install();

            var skillFilePath = Path.Combine(tempDir, ".claude", "skills", "spec", "SKILL.md");
            var content = File.ReadAllText(skillFilePath);

            // Verify key structural elements
            Assert.StartsWith("---", content);
            Assert.Contains("# Spec Skill", content);
            Assert.Contains("docs/specs", content);
            Assert.Contains("L1.md", content);
            Assert.Contains("L2.md", content);
            Assert.Contains("acceptance criteria", content);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }
}
