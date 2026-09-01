# Current Architecture Design:

## Structure:

- Pages/
- Models/
- Services/
- Interfaces/

## Current key files:

Models/

- TaskModel.cs
- AppointmentModel.cs

Services/

- TaskListService.cs

Interfaces/
    ITaskListService.cs

Pages/

- ChecklistPage
- AddTaskPage
- CalendarPage
- MainPage

## Future Architecture Direction

When architectural redesign becomes necessary, prefer:

Features/
- Tasks
- Appointments
- Calendar
- Dashboard
- Shared

Avoid:

- Pages/
- ViewModels/
- Services/
- Models/

as top-level buckets.

Feature-based organization is the intended end-state.
