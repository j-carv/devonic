namespace Devonic.Core.Entities;

public sealed class Project
{
    public required string Name { get; init; }
    public required string Path { get; init; }
    public required Ide Ide { get; init; }
    public string? Alias { get; init; }
    public string? RunCommand { get; init; }
    public bool IsFavorite { get; init; }
    public List<string> Tags { get; init; } = [];
}
