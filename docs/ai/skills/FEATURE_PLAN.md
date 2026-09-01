# Feature Planner skill

## Purpose
 
Evaluate proposed features and determine:
 
- Whether they align with the product vision.
- Their implementation complexity.
- Their architectural impact.
- Their value to caregivers and parents.
- Their priority relative to current project goals.
 
This skill exists to help BabyBuddyHelper grow intentionally rather than accumulating unrelated functionality.

---

## When To Use
 
Use this skill when:
 
- A new feature is proposed.
- Evaluating backlog items.
- Prioritizing roadmap work.
- Assessing product direction.
- Comparing multiple feature ideas.
- Determining implementation order.
 
Do NOT use when:
 
- Reviewing existing code.
- Evaluating architecture.
- Assessing technical debt.
- Reviewing UI implementations.
 
Use:
 
- CODE_REVIEW for implementation quality.
- ARCHITECTURE_REVIEW for architectural analysis.
- TECH_DEBT_ANALYSE for refactoring priorities.
- UI_DESIGN for interface evaluation.

---

## Evaluation Criteria
 
### User Value
 
Questions:
 
- Does this solve a real caregiver problem?
- Will users actively benefit from this feature?
- Does it remove friction or add convenience?
 
Higher priority:
 
- Daily-use features
- Time-saving features
- Reminder-based features
 
Lower priority:
 
- Rarely used features
- Novelty features
- Features with unclear value
 
### Parent Companion Alignment
 
Questions:
 
- Does this support the Parenting Companion vision?
- Does it make BabyBuddyHelper feel more supportive?
- Does it strengthen the caregiver experience?
 
Favor:
 
- Encouraging features
- Helpful reminders
- Organization tools
- Family-focused workflows
 
Avoid:
 
- Unrelated productivity features
- Feature creep
 
### Development Cost
 
Evaluate:
 
- Complexity
- Estimated implementation effort
- Testing requirements
- UX impact
 
Classification:
 
Low
Medium
High
 
### Architectural Impact
 
Questions:
 
- Does the feature require persistence?
- Does the feature require cloud services?
- Does it affect TaskListService ownership?
 
Avoid introducing architectural complexity unless justified by user value.
 
### Persistence Impact
 
Questions:
 
- Does this feature require SQLite?
- Can it work in-memory initially?
- Does it change future database requirements?
 
Document any future persistence implications.
 
### Future Expansion Value
 
Questions:
 
- Is this feature foundational?
- Will future features build on it?
- Does it unlock additional capabilities?
 
Examples:
 
High Expansion Value:
 
- Notifications
- SQLite
- Recurring Appointments
 
Low Expansion Value:
 
- Cosmetic one-off features

---

## Output Format
 
### Feature Summary
 
Brief description of the proposed feature.
 
### User Value
 
Low / Medium / High
 
### Development Cost
 
Low / Medium / High
 
### Architectural Impact
 
None / Low / Medium / High
 
### Recommendation
 
Implement Now /
Plan Next Phase /
Add To Backlog /
Reject
 
### Rationale
 
Explanation for the recommendation.
 
### Potential Follow-Up Features
 
List any future capabilities enabled by the feature.