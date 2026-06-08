namespace Devonic.Core.Entities;

public sealed class ProjectUsage
{
    public required string ProjectName { get; init; }
    public required DateTime LastOpened { get; init; }
    public required int OpenCount { get; init; }
}
