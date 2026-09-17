# Current State:

Status:

- Functional Prototype
- Active Development

---

# Current Development Focus:

## Phase 3: 

CodeBase Refactor:

- Refactor baseline completed for oversized files and service extraction work (including filtering service split). CONCLUDED
- Remaining security pass: run and document a full secrets scan.
- Prepare codebase for database interaction (#27).

Data Persistence:

- Reflect on which strategy to use: Local DB with cloud Sync later OR Online DB
- Implement data persistence for tasks, appointments, and Baby Profiles
- Use SQLite or similar local database for offline-first functionality OR PostgresDB for online DB
- Ensure data is saved, read, and retrieved efficiently
- Turn Current TaskListService into a cache service (Will hold in-memory copies of tasks and appointments)

---
