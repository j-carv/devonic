using Spectre.Console;

namespace Devonic.CLI.Commands;

internal static class CompletionsCommand
{
    public static async Task<int> RunAsync(ServiceLocator services, string? shell)
    {
        if (shell is "--list-projects")
        {
            var projects = await services.ProjectRepository.GetAllAsync();
            foreach (var p in projects)
            {
                Console.WriteLine(p.Name);
                if (p.Alias is not null)
                    Console.WriteLine(p.Alias);
            }
            return 0;
        }

        var script = shell?.ToLowerInvariant() switch
        {
            "powershell" or "pwsh" => PowerShellScript(),
            "bash" => BashScript(),
            "zsh" => ZshScript(),
            _ => null
        };

        if (script is null)
        {
            AnsiConsole.MarkupLine("\n  [red]x[/] Usage: [green]dev completions <powershell|bash|zsh>[/]");
            AnsiConsole.MarkupLine("  [dim]Pipe the output to your shell profile to enable tab completion.[/]\n");
            return 1;
        }

        Console.Write(script);
        return 0;
    }

    private static string PowerShellScript() => """
        Register-ArgumentCompleter -Native -CommandName dev -ScriptBlock {
            param($wordToComplete, $commandAst, $cursorPosition)
            $commands = @('add','remove','rm','edit','list','ls','search','s','recent','favorites','fav','clone','config','scan','doctor','check','stats','cd','init','group','notes','completions','help')
            $tokens = $commandAst.ToString() -split '\s+'
            if ($tokens.Count -le 2) {
                $items = $commands + @(& dev completions --list-projects 2>$null)
                $items | Where-Object { $_ -like "$wordToComplete*" } | ForEach-Object {
                    [System.Management.Automation.CompletionResult]::new($_, $_, 'ParameterValue', $_)
                }
            } elseif ($tokens[1] -in @('remove','rm','edit','cd','notes') -and $tokens.Count -le 3) {
                & dev completions --list-projects 2>$null | Where-Object { $_ -like "$wordToComplete*" } | ForEach-Object {
                    [System.Management.Automation.CompletionResult]::new($_, $_, 'ParameterValue', $_)
                }
            }
        }

        """;

    private static string BashScript() => """
        _dev_completions() {
            local cur="${COMP_WORDS[COMP_CWORD]}"
            local commands="add remove rm edit list ls search s recent favorites fav clone config scan doctor check stats cd init group notes completions help"
            if [ "$COMP_CWORD" -eq 1 ]; then
                local projects
                projects=$(dev completions --list-projects 2>/dev/null)
                COMPREPLY=($(compgen -W "$commands $projects" -- "$cur"))
            elif [ "$COMP_CWORD" -eq 2 ]; then
                case "${COMP_WORDS[1]}" in
                    remove|rm|edit|cd|notes)
                        local projects
                        projects=$(dev completions --list-projects 2>/dev/null)
                        COMPREPLY=($(compgen -W "$projects" -- "$cur"))
                        ;;
                esac
            fi
        }
        complete -F _dev_completions dev

        """;

    private static string ZshScript() => """
        _dev() {
            local commands=(add remove rm edit list ls search s recent favorites fav clone config scan doctor check stats cd init group notes completions help)
            if (( CURRENT == 2 )); then
                local projects=(${(f)"$(dev completions --list-projects 2>/dev/null)"})
                _describe 'command' commands -- projects
            elif (( CURRENT == 3 )); then
                case "${words[2]}" in
                    remove|rm|edit|cd|notes)
                        local projects=(${(f)"$(dev completions --list-projects 2>/dev/null)"})
                        _describe 'project' projects
                        ;;
                esac
            fi
        }
        compdef _dev dev

        """;
}
