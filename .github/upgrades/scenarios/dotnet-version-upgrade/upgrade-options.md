# Upgrade Options — PDXSMS_V2

Assessment: 6 SDK-style projects on net8.0 with one incompatible package and multiple binary/source API compatibility issues for net10.0.

## Strategy

### Upgrade Strategy
The solution is already modern (.NET 8, SDK-style) with a moderate project count and no .NET Framework migration boundary.

| Value | Description |
|-------|-------------|
| **All-at-Once** (selected) | Upgrade all projects together in a single pass, then validate the full solution. |
| Top-Down | Upgrade entry-point apps first and keep shared libraries temporarily multi-targeted until consolidation. |

## Compatibility

### Unsupported Packages
Assessment reports 1 incompatible package for target net10.0, which is a small enough set to resolve during normal task execution.

| Value | Description |
|-------|-------------|
| **Resolve Inline** (selected) | Research and resolve incompatible package references in the same task without deferring follow-up work. |
| Defer Resolution | Keep projects compiling using temporary stubs/deferrals and create follow-up subtasks for full package replacement. |
| Compatibility Mode | Keep framework-style compatibility references temporarily with higher runtime risk. |

### Unsupported API Handling
Assessment reports binary/source API changes, and the recommended approach is to fix these directly during each task.

| Value | Description |
|-------|-------------|
| **Fix Inline** (selected) | Resolve API incompatibilities directly in the active task, including simple and complex changes. |
| Defer Complex Changes | Apply simple replacements now, use temporary stubs for complex changes, and schedule dedicated follow-up resolution tasks. |
