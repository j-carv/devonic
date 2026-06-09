using Devonic.Core.Entities;
using Spectre.Console;

namespace Devonic.CLI.Commands;

internal static class InitCommand
{
    private static readonly Dictionary<string, string> Indicators = new()
    {
        ["*.sln"] = "dotnet",
        ["*.slnx"] = "dotnet",
        ["*.csproj"] = "dotnet",
        ["package.json"] = "node",
        ["Cargo.toml"] = "rust",
        ["go.mod"] = "go",
        ["pom.xml"] = "java",
        ["build.gradle"] = "java",
        ["pyproject.toml"] = "python",
        ["requirements.txt"] = "python",
        ["Gemfile"] = "ruby",
        ["composer.json"] = "php",
    };

    public static async Task<int> RunAsync(ServiceLocator services)
    {
        var path = Directory.GetCurrentDirectory();
        var name = Path.GetFileName(path).ToLowerInvariant();

        var existing = await services.ProjectRepository.GetByNameAsync(name);
        if (existing is not null)
        {
            AnsiConsole.MarkupLine($"\n  [yellow]!![/] [bold]{Markup.Escape(name)}[/] is already registered.\n");
            return 1;
        }

        var tag = DetectTag(path);

        var config = await services.ConfigRepository.GetAsync();
        var ide = config.DefaultIde ?? AnsiConsole.Prompt(
            new SelectionPrompt<Ide>()
                .Title($"  IDE for [green]{Markup.Escape(name)}[/]:")
                .AddChoices(Enum.GetValues<Ide>())
                .UseConverter(i => i.ToString()));

        var project = new Project
        {
            Name = name,
            Path = path,
            Ide = ide,
            Tags = tag is not null ? [tag] : []
        };

        var result = await services.AddProject.ExecuteAsync(project);

        if (result.IsSuccess)
        {
            var tagInfo = tag is not null ? $" [dim]#{tag}[/]" : "";
            AnsiConsole.MarkupLine($"\n  [green]+[/] [bold]{Markup.Escape(name)}[/] registered{tagInfo}. Open with [green]dev {Markup.Escape(name)}[/]\n");
            return 0;
        }

        AnsiConsole.MarkupLine($"\n  [red]x[/] {Markup.Escape(result.Error!)}\n");
        return 1;
    }

    private static string? DetectTag(string path)
    {
        foreach (var (pattern, tag) in Indicators)
        {
            if (Directory.GetFiles(path, pattern, SearchOption.TopDirectoryOnly).Length > 0)
                return tag;
        }
        return null;
    }
}
