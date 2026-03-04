namespace Spec.Cli;

public class SkillInstaller : ISkillInstaller
{
    private readonly string _basePath;

    public SkillInstaller(string basePath)
    {
        _basePath = basePath;
    }

    public void Install()
    {
        var skillDir = Path.Combine(_basePath, ".claude", "skills", "spec");
        Directory.CreateDirectory(skillDir);

        var skillFilePath = Path.Combine(skillDir, "SKILL.md");
        File.WriteAllText(skillFilePath, SkillContent);
    }

    private const string SkillContent = """
        ---
        name: spec
        description: When asked to do any kind of code generation, follow the Spec process to generate requirements and implement them with a team of agents.
        ---

        # Spec Skill

        When asked to do any kind of code generation, follow this process:

        ## Step 1: Requirements Generation

        1. Create `docs/specs` folder in the project root if it does not already exist
        2. Create `docs/specs/L1.md` containing high-level requirements for the ask
           - L1 requirements are extreme high-level descriptions of what the system shall do
           - Each L1 requirement has a unique identifier (e.g., `L1-1`, `L1-2`)
        3. Create `docs/specs/L2.md` containing detailed requirements that trace to L1 requirements
           - Each L2 requirement traces to exactly one L1 requirement via its identifier
           - Each L2 requirement includes acceptance criteria defining done

        ## Step 2: Team-Based Implementation Process

        For each L1 requirement, create a team of agents:

        ### Implementor Agent

        Implement the L1 requirement:

        - Systematically implement each L2 requirement for the assigned L1 and then mark as complete
        - Create a failing acceptance test for each acceptance criteria for the L2 requirement
        - Implement the minimal code to make the acceptance test pass

        ### Quality Assurance Agent

        For each completed L2 requirement marked as complete:

        - Verify the implementation is complete — no TODOs or placeholder code
        - Verify all acceptance tests pass
        - Mark the L2 as verified, or reject it with feedback so the Implementor can fix

        ### Iteration

        Iterate until all L2 requirements under the L1 are verified by the QA agent.
        """;
}
