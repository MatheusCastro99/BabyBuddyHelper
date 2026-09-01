# Code Review Skill

## Purpose

Review code changes for:

- Correctness
- Maintainability
- Stability
- Future extensibility
- Alignment with project philosophy

This skill exists to ensure BabyBuddyHelper continues to grow sustainably without sacrificing simplicity or introducing unnecessary complexity.

---

## When To Use

Use this skill when:

- Reviewing pull requests.
- Reviewing completed features.
- Evaluating refactors.
- Evaluating architectural changes.
- Reviewing generated code.
- Assessing implementation quality.

Do NOT use when:

- Planning new features.
- Reviewing UI designs.
- Evaluating technical debt.
- Making product roadmap decisions.

Use:

- FEATURE_PLAN for feature prioritization.
- UI_DESIGN for user experience.
- ARCHITECTURE_REVIEW for high-level architectural decisions.
- TECH_DEBT_ANALYSE for debt classification.

---

## Primary Review Pillars

### 1. Platform Stability

Questions:

- Does the change introduce regressions?
- Does it break existing workflows?
- Does it impact application reliability?
- Does it create race conditions or event loops?
- Does it create inconsistent application state?

Verify:

- Existing functionality continues to work.
- TaskListService remains reliable.
- Scheduler integration remains stable.
- No duplicate state is introduced.

Examples:

Good:

- Existing behavior preserved.
- New functionality isolated.
- Predictable state transitions.

Bad:

- CollectionChanged event loops.
- Multiple sources of truth.
- Hidden side effects.
- Stability regressions.

---

### 2. Expansibility

Questions:

- Can future features build upon this implementation?
- Does this solution scale with project growth?
- Does it support the roadmap?

Verify:

- New code is reasonably reusable.
- Future extension remains possible.
- Design does not block planned roadmap items.

Examples:

Good:

- Service methods that support future consumers.
- Reusable utility methods.
- Flexible component designs.

Bad:

- Hard-coded assumptions.
- Tight coupling.
- UI-specific logic leaking into services.

---

### 3. Simplicity Over Complexity

Questions:

- Is the added complexity justified?
- Is there a simpler implementation?
- Does this introduce unnecessary abstractions?

Verify:

- Solution complexity matches the problem.
- No premature abstractions.
- No speculative architecture.

Avoid recommending:

- CQRS
- MediatR
- Full Clean Architecture
- Repository Pattern
- MVVM
- Additional service layers

unless project complexity clearly requires them.

Examples:

Good:

- Clear implementation.
- Direct solution.
- Simple service interactions.

Bad:

- Layers with no clear value.
- Future-proofing for hypothetical requirements.
- Architectural patterns without business justification.

---

## BabyBuddyHelper Specific Checks

### Source of Truth

Verify:

- TaskListService remains the source of truth.
- No page owns business data.
- No duplicate collections are introduced.

---

### Editing Workflow

Verify:

- AddTaskPage remains the single editing experience.
- Editing behavior remains consistent across consumers.

Consumers:

- ChecklistPage
- CalendarPage
- Future Dashboard

---

### Scheduler Integration

Verify:

- Scheduler types remain inside the UI layer.
- Syncfusion dependencies do not leak into services.
- Appointment projection remains intact.

---

### Persistence Strategy

Verify:

- In-memory model remains supported.
- Domain evolution remains possible.

---

## Severity Levels

### S0 - Critical

Must fix before merge.

Examples:

- Application crashes.
- Business rule violations.
- Broken editing workflows.
- Data integrity issues.
- Source-of-truth violations.

---

### S1 - Major

Should be addressed before feature expansion.

Examples:

- Significant maintainability concerns.
- Architectural inconsistencies.
- Reliability concerns.

---

### S2 - Minor

Improvement opportunities.

Examples:

- Readability improvements.
- Small refactors.
- Naming improvements.

---

### S3 - Accepted Trade-Off

Intentional technical compromise aligned with project philosophy.

Examples:

- Delaying MVVM.
- Delaying SQLite.
- Temporary duplication during feature discovery.

---

## Output Format

### Summary

High-level review findings.

### Strengths

What was implemented well.

### Findings

List issues found.

### Severity

Critical / Major / Minor

### Recommendation

Approve /
Approve With Changes /
Refactor Recommended

### Future Considerations

Potential future improvements.

### Philosophy Check

Does the implementation align with:

- Platform Stability
- Expansibility
- Simplicity First

Assessment:
Pass / Needs Improvement