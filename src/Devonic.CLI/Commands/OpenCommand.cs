using Devonic.Core.Entities;
using Spectre.Console;

namespace Devonic.CLI.Commands;

internal static class OpenCommand
{
    public static async Task<int> RunAsync(ServiceLocator services, string projectName, bool run, Ide? ideOverride = null, bool shell = false)
    {
        var result = await services.OpenProject.ExecuteAsync(projectName, run, ideOverride, shell);

        if (result.IsSuccess)
        {
            if (shell)
                AnsiConsole.MarkupLine($"[green]  -> Opening shell in '{Markup.Escape(projectName)}'...[/]");
            else
                AnsiConsole.MarkupLine($"[green]  -> Opening '{Markup.Escape(projectName)}'...[/]");

            if (run)
                AnsiConsole.MarkupLine("[dim]  -> Running configured command...[/]");
            return 0;
        }

        AnsiConsole.MarkupLine($"[red]  Error: {Markup.Escape(result.Error!)}[/]");
        return 1;
    }
}
