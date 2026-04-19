using System.Text;
using AgentSquad.Core.Prompts;

namespace AgentSquad.Agents.AI;

/// <summary>
/// Shared single-pass implementation prompt builder. Used by both
/// <see cref="SoftwareEngineerAgent"/>'s legacy single-pass code-gen path and the
/// strategy framework's <see cref="BaselineCodeGenerator"/> so the two paths stay
/// in lock-step. If you change the prompt structure here, both paths pick it up.
///
/// When an <see cref="IPromptTemplateService"/> is supplied AND the template files
/// are present on disk, the rendered template wins. Otherwise we fall back to
/// inline strings that mirror the original SE single-pass wording.
/// </summary>
public static class SinglePassPromptBuilder
{
    /// <summary>System prompt anchoring the SE role and runnable/dependency rules.</summary>
    public static async Task<string> BuildSystemPromptAsync(
        string techStack,
        IPromptTemplateService? promptService,
        CancellationToken ct = default)
    {
        if (promptService is not null)
        {
            var rendered = await promptService.RenderAsync(
                "software-engineer/implementation-system",
                new Dictionary<string, string> { ["tech_stack"] = techStack },
                ct);
            if (!string.IsNullOrWhiteSpace(rendered)) return rendered;
        }

        return $"You are a Software Engineer implementing a high-complexity engineering task. " +
            $"The project uses {techStack} as its technology stack. " +
            "The PM Specification defines the business requirements, and the Architecture " +
            "document defines the technical design. The GitHub Issue contains the User Story " +
            "and acceptance criteria for this specific task. " +
            "Produce detailed, production-quality code. " +
            "Ensure the implementation fulfills the business goals from the PM spec. " +
            "Be thorough — this is the most critical part of the system.\n\n" +
            "RUNNABLE RULE: The application MUST compile and be runnable after your changes. " +
            "Do not leave stub methods that throw NotImplementedException, do not reference types " +
            "or services that don't exist yet, and do not break the build. If a feature depends on " +
            "code from another task that hasn't been implemented yet, use graceful fallbacks " +
            "(e.g., return empty collections, show placeholder text) instead of throwing exceptions. " +
            "After your implementation, `dotnet build` must succeed and `dotnet run` must start without errors.\n\n" +
            "DEPENDENCY RULE: Before using ANY external library, package, or framework, check the project's " +
            "dependency manifest (e.g., .csproj, package.json, requirements.txt, etc.). " +
            "If a dependency is not already listed, add it to the manifest and include that file in your output. " +
            "Never import/using/require a package without ensuring it is declared in the project.";
    }

    /// <summary>
    /// User prompt with PM spec, architecture, issue/design context, and the FILE: marker contract.
    /// Mirrors the inline fallback used in <c>SoftwareEngineerAgent.WorkOnOwnTasksAsync</c> single-pass.
    /// </summary>
    public static async Task<string> BuildUserPromptAsync(
        SinglePassPromptInputs inputs,
        IPromptTemplateService? promptService,
        CancellationToken ct = default)
    {
        if (promptService is not null)
        {
            var rendered = await promptService.RenderAsync(
                "software-engineer/single-pass-implementation",
                new Dictionary<string, string>
                {
                    ["pm_spec"] = inputs.PmSpec ?? "",
                    ["architecture"] = inputs.Architecture ?? "",
                    ["issue_context"] = inputs.IssueContext ?? "",
                    ["design_context"] = inputs.DesignContext ?? "",
                    ["task_name"] = inputs.TaskName,
                    ["task_description"] = inputs.TaskDescription ?? "",
                    ["tech_stack"] = inputs.TechStack ?? "",
                },
                ct);
            if (!string.IsNullOrWhiteSpace(rendered)) return rendered;
        }

        var sb = new StringBuilder();
        sb.Append("## PM Specification\n").Append(inputs.PmSpec ?? "").Append("\n\n");
        sb.Append("## Architecture\n").Append(inputs.Architecture ?? "");
        if (!string.IsNullOrWhiteSpace(inputs.IssueContext))
            sb.Append(inputs.IssueContext);
        if (!string.IsNullOrWhiteSpace(inputs.DesignContext))
            sb.Append("\n\n").Append(inputs.DesignContext);
        sb.Append("\n\n## Task: ").Append(inputs.TaskName).Append('\n').Append(inputs.TaskDescription ?? "").Append("\n\n");
        sb.Append("Implement ONLY the files needed for this specific task. ");
        sb.Append("Output each file using this exact format:\n\n");
        sb.Append("FILE: path/to/file.ext\n```language\n<file content>\n```\n\n");
        sb.Append($"Use the {inputs.TechStack ?? ""} technology stack. ");
        sb.Append("SCOPE RULE: Only output files that are NEW or MINIMALLY MODIFIED for this task. ");
        sb.Append("Do NOT regenerate .sln, .csproj, Program.cs, or other infrastructure files unless ");
        sb.Append("this task explicitly requires changes to them. ");
        sb.Append("If the task has a FilePlan (CREATE:/MODIFY:/USE:), follow it strictly. ");
        sb.Append("Every file MUST use the FILE: marker format so it can be parsed and committed.");
        return sb.ToString();
    }
}

/// <summary>Inputs that vary per task for <see cref="SinglePassPromptBuilder.BuildUserPromptAsync"/>.</summary>
public record SinglePassPromptInputs
{
    public required string TaskName { get; init; }
    public string? TaskDescription { get; init; }
    public string? TechStack { get; init; }
    public string? PmSpec { get; init; }
    public string? Architecture { get; init; }
    public string? IssueContext { get; init; }
    public string? DesignContext { get; init; }
}
