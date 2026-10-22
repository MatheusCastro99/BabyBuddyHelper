---
name: review-guardrails
description: BabyBuddyHelper's project-specific review checklist. Covers ADR-backed architecture rules, known regression traps, and a severity scale for findings and tech debt. Use when reviewing a diff or PR, evaluating a refactor or design (e.g. persistence work in #25/#26), or deciding whether tech debt should be fixed now. Preloaded by the red-team agent.
---

# Review Guardrails

This is the checklist for judging a change against **this project's** rules rather than generic taste. It holds the procedure and the traps. The rules themselves live in the authoritative docs, so read those instead of trusting any restatement here.

## 1. Load the rules first

Read these before judging anything. If a check below disagrees with them, the docs win. Say so in the output so the skill can be fixed.

- `Harness_Config/Context/DECISIONS.md` (ADRs and their current status)
- `Harness_Config/Context/ARCHITECTURE_DESIGN.md` (the Platform notes section especially)
- `Harness_Config/Context/ROADMAP.md` (what phase we're in, and what's intentionally unfinished)

An ADR marked "Pending" or "temporary" describes a target, not the current state. Don't report the gap as a defect unless the change under review claims to close it.

## 2. Project checks

For each check, confirm it with evidence from the code (`file:line`), not from memory.

### Data ownership (ADR-001, ADR-008, ADR-011, ADR-014)
- `TaskListService` is the only write entry point for tasks and appointments. `BabyProfileService` is the only write entry point for baby profiles.
- No page owns business data or keeps a second collection that mirrors service state.
- Only the cache services talk to `ITrackerDbService`. A page or control that touches it is a violation.
- `EfTrackerDbService` commits first, and the cache mirrors the persisted result.
- Failed writes throw `DbCommunicationException` and leave the cache untouched.
- Startup load failures surface alerts so the user can retry.

### Identity (ADR-007)
- Lookup, update and remove go through the `Guid` Id, never object reference or `IndexOf(instance)`.
- Watch for references held across modal navigation, since those go stale.
- The Guid migration is done. Never suggest reverting it.

### Editing workflow (ADR-004)
- `AddTaskPage` is the only add/edit surface for tasks and appointments, and `AddBabyPage` is the only one for profiles.
- A new entry point (Checklist, Calendar, the future Dashboard) must route there, not reimplement editing.

### Scheduler boundary (ADR-003)
- Syncfusion types stay in the UI layer. `AppointmentModel` becomes `SchedulerAppointment` inside `CalendarPage` only.
- Services must never reference Syncfusion.

### Services (ADR-006)
- Reusable logic sits behind an interface in `Core/Interfaces/` with its implementation in `Core/Services/` (or `Core/Persistence/` for the database boundary), and is registered in `MauiProgram.cs`.

### Reference data (ADR-012, ADR-014)
- The vaccine catalog is fixed data written into the code. It never goes through `ITrackerDbService`, and its Ids never change.
- Pages may read `IVaccineCatalog` directly. It is the one exception to "pages read from the cache services only".

### Layout (ADR-015)
- New files go by layer: `Core/` (models, services, interfaces, persistence, collections, exceptions) or `UI/` (pages, controls, UI services, converters). Namespaces follow folders.

### Navigation (ADR-016)
- Detail and read-only pages use `PushAsync`; add/edit editors use `PushModalAsync`.

### Seeding (ADR-009)
- Mock data goes only through `TaskListService`, only in Debug builds, and only when the list is empty. Check for `#if DEBUG` guards.

### User Feedback (ADR-013)
- Reinforcing and positive feedback (Saves, completions, updates, deletions) will be done through Toast notifications with Cub interactions.
- Negative feedback (Database errors, failures) will be communicated through Alerts, requiring user acknowledgment before proceeding. Cub interactions will not be used for negative feedback.

### Known regression traps (these have bitten before)
- **Sorting from `CollectionChanged` causes `ObservableCollection` re-entrancy exceptions.** Refreshing from collection events is fine; sorting from them is not.
- **Checklist completion must stay `OneWay` in XAML and route through `SetCompletionAsync`.** `TwoWay` silently skips persistence.
- **Never set a bound property from code** (for example, `checkBox.IsChecked = ...`); if a UI reset is required, use `ClearValue` so the binding remains authoritative.
- Converting a task into an appointment or back replaces the instance, so a reference-based caller breaks silently (see ADR-007).
- An async service method called without `await` from an event handler loses its exceptions.

<!-- Local skill copy updated for #26 database-first write behavior and the new bound-property regression trap. -->

### Philosophy (standing rules in `.claude/CLAUDE.md`)
- The following are **not** defects: no MVVM, no Repository pattern, no CQRS or MediatR, no Clean Architecture. Recommend one only when a concrete, present problem needs it, and name that problem.
- Prefer the simplest change that fits. Speculative future-proofing is a finding in its own right.

## 3. Verification you can do

- Build: `dotnet build BabyBuddyHelper.csproj -f net10.0-windows10.0.19041.0`. Name the project: the solution also holds the test project, which has no Windows target. Report new warnings, not just errors.
- Tests: `dotnet test --project test/BabyBuddyHelper.Tests/BabyBuddyHelper.Tests.csproj`. The test project (`test/BabyBuddyHelper.Tests`, ADR-018) runs the cache services on a SQLite in-memory database and covers create/read/update/delete by id for tasks, babies and vaccination records, colliding data, task/appointment conversion, baby removal, date flags and measurement dates, sorting and filtering. A change to Core/ behaviour comes with tests, and a Core/ bug fix comes with a test that reproduces it: flag a change that has none and states no exception.
- GUI behavior can't be driven from here, and `UI/` has no automated tests. The owner runs a full manual create/read/update/delete pass on every entity, so don't list ordinary CRUD paths as "needs testing". List only what the tests and that pass would miss: culture or locale-dependent parsing and week boundaries, unusual window sizes, and ordering or race conditions around async saves.

## 4. Severity scale (findings and tech debt)

Sections 4 and 5 apply when this skill is used inline. The red-team agent uses its own scoring and output instead.

| Level | Meaning | Examples here | Action |
|---|---|---|---|
| **Critical** | Wrong data, lost data, crash, or a broken ADR | Updating the wrong record; a second source of truth; a page writing to `ITrackerDbService`; persistence skipped | Fix before merge |
| **Major** | Real maintainability or reliability risk | Reference-based lookup in a new path; Syncfusion leaking into a service; an un-awaited save | Fix before the next feature |
| **Minor** | Friction, clarity, naming | Duplicated UI code; unclear names | Mention; fix if cheap |
| **Accepted trade-off** | Intentional, per the docs | No MVVM yet; the in-memory DB stand-in; scheduler projection complexity | Don't raise it again |

## 5. Output

- **Verdict:** Approve / Approve with changes / Rework recommended
- **Findings:** list them most severe first. Each one gets a title, the evidence (`file:line`), its impact, a fix, the severity, and the ADR or trap it violates.
- **Build result**
- **Edge cases for manual check** (only the kind named in section 3)
- **Docs made stale** by the change (file and section). The owner edits docs; you only list them.

These are recommendations. The project owner makes every decision, including ADR status changes.
