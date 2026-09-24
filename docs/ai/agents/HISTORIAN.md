# historian: Agent Documentation

The runnable definition is `.claude/agents/historian.md`. It's gitignored (the maintainer's personal tooling), so changes to it are logged as comments on issue #68. This documentation is tracked.

## Why it exists

Doc updates are the last step before every merge. The main session already keeps a running list of the docs each change makes stale, with file, location and suggested text. The historian turns that list into edits, so the maintainer reviews doc changes instead of writing them.

Its value is **a cross-check between two parties**:
- The main session writes the list from the implementation.
- The historian checks each item against the code before applying it, and sweeps for anything the list missed.
- The maintainer reviews the historian's diff against the list and the implementation.

If one side makes a mistake, the other is positioned to catch it.

## When to spawn it

- Right before a PR merges: after CI, the red-team pass and the fixes, once the code is final
- With a stale-docs list in hand

## When not to

- While the code is still changing, because the docs would go stale again
- Without a list: it doesn't build its own list from scratch
- For personal tooling (`.claude/agents`, `.claude/skills`) or `.claude/CLAUDE.md`, which stay with the main session and the maintainer

## Configuration

| Field | Value | Why |
|---|---|---|
| `tools` | Read, Grep, Glob, Edit, Bash | Edit applies the listed changes; there's no Write, so it can't create or overwrite whole files. Bash is for read-only `git`/`gh` inspection. "Edit only `docs/`" and "read-only Bash" are enforced by the prompt, not by permissions. |
| `model` | sonnet | The work is bounded and driven by the list: verify a fact, apply a small edit. Depth matters less here than it does for red-team; the trade-off is a slightly higher chance of a missed finding in the sweep, which the maintainer's review covers |
| `effort` | medium | Same reason |
| `skills` | none | The rules it needs live in the docs it reads |
| No `Agent` tool | — | Can't spawn other agents; returns one report |

## Rules it follows

- **Applies only the listed items.** Anything else it finds is reported and waits for the maintainer's confirmation.
- **The code wins.** An item that doesn't match the code is left unapplied and reported with evidence.
- **Checks the whole section it edits,** not just the listed facts. It reports extra errors without fixing them.
- **Reports every adaptation** of the suggested text, including format changes, so the report matches its diff.
- **ADRs:**
  - It may correct a listed factual detail inside an existing ADR, such as a path or class name.
  - It never changes a `Status:` line.
  - It never adds, removes or renumbers an ADR.
  - New ADR candidates come back as drafts in its report.
- **Keeps the maintainer's voice:** small edits in the surrounding style, no reflowing or reformatting outside the listed location.
- **Never commits.** The main session commits after the maintainer's review.

## Brief it with

The target branch or PR, the stale-docs list (ID, file and location, what's stale, what it should say, evidence), any ADR candidates, and the maintainer's notes on items already rejected or changed.

## Output

A summary count, then:
- applied items, with any adaptation of the suggested text
- items it didn't apply, with the reason
- findings beyond the list (not edited)
- ADR drafts
- the files it touched

The report must match its diff exactly, since the maintainer reviews one against the other.

## History

- 2026-09-24 (#68): Created. It brings back a documentation agent after ProjectHistorian was retired in #33. This time its scope is narrower: it applies a list the main session prepared, instead of tracking docs on its own.
- 2026-09-24 (#68, #66): First run, on the Phase 3.5 docs: all 10 items applied correctly. It had two misses: a format adaptation it didn't report, and a stale fact inside an edited section that it didn't check. Two rules were added in response: check every fact in an edited section, and report format changes as adaptations.
