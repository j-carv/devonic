using System.Text.Json;
using Devonic.Core.Entities;
using Devonic.Core.Interfaces;

namespace Devonic.Infrastructure.Persistence;

public sealed class JsonProjectRepository(string filePath) : IProjectRepository
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public async Task<IReadOnlyList<Project>> GetAllAsync()
    {
        var dtos = await ReadDtosAsync();
        return dtos
            .Where(d => Enum.TryParse<Ide>(d.Ide, ignoreCase: true, out _))
            .Select(MapToProject)
            .ToList();
    }

    public async Task<Project?> GetByNameAsync(string name)
    {
        var projects = await GetAllAsync();
        return projects.FirstOrDefault(p =>
            p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<Project?> GetByNameOrAliasAsync(string nameOrAlias)
    {
        var projects = await GetAllAsync();
        return projects.FirstOrDefault(p =>
            p.Name.Equals(nameOrAlias, StringComparison.OrdinalIgnoreCase) ||
            (p.Alias is not null && p.Alias.Equals(nameOrAlias, StringComparison.OrdinalIgnoreCase)));
    }

    public async Task AddAsync(Project project)
    {
        var dtos = await ReadDtosAsync();
        dtos.Add(MapToDto(project));
        await WriteDtosAsync(dtos);
    }

    public async Task<bool> RemoveAsync(string name)
    {
        var dtos = await ReadDtosAsync();
        var removed = dtos.RemoveAll(d =>
            d.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (removed == 0) return false;
        await WriteDtosAsync(dtos);
        return true;
    }

    public async Task<bool> UpdateAsync(Project project)
    {
        var dtos = await ReadDtosAsync();
        var index = dtos.FindIndex(d =>
            d.Name.Equals(project.Name, StringComparison.OrdinalIgnoreCase));
        if (index < 0) return false;
        dtos[index] = MapToDto(project);
        await WriteDtosAsync(dtos);
        return true;
    }

    public async Task<IReadOnlyList<Project>> SearchAsync(string query)
    {
        var projects = await GetAllAsync();
        return projects
            .Where(p =>
                p.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                (p.Alias is not null && p.Alias.Contains(query, StringComparison.OrdinalIgnoreCase)) ||
                p.Tags.Any(t => t.Contains(query, StringComparison.OrdinalIgnoreCase)))
            .ToList();
    }

    public async Task<IReadOnlyList<Project>> GetByTagAsync(string tag)
    {
        var projects = await GetAllAsync();
        return projects
            .Where(p => p.Tags.Any(t => t.Equals(tag, StringComparison.OrdinalIgnoreCase)))
            .ToList();
    }

    private async Task<List<ProjectDto>> ReadDtosAsync()
    {
        if (!File.Exists(filePath))
            return [];

        var json = await File.ReadAllTextAsync(filePath);
        return JsonSerializer.Deserialize<List<ProjectDto>>(json, JsonOptions) ?? [];
    }

    private async Task WriteDtosAsync(List<ProjectDto> dtos)
    {
        var dir = Path.GetDirectoryName(filePath);
        if (dir is not null && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        var json = JsonSerializer.Serialize(dtos, JsonOptions);
        await File.WriteAllTextAsync(filePath, json);
    }

    private static Project MapToProject(ProjectDto dto) => new()
    {
        Name = dto.Name,
        Path = dto.Path,
        Ide = Enum.Parse<Ide>(dto.Ide, ignoreCase: true),
        Alias = dto.Alias,
        RunCommand = dto.RunCommand,
        IsFavorite = dto.IsFavorite,
        Tags = dto.Tags
    };

    private static ProjectDto MapToDto(Project project) => new()
    {
        Name = project.Name,
        Path = project.Path,
        Ide = project.Ide.ToString().ToLowerInvariant(),
        Alias = project.Alias,
        RunCommand = project.RunCommand,
        IsFavorite = project.IsFavorite,
        Tags = project.Tags
    };
}
