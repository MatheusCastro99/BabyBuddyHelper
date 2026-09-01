# Architecture Review Skill

## Purpose

Evaluate architectural decisions and proposed changes to ensure BabyBuddyHelper remains:

- Stable
- Maintainable
- Extensible
- Consistent with project philosophy

This skill focuses on system design rather than implementation details.

---

## When To Use

Use this skill when:

- Reviewing architecture changes.
- Evaluating refactors.
- Considering new architectural patterns.
- Introducing new services or layers.
- Planning major feature integrations.
- Assessing long-term maintainability.

Do NOT use when:

- Reviewing implementation details.
- Evaluating UI/UX.
- Assessing technical debt.
- Reviewing feature requests.

Use:

- CODE_REVIEWER for implementation review.
- FEATURE_PLANNER for feature evaluation.
- TECH_DEBT for debt classification.
- UI_DESIGNER for interface reviews.

---

## Core Architectural Principles

### 1. Stability First

Questions:

- Does this change preserve existing functionality?
- Does it maintain system reliability?
- Does it introduce new failure points?
- Does it protect current workflows?

Verify:

- Existing features remain functional.
- Source-of-truth ownership remains clear.
- No unstable dependencies are introduced.

---

### 2. Simplicity Over Complexity

Questions:

- Is the complexity justified?
- Is there a simpler solution?
- Does the architecture solve a real problem?

Favor:

- Direct solutions
- Clear data flows
- Minimal abstraction

Avoid:

- Architectural patterns introduced without clear benefit.
- Layers that provide little value.
- Speculative future-proofing.

---

### 3. Evolvability

Questions:

- Can future features build upon this design?
- Does the solution support roadmap goals?
- Does it allow domain evolution?

Verify:

- Design remains adaptable.
- Future modifications remain possible.
- Planned roadmap features remain achievable.

---

### 4. Single Source of Truth

Current Source:

TaskListService

Verify:

- Ownership remains clear.
- Duplicate state is not introduced.
- State mutations remain predictable.

Warnings:

- Multiple collections representing the same data.
- State ownership moving into pages.
- Conflicting data sources.

---

## Architectural Evaluation Areas

### Service Design

Review:

- Service responsibilities
- Dependency boundaries
- Business logic placement

Good:

- Well-defined responsibilities.
- Clear ownership.
- Minimal coupling.

Bad:

- God services.
- Circular dependencies.
- Mixed responsibilities.

---

### Data Flow

Review:

- Data ownership
- State transitions
- Event usage

Good:

- Predictable flow.
- Single source of truth.

Bad:

- Hidden state.
- Duplicate ownership.
- Complex event chains.

---

### UI Boundaries

Review:

- Separation between UI and business logic.
- Framework-specific dependencies.

Good:

- Domain remains UI-independent.
- UI concerns remain in UI layer.

Bad:

- Scheduler dependencies leaking into services.
- Business logic embedded in UI.

---

### Persistence Readiness

Review:

- Future database compatibility.
- Storage assumptions.

Remember:

SQLite implementation is intentionally deferred.

Avoid introducing persistence abstractions before they are needed.

---

## Architectural Pattern Evaluation

When evaluating patterns:

Examples:

- MVVM
- Feature-Based Organization
- Repository Pattern
- CQRS
- Domain Events

Ask:

### Problem

What specific problem does this solve?

### Current Pain

Does that problem exist today?

### Cost

What complexity does the pattern introduce?

### Benefit

What measurable value does it provide?

### Timing

Is now the correct time?

---

## BabyBuddyHelper Guidance

Current philosophy:

Build functionality first.

Discover the domain.

Refactor when patterns emerge.

The presence of a popular architectural pattern is NOT sufficient justification for adopting it.

Architectural changes should solve real and existing problems.

---

## Red Flags

### Major Concerns

- Multiple sources of truth.
- Unnecessary abstraction layers.
- Tight coupling between unrelated components.
- Architectural complexity without clear benefit.
- UI dependencies leaking into domain concerns.

---

### Acceptable Trade-Offs

Currently accepted:

- No SQLite
- No MVVM
- No Repository Pattern
- No CQRS
- No MediatR
- Limited abstraction

These are not architectural problems unless project complexity creates a real need.

---

## Output Format

### Summary

High-level assessment.

### Architectural Strengths

Positive findings.

### Architectural Risks

Potential concerns.

### Stability Impact

Low / Medium / High

### Complexity Impact

Low / Medium / High

### Future Flexibility

Low / Medium / High

### Recommendation

Approve /
Approve With Adjustments /
Reconsider

### Rationale

Explanation of the recommendation.

### Alignment Check

Evaluate alignment with:

- Stability
- Simplicity
- Evolvability
- Single Source of Truth

Assessment:

Pass / Needs Review