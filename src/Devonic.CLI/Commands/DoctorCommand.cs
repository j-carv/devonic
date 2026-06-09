using System.Diagnostics;
using Spectre.Console;

namespace Devonic.CLI.Commands;

internal static class DoctorCommand
{
    public static async Task<int> RunAsync(ServiceLocator services, bool fix = false)
    {
        var projects = await services.ProjectRepository.GetAllAsync();

        if (projects.Count == 0)
        {
            AnsiConsole.MarkupLine("\n  [dim]No projects to check.[/]\n");
            return 0;
        }

        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule("[bold]Health check[/]").LeftJustified());
        AnsiConsole.WriteLine();

        var healthy = 0;
        var warnings = 0;
        var broken = new List<string>();

        foreach (var project in projects.OrderBy(p => p.Name))
        {
            var problems = new List<string>();
            var hints = new List<string>();

            if (!Directory.Exists(project.Path))
            {
                problems.Add("path missing");
            }
            else
            {
                var gitDir = Path.Combine(project.Path, ".git");
                if (!Directory.Exists(gitDir) && !File.Exists(gitDir))
                {
                    problems.Add("not a git repo");
                }
                else
                {
                    var branch = await RunGitAsync(project.Path, "rev-parse --abbrev-ref HEAD");
                    if (branch is not null)
                        hints.Add(branch);

                    var status = await RunGitAsync(project.Path, "status --porcelain");
                    if (status is not null && status.Length > 0)
                        hints.Add("dirty");

                    var behind = await RunGitAsync(project.Path, "rev-list --count HEAD..@{u}");
                    if (behind is not null && int.TryParse(behind, out var n) && n > 0)
                        hints.Add($"{n} behind");
                }
            }

            if (problems.Count > 0)
            {
                broken.Add(project.Name);
                AnsiConsole.MarkupLine($"  [red]x[/]  [bold]{Markup.Escape(project.Name)}[/]  [red]{string.Join(", ", problems)}[/]");
            }
            else if (hints.Contains("dirty"))
            {
                warnings++;
                AnsiConsole.MarkupLine($"  [yellow]!![/] [bold]{Markup.Escape(project.Name)}[/]  [dim]{string.Join(" · ", hints)}[/]");
            }
            else
            {
                healthy++;
                var detail = hints.Count > 0 ? $"  [dim]{string.Join(" · ", hints)}[/]" : "";
                AnsiConsole.MarkupLine($"  [green]OK[/]  [bold]{Markup.Escape(project.Name)}[/]{detail}");
            }
        }

        AnsiConsole.WriteLine();
        var parts = new List<string>();
        if (healthy > 0) parts.Add($"[green]{healthy} clean[/]");
        if (warnings > 0) parts.Add($"[yellow]{warnings} dirty[/]");
        if (broken.Count > 0) parts.Add($"[red]{broken.Count} broken[/]");
        AnsiConsole.MarkupLine($"  {string.Join("[dim] · [/]", parts)}\n");

        if (fix && broken.Count > 0)
        {
            var toRemove = AnsiConsole.Prompt(
                new MultiSelectionPrompt<string>()
                    .Title("  Select broken projects to [red]remove[/]:")
                    .InstructionsText("[dim](Space to toggle, Enter to confirm)[/]")
                    .AddChoices(broken));

            foreach (var name in toRemove)
            {
                await services.ProjectRepository.RemoveAsync(name);
                AnsiConsole.MarkupLine($"  [red]-[/] {Markup.Escape(name)}");
            }

            if (toRemove.Count > 0)
                AnsiConsole.MarkupLine($"\n  Removed {toRemove.Count} {(toRemove.Count == 1 ? "project" : "projects")}.\n");
        }
        else if (!fix && broken.Count > 0)
        {
            AnsiConsole.MarkupLine("  [dim]Run[/] [green]dev doctor --fix[/] [dim]to remove broken projects.[/]\n");
        }

        return broken.Count > 0 ? 1 : 0;
    }

    private static async Task<string?> RunGitAsync(string workingDir, string arguments)
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = arguments,
                    WorkingDirectory = workingDir,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            var output = await process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();
            return process.ExitCode == 0 ? output.Trim() : null;
        }
        catch
        {
            return null;
        }
    }
}
