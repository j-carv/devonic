using Devonic.Core.Entities;
using Spectre.Console;

namespace Devonic.CLI.Commands;

internal static class OpenCommand
{
    public static async Task<int> RunAsync(ServiceLocator services, string projectName, bool run, Ide? ideOverride = null, bool shell = false)
    {
        var result = await services.OpenProject.ExecuteAsync(projectName, run, ideOverride, shell);

        if (!result.IsSuccess)
        {
            AnsiConsole.MarkupLine($"\n  [red]x[/] {Markup.Escape(result.Error!)}");
            AnsiConsole.MarkupLine("[dim]    Run 'dev list' to see registered projects or 'dev add' to register one.[/]\n");
            return 1;
        }

        var ide = ideOverride?.ToString() ?? "";
        if (shell)
            AnsiConsole.MarkupLine($"\n  [green]>[/] Shell opened in [bold]{Markup.Escape(projectName)}[/]\n");
        else if (run)
            AnsiConsole.MarkupLine($"\n  [green]>[/] Launched [bold]{Markup.Escape(projectName)}[/] + running command\n");
        else
            AnsiConsole.MarkupLine($"\n  [green]>[/] Launched [bold]{Markup.Escape(projectName)}[/]{(ideOverride.HasValue ? $" in {ide}" : "")}\n");

        return 0;
    }
}
