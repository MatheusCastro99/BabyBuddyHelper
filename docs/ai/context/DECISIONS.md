# Current Architectural Decisions

### ADR-001

TaskListService remains the single source of truth.

Reason: 
- Keeps domain model simple and updating the UI is easier. 
- TaskListService is the only place where tasks are added, updated, or removed. 
- This reduces the risk of inconsistencies in the application state.

Status: Accepted, current.

### ADR-002

SQLite implementation deferred until domain stabilizes.

Reason:
- Domain and Models are still evolving
- SQLite implementation will be added when the domain is stable.

Status: Accepted, current.

### ADR-003

Scheduler uses SchedulerAppointment projection layer.

Reason:
- Appointment mapping integration without middle layer was unreliable.

Status: Accepted, permanent.

### ADR-004

AddTaskPage is the single adding/editing experience for Tasks and Appointments.

Reason:
- Enables consistency and uniformity when creating / editing instances

Status: Accepted, permanent.

### ADR-005

MVVM deferred until application complexity justifies it.

Status: Accepted, current.

### ADR-006

Modular components that are highly reusable are to be developed as Services and used through Interfaces

Examples:
- Filtering
- TaskList Management
- Baby Profile Management
- Database interactions (future)

Status: Accepted, current.

### ADR-007

Domain data is located, updated, and removed by Id whenever possible, never by object reference.

Reason:
- Guid identifiers exist precisely so a record can be found regardless of which instance a caller happens to hold.
- Reference equality fails silently. A collection entry that has been swapped for a new instance will not match, so the operation becomes a no-op instead of an error.
- Services already replace instances during normal operation. TaskListService rebuilds entries through CloneWithAssociation whenever a baby profile is renamed or removed.
- Pages hold references across modal navigation, which is exactly when those references go stale.
- Reference identity carries no meaning once records round-trip through a database. Id-based access is the pattern that survives persistence.

Examples:
- Service lookups, updates, and removals
- Future database CRUD operations
- Matching UI selections back to domain records

Status: Accepted, permanent.