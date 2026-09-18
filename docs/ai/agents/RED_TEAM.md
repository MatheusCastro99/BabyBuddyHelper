# red-team: Agent Documentation

The runnable definition is `.claude/agents/red-team.md`. It's gitignored (the maintainer's personal tooling), so changes to it are logged as comments on issue #33. This documentation is tracked.

## Why it exists

This is the one subagent that passed the #33 cost-efficiency review. Its value is **independence**: a fresh context with no stake in the code, challenging work the main session wrote. Everything else (design, product and UX lenses, doc tracking) runs inline as skills or in the main session, because rebuilding context for those costs more than it returns.

## When to spawn it

- Before handing a large or risky PR to the owner for review
- When a design needs an independent challenge (e.g. the persistence schema in #25)

## When not to

- Small, mechanical changes; the context rebuild costs more than it returns
- Work the main session still has open; review finished work, not work in progress

## Configuration

| Field | Value | Why |
|---|---|---|
| `tools` | Read, Grep, Glob, Bash | No Edit/Write. Bash is for `git`/`gh` inspection and `dotnet build`. "Read-only" for Bash is enforced by the prompt, not by permissions. |
| `model` | opus | Adversarial review is where depth matters most; the trade-off is cost per run |
| `effort` | high | Same reason |
| `skills` | review-guardrails | Reviews against the project's ADRs and known traps, not generic taste |
| No `Agent` tool | — | Can't spawn other agents; returns one report |

## Brief it with

The goal (issue number and decided scope), the target (branch, commit range, PR or design doc), the constraints (ADRs in play, out-of-scope items) and any prior findings on a re-review.

## Output

A verdict per dimension (goal, architecture, implementation), findings scored by `Severity + Confidence + Impact − Fix cost` with `file:line` evidence, the build result, manual edge cases, adjacent issues, and docs made stale. At most two review cycles per change. The maintainer decides; red-team only recommends.

## History

- 2026-09-18 (#33): Rebuilt for Claude Code. The VS Code tool names had made it impossible to launch. The TechLead handoff loop and the "final validator" authority were removed, and it now preloads `review-guardrails`. Coordinator, TechLead and ProjectHistorian were deleted; ProductOwner and UXDesigner became skills.
