using Devonic.Core.Entities;
using Spectre.Console;

namespace Devonic.CLI.Commands;

internal static class CloneCommand
{
    public static async Task<int> RunAsync(ServiceLocator services, string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            AnsiConsole.MarkupLine("[red]  Usage: dev clone <git-url>[/]");
            return 1;
        }

        var config = await services.ConfigRepository.GetAsync();

        string? targetPath = null;
        if (config.DefaultProjectsPath is null)
        {
            targetPath = AnsiConsole.Prompt(
                new TextPrompt<string>("  Clone to [green]directory[/]:")
                    .DefaultValue(Directory.GetCurrentDirectory()));
        }

        var ide = config.DefaultIde ?? AnsiConsole.Prompt(
            new SelectionPrompt<Ide>()
                .Title("  Select [green]IDE[/] for this project:")
                .AddChoices(Enum.GetValues<Ide>())
                .UseConverter(i => i.ToString()));

        var result = await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .StartAsync("  Cloning repository...", async _ =>
                await services.CloneProject.ExecuteAsync(url, ide, targetPath));

        if (result.IsSuccess)
        {
            AnsiConsole.MarkupLine($"[green]  -> Repository cloned and registered.[/]");
            return 0;
        }

        AnsiConsole.MarkupLine($"[red]  Error: {Markup.Escape(result.Error!)}[/]");
        return 1;
    }
}
