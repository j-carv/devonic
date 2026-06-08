# devonic

Open your projects fast from the terminal.

**devonic** is a CLI tool that lets you register local projects and open them in your preferred IDE with a single command.

```
dev frontend
```

## Installation

### npm (recommended)

```bash
npm install -g devonic
```

### .NET Global Tool

```bash
dotnet tool install -g Devonic
```

### Standalone binary

Download from [Releases](https://github.com/devonic-cli/devonic/releases) and add to your PATH.

### From source

```bash
git clone https://github.com/devonic-cli/devonic.git
cd devonic
dotnet build -c Release
```

## Quick start

```bash
# Register a project
dev add

# Open it
dev frontend

# Open and run its dev server
dev frontend --run
```

## Commands

| Command | Description |
|---|---|
| `dev <project>` | Open a project in its IDE |
| `dev <project> --run` | Open and run configured command |
| `dev add` | Register a new project (interactive) |
| `dev remove <name>` | Remove a project |
| `dev edit <name>` | Edit a project |
| `dev list` | List all projects |
| `dev search <text>` | Search projects by name |
| `dev recent` | Show recently opened projects |
| `dev favorites` | Show favorite projects |
| `dev clone <url>` | Clone a git repo and register it |
| `dev config` | Show global configuration |
| `dev config set <key> <value>` | Set a configuration value |

## Configuration

All data is stored in `~/.devonic/`:

| File | Purpose |
|---|---|
| `projects.json` | Registered projects |
| `config.json` | Global settings |
| `usage.json` | Usage history |

### Project structure

```json
{
  "name": "frontend",
  "path": "D:\\Projects\\Frontend",
  "ide": "vscode",
  "runCommand": "npm run dev",
  "isFavorite": true
}
```

### Global config

```bash
# Set default IDE
dev config set defaultIde vscode

# Set default projects directory
dev config set defaultPath D:\Projects

# Set custom IDE executable path
dev config set idePath.rider "C:\Program Files\JetBrains\Rider\bin\rider64.exe"
```

### Supported IDEs

- **Rider** — JetBrains Rider
- **IntelliJ** — JetBrains IntelliJ IDEA
- **VsCode** — Visual Studio Code

## Cross-platform

devonic works on Windows, Linux, and macOS. IDE executable paths are auto-detected per platform. If auto-detection fails, configure custom paths:

```bash
dev config set idePath.rider /snap/bin/rider
```

## Architecture

```
src/
├── Devonic.Core/              # Domain: entities, interfaces, use cases
│   ├── Entities/
│   ├── Interfaces/
│   ├── Results/
│   └── UseCases/
├── Devonic.Infrastructure/    # Implementations: JSON persistence, IDE, Git
│   ├── Persistence/
│   ├── IdeIntegration/
│   └── Git/
└── Devonic.CLI/               # Entry point: commands, routing
    └── Commands/
```

Follows clean architecture — Core has zero dependencies, Infrastructure implements Core interfaces, CLI is the composition root.

## Contributing

See [docs/contributing.md](docs/contributing.md).

## License

[MIT](LICENSE)
