# dotADR

.NET Global Tool to create [Architectural Decision Records](https://adr.github.io) in .NET solutions.

[![Pre-release](https://github.com/robyvandamme/dotadr/actions/workflows/pre-release.yml/badge.svg)](https://github.com/robyvandamme/dotadr/actions/workflows/pre-release.yml)

## Install

```shell
dotnet tool install dotADR --prerelease
```

### Upgrade

```shell
dotnet tool update dotADR --prerelease
```

## Features

### Initialize the ADR Directory

Creates the ADR directory, adds the default template and a first decision record to the directory, and saves the ADR directory to a `dotadr.json` file.

```text
DESCRIPTION:
Initialize the ADR directory

USAGE:
    dotnet dotadr init [OPTIONS]

EXAMPLES:
    dotnet dotadr init
    dotnet dotadr init -d ./doc/arch/adr -o true
    dotnet dotadr init --debug true --logfile log.txt

OPTIONS:
                       DEFAULT                                              
    -h, --help                      Prints help information                 
        --debug                     Enable debug logging for troubleshooting
        --logfile                   The file to send the log output to      
    -d, --directory    ./doc/adr    The directory to initialize             
    -o, --overwrite    false        Whether to overwrite existing files     

```

### Add a New Decision Record

Adds a new decision record in the configured ADR directory using the `template.md` template file.

```text
DESCRIPTION:
Add a new decision record

USAGE:
    dotnet dotadr add [title] [OPTIONS]
    dotnet dotadr new [title] [OPTIONS]

EXAMPLES:
    dotnet dotadr add "Implement Circuit Breaker Pattern for External Service Calls"
    dotnet dotadr add "Separate Read and Write Data Models" -s 002
    dotnet dotadr add "Use Database Per Service Pattern" --debug true --logfile log.txt
    dotnet dotadr new "Implement Request Rate Limiting"

ARGUMENTS:
    [title]    The title of the new decision record

OPTIONS:
    -h, --help          Prints help information                                      
        --debug         Enable debug logging for troubleshooting                     
        --logfile       The file to send the log output to                           
    -s, --supersedes    The ID of the decision record this decision record supersedes

```

## The ADR Template

The default template is based on [Documenting Architecture Decisions](https://cognitect.com/blog/2011/11/15/documenting-architecture-decisions) and looks like this:

```text
# {{ID}} {{TITLE}}

* Status: Draft
* Date: {{DATE}} 
* Supersedes: {{SUPERSEDES}}

## Context

## Decision

## Consequences

```

The decision record that is added upon initialization looks like this:

```markdown
# 001 Use Architectural Decision Records

* Status: Draft
* Date: 2025-08-19

## Context

## Decision

## Consequences

```
The line containing the `{{SUPERSEDES}}` variable is removed unless the `--supersedes`option is provided.


### Changing the Template

Feel free to customize the template, but consider keeping the `Status` and `Supersedes` sections as-is to maintain full feature compatibility.
You can choose to not include a variable, in that case it is simply ignored.

### Superseding a Decision Record

In order for the superseding functionality to work you will need to keep the `"* Status: xxxx"` and `"* Supersedes: {{SUPERSEDES}}"` sections in the template more or less intact.

```shell
dotnet dotadr add "Superseding Decision Record" -s 002
```

When a record supersedes another record:
* In the new record the `{{SUPERSEDES}}` variable is replaced by a link to the superseded record.
* In the superseded record "* Status: [current status]" is replaced by "* Status: [current status] - Superseded by " + a link to the new superseding decision record and the current date.

#### Example

```markdown
* Supersedes: [002](002-the-superseded-decision.md)
```

```markdown
* Status: Accepted - Superseded by [077](077-the-superseding-decision.md) 2025-08-21
```