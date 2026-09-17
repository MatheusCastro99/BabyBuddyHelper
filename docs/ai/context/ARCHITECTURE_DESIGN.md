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
- BabyFilterService.cs
- ToastService.cs

Interfaces/

- ITaskListService.cs
- IBabyProfileService.cs
- IBabyFilterService.cs

Pages/

- MainPage
- ChecklistPage
- AddTaskPage
- AddBabyPage
- CalendarPage

Controls/

- CompanionView
- ToastView

## Future Architecture Direction

When architectural redesign becomes necessary, Service-based organization is the intended end-state.