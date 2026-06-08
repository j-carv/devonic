using System.Text.Json;
using Devonic.Core.Entities;
using Devonic.Core.Interfaces;

namespace Devonic.Infrastructure.Persistence;

public sealed class JsonUsageTracker(string filePath) : IUsageTracker
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public async Task RecordUsageAsync(string projectName)
    {
        var entries = await ReadEntriesAsync();
        var entry = entries.FirstOrDefault(e =>
            e.ProjectName.Equals(projectName, StringComparison.OrdinalIgnoreCase));

        if (entry is not null)
        {
            entry.OpenCount++;
            entry.LastOpened = DateTime.UtcNow;
        }
        else
        {
            entries.Add(new UsageEntryDto
            {
                ProjectName = projectName,
                LastOpened = DateTime.UtcNow,
                OpenCount = 1
            });
        }

        await WriteEntriesAsync(entries);
    }

    public async Task<IReadOnlyList<ProjectUsage>> GetRecentAsync(int count = 10)
    {
        var entries = await ReadEntriesAsync();
        return entries
            .OrderByDescending(e => e.LastOpened)
            .Take(count)
            .Select(e => new ProjectUsage
            {
                ProjectName = e.ProjectName,
                LastOpened = e.LastOpened,
                OpenCount = e.OpenCount
            })
            .ToList();
    }

    private async Task<List<UsageEntryDto>> ReadEntriesAsync()
    {
        if (!File.Exists(filePath))
            return [];

        var json = await File.ReadAllTextAsync(filePath);
        return JsonSerializer.Deserialize<List<UsageEntryDto>>(json, JsonOptions) ?? [];
    }

    private async Task WriteEntriesAsync(List<UsageEntryDto> entries)
    {
        var dir = Path.GetDirectoryName(filePath);
        if (dir is not null && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        var json = JsonSerializer.Serialize(entries, JsonOptions);
        await File.WriteAllTextAsync(filePath, json);
    }
}
