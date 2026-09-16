# Current State:

Status:

- Functional Prototype
- Active Development

---

# Current Development Focus:

## Phase 3: 

CodeBase Refactor:

- When Refactoring, ALWAYS consult AI_CONTEXT.md, DECISIONS.md, and ARCHITECTURE_DESIGN.md as guidelines for code production
- Scan codebase for files that
	-> Have too much responsibility
	-> Are way too large (e.g AddTaskPage)
	-> Could be broken down into services (Filtering processes)
- Scan codebase for secrets
- Prepare codebase for DataBase interaction

Data Persistence:

- Reflect on which strategy to use: Local DB with cloud Sync later OR Online DB
- Implement data persistence for tasks, appointments, and Baby Profiles
- Use SQLite or similar local database for offline-first functionality OR PostgresDB for online DB
- Ensure data is saved, read, and retrieved efficiently
- Turn Current TaskListService into a cache service (Will hold in-memory copies of tasks and appointments)

---