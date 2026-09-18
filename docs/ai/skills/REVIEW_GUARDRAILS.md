# review-guardrails

## Purpose

Judge a change against **BabyBuddyHelper's own rules** rather than generic best practice. It is the single review checklist for code, architecture and tech-debt questions.

## When to use

- Reviewing a branch, diff or pull request
- Evaluating a refactor or a design proposal (for example, the persistence work in #25 and #26)
- Deciding whether a piece of tech debt should be fixed now, scheduled, or accepted

It is preloaded by the `red-team` agent, and can also be used directly in a session.

## What it checks

| Area | Source of truth |
|---|---|
| Data ownership: cache services are the only writers; only they touch `ITrackerDbService` | ADR-001, ADR-008 |
| Id-based lookup, update and remove; no reference equality | ADR-007 |
| Single add/edit pages (`AddTaskPage`, `AddBabyPage`) | ADR-004 |
| Syncfusion types stay in the UI layer | ADR-003 |
| Reusable logic behind interfaces | ADR-006 |
| Debug-only seeding through `TaskListService` | ADR-009 |
| No enterprise patterns without a present need | AI_CONTEXT → Technical Philosophy |

It also carries the project's **known regression traps**:

- Sorting from `CollectionChanged` causes re-entrancy exceptions.
- Checklist completion must stay `OneWay` and go through `SetCompletionAsync`.
- Converting between task and appointment replaces the instance.
- Un-awaited async saves lose their exceptions.

## Severity scale

| Level | Action |
|---|---|
| Critical: wrong or lost data, crash, broken ADR | Fix before merge |
| Major: real reliability or maintainability risk | Fix before the next feature |
| Minor: friction, clarity | Mention; fix if cheap |
| Accepted trade-off: intentional per the docs | Don't raise again |

## Verification limits

- Build validation: `dotnet build -f net10.0-windows10.0.19041.0`.
- There is no automated test project. The maintainer runs a full manual create/read/update/delete pass on every entity, so the skill lists only edge cases that pass would miss: colliding data, culture and date parsing, unusual window sizes, and async ordering.

## Output

A verdict (Approve / Approve with changes / Rework recommended), then findings with `file:line` evidence, severity and the rule they violate, then the build result, edge cases for manual checking, and docs made stale.
