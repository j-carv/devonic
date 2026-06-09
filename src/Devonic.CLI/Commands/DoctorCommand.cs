using Spectre.Console;

namespace Devonic.CLI.Commands;

internal static class DoctorCommand
{
    public static async Task<int> RunAsync(ServiceLocator services)
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
        var issues = 0;

        foreach (var project in projects.OrderBy(p => p.Name))
        {
            var problems = new List<string>();

            if (!Directory.Exists(project.Path))
                problems.Add("path missing");
            else if (!Directory.Exists(Path.Combine(project.Path, ".git")))
                problems.Add("no git repo");

            if (problems.Count == 0)
            {
                healthy++;
                AnsiConsole.MarkupLine($"  [green]OK[/]  {Markup.Escape(project.Name)}");
            }
            else
            {
                issues++;
                var detail = string.Join(", ", problems);
                AnsiConsole.MarkupLine($"  [red]!![/]  {Markup.Escape(project.Name)} [dim]— {detail}[/]");
            }
        }

        AnsiConsole.WriteLine();
        if (issues == 0)
            AnsiConsole.MarkupLine($"  [green]All {healthy} project(s) healthy.[/]\n");
        else
            AnsiConsole.MarkupLine($"  [green]{healthy} healthy[/], [red]{issues} with issues[/]\n");

        return issues > 0 ? 1 : 0;
    }
}
