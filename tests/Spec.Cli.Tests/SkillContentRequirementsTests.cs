namespace Spec.Cli.Tests;

public class SkillContentRequirementsTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _skillContent;

    public SkillContentRequirementsTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDir);

        var installer = new SkillInstaller(_tempDir);
        installer.Install();

        var skillFilePath = Path.Combine(_tempDir, ".claude", "skills", "spec", "SKILL.md");
        _skillContent = File.ReadAllText(skillFilePath);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    // --- L2-3.1: Docs/Specs Folder Creation instruction ---

    [Fact]
    public void SkillContent_ContainsInstructionToCreateDocsSpecsFolder()
    {
        Assert.Contains("docs/specs", _skillContent);
    }

    [Fact]
    public void SkillContent_DocsSpecsCreation_DoesNotErrorIfAlreadyExists()
    {
        // The instruction says "if it does not already exist", which prevents
        // duplicate creation or errors when the folder is already present
        Assert.Contains("if it does not already exist", _skillContent);
    }

    // --- L2-3.2: L1 Requirements Document instruction ---

    [Fact]
    public void SkillContent_ContainsInstructionToProduceL1Document()
    {
        Assert.Contains("docs/specs/L1.md", _skillContent);
    }

    [Fact]
    public void SkillContent_L1Requirements_AreExtremeHighLevelDescriptions()
    {
        Assert.Contains("extreme high-level descriptions", _skillContent);
    }

    [Fact]
    public void SkillContent_L1Requirements_HaveUniqueIdentifiers()
    {
        // Verify the skill instructs that each L1 requirement has a unique identifier
        Assert.Contains("unique identifier", _skillContent);
        // Verify example format is provided
        Assert.Contains("L1-1", _skillContent);
        Assert.Contains("L1-2", _skillContent);
    }

    // --- L2-3.3: L2 Requirements Document instruction ---

    [Fact]
    public void SkillContent_ContainsInstructionToProduceL2Document()
    {
        Assert.Contains("docs/specs/L2.md", _skillContent);
    }

    [Fact]
    public void SkillContent_L2Requirements_TraceToExactlyOneL1Requirement()
    {
        Assert.Contains("traces to exactly one L1 requirement via its identifier", _skillContent);
    }

    [Fact]
    public void SkillContent_L2Requirements_IncludeAcceptanceCriteria()
    {
        Assert.Contains("acceptance criteria defining done", _skillContent);
    }
}
