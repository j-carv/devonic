# devonic

CLI tool to register and open local projects fast from the terminal.

```
dev             Pick a project interactively
dev myapp       Open in configured IDE
dev myapp --run Open + run dev command
```

## Install

```bash
npm install -g devonic
```

Requires Node.js 18+. The postinstall script downloads the correct binary for your platform (Windows, Linux, macOS).

## What it does

- Register projects with `dev add` or auto-detect them with `dev scan <dir>`
- Open any project in your IDE with `dev <name>`
- Clone and register in one step with `dev clone <url>`
- Track usage, star favorites, filter by tags
- Health check all paths with `dev doctor`

Supports VS Code, Rider, IntelliJ, WebStorm, Visual Studio, Cursor, Zed, Fleet, and Neovim.

All data lives in `~/.devonic/` as plain JSON files.

## About

This project was built entirely by **Claude Opus 4.6** (Anthropic) using [Claude Code](https://claude.ai/code).

It was made for personal use by [@j-carv](https://github.com/j-carv), but published on npm in case anyone else finds it useful. No guarantees, no support promises — just a tool that works for me.

## License

MIT
