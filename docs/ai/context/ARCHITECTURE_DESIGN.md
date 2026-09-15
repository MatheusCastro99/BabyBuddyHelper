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
- BabyModel.cs

Services/

- TaskListService.cs
- BabyProfileService.cs

Interfaces/
    ITaskListService.cs
    IBabyProfileService.cs

Pages/

- ChecklistPage
- AddTaskPage
- CalendarPage
- MainPage

## Future Architecture Direction

When architectural redesign becomes necessary, Service-based organization is the intended end-state.