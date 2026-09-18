# product-owner

## Purpose

A product lens used alongside the working session. It checks whether a feature or scope decision strengthens BabyBuddyHelper as an offline-first **Parenting Companion**, and when it should happen.

## When to use

- A new feature or backlog item is proposed
- An issue's scope or acceptance criteria are unclear
- Comparing candidate features for the next phase

## Context it loads

`docs/ProjectVision.md`, `docs/ai/context/ROADMAP.md`, `docs/ai/context/CURRENT_STATE.md` and `docs/FeaturesSet.md`, plus the GitHub issue comments where decided scope is recorded.

## What it evaluates

- **User need:** the problem, who has it, and how often
- **Parenting Companion fit:** helpful, supportive, encouraging, rather than merely feature-rich
- **Cost and architectural impact:** cache services, the persistence boundary, models, the scheduler projection, ADR conflicts
- **Timing:** does it belong in the current roadmap phase or a later one?
- **Expansion value:** is it foundational or a one-off?

## Priority scale

| Priority | Meaning |
|---|---|
| P0 Foundational | Later roadmap work depends on it |
| P1 High value | Strong caregiver value at a reasonable cost |
| P2 Nice to have | Improves the experience; not essential |
| P3 Low | Novelty, cosmetic, experimental |

## Output

Ratings for value, fit, cost and impact; a priority; a recommendation (Now / Next phase / Backlog / Reject) with advantages and disadvantages; acceptance criteria when scoping; and 2-3 options when the decision is ambiguous. The maintainer makes the final call.
