CLAUDE.md — Repository guidance for claude-code and subagents

Purpose
-------
This file tells claude-code and local subagents how to safely interact with this repository. It collects the essential project facts, authoritative docs that must be loaded before making changes, coding constraints, and high-level behavioral rules for all subagents.

Repository facts
---------------
- Repo root: the directory containing `BabyBuddyHelper.slnx`
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

AI tooling (Claude Code)
------------------------
Runnable definitions live in `.claude/agents/` and `.claude/skills/` (gitignored, maintainer's personal tooling). Documentation lives in `docs/ai/agents/` and `docs/ai/skills/`.

Subagent:
- red-team — independent adversarial reviewer, read-only. Spawn it before handing over a large or risky PR, or to challenge a design. It preloads `review-guardrails`.

Skills (applied inline, alongside the session context):
- review-guardrails — project review checklist: ADR rules, known regression traps, severity scale
- product-owner — feature value, scope, acceptance criteria, P0–P3 priority
- ux-design — Parenting Companion identity, style tokens, accessibility

Subagent invocation
-------------------
- Brief the subagent with: goal (issue number and decided scope), target (branch, commit range, PR, or design doc), constraints (ADRs in play, out-of-scope items), and expected deliverable.
- Agents and skills recommend; the maintainer makes every decision.
- Always return a concise summary and list of modified files before committing.

Coding and design constraints
----------------------------
- Preserve .NET MAUI idioms; do not recommend Xamarin.Forms.
- Keep C# 14 and .NET 10 compatibility.
- TaskListService is the single source of truth for tasks/appointments; BabyProfileService for baby profiles (ADR-001).
- Follow ADRs in docs/ai/context/DECISIONS.md (especially ADR-007: id-based CRUD). Do not change GUID migration.
- Prefer small, safe changes. Avoid big architectural rewrites unless explicitly requested.
- Avoid introducing: CQRS, MediatR, Repository pattern, heavy MVVM unless the user explicitly requests an MVVM migration.
- Tests and builds: always run `dotnet build` after code changes; fix warnings where possible.

Documentation rules
-------------------
- When a change affects public behavior or exported types, notify the engineer about updating corresponding docs in docs/ (including docs/README.md).
- Keep docs consistent with reality: update implemented feature lists, architecture lists, and roadmap when features ship.

Branching and commits
---------------------
- Use a short feature branch for multi-file edits.
- Provide a clear commit message summarizing intent and files changed.

Safety and secrets
------------------
- Do not commit secrets or .env with secrets. red-team checks for secrets only within the change it reviews; a repository-wide secrets scan is separate work (see issue #30).

Contacting the user
-------------------
- For ambiguous decisions that affect product direction, present options (2–3) and request confirmation.
- For small implementation choices (naming, formatting) prefer reasonable defaults and proceed.

Change log
----------
- 2026-09-17: Initial CLAUDE.md added (aligns with repo docs and subagent list).
- 2026-09-18 (#33): Replaced the subagent list with the Claude Code tooling (red-team + three skills); retired TechLead, ProductOwner, UXDesigner, ProjectHistorian and Coordinator. Secrets scans now route to #30.
