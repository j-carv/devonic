# Contributing

## Getting started

1. Fork the repository
2. Clone your fork

```bash
git clone https://github.com/<your-user>/devonic.git
cd devonic
```

3. Build

```bash
dotnet build
```

4. Run

```bash
dotnet run --project src/Devonic.CLI/Devonic.CLI.csproj -- <args>
```

## Project structure

```
src/
├── Devonic.Core/              # Domain layer (no dependencies)
│   ├── Entities/              # Project, Ide, AppConfig, ProjectUsage
│   ├── Interfaces/            # IProjectRepository, IIdeOpener, etc.
│   ├── Results/               # Result<T> pattern
│   └── UseCases/              # Application logic
├── Devonic.Infrastructure/    # Implementation layer
│   ├── Persistence/           # JSON file repositories
│   ├── IdeIntegration/        # Cross-platform IDE launcher
│   └── Git/                   # Git clone service
└── Devonic.CLI/               # Presentation layer
    └── Commands/              # One file per command
```

## Architecture rules

- **Core** has zero external dependencies — no NuGet packages, no project references
- **Infrastructure** depends only on Core
- **CLI** depends on Core and Infrastructure (composition root)
- New features start with an interface in Core, implementation in Infrastructure, command in CLI

## Adding a new IDE

1. Add the value to `Ide` enum in `src/Devonic.Core/Entities/Ide.cs`
2. Add platform commands in `src/Devonic.Infrastructure/IdeIntegration/IdeOpener.cs`

## Adding a new command

1. Create `src/Devonic.CLI/Commands/YourCommand.cs` with a static `RunAsync` method
2. Add routing in `src/Devonic.CLI/Program.cs`

## Pull requests

- One feature per PR
- Keep PRs small and focused
- Make sure `dotnet build` passes
- Follow existing code style

## Reporting issues

Open an issue on GitHub with:
- What you expected
- What happened
- Your OS and .NET version (`dotnet --info`)
