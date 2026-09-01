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

AddTaskPage is the single adding/editing experience.

Reason:
- Enables consistency and uniformity when creating / editing instances

Status: Accepted, permanent.

### ADR-005

MVVM deferred until application complexity justifies it.

Status: Accepted, current.