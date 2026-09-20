# Current State:

Status:

- Functional Prototype
- Active Development

---

# Current Development Focus:

## Phase 3:

CodeBase Refactor: CONCLUDED

- Oversized-file audit completed.
- Filtering processes split into dedicated services.
- Baby profile and checklist/calendar filtering now run through service interfaces.
- Repository-wide secret audit completed

Data Persistence:

- Prior to introducing data persistence, change BabyModel logic from Age to DateOfBirth.
- Persistence boundary introduced through `ITrackerDbService`.
- `TaskListService` and `BabyProfileService` are the cache services backed by `ITrackerDbService`.
- SQLite / EF Core implementation (#25) is still pending.
- Implement data persistence for tasks, appointments, and Baby Profiles.
- Ensure data is saved, read, and retrieved efficiently.

---
