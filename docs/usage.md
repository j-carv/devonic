# Usage

## Registering projects

```bash
dev add
```

This starts an interactive prompt:

```
  Project name: whatsapp-api
  Project path: D:\Projects\WhatsappApi
  Select IDE: Rider
  Add a run command? [y/n]: y
  Run command: dotnet run
  Mark as favorite? [y/n]: n
```

## Opening projects

```bash
dev whatsapp-api
```

This finds the project and opens it in the configured IDE.

### With run command

```bash
dev whatsapp-api --run
```

Opens the IDE and executes the configured run command in the project directory.

## Managing projects

### List all

```bash
dev list
```

### Edit a project

```bash
dev edit whatsapp-api
```

If no name is given, shows a selection prompt.

### Remove a project

```bash
dev remove whatsapp-api
```

Asks for confirmation before removing.

## Search and discovery

### Search by name

```bash
dev search what
```

Matches any project containing "what" in the name (case-insensitive).

### Recent projects

```bash
dev recent
```

Shows the 10 most recently opened projects with open count.

### Favorites

```bash
dev favorites
```

Shows projects marked as favorite.

## Cloning repositories

```bash
dev clone https://github.com/user/repo.git
```

This will:
1. Clone the repository to the default projects path (or ask)
2. Ask which IDE to use (unless a default is configured)
3. Register the project automatically

## Configuration

### View current config

```bash
dev config
```

### Set default IDE

```bash
dev config set defaultIde vscode
```

### Set default projects directory

```bash
dev config set defaultPath D:\Projects
```

### Set custom IDE path

```bash
dev config set idePath.rider "C:\Program Files\JetBrains\Rider\bin\rider64.exe"
dev config set idePath.vscode /usr/local/bin/code
```
