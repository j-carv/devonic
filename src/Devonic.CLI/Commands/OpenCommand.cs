using Spectre.Console;

namespace Devonic.CLI.Commands;

internal static class OpenCommand
{
    public static async Task<int> RunAsync(ServiceLocator services, string projectName, bool run)
    {
        var result = await services.OpenProject.ExecuteAsync(projectName, run);

        if (result.IsSuccess)
        {
            AnsiConsole.MarkupLine($"[green]  -> Opening '{Markup.Escape(projectName)}'...[/]");
            if (run)
                AnsiConsole.MarkupLine("[dim]  -> Running configured command...[/]");
            return 0;
        }

        AnsiConsole.MarkupLine($"[red]  Error: {Markup.Escape(result.Error!)}[/]");
        return 1;
    }
}
