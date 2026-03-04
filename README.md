# Spec CLI

[![NuGet](https://img.shields.io/nuget/v/QuinntyneBrown.Spec.Cli.svg)](https://www.nuget.org/packages/QuinntyneBrown.Spec.Cli)

A .NET CLI tool that installs the **spec** skill into Claude Code projects, enforcing a structured, requirements-driven development process for AI-assisted code generation.

## What It Does

Running `spec-cli install` creates a `.claude/skills/spec.md` file in your project. This skill guides Claude Code agents through:

1. **Requirements documentation** — L1 (high-level) and L2 (detailed) specs are created before any code is written
2. **Team-based implementation** — Implementor and QA agents work iteratively on each requirement
3. **Quality verification** — QA verifies completion against acceptance criteria before moving on

## Installation

```bash
dotnet tool install --global QuinntyneBrown.Spec.Cli
```

## Usage

```bash
spec-cli install
```

This creates `.claude/skills/spec.md` in the current directory.

## Development

### Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)

### Build

```bash
dotnet build
```

### Run Tests

```bash
dotnet test
```

### Install from Source

```bash
dotnet pack src/Spec.Cli -c Release
dotnet tool install --global --add-source src/Spec.Cli/bin/Release QuinntyneBrown.Spec.Cli
```

## Project Structure

```
├── docs/specs/          # L1 and L2 requirements for this project
├── src/Spec.Cli/        # CLI application
│   ├── Program.cs       # Entry point with DI, logging, and configuration
│   ├── InstallCommand.cs
│   ├── ISkillInstaller.cs
│   └── SkillInstaller.cs
└── tests/Spec.Cli.Tests/
```

## License

See [LICENSE](LICENSE) for details.
