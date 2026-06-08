namespace Devonic.Core.Entities;

public sealed class AppConfig
{
    public Ide? DefaultIde { get; init; }
    public string? DefaultProjectsPath { get; init; }
    public Dictionary<Ide, string> IdePaths { get; init; } = new();
}
