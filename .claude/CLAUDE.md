# BabyBuddyHelper

A cross-platform parenting companion built with .NET MAUI: it helps parents and caregivers organize baby-care tasks, appointments, baby profiles and vaccines, with a friendly companion, Cub. It is a functional prototype in active development, offline-first.

## Repository facts

- Stack: .NET MAUI on .NET 10, C# 14. Targets Android, iOS, Mac Catalyst and Windows (`net10.0-windows10.0.19041.0`). EF Core on SQLite. xUnit tests on Microsoft.Testing.Platform
- Solution: `BabyBuddyHelper.slnx`; the app project `BabyBuddyHelper.csproj` sits at the repo root
- GitHub: MatheusCastro99/BabyBuddyHelper (public). Default branch: `main`
- Project config for the harness: `.claude/harness.json` (checks, docs, CI checks)

## Where things are

Docs are read when the work needs them, never all up front.

- `Core/` — models, services, interfaces, persistence, storage, collections, exceptions
- `UI/` — pages, controls, UI services, converters
- `Resources/Styles/` — palettes, shared colors and named styles
- `test/BabyBuddyHelper.Tests/` — the xUnit project; it compiles `Core/` source files directly
- `Harness_Config/Context/DECISIONS.md` — the ADR log; read the ADRs a change touches
- `Harness_Config/Context/ARCHITECTURE_DESIGN.md` — layers, data flow, persistence
- `Harness_Config/Context/UI_GUIDELINES.md` — the UI authority (essentials in `.claude/rules/ui.md`)
- `Harness_Config/Context/ROADMAP.md` — phases and what comes next
- `Harness_Config/Context/CUB_GUIDELINES_CHECKLIST.md` — the questions Cub's rules are checked against by hand, with a real key (ADR-019)
- `docs/README.md`, `docs/FeaturesSet.md`, `docs/PRD.md`, `docs/ProjectVision.md` — the public project docs

## Commands

Run project checks through the runner, never by hand: `node .claude/scripts/check.cjs <fast|full|id>`. It keeps the real exit code and trims the output. Check IDs are in `.claude/harness.json`.

- Build the Windows app (optional; CI's `build` check is the gate): `dotnet build BabyBuddyHelper.csproj -f net10.0-windows10.0.19041.0`. With a target framework, always name the project.

## Workflow

Every issue goes through the harness skills, in order. Each block ends where its skill says.

- **Standard:**
  1. `/scope <n>` — plan mode, one plan, gate G1
  2. `build` — autonomous; draft PR and handoff
  3. a fresh Opus session: `/review <n>` — red-team, gate G2
  4. the owner merges, then `/close <n>`
- **Small:** `/scope <n> small` — the same gates, in one session.

Working state lives in `.harness/work/<n>/` (gitignored). The approved plan and the handoff are also posted to the issue. If `.harness/work/<n>/BLOCKED` exists, stop and tell the owner.

## Standing rules

- Preserve .NET MAUI idioms; do not recommend Xamarin.Forms.
- Keep C# 14 and .NET 10 compatibility.
- `TaskListService` is the single source of truth for tasks and appointments; `BabyProfileService` for baby profiles (ADR-001).
- Follow the ADRs in `DECISIONS.md`, especially ADR-007: id-based CRUD. Do not change the GUID migration.
- Prefer small, safe changes. Avoid big architectural rewrites unless explicitly requested.
- Avoid introducing CQRS, MediatR, the Repository pattern, or heavy MVVM, unless the owner explicitly requests an MVVM migration.
- Fix build warnings where possible.
- A change to `Core/` behaviour comes with tests, and a bug fix in `Core/` comes with a test that reproduces the bug; state any exception in the PR (ADR-018).
- Place new files by layer: `Core/` (models, services, interfaces, persistence, storage, collections, exceptions), `UI/` (pages, controls, UI services, converters) or `Remote/` (off-device implementations only: clients for external services, their DTOs and mapping). Contracts stay in `Core/Interfaces`, and only `MauiProgram` references `Remote/`. Namespaces follow folders.
- Place tests in `test/BabyBuddyHelper.Tests`: `Services/` (through the cache services, on the in-memory database), `Models/` (model logic) or `Infrastructure/` (the test host).

## Working agreements

1. The owner makes every project decision: scope, architecture, product, priorities, wording. Recommend; never assume or decide.
2. A decision that affects direction or scope comes as 2–3 options, one marked recommended, each with trade-offs and why it fits this project.
3. Say so when a path looks sub-optimal, and recommend an alternative from evidence. Still don't choose.
4. Anything the plan doesn't cover stops the work: a behaviour change, a new dependency, a scope change. Ask first. A scope change goes into the issue before it goes into the code.
5. Fix what was asked and name what else you found. The owner decides whether the scope grows.
6. For each finding, before proposing a fix: can it be reached today? Does it affect the people who use the app? Is the fix worth its cost? The owner decides on every finding.
7. Verify a claim before relying on it (a small test, the source file); don't trust memory.
8. For small implementation choices (naming, formatting), take a reasonable default and proceed.
9. Standing rules belong to the owner. Propose or draft one; don't add, reword or change one yourself.

## Branches and commits

- One issue, one branch, one PR: `<type>/<n>-<short-name>`, with `feat`, `fix`, `docs` or `chore`. Never use auto-generated names (e.g. `claude/nifty-thompson-xxxx`); if a session or tool assigns one, rename it before pushing or ask the owner.
- One commit per sub-phase: `<type>: <summary> (#<n>)`. Docs that a change makes untrue are updated in the same commit.
- Pushing a feature branch, opening a **draft** PR and commenting on the issue are pre-authorized. Never push to the default branch, force-push, or merge: the owner merges.

## Safety

- Public repository: no secrets (`.env`, API keys, the Syncfusion license key), no personal notes, no unfixed security findings in commits, comments or PRs. Report security findings to the owner directly.
- Never edit the hooks, `.claude/scripts/` or `harness.json` to make a check pass.

## Compact instructions

When compacting, keep: the issue number and tier, the plan's sub-phase list with each one's status, the last check results, the open owner decisions, and any BLOCKED state. Drop file contents and tool output.
