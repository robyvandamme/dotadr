# dotADR

.NET Global Tool to create [Architectural Decision Records](https://adr.github.io) in .NET solutions.

[![Pre-release](https://github.com/robyvandamme/dotadr/actions/workflows/pre-release.yml/badge.svg)](https://github.com/robyvandamme/dotadr/actions/workflows/pre-release.yml)

NOTE: only tested on MacOS so far.

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
    dotnet dotadr add "Title of the new decision record"
    dotnet dotadr add "Title of the new decision record" --debug true --logfile log.txt
    dotnet dotadr new "Title of the new decision record"

ARGUMENTS:
    [title]    The title of the new decision record

OPTIONS:
    -h, --help       Prints help information                 
        --debug      Enable debug logging for troubleshooting
        --logfile    The file to send the log output to   

```


## The ADR Template

The default template is based on [Documenting Architecture Decisions](https://cognitect.com/blog/2011/11/15/documenting-architecture-decisions) and looks like this:

```text
# {{ID}} {{TITLE}}

* Status: Draft
* Date: {{DATE}}

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

### Changing the Template

Feel free to customize the template, but consider keeping the Status and Date sections as-is to maintain full feature compatibility.
You can choose to not include a variable, in that case it is simply ignored.
