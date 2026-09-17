CLAUDE.md — Repository guidance for claude-code and subagents

Purpose
-------
This file tells claude-code and local subagents how to safely interact with this repository. It collects the essential project facts, authoritative docs that must be loaded before making changes, coding constraints, and high-level behavioral rules for all subagents.

Repository facts
---------------
- Repo root: C:\Users\mathe\source\repos\BabyBuddyHelper
- Solution: BabyBuddyHelper.slnx
- Platform: .NET MAUI targeting .NET 10
- C# language version: 14.0

Authoritative docs (always load before making edits)
---------------------------------------------------
When planning or making changes, the agent MUST read these files first and follow their rules exactly:
- docs/ai/AI_CONTEXT.md
- docs/ai/context/ARCHITECTURE_DESIGN.md
- docs/ai/context/CURRENT_STATE.md
- docs/ai/context/DECISIONS.md
- docs/ai/context/ROADMAP.md
- docs/README.md
- docs/FeaturesSet.md

Subagents available
-------------------
The codebase exposes the following logical subagents. Use them for specialized tasks:
- TechLead (technical design, refactors)
- ProductOwner (feature scope and acceptance criteria)
- UXDesigner (UI / accessibility guidance)
- ProjectHistorian (changelog, decisions)
- RedTeam (security, secrets, and sensitive data)

Subagent invocation
-------------------
- When invoking a specialized subagent, include: goal, files (globs or exact paths), constraints, and expected deliverable.
- Always return a concise summary and list of modified files before committing.

Coding and design constraints
----------------------------
- Preserve .NET MAUI idioms; do not recommend Xamarin.Forms.
- Keep C# 14 and .NET 10 compatibility.
- TaskListService is the single source of truth for tasks/appointments.
- Follow ADRs in docs/ai/context/DECISIONS.md (especially ADR-007: id-based CRUD). Do not change GUID migration.
- Prefer small, safe changes. Avoid big architectural rewrites unless explicitly requested.
- Avoid introducing: CQRS, MediatR, Repository pattern, heavy MVVM unless the user explicitly requests an MVVM migration.
- Tests and builds: always run `dotnet build` after code changes; fix warnings where possible.

Documentation rules
-------------------
- When a change affects public behavior or exported types, notify the engineer about updating corresponding docs in docs/ and the top-level README.md.
- Keep docs consistent with reality: update implemented feature lists, architecture lists, and roadmap when features ship.

Branching and commits
---------------------
- Use a short feature branch for multi-file edits.
- Provide a clear commit message summarizing intent and files changed.

Safety and secrets
------------------
- Do not commit secrets or .env with secrets. If a secrets scan is needed, invoke the RedTeam subagent and follow its remediation guidance.

Contacting the user
-------------------
- For ambiguous decisions that affect product direction, present options (2–3) and request confirmation.
- For small implementation choices (naming, formatting) prefer reasonable defaults and proceed.

Change log
----------
- 2026-09-17: Initial CLAUDE.md added (aligns with repo docs and subagent list).
