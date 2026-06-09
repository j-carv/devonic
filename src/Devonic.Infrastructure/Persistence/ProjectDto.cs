using System.Text.Json.Serialization;

namespace Devonic.Infrastructure.Persistence;

internal sealed class ProjectDto
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("path")]
    public string Path { get; set; } = string.Empty;

    [JsonPropertyName("ide")]
    public string Ide { get; set; } = string.Empty;

    [JsonPropertyName("alias")]
    public string? Alias { get; set; }

    [JsonPropertyName("runCommand")]
    public string? RunCommand { get; set; }

    [JsonPropertyName("isFavorite")]
    public bool IsFavorite { get; set; }

    [JsonPropertyName("tags")]
    public List<string> Tags { get; set; } = [];
}
