# Current Architecture Design:

## Structure:

- Pages/
- Models/
- Services/
- Interfaces/
- Exceptions/

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
- AlertService.cs
- TrackerDataSeeder.cs
- EfTrackerDbService.cs

Interfaces/

- ITaskListService.cs
- IBabyProfileService.cs
- IBabyFilterService.cs
- ITrackerDbService.cs

Pages/

- MainPage
- ChecklistPage
- AddTaskPage
- AddBabyPage
- CalendarPage

Controls/

- CompanionView
- ToastView

Data/

- TrackerContext.cs

Collections/

- RangeObservableCollection.cs

Exceptions/

- DbCommunicationException.cs

See the data-flow diagram: [DataFlow.excalidraw](../../DataFlow.excalidraw)

## Future Architecture Direction

When architectural redesign becomes necessary, Service-based, Feature-based, Layered, and Modular organization are some of the candidates for potential end-state.
