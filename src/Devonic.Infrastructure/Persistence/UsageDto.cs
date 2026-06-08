using System.Text.Json.Serialization;

namespace Devonic.Infrastructure.Persistence;

internal sealed class UsageEntryDto
{
    [JsonPropertyName("projectName")]
    public string ProjectName { get; set; } = string.Empty;

    [JsonPropertyName("lastOpened")]
    public DateTime LastOpened { get; set; }

    [JsonPropertyName("openCount")]
    public int OpenCount { get; set; }
}
