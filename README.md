# DotADR

.NET Global Tool to create [Architectural Decision Records](https://adr.github.io) in .NET solutions.

## ADR Template

The default template is based on [Documenting Architecture Decisions](https://cognitect.com/blog/2011/11/15/documenting-architecture-decisions) and looks like this:

```text
# {{ID}} {{TITLE}}

* Status: Draft
* Date: {{DATE}}

## Context

## Decision

## Consequences

```

The output for the initial decision record that is added upon initialization looks like this:

```markdown
# 001 Use Architectural Decision Records

* Status: Draft
* Date: 2025-08-13

## Context

## Decision

## Consequences

```

### Template Variables

The template variables that are updated when a new decision record is created are:

* `{{ID}}`: the identifier of the decision record
* `{{TITLE}}`: the title of the decision record
* `{{DATE}}`: the date the decision record is added

### Changing the Template

If you want to change the template or use a different template it's probably best to keep the title and the header section more or less intact to avoid unexpected behavior.
You can choose to not include a variable, in that case it is simply ignored.

In order for the superseding functionality to work you will need to keep the `"* Status: xxxx"` section in the template more or less intact. See below for more information. (TBD)

### Superseding a Decision Record (TBD)

```shell
dotnet dotadr add "Superseding decision record" -s 001 or -s 1
```

When a record supersedes another record:
* In the new record the `{{SUPERSEDES}}` variable is replaced by "* Supersedes: " + a link to the superseded record.
* In the superseded record "* Status: [current status]" is replaced by "* Status: [current status] - Superseded by " + a link to the new superseding decision record and the date.

```markdown
* Status: Accepted - Superseded by [005](...) 2025-08-10
```

#### Variables

* `{{SUPERSEDES-LINK}}`: A link to the decision record that this record supersedes (TBD)
* `{{SUPERSEDES-LIST-ITEM}}`: "* Supersedes: " (TBD)
* `{{SUPERSEDES}}`: "Supersedes" (TBD)