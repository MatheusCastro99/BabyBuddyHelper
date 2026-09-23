# Current Architectural Decisions

### ADR-001

TaskListService remains the single source of truth for Tasks and Appointments.
BabyProfileService remains the single source of truth for Baby Profiles.

Reason: 
- Keeps domain model simple and updating the UI is easier. 
- TaskListService is the only writer that can initiate add, update, or remove transactions for tasks and appointments.
- BabyProfileService is the only writer that can initiate add, update, or remove transactions for baby profiles.
- This reduces the risk of inconsistencies in the application state, specially as data persistance arrives.

Data Flow:
Page -> TaskListService / BabyProfileService (in-memory cache) -> ITrackerDbService (currently implemented by EfTrackerDbService over EF Core + SQLite)

Status: Accepted, current.

### ADR-002

EF Core + SQLite implementation is active through `EfTrackerDbService`.

Implementation notes:
- Database lifecycle uses `EnsureCreated` (no EF migrations yet).
- While the app is not live, schema changes may reset the local database.
- Debug-only `ResetDatabaseOnStartup` in `Services/EfTrackerDbService.cs` exists for one-run schema resets.
- Persistence operations use one short-lived `TrackerContext` per call through `IDbContextFactory<TrackerContext>`.

Status: Accepted, current.

### ADR-003

Scheduler uses SchedulerAppointment projection layer.

Reason:
- Appointment mapping integration without middle layer was unreliable.

Status: Accepted, permanent.

### ADR-004

AddTaskPage is the single adding/editing experience for Tasks and Appointments.
AddBabyPage is the single adding/editing experience for Baby Profiles.

AddTaskPage can be accessed from:
- ChecklistPage (Add / Edit)
- CalendarPage (Add / Edit Appointment)

AddBabyPage can be accessed from:
- MainPage (Add / Edit)

Reason:
- Enables consistency and uniformity when creating / editing instances, specially as data persistence arrives.

Status: Accepted, current.

### ADR-005

Proper architecture pattern deferred until application complexity justifies it.

Candidate patterns include:
- MVVM
- Service-based architecture
- Feature-based folder structure

Status: Accepted, current.

### ADR-006

Modular components that are highly reusable are to be developed as Services and used through Interfaces

Examples:
- Filtering (BabyFilterService)
- TaskList Management (TaskListService)
- Baby Profile Management (BabyProfileService)
- Database interactions (ITrackerDbService consuming EfTrackerDbService)

Status: Accepted, current.

### ADR-007

Domain data is located, retrieved, updated, and removed by Id whenever possible, never by object reference.

Reason:
- Guid identifiers exist precisely so a record can be found regardless of which instance a caller happens to hold.
- Reference equality fails silently. A collection entry that has been swapped for a new instance will not match, so the operation becomes a no-op instead of an error.
- Services already replace instances during normal operation.
- Pages hold references across modal navigation, which is exactly when those references go stale.
- Reference identity carries no meaning once records round-trip through a database. Id-based access is the pattern that survives persistence.

Examples:
- Service lookups, updates, and removals
- Future database CRUD operations
- Matching UI selections back to domain records

Status: Accepted, permanent.

### ADR-008

`TaskListService` and `BabyProfileService` are cache services for UI state.
`ITrackerDbService` is the persistence contract, and `EfTrackerDbService` is its current implementation.
Only cache services should interact with the persistence boundary.

Reason:
- Keeps pages out of storage concerns.
- Keeps the UI responsive while the cache services handle persistence through a separate boundary.
- Write, read, and failure order, and the full data path: see ADR-014
- Keeps persistence isolated behind `ITrackerDbService`, so implementation swaps remain localized.
- Supports one-context-per-operation persistence through `IDbContextFactory<TrackerContext>`.

Status: Accepted, current.

### ADR-009

Mock data is seeded only through TaskListService, only in Debug builds, and only when the task list is empty.

Reason:
- Keeps the app in a known state for development and testing.
- Keeps clear separation between development and production builds and features.
- Mock data still obeys the same rules as real data, so it is a valid test of the app's behavior.
- Data flow is still through the cache services, so the persistence boundary is exercised even with mock data.

Status: Accepted, permanent.

### ADR-010

Secret Rules:
- Secrets are NEVER committed to source control.
- Secrets will always be compiled in at build time, file is a build input only.
- No Server-side credentials in the client app.

Status: Accepted, permanent.

### ADR-011

Database referential integrity is enforced in the database model, and cache services mirror the persisted result.

Reason:
- Deleting a baby profile must not leave orphaned task associations.
- `TrackerContext` enforces `ON DELETE SET NULL` for `TaskModel.AssociatedBabyId`.
- Services continue using Id-based operations while persisted constraints guarantee data consistency.

Status: Accepted, permanent.

### ADR-012

The vaccine catalog is fixed reference data and lives outside the database.

Reason:
- Vaccine definitions are not user-owned records.
- The catalog should remain stable and reusable while vaccination records stay per-baby.
- Keeping the catalog outside the database avoids unnecessary churn when local persistence evolves.

Status: Accepted, current.

### ADR-013

ToastService confirms successful user actions, AlertService reports failures that need acknowledgement, and the companion stays out of alert flows.

Reason:
- Positive actions should feel encouraging and lightweight.
- Failures need a blocking, acknowledged response so the user knows the app did not save or load as expected.
- Alert text belongs to recovery and safety, not to the companion persona.
- Cub remains part of positive feedback only.

Status: Accepted, current.

### ADR-014
Data flows through one fixed path. Every layer talks only to the interface of the layer below it.

Write path:
Page (UI) -> ITaskListService / IBabyProfileService (in-memory cache services) -> ITrackerDbService (EfTrackerDbService) -> TrackerContext -> babybuddy.db3 (SQLite)

Return path:
The database commits -> the call returns up the same path -> the cache service updates its collection -> the UI refreshes through bindings.

Failure path:
EfTrackerDbService throws DbCommunicationException -> the cache is left untouched -> the page tells the user through an alert.

Read path:
Pages read from the cache services only. The database is read once, at startup, to fill the cache.

Rules:
- The database is the source of truth. The cache only mirrors what the database has already committed.
- Pages never skip a layer. They never talk to ITrackerDbService, TrackerContext or the database (ADR-001, ADR-008).
- Each database call uses its own short-lived TrackerContext, created through IDbContextFactory (ADR-002).

Reason:
- One write order means no rollback: a failed save leaves nothing to undo.
- The UI can never show data that wasn't stored.
- Constraints enforced by the database (ADR-011) reach the cache as results, not predictions.
- Every layer can be swapped behind its interface without touching the layers above it.

Diagram: docs/DataFlow.excalidraw

Status: Accepted, current.
