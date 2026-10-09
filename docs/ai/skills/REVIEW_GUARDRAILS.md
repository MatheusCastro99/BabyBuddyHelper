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
| Data ownership: cache services are the only write entry points; `EfTrackerDbService` commits first and the cache mirrors the persisted result | ADR-001, ADR-008, ADR-011, ADR-014 |
| Id-based lookup, update and remove; no reference equality | ADR-007 |
| Single add/edit pages (`AddTaskPage`, `AddBabyPage`) | ADR-004 |
| Scheduler projection: `AppointmentModel` becomes `SchedulerAppointment` inside `CalendarPage`; Syncfusion types stay in the UI layer | ADR-003, AI_CONTEXT → Calendar Architecture |
| Reusable logic behind interfaces | ADR-006 |
| Fixed reference data (the vaccine catalog) stays outside the database; pages read it straight from `IVaccineCatalog` | ADR-012, ADR-014 |
| Files placed by layer (`Core/`, `UI/`); namespaces follow folders | ADR-015 |
| Detail pages are pushed; add/edit editors open modally | ADR-016 |
| Debug-only seeding through `TaskListService` | ADR-009 |
| Feedback channels: toasts confirm success, alerts report failures, Cub stays out of alerts | ADR-013 |
| No enterprise patterns without a present need | AI_CONTEXT → Technical Philosophy |

It also carries the project's **known regression traps**:

- Sorting from `CollectionChanged` causes re-entrancy exceptions.
- Checklist completion must stay `OneWay` and go through `SetCompletionAsync`.
- Never set a bound property from code (for example, `checkBox.IsChecked = ...`); if a UI reset is required, use `ClearValue` so the binding remains authoritative.
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

- Build validation: `dotnet build BabyBuddyHelper.csproj -f net10.0-windows10.0.19041.0`. Name the project: the solution also holds the test project, which has no Windows target.
- Tests: `dotnet test --project test/BabyBuddyHelper.Tests/BabyBuddyHelper.Tests.csproj`.
- A test project exists (`test/BabyBuddyHelper.Tests`, ADR-018). Through the cache services on a SQLite in-memory database it covers create/read/update/delete by id for tasks, babies and vaccination records, colliding data, task/appointment conversion, baby removal, date flags and measurement dates, sorting and filtering. A change to `Core/` should come with tests, and a `Core/` bug fix with a test that reproduces it; flag a change that has none.
- The maintainer's full manual create/read/update/delete pass still owns everything in `UI/`. The skill lists only the edge cases the tests and that pass would miss: culture and date parsing, unusual window sizes, and async ordering.

## Output

A verdict (Approve / Approve with changes / Rework recommended), then findings with `file:line` evidence, severity and the rule they violate, then the build result, edge cases for manual checking, and docs made stale.
