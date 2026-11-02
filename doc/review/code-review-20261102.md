# Code Review — `version-command` branch

**Date:** 2026-11-02
**Reviewer:** .NET Tech Lead (automated review)
**Scope:** `main...version-command` (commits `594cfc0`, `725990b`)

| File | Change |
|---|---|
| `src/DotAdr/CommandConfiguration.cs` | +16 −1 |
| `src/DotAdr/Program.cs` | +1 −1 |
| `test/DotAdr.Tests/CommandConfigurationTests.cs` | +40 −5 |

## Summary

The branch wires application version display into the CLI: a pre-existing `VersionInfo` record is
now injected into `CommandConfiguration.Configure`, the Spectre.Console.Cli
`SetApplicationVersion` call enables the `--version` flag, and an optional `IAnsiConsole`
parameter allows output to be captured in tests. Program.cs passes the shared `VersionInfo`
instance through instead of letting each construct its own.

Overall this is a small, well-shaped change. The dependency is injected rather than
hard-constructed, null-guard style matches the existing code, and the new tests exercise both
the happy path (`--version` output) and the new guard clause. Findings below are minor.

---

## Findings

### 1. Minor — Untested fallback branch: `Version ?? "not available"` — ✅ Fixed in `fd31358`

`src/DotAdr/CommandConfiguration.cs:32`

```csharp
config.SetApplicationVersion(versionInfo.Version ?? "not available");
```

Both branches of the null-coalescing expression are behavior; only the non-null branch is
covered (`Prints_Application_Version`). The "not available" fallback is unlikely to trigger in
practice, but as written it is dead-untested code. Options:

- Keep it simple and accept as-is (acceptable — cheap expression, low risk), or
- Make it testable by extracting the fallback decision, e.g. an `internal` overload or a small
  static helper mirroring the pattern already used for `VersionInfo.ParseProductVersion`:

```csharp
internal static string ResolveApplicationVersion(VersionInfo versionInfo) =>
    versionInfo.Version ?? "not available";
```

This follows the project precedent in `VersionInfo` where the parsing logic is exposed as an
`internal static` for direct unit testing.

**Severity:** Minor (coverage gap on a trivial branch).

**Resolution:** The fallback decision moved into `VersionInfo` as a testable `ResolveVersion`
chain (`ProductVersion` → `FileVersion` → `AssemblyVersion` → `"not available"`), and
`CommandConfiguration` now passes `versionInfo.Version` directly. All branches are covered by
`Resolves_Version_With_Fallback`.

### 2. Minor — Test-only seam in production API changed to optional parameter

`src/DotAdr/CommandConfiguration.cs:19`

```csharp
internal static void Configure(
    this CommandApp commandApp,
    ILogger logger,
    VersionInfo versionInfo,
    IAnsiConsole? console = null)
```

The optional `console` parameter exists only so tests can swap the output sink; production
(`Program.cs`) never passes it. This is a pragmatic and common pattern with Spectre.Console
(`ConfigureConsole` is a first-class configuration option), so it is acceptable. Two remarks:

- If the number of such test seams grows, prefer making them explicit required parameters in an
  internal overload rather than accumulating optional parameters — optional parameters default
  silently and can mask call-site mistakes.
- The behavior difference (custom console vs. `AnsiConsole.Console`) is worth a one-line `<remarks>`
  or inline comment clarifying that `null` means "leave Spectre's default console untouched".
  Right now the semantics must be inferred from the `if (console is not null)` block.

**Severity:** Minor (design note, no change required).

### 3. Minor — `VersionInfo` calls `FileVersionInfo.GetVersionInfo` twice and assumes non-single-file publish — ✅ Fixed in `fd31358`

Note: this code is **pre-existing** on `main` and not part of this branch's diff, but the branch
promotes `VersionInfo` onto the startup path of every command, so it is worth flagging while it
is in context — `src/DotAdr/Common/VersionInfo.cs:20-21`:

```csharp
AssemblyFileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location).FileVersion;
ProductVersion = FileVersionInfo.GetVersionInfo(assembly.Location).ProductVersion;
```

- The same (relatively expensive) filesystem call is made twice; a single local would do:

```csharp
var fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
AssemblyFileVersionInfo = fileVersionInfo.FileVersion;
ProductVersion = fileVersionInfo.ProductVersion;
```

- `Assembly.Location` returns an empty string under single-file publish / metadata-only loads.
  For a `dotnet tool` this currently works (NuGet tools are not single-file), but if the packaging
  model ever changes, `Version` will silently degrade to `null` and `--version` will print
  "not available". Consider a guard or fallback to `Assembly.GetName().Version` (which is already
  read into `AssemblyVersion`) when `Location` is empty.

**Severity:** Minor (robustness / duplicate call; pre-existing, not blocking the branch).

**Resolution:** File metadata is read once into a local and skipped when `Assembly.Location` is
empty or the file is missing. `VersionInfo.Version` is now non-nullable and falls back to
`AssemblyFileVersionInfo` / `AssemblyVersion` instead of degrading to `null`. The empty-location
path is covered by `Falls_Back_To_Assembly_Version_When_Assembly_Has_No_Location`.

### 4. Info — Synchronous `Run` in tests vs. `RunAsync` in production

`test/DotAdr.Tests/CommandConfigurationTests.cs:37`

```csharp
var result = commandApp.Run(["--version"]);
```

Production uses `await commandApp.RunAsync(args)`. Using the synchronous overload in the test is
fine here (no async command bodies are exerted by `--version`), but be aware the test path does
not exercise the same code branch as production. Acceptable for this purpose; just avoid
proliferating it in command-level tests that do exercise async commands.

**Severity:** Info.

### 5. Info — Redundant assertion in `Prints_Application_Version`

`test/DotAdr.Tests/CommandConfigurationTests.cs:40-41`

```csharp
versionInfo.Version.ShouldNotBeNullOrWhiteSpace();
console.Output.ShouldContain(versionInfo.Version);
```

The first assertion is self-referential: `CreateVersionInfo()` builds `VersionInfo` from the test
assembly, whose version is effectively guaranteed non-empty elsewhere
(`VersionInfoTests.Reads_Version_Metadata_From_Assembly`). It is harmless, but the test would be
sharper asserting the *captured* output directly, e.g.:

```csharp
console.Output.ShouldContain(versionInfo.Version);
```

Optionally also assert that `--version` returns before touching output semantics, which is already
done via `result.ShouldBe(0)`. No change required.

**Severity:** Info.

---

## What was done well

- **Dependency injection over construction:** thread `VersionInfo` through `Configure` instead of
  instantiating it internally keeps the configuration testable and mirrors how `ILogger` already
  flows through.
- **Consistent guard style:** `ArgumentNullException.ThrowIfNull` for all three required
  parameters, matching the existing code and including a dedicated regression test
  (`Throws_When_VersionInfo_Is_Null`).
- **Correct use of the framework primitive:** `config.SetApplicationVersion` + `ConfigureConsole`
  are the intended Spectre.Console.Cli extension points; no custom hand-rolled version flag.
- **Realistic output test:** using `Spectre.Console.Testing.TestConsole` with
  `EmitAnsiSequences = false` produces a stable, assertable output — a nice touch that avoids
  ANSI-sequence flakiness.
- **Test structure:** nested `Configure` class and naming conventions (`Throws_When_X_Is_Null`)
  stay consistent with the rest of the suite.

## Verdict

**Approve with minor comments.** No blocking issues. Findings 1 and 3 were addressed in `fd31358`
(testable version fallback chain, single `FileVersionInfo` call with an empty/missing-location
guard); findings 2, 4 and 5 remain informational.

---

*Findings count: 3 minor, 2 informational. No security or correctness issues found.*
*Status: Findings 1 and 3 fixed in `fd31358`; findings 2, 4 and 5 accepted as-is.*
