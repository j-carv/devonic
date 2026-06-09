using System.Diagnostics;
using System.Runtime.InteropServices;
using Devonic.Core.Entities;
using Devonic.Core.Interfaces;
using Devonic.Core.Results;

namespace Devonic.Infrastructure.IdeIntegration;

public sealed class IdeOpener(IConfigRepository configRepository) : IIdeOpener
{
    private static readonly Dictionary<Ide, string[]> WindowsCommands = new()
    {
        [Ide.VsCode] = ["code.cmd", "code"],
        [Ide.Rider] = ["rider64.exe", "rider"],
        [Ide.IntelliJ] = ["idea64.exe", "idea"],
        [Ide.WebStorm] = ["webstorm64.exe", "webstorm"],
        [Ide.VisualStudio] = ["devenv.exe"],
        [Ide.Cursor] = ["cursor.cmd", "cursor"],
        [Ide.Zed] = ["zed.exe", "zed"],
        [Ide.Fleet] = ["fleet.exe", "fleet"],
        [Ide.Neovim] = ["nvim"]
    };

    private static readonly Dictionary<Ide, string[]> LinuxCommands = new()
    {
        [Ide.VsCode] = ["code"],
        [Ide.Rider] = ["rider"],
        [Ide.IntelliJ] = ["idea"],
        [Ide.WebStorm] = ["webstorm"],
        [Ide.VisualStudio] = [],
        [Ide.Cursor] = ["cursor"],
        [Ide.Zed] = ["zed"],
        [Ide.Fleet] = ["fleet"],
        [Ide.Neovim] = ["nvim"]
    };

    private static readonly Dictionary<Ide, string[]> MacCommands = new()
    {
        [Ide.VsCode] = ["code", "open -a Visual\\ Studio\\ Code"],
        [Ide.Rider] = ["rider", "open -a Rider"],
        [Ide.IntelliJ] = ["idea", "open -a IntelliJ\\ IDEA"],
        [Ide.WebStorm] = ["webstorm", "open -a WebStorm"],
        [Ide.VisualStudio] = ["open -a Visual\\ Studio"],
        [Ide.Cursor] = ["cursor", "open -a Cursor"],
        [Ide.Zed] = ["zed", "open -a Zed"],
        [Ide.Fleet] = ["fleet", "open -a Fleet"],
        [Ide.Neovim] = ["nvim"]
    };

    public async Task<Result> OpenAsync(Project project)
    {
        var config = await configRepository.GetAsync();

        if (config.IdePaths.TryGetValue(project.Ide, out var customPath))
        {
            var result = TryOpen(customPath, project.Path);
            if (result.IsSuccess) return result;
        }

        var commands = GetPlatformCommands(project.Ide);
        foreach (var command in commands)
        {
            var result = TryOpen(command, project.Path);
            if (result.IsSuccess) return result;
        }

        return Result.Failure(
            $"Could not open {project.Ide}. Make sure it is installed and available in PATH, " +
            "or configure its path with 'dev config set idePath.<ide> <path>'.");
    }

    private static string[] GetPlatformCommands(Ide ide)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return WindowsCommands.GetValueOrDefault(ide, []);
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return MacCommands.GetValueOrDefault(ide, []);
        return LinuxCommands.GetValueOrDefault(ide, []);
    }

    private static Result TryOpen(string command, string projectPath)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = command,
                Arguments = $"\"{projectPath}\"",
                UseShellExecute = true
            });
            return Result.Success();
        }
        catch
        {
            return Result.Failure($"Failed to launch '{command}'.");
        }
    }
}
