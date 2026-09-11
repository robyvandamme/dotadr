# AGENTS.md

## Project Snapshot
- `dotadr` is a .NET 8 global CLI tool for Architecture Decision Records (ADRs).
- Main user flows are `dotadr init` (bootstrap) and `dotadr add`/`dotadr new` (create ADRs).
- Core implementation lives in `src/DotAdr`; tests live in `test/DotAdr.Tests`.

## Architecture You Should Learn First
- Entry point: `src/DotAdr/Program.cs` sets up Serilog and pre-parses `--debug` / `--logfile` before CLI dispatch.
- Command wiring: `src/DotAdr/CommandConfiguration.cs` registers commands + DI services for Spectre.Console.Cli.
- Commands are thin orchestrators:
  - `src/DotAdr/Commands/Init/InitAdrCommand.cs`
  - `src/DotAdr/Commands/Add/AddAdrCommand.cs`
- Business logic is in services behind interfaces:
  - `IAdrFileService` (`src/DotAdr/Commands/AdrFileService.cs`) handles file/config/template I/O.
  - `IAdrFactory` (`src/DotAdr/Commands/AdrFactory.cs`) renders ADR content from template variables.
  - `IConfigurationService` (`src/DotAdr/Commands/ConfigurationService.cs`) reads/writes `dotadr.json`.

## ADR Data Flow (Important)
- `add` command loads config (`dotadr.json`) -> resolves ADR directory -> reads `template.md`.
- Next ADR id is inferred from existing files (`001-...md`, `002-...md`, etc.).
- Template variables used by factory: `{{ID}}`, `{{TITLE}}`, `{{DATE}}`, `{{SUPERSEDES}}`.
- Supersede behavior updates both records: new ADR contains "Supersedes" link; old ADR gets "Superseded by" link.

## Project-Specific Conventions
- ADR filename format: `{id:000}-{safe-title}.md` (lowercase, spaces to dashes, invalid chars removed).
- Template filename is fixed to `template.md` in the ADR directory.
- Config file is `dotadr.json` in working directory; JSON uses camelCase (`directory`).
- Logging pattern uses extension methods in `src/DotAdr/Common/LoggerExtensions.cs`:
  - `MethodStart(class, method)` / `MethodReturn(class, method[, result])`.
- Error flow in commands: catch exception, log via Serilog, print via Spectre, return exit code `1`.

## Build / Test / Pack Workflows
- Preferred orchestration uses Nuke (`build/Build.cs`) via scripts in repo root.
- Common commands:
  - `./build.sh` (restore, compile, test, pack pipeline)
  - `./build.sh Test`
  - `dotnet test test/DotAdr.Tests/DotAdr.Tests.csproj`
  - `dotnet run --project src/DotAdr -- init --debug true --logfile debug.log`
- Packaging: project is a .NET tool (`PackAsTool=true`, command name `dotadr`).

## Testing Patterns To Follow
- Testing stack: xUnit + Moq + Shouldly + Spectre.Console.Testing.
- Keep command tests focused on behavior and filesystem effects (see `test/DotAdr.Tests/Commands`).
- `TestFileHelpers` creates temp directories/files for integration-like file service tests.

## External Dependencies / Boundaries
- CLI framework: `Spectre.Console.Cli`.
- Logging: `Serilog` with console and file sinks.
- Most side effects are local filesystem writes; no network/service dependencies in core flows.

## Practical Agent Tips
- If changing command behavior, update both command tests and service tests that assert file output.
- Preserve current CLI options and aliases (`add` + `new`), and keep examples in command config aligned.
- Prefer extending service interfaces over adding logic directly inside command classes.
