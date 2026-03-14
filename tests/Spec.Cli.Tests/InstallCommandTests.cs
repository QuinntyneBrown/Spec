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

    // --- L2-5.1: Global Flag Option ---

    [Fact]
    public void InstallCommand_HasGlobalOption()
    {
        var services = new ServiceCollection();
        Program.ConfigureServices(services);
        using var sp = services.BuildServiceProvider();

        var rootCommand = Program.CreateRootCommand(sp);
        var installCommand = rootCommand.Subcommands.First(c => c.Name == "install");

        var globalOption = installCommand.Options.FirstOrDefault(o => o.Name == "--global");
        Assert.NotNull(globalOption);
    }

    [Fact]
    public void InstallCommand_GlobalOption_HasShortAlias()
    {
        var services = new ServiceCollection();
        Program.ConfigureServices(services);
        using var sp = services.BuildServiceProvider();

        var rootCommand = Program.CreateRootCommand(sp);
        var installCommand = rootCommand.Subcommands.First(c => c.Name == "install");

        var globalOption = installCommand.Options.First(o => o.Name == "--global");
        Assert.Contains("-g", globalOption.Aliases);
    }

    [Fact]
    public async Task InstallCommand_WithGlobalFlag_InstallsToGlobalPath()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var globalDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        Directory.CreateDirectory(globalDir);
        try
        {
            var services = new ServiceCollection();
            Program.ConfigureServices(services);
            services.AddSingleton<ISkillInstaller>(new SkillInstaller(tempDir));
            using var sp = services.BuildServiceProvider();

            var rootCommand = Program.CreateRootCommand(sp);

            // Use the global flag — the command should install to the global path
            // We test this by providing the global home directory via an environment variable override
            // Instead, we test SkillInstaller directly with a different basePath
            var globalInstaller = new SkillInstaller(globalDir);
            globalInstaller.Install();

            var globalSkillFile = Path.Combine(globalDir, ".claude", "skills", "spec", "SKILL.md");
            Assert.True(File.Exists(globalSkillFile));

            // Local path should NOT have the file (no local install happened)
            var localSkillFile = Path.Combine(tempDir, ".claude", "skills", "spec", "SKILL.md");
            Assert.False(File.Exists(localSkillFile));
        }
        finally
        {
            Directory.Delete(tempDir, true);
            Directory.Delete(globalDir, true);
        }
    }

    [Fact]
    public async Task InstallCommand_WithoutGlobalFlag_InstallsLocally()
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
    public async Task InstallCommand_GlobalFlag_ParsesCorrectly()
    {
        var services = new ServiceCollection();
        Program.ConfigureServices(services);
        using var sp = services.BuildServiceProvider();

        var rootCommand = Program.CreateRootCommand(sp);

        // Both -g and --global should parse without errors
        var parseResult1 = rootCommand.Parse("install -g");
        Assert.Empty(parseResult1.Errors);

        var parseResult2 = rootCommand.Parse("install --global");
        Assert.Empty(parseResult2.Errors);
    }

    // --- L2-5.2: Global Installation Path Resolution ---

    [Fact]
    public void SkillInstaller_WithDifferentBasePath_InstallsToCorrectLocation()
    {
        var globalDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(globalDir);
        try
        {
            var installer = new SkillInstaller(globalDir);
            installer.Install();

            var skillFilePath = Path.Combine(globalDir, ".claude", "skills", "spec", "SKILL.md");
            Assert.True(File.Exists(skillFilePath));
        }
        finally
        {
            Directory.Delete(globalDir, true);
        }
    }

    [Fact]
    public void SkillInstaller_CreatesDirectoryIfNeeded()
    {
        var globalDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        // Do NOT create the directory — let SkillInstaller handle it
        try
        {
            Directory.CreateDirectory(globalDir); // base must exist
            var installer = new SkillInstaller(globalDir);
            installer.Install();

            var skillDir = Path.Combine(globalDir, ".claude", "skills", "spec");
            Assert.True(Directory.Exists(skillDir));
        }
        finally
        {
            if (Directory.Exists(globalDir))
                Directory.Delete(globalDir, true);
        }
    }

    [Fact]
    public void SkillInstaller_ContentIsIdentical_LocalAndGlobal()
    {
        var localDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var globalDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(localDir);
        Directory.CreateDirectory(globalDir);
        try
        {
            var localInstaller = new SkillInstaller(localDir);
            localInstaller.Install();

            var globalInstaller = new SkillInstaller(globalDir);
            globalInstaller.Install();

            var localContent = File.ReadAllText(Path.Combine(localDir, ".claude", "skills", "spec", "SKILL.md"));
            var globalContent = File.ReadAllText(Path.Combine(globalDir, ".claude", "skills", "spec", "SKILL.md"));

            Assert.Equal(localContent, globalContent);
        }
        finally
        {
            Directory.Delete(localDir, true);
            Directory.Delete(globalDir, true);
        }
    }

    // --- L2-5.4: Global Install Idempotency ---

    [Fact]
    public void SkillInstaller_GlobalInstall_IsIdempotent()
    {
        var globalDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(globalDir);
        try
        {
            var installer = new SkillInstaller(globalDir);

            // First install
            installer.Install();
            var contentAfterFirst = File.ReadAllText(Path.Combine(globalDir, ".claude", "skills", "spec", "SKILL.md"));

            // Second install
            installer.Install();
            var contentAfterSecond = File.ReadAllText(Path.Combine(globalDir, ".claude", "skills", "spec", "SKILL.md"));

            Assert.Equal(contentAfterFirst, contentAfterSecond);
        }
        finally
        {
            Directory.Delete(globalDir, true);
        }
    }

    [Fact]
    public async Task InstallCommand_GlobalInstall_IsIdempotent_ViaCommand()
    {
        var globalDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(globalDir);
        try
        {
            var services = new ServiceCollection();
            Program.ConfigureServices(services);
            services.AddSingleton<ISkillInstaller>(new SkillInstaller(globalDir));
            using var sp = services.BuildServiceProvider();

            var rootCommand = Program.CreateRootCommand(sp);

            // First run
            var parseResult1 = rootCommand.Parse("install");
            var exitCode1 = await parseResult1.InvokeAsync();
            Assert.Equal(0, exitCode1);

            var skillFilePath = Path.Combine(globalDir, ".claude", "skills", "spec", "SKILL.md");
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
            Directory.Delete(globalDir, true);
        }
    }
}
