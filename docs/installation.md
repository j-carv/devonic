# Installation

## npm (recommended)

```bash
npm install -g devonic
```

No .NET required. The installer downloads the correct binary for your platform automatically.

To update:

```bash
npm update -g devonic
```

To uninstall:

```bash
npm uninstall -g devonic
```

## .NET Global Tool

Requires [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0).

```bash
dotnet tool install -g Devonic
```

After installation, the `dev` command is available globally.

To update:

```bash
dotnet tool update -g Devonic
```

To uninstall:

```bash
dotnet tool uninstall -g Devonic
```

## Standalone binary

Download the archive for your platform from [Releases](https://github.com/devonic-cli/devonic/releases):

- `devonic-win-x64.zip` — Windows
- `devonic-linux-x64.tar.gz` — Linux
- `devonic-osx-x64.tar.gz` — macOS

Extract and add the directory to your PATH.

### Windows

```powershell
Expand-Archive devonic-win-x64.zip -DestinationPath C:\tools\devonic
[Environment]::SetEnvironmentVariable("PATH", "$env:PATH;C:\tools\devonic", "User")
```

### Linux / macOS

```bash
tar -xzf devonic-linux-x64.tar.gz -C ~/.local/bin/
```

## From source

```bash
git clone https://github.com/devonic-cli/devonic.git
cd devonic
dotnet build -c Release
```

Run directly:

```bash
dotnet run --project src/Devonic.CLI/Devonic.CLI.csproj -- <args>
```

Or install as a local tool:

```bash
dotnet pack src/Devonic.CLI/Devonic.CLI.csproj -c Release -o ./nupkg
dotnet tool install -g --add-source ./nupkg Devonic
```
