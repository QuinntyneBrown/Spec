namespace Spec.Cli.Tests;

public class SkillContentTeamProcessTests
{
    private static string GetSkillContent()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        try
        {
            var installer = new SkillInstaller(tempDir);
            installer.Install();

            var skillFilePath = Path.Combine(tempDir, ".claude", "skills", "spec.md");
            return File.ReadAllText(skillFilePath);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    // --- L2-4.1: Per-L1 Team Creation ---

    [Fact]
    public void SkillContent_DescribesCreatingTeamPerL1Requirement()
    {
        var content = GetSkillContent();

        Assert.Contains("For each L1 requirement", content);
        Assert.Contains("team", content);
    }

    [Fact]
    public void SkillContent_TeamIncludesImplementorAgent()
    {
        var content = GetSkillContent();

        Assert.Contains("Implementor Agent", content);
    }

    [Fact]
    public void SkillContent_TeamIncludesQualityAssuranceAgent()
    {
        var content = GetSkillContent();

        Assert.Contains("Quality Assurance Agent", content);
    }

    // --- L2-4.2: Implementor Agent Behavior ---

    [Fact]
    public void SkillContent_ImplementorIteratesThroughEachL2()
    {
        var content = GetSkillContent();

        Assert.Contains("each L2 requirement", content);
        Assert.Contains("Implementor", content);
    }

    [Fact]
    public void SkillContent_ImplementorCreatesFailingAcceptanceTestBeforeCode()
    {
        var content = GetSkillContent();

        // The content must instruct creating a failing acceptance test before writing production code
        Assert.Contains("failing acceptance test", content);
    }

    [Fact]
    public void SkillContent_ImplementorWritesMinimalCodeToPassTest()
    {
        var content = GetSkillContent();

        Assert.Contains("minimal code", content);
        Assert.Contains("acceptance test pass", content);
    }

    [Fact]
    public void SkillContent_ImplementorMarksL2AsCompleteWhenTestsPass()
    {
        var content = GetSkillContent();

        Assert.Contains("mark", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("complete", content, StringComparison.OrdinalIgnoreCase);
    }

    // --- L2-4.3: QA Agent Behavior ---

    [Fact]
    public void SkillContent_QAReviewsCompletedL2Requirements()
    {
        var content = GetSkillContent();

        Assert.Contains("completed L2 requirement", content);
        Assert.Contains("marked as complete", content);
    }

    [Fact]
    public void SkillContent_QAVerifiesNoTodosOrPlaceholderCode()
    {
        var content = GetSkillContent();

        Assert.Contains("no TODOs", content);
        Assert.Contains("placeholder code", content);
    }

    [Fact]
    public void SkillContent_QAVerifiesAllAcceptanceTestsPass()
    {
        var content = GetSkillContent();

        // QA section should mention verifying acceptance tests pass
        Assert.Contains("acceptance tests pass", content);
    }

    [Fact]
    public void SkillContent_QAMarksAsVerifiedOrRejectsWithFeedback()
    {
        var content = GetSkillContent();

        Assert.Contains("verified", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("reject", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("feedback", content, StringComparison.OrdinalIgnoreCase);
    }

    // --- L2-4.4: Iteration Until Verified ---

    [Fact]
    public void SkillContent_SpecifiesIterationUntilAllL2sVerified()
    {
        var content = GetSkillContent();

        Assert.Contains("Iterate", content);
        Assert.Contains("verified", content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void SkillContent_RejectedL2sReworkedByImplementor()
    {
        var content = GetSkillContent();

        // The content should indicate that rejected items go back to the Implementor for rework
        Assert.Contains("reject", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Implementor", content);
        Assert.Contains("fix", content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void SkillContent_ProcessCompletesOnlyWhenAllL2sVerified()
    {
        var content = GetSkillContent();

        // "Iterate until all L2 requirements under the L1 are verified"
        Assert.Contains("until all L2 requirements", content);
        Assert.Contains("verified", content, StringComparison.OrdinalIgnoreCase);
    }
}
