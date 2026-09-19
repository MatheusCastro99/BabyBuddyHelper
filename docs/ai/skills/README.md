# AI Skills: Documentation

This folder documents the Claude Code skills used on BabyBuddyHelper: what each one is for, when to use it, and what it checks.

The runnable skills live in `.claude/skills/<name>/SKILL.md`. That folder is gitignored on purpose, because the skills are the maintainer's personal tooling. The documentation here stays public so contributors can see the review standards the project is held to.

| Skill | Documentation | Purpose |
|---|---|---|
| `review-guardrails` | [REVIEW_GUARDRAILS.md](REVIEW_GUARDRAILS.md) | Project-specific review checklist: ADR rules, known regression traps, severity scale |
| `product-owner` | [PRODUCT_OWNER.md](PRODUCT_OWNER.md) | Product lens: caregiver value, companion fit, cost, roadmap timing, P0-P3 priority |
| `ux-design` | [UX_DESIGN.md](UX_DESIGN.md) | UX/UI lens: Parenting Companion identity, style tokens, accessibility |

## Design rules for skills

- **Skills hold procedure, not project truth.** Rules live in `docs/ai/context/` (DECISIONS, AI_CONTEXT, UI_GUIDELINES). A skill tells the reader to load those docs and apply them, so a doc update never leaves a skill stale.
- **Skills recommend; the maintainer decides.** No skill claims authority over scope, architecture or ADR status.
- **Invoking a skill:** Claude loads a skill automatically when its description matches the task, or on demand with `/<skill-name>`. An agent can preload one through the `skills:` field in its frontmatter.

## History

- 2026-09-18 (#33): Replaced the VS Code Copilot-era prose skills. ARCHITECTURE_REVIEW, CODE_REVIEW and TECH_DEBT_ANALYSE were merged into `review-guardrails`, and their stale "SQLite deferred" guidance was removed. FEATURE_PLAN plus the ProductOwner agent became `product-owner`. UI_DESIGN plus the UXDesigner agent became `ux-design`.
