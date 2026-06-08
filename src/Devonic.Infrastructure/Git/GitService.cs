using System.Diagnostics;
using Devonic.Core.Interfaces;
using Devonic.Core.Results;

namespace Devonic.Infrastructure.Git;

public sealed class GitService : IGitService
{
    public async Task<Result<string>> CloneAsync(string url, string targetPath)
    {
        var repoName = ExtractRepoName(url);
        var fullPath = Path.Combine(targetPath, repoName);

        if (Directory.Exists(fullPath))
            return Result<string>.Failure($"Directory '{fullPath}' already exists.");

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = $"clone \"{url}\" \"{fullPath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            }
        };

        try
        {
            process.Start();
            var stderr = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            return process.ExitCode == 0
                ? Result<string>.Success(fullPath)
                : Result<string>.Failure($"Git clone failed: {stderr.Trim()}");
        }
        catch
        {
            return Result<string>.Failure("Git is not installed or not available in PATH.");
        }
    }

    private static string ExtractRepoName(string url)
    {
        var name = url.Split('/').Last();
        if (name.EndsWith(".git", StringComparison.OrdinalIgnoreCase))
            name = name[..^4];
        return name;
    }
}
