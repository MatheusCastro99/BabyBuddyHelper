# Current Architecture Design:

## Structure:

- Core/
  - Models/
  - Services/
  - Interfaces/
  - Persistence/
  - Collections/
  - Exceptions/
- UI/
  - Pages/
  - Controls/
  - Services/
  - Converters/

## Current key files:

Core/Models/

- TaskModel.cs
- AppointmentModel.cs
- BabyModel.cs

Core/Services/

- TaskListService.cs
- BabyProfileService.cs
- BabyFilterService.cs
- TrackerDataSeeder.cs

Core/Interfaces/

- ITaskListService.cs
- IBabyProfileService.cs
- IBabyFilterService.cs
- ITrackerDbService.cs

Core/Persistence/

- TrackerContext.cs
- EfTrackerDbService.cs

Core/Collections/

- RangeObservableCollection.cs

Core/Exceptions/

- DbCommunicationException.cs

UI/Pages/

- MainPage
- ChecklistPage
- AddTaskPage
- AddBabyPage
- CalendarPage

UI/Controls/

- CompanionView
- ToastView

UI/Services/

- ToastService.cs
- AlertService.cs

UI/Converters/

- AssociatedBabyNameConverter.cs
- IsAppointmentModelConverter.cs

See the data-flow diagram: [DataFlow.excalidraw](../../DataFlow.excalidraw)

## Future Architecture Direction

When architectural redesign becomes necessary, Service-based, Feature-based, Layered, and Modular organization are some of the candidates for potential end-state.
