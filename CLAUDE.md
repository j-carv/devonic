# Devonic

CLI tool to register and open local projects fast from the terminal.

## Stack

- C# / .NET 10
- Spectre.Console for UI (prompts, tables, charts, figlet)
- JSON file persistence in `~/.devonic/`
- Distributed via npm (binary wrapper) and .NET Global Tool

## Architecture

Clean architecture with 3 layers:

- **Devonic.Core** — Domain. Entities, interfaces, use cases, result types. Zero dependencies.
- **Devonic.Infrastructure** — Implementations. JSON repositories, cross-platform IDE opener, git service.
- **Devonic.CLI** — Entry point. Command routing in Program.cs, one static class per command in Commands/.

Dependencies flow inward: CLI → Infrastructure → Core.

## Build & Run

```bash
dotnet build Devonic.slnx
dotnet run --project src/Devonic.CLI/Devonic.CLI.csproj -- <args>
```

## Project Layout

```
src/
  Devonic.Core/
    Entities/          Project, Ide, AppConfig, ProjectUsage
    Interfaces/        IProjectRepository, IIdeOpener, IConfigRepository, IUsageTracker, IGitService
    Results/           Result, Result<T>
    UseCases/          OpenProject, AddProject, RemoveProject, EditProject, CloneProject
  Devonic.Infrastructure/
    Persistence/       JsonProjectRepository, JsonConfigRepository, JsonUsageTracker, DTOs
    IdeIntegration/    IdeOpener (cross-platform: Windows/Linux/macOS)
    Git/               GitService
  Devonic.CLI/
    Commands/          Open, Add, Remove, Edit, List, Search, Recent, Favorites, Clone, Config, Scan, Doctor, Stats
    Program.cs         Command routing, help, interactive selector
    ServiceLocator.cs  Composition root
npm/
  bin/dev.js           Node.js launcher for npm distribution
  scripts/postinstall.js  Downloads platform binary from GitHub Releases
```

## Data Files

All in `~/.devonic/`:
- `projects.json` — registered projects
- `config.json` — global settings (default IDE, paths)
- `usage.json` — open history and counts

## Releasing

1. Bump version in: `npm/package.json`, `src/Devonic.CLI/Devonic.CLI.csproj`, `Program.cs` (ShowVersion)
2. Commit and push
3. `git tag v<version> && git push origin v<version>`
4. GitHub Actions builds standalone binaries + publishes to npm

## Conventions

- No unit tests
- No comments unless the why is non-obvious
- Commands are static classes with a `RunAsync` method
- Use Spectre.Console markup for all output: `>` success, `+` added, `-` removed, `x` error, `!!` warning
- All user-facing text in English
- Entities use `required` and `init` properties
- Repository methods are async and return `Task<>`
- Use cases take interfaces via primary constructor
