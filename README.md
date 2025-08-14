# DotADR

.NET Global Tool to create [Architectural Decision Records](https://adr.github.io) in .NET solutions.

The default template is based on [Documenting Architecture Decisions](https://cognitect.com/blog/2011/11/15/documenting-architecture-decisions).

## Using a Different Template

If you want to change the template or use a different template altogether it's probably best to keep the title and the header section as is to avoid unexpected behavior.
The template variables that are updated when a new decision record is created are:
* `{{ID}}`: the numeric identifier of the decision record
* `{{TITLE}}`: the title of the decision record
* `{{DATE}}`: the date the decision record is added
* `{{SUPERSEDES}}`: "Supersedes: " and a link to the decision record that this record supersedes 

You can choose to not include a variable, in that case it is simply ignored.

In order for the superseding functionality to work you will need to keep the `"* Status: xxxx"` section in the template more or less intact. See below for more information.

### Superseding a Decision Record

```shell
dotnet dotadr add "Superseded functionality implementation" -s 001 or -s 1
```

When a record supersedes another record:
* In the new record the `{{SUPERSEDES}}`variable is replaced by "Supersedes: " + a link to the superseded record.
* In the superseded record "* Status: whatever current status" is replaced by "* Status: Superseded by " + a link to the superseding decision record.




