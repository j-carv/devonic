using System.Text.Json.Serialization;

namespace Devonic.Infrastructure.Persistence;

internal sealed class ConfigDto
{
    [JsonPropertyName("defaultIde")]
    public string? DefaultIde { get; set; }

    [JsonPropertyName("defaultProjectsPath")]
    public string? DefaultProjectsPath { get; set; }

    [JsonPropertyName("idePaths")]
    public Dictionary<string, string> IdePaths { get; set; } = new();
}
