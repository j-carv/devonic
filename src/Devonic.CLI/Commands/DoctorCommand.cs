using Spectre.Console;

namespace Devonic.CLI.Commands;

internal static class DoctorCommand
{
    public static async Task<int> RunAsync(ServiceLocator services)
    {
        var projects = await services.ProjectRepository.GetAllAsync();

        if (projects.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]  No projects registered.[/]");
            return 0;
        }

        AnsiConsole.MarkupLine("\n  [bold]Project health check[/]\n");

        var issues = 0;
        var table = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn("[bold]Project[/]")
            .AddColumn("[bold]Status[/]")
            .AddColumn("[bold]Issues[/]");

        foreach (var project in projects.OrderBy(p => p.Name))
        {
            var problems = new List<string>();

            if (!Directory.Exists(project.Path))
                problems.Add("Path does not exist");

            if (!HasGitRepo(project.Path))
                problems.Add("No git repository");

            if (problems.Count == 0)
            {
                table.AddRow(
                    Markup.Escape(project.Name),
                    "[green]OK[/]",
                    "[dim]-[/]");
            }
            else
            {
                issues += problems.Count;
                table.AddRow(
                    Markup.Escape(project.Name),
                    "[red]ISSUES[/]",
                    $"[yellow]{Markup.Escape(string.Join("; ", problems))}[/]");
            }
        }

        AnsiConsole.Write(table);

        if (issues == 0)
            AnsiConsole.MarkupLine("\n  [green]All projects are healthy.[/]");
        else
            AnsiConsole.MarkupLine($"\n  [yellow]{issues} issue(s) found across {projects.Count} project(s).[/]");

        return issues > 0 ? 1 : 0;
    }

    private static bool HasGitRepo(string path)
    {
        if (!Directory.Exists(path)) return false;
        return Directory.Exists(Path.Combine(path, ".git"));
    }
}
