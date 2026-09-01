# Tech Debt skill

## Purpose:

Evaluate whether technical debt should be addressed now, planned for later, or intentionally accepted.

This skill exists to prevent unnecessary refactors while still ensuring the project remains maintainable as it grows.

---

## When To Use:

Use this skill when:
 
- Reviewing a proposed refactor.
- Evaluating a design improvement.
- Assessing architectural changes.
- Prioritizing backlog items.
- Determining whether technical debt should be addressed.
- Balancing feature delivery against code quality improvements.
 
Do NOT use when:
 
- Implementing a specific feature.
- Performing code review focused on correctness.
- Reviewing UI/UX decisions.
 
Use:
- CODE_REVIEW for implementation quality.
- ARCHITECTURE_REVIEW for system design.
- FEATURE_PLAN for new feature proposals.

---

## Classify:

### P0 - Critical
 
Technical debt that causes:
 
- Logic failures
- Broken business rules
- Application crashes
- Data corruption
- Security vulnerabilities
- Loss of source-of-truth integrity
 
Examples:
 
- Tasks updating the wrong records.
- Multiple conflicting sources of truth.
- Logic that causes scheduler corruption.
- Security flaws exposing secrets.
 
Action:
 
Address immediately.
 
### P1 - High Priority
 
Technical debt that significantly affects:
 
- Maintainability
- Reliability
- Future feature development
 
Examples:
 
- Repeated logic causing recurring bugs.
- Major architectural inconsistencies.
- Difficult-to-test critical services.
- High-risk implementation patterns.
 
Action:
 
Address before major new feature work.
 
### P2 - Medium Priority
 
Technical debt that creates friction but does not currently block development.
 
Examples:
 
- Scheduler projection complexity.
- Repeated UI code.
- Areas that would benefit from MVVM in the future.
- Code structure that is becoming difficult to navigate.
 
Action:
 
Plan for future refactoring.
 
Do not interrupt active feature work.
 
### P3 - Low Priority
 
Technical debt that has little impact on current development.
 
Examples:
 
- Minor code duplication.
- Style inconsistencies.
- Naming improvements.
- Potential future optimizations.
 
Action:
 
Document if necessary.
 
Otherwise ignore.

---

## Rules:

Favor feature stability
over unnecessary refactors.

---

## Output Format
 
### Summary
 
Brief description of the technical debt.
 
### Classification
 
P0 / P1 / P2 / P3
 
### Risks
 
Potential consequences if left unresolved.
 
### Recommendation
 
Address Now / Schedule Later / Accept
 
### Rationale
 
Reasoning behind the recommendation.